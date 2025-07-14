using CaseMaroon.Miscellaneous;
using GridMapMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CaseMaroon.WorldMap
{
    [CreateAssetMenu(fileName = "BiomeConfig", menuName = "CaseMaroon/Biome Generator")]
    public class BiomeGenerator : ScriptableObject
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
        public struct BiomeTraversalCost
        {
            // set to -1 to denote cant pass
            public int InfantryCost;

            public int TrackedCost;
        }

        [Serializable]
        public struct BiomeRules
        {
            public BiomeType biomeType;

            [Vector2Range(0f, 1f, 0f, 1f)]
            public Vector2 tempRange;

            [Vector2Range(0f, 1f, 0f, 1f)]
            public Vector2 rainRange;

            public BiomeTraversalCost traversalCost;

            public bool WithinRules(float temp, float rain)
            {
                return temp >= tempRange.x && temp <= tempRange.y &&
                       rain >= rainRange.x && rain <= rainRange.y;
            }
        }

        [Serializable]
        public struct BiomeVisualHolder
        {
            public Material LandMaterial;
            public Material SnowMaterial;
            public Material LavaMaterial;
            public Material WaterMaterial;
            public Material HighlightMaterial;

            public Color highlightColor;

            [Tooltip("Rounding factor for the biome data. Used to group noise values. For example, if you have 2 noise values .82 and .81, and your Rounding factor is 5, these noise values will be rounded to the next .05 values, thus, the noise value will actually be .80. A low rounding factor means more unique values vice value of high.")]
            [Range(1, 50)]
            public int roundingFactor;
        }

        public BiomeVisualHolder biomeProp;

        public float waterThreshold;
        public float snowThreshold;

        public List<BiomeRules> biomeRules = new();

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
        public BiomeData GetMatchingBiome(float land, float temperature, float rainfall)
        {
            BiomeRules rule = new();

            if (land < waterThreshold)
            {
                rule = biomeRules.FirstOrDefault(x => x.biomeType == BiomeType.Ocean);
            }
            else
            {
                foreach (BiomeRules r in biomeRules)
                {
                    if (r.biomeType == BiomeType.Ocean)
                    {
                        continue;
                    }

                    if(r.WithinRules(temperature, rainfall))
                    {
                        rule = r;
                    }
                }
            }

            return new BiomeData(rule.biomeType, temperature, rainfall, rule.traversalCost);
        }
        public BiomeData GetMatchingBiome(float temperature, float rainfall)
        {
            BiomeRules rule = new();

            foreach (BiomeRules r in biomeRules)
            {
                if (r.WithinRules(temperature, rainfall))
                {
                    rule = r;
                }
            }

            return new BiomeData(rule.biomeType, temperature, rainfall, rule.traversalCost);
        }
        private void UseDefaultRules()
        {
            biomeRules = GetDefaultRules();
        }

        public ShapeVisualData GetLandVisualData(float land, float rain, float temp)
        {
            if (land < waterThreshold)
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
            if (temp <= snowThreshold)
            {
                float normalize = 1 - temp / snowThreshold;
                normalize = Math.Clamp(Mathf.RoundToInt(normalize * 10), 0, 10);

                SnowVisualData svd = new SnowVisualData(biomeProp.SnowMaterial,
                                    normalize / 10f);
            }

            return null;
        }

        public bool IsLand(float land)
        {
            return land > waterThreshold;
        }

        // this shouldn't matter
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
        public static List<BiomeRules> GetDefaultRules()
        {
            return new List<BiomeRules>
            {
                new BiomeRules
                {
                    biomeType = BiomeType.Tundra,
                    tempRange = new Vector2(0.0f, 0.33f),
                    rainRange = new Vector2(0.0f, 0.33f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = 25, TrackedCost = 35 }
                },
                new BiomeRules
                {
                    biomeType = BiomeType.Taiga,
                    tempRange = new Vector2(0.0f, 0.33f),
                    rainRange = new Vector2(0.33f, 0.66f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = 30, TrackedCost = 45 }
                },
                new BiomeRules
                {
                        biomeType = BiomeType.SnowForest,
                    tempRange = new Vector2(0.0f, 0.33f),
                    rainRange = new Vector2(0.66f, 1.0f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = 35, TrackedCost = 50 }
                },

                new BiomeRules
                {
                    biomeType = BiomeType.Desert,
                    tempRange = new Vector2(0.33f, 0.66f),
                    rainRange = new Vector2(0.0f, 0.33f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = 30, TrackedCost = 45 }
                },
                new BiomeRules
                {
                    biomeType = BiomeType.Grassland,
                    tempRange = new Vector2(0.33f, 0.66f),
                    rainRange = new Vector2(0.33f, 0.66f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = 10, TrackedCost = 10 }
                },
                new BiomeRules
                {
                    biomeType = BiomeType.DeciduousForest,
                    tempRange = new Vector2(0.33f, 0.66f),
                    rainRange = new Vector2(0.66f, 1.0f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = 25, TrackedCost = 35 }
                },

                new BiomeRules
                {
                    biomeType = BiomeType.Savannah,
                    tempRange = new Vector2(0.66f, 1.0f),
                    rainRange = new Vector2(0.0f, 0.33f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = 15, TrackedCost = 20 }
                },
                new BiomeRules
                {
                    biomeType = BiomeType.Swamp,
                    tempRange = new Vector2(0.66f, 1.0f),
                    rainRange = new Vector2(0.33f, 0.66f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = 50, TrackedCost = -1 } // Tracked can't pass
                },
                new BiomeRules
                {
                    biomeType = BiomeType.Rainforest,
                    tempRange = new Vector2(0.66f, 1.0f),
                    rainRange = new Vector2(0.66f, 1.0f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = 55, TrackedCost = -1 } // Tracked can't pass
                },
                new BiomeRules
                {
                    biomeType = BiomeType.Ocean,
                    tempRange = new Vector2(0f, 1f),
                    rainRange = new Vector2(0f, 1f),
                    traversalCost = new BiomeTraversalCost { InfantryCost = -1, TrackedCost = -1 }
                },
            };
        }

        public string ToJson()
        {
            BiomeConfigSerializer serializer = new BiomeConfigSerializer(this);
            return JsonUtility.ToJson(serializer, true);
        }

        public void FromJson(string json)
        {
            BiomeConfigSerializer serializer = JsonUtility.FromJson<BiomeConfigSerializer>(json);

            waterThreshold = serializer.waterThreshold;
            snowThreshold = serializer.snowThreshold;
            biomeRules = serializer.biomeRules;
        }

        [Serializable]
        public struct BiomeConfigSerializer
        {
            public float waterThreshold;
            public float snowThreshold;
            public List<BiomeRules> biomeRules;

            public BiomeConfigSerializer(BiomeGenerator config)
            {
                biomeRules = config.biomeRules;
                waterThreshold = config.waterThreshold;
                snowThreshold = config.snowThreshold;
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(BiomeGenerator))]
        public class BiomeConfigEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                BiomeGenerator exampleScript = (BiomeGenerator)target;

                if (GUILayout.Button("Use Default Values"))
                {
                    exampleScript.UseDefaultRules();
                }
            }
        }
#endif

    }

}