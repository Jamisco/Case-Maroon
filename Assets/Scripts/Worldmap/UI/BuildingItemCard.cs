using CaseMaroon.WorldMapUI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.WorldMapUI
{
    public class BuildingItemCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Outline outlineObj;
        public Image imageObject;
        public TextMeshProUGUI textObject;
        public BuildingType buildType;

        public void OnPointerClick(PointerEventData eventData)
        {
            WorldUI.Instance.OnInputStateChanged?.Invoke(InputState.PlacingBuilding, buildType);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            outlineObj.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            outlineObj.enabled = false;
        }
    }
}
