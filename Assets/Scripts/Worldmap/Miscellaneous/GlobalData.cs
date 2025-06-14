using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Worldmap.Miscellaneous
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
    }
}
