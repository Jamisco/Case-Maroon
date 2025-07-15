using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using GridMapMaker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    public class UnitUIHelper
    {
        public Dictionary<Vector2Int, List<UnitInfoUI_1>> battleUnits = new();

        GameObject unitParent;

        WorldUI wu;
        GridManager grid;
        public UnitUIHelper(GameObject unitParent)
        {
            wu = WorldUI.Instance;
            grid = Worldmap.Instance.gridManager;

            this.unitParent = unitParent;
        }

        public void SpawnUnit(Unit data, Vector2Int gridPos)
        {
            Vector3 position = grid.GridToWorldPostion(gridPos);
            position.z += -.1f;

            UnitInfoUI_1 prefab = wu.UIManager.unitInfo_1;

            UnitInfoUI_1 unitUI 
                = Object.Instantiate(prefab, unitParent.transform);

            unitUI.gameObject.name = data.UnitName;
            unitUI.Initiliaze(data);

            RectTransform rect = unitUI.GetComponent<RectTransform>();

            rect.position = position;

            AddUnitToList(unitUI, gridPos);
            unitUI.GridPosition = gridPos;
            data.GridPosition = gridPos;

            WorldUI.Instance.InvokeUnitPlaced(gridPos, data.UnitType);
        }

        public void RemoveUnit(UnitInfoUI_1 unitInfo)
        {
            Vector2Int gridPos = unitInfo.GridPosition;
            RemoveUnitFromList(unitInfo, gridPos);
            Object.Destroy(unitInfo.gameObject);
            StackUnits(gridPos);
        }

        public void RemoveUnit(Vector2Int gridPos)
        {
            GetUnitInfos(gridPos, out List<UnitInfoUI_1> units);

            if (units == null || units.Count == 0)
            {
                return;
            }

            UnitInfoUI_1 unitInfo = units.Last();

            RemoveUnitFromList(unitInfo, gridPos);
            Object.Destroy(unitInfo.gameObject);
            StackUnits(gridPos);
        }


        public void MoveToPosition_Instant(UnitInfoUI_1 unitInfo, Vector2Int newPos)
        {
            Vector3 worldPos = grid.GridToWorldPostion(unitInfo.GridPosition);

            Vector2Int oldPos = unitInfo.GridPosition;

            unitInfo.MoveToPosition_Instant(worldPos);
                
            RemoveUnitFromList(unitInfo, unitInfo.GridPosition);
            AddUnitToList(unitInfo, newPos);

            unitInfo.GridPosition = newPos;

            StackUnits(oldPos);
            StackUnits(newPos);

            WorldUI.Instance.InvokeUnitPlaced(newPos, unitInfo.unit.UnitType);

        }

        public void MoveToPosition_Animate(Unit unit, List<Vector2Int> gridPositions)
        {
            // the problem with these move units is that UnitInfo UI holds a unit object, however said object is not thesame reference as the unit object that is passed into this method
            UnitInfoUI_1 unitInfo = null;

            GetUnitInfo(unit, out unitInfo);

            unitInfo.MoveToPosition_Animate(gridPositions);

            Vector2Int oldPos = unit.GridPosition;

            RemoveUnitFromList(unitInfo, oldPos);
            unitInfo.unit.GridPosition = gridPositions.Last();

            // i dont think we need this just in case.
            //unit.GridPosition = gridPositions.Last();
            AddUnitToList(unitInfo, gridPositions.Last());


            StackUnits(oldPos);
            StackUnits(unit.GridPosition);

            WorldUI.Instance.InvokeUnitPlaced(unit.GridPosition, unit.UnitType);
        }

        private void AddUnitToList(UnitInfoUI_1 unit, Vector2Int gridPos)
        {
            if (battleUnits.ContainsKey(gridPos))
            {
                battleUnits[gridPos].Add(unit);
            }
            else
            {
                battleUnits.Add(gridPos, new());
                battleUnits[gridPos].Add(unit);
            }
        }

        private void RemoveUnitFromList(UnitInfoUI_1 unit, Vector2Int gridPos)
        {
            if (battleUnits.ContainsKey(gridPos))
            {
                battleUnits[gridPos].Remove(unit);
            }
        }
        public bool GetUnitInfos(Vector2Int gridPos, out List<UnitInfoUI_1> units)
        {
            if (battleUnits.ContainsKey(gridPos))
            {
                if (battleUnits[gridPos].Count == 0)
                {
                    units = null;
                    return false;
                }

                units = battleUnits[gridPos];
                return true;
            }
            else
            {
                units = null;
                return false;
            }
        }

        public bool GetUnitInfo(Unit unit, out UnitInfoUI_1 unitInfo)
        {
            Vector2Int gridPos = unit.GridPosition;

            if (battleUnits.ContainsKey(unit.GridPosition))
            {
                if (battleUnits[gridPos].Count == 0)
                {
                    unitInfo = null;
                    return false;
                }

                unitInfo = battleUnits[gridPos]
                    .FirstOrDefault(x => unit.UnitId == x.unit.UnitId);

                if(unitInfo == null)
                {
                    return false;
                }

                return true;
            }
            else
            {
                unitInfo = null;
                return false;
            }
        }

        private void StackUnits(Vector2Int gridPos)
        {
            // we can furhte modify this by only stacking the most recently added unit
            if (battleUnits.ContainsKey(gridPos))
            {
                List<UnitInfoUI_1> units = battleUnits[gridPos];
                
                Vector3 worldPos = grid.GridToWorldPostion(gridPos);

                for (int i = 0; i < units.Count; i++)
                {
                    units[i].StackUnit(i, worldPos);
                }

               
            }
        }

        public void RefreshDrawOrder()
        {
            // since ui elements are rendered in the order they are added to the hierarchy, When stacking units, we need that units that are below in the stack are rendered first or come before units that are above it

            
        }

        public void Clear()
        {
            battleUnits.Clear();
        }
    }
}
