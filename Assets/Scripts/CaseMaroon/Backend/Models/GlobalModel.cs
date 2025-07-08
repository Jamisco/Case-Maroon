using CaseMaroon.Units;
using System;
using UnityEngine;

namespace CaseMaroon.Backend
{
    public static class GlobalModel
    {
        [Serializable]
        public struct Vector2IntWrap
        {
            public int x;
            public int y;
            public Vector2IntWrap(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }


        [Serializable]
        public struct UnitDataWrap
        {
            public int unitId;
            public string unitType;
            public Vector2IntWrap gridPosition;
            public int movePoints;

            public UnitDataWrap(Unit data)
            {
                unitId = data.UnitId;
                movePoints = data.MovementPoints;
                unitType = data.UnitType.ToString();
                gridPosition = new Vector2IntWrap(data.GridPosition.x, data.GridPosition.y);
            }
        }

    }
}
