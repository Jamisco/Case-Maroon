using CaseMaroon.Miscellaneous;
using CaseMaroon.Units;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static CaseMaroon.WorldMap.BackendMessenger;
using CaseMaroon.WorldMapUI;
using static CaseMaroon.Miscellaneous.JsonHelper;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;
using System;
using System.Timers;

namespace CaseMaroon.WorldMap
{
    public class BackendTester : MonoBehaviour
    {
        public static BackendTester Instance { get; private set; }
        public string BASE_URL = "http://localhost:3001/api";
        public bool USELOCALBACKEND = false;

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

        public void SpawnUnit(Vector2Int gridPos, UnitData data)
        {
            StartCoroutine(SpawnUnit_Post(gridPos, data));
        }
        public void UploadMapData(WorldMapConfig worldConfig)
        {
            if (worldConfig == null)
            {
                Debug.LogError("WorldMapConfig is null, cannot upload map data.");
                return;
            }
            StartCoroutine(UploadMapData_Post(worldConfig));
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

        private IEnumerator SendGridPositionCoroutine(Vector2Int gridPos)
        {
            string json = JsonUtility.ToJson(gridPos);

            yield return SendPostRequest("gridPosition", json, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                    Debug.Log("Grid position sent to backend");
                else
                    Debug.LogError("Error sending grid position: " + request.error);
            });
        }

        private IEnumerator SpawnUnit_Post(Vector2Int gridPos, UnitData data)
        {
            SpawnUnitPayload payload = new SpawnUnitPayload(gridPos, data);

            string json = JsonUtility.ToJson(payload, true);

            yield return SendPostRequest("spawnunit", json, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                    WorldUI.Instance.SpawnUnit(gridPos, data);
                else
                    Debug.LogError("Error spawning unit: " + request.error);
            });
        }
        private IEnumerator UploadMapData_Post(WorldMapConfig worldConfig)
        {
            string url = BASE_URL + "mapdata";
            string json = JsonUtility.ToJson(worldConfig, true);

            yield return SendPostRequest("mapdata", json, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Worldmap.Instance.GenerateGrid();
                }
                else
                {
                    Debug.LogError("Error uploading map data: " + request.error);
                }

            });
        }

    }
}