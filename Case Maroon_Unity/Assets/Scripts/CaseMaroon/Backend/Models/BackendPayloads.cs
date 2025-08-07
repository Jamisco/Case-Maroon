using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CaseMaroon.Backend.GlobalModel;
using static CaseMaroon.WorldMap.BiomeGenerator;
using static CaseMaroon.WorldMap.NoiseGenerator;

namespace CaseMaroon.Backend
{
    public class BackendPayloads
    {
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

        public struct MoveUnitPayload
        {
            public UnitDataWrap unit;
            public List<Vector2Int> path;

            public MoveUnitPayload(Unit data, List<Vector2Int> path)
            {
                unit = new UnitDataWrap(data);
                this.path = path;
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

        

    }
}
