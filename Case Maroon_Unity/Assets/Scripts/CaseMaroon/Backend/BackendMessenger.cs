using CaseMaroon.Backend;
using CaseMaroon.GameSystem;
using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using CaseMaroon.WorldMapUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;
using UnityEngine.Networking;
using static CaseMaroon.Backend.BackendModels;
using static CaseMaroon.Backend.BackendPayloads;
using static CaseMaroon.Backend.BackendRequests;
using static CaseMaroon.Backend.BackendResponses;
using Debug = UnityEngine.Debug;

namespace CaseMaroon.Backend
{
    public delegate void GameManagerSyncedHandler(GameManagerModel gsr);

    public delegate void PingResponseHandler(bool status);

    public delegate void LoginResponseHandler(bool status, LoginResponse loginResponse);


    public class BackendMessenger : MonoBehaviour
    {
        public static BackendMessenger Instance { get; private set; }
        public string BASE_URL = "http://localhost:3001/api";
        public bool USELOCALBACKEND = false;

        public event GameManagerSyncedHandler GameStateSynced;
        public event PingResponseHandler OnPingResponse;
        public event LoginResponseHandler OnLoginResponse;

        public Action<bool, QueueJoinResponse> OnQueueJoined;

        public Action<QueueStatusResponse> OnQueueStatusChecked;
        public Action<bool> OnQueueLeft;
        public Action<bool> OnLoggedOut;

        public Action<PlayersStatusResponse> OnPlayerStatesResponse;
        private void Awake()
        {
            Instance = this;

            // Ensure only one instance exists
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Optional: persist between scenes
            // DontDestroyOnLoad(gameObject);
        }

        private Process backendProcess;
        private StringBuilder nodeBuffer = new StringBuilder();

        private Timer stdoutFlushTimer;

        [SerializeField]
        float flushInterval = 200f; // Interval to flush stdout buffer

        bool stdoutTimerArmed = false;

        void Start()
        {
            stdoutFlushTimer = new Timer(flushInterval);

            //StartBackend();
        }

        void OnApplicationQuit()
        {
            //StopBackend();
        }
        private void StartBackend()
        {
            if (!USELOCALBACKEND)
            {
                return;
            }

            if (backendProcess != null && !backendProcess.HasExited)
            {
                Debug.Log("Backend already running.");
                return;
            }

            string scriptPath = @"W:\Unity Projects\Case Maroon\Case Maroon_Backend\src\index.js";

            if (!File.Exists(scriptPath))
            {
                Debug.LogError("Backend script not found at: " + scriptPath);
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath)
            };

            backendProcess = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            backendProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    nodeBuffer.AppendLine(e.Data);

                    if (!stdoutTimerArmed)
                    {
                        stdoutTimerArmed = true;
                        stdoutFlushTimer.Start();
                    }
                }
            };

            stdoutFlushTimer.Elapsed += (s, e) =>
            {
                if (nodeBuffer.Length > 0)
                {
                    //Debug.Log(nodeBuffer.ToString());
                    //nodeBuffer.Clear();
                    // Reset the timer interval
                    stdoutFlushTimer.Interval = flushInterval;
                }

                stdoutFlushTimer.Stop();
                stdoutTimerArmed = false;
            };

            backendProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (!stdoutTimerArmed)
                    {
                        nodeBuffer.AppendLine("Error Data \n");
                        stdoutTimerArmed = true;
                        stdoutFlushTimer.Start();
                    }

                    nodeBuffer.AppendLine(e.Data);
                }
            };

            backendProcess.Start();
            backendProcess.BeginOutputReadLine();
            backendProcess.BeginErrorReadLine();

            //Debug.Log("Node.js backend started.");
        }
        private void StopBackend()
        {
            if (!USELOCALBACKEND)
            {
                return;
            }

            if (backendProcess != null && !backendProcess.HasExited)
            {
                backendProcess.Kill();
                backendProcess.WaitForExit(); // force flush event
                backendProcess.Dispose();
                Debug.Log("Node.js backend stopped.");
            }
        }
        public void SendGridPosition(Vector2Int gridPos)
        {
            StartCoroutine(SendGridPositionCoroutine(gridPos));
        }
        public void SpawnUnit(Vector2Int gridPos, Unit data)
        {
            StartCoroutine(SpawnUnit_Post(gridPos, data));
        }
        public void MoveUnit(Unit unit, List<Vector2Int> path)
        {
            StartCoroutine(MoveUnit_Post(unit, path));
        }
        public void SpawnBuilding(Building building)
        {
            StartCoroutine(PlaceBuilding_Post(building));
        }
        public void GetBiome(Vector2Int gridPos)
        {
            StartCoroutine(GetBiome_Get(gridPos));
        }
        public void UploadMapConfig(Worldmap worldMap)
        {
            if (worldMap == null)
            {
                Debug.LogError("worldMap is null, cannot upload map data.");
                return;
            }

            StartCoroutine(UploadMapConfig_Post(worldMap));
        }
        public void GetGameState()
        {
            StartCoroutine(GetGameState_Get());
        }

        private IEnumerator UploadMapConfig_Post(Worldmap worldMap)
        {
            string gid =  "game/" + AuthManager.GameId;
            MapConfig mc = new MapConfig(worldMap);
            string json = JsonUtility.ToJson(mc, true);

            yield return SendPostRequest("GenerateGrid", json, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    MapConfigResponse response = MapConfigResponse.FromJson(request.downloadHandler.text);

                    Worldmap.Instance.ComputeNoise();

                    float clientHash = Worldmap.Instance.noiseGenerator.NoiseHash;

                    float serverHash = response.noiseHash;

                    // will only be work to a gridSize of size 2000 x 2000
                    float tolerancePercent = 0.1f; // e.g., 0.1% tolerance

                    float difference = Mathf.Abs(clientHash - serverHash);
                    float percentDifference = (difference / Mathf.Abs(serverHash)) * 100f;

                    if (percentDifference <= tolerancePercent)
                    {
                        string grid = worldMap.gridManager.GridSize.x + "  x " + worldMap.gridManager.GridSize.y;

                        //Debug.Log($"Hash Match! " +
                        //    $"Local: {clientHash}, Server: {serverHash}, Difference: {percentDifference}%");

                        Worldmap.Instance.GenerateGrid();
                    }
                    else
                    {
                        Debug.LogError($"Hash mismatch! Local: {clientHash}, Server: {serverHash}, Difference: {percentDifference}%");
                    }
                }
                else
                {
                    Debug.LogError("Error uploading map data: " + request.error);
                }
            });
        }

        public Action<bool, MapConfig> OnMapConfigReceived;
        public void GetMapConfig()
        {
            StartCoroutine(GetMapConfig_Get());
        }

        public Action<bool, HashValidResponse> OnNoiseHashValidated;
        private IEnumerator GetMapConfig_Get()
        {
            string gid =  "game/" + AuthManager.GameId;

            yield return SendGetRequest($"{gid}/GetMapConfig", "", (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    MapConfig response = JsonUtility.FromJson<MapConfig>(json);

                    OnMapConfigReceived?.Invoke(true, response);
                }
                else
                {
                    OnMapConfigReceived?.Invoke(false, new());
                }
            });
        }

        public void ValidateNoiseHash(float clientHash)
        {
            StartCoroutine(ValidateNoiseHash_Post(clientHash));
        }
        private IEnumerator ValidateNoiseHash_Post(float clientHash)
        {
            string gid = "game/" + AuthManager.GameId;
            string query = $"?noiseHash={clientHash}";

            yield return SendGetRequest($"{gid}/validateNoiseHash{query}", "", (request) =>
            {
                string json = request.downloadHandler.text;

                HashValidResponse response = JsonUtility.FromJson<HashValidResponse>(json);

                if (request.result == UnityWebRequest.Result.Success)
                {
                   
                    bool isValid = response.success; // from your backend JSON
                    OnNoiseHashValidated?.Invoke(isValid, response);
                }
                else
                {
                    OnNoiseHashValidated?.Invoke(false, response);
                }
            });
        }
        private IEnumerator SendPostRequest(string endpoint, string json, Action<UnityWebRequest> onComplete)
        {
            string url = BASE_URL + endpoint;

            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // ✅ Add token if available
            if (!string.IsNullOrEmpty(AuthManager.Token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + AuthManager.Token);
            }

            yield return request.SendWebRequest();

            onComplete?.Invoke(request);
        }
        private IEnumerator SendGetRequest(string endpoint, string queryString, Action<UnityWebRequest> onComplete)
        {
            string url = BASE_URL + endpoint;

            if (!string.IsNullOrEmpty(queryString))
            {
                url += "?" + queryString;
            }

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // ✅ Add token if available
            if (!string.IsNullOrEmpty(AuthManager.Token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + AuthManager.Token);
            }

            yield return request.SendWebRequest();

            onComplete?.Invoke(request);
        }

        private IEnumerator SendGridPositionCoroutine(Vector2Int gridPos)
        {
            string json = JsonUtility.ToJson(gridPos);

            yield return SendPostRequest("gridPosition", json, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                    Debug.Log("Grid position sent to backend");
                else
                    Debug.LogError("Error: " + request.error);
            });
        }

        private IEnumerator SpawnUnit_Post(Vector2Int gridPos, Unit data)
        {
            string gid =  "game/" + AuthManager.GameId;

            SpawnUnitPayload payload = new SpawnUnitPayload(gridPos, data);
            string json = JsonUtility.ToJson(payload, true);

            yield return SendPostRequest($"{gid}/spawnunit", json, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    WorldUI.Instance.SpawnUnit(gridPos, data);
                }
                else
                {
                    Debug.LogError("Error Spawning Unit");
                }
            });
        }
        private IEnumerator MoveUnit_Post(Unit data, List<Vector2Int> path)
        {
            string gid =  "game/" + AuthManager.GameId;

            MoveUnitPayload payload = new MoveUnitPayload(data, path);
            string json = JsonUtility.ToJson(payload, true);

            yield return SendPostRequest($"{gid}/moveunit", json, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    WorldUI.Instance.MoveSelectedUnit(data, path);
                }
                else
                {
                    Debug.LogError("Error Moving Unit");
                }
            });
        }
        private IEnumerator PlaceBuilding_Post(Building building)
        {
            string gid =  "game/" + AuthManager.GameId;
            string json = JsonUtility.ToJson(building, true);

            yield return SendPostRequest($"{gid}/placebuilding", json, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    BuildingOverlay.Instance.PlaceBuilding(building);
                    Debug.Log("Building spawned successfully.");
                }
                else
                {
                    Debug.LogError("Error Spawning Building");
                }
            });
        }
        private IEnumerator GetBiome_Get(Vector2Int pos)
        {
            string gid =  "game/" + AuthManager.GameId;
            string queryString = pos.ToQuery();

            yield return SendGetRequest($"{gid}/GetBiome", queryString, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    BiomeData biome = JsonUtility.FromJson<BiomeData>(json);
                    BotOverlay.Instance.SyncServerBiome(biome);
                }
                else
                {
                    Debug.LogError("Error fetching biome:" + request.error);
                }
            });
        }
        private IEnumerator GetGameState_Get()
        {
            string gid =  "game/" + AuthManager.GameId;

            yield return SendGetRequest($"{gid}/GetGameState", "", (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    GameManagerModel gameState = GameManagerModel.FromJson(json);
                    GameStateSynced?.Invoke(gameState);
                }
                else
                {
                    Debug.LogError("Error fetching game state: " + request.error);
                }
            });
        }
        private Coroutine pingCoroutine;

        public void StartPingPolling()
        {
            if (pingCoroutine == null)
            {
                pingCoroutine = StartCoroutine(PingServer());
            }
        }

        public void StopPingPolling()
        {
            if (pingCoroutine != null)
            {
                StopCoroutine(pingCoroutine);
                pingCoroutine = null;
            }
        }

        private IEnumerator PingServer()
        {
            while (true)
            {
                yield return SendGetRequest("auth/ping", "", (request) =>
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        OnPingResponse?.Invoke(true);
                    }
                    else
                    {
                        OnPingResponse?.Invoke(false);
                    }
                });

                yield return new WaitForSeconds(3f);
            }
        }

        public void Login(string username)
        {
            StartCoroutine(Login_Post(username));
        }
        private IEnumerator Login_Post(string username)
        {
            var loginPayload = new UsernameRequest { username = username };

            string json = JsonUtility.ToJson(loginPayload);

            yield return SendPostRequest("auth/login", json, (request) =>
            {
                var responseJson = request.downloadHandler.text;

                LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseJson);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (response.success)
                    {
                        //Debug.Log("Login successful. Token: " + response.token);
                        OnLoginResponse?.Invoke(true, response);
                    }
                    else
                    {
                        OnLoginResponse?.Invoke(false, response);
                    }
                }
                else
                {
                    OnLoginResponse?.Invoke(false, response);
                }
            });
        }

        public void Logout(string username)
        {
            StartCoroutine(SendLogoutRequest(username));
        }

        private IEnumerator SendLogoutRequest(string username)
        {
            var json = JsonUtility.ToJson(new UsernameRequest { username = username });

            yield return SendPostRequest("auth/logout", json, (request) =>
            {
                var responseJson = request.downloadHandler.text;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log(request.downloadHandler.text);
                    OnLoggedOut?.Invoke(true);
                }
                else
                {
                    Debug.LogError(request.downloadHandler.text);
                    OnLoggedOut?.Invoke(false);
                }
            });
        }

        public void JoinQueue()
        {
            StartCoroutine(JoinQueue_Post());
        }
        private IEnumerator JoinQueue_Post()
        {
            yield return SendPostRequest("auth/queue/join", "{}", (request) =>
            {
                QueueJoinResponse response = JsonUtility.FromJson<QueueJoinResponse>(request.downloadHandler.text);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    OnQueueJoined?.Invoke(response.success, response);
                }
                else
                {
                    Debug.LogError("Queue join failed: " + request.error);
                    OnQueueJoined?.Invoke(false, response);
                }
            });
        }

        public void LeaveQueue()
        {
            StartCoroutine(LeaveQueue_Post());
        }
        private IEnumerator LeaveQueue_Post()
        {
            yield return SendPostRequest("auth/queue/leave", "{}", (request) =>
            {
                OnQueueLeft?.Invoke(request.result == UnityWebRequest.Result.Success);
            });
        }

        private Coroutine queueStatus;

        public bool IsPollingQueue => queueStatus != null;
        public void PollQueueStatus()
        {
           queueStatus = StartCoroutine(GetQueueStatus());
        }

        public void StopPollingQueueStatus()
        {
            StopCoroutine(queueStatus);
            queueStatus = null;
        }

        private IEnumerator GetQueueStatus()
        {
            while (true)
            {
                yield return SendGetRequest("auth/queue/status", "", (request) =>
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var response = JsonUtility.FromJson<QueueStatusResponse>(request.downloadHandler.text);
                        OnQueueStatusChecked?.Invoke(response);
                    }
                });

                // Wait 2 seconds before next poll
                yield return new WaitForSeconds(2f); 
            }
        }

        private Coroutine playerStates;

        public void PollPlayerStates()
        {
            playerStates = StartCoroutine(GetPlayerStates());
        }

        public void StopPollingPlayerStates()
        {
            StopCoroutine(playerStates);
        }
        private IEnumerator GetPlayerStates()
        {
            while (true)
            {
                yield return SendGetRequest("playersStatus", "", (request) =>
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var response = JsonUtility.FromJson<PlayersStatusResponse>(request.downloadHandler.text);
                        OnPlayerStatesResponse?.Invoke(response);
                    }
                });

                // Wait 2 seconds before next poll
                yield return new WaitForSeconds(2f);
            }
        }
    }
}