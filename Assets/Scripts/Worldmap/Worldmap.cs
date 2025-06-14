using GridMapMaker;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static CaseMaroon.WorldMap.BiomeData;
using static CaseMaroon.WorldMap.WorldmapOverlay;

namespace CaseMaroon.WorldMap
{
    public delegate void WorldGenerated(Worldmap map);

    public class Worldmap : MonoBehaviour
    {
        public static Worldmap Instance { get; private set; }
        public event WorldGenerated OnWorldGenerated;

        // create grid generated event
        public GridManager gridManager;
        public BiomeConfig biomeConfig;
        public NoiseGenerator noiseGenerator;

        [SerializeField]
        public MeshLayerSettings baseLayer;

        [SerializeField]
        public MeshLayerSettings snowLayer;

        [SerializeField]
        public MeshLayerSettings highlightLayer;

        public ColorVisualData highlightVisualData;

        [Tooltip("Only works when game is running.")]
        public bool instantUpdate = false;

        [Tooltip("If true, will insert all visual data as a block instead of individually ")]
        public bool blockInsert = false;

        [NonSerialized]
        public bool generating = false;
        //
        [NonSerialized]
        public bool gridGenerated = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);  // Prevent duplicates
                return;
            }

            Instance = this;
        }

        public void Start()
        {
            try
            {
                Init();
                GenerateGrid();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Can't Genetate World map, " + ex.Message);
                throw;
            }
        }

        public void OnValidate()
        {
            if(gridManager == null)
            {
                Init();
            }

        }
        public void Init()
        {
            gridManager = GetComponent<GridManager>();
            noiseGenerator = GetComponent<NoiseGenerator>();
            
        }
        public void ComputeNoise()
        {
            noiseGenerator.ComputeNoises(gridManager.GridSize, false);
        }

        public void Update()
        {
            if (noiseGenerator.NoiseModified && instantUpdate)
            {
                GenerateGrid();
            }
        }
        public void GenerateGrid()
        {
            if (generating)
            {
                return;
            }

            generating = true;

            ComputeNoise();

            gridManager.Initialize();
            gridManager.CreateLayer(baseLayer, true);
            gridManager.CreateLayer(snowLayer, false);
            gridManager.CreateLayer(highlightLayer, false);

            Vector2Int pos;
            ShapeVisualData vData;
            ShapeVisualData snowData;

            for (int x = 0; x < gridManager.GridSize.x; x++)
            {
                for (int y = 0; y < gridManager.GridSize.y; y++)
                {
                    pos = new Vector2Int(x, y);

                    float land = noiseGenerator.GetLandNoise(x, y);
                    float temp = noiseGenerator.GetTempNoise(x, y);
                    float rain = noiseGenerator.GetRainNoise(x, y);

                    vData = biomeConfig.GetLandVisualData(land, temp, rain);
                    snowData = biomeConfig.GetSnowVisualData(temp);

                    gridManager.InsertVisualData(pos, vData);
                    gridManager.InsertVisualData(pos, snowData, snowLayer.LayerId);

                }
            }

            gridManager.DrawGrid();

            AddPolyCollider();

            generating = false;

            OnWorldGenerated?.Invoke(this);
        }   

        //private void NoiseModified()
        //{
        //    ComputeNoise();

        //    Vector2Int pos;
        //    ShapeVisualData vData;
        //    ShapeVisualData snowData;

        //    for (int x = 0; x < gridManager.GridSize.x; x++)
        //    {
        //        for (int y = 0; y < gridManager.GridSize.y; y++)
        //        {
        //            pos = new Vector2Int(x, y);

        //            float land = noiseGenerator.GetLandNoise(x, y);
        //            float temp = noiseGenerator.GetTempNoise(x, y);
        //            float rain = noiseGenerator.GetRainNoise(x, y);

        //            vData = biomeConfig.GetLandVisualData(land, temp, rain);
        //            snowData = biomeConfig.GetSnowVisualData(temp);

        //            gridManager.InsertVisualData(pos, vData);
        //            gridManager.InsertVisualData(pos, snowData, snowLayer.LayerId);

        //        }
        //    }

        //    gridManager.DrawGrid();

        //}

        public string saveLocation = "Assets/Worldmap/WorldmapSave.txt";

        public PolygonCollider2D polygonCollider;
        public void AddPolyCollider()
        {
            Bounds mapBounds = gridManager.LocalBounds;

            polygonCollider = gameObject.GetComponent<PolygonCollider2D>();

            if (polygonCollider == null)
            {
                polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
            }

            Vector2 min = mapBounds.min;
            Vector2 max = mapBounds.max;

            // Create the four corners in clockwise or counter-clockwise order
            Vector2[] points = new Vector2[]
            {
                new Vector2(min.x, min.y), // Bottom Left
                new Vector2(max.x, min.y), // Bottom Right
                new Vector2(max.x, max.y), // Top Right
                new Vector2(min.x, max.y)  // Top Left
            };

            // Assign the path to the collider
            polygonCollider.SetPath(0, points);
        }
        public Vector2Int GetGridPosition(Vector3 screenPos)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            return gridManager.WorldToGridPosition(mousePos);
        }

        public void SetHighlightedGridPositions(List<Vector2Int> positions)
        {
            
        }


        public WorldmapOverlay worldmapOverlay;
        public void HightlightPos(Vector2Int pos)
        {
            if (gridManager.WithinGridBounds(pos))
            {
                OverlayData data = new OverlayData();

                data.localPosition = gridManager.GridToWorldPostion(pos);
                data.HighlightedSides = new bool[6] { false, false, false, false, false, false };

                worldmapOverlay.AddOverlay(pos, data);
            }
        }

        public void HightlightPos(List<Vector2Int> pos)
        {
            foreach (Vector2Int p in pos)
            {
                if (gridManager.WithinGridBounds(p))
                {
                    gridManager.InsertVisualData(p, biomeConfig.HighlightVisualData, highlightLayer.LayerId);
                }
            }

            gridManager.DrawLayer(highlightLayer.LayerId);
        }

        public bool WithinWorldBounds(Vector2 worldPos)
        {
            return gridManager.ContainsWorldPosition(worldPos);
        }

        public void HighlightSide(Vector2Int pos, int index)
        {
            if (gridManager.WithinGridBounds(pos))
            {
                HexShape shape = (HexShape)gridManager.GetShape();

               //shape.HighlightSide(index);
                //gridManager.InsertVisualData(pos, shape, highlightLayer.LayerId);
                gridManager.DrawLayer(highlightLayer.LayerId);  
            }
        }

        public void ClearHighlightLayer()
        {
            gridManager.RemoveAllVisualData(highlightLayer.LayerId);
        }

        public List<Vector2Int> GetSurroudingPositions(Vector2Int start)
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            positions = HexFunctions.GetSurroundingTiles(start, 1);

            // remove all positions not within grid bounds

            for (int i = positions.Count - 1; i >= 0; i--)
            {
                if (!gridManager.WithinGridBounds(positions[i]))
                {
                    positions.RemoveAt(i);
                }
            }

            return positions;
        }

        public BiomeData GetBiomeData(Vector2Int pos)
        {
            float temp = noiseGenerator.GetTempNoise(pos.x, pos.y);
            float rain = noiseGenerator.GetRainNoise(pos.x, pos.y);
            float land = noiseGenerator.GetLandNoise(pos.x, pos.y);


            return biomeConfig.GetMatchingRule(land, temp, rain);
        }

        public ShapeVisualData GetVisualData(Vector2Int pos)
        {
            float land = noiseGenerator.GetLandNoise(pos.x, pos.y);
            float temp = noiseGenerator.GetTempNoise(pos.x, pos.y);
            float rain = noiseGenerator.GetRainNoise(pos.x, pos.y);

            ShapeVisualData vData = biomeConfig.GetLandVisualData(land, temp, rain);

            return vData;
        }
        public Material GetMaterial(Vector2Int pos)
        {
            float land = noiseGenerator.GetLandNoise(pos.x, pos.y);
            float temp = noiseGenerator.GetTempNoise(pos.x, pos.y);
            float rain = noiseGenerator.GetRainNoise(pos.x, pos.y);

            ShapeVisualData vData = biomeConfig.GetLandVisualData(land, temp, rain);

            LandVisualData lv = (LandVisualData)vData;

            return lv.NewMatWithProps();
        }

        public BiomeStats GetBiomeStats(Vector2Int pos)
        {
            BiomeData bd = GetBiomeData(pos);
            BiomeStats b = new BiomeStats();

            b.temp = noiseGenerator.GetTempNoise(pos.x, pos.y);
            b.rain = noiseGenerator.GetRainNoise(pos.x, pos.y);
            b.moveCost = bd.movementCost;
            b.biomeType = bd.biomeType;

            return b;
        }
        public void Clear()
        {
            gridManager.Clear();
            generating = false;

#if UNITY_EDITOR
                
            DestroyImmediate(polygonCollider);
#else
            Destroy(polygonCollider);
#endif
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(Worldmap))]
    public class WorldmapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Worldmap exampleScript = (Worldmap)target;

            if (GUILayout.Button("Generate Grid"))
            {
                exampleScript.Init();
                exampleScript.GenerateGrid();
            }

            if (GUILayout.Button("Clear Grid"))
            {
                exampleScript.Clear();
            }
        }
    }

#endif

}
