using UnityEngine;
using System;
using System.Collections.Generic;
using static CaseMaroon.Miscellaneous.GlobalData;

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

    }
}
