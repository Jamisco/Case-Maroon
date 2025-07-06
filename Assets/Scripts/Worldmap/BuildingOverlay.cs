using UnityEngine;
using CaseMaroon.WorldMapUI;
using System.Collections.Generic;
using static CaseMaroon.Miscellaneous.GlobalData;
using static CaseMaroon.WorldMapUI.InputContext;
using CaseMaroon.Units;

namespace CaseMaroon.WorldMap
{
    public class BuildingOverlay : MonoBehaviour
    {
        public static BuildingOverlay Instance { get; private set; }

        public float spriteScale = .5f;

        private Worldmap worldmap;

        public LogisticsOverlay logistics;

        public Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();

 

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Prevent duplicates
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            worldmap = Worldmap.Instance;

            WorldUI.Instance.GridPositionSelected += OnGridPositionSelected;
            WorldUI.Instance.GridRightClicked += OnGridRightClicked;

        }

        private void Update()
        {

        }
        public void OnGridRightClicked(Vector2Int gridPos)
        {
            // we can remove the building here
        }
        private void OnGridPositionSelected(Vector2Int gridPos)
        {
        
        }
        public void PlaceBuilding(Building building)
        {
            Vector2Int gridPos = building.gridPosition;
            BuildingType buildType = building.buildingType;

            if (gridPos == Vector2Int.left)
            {
                return;
            }

            // send signal to server first 

            Vector2 worldPos = worldmap.gridManager.GridToWorldPostion(building.gridPosition);

            GameObject headquarters = new GameObject("Building");
            SpriteRenderer renderer = headquarters.AddComponent<SpriteRenderer>();

            headquarters.transform.SetParent(transform, false);
            renderer.transform.position = worldPos;
            renderer.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);

            renderer.sprite = GameAssets.Instance
                                .GetBuildingImage(buildType);

            buildings[gridPos] = building;

            WorldUI.Instance.InvokeBuildingPlace(gridPos, building);
        }
    }
}
