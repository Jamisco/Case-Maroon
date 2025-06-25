using CaseMaroon.Units;
using System;
using UnityEngine;
using static CaseMaroon.WorldMap.NoiseGenerator;

namespace CaseMaroon.Miscellaneous
{
    public static class JsonHelper
    {
        [Serializable]
        public struct Vector2IntWrap
        {
            public int x;
            public int y;
            public Vector2IntWrap(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [Serializable]
        public struct UnitDataWrap
        {
            public int unitId;
            public string unitType;
            public Vector2IntWrap gridPosition;
            public int movePoints;

            public UnitDataWrap(UnitData data)
            {
                unitId = data.UnitId;
                movePoints = data.MovementPoints;
                unitType = data.UnitType.ToString();
                gridPosition = new Vector2IntWrap(data.GridPosition.x, data.GridPosition.y);
            }
        }

        [System.Serializable]
        public struct SpawnUnitPayload
        {
            public Vector2Int position;
            public UnitDataWrap unit;

            public SpawnUnitPayload(Vector2Int pos, UnitData data)
            {
                position = pos;
                unit = new UnitDataWrap(data);
            }
        }

        public struct MapGenerationSettings
        {
            public Vector2 ShapeScale;
            public Vector2Int GridSize;
            public Vector2Int ChunkSize;

            public NoiseSettings LandNoiseSettings;
            public NoiseSettings RainNoiseSettings;
            public NoiseSettings TempNoiseSettings;
        }

        public static string ToJson(this Vector2Int v)
        {
            return JsonUtility.ToJson(v);
        }

        public static Vector2Int ToVector2Int(this string json)
        {
            Vector2Int data;

            try
            {
                Vector2IntWrap wrap = JsonUtility.FromJson<Vector2IntWrap>(json);
                data = new Vector2Int(wrap.x, wrap.y);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse JSON to Vector2Int: {e.Message}");
                data = Vector2Int.zero; // Default value in case of error
            }

            return data;
        }



        public static string ToJson(this UnitData unit)
        {
            return JsonUtility.ToJson(new UnitDataWrap(unit));
        }
    }
}
