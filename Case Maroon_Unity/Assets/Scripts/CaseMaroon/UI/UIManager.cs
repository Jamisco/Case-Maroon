using CaseMaroon.GameSystem;
using CaseMaroon.Units;
using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    [CreateAssetMenu(menuName = "UI/UI Manager")]
    public class UIManager : ScriptableObject
    {
        public UnitInfoUI_1 unitInfo_1;
        public UnitInfoUI_2 unitInfo_2;
        public StatItemCard starItemCard;

        public BuildingItemCard buildingItemCard;
        public UnitItemCard unitItemCard;
        public MessageBox messageBox;

    }
}
