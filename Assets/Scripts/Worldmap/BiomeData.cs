using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Worldmap.Miscellaneous;
using CaseMaroon.Units;
using CaseMaroon.WorldMapUI;

namespace CaseMaroon.WorldMap
{
    [System.Serializable]
    public class BiomeData
    {
        public BiomeType biomeType;

        [Vector2Range(0f, 1f, 0f, 1f)]
        public Vector2 tempRange;

        [Vector2Range(0f, 1f, 0f, 1f)]
        public Vector2 rainRange;

        public MoveCost movementCost;

        public struct BiomeStats
        {
            public float temp;
            public float rain;
            public BiomeType biomeType;
            public MoveCost moveCost;

            public List<StatItemCard> CreateList(StatItemCard prefab)
            {
                List<StatItemCard> stats = new List<StatItemCard>();

                StatItemCard t = Object.Instantiate(prefab);

                t.Label = "Temperature:";
                t.Value = temp.ToString("F2");

                stats.Add(t);

                StatItemCard ra = Object.Instantiate(prefab);

                ra.Label = "Rainfall:";
                ra.Value = rain.ToString("F2");

                stats.Add(ra);

                StatItemCard biome = Object.Instantiate(prefab);

                biome.Label = "Biome Type:";
                biome.Value = biomeType.ToString();

                stats.Add(biome);

                StatItemCard moveInf = Object.Instantiate(prefab);

                moveInf.Label = "Infantry Move Cost:";
                moveInf.Value = moveCost.InfantryCost.ToString();

                StatItemCard moveTracked = Object.Instantiate(prefab);
                moveTracked.Label = "Tracked Move Cost:";
                moveTracked.Value = moveCost.TrackedCost.ToString();

                stats.Add(moveInf);

                stats.Add(moveTracked);

                return stats;
            }
        }

        public BiomeData(BiomeType biomeType, Vector2 tempRange, Vector2 rainRange, MoveCost moveCost)
        {
            this.biomeType = biomeType;
            this.tempRange = tempRange;
            this.rainRange = rainRange;
            this.movementCost = moveCost;
        }

        public int GetMovementCost(MovementType movementType)
        {
            return movementType switch
            {
                MovementType.Feet => movementCost.InfantryCost,
                MovementType.Tracked => movementCost.TrackedCost,
                _ => movementCost.InfantryCost,
            };
        }
    }
}
