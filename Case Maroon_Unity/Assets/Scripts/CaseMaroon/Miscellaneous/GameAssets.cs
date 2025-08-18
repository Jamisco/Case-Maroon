using CaseMaroon.Units;
using System;
using System.Linq;
using UnityEngine;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.WorldMap
{
    /// <summary>
    /// Holds the data used to create units. Data such as sprites, unit type etc
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "GameAssets", menuName = "CaseMaroon/GameAssets", order = 1)]
    public class GameAssets : ScriptableObject
    {
        public static GameAssets Instance { get; private set; }

        public UnitAssets[] unitSettings;
        public BuildingAssets[] buildingSettings;

        private void OnEnable()
        {
            Instance = this;
        }

        [Serializable]
        public struct UnitAssets
        {
            public UnitType unitType;
            public Sprite[] Images;
        }

        [Serializable]
        public struct BuildingAssets
        {
            public BuildingType buildType;
            public Sprite[] Images;
        }

        public Sprite GetUnitImage(UnitType ut)
        {
            return unitSettings.Where(x => x.unitType == ut).FirstOrDefault().Images[0];
        }

        public Sprite GetBuildingImage(BuildingType bt)
        {
            return buildingSettings.Where(x => x.buildType == bt).FirstOrDefault().Images[0];
        }
    }
}
