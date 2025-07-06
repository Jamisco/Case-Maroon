using CaseMaroon.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.WorldMapUI
{
    public class SideOverlay : MonoBehaviour
    {
        public static SideOverlay Instance { get; private set; }

        public Button buildingButton;
        public Button unitButton;

        public GameObject cardParent;

        private List<UnitItemCard> unitCards = new List<UnitItemCard>();
        private List<BuildingItemCard> buildingCards = new List<BuildingItemCard>();

        public bool BuildingMode { get; private set; } = true;

        private void Awake()
        {
            Instance = this;

            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
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
            UnitItemCard unitPrefab = WorldUI.Instance.UIManager.unitItemCard;
            // Initialize unit cards
            foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
            {
                UnitItemCard card = Instantiate(unitPrefab, cardParent.transform);
                card.SetUnit(type);
                unitCards.Add(card);
            }

            BuildingItemCard buildingPrefab = WorldUI.Instance.UIManager.buildingItemCard;

            // Initialize building cards
            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                BuildingItemCard card = Instantiate(buildingPrefab, cardParent.transform);

                card.SetBuilding(type);
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


        private Coroutine flickerCoroutine;
        private float initAlpha;
        private bool outlineState = false;
        public void HighlightCard(BuildingType buildType, bool enable = true)
        {
            BuildingItemCard card = buildingCards.FirstOrDefault(x => x.buildType == buildType);

            if (card == null || card.outlineObj == null)
                return;

            if(enable)
            {
                initAlpha = card.outlineObj.effectColor.a;
                outlineState = card.outlineObj.enabled;

                flickerCoroutine = StartCoroutine(FlickerOutline(card.outlineObj));
            }
            else
            {
                StopCoroutine(flickerCoroutine);
                SetOutlineAlpha(card.outlineObj, initAlpha);
                card.outlineObj.enabled = outlineState;
            }
        }

        private IEnumerator FlickerOutline(Outline outline)
        {
            float speed = 2f;
            Color baseColor = outline.effectColor;
            baseColor.a = 1f; // ensure max alpha is 1

            while (true)
            {
                outline.enabled = true;

                float t = Mathf.PingPong(Time.time * speed, 1f); // 0 ↔ 1
                float alpha = Mathf.Lerp(0.2f, 1f, t); // flicker between low and high alpha

                SetOutlineAlpha(outline, alpha);
                yield return null;
            }
        }

        private void SetOutlineAlpha(Outline outline, float alpha)
        {
            Color c = outline.effectColor;
            c.a = alpha;
            outline.effectColor = c;
        }

    }
}
