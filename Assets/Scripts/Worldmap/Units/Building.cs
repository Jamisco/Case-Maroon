using System;
using UnityEngine;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.Units
{
    [Serializable]
    public class Building
    {
        public Vector2Int gridPosition;
        public BuildingType buildingType;

        public Building(Vector2Int gridPosition, BuildingType buildingType)
        {
            this.gridPosition = gridPosition;
            this.buildingType = buildingType;
        }
    }
}
