using Assets.Scripts.Units;
using CaseMaroon.Units;
using CaseMaroon.WorldMapUI;
using UnityEngine;
using UnityEngine.InputSystem;
using static CaseMaroon.Miscellaneous.GlobalData;
using static CaseMaroon.WorldMapUI.InputContext;

namespace CaseMaroon.WorldMap
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PreviewOverlay : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        private Sprite selectedSprite;
        public bool IsPreviewing { get; private set; } = false;

        [SerializeField]
        private float zDepth = -0.06f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            WorldUI.Instance.InputStateChanged += OnInputStateChanged;
            WorldUI.Instance.GridPositionSelected += OnGridPositionSelected;
            WorldUI.Instance.GridRightClicked += GridRightClicked;
        }

        private void GridRightClicked(Vector2Int gridPos)
        {
            if(IsPreviewing)
            {
                CancelPreview();
            }
        }

        private void Update()
        {
            PreviewOverMouse();
        }

        private void OnInputStateChanged(InputContext context)
        {
            if (context.State == InputState.PlacingBuilding)
            {
                selectedSprite = GameAssets.Instance.GetBuildingImage((BuildingType)context.BuildType);

                IsPreviewing = true;
            }
            else if(context.State == InputState.PlacingUnit)
            {
                selectedSprite = GameAssets.Instance.GetUnitImage((UnitType)context.UnitType);

                IsPreviewing = true;
            }
            else
            {
                return;
            }

            spriteRenderer.sprite = selectedSprite;
        }
        public void PreviewOverMouse()
        {
            if (IsPreviewing == false)
            {
                return;
            }

            if(Worldmap.Instance.TryGetMouseMapPosition(out Vector2Int gridPos, out Vector3 worldPos))
            {
                worldPos.z = zDepth;
                spriteRenderer.transform.position = worldPos;
            }
        }
        private void CancelPreview()
        {
            IsPreviewing = false;
            spriteRenderer.sprite = null;

            InputContext context = new InputContext
            {
                State = InputState.Idle,
            };

            WorldUI.Instance.InvokeInputState(context);
        }
        private void OnGridPositionSelected(Vector2Int gridPos)
        {
            if(!IsPreviewing)
            {
                return;
            }

            InputContext cur = WorldUI.Instance.WorldInputContext;

            if (cur.State == InputState.PlacingBuilding)
            {
                //BuildingOverlay.Instance.PlaceBuilding(
                //    (BuildingType)cur.BuildType, gridPos);

                Building newBuilding = new Building(gridPos, 
                                (BuildingType) cur.BuildType);

                BackendTester.Instance.SpawnBuilding(newBuilding);
            }
            else if (cur.State == InputState.PlacingUnit)
            {
                Sprite img = GameAssets.Instance.GetUnitImage   
                    ((UnitType)cur.UnitType);

                Unit ud = DefaultUnitData.CreateDefaultUnit((UnitType)cur.UnitType, img);

                BackendTester.Instance.SpawnUnit(gridPos, ud);
            }

            IsPreviewing = false;

            InputContext context = new InputContext
            {
                State = InputState.Idle,
            };

            WorldUI.Instance.InvokeInputState(context);

            CancelPreview();
        }
    }
}
