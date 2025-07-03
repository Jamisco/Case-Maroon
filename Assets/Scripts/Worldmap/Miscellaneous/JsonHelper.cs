using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static CaseMaroon.WorldMap.BiomeGenerator;
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

            public UnitDataWrap(Unit data)
            {
                unitId = data.UnitId;
                movePoints = data.MovementPoints;
                unitType = data.UnitType.ToString();
                gridPosition = new Vector2IntWrap(data.GridPosition.x, data.GridPosition.y);
            }
        }

        [Serializable]
        public struct SpawnUnitPayload
        {
            public Vector2Int gridPosition;
            public UnitDataWrap unit;

            public SpawnUnitPayload(Vector2Int pos, Unit data)
            {
                gridPosition = pos;
                unit = new UnitDataWrap(data);
            }
        }

        [Serializable]
        public struct MapConfig
        {
            public WorldmapConfig worldmapConfig;
            public NoiseConfig noiseConfig;
            public BiomeConfig biomeConfig;

            public MapConfig(Worldmap map)
            {
                worldmapConfig = new WorldmapConfig(map);
                noiseConfig = new NoiseConfig(map.noiseGenerator);
                biomeConfig = new BiomeConfig(map.biomeGenerator);
            }
        }

        [Serializable]
        public struct MapConfigResponse
        {
            public string message;
            public bool sucess;
            public float noiseHash;

            public static MapConfigResponse FromJson(string json)
            {
               return JsonUtility.FromJson<MapConfigResponse>(json);
            }
        }




        [Serializable]
        public struct WorldmapConfig
        {
            public Vector2 shapeScale;
            public Vector2Int gridSize;
            public Vector2Int chunkSize;

            public WorldmapConfig(Worldmap map)
            {
                shapeScale = map.ShapeScale;
                gridSize = map.gridManager.GridSize;
                chunkSize = map.gridManager.ChunkSize;
            }

        }

        [Serializable]
        public struct NoiseConfig
        {
            public NoiseSettings landNoiseSettings;
            public NoiseSettings rainNoiseSettings;
            public NoiseSettings tempNoiseSettings;

            public NoiseConfig(NoiseGenerator noiseGenerator)
            {
                landNoiseSettings = noiseGenerator.landNoiseSettings;
                rainNoiseSettings = noiseGenerator.rainNoiseSettings;
                tempNoiseSettings = noiseGenerator.tempNoiseSettings;
            }
        }

        [Serializable]
        public struct BiomeConfig
        {
            public float waterThreshold;
            public float snowThreshold;

            public List<BiomeRules> biomeRules;

            public BiomeConfig(BiomeGenerator biomeGenerator)
            {
                waterThreshold = biomeGenerator.waterThreshold;
                snowThreshold = biomeGenerator.snowThreshold;
                biomeRules = biomeGenerator.biomeRules;
            }
        }

        public static string ToJson(this Vector2Int v)
        {
            return JsonUtility.ToJson(v);
        }
        public static string ToQuery(this Vector2Int v)
        {
            return $"x={v.x}&y={v.y}";
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

        public static string ToJson(this Unit unit)
        {
            return JsonUtility.ToJson(new UnitDataWrap(unit));
        }
    }
}
