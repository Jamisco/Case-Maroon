using GridMapMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

namespace CaseMaroon.WorldMap
{
    [Serializable]
    [CreateAssetMenu(fileName = "Hex Shape", menuName = "CaseMaroon/CreateShape/Hexagon")]
    public class HexShape : GridShape
    {
        public float Width { get => 1 * scale.x; }
        public float Depth { get => 1 * scale.y; }

        public List<Color> vertexColors = new List<Color>();

        public float outlineScale = 0.1f;

        private bool[] highlightedSides = new bool[6] { false, false, false, false, false, false };

        public Color DefaultColor = Color.white;
        public Mesh GetMesh()
        {
            SetBaseValues();

            Mesh mesh = new Mesh();

            List<Vector3> vec = BaseVertices.
                Select(v => new Vector3(v.x, v.y, 0)).ToList();

            mesh.vertices = vec.ToArray();
            mesh.triangles = BaseTriangles.ToArray();
            mesh.uv = BaseUVs.Select(v => new Vector2(v.x, v.y)).ToArray();
            mesh.colors = vertexColors.ToArray();

            return mesh;
        }

        public MeshData GetMeshData()
        {
            SetBaseValues();

            MeshData data = new MeshData();

            data.Vertices = BaseVertices.Select(v => new Vector3(v.x, v.y, 0)).ToList();
            data.Triangles = BaseTriangles.ToList();
            data.Uvs = BaseUVs.Select(v => new Vector2(v.x, v.y)).ToList();
            data.Colors = vertexColors.ToList();

            return data;
        }

        private void OnValidate()
        {

        }

        float xTesselationConstant;
        float yTesselationConstant;
        protected override void SetBaseValues()
        {
            SetBaseVertices();
            SetColors();
            SetBaseTriangles();
            SetBaseUVs();

            //BaseOrientation = Orientation.XZ;

            xTesselationConstant = (Width / 2.0f);
            yTesselationConstant = (Depth - Depth / 4.0f);
        }
        private void SetBaseVertices()
        {
            BaseVertices = new List<Vector2>
            {
                new Vector2(0f, Depth / 2),          // Top (0)
                new Vector2(Width / 2, 0.25f * Depth),    // Top right (1)
                new Vector2(Width / 2, -0.25f * Depth),   // Bottom right (2)
                new Vector2(0f, -(Depth / 2)),       // Bottom (3)
                new Vector2(-(Width / 2), -(0.25f * Depth)), // Bottom left (4)
                new Vector2(-(Width / 2), (0.25f * Depth))   // Top left (5)
            };

            // Create the inner vertices by scaling the outer vertices
            List<Vector2> outerVertices = new List<Vector2>(BaseVertices);
            foreach (Vector2 vertex in outerVertices)
            {
                BaseVertices.Add(vertex * (1 - outlineScale));
            }
        }

        private void SetColors()
        {
            // Initialize colors - one color per vertex
            vertexColors.Clear();

            for (int i = 0; i < BaseVertices.Count; i++)
            {
                vertexColors.Add(Color.white); // Default color
            }

            HighlightSides();
        }

        public void HighlightSide(int side, bool highlight)
        {
            highlightedSides[side - 1] = highlight;
        }

        public void SetHightlights(bool[] sides)
        {
            if(sides.Length != 6)
            {
                Debug.LogError("Array Must be exactly 6.");
                return;
            }

            highlightedSides = sides;
        }

        private void HighlightSides()
        {
            // First reset all vertices to default color
            for (int i = 0; i < vertexColors.Count; i++)
            {
                vertexColors[i] = DefaultColor;
            }

            // Then apply highlighting for selected sides
            for (int i = 0; i < highlightedSides.Length; i++)
            {
                if (highlightedSides[i])
                {
                    Highlight(i, Color.red);
                }
            }

        }

        void Highlight(int sideIndex, Color color)
        {
            int outerCurrent = sideIndex;
            int outerNext = (sideIndex + 1) % 6;
            int innerCurrent = sideIndex + 6;
            int innerNext = ((sideIndex + 1) % 6) + 6;

            // Apply color to the four vertices of this side
            vertexColors[outerCurrent] = color;
            vertexColors[outerNext] = color;
            vertexColors[innerCurrent] = color;
            vertexColors[innerNext] = color;
        }

        private void SetBaseTriangles()
        {
            BaseTriangles = new List<int>();

            // Create triangles connecting inner and outer hexagon edges
            for (int i = 0; i < 6; i++)
            {
                int outerCurrent = i;
                int outerNext = (i + 1) % 6; // Wrap around to 0 when i = 5

                int innerCurrent = i + 6;    // Inner vertices start at index 6 now
                int innerNext = ((i + 1) % 6) + 6;

                // First triangle of the quad
                BaseTriangles.Add(outerCurrent);
                BaseTriangles.Add(outerNext);
                BaseTriangles.Add(innerCurrent);

                // Second triangle of the quad
                BaseTriangles.Add(innerCurrent);
                BaseTriangles.Add(outerNext);
                BaseTriangles.Add(innerNext);
            }
        }

        private void SetBaseUVs()
        {
            BaseUVs = new List<Vector2>();

            // UV mapping for outer vertices (0-5)
            for (int i = 0; i < 6; i++)
            {
                float angle = (i / 6.0f) * Mathf.PI * 2;
                BaseUVs.Add(new Vector2(0.5f + 0.5f * Mathf.Cos(angle), 0.5f + 0.5f * Mathf.Sin(angle)));
            }

            // UV mapping for inner vertices (6-11)
            for (int i = 0; i < 6; i++)
            {
                float angle = (i / 6.0f) * Mathf.PI * 2;
                // Use a smaller radius for inner vertices to maintain UV mapping proportion
                BaseUVs.Add(new Vector2(0.5f + 0.3f * Mathf.Cos(angle), 0.5f + 0.3f * Mathf.Sin(angle)));
            }
        }


        protected override Vector2 GetBaseTesselatedPosition(int x, int y)
        {
            Vector2 position = new Vector2();
            // Calculate the center of each hexagon
            position.x = x * Width + ((y % 2) * (xTesselationConstant));
            position.y = y * yTesselationConstant;

            return position;
        }

        protected override Vector2Int GetBaseGridCoordinate(Vector2 localPosition)
        {
            // this function works as follows
            // we get a ball park estimate of the grid coordinate
            // then we iterate over the surrounding grid coordinates within a certain range
            // denoted by count variable
            // we calculate the distance of each grid coordinate from the local position
            // and return the grid coordinate with the smallest distance

            //float x = localPosition.x / (Width + cellGap.x);

            //// revese the function from getbasetesselation
            //float z = localPosition.y / (Depth + cellGap.y);

            float y = localPosition.y / (yTesselationConstant + cellGap.y);
            int y1 = Mathf.RoundToInt(y);

            // given a  position, choose random x postion, compare only y values
            float x = (localPosition.x - (y1 % 2) * (xTesselationConstant)) / (Width + cellGap.x);

            int x1 = Mathf.RoundToInt(x);

            // because of the way hexes are shaped, multiple grids can share thesame x or y world position. So when we narrow the world position to a specific grid position, we then check the surrounding positions to get the precise grid positions

            return GetClosestGrid(x1, y1);

            Vector2Int GetClosestGrid(int maxX, int maxZ)
            {
                int count = 1;

                int xMin = Mathf.Max(0, maxX - count);
                int zMin = Mathf.Max(0, maxZ - count);

                maxX += count;
                maxZ += count;

                for (int x = maxX; x >= xMin; x--)
                {
                    for (int z = maxZ; z >= zMin; z--)
                    {
                        if (IsLocalPositionInShape(localPosition, x, z))
                        {
                            return new Vector2Int(x, z);
                        }
                    }
                }

                // this should never run
                return Vector2Int.left;
            }
        }
    }
}
