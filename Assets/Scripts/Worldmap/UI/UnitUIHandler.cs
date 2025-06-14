using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using GridMapMaker;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    public class UnitUIHandler
    {
        public Dictionary<Vector2Int, List<UnitInfoUI_1>> battleUnits = new();

        GameObject unitParent;

        WorldUI wu;
        GridManager grid;
        public UnitUIHandler(GameObject unitParent)
        {
            wu = WorldUI.Instance;
            grid = Worldmap.Instance.gridManager;

            this.unitParent = unitParent;
        }

        public void SpawnUnit(Vector2Int gridPos, UnitData data)
        {
            Vector3 position = grid.GridToWorldPostion(gridPos);
            position.z += -.1f;

            UnitInfoUI_1 prefab = wu.uiManager.unitInfo_1;

            UnitInfoUI_1 unitUI 
                = Object.Instantiate(prefab, unitParent.transform);

            unitUI.gameObject.name = data.UnitName;
            unitUI.Initiliaze(data);

            RectTransform rect = unitUI.GetComponent<RectTransform>();

            rect.position = position;

            AddUnitToList(gridPos, unitUI);
            unitUI.gridPosition = gridPos;
        }

        public void MoveToPosition(UnitInfoUI_1 unitInfo, Vector2Int newPos)
        {
            Vector3 worldPos = grid.GridToWorldPostion(unitInfo.gridPosition);

            Vector2Int oldPos = unitInfo.gridPosition;

            unitInfo.MoveToPosition(worldPos);
                
            RemoveUnitFromList(unitInfo.gridPosition, unitInfo);
            AddUnitToList(newPos, unitInfo);

            unitInfo.gridPosition = newPos;

            StackUnits(oldPos);
            StackUnits(newPos);
        }

        private void AddUnitToList(Vector2Int gridPos,  UnitInfoUI_1 unit)
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

        private void RemoveUnitFromList(Vector2Int gridPos, UnitInfoUI_1 unit)
        {
            if (battleUnits.ContainsKey(gridPos))
            {
                battleUnits[gridPos].Remove(unit);
            }
        }
        public bool GetUnit(Vector2Int gridPos, out List<UnitInfoUI_1> units)
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
