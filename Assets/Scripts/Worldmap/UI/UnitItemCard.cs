using CaseMaroon.Units;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CaseMaroon.WorldMapUI.InputContext;

namespace CaseMaroon.WorldMapUI
{
    public class UnitItemCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Outline outlineObj;
        public Image imageObject;
        public TextMeshProUGUI textObject;
        public UnitType unitType;

        public void SetUnit(UnitType type)
        {
            Sprite img = WorldUI.Instance.unitCreator.GetUnitImage(type);

            string name = type.ToString();

            imageObject.sprite = img;
            textObject.text = name;
            unitType = type;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            InputContext context = new InputContext();

            context.UnitType = unitType;
            context.State = InputState.PlacingUnit;

            WorldUI.Instance.OnInputStateChanged?.Invoke(context);
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
