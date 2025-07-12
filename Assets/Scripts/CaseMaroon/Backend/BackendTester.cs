using CaseMaroon.Backend;
using CaseMaroon.GameSystem;
using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using CaseMaroon.WorldMapUI;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Timers;
using UnityEngine;
using UnityEngine.Networking;
using static CaseMaroon.Backend.BackendPayloads;
using static CaseMaroon.Backend.BackendResponses;
using Debug = UnityEngine.Debug;

namespace CaseMaroon.Backend
{
    public delegate void GameStateSyncedHandler(GameStateResponse gsr);

    public class BackendTester : MonoBehaviour
    {
        public static BackendTester Instance { get; private set; }
        public string BASE_URL = "http://localhost:3001/api";
        public bool USELOCALBACKEND = false;

        public event GameStateSyncedHandler GameStateSynced;

        private void Awake()
        {
            // Ensure only one instance exists
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

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

            StartBackend();
        }

        void OnApplicationQuit()
        {
            StopBackend();
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

            string scriptPath = @"W:\Unity Projects\Case Maroon Root\Case Maroon_Backend\src\index.js";

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
                    Debug.Log(nodeBuffer.ToString());
                    nodeBuffer.Clear();
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

            StartCoroutine(GenerateGrid_Post(worldMap));
        }
        public void SyncGameState()
        {
            StartCoroutine(GetGameState_Get());
        }

        private IEnumerator GenerateGrid_Post(Worldmap worldMap)
        {
            string url = BASE_URL + "GenerateGrid";

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
        private IEnumerator SendPostRequest(string endpoint, string json, System.Action<UnityWebRequest> onComplete)
        {
            string url = BASE_URL + endpoint;

            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            onComplete?.Invoke(request);
        }

        private IEnumerator SendGetRequest(string endpoint, string queryString, System.Action<UnityWebRequest> onComplete)
        {
            string url = BASE_URL + endpoint + "?" + queryString;

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

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
            SpawnUnitPayload payload = new SpawnUnitPayload(gridPos, data);

            string json = JsonUtility.ToJson(payload, true);

            yield return SendPostRequest("spawnunit", json, (request) =>
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

        private IEnumerator PlaceBuilding_Post(Building building)
        {
            string url = BASE_URL + "placebuilding";
            string json = JsonUtility.ToJson(building, true);

            yield return SendPostRequest("placebuilding", json, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    BuildingOverlay.Instance.PlaceBuilding(building);
                    // Handle successful building spawn
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
            // the problem is that MapConfig.BiomeRules is not passing the list of biome rules.
            // check the generate grid route function
            string queryString = pos.ToQuery();

            yield return SendGetRequest("GetBiome", queryString, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    BiomeData biome = JsonUtility.FromJson<BiomeData>(json);

                    Debug.Log("Received Biome:\n " + json);
                }
                else
                {
                    Debug.LogError("Error fetching biome:" + request.error);
                }
            });
        }
        private IEnumerator GetGameState_Get()
        {
            // No query string needed if it's a general state fetch
            yield return SendGetRequest("GetGameState", "", (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;

                    GameStateResponse gameState = GameStateResponse.FromJson(json);

                    GameStateSynced?.Invoke(gameState);

                    Debug.Log("Received Game State:\n" + json);

                    // You can now use `gameState` to update the client view
                }
                else
                {
                    Debug.LogError("Error fetching game state: " + request.error);
                }
            });
        }
    }
}