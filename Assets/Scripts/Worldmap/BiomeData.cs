using CaseMaroon.Units;
using CaseMaroon.WorldMapUI;
using System.Collections.Generic;
using UnityEngine;
using static CaseMaroon.WorldMap.BiomeGenerator;

namespace CaseMaroon.WorldMap
{
    [System.Serializable]
    public class BiomeData
    {
        public BiomeType biomeType;
        public float temperature;
        public float rain;

        public BiomeTraversalCost moveCost;

        public BiomeData(BiomeType biomeType, float temperature, float rain, BiomeTraversalCost cost)
        {
            this.biomeType = biomeType;
            this.temperature = temperature;
            this.rain = rain;
            this.moveCost = cost;
        }

        public List<StatItemCard> CreateList(StatItemCard prefab)
        {
            List<StatItemCard> stats = new List<StatItemCard>();

            StatItemCard t = Object.Instantiate(prefab);

            t.Label = "Temperature:";
            t.Value = temperature.ToString("F2");

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
        public int GetMovementCost(MovementType movementType)
        {
            return movementType switch
            {
                MovementType.Feet => moveCost.InfantryCost,
                MovementType.Tracked => moveCost.TrackedCost,
                _ => moveCost.InfantryCost,
            };
        }
    }
}
