using CaseMaroon.Backend;
using CaseMaroon.GameSystem;
using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using CaseMaroon.WorldMapUI;
using GridMapMaker;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CaseMaroon.Miscellaneous.GlobalData;
using static CaseMaroon.WorldMap.BiomeData;

namespace CaseMaroon.WorldMapUI
{
    public class BotOverlay : MonoBehaviour
    {
        public static BotOverlay Instance { get; private set; }
        public string BuildingTitle
        {
            get
            {
                return buildingTitleObj.text;
            }
            set
            {
                buildingTitleObj.text = value;
            }
        }
        public string BuildingDescription
        {
            get
            {
                return buildingDcptObj.text;
            }
            set
            {
                buildingDcptObj.text = value;
            }
        }

        public bool EnableBuildingQuestionMark
        {
            get
            {
                return buildingQuestionMark.activeSelf;
            }
            set
            {
                buildingQuestionMark.SetActive(value);
            }
        }

        public Image BuildingImage;

        [SerializeField]
        private TextMeshProUGUI buildingTitleObj;

        [SerializeField]
        private TextMeshProUGUI buildingDcptObj;

        [SerializeField]
        private GameObject buildingQuestionMark;

        public GameObject unitParent;

        public Image hexImage;
        public GameObject hexStatsParent;

        private void Awake()
        {
            Instance = this;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            ClearChilds();

            WorldUI.Instance.GridPositionSelected += GridPositionSelected;
        }

        public void AddUnit(UnitInfoUI_1 unit)
        {
            UnitInfoUI_1 clone = Instantiate(unit, unitParent.transform);
        }

        private void GridPositionSelected(Vector2Int gridPos)
        {
            ClearHexStats();
            InsertUnits(gridPos);
            InsertBiomeData(gridPos);

            // we have no data for now
            EnableBuildingQuestionMark = true;
        }

        private void InsertUnits(Vector2Int gridPos)
        {
            List<UnitInfoUI_1> info;
            WorldUI.Instance.GetUnits(gridPos, out info);
            if (info != null)
            {
                ClearUnits();
                foreach (UnitInfoUI_1 unit in info)
                {
                    UnitInfoUI_1 clone = Instantiate(unit, unitParent.transform);
                }
            }
        }
        private void InsertBiomeData(Vector2Int gridPos)
        {
            recent = gridPos;

            Material data = Worldmap.Instance.GetMaterial(gridPos);

            hexImage.material = data;

            BackendMessenger.Instance.GetBiome(gridPos);

            return;

            BiomeData biomeData = Worldmap.Instance.GetBiomeData(gridPos);

            StatItemCard startCardPrefab = WorldUI.Instance.UIManager.starItemCard;

            List<StatItemCard> stats = biomeData.CreateList(startCardPrefab);

            foreach (StatItemCard stat in stats)
            {
                stat.gameObject.SetActive(true);
                stat.transform.SetParent(hexStatsParent.transform, false);
            }
        }

        private Vector2Int recent;
        public void SyncServerBiome(BiomeData data)
        {
            BiomeData biomeData = Worldmap.Instance.GetBiomeData(recent);

            StatItemCard startCardPrefab = WorldUI.Instance.UIManager.starItemCard;

            List<StatItemCard> stats = biomeData.CreateList_Sync(startCardPrefab, data);

            StatItemCard owned = Object.Instantiate(startCardPrefab);

            owned.Label = "Hex Owned: ";
            owned.Value = GameManager.Instance.GamePlayer.OwnedPositions.Contains(recent).ToString();

            StatItemCard recon = Object.Instantiate(startCardPrefab);

            ReconPosition rp = new ReconPosition(recent);

            GameManager.Instance.GamePlayer.ReconPositions.TryGetValue(rp, out rp);

            recon.Label = "Recon: ";
            recon.Value = rp.ReconLevel.ToString();

            foreach (StatItemCard stat in stats)
            {
                stat.gameObject.SetActive(true);
                stat.transform.SetParent(hexStatsParent.transform, false);
            }

            owned.gameObject.SetActive(true);
            owned.transform.SetParent(hexStatsParent.transform, false);

            recon.gameObject.SetActive(true);
            recon.transform.SetParent(hexStatsParent.transform, false);
        }

        public void ClearBuildings()
        {
            BuildingTitle = "";
            BuildingDescription = "";
        }

        public void ClearUnits()
        {
            foreach (Transform child in unitParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void ClearHexStats()
        {
            hexImage.material = null;

            foreach (Transform child in hexStatsParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void ClearChilds()
        {
            ClearBuildings();
            ClearUnits();
            ClearHexStats();
        }

    }
}
