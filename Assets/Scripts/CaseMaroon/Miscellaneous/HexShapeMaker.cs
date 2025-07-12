using GridMapMaker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

namespace CaseMaroon.WorldMap
{
    public class HexShapeMaker : MonoBehaviour
    {
        public static HexShapeMaker Instance { get; private set; }

        private HexagonalShape baseShape;

        private void Awake()
        {
            Instance = this;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Prevent duplicates
                return;
            }
        }
        private void Start()
        {
            Worldmap.Instance.OnWorldInitialized += OnWorldInitialized;
        }

        private void OnWorldInitialized(Worldmap map)
        {
            baseShape = (HexagonalShape)map.gridManager.GetShape(Vector2Int.zero);

        }

        #region Test Methods

        [SerializeField]
        private GameObject test;

        private bool[] testHigh
            = new bool[6] { true, false, true, false, true, false };

        [SerializeField]
        private float testScale = 1f;

        private Color[] testColors
            = new Color[6] { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
        private void Test_Mesh()
        {
            Mesh tn = GenerateHighlightMesh(testScale, testHigh, testColors.ToList());

            test.GetComponent<MeshFilter>().mesh = tn;
        }

        #endregion
        public Mesh GenerateHighlightMesh(float scale, bool[] highlightedSides, List<Color> colors)
        {
            // smaller scale, smaller size vice versa
            scale = 1 - scale;
            baseShape.UpdateShape();

            Mesh outerMesh = baseShape.ShapeMesh.GetMesh();
            Mesh innerMesh = baseShape.ShapeMesh.GetMesh();

            List<Vector3> outerVerts = outerMesh.vertices.ToList();
            List<Vector3> innerVerts = innerMesh.vertices
                .Select(v => new Vector3(v.x * scale, v.y * scale, 0))
                .ToList();

            List<Vector3> finalVertices = new List<Vector3>();
            finalVertices.AddRange(outerVerts);
            finalVertices.AddRange(innerVerts);

            List<Color> vertexColors = new List<Color>();

            vertexColors.AddRange(outerMesh.colors);
            vertexColors.AddRange(innerMesh.colors);


            List<Vector2> uvs = new List<Vector2>();
            uvs.AddRange(outerMesh.uv);
            uvs.AddRange(innerMesh.uv);

            List<int> triangles = new List<int>();

            for (int i = 0; i < 6; i++)
            {
                int next = (i + 1) % 6;

                int outerCurrent = i;
                int outerNext = next;

                int innerCurrent = i + 6;
                int innerNext = next + 6;

                // Triangle 1
                triangles.Add(outerCurrent);
                triangles.Add(outerNext);
                triangles.Add(innerNext);

                // Triangle 2
                triangles.Add(outerCurrent);
                triangles.Add(innerNext);
                triangles.Add(innerCurrent);

                // Assign color to all 4 vertices of this side
                Color color = colors[i];
                vertexColors[outerCurrent] = color;
                vertexColors[outerNext] = color;
                vertexColors[innerCurrent] = color;
                vertexColors[innerNext] = color;

                // Assign dummy UVs (simple side-based mapping)
                // You can adjust these based on how you want to texture it
                uvs[outerCurrent] = new Vector2(0, 1);
                uvs[outerNext] = new Vector2(1, 1);
                uvs[innerCurrent] = new Vector2(0, 0);
                uvs[innerNext] = new Vector2(1, 0);
            }

            Mesh highlightMesh = new Mesh();
            highlightMesh.name = "HighlightOverlay";

            highlightMesh.SetVertices(finalVertices);
            highlightMesh.SetTriangles(triangles, 0);
            highlightMesh.SetColors(vertexColors);
            highlightMesh.SetUVs(0, uvs);

            highlightMesh.RecalculateNormals();
            highlightMesh.RecalculateBounds();

            return highlightMesh;
        }

        public Mesh GenerateHighlightMesh(float scale)
        {
            return GenerateHighlightMesh(scale, new bool[6], new List<Color> { Color.white, Color.white, Color.white, Color.white, Color.white, Color.white });
        }

        public Mesh GetHexMesh()
        {
            return baseShape.ShapeMesh.GetMesh();
        }

    }
}
