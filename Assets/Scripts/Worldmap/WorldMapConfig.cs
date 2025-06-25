using System;
using UnityEngine;
using static CaseMaroon.WorldMap.NoiseGenerator;

namespace CaseMaroon.WorldMap
{
    [Serializable]
    [CreateAssetMenu(fileName = "WorldMapConfig",
                     menuName = "CaseMaroon/Map Config")]
    public class WorldMapConfig : ScriptableObject
    {
        public Vector2 shapeScale;
        public Vector2Int gridSize;
        public Vector2Int chunkSize;

        public NoiseSettings landNoiseSettings;
        public NoiseSettings rainNoiseSettings;
        public NoiseSettings tempNoiseSettings;

        public void UpdateConfigFromMap(Worldmap map)
        {
            if (map == null)
            {
                Debug.LogError("Worldmap is null, cannot update config.");
                return;
            }

            shapeScale = map.ShapeScale;
            gridSize = map.gridManager.GridSize;
            chunkSize = map.gridManager.ChunkSize;

            landNoiseSettings = map.noiseGenerator.landNoiseSettings;
            rainNoiseSettings = map.noiseGenerator.rainNoiseSettings;
            tempNoiseSettings = map.noiseGenerator.tempNoiseSettings;
        }

        public void UpdateMapFromConfig(Worldmap map)
        {
            if (map == null)
            {
                Debug.LogError("Worldmap is null, cannot update config.");
                return;
            }

            map.ShapeScale = shapeScale;
            map.gridManager.GridSize = gridSize;
            map.gridManager.ChunkSize = chunkSize;

            map.noiseGenerator.landNoiseSettings = landNoiseSettings;
            map.noiseGenerator.rainNoiseSettings = rainNoiseSettings;
            map.noiseGenerator.tempNoiseSettings = tempNoiseSettings;
        }
    }

    

}
