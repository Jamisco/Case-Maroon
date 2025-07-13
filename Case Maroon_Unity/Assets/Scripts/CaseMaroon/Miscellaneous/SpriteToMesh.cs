using UnityEngine;
using UnityEditor;
using CaseMaroon.WorldMap;
using CaseMaroon.Miscellaneous;


namespace CaseMaroon.Miscellaneous
{
    public class SpriteToMesh : MonoBehaviour
    {
        public float meshScale = 1f;
        public Sprite sprite;
        public string meshAssetName = "NewSpriteMesh";

#if UNITY_EDITOR

        [Tooltip("Convert and Save Mesh (Editor Only)")]
        public void ConvertAndSaveMesh()
        {
            Mesh mesh = SpriteToMesh.Generate(sprite, meshScale);
            mesh.name = meshAssetName;

            if (mesh == null) return;

            string path = $"Assets/{meshAssetName}.asset";
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Mesh saved to {path}");
        }
#endif

        /// <summary>
        /// Static method to generate a mesh from a sprite. Usable at runtime or in editor.
        /// </summary>
        public static Mesh Generate(Sprite sprite, float scale = 1f)
        {
            if (sprite == null)
            {
                Debug.LogError("Sprite is null. Cannot generate mesh.");
                return null;
            }

            Vector2[] spriteVertices = sprite.vertices;
            ushort[] spriteTriangles = sprite.triangles;
            Vector2[] spriteUVs = sprite.uv;

            if (spriteVertices == null || spriteVertices.Length == 0)
            {
                Debug.LogError("Sprite has no vertex data.");
                return null;
            }

            Mesh mesh = new Mesh();

            Vector3[] vertices3D = new Vector3[spriteVertices.Length];
            for (int i = 0; i < spriteVertices.Length; i++)
            {
                vertices3D[i] = (Vector3)(spriteVertices[i] * scale);
            }

            int[] triangles = new int[spriteTriangles.Length];
            for (int i = 0; i < spriteTriangles.Length; i++)
            {
                triangles[i] = spriteTriangles[i];
            }

            mesh.vertices = vertices3D;
            mesh.triangles = triangles;
            mesh.uv = spriteUVs;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SpriteToMesh))]
        public class SpriteToMeshEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                SpriteToMesh exampleScript = (SpriteToMesh)target;

                if (GUILayout.Button("Convert To Mesh"))
                {
                    exampleScript.ConvertAndSaveMesh();
                }
            }
        }

    #endif

}




