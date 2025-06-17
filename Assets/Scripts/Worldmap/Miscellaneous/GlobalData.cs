using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;

namespace CaseMaroon.Miscellaneous
{
    public static class GlobalData
    {
        public static int UI_ScreenMask = LayerMask.NameToLayer("UI_Screen");
        public static int UI_WorldMask = LayerMask.NameToLayer("UI_World");

        private static List<RaycastResult> SendRays()
        {
            if (EventSystem.current == null)
                return default;

            PointerEventData pointerData = 
                new PointerEventData(EventSystem.current)
                {
                    position = Mouse.current.position.ReadValue()
                };

            List<RaycastResult> raycastResults = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, raycastResults);

            return raycastResults;

        }
        public static bool IsMouseOverScreenUI
        {
            get
            {
                List<RaycastResult> raycastResults = SendRays();

                return raycastResults.Any(r => r.gameObject.layer == UI_ScreenMask);
            }
        }
        public static bool IsMouseOverWorldUI
        {
            get
            {
                List<RaycastResult> raycastResults = SendRays();
                return raycastResults.Any(r => r.gameObject.layer == UI_WorldMask);
            }
        }

        public static bool LeftBtnPressedThisFrame
        {
            get
            {
                return Mouse.current.leftButton.wasPressedThisFrame;
            }
        }

        public static Mesh CombineMeshes(List<Mesh> meshes)
        {
            List<CombineInstance> combineInstances = new List<CombineInstance>();

            foreach (var mesh in meshes)
            {
                CombineInstance ci = new CombineInstance
                {
                    mesh = mesh,
                    transform = Matrix4x4.identity
                };

                combineInstances.Add(ci);
            }

            Mesh combinedMesh = new Mesh();

            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, false);

            return combinedMesh;
        }

        public static Mesh CombineMeshes(List<Mesh> meshes, List<Vector3> worldPos)
        {
            List<CombineInstance> combineInstances = new List<CombineInstance>();

            if (meshes.Count != worldPos.Count)
            {
                Debug.LogError("Meshes and world positions count mismatch.");
                return null;
            }

            for (int i = 0; i < meshes.Count; i++)
            {
                CombineInstance ci = new CombineInstance
                {
                    mesh = meshes[i],
                    transform = Matrix4x4.TRS(worldPos[i], Quaternion.identity, Vector3.one)
                };
                combineInstances.Add(ci);
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

            return combinedMesh;
        }

        public enum BuildingType { Headquarters, SupplyDepot, Infantry, Tank}
    }
}
