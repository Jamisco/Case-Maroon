using CaseMaroon.WorldMap;
using GridMapMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CaseMaroon.WorldMap
{
    public class WorldmapOverlay : MonoBehaviour
    {
        public Worldmap worldmap;

        public HexShape shape;
        public Material material;
        private Mesh OverlayMesh;

        [Range(0.01f, 1f)]
        public float outlineScale = 0.1f;
        public Vector2 HexScale = Vector2.one;

        public Sprite cirlceSprite;
        private Mesh circleMesh;
        private Mesh OverlayCircleMesh;
        [Range(.01f, 2f)]
        public float radiusScale = 0.1f;


        private bool OverlayIsDirty = true;

        public List<Vector2Int> MovementPositions = new List<Vector2Int>();
        public struct OverlayData
        {
            public Vector2 localPosition;
            public bool[] HighlightedSides;
        }
            
        private Dictionary<Vector2Int, OverlayData> overlayHexes = new Dictionary<Vector2Int, OverlayData>();

        private void Start()
        {
            if (shape == null)
            {
                Debug.LogError("Shape is not assigned.");
                return;
            }
            
            shape.outlineScale = outlineScale;

            OverlayMesh = new Mesh();
            OverlayCircleMesh = new Mesh();

            ValidateCircleMesh();
        }

        private void ValidateCircleMesh()
        {
            if (cirlceSprite == null)
            {
                Debug.LogError("Circle sprite is not assigned.");
                return;
            }

            circleMesh = new Mesh();

            circleMesh.vertices = Array.ConvertAll(cirlceSprite.vertices, v => new Vector3(v.x, v.y, 0f) * radiusScale);

            circleMesh.triangles = Array.ConvertAll(cirlceSprite.triangles, t => (int)t);
            circleMesh.uv = cirlceSprite.uv;

            circleMesh.colors = Enumerable.Repeat(Color.white, cirlceSprite.vertices.Length).ToArray();
        }

        private void Update()
        {
            if (OverlayIsDirty)
            {
                CreateOverlayMesh();
                CreateCircleMesh();
                OverlayIsDirty = false;


            }

            if (OverlayMesh.vertexCount > 0)
            {
                Graphics.DrawMesh(OverlayMesh,
                                  Matrix4x4.identity,
                                  material,
                                  gameObject.layer);
            }

            if(MovementPositions.Count > 0)
            {
                Graphics.DrawMesh(OverlayCircleMesh,
                                  Matrix4x4.identity,
                                  material,
                                  gameObject.layer);
            }
        }

        public void AddOverlay(Vector2Int overlay, OverlayData data)
        {
            if (overlayHexes.ContainsKey(overlay))
            {
                overlayHexes.Remove(overlay);
                MovementPositions.Clear();
            }
            else
            {
                overlayHexes.Add(overlay, data);

                MovementPositions.Clear();

                // add the movements 5 positions to right
                for (int i = 0; i < 5; i++)
                {
                    Vector2Int pos = overlay + new Vector2Int(i, 0);

                    if (worldmap.gridManager.WithinGridBounds(pos))
                    {
                        MovementPositions.Add(pos);
                    }
                }
            }

            OverlayIsDirty = true;
        }
        private void CreateOverlayMesh()
        {
            List<Mesh> meshes = new List<Mesh>();
            CombineInstance[] combine = new CombineInstance[overlayHexes.Count];

            for (int i = 0; i < overlayHexes.Count; i++)
            {
                Vector2Int key = overlayHexes.Keys.ElementAt(i);
                OverlayData data = overlayHexes[key];

                shape.SetHightlights(data.HighlightedSides);

                Mesh mesh = shape.GetMesh();

                combine[i].mesh = mesh;

                // Convert Vector2 to Matrix4x4 using Matrix4x4.TRS
                Vector3 position = new Vector3(data.localPosition.x, data.localPosition.y, 0);

                combine[i].transform = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);

                meshes.Add(mesh);
            }

            OverlayMesh.Clear();

            OverlayMesh.CombineMeshes(combine, true, true);
        }

        private void CreateCircleMesh()
        {
            if (MovementPositions.Count == 0)
            {
                return;
            }

            OverlayCircleMesh.Clear();

            List<CombineInstance> combine = new List<CombineInstance>();
            foreach (Vector2Int pos in MovementPositions)
            {
                Vector3 worldPos = worldmap.gridManager.GridToWorldPostion(pos);
                CombineInstance instance = new CombineInstance
                {
                    mesh = circleMesh,
                    transform = Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one)
                };
                combine.Add(instance);
            }

            OverlayCircleMesh.CombineMeshes(combine.ToArray(), true, true);
        }

        public bool ShowMesh = false;

        private void OnValidate()
        {
            ShowMeshes();
        }

        private void ShowMeshes()
        {
            if (ShowMesh)
            {
                if (shape != null)
                {
                    shape.outlineScale = outlineScale;
                    shape.scale = HexScale;
                }

                MeshFilter meshFilter = GetComponent<MeshFilter>();

                if (meshFilter == null)
                {
                    meshFilter = gameObject.AddComponent<MeshFilter>();
                }

                ValidateCircleMesh();
                Mesh hex = shape.GetMesh();
                Mesh circle = circleMesh;

                Mesh combined = new Mesh();

                // combined both mesh and make them side to side

                combined.CombineMeshes(new CombineInstance[]
                {
                    new CombineInstance { mesh = hex, transform = Matrix4x4.identity },
                    new CombineInstance { mesh = circle, transform = Matrix4x4.identity }
                }, true, true);

                meshFilter.mesh = combined;

                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

                if (meshRenderer == null)
                {
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                }
                meshRenderer.material = material;
            }
            else
            {
                // call removemeshrenderersmethod in 1 second delay

                // call removemeshrenderersmethod in 1 second delay
                Invoke(nameof(RemoveMeshRenderers), .2f);

            }
        }

        private void RemoveMeshRenderers()
        {
            // put this on a timed delay

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                DestroyImmediate(meshFilter);
            }

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                DestroyImmediate(meshRenderer);
            }
        }
    }
}
