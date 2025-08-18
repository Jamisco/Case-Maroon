using CaseMaroon.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.Backend
{
    public static class BackendModels
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
        public struct UnitModel
        {
            public int unitId;
            public int playerId;
            public string unitType;
            public Vector2IntWrap gridPosition;
            public int movePoints;

            public UnitModel(Unit data)
            {
                unitId = data.UnitId;
                playerId = data.PlayerId;
                unitType = data.UnitType.ToString();
                gridPosition = new Vector2IntWrap(data.GridPosition.x, data.GridPosition.y);
                movePoints = data.MovementPoints;
            }

            public Unit ToUnit()
            {
                UnitType unitType = (UnitType)Enum.Parse(typeof(UnitType), this.unitType);

                Unit unit = Unit.CreateUnit(unitType);

                unit.UnitId = unitId;
                unit.PlayerId = playerId;
                unit.GridPosition = new Vector2Int(gridPosition.x, gridPosition.y);
                unit.MovementPoints = movePoints;
                return unit;
            }
        }

        [Serializable]
        public struct PlayerModel
        {
            public int id;
            public string username;
            public List<ReconPosition> reconPositions;
        }

        [System.Serializable]
        public struct GameManagerModel
        {
            public int gameId;

            public List<PlayerModel> players;
            public List<UnitModel> units;

            public List<OwnedPosition> ownedPositions;

            public bool gridGenerated;
            public int noiseHash;

            public static GameManagerModel FromJson(string json)
            {
                return JsonUtility.FromJson<GameManagerModel>(json);
            }
        }

    }
}
