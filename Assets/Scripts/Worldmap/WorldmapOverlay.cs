using CaseMaroon.Miscellaneous;
using GridMapMaker;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace CaseMaroon.WorldMap
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class WorldmapOverlay : MonoBehaviour
    {
        private Worldmap worldmap;

        public HexShapeMaker shape;
        public Material material;

        [SerializeField]
        private float zOffset = -1;

        [Range(0.01f, 1f)]
        public float outlineScale = 0.1f;

        /// <summary>
        /// Use to highlight hexes
        /// </summary>
        private Mesh HexOverlay;

        [SerializeField]
        private Color borderColor;

        private ShapeMeshFuser enemyMeshFuser;

        /// <summary>
        /// Used to distinguish between friendly and Non Friendly hexes
        /// </summary>
        private Mesh enemyOverlay;


        /// <summary>
        /// A list of friendly positions which will be excluded from being marked
        /// </summary>
        private List<Vector2Int> friendlyPosition = new List<Vector2Int>();
        private void Awake()
        {
        }

        private void Start()
        {
            if (shape == null)
            {
                Debug.LogError("Shape is not assigned.");
                return;
            }

            Worldmap.Instance.OnWorldGenerated += OnWorldGenerated;
        }

        private void OnWorldGenerated(Worldmap map)
        {
            worldmap = map;
            CreateHighlightMesh();

            InitBorderFuser();
            DrawBorderOverlay();

            canDraw = true;
        }
        private void Update()
        {
            DrawHexOverlay();
        }
        private void CreateHighlightMesh()
        {
            HexOverlay = shape.GenerateHighlightMesh(outlineScale);
        }

        private bool canDraw = false;
        private void DrawHexOverlay()
        {
            if(!canDraw)
            {
                return;
            }

            Vector2Int gridPos = Vector2Int.zero; 
            Vector3 worldPos = Vector3.zero;

            if (worldmap.TryGetMouseMapPosition(out gridPos, 
                                        out worldPos))
            {
                worldPos.z = zOffset;

                Graphics.DrawMesh(HexOverlay,
                                        worldPos,
                                        Quaternion.identity,
                                        material,
                                        0);
            }
        }

        private void Test_Border()
        {
            InitBorderFuser();
            DrawBorderOverlay();
        }

        private void InitBorderFuser()
        {
#if UNITY_EDITOR

            worldmap = FindAnyObjectByType<Worldmap>();
#endif
            GridShape shape = worldmap.gridManager.GetShape(Vector2Int.zero);

            enemyMeshFuser = new ShapeMeshFuser(shape);

            Vector2Int gridSize = worldmap.gridManager.GridSize;

            for (int x = gridSize.x / 2; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    enemyMeshFuser.InsertPosition(pos, borderColor);
                }
            }
        }
        private void DrawBorderOverlay()
        {
            //if(Worldmap.Instance.WorldGenerated == false)
            //{
            //    return;
            //}

            Vector2Int gridSize = worldmap.gridManager.GridSize;

            List<Mesh> md = new List<Mesh>();

            enemyMeshFuser.GetFuseMesh().ForEach(x => md.Add(x.GetMesh()));
            // this took way too mcuh effort,
            // consider adding a mesh layer and just removing the hexes

            enemyOverlay = GlobalData.CombineMeshes(md);

            GetComponent<MeshFilter>().mesh = enemyOverlay;
            GetComponent<MeshRenderer>().material = material;
        }
        private void AddEnemyPosition(Vector2Int pos)
        {
            enemyMeshFuser.InsertPosition(pos);
            DrawBorderOverlay();
        }
        private void RemoveEnemyPosition(Vector2Int pos)
        {
            enemyMeshFuser.RemovePosition(pos);
            DrawBorderOverlay();
        }
    }
}
