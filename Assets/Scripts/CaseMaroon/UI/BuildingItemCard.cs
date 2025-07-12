using CaseMaroon.WorldMap;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CaseMaroon.Miscellaneous.GlobalData;
using static CaseMaroon.WorldMapUI.InputContext;

namespace CaseMaroon.WorldMapUI
{
    public class BuildingItemCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Outline outlineObj;
        public Image imageObject;
        public TextMeshProUGUI textObject;
        public BuildingType buildType;

        public void SetBuilding(BuildingType type)
        {
            Sprite img = GameAssets.Instance.GetBuildingImage(type);
            string name = type.ToString();
            imageObject.sprite = img;
            textObject.text = name;
            buildType = type;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            InputContext context = new InputContext();

            context.BuildType = buildType;
            context.State = InputState.PlacingBuilding;

            WorldUI.Instance.InvokeInputState(context);
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
