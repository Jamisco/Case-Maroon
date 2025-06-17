using CaseMaroon.Miscellaneous;
using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.WorldMapUI
{
    public delegate void UnitSelected(UnitInfoUI_1 unit);
    public delegate void GridPositionSelected(Vector2Int gridPos);
    public delegate void GridRightClicked(Vector2Int gridPos);
    public delegate void InputStateChanged(InputState newState, BuildingType buildType);
    public delegate void BuildingPlaced(Vector2Int gridPos);
    public enum InputState
    {
        None,
        PlacingBuilding,
        SelectingUnit,
        MovingUnit,
        CreatingUnit
    }
    public class WorldUI : MonoBehaviour
    {
        public static WorldUI Instance { get; private set; }

        public UnitUIHandler unitHandler;
        private InputState worldInputState;

        public InputStateChanged OnInputStateChanged;
        public GridRightClicked OnGridRightClicked;

        public InputState WorldInputState
        {
            get => worldInputState;
        }

        [SerializeField]
        private float maxDragDistance = 5f;

        private Vector2 dragOrigin;
        private float draggedDistance;
        private bool isDragging = false;

        public bool MouseDragged => draggedDistance > maxDragDistance;

        private void CheckMouse()
        {
            if(GlobalData.IsMouseOverScreenUI)
            {
                return;
            }

            Vector2 mousePos = Mouse.current.position.ReadValue();

            // LEFT MOUSE HANDLING
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                dragOrigin = mousePos;
                isDragging = true;
            }

            if (isDragging && Mouse.current.leftButton.isPressed)
            {
                draggedDistance = Vector2.Distance(dragOrigin, mousePos);
            }

            Vector2Int clickedPos = worldMap.GetGridPosition(mousePos);

            if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (!MouseDragged)
                {
                    if(worldMap.gridManager
                        .ContainsGridPosition(clickedPos) )
                    {
                        GridPositionSelected(clickedPos);
                    }
                }

                ResetDrag();
            }

            // RIGHT MOUSE HANDLING
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                OnGridRightClicked?.Invoke(clickedPos);
                unitHandler.RemoveUnit(clickedPos);
            }
        }
        private void ResetDrag()
        {
            draggedDistance = 0f;
            isDragging = false;
            dragOrigin = Vector2.left;
        }

        //private void ResetDrag()
        //{
        //    dragOrigin = Vector2.left;
        //    DraggedDistance = 0;
        //}

        public Worldmap worldMap;
        public UnitCreator unitCreator;
        public UnitInfoUI_1 prefab;

        public UIManager uiManager;

        public Canvas UnitCanvas;

        private PolygonCollider2D gridCollider;
        public GameObject AllUnitsParent;

        public event UnitSelected UnitSelected;
        public event GridPositionSelected OnGridPositionSelected;
        public event BuildingPlaced OnBuildingPlaced;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Prevent duplicates
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional, if persistent

            worldMap = FindAnyObjectByType<Worldmap>();
            unitCreator = FindAnyObjectByType<UnitCreator>();

            worldMap.OnWorldGenerated += WorldMap_OnWorldGenerated;
        }
        private void WorldMap_OnWorldGenerated(Worldmap map)
        {
            //if(gridCollider == null)
            //{
            //    gridCollider = this.AddComponent<PolygonCollider2D>();
            //}

            //gridCollider.points = worldMap.polygonCollider.points;
        }

        private void Start()
        {
            ValidateUnitParentObj();

            unitHandler = new UnitUIHandler(AllUnitsParent);
        }

        private void Update()
        {
            CheckMouse();
        }
        
        private void ValidateUnitParentObj()
        {
            // remember that the scale of this object must be .01 for the unit ui to fit
            if (AllUnitsParent == null)
            {
                AllUnitsParent = new GameObject("All Units ");
                AllUnitsParent.transform.SetParent(UnitCanvas.transform);
                float sc = .01f;

                AllUnitsParent.AddComponent<RectTransform>();
                AllUnitsParent.transform.localScale = new Vector3(sc, sc, sc);
            }
        }
        protected virtual void GridPositionSelected(Vector2Int gridPos)
        {
            // move the unit to the new position
            // this is used to get the center of the shape at the grid position, 
            // this is the position we will spawn the unit at

            //Debug.Log("Position Selected: " + gridPos.ToString());

            CheckUnit(gridPos);

            OnGridPositionSelected?.Invoke(gridPos);
        }

        private UnitInfoUI_1 SelectedUnit { get; set; }
        private List<Vector2Int> MoveablePositions = new List<Vector2Int>();
        private void CheckUnit(Vector2Int gridPos)
        {
            // if a unit was already selected, move it to the new position
            if (SelectedUnit != null)
            {
                if(gridPos == SelectedUnit.gridPosition)
                {
                    // if the user clicks on the selected unit again, deselect it
                    DeselectCurrentUnit();
                }
                else
                {
                    MoveSelectedUnit(SelectedUnit, gridPos);
                }
            }
            else
            {
                // if there is a unit on that position, select it
                if (unitHandler.GetUnit(gridPos, 
                                    out List<UnitInfoUI_1> unit))
                {
                    OnUnitSelected(unit.Last());
                }
            }
        }

        public Vector2Int prevBuildPos = Vector2Int.left;

        protected virtual void OnUnitSelected(UnitInfoUI_1 unit)
        {
            if (!unit.Equals(SelectedUnit))
            {
                DeselectCurrentUnit();

                SelectedUnit = unit;
                SelectedUnit.EnableOutline();

                MoveablePositions = GetUnitReachablePositions(SelectedUnit.data, SelectedUnit.gridPosition);

                MoveablePositions = MoveablePositions.OrderBy(x => x.x).ToList();

                worldMap.ClearHighlightLayer();

                worldMap.HightlightPos(MoveablePositions);
                UnitSelected?.Invoke(SelectedUnit);
            }
            else
            {
                DeselectCurrentUnit();
            }
        }

        public bool GetUnits(Vector2Int gridPos, out List<UnitInfoUI_1> unit)
        {
            return unitHandler.GetUnit(gridPos, out unit);
        }

        public void MoveSelectedUnit(UnitInfoUI_1 unitInfo, Vector2Int gridPos)
        {
            // cannot move unit to its current position
            if (unitInfo == null || gridPos == unitInfo.gridPosition)
            {
                return;
            }

            // check if the position is valid, if not, deselect the unit
            if (MoveablePositions.Contains(gridPos) == false)
            {
                DeselectCurrentUnit();
                return;
            }



            List<Vector2Int> path = GetFastestPath(unitInfo.data, unitInfo.gridPosition, gridPos);

            unitHandler.MoveToPosition_Animate(unitInfo, path);
            DeselectCurrentUnit();

            Canvas.ForceUpdateCanvases();
        }

        private void DeselectCurrentUnit()
        {
            if (SelectedUnit != null)
            {
                SelectedUnit.DisableOutline();
                SelectedUnit = null;

                MoveablePositions.Clear();
                worldMap.ClearHighlightLayer();
            }
        }
        public void GetMoveablePositions(UnitInfoUI_1 unit, out List<Vector2Int> moveablePositions)
        {
            moveablePositions = HexFunctions.GetSurroundingTiles(unit.gridPosition, 2);
        }

        private List<Vector2Int> GetUnitReachablePositions(UnitData data, Vector2Int curPos)
        {
            int maxMovement = data.MovementPoints;

            Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();
            Queue<(Vector2Int pos, int cost)> queue = new Queue<(Vector2Int, int)>();

            queue.Enqueue((curPos, 0));
            visited[curPos] = 0;

            while (queue.Count > 0)
            {
                var (current, costSoFar) = queue.Dequeue();

                foreach (Vector2Int neighbor in worldMap.GetSurroudingPositions(current))
                {
                    BiomeData biome = worldMap.GetBiomeData(neighbor);

                    MovementType mt = data.MovementType;

                    int moveCost = biome.GetMovementCost(mt);

                    if (moveCost < 0 || moveCost == int.MaxValue)
                        continue; // Impassable tile

                    int newCost = costSoFar + moveCost;

                    if (newCost <= maxMovement &&
                        (!visited.ContainsKey(neighbor) || newCost < visited[neighbor]))
                    {
                        visited[neighbor] = newCost;
                        queue.Enqueue((neighbor, newCost));
                    }
                }
            }

            return visited.Keys.ToList();
        }

        public List<Vector2Int> GetFastestPath(UnitData data, Vector2Int start, Vector2Int dest)
        {
            MovementType mt = data.MovementType;

            Dictionary<Vector2Int, int> costSoFar = new Dictionary<Vector2Int, int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            List<Vector2Int> openSet = new List<Vector2Int> { start };
            costSoFar[start] = 0;

            while (openSet.Count > 0)
            {
                // Find node in openSet with the lowest cost
                Vector2Int current = openSet[0];
                int bestCost = costSoFar[current];

                for (int i = 1; i < openSet.Count; i++)
                {
                    Vector2Int pos = openSet[i];
                    int cost = costSoFar[pos];
                    if (cost < bestCost)
                    {
                        current = pos;
                        bestCost = cost;
                    }
                }

                openSet.Remove(current);

                if (current == dest)
                    break;

                foreach (Vector2Int neighbor in worldMap.GetSurroudingPositions(current))
                {
                    BiomeData biome = worldMap.GetBiomeData(neighbor);
                    int moveCost = biome.GetMovementCost(mt);

                    if (moveCost < 0 || moveCost == int.MaxValue)
                        continue; // Impassable

                    int newCost = costSoFar[current] + moveCost;

                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                    {
                        costSoFar[neighbor] = newCost;
                        cameFrom[neighbor] = current;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            // Reconstruct path
            if (!cameFrom.ContainsKey(dest))
                return new List<Vector2Int>(); // No path found

            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int step = dest;
            while (step != start)
            {
                path.Add(step);
                step = cameFrom[step];
            }

            path.Add(start);
            path.Reverse();
            return path;
        }
            
        public List<Vector2Int> GetLogisticsPath(Vector2Int start, Vector2Int dest)
        {
            UnitData supply = unitCreator.CreateUnit(UnitType.Armored);

            return GetFastestPath(supply, start, dest);
        }

        public void SpanwUnit(Vector2Int gridPos, UnitData data)
        {
            unitHandler.SpawnUnit(gridPos, data);
        }

        public void SpawnTestUnit(Vector2Int gridPos)
        {
            ValidateUnitParentObj();

            UnitData newUnit = unitCreator.CreateUnit(UnitType.Infantry);

            unitHandler.SpawnUnit(gridPos, newUnit);
        }

        public Dictionary<Vector2Int, List<UnitInfoUI_1>> GetAllUnits()
        {
            return unitHandler.battleUnits;
        }

        public void Clear()
        {
#if UNITY_EDITOR

            DestroyImmediate(AllUnitsParent);
#else
            DestroyImmediate(AllUnitsParent);
#endif
            unitHandler.Clear();
        }



#if UNITY_EDITOR
        [CustomEditor(typeof(WorldUI))]
        public class WorldUIEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                WorldUI exampleScript = (WorldUI)target;

                if (GUILayout.Button("Clear units"))
                {
                    exampleScript.Clear();
                }

            }
        }
#endif
    }

}
