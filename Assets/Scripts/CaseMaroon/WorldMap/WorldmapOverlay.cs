using CaseMaroon.GameSystem;
using CaseMaroon.Miscellaneous;
using GridMapMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.WorldMap
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(HexShapeMaker))]
    public class WorldmapOverlay : MonoBehaviour
    {
        public static WorldmapOverlay Instance { get; private set; }   

        private Worldmap worldMap;

        [Tooltip("The zoffset for the overlay that shows when a mouse is over a hex")]
        [SerializeField]
        private float mouseOverlayZOffset = -1;

        [Tooltip("The scale of the hex overlay that is use to show which hex the mouse is hovering")]
        [Range(0.01f, 1f)]
        public float outlineScale = 0.1f;

        [SerializeField]
        private Material overlayMaterial;

        [SerializeField]
        private Color enemyOverlayColor;

        [SerializeField]
        private Color neutralOverlayColor;

        [SerializeField]
        private Material fogMaterial;

        private ShapeMeshFuser enemyMeshFuser;
        private ShapeMeshFuser fogMeshFuser;


        /// <summary>
        /// Use to highlight/overlay hexes
        /// </summary>
        private Mesh HexOverlay;

        /// <summary>
        /// Used to distinguish between friendly and Non Friendly hexes
        /// </summary>
        private Mesh enemyOverlay;

        /// <summary>
        /// Mesh used to draw fog 
        /// </summary>
        private Mesh fogOverlay;

        private List<ReconPosition> reconPositions = new List<ReconPosition>();


        private void Awake()
        {
            Instance = this;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            Worldmap.Instance.OnWorldGenerated += OnWorldGenerated;
        }

        private void OnWorldGenerated(Worldmap map)
        {
            worldMap = map;

            InitOverlay();

            canDraw = true;
        }
        private void Update()
        {
            DrawHexMouseOverlay();
        }

        private bool canDraw = false;

        /// <summary>
        /// Draws a hex overlay at the current mouse position to denote which hex is selected
        /// </summary>
        private void DrawHexMouseOverlay()
        {
            if(!canDraw)
            {
                return;
            }

            Vector2Int gridPos = Vector2Int.zero; 
            Vector3 worldPos = Vector3.zero;

            if (worldMap.TryGetMouseMapPosition(out gridPos, 
                                        out worldPos))
            {
                worldPos.z = mouseOverlayZOffset;

                Graphics.DrawMesh(HexOverlay,
                                        worldPos,
                                        Quaternion.identity,
                                        overlayMaterial,
                                        0);
            }
        }
        private void Test_Border()
        {
            InitOverlay();
            
        }
        private void InitOverlay()
        {
#if UNITY_EDITOR

            worldMap = FindAnyObjectByType<Worldmap>();
#endif
            GridShape shape = worldMap.gridManager.GetShape(Vector2Int.zero);

            enemyMeshFuser = new ShapeMeshFuser(shape);
            fogMeshFuser = new ShapeMeshFuser(shape);

            reconPositions.Clear();

            foreach (Vector2Int landPos in Worldmap.Instance.landPositions)
            {
                reconPositions.Add(new ReconPosition(landPos));
            }

            HexOverlay = HexShapeMaker.Instance.GenerateHighlightMesh(outlineScale);
        }
        
        public void UpdateReconOverlay()
        {
            Player mp = GameState.Instance.MainPlayer;

            fogMeshFuser.Clear();
            enemyMeshFuser.Clear();

            foreach(ReconPosition pos in reconPositions)
            {
                int recon = pos.ReconLevel;

                if(mp.ReconPositions.TryGetValue(pos, out ReconPosition rp))
                {
                    recon = rp.ReconLevel;
                }

                if(recon == 0)
                {
                    fogMeshFuser.InsertPosition(pos.gridPosition);
                }
                else if (recon == 1)
                {
                    // here we can check if the hex belongs to enemy or is neutral and send in the appropriate color

                    if (!mp.OwnedPositions.Contains(pos.gridPosition))
                    {
                        enemyMeshFuser.InsertPosition(pos.gridPosition, enemyOverlayColor);
                    }
                }
            }

            DrawEnemyOverlay();
            DrawFogOverlay();

            List<Mesh> com = new List<Mesh> { enemyOverlay, fogOverlay };
            Material[] mats = new Material[] { overlayMaterial, fogMaterial };

            Mesh mesh = GlobalData.CombineMeshes_Sub(com);

            GetComponent<MeshFilter>().sharedMesh = mesh;   
            GetComponent<MeshRenderer>().materials = mats;
        }

        private void DrawEnemyOverlay()
        {
            Vector2Int gridSize = worldMap.gridManager.GridSize;

            List<Mesh> md = new List<Mesh>();

            enemyMeshFuser.GetFuseMesh().ForEach(x => md.Add(x.GetMesh()));
            // this took way too mcuh effort,
            // consider adding a mesh layer and just removing the hexes

            enemyOverlay = GlobalData.CombineMeshes(md);
        }
        private void DrawFogOverlay()
        {
            Vector2Int gridSize = worldMap.gridManager.GridSize;

            List<Mesh> md = new List<Mesh>();

            fogMeshFuser.GetFuseMesh().ForEach(x => md.Add(x.GetMesh()));
            // this took way too much effort,
            // consider adding a mesh layer and just removing the hexes

            fogOverlay = GlobalData.CombineMeshes(md);
        }

        

    }
}
