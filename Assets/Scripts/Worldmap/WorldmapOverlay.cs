using CaseMaroon.Miscellaneous;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseMaroon.WorldMap
{
    public class WorldmapOverlay : MonoBehaviour
    {
        public Worldmap worldmap;

        public HexShapeMaker shape;
        public Material material;

        [Range(0.01f, 1f)]
        public float outlineScale = 0.1f;

        [Range(.01f, 2f)]
        public float circleScale = 0.1f;
        public Sprite circleSprite;
        private Mesh circleMesh;

        private Mesh HexOverlay;
        public struct OverlayData
        {
            public Vector2 localPosition;
            public bool[] HighlightedSides;
        }

        private void Start()
        {
            if (shape == null)
            {
                Debug.LogError("Shape is not assigned.");
                return;
            }

            circleMesh = new Mesh();

            CreateCircleMesh();
            CreateHighlightMesh();
        }
        private void Update()
        {
            DrawHexOverlay();
        }

        private void OnValidate()
        {

        }
        private void CreateCircleMesh()
        {
            if (circleSprite == null)
            {
                Debug.LogError("Circle sprite is not assigned.");
                return;
            }

            circleMesh = SpriteToMesh.Generate(circleSprite, circleScale);
        }
        private void CreateHighlightMesh()
        {
            HexOverlay = shape.Generate(outlineScale);
        }
        private void DrawHexOverlay()
        {
            Vector2Int gridPos = Vector2Int.zero; 
            Vector3 worldPos = Vector3.zero;

            if (worldmap.TryGetMouseMapPosition(out gridPos, 
                                        out worldPos))
            {
                Graphics.DrawMesh(HexOverlay,
                                        worldPos,
                                        Quaternion.identity,
                                        material,
                                        0);
            }
        }

    }
}
