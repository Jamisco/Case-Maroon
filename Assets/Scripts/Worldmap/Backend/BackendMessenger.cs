using CaseMaroon.Units;
using CaseMaroon.WorldMapUI;
using System.Runtime.InteropServices;
using UnityEngine;
using static CaseMaroon.Miscellaneous.JsonHelper;

namespace CaseMaroon.WorldMap
{
    public class BackendMessenger : MonoBehaviour
    {
        public const string POST = "POST";
        public const string GET = "GET";
        public const string PUT = "PUT";
        public static BackendMessenger Instance { get; private set; }

        [DllImport("__Internal")]
        private static extern void SendGridPositionToJS(string positionJson);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject); // if needed
        }

        public void SendGridPos(Vector2Int position)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                string json = JsonUtility.ToJson(position);
                SendGridPositionToJS(json);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                BackendTester.Instance.SendGridPosition(position);
            }
        }

        public void SpawnUnit(Vector2Int position, UnitData data)
        {
            if(Application.platform == RuntimePlatform.WebGLPlayer)
            {
                //string json = JsonUtility.ToJson(data);
                //SendUnitDataToJS(position, json);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                BackendTester.Instance.SpawnUnit(position, data);
            }
        }

        public void UploadMapData(Worldmap worldMap)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                //string json = JsonUtility.ToJson(worldConfig);
                //SendMapDataToJS(json);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                BackendTester.Instance.UploadMapConfig(worldMap);
            }
        }

        public void UploadMapConfig(MapConfig mapConfig)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                
            }
        }
    }
}
