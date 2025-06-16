using GridMapMaker;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    public class MasterRoadHex : MonoBehaviour
    {
        public HexagonalShape baseShape;
        private Mesh centerMesh;

        public float roadScale = 1f;
        public float mapScale = 1.5f;

        /// <summary>
        /// Dictionary to store generated road meshes based on side masks. For speed improvements
        /// </summary>
        private Dictionary<string, Mesh> roadMeshes = new Dictionary<string, Mesh>();

        public GameObject testObj;

        public string testMask = "000000";

        private void Start()
        {
            roadMeshes.Clear();
        }
        public Mesh GenerateHexWithRoad(string sideMask)
        {
            baseShape.scale = new Vector2(mapScale, mapScale);

            baseShape.UpdateShape();

            Mesh hexMesh = baseShape.ShapeMesh.GetMesh();

            List<Vector3> vector3s = hexMesh.vertices.ToList();

            vector3s = vector3s.
                Select(v => new Vector3(v.x * roadScale, 
                                        v.y * roadScale, 0)).ToList();

            hexMesh.vertices = vector3s.ToArray();

            centerMesh = hexMesh;

            Mesh mesh = AddRoads(sideMask);

            if (mesh != null)
            {
                roadMeshes.TryAdd(sideMask, mesh);
            }

            testObj.GetComponent<MeshFilter>().mesh = mesh;

            return mesh;
        }
        private Mesh AddRoads(string sideMask)
        {
            if (centerMesh == null || centerMesh.vertexCount < 6)
            {
                Debug.LogError("Center mesh is not initialized or invalid.");
                return null;
            }

            char[] bits = sideMask.ToCharArray();

            if (bits.Length != 6)
            {
                Debug.LogError("Side mask must be 6 bits long.");
                return null;
            }

            Vector3[] innerVerts = centerMesh.vertices;
            Vector3[] outerVerts = baseShape.ShapeMesh.GetMesh().vertices;

            List<CombineInstance> combines = new List<CombineInstance>();

            for (int side = 0; side < bits.Length; side++)
            {
                if (bits[side] != '1') continue;

                int i0 = side;
                int i1 = (side + 1) % 6;

                Vector3 v0 = innerVerts[i0];
                Vector3 v1 = innerVerts[i1];

                Vector3 innerMid = (v0 + v1) * 0.5f;
                Vector3 outerMid = (outerVerts[i0] + outerVerts[i1]) * 0.5f;

                Vector3 direction = (outerMid - innerMid).normalized;
                float length = (outerMid - innerMid).magnitude;

                Vector3 v2 = v1 + direction * length;
                Vector3 v3 = v0 + direction * length;

                Mesh roadMesh = new Mesh();
                roadMesh.vertices = new Vector3[] { v0, v1, v2, v3 };
                roadMesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
                roadMesh.uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                };

                roadMesh.RecalculateNormals();
                roadMesh.RecalculateBounds();

                CombineInstance ci = new CombineInstance
                {
                    mesh = roadMesh,
                    transform = Matrix4x4.identity
                };

                combines.Add(ci);
            }

            if(combines.Count > 0)
            {
                CombineInstance ci = new CombineInstance
                {
                    mesh = centerMesh,
                    transform = Matrix4x4.identity
                };

                combines.Add(ci);
            }
            else
            {
                return null;
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.name = "CombinedRoadMesh";
            combinedMesh.CombineMeshes(combines.ToArray(), true, false);
            return combinedMesh;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MasterRoadHex))]
    public class MasterRoadHexEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MasterRoadHex exampleScript = (MasterRoadHex)target;

            if (GUILayout.Button("Test Hex"))
            {
                exampleScript.GenerateHexWithRoad(exampleScript.testMask);
            }
        }
    }

#endif
}
