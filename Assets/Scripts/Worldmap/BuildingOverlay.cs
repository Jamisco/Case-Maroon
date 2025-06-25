using CaseMaroon.Units;
using CaseMaroon.WorldMapUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.WorldMap
{
    public class BuildingOverlay : MonoBehaviour
    {
        public float spriteScale = .5f;
        public Sprite HeadquartersSprite;
        public Sprite SupplyDepotSprite;

        public Sprite InfantrySprite;
        public Sprite TankSprite;

        private SpriteRenderer hoverRen;
        private Sprite selectedSprite;
        private BuildingType selectedType;

        private Worldmap worldmap;

        public LogisticsOverlay logistics;

        private bool hoverBuilding = false;

        public Dictionary<Vector2Int, BuildingType> buildPositions = new Dictionary<Vector2Int, BuildingType>();

        private void Start()
        {
            worldmap = Worldmap.Instance;

            WorldUI.Instance.OnInputStateChanged += OnInputStateChanged;
            WorldUI.Instance.OnGridPositionSelected += OnGridPositionSelected;
            WorldUI.Instance.OnBuildingPlaced += OnBuildingPlaced;
            WorldUI.Instance.OnGridRightClicked += OnGridRightClicked;


            CreateHoverRen();
        }
        private void OnBuildingPlaced(Vector2Int gridPos)
        {
            throw new System.NotImplementedException();
        }

        private void Update()
        {
            HoverBuildingOnMouse();
        }

        public void OnGridRightClicked(Vector2Int gridPos)
        {
            // cancal building mod
            if (hoverBuilding)
            {
                hoverBuilding = false;
                selectedSprite = null;
                hoverRen.enabled = false;
                return;
            }

            //if (gridPos == buildPos)
            //{
            //    // here we can remove the building if we like
            //}
        }

        private void OnInputStateChanged(InputState newState, BuildingType buildType)
        {
            if (newState == InputState.PlacingBuilding)
            {
                hoverBuilding = true;
            }

            switch (buildType)
            {
                case BuildingType.Headquarters:
                    selectedSprite = HeadquartersSprite;
                    break;
                case BuildingType.SupplyDepot:
                    selectedSprite = SupplyDepotSprite;
                    break;
                case BuildingType.Infantry:
                    selectedSprite = InfantrySprite;
                    break;
                case BuildingType.Tank:
                    selectedSprite = TankSprite;
                    break;
                default:
                    break;
            }

            selectedType = buildType;
        }
        private void OnGridPositionSelected(Vector2Int gridPos)
        {
            if(hoverBuilding)
            {
                hoverBuilding = false;
                hoverRen.enabled = false;

                PlaceBuilding(gridPos);

                buildPositions[gridPos] = selectedType;
                return;
            }

            if(buildPositions.ContainsKey(gridPos))
            {
                BuildingType buildType = buildPositions[gridPos];
                Vector2Int buildPos = gridPos;

                // run logistics only for building
                if (buildType == BuildingType.Headquarters || buildType == BuildingType.SupplyDepot)
                {
                    //Vector2Int end = gridPos + new Vector2Int(5, 5);
                    //logistics.RunSupplyLink(buildPos, end);
                    logistics.SupplyMapUnits(buildPos);
                }

                return;
            }
        }

        private void CreateHoverRen()
        {
            if (hoverRen != null)
            {
                Destroy(hoverRen.gameObject);
            }

            hoverRen = new GameObject("HoverBuilding").AddComponent<SpriteRenderer>();
            hoverRen.transform.SetParent(transform);

            hoverRen.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);
            hoverRen.color = new Color(1f, 1f, 1f, 0.8f); // semi-transparent
            hoverRen.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);

            hoverRen.enabled = false;
        }
        public void HoverBuildingOnMouse()
        {
            if (hoverBuilding == false)
            {
                return;
            }

            Vector3 wp = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2Int gridPos =
                worldmap.gridManager.WorldToGridPosition(wp);

            if (gridPos == Vector2Int.left)
            {
                return;
            }

            Vector2 worldPos = worldmap.gridManager.GridToWorldPostion(gridPos);

            hoverRen.enabled = true;
            hoverRen.sprite = selectedSprite;

            hoverRen.transform.position = worldPos;
        }
        public void PlaceBuilding(Vector2Int gridPos)
        {
            if (HeadquartersSprite == null)
            {
                return;
            }

            if (gridPos == Vector2Int.left)
            {
                return;
            }

            Vector2 worldPos = worldmap.gridManager.GridToWorldPostion(gridPos);

            switch (selectedType)
            {
                case BuildingType.Headquarters:
                case BuildingType.SupplyDepot:
                    GameObject headquarters = new GameObject("Building");
                    SpriteRenderer renderer = headquarters.AddComponent<SpriteRenderer>();

                    renderer.transform.position = worldPos;
                    renderer.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);

                    renderer.sprite = selectedSprite;

                    break;
                case BuildingType.Infantry:

                    UnitData inf = WorldUI.Instance.unitCreator
                                        .CreateUnit(UnitType.Infantry);

                    BackendTester.Instance
                        .SpawnUnit(gridPos, inf);

                    //WorldUI.Instance.SpanwUnit(gridPos, inf);

                    break;
                case BuildingType.Tank:

                    UnitData tank = WorldUI.Instance.unitCreator
                                        .CreateUnit(UnitType.Armored);

                    WorldUI.Instance.SpawnUnit(gridPos, tank);

                    break;
                default:
                    break;
            }


        }
    }
}
