using UnityEngine;
using System;
using System.Collections.Generic;
using static CaseMaroon.Miscellaneous.GlobalData;
using CaseMaroon.Units;
using CaseMaroon.GameSystem;
using CaseMaroon.WorldMap;

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

        [Serializable]
        public struct PlayerResponse
        {
            public int id;
            public List<ReconPosition > reconPositions;
            public List<Vector2Int> ownedPositions;
        }

        [Serializable]
        public struct GameStateResponse
        {
            public List<PlayerResponse> players;
            public Vector2Int gridSize;

            public static GameStateResponse FromJson(string json)
            {
                return JsonUtility.FromJson<GameStateResponse>(json);
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

            public string gameId;
            public string opponent;

            public int queuePosition;
            public int playersInQueue;
        }

        // Add these classes for queue responses
        [System.Serializable]
        public struct GameFoundResponse
        {
            public bool gameFound;
            public string message;

            public string gameId;
            public string opponent;

            public int playerId;
        }

        [System.Serializable]
        public struct QueueStatusResponse
        {
            public bool success;
            public bool isInQueue;

            public int queuePosition;
            public int playersInQueue;

            public string estimatedWaitTime;
        }

    }
}
