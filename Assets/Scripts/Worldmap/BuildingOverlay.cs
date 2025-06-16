using CaseMaroon.WorldMapUI;
using UnityEngine;
using UnityEngine.InputSystem;
using static Assets.Scripts.Worldmap.Miscellaneous.GlobalData;

namespace CaseMaroon.WorldMap
{
    public class BuildingOverlay : MonoBehaviour
    {
        public float spriteScale = .5f;
        public Sprite HeadquartersSprite;
        public Sprite SupplyDepotSprite;

        private SpriteRenderer hoverRen;
        private Sprite selectedSprite;

        private Worldmap worldmap;

        public LogisticsOverlay logistics;

        private bool hoverBuilding = false;

        private void Start()
        {
            worldmap = Worldmap.Instance;

            WorldUI.Instance.OnInputStateChanged += OnInputStateChanged;
            WorldUI.Instance.OnGridPositionSelected += OnGridPositionSelected;
            WorldUI.Instance.OnBuildingPlaced += OnBuildingPlaced;

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

        private void OnInputStateChanged(InputState newState, BuildingType buildType)
        {
            if (newState == InputState.PlacingBuilding)
            {
                hoverBuilding = true;
            }

            if (buildType == BuildingType.Headquarters)
            {
                selectedSprite = HeadquartersSprite;
            }
            else if (buildType == BuildingType.SupplyDepot)
            {
                selectedSprite = SupplyDepotSprite;
            }

        }
        private void OnGridPositionSelected(Vector2Int gridPos)
        {
            if(hoverBuilding)
            {
                hoverBuilding = false;
                hoverRen.enabled = false;

                PlaceHeadquarters(gridPos);

                buildPos = gridPos;
                return;
            }

            if (gridPos == buildPos)
            {
                Vector2Int end = gridPos + new Vector2Int(5, 5);
                logistics.RunSupplyLink(buildPos, end);
                return;
            }
        }

        private Vector2Int buildPos = Vector2Int.left;



        private void CreateHoverRen()
        {
            if (hoverRen != null)
            {
                Destroy(hoverRen.gameObject);
            }

            hoverRen = new GameObject("HoverBuilding").AddComponent<SpriteRenderer>();
            hoverRen.transform.SetParent(transform);

            hoverRen.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);
            hoverRen.color = new Color(1f, 1f, 1f, 0.5f); // semi-transparent
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
        public void PlaceHeadquarters(Vector2Int gridPos)
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

            GameObject headquarters = new GameObject("Headquarters");
            SpriteRenderer renderer = headquarters.AddComponent<SpriteRenderer>();

            renderer.transform.position = worldPos;
            renderer.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);

            renderer.sprite = selectedSprite;
        }
    }
}
