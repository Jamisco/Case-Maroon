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

        public enum SelectedCard { BuildingCards, UnitCards }

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

        public void FlipOutlines(SelectedCard card)
        {
            if(card == SelectedCard.BuildingCards)
            {
                buildingButton.GetComponent<Outline>().enabled = true;
                unitButton.GetComponent<Outline>().enabled = false;
            }
            else
            {
                buildingButton.GetComponent<Outline>().enabled = false;
                unitButton.GetComponent<Outline>().enabled = true;
            }
        }

        private void Start()
        {
            // by default, we show buildings first
            buildingButton.GetComponent<Outline>().enabled = false;
            unitButton.GetComponent<Outline>().enabled = false;

            buildingButton.onClick.AddListener(() =>
            {
                FlipOutlines(SelectedCard.BuildingCards);
                ShowBuildingCards();
            });

            unitButton.onClick.AddListener(() =>
            {
                FlipOutlines(SelectedCard.UnitCards);
                ShowUnitCards();
            });
        }
        public void CreateAllCards()
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
        public void ShowUnitCards()
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
        public void ShowBuildingCards()
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

        public void AddBuildingCard(BuildingType type)
        {
            bool contains = buildingCards.Any(x => x.buildType == type);

            if(contains)
            {
                return;
            }

            BuildingItemCard buildingPrefab = WorldUI.Instance.UIManager.buildingItemCard;

            BuildingItemCard card = Instantiate(buildingPrefab, cardParent.transform);

            card.SetBuilding(type);
            buildingCards.Add(card);
        }

        public void AddUnitCard
            (UnitType type)
        {
            bool contains = unitCards.Any(x => x.unitType == type);

            if (contains)
            {
                return;
            }

            UnitItemCard prefab = WorldUI.Instance.UIManager.unitItemCard;

            UnitItemCard card = Instantiate(prefab, cardParent.transform);

            card.SetUnit(type);
            unitCards.Add(card);
        }

        private Coroutine flickerCoroutine;
        private float initAlpha;
        private bool outlineState = false;
        public void HighlightBuildingCard(BuildingType buildType, bool enable = true)
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

        public void HighlightUnitCard(UnitType unitType, bool enable = true)
        {
            UnitItemCard card = unitCards.FirstOrDefault(x => x.unitType == unitType);

            if (card == null || card.outlineObj == null)
                return;

            if (enable)
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

        private Coroutine btnCoroutine;
        private bool btnOtState;
        private float btnOtAlpha;

        public void HighlightButton(SelectedCard card, bool enable = true)
        {
            Outline ot = (card == SelectedCard.BuildingCards) ? buildingButton.GetComponent<Outline>() : unitButton.GetComponent<Outline>();

            if (enable)
            {
                btnOtState = ot.enabled;
                btnOtAlpha = ot.effectColor.a;

                btnCoroutine = StartCoroutine(FlickerOutline(ot));
            }
            else
            {
                StopCoroutine(btnCoroutine);
                SetOutlineAlpha(ot, btnOtAlpha);
                ot.enabled = btnOtState;
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
        public void RemoveAllCards()
        {
            foreach (UnitItemCard child in unitCards)
            {
                child.gameObject.SetActive(false);
            }

            // Instantiate building cards
            foreach (BuildingItemCard card in buildingCards)
            {
                card.gameObject.SetActive(false);
            }
        }
    }
}
