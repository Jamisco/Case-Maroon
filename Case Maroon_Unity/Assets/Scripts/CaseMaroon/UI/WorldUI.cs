using Assets.Scripts.Units;
using CaseMaroon.Backend;
using CaseMaroon.Miscellaneous;
using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static CaseMaroon.Miscellaneous.GlobalData;
using static CaseMaroon.WorldMapUI.InputContext;

namespace CaseMaroon.WorldMapUI
{
    public delegate void UnitSelectedHandler(UnitInfoUI_1 unit);
    public delegate void GridPositionSelectedHandler(Vector2Int gridPos);
    public delegate void GridRightClickedHandler(Vector2Int gridPos);
    public delegate void InputStateChangedHandler(InputContext inputContext);
    public delegate void BuildingPlacedHandler(Vector2Int gridPos, Building building);
    public delegate void UnitPlacedHandler(Vector2Int gridPos, UnitType unitType);

    public struct InputContext
    {
        public enum InputState
        {
            Idle,
            PlacingBuilding,
            PlacingUnit,
            SelectingUnit,
            MovingUnit,
            CreatingUnit
        }

        public InputState State;
        public BuildingType? BuildType;
        public UnitType? UnitType;
    }

    public class WorldUI : MonoBehaviour
    {
        public static WorldUI Instance { get; private set; }

        [SerializeField]
        private GameAssets GameAssets;

        public Worldmap worldMap;

        public UnitInfoUI_1 prefab;

        public UIManager UIManager;

        public Canvas UnitCanvas;

        private PolygonCollider2D gridCollider;
        public GameObject AllUnitsParent;

        public UnitUIHelper unitUIHelper;

        public event InputStateChangedHandler InputStateChanged;
        public event GridRightClickedHandler GridRightClicked;
        public event UnitSelectedHandler UnitSelected;
        public event GridPositionSelectedHandler GridPositionSelected;
        public event BuildingPlacedHandler BuildingPlaced;
        public event UnitPlacedHandler UnitPlaced;

        private InputContext worldInputContext;
        public InputContext WorldInputContext
        {
            get => worldInputContext;
            private set
            {
                worldInputContext = value;
            }
        }

        [SerializeField]
        private float maxDragDistance = 5f;

        private Vector2 dragOrigin;
        private float draggedDistance;
        private bool isDragging = false;

        public bool MouseDragged => draggedDistance > maxDragDistance;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Prevent duplicates
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional, if persistent

            InputStateChanged += OnInputStateChanged;

            InputStateChanged?.Invoke(new InputContext
            {
                State = InputContext.InputState.Idle,
                BuildType = null,
                UnitType = null
            });
        }

        private void Start()
        {
            worldMap = Worldmap.Instance;
            Worldmap.Instance.OnWorldGenerated += OnWorldGenerated;

            ValidateUnitParentObj();
            unitUIHelper = new UnitUIHelper(AllUnitsParent);
        }

        private void Update()
        {
            if (Worldmap.Instance.WorldGenerated)
            {
                CheckMouse();
            }
        }
        private void OnInputStateChanged(InputContext inputContext)
        {
            WorldInputContext = inputContext;
        }
        private void CheckMouse()
        {
            if (GlobalData.IsMouseOverScreenUI)
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

            Vector2Int clickedPos = Vector2Int.left;

            try
            {
                clickedPos = worldMap.GetGridPosition(mousePos);
            }
            catch (System.Exception)
            {

            }

            if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (!MouseDragged)
                {
                    if (worldMap.gridManager
                        .ContainsGridPosition(clickedPos))
                    {
                        OnGridPositionSelected(clickedPos);
                    }
                }

                ResetDrag();
            }

            // RIGHT MOUSE HANDLING
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                GridRightClicked?.Invoke(clickedPos);
                unitUIHelper.RemoveUnit(clickedPos);
            }
        }
        private void ResetDrag()
        {
            draggedDistance = 0f;
            isDragging = false;
            dragOrigin = Vector2.left;
        }
        public void InvokeInputState(InputContext context)
        {
            InputStateChanged?.Invoke(context);
        }
        public void InvokeBuildingPlaced(Vector2Int gridPos, Building building)
        {
            BuildingPlaced?.Invoke(gridPos, building);
            BackendTester.Instance.SyncGameState();
        }

        public void InvokeUnitPlaced(Vector2Int gridPos, UnitType type)
        {
            UnitPlaced?.Invoke(gridPos, type);
            BackendTester.Instance.SyncGameState();
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

        protected virtual void OnGridPositionSelected(Vector2Int gridPos)
        {
            // move the unit to the new position
            // this is used to get the center of the shape at the grid position, 
            // this is the position we will spawn the unit at

            //Debug.Log("Position: " + gridPos.ToString());

            CheckUnit(gridPos);

            GridPositionSelected?.Invoke(gridPos);

            //BackendMessenger.Instance.SendGridPos(gridPos);
        }
        private void OnWorldGenerated(Worldmap map)
        {
            //if(gridCollider == null)
            //{
            //    gridCollider = this.AddComponent<PolygonCollider2D>();
            //}

            //gridCollider.points = worldMap.polygonCollider.points;

        }

        private UnitInfoUI_1 SelectedUnit { get; set; }
        private List<Vector2Int> MoveablePositions = new List<Vector2Int>();
        private void CheckUnit(Vector2Int gridPos)
        {
            // we should only click on units when we arent doing anything else
            if (worldInputContext.State != InputState.Idle)
            {
                return;
            }

            // if a unit was already selected, move it to the new position
            if (SelectedUnit != null)
            {
                if(gridPos == SelectedUnit.GridPosition)
                {
                    // if the user clicks on the selected unit again, deselect it
                    DeselectCurrentUnit();
                }
                else
                {
                    List<Vector2Int> path = GetFastestPath(SelectedUnit.unit, SelectedUnit.unit.GridPosition, gridPos);

                    BackendTester.Instance.MoveUnit(SelectedUnit.unit, path);
                }
            }
            else
            {
                // if there is a unit on that position, select it
                if (unitUIHelper.GetUnitInfos(gridPos, 
                                    out List<UnitInfoUI_1> unit))
                {
                    OnUnitSelected(unit.Last());
                }
            }
        }

        protected virtual void OnUnitSelected(UnitInfoUI_1 unit)
        {
            if (!unit.Equals(SelectedUnit))
            {
                DeselectCurrentUnit();

                SelectedUnit = unit;
                SelectedUnit.EnableOutline();

                MoveablePositions = GetUnitReachablePositions(SelectedUnit.unit, SelectedUnit.GridPosition);

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
            return unitUIHelper.GetUnitInfos(gridPos, out unit);
        }

        public void MoveSelectedUnit(Unit unit, List<Vector2Int> path)
        {
            // cannot move unit to its current position
            if (unit == null || path.Last() == unit.GridPosition)
            {
                return;
            }

            // check if the position is valid, if not, deselect the unit
            if (MoveablePositions.Contains(path.Last()) == false)
            {
                DeselectCurrentUnit();
                return;
            }

            unitUIHelper.MoveToPosition_Animate(unit, path);
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
            moveablePositions = HexFunctions.GetSurroundingTiles(unit.GridPosition, 2);
        }

        // This method uses a priority queue
        private List<Vector2Int> GetUnitReachablePositions2(Unit data, Vector2Int curPos)
        {
            int maxMovement = data.MovementPoints;

            // Stores the best cost to reach each position
            Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();

            // Priority queue: SortedSet with custom comparer for (cost, position)
            // SortedSet automatically sorts by cost ascending
            var comparer = Comparer<(int cost, Vector2Int pos)>.Create((a, b) =>
            {
                int comp = a.cost.CompareTo(b.cost);
                if (comp == 0)
                    comp = a.pos.GetHashCode().CompareTo(b.pos.GetHashCode());
                return comp;
            });

            SortedSet<(int cost, Vector2Int pos)> priorityQueue = new SortedSet<(int, Vector2Int)>(comparer);

            priorityQueue.Add((0, curPos));
            visited[curPos] = 0;

            while (priorityQueue.Count > 0)
            {
                // Extract position with lowest cost
                var current = priorityQueue.Min;
                priorityQueue.Remove(current);

                Vector2Int currentPos = current.pos;
                int costSoFar = current.cost;

                foreach (Vector2Int neighbor in worldMap.GetSurroudingPositions(currentPos))
                {
                    BiomeData biome = worldMap.GetBiomeData(neighbor);
                    MovementType mt = data.MovementType;

                    int moveCost = biome.GetMovementCost(mt);

                    if (moveCost < 0 || moveCost == int.MaxValue)
                        continue; // impassable

                    int newCost = costSoFar + moveCost;

                    if (newCost <= maxMovement && (!visited.ContainsKey(neighbor) || newCost < visited[neighbor]))
                    {
                        // If neighbor was visited before with higher cost, remove old entry from priority queue
                        if (visited.ContainsKey(neighbor))
                        {
                            priorityQueue.Remove((visited[neighbor], neighbor));
                        }

                        visited[neighbor] = newCost;
                        priorityQueue.Add((newCost, neighbor));
                    }
                }
            }

            return visited.Keys.ToList();
        }

        private List<Vector2Int> GetUnitReachablePositions(Unit data, Vector2Int curPos)
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

        public List<Vector2Int> GetFastestPath(Unit data, Vector2Int start, Vector2Int dest)
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
            Sprite img = GameAssets.Instance.GetUnitImage(UnitType.Armored);

            Unit supply = DefaultUnitData.CreateDefaultUnit<Tank>(img);

            return GetFastestPath(supply, start, dest);
        }

        public void SpawnUnit(Vector2Int gridPos, Unit data)
        {
            unitUIHelper.SpawnUnit(data, gridPos);
        }
        public void SpawnTestUnit(Vector2Int gridPos)
        {
            ValidateUnitParentObj();

            Unit newUnit = GameAssets.CreateUnit(UnitType.Infantry);

            unitUIHelper.SpawnUnit(newUnit, gridPos);
        }
        public Dictionary<Vector2Int, List<UnitInfoUI_1>> GetAllUnits()
        {
            return unitUIHelper.battleUnits;
        }
        public void Clear()
        {
#if UNITY_EDITOR

            DestroyImmediate(AllUnitsParent);
#else
            DestroyImmediate(AllUnitsParent);
#endif
            unitUIHelper.Clear();
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
