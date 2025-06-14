using Assets.Scripts.Worldmap.Miscellaneous;
using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CaseMaroon.WorldMapUI
{
    public delegate void UnitSelected(UnitInfoUI_1 unit);
    public delegate void GridPositionSelected(Vector2Int gridPos);
    public class WorldUI : MonoBehaviour
    {
        public static WorldUI Instance { get; private set; }

        public UnitUIHandler unitHandler;

        [SerializeField]
        private float maxDragDistance = 5f;

        private Vector2 dragOrigin;
        private float draggedDistance;
        private bool isDragging = false;

        public bool MouseDragged => draggedDistance > maxDragDistance;

        public bool IsMouseOverUI()
        {
            if (EventSystem.current == null)
                return false;

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            foreach (var result in raycastResults)
            {
                GameObject hit = result.gameObject;

                // 1. Option A: Ignore specific layers
                if (hit.layer != LayerMask.NameToLayer("UI"))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

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

                Debug.Log("Mouse Down (Left)");
            }

            if (isDragging && Mouse.current.leftButton.isPressed)
            {
                draggedDistance = Vector2.Distance(dragOrigin, mousePos);
            }

            if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (!MouseDragged)
                {
                    Vector2Int gridPos = worldMap.GetGridPosition(dragOrigin);
                    OnGridPositionSelected(gridPos);
                }

                ResetDrag();
            }

            // RIGHT MOUSE HANDLING
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Debug.Log("Right Click");

                Vector2Int gridPos = worldMap.GetGridPosition(mousePos);
                worldMap.HightlightPos(gridPos);
                unitHandler.RemoveUnit(gridPos);
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
        public event GridPositionSelected GridPositionSelected;

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
        protected virtual void OnGridPositionSelected(Vector2Int gridPos)
        {
            // move the unit to the new position
            // this is used to get the center of the shape at the grid position, 
            // this is the position we will spawn the unit at

            //Debug.Log("Position Selected: " + gridPos.ToString());

            CheckUnit(gridPos);

            GridPositionSelected?.Invoke(gridPos);
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
                else
                {
                    SpawnTestUnit(gridPos);
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

                MoveablePositions = GetReachablePositions(SelectedUnit, SelectedUnit.gridPosition);

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

            unitHandler.MoveToPosition(unitInfo, gridPos);
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

        private List<Vector2Int> GetReachablePositions(UnitInfoUI_1 unit, Vector2Int curPos)
        {
            UnitData data = unit.data;
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

        public void SpawnTestUnit(Vector2Int gridPos)
        {
            ValidateUnitParentObj();

            UnitData newUnit = unitCreator.CreateUnit(UnitType.Infantry);


            unitHandler.SpawnUnit(gridPos, newUnit);
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
