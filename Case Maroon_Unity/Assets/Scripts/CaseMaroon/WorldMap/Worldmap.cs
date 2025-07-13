using CaseMaroon.Miscellaneous;
using GridMapMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using CaseMaroon.Backend;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CaseMaroon.WorldMap
{
    public delegate void WorldInitialized(Worldmap map);

    public delegate void WorldGenerated(Worldmap map);
    public class Worldmap : MonoBehaviour
    {
        public static Worldmap Instance { get; private set; }

        /// <summary>
        /// This is invoked when all the map data has been inserted but not the Map itself(the meshes) have not been created and drawn 
        /// </summary>
        public event WorldGenerated OnWorldInitialized;

        /// <summary>
        /// This is invoke immediately after all the meshes have been drawn
        /// </summary>
        public event WorldGenerated OnWorldGenerated;

        // create grid generated event
        public GridManager gridManager;
        public BiomeGenerator biomeGenerator; 
        public NoiseGenerator noiseGenerator;

        [SerializeField]
        public Vector2 ShapeScale;

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
        public bool WorldGenerated { get; private set; } = false;

        [HideInInspector]
        public List<Vector2Int> landPositions = new List<Vector2Int>();

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

                BackendTester.Instance.UploadMapConfig(this);

                //GenerateGrid();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Can't Generate World map, " + ex.Message);
            }
        }

        public void OnValidate()
        {
            if(gridManager == null)
            { 
                Init();
            }

            ValidateLayerScale();
        }
        public void Update()
        {
            if (noiseGenerator.NoiseModified && instantUpdate)
            {
                GenerateGrid();
            }
        }

        private void ValidateLayerScale()
        {
            baseLayer.ShapeSize = ShapeScale;
            snowLayer.ShapeSize = ShapeScale;
            highlightLayer.ShapeSize = ShapeScale;
        }
        public void Init()
        {
            gridManager = GetComponent<GridManager>();
            noiseGenerator = GetComponent<NoiseGenerator>();

            ValidateLayerScale();
        }
        public void ComputeNoise()
        {
            noiseGenerator.ComputeNoises(gridManager.GridSize, false);
        }

        public void GenerateGrid()
        {
            if (generating)
            {
                return;
            }

            generating = true;

            ComputeNoise();

            DrawGrid();
        }   

        private void DrawGrid()
        {
            gridManager.Initialize();
            gridManager.CreateLayer(baseLayer, true);
            gridManager.CreateLayer(snowLayer, false);
            gridManager.CreateLayer(highlightLayer, false);

            Vector2Int pos;
            ShapeVisualData vData;
            ShapeVisualData snowData;

            landPositions.Clear();

            for (int x = 0; x < gridManager.GridSize.x; x++)
            {
                for (int y = 0; y < gridManager.GridSize.y; y++)
                {
                    pos = new Vector2Int(x, y);

                    float land = noiseGenerator.GetLandNoise(x, y);
                    float temp = noiseGenerator.GetTempNoise(x, y);
                    float rain = noiseGenerator.GetRainNoise(x, y);

                    vData = biomeGenerator.GetLandVisualData(land, temp, rain);
                    snowData = biomeGenerator.GetSnowVisualData(temp);

                    if(biomeGenerator.IsLand(land))
                    {
                        landPositions.Add(pos);
                    }

                    gridManager.InsertVisualData(pos, vData);
                    gridManager.InsertVisualData(pos, snowData, snowLayer.LayerId);
                }
            }

            OnWorldInitialized?.Invoke(this);

            gridManager.DrawGrid();

            AddPolyCollider();

            generating = false;

            WorldGenerated = true;
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

        public List<Vector2Int> GetRemainderPositions(List<Vector2Int> positions)
        {
            List<Vector2Int> r = new List<Vector2Int>();

            Vector2Int pos;

            for (int x = 0; x < gridManager.GridSize.x; x++)
            {
                for (int y = 0; y < gridManager.GridSize.y; y++)
                {
                    pos = new Vector2Int(x, y);

                    if(!positions.Contains(pos))
                    {
                        r.Add(pos);
                    }
                }
            }

            return r;
        }

        public bool TryGetMouseMapPosition(out Vector2Int gridPos, out Vector3 worldPos)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if(gridManager.ContainsWorldPosition(mousePos))
            {
                gridPos = gridManager.WorldToGridPosition(mousePos);

                if(gridManager.ContainsGridPosition(gridPos))
                {
                    worldPos = gridManager.GridToWorldPostion(gridPos);
                    return true;
                }
            }

            gridPos = Vector2Int.left;
            worldPos = Vector3.negativeInfinity;

            return false;
        }

        public Vector2Int GetGridPosition(Vector3 screenPos)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            return gridManager.WorldToGridPosition(mousePos);
        }

        public void SetHighlightedGridPositions(List<Vector2Int> positions)
        {
            
        }

        public void HightlightPos(Vector2Int pos)
        {
            //if (gridManager.WithinGridBounds(pos))
            //{
            //    OverlayData data = new OverlayData();

            //    data.localPosition = gridManager.GridToWorldPostion(pos);
            //    data.HighlightedSides = new bool[6] { false, false, false, false, false, false };

            //    worldmapOverlay.AddOverlay(pos, data);
            //}
        }

        public void HightlightPos(List<Vector2Int> pos)
        {
            foreach (Vector2Int p in pos)
            {
                if (gridManager.ContainsGridPosition(p))
                {
                    gridManager.InsertVisualData(p, biomeGenerator.HighlightVisualData, highlightLayer.LayerId);
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
            if (gridManager.ContainsGridPosition(pos))
            {
                //HexShape shape = (HexShape)gridManager.GetShape();

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
                if (!gridManager.ContainsGridPosition(positions[i]))
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

            return biomeGenerator.GetMatchingBiome(land, temp, rain);
        }

        public ShapeVisualData GetVisualData(Vector2Int pos)
        {
            float land = noiseGenerator.GetLandNoise(pos.x, pos.y);
            float temp = noiseGenerator.GetTempNoise(pos.x, pos.y);
            float rain = noiseGenerator.GetRainNoise(pos.x, pos.y);

            ShapeVisualData vData = biomeGenerator.GetLandVisualData(land, temp, rain);

            return vData;
        }
        public Material GetMaterial(Vector2Int pos)
        {
            float land = noiseGenerator.GetLandNoise(pos.x, pos.y);
            float temp = noiseGenerator.GetTempNoise(pos.x, pos.y);
            float rain = noiseGenerator.GetRainNoise(pos.x, pos.y);

            ShapeVisualData vData = biomeGenerator.GetLandVisualData(land, temp, rain);

            LandVisualData lv = (LandVisualData)vData;

            return lv.NewMatWithProps();
        }

        public BiomeData GetBiomeStats(Vector2Int pos)
        {
            return GetBiomeData(pos);
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

            if (GUILayout.Button("Restart "))
            {
                exampleScript.Start();
            }

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
