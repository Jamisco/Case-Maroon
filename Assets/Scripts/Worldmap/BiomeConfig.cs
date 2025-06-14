using GridMapMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CaseMaroon.WorldMap
{
    public enum BiomeType
    {
        Tundra,
        Taiga,
        SnowForest,
        Grassland,
        DeciduousForest,
        Swamp,
        Desert,
        Savannah,
        Rainforest,
        Ocean
    }

    [Serializable]
    public struct MoveCost
    {
        // set to -1 to denote cant pass
        public int InfantryCost;

        public int TrackedCost;
    }


    [Serializable]
    public class BiomeProperties
    {
        public Material LandMaterial;
        public Material SnowMaterial;
        public Material LavaMaterial;
        public Material WaterMaterial;
        public Material HighlightMaterial;
        public Color highlightColor;

        public float waterThreshold;
        public float snowThreshhold;

        [Tooltip("Rounding factor for the biome data. Used to group noise values. For example, if you have 2 noise values .82 and .81, and your Rounding factor is 5, these noise values will be rounded to the next .05 values, thus, the noise value will actually be .80. A low rounding factor means more unique values vice value of high.")]
        [Range(1, 50)]
        public int roundingFactor = 1;
    }

    [CreateAssetMenu(fileName = "BiomeConfig", menuName = "CaseMaroon/Biome Config")]
    public class BiomeConfig : ScriptableObject
    {
        public BiomeProperties biomeProp;

        public List<BiomeData> biomeRules = new();

        // BY default it has to be set to null so the property can init it
        [HideInInspector]
        private ColorVisualData highlightVisualData = null;
        public ShapeVisualData HighlightVisualData
        {
            get
            {
                if (biomeProp.HighlightMaterial == null)
                {
                    Debug.LogError("Highlight material is not set in BiomeProperties.");
                    return null;
                }

                return new ColorVisualData(biomeProp.HighlightMaterial, biomeProp.highlightColor);
            }
        }

        private void OnValidate()
        {
            for (int i = 0; i < biomeRules.Count; i++)
            {
                var a = biomeRules[i];
                Rect aRect = new Rect(a.tempRange.x, a.rainRange.x,
                                      a.tempRange.y - a.tempRange.x,
                                      a.rainRange.y - a.rainRange.x);

                for (int j = i + 1; j < biomeRules.Count; j++)
                {
                    var b = biomeRules[j];
                    Rect bRect = new Rect(b.tempRange.x, b.rainRange.x,
                                          b.tempRange.y - b.tempRange.x,
                                          b.rainRange.y - b.rainRange.x);

                    if (aRect.Overlaps(bRect))
                    {
                        Debug.LogWarning($"Biome overlap detected: {a.biomeType} and {b.biomeType}", this);
                    }
                }
            }
        }

        /// Returns the first matching biome for given temperature and rainfall
        public BiomeData GetMatchingRule(float land, float temperature, float rainfall)
        {
            if (land < biomeProp.waterThreshold)
            {
                return biomeRules.FirstOrDefault(x => x.biomeType == BiomeType.Ocean);
            }

            foreach (BiomeData rule in biomeRules)
            {
                if (temperature >= rule.tempRange.x && temperature <= rule.tempRange.y &&
                    rainfall >= rule.rainRange.x && rainfall <= rule.rainRange.y)
                {
                    return rule;
                }
            }

            return null;
        }

        public BiomeData GetMatchingRule(float temperature, float rainfall)
        {
            foreach (BiomeData rule in biomeRules)
            {
                if (temperature >= rule.tempRange.x && temperature <= rule.tempRange.y &&
                    rainfall >= rule.rainRange.x && rainfall <= rule.rainRange.y)
                {
                    return rule;
                }
            }

            return null;
        }

        private void UseDefaultRules()
        {
            biomeRules = GetDefaultRules();
        }

        public ShapeVisualData GetLandVisualData(float land, float rain, float temp)
        {
            if (land < biomeProp.waterThreshold)
            {
                return new WaterVisualData(biomeProp.WaterMaterial);
            }
            else
            {
                rain = Round(rain);
                temp = Round(temp);

                if (rain <= .1 && temp >= .85 && land >= .85)
                {
                    return new LavaVisualData(biomeProp.LavaMaterial);
                }

                LandVisualData v = new LandVisualData(biomeProp.LandMaterial, temp, rain);

                return v;
            }
        }
        public ShapeVisualData GetSnowVisualData(float temp)
        {
            if (temp <= biomeProp.snowThreshhold)
            {
                float normalize = 1 - temp / biomeProp.snowThreshhold;
                normalize = Math.Clamp(Mathf.RoundToInt(normalize * 10), 0, 10);

                SnowVisualData svd = new SnowVisualData(biomeProp.SnowMaterial, 
                                    normalize / 10f);
            }

            return null;
        }

        // this shouldnt matter
        private float Round(float number)
        {
            // round to 2 decimal places
            number = (float)Math.Round(number, 2);

            float rf = biomeProp.roundingFactor / 100f;
            return (float)Math.Round(number / rf) * rf;
        }

        /// <summary>
        /// Returns default BiomeRules based on a fixed 2D biome graph (temperature x rain).
        /// </summary>
        private static List<BiomeData> GetDefaultRules()
        {
            return new List<BiomeData>
            {
                new BiomeData(BiomeType.Tundra, new Vector2(0.0f, 0.33f), new Vector2(0.0f, 0.33f), new MoveCost { InfantryCost = 25, TrackedCost = 35 }),
                new BiomeData(BiomeType.Taiga, new Vector2(0.0f, 0.33f), new Vector2(0.33f, 0.66f), new MoveCost { InfantryCost = 30, TrackedCost = 45 }),
                new BiomeData(BiomeType.SnowForest, new Vector2(0.0f, 0.33f), new Vector2(0.66f, 1.0f), new MoveCost { InfantryCost = 35, TrackedCost = 50 }),

                new BiomeData(BiomeType.Desert, new Vector2(0.33f, 0.66f), new Vector2(0.0f, 0.33f), new MoveCost { InfantryCost = 30, TrackedCost = 45 }),
                new BiomeData(BiomeType.Grassland, new Vector2(0.33f, 0.66f), new Vector2(0.33f, 0.66f), new MoveCost { InfantryCost = 10, TrackedCost = 10 }),
                new BiomeData(BiomeType.DeciduousForest, new Vector2(0.33f, 0.66f), new Vector2(0.66f, 1.0f), new MoveCost { InfantryCost = 25, TrackedCost = 35 }),

                new BiomeData(BiomeType.Savannah, new Vector2(0.66f, 1.0f), new Vector2(0.0f, 0.33f), new MoveCost { InfantryCost = 15, TrackedCost = 20 }),
                new BiomeData(BiomeType.Swamp, new Vector2(0.66f, 1.0f), new Vector2(0.33f, 0.66f), new MoveCost { InfantryCost = 50, TrackedCost = -1 }), // Tracked can't pass
                new BiomeData(BiomeType.Rainforest, new Vector2(0.66f, 1.0f), new Vector2(0.66f, 1.0f), new MoveCost { InfantryCost = 55, TrackedCost = -1 }), // Tracked can't pass
                new BiomeData(BiomeType.Ocean, new Vector2(0f, 1f), new Vector2(0f, 1f), new MoveCost { InfantryCost = -1, TrackedCost = -1 }),

            };
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(BiomeConfig))]
        public class BiomeConfigEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                BiomeConfig exampleScript = (BiomeConfig)target;

                if (GUILayout.Button("Use Default Values"))
                {
                    exampleScript.UseDefaultRules();
                }
            }
        }
#endif

    }

}