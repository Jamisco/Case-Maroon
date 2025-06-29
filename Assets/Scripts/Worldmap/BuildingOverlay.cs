using CaseMaroon.Units;
using CaseMaroon.WorldMapUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static CaseMaroon.Miscellaneous.GlobalData;
using static CaseMaroon.WorldMapUI.InputContext;

namespace CaseMaroon.WorldMap
{
    public class BuildingOverlay : MonoBehaviour
    {
        public static BuildingOverlay Instance { get; private set; }

        public float spriteScale = .5f;

        private Worldmap worldmap;

        public LogisticsOverlay logistics;

        private bool hoverBuilding = false;

        public Dictionary<Vector2Int, BuildingType> buildPositions = new Dictionary<Vector2Int, BuildingType>();


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
            WorldUI.Instance.BuildingPlaced += OnBuildingPlaced;
            WorldUI.Instance.GridRightClicked += OnGridRightClicked;

        }
        private void OnBuildingPlaced(Vector2Int gridPos)
        {
            throw new System.NotImplementedException();
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
            InputContext cur = WorldUI.Instance.WorldInputContext;

            if (cur.State == InputState.PlacingBuilding)
            {
                PlaceBuilding((BuildingType)cur.BuildType, gridPos);

                InputContext context = new InputContext
                {
                    State = InputState.None,
                };

                WorldUI.Instance.InvokeInputState(context);
            }
        }
        public void PlaceBuilding(BuildingType buildType, Vector2Int gridPos)
        {
            if (gridPos == Vector2Int.left)
            {
                return;
            }

            Vector2 worldPos = worldmap.gridManager.GridToWorldPostion(gridPos);

            GameObject headquarters = new GameObject("Building");
            SpriteRenderer renderer = headquarters.AddComponent<SpriteRenderer>();

            renderer.transform.position = worldPos;
            renderer.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);

            renderer.sprite = GameAssets.Instance
                                .GetBuildingImage(buildType);
        }
    }
}
