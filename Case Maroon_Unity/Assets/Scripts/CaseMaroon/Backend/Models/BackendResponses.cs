using UnityEngine;
using System;
using System.Collections.Generic;
using static CaseMaroon.Miscellaneous.GlobalData;
using CaseMaroon.Units;
using CaseMaroon.GameSystem;
using CaseMaroon.WorldMap;

using static CaseMaroon.Backend.BackendModels;

namespace CaseMaroon.Backend
{
    public static class BackendResponses
    {
        [Serializable]
        public struct PlacedBuildingResponse
        {
            public bool success;
            public List<ReconPosition> reconPositions;

        }

        [Serializable]
        public struct MapConfigResponse
        {
            public bool success;
            public string message;
            public float noiseHash;

            public static MapConfigResponse FromJson(string json)
            {
                return JsonUtility.FromJson<MapConfigResponse>(json);
            }
        }

        [System.Serializable]
        public struct LoginResponse
        {
            public bool success;
            public string message;
            public string token;
            public string username;
        }

        // Add these classes for queue responses
        [System.Serializable]
        public struct QueueJoinResponse
        {
            public bool success;
            public string message;
        }

        [System.Serializable]
        public struct QueueStatusResponse
        {
            public bool success;
            public int playersInQueue;

            public bool gameFound;
            public string gameId;
            public string opponentName;
        }

        [System.Serializable]
        public struct PlayerStatus
        {
            public string id;
            public int state;
        }

        [System.Serializable]
        public struct PlayersStatusResponse
        {
            public bool success;
            public PlayerStatus[] players;
        }

        [System.Serializable]
        public class HashValidResponse
        {
            public bool success;
            public string message;
            public float serverHash;
        }
    }
}
