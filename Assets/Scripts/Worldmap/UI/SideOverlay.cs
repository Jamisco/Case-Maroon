using CaseMaroon.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.WorldMapUI
{
    public class SideOverlay : MonoBehaviour
    {
        public Button buildingButton;
        public Button unitButton;

        public GameObject cardParent;

        private List<UnitItemCard> unitCards = new List<UnitItemCard>();
        private List<BuildingItemCard> buildingCards = new List<BuildingItemCard>();

        public bool BuildingMode { get; private set; } = true;

        private void Awake()
        {
            
        }

        void FlipOutlines()
        {
            bool bb = buildingButton.GetComponent<Outline>().enabled;
            buildingButton.GetComponent<Outline>().enabled = !bb;

            bool ub = unitButton.GetComponent<Outline>().enabled;
            unitButton.GetComponent<Outline>().enabled = !ub;
        }

        private void Start()
        {
            InitCards();

            // by default, we show buildings first
            buildingButton.GetComponent<Outline>().enabled = true;
            unitButton.GetComponent<Outline>().enabled = false;
            FillWithBuildings();

            buildingButton.onClick.AddListener(() =>
            {
                FlipOutlines();
                FillWithBuildings();
            });

            unitButton.onClick.AddListener(() =>
            {
                FlipOutlines();
                FillWithTroops();
            });
        }

        void InitCards()
        {
            UnitItemCard unitPrefab = WorldUI.Instance.uiManager.unitItemCard;
            // Initialize unit cards
            foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
            {
                UnitItemCard card = Instantiate(unitPrefab, cardParent.transform);
                card.SetUnit(type);
                unitCards.Add(card);
            }

            BuildingItemCard buildingPrefab = WorldUI.Instance.uiManager.buildingItemCard;

            // Initialize building cards
            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                BuildingItemCard card = Instantiate(buildingPrefab, cardParent.transform);

                card.buildType = type;
                buildingCards.Add(card);
            }
        }

        void FillWithTroops()
        {
            // Clear existing cards
            foreach (BuildingItemCard child in buildingCards)
            {
               child.gameObject.SetActive(false);
            }

            // Instantiate unit cards
            foreach (UnitItemCard card in unitCards)
            {
                card.gameObject.SetActive(true);
            }
        }
        void FillWithBuildings()
        {
            // Clear existing cards
            foreach (UnitItemCard child in unitCards)
            {
                child.gameObject.SetActive(false);
            }

            // Instantiate building cards
            foreach (BuildingItemCard card in buildingCards)
            {
                card.gameObject.SetActive(true);
            }
        }
    }
}
