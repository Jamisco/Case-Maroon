using CaseMaroon.Miscellaneous;
using GridMapMaker;
using System.Collections.Generic;
using UnityEngine;

namespace CaseMaroon.WorldMap
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class FogOverlay : MonoBehaviour
    {
        private ShapeMeshFuser fogMeshFuser;

        private Mesh fogOverlay;
        private GridShape hexShape;

        [SerializeField]
        private Material fogMaterial;

        [SerializeField]
        private List<Vector2Int> visiblePosition = new();
        private Vector2Int gridSize;

        private void Start()
        {
            Worldmap.Instance.OnWorldGenerated += OnWorldGenerated;
        }

        private void Update()
        {
        }

        private void OnWorldGenerated(Worldmap map)
        {
            gridSize = map.gridManager.GridSize;
            hexShape = map.gridManager.GetShape(Vector2Int.zero);

            fogOverlay = new Mesh();

            InitMeshFuser();
            DrawFogMesh();
        }

        private void Test_Fog()
        {
            Worldmap map = FindAnyObjectByType<Worldmap>();
            map.GenerateGrid();

            OnWorldGenerated(map);
        }
        private void InitMeshFuser()
        {
            // The fog starts off with all positions covered
            // as you add visible positions, it removes said positions from the fuser
            fogMeshFuser = new ShapeMeshFuser(hexShape, Vector3.zero, Worldmap.Instance.landPositions);
        }

        private void DrawFogMesh()
        {
            List<Mesh> md = new List<Mesh>();

            fogMeshFuser.GetFuseMesh().ForEach(x => md.Add(x.GetMesh()));
            // this took way too mcuh effort,
            // consider adding a mesh layer and just removing the hexes

            fogOverlay = GlobalData.CombineMeshes(md);

            GetComponent<MeshFilter>().mesh = fogOverlay;
            GetComponent<MeshRenderer>().material = fogMaterial;
        }

        public void AddVisiblePosition(Vector2Int position)
        {
            fogMeshFuser.RemovePosition(position);
            DrawFogMesh();
        }

        public void AddVisiblePosition(List<Vector2Int> positions)
        {
            positions.ForEach(x => fogMeshFuser.RemovePosition(x));
            DrawFogMesh();
        }
    }
}
