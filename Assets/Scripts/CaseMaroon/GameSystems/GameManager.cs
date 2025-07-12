using System;
using UnityEngine;
using CaseMaroon.WorldMap;
using CaseMaroon.WorldMapUI;
using CaseMaroon.Miscellaneous;
using System.Collections;
using CaseMaroon.Units;
using static CaseMaroon.Miscellaneous.GlobalData;
using CaseMaroon.Backend;

namespace CaseMaroon.GameSystem
{
    [Serializable]
    public struct MessageData
    {
        [StringDropdown]
        public string id;
        public string title;
        [TextArea(3, 10)]
        public string message;
    }
    public class GameManager : MonoBehaviour
    {
        public GameManager Instance { get; private set; }
        public MessageManager messageManager;

        public bool StartSequence = false;
        public enum InitMessage { Welcome, SpawnHQ, PlaceUnits}
        public enum InitGameState
        {
            Idle, 
            WaitingForHQPlacement,
            WaitingForUnitPlacement,
            WaitingForNext
        }

        public InitGameState currentInitState;
        private void Awake()
        {
            Instance = this;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Prevent duplicates
                return;
            }
        }
        private void Start()
        {
            DisableButtons();
            Worldmap.Instance.OnWorldGenerated += OnWorldGenerated;
        }

        private void OnValidate()
        {
            if (Application.isPlaying && StartSequence && Worldmap.Instance.WorldGenerated)
            {
                StopCoroutine(StartGameSequenceCoroutine());
                StartCoroutine(StartGameSequenceCoroutine());
            }
        }

        private void OnWorldGenerated(Worldmap map)
        {
            BackendTester.Instance.SyncGameState();

            if(StartSequence)
            {
                StartCoroutine(StartGameSequenceCoroutine());
            }
        }

        public void DisableButtons()
        {
            SideOverlay.Instance.buildingButton.enabled = false;
            SideOverlay.Instance.unitButton.enabled = false;
        }
        private void SpawnHQ_Step()
        {
            SideOverlay.Instance.buildingButton.enabled = true;

            SideOverlay.Instance.FlipOutlines(SideOverlay.SelectedCard.BuildingCards);

            SideOverlay.Instance.AddBuildingCard(BuildingType.Headquarters);

            SideOverlay.Instance.HighlightBuildingCard(BuildingType.Headquarters, true);

            currentInitState = InitGameState.WaitingForHQPlacement;

            // Define the handler using the correct delegate type
            BuildingPlacedHandler onBuildingPlaced = null;

            onBuildingPlaced = (Vector2Int gridPos, Building building) =>
            {
                if (building.buildingType == BuildingType.Headquarters)
                {
                    SideOverlay.Instance.HighlightBuildingCard(BuildingType.Headquarters, false);

                    currentInitState = InitGameState.Idle;
                    // Unsubscribe from event
                    WorldUI.Instance.BuildingPlaced -= onBuildingPlaced;
                }
            };

            // Subscribe to the event
            WorldUI.Instance.BuildingPlaced += onBuildingPlaced;
        }
        private void SpawnUnit_Step()
        {
            SideOverlay.Instance.unitButton.enabled = true;
            SideOverlay.Instance.buildingButton.enabled = false;

            SideOverlay.Instance.AddUnitCard(UnitType.Infantry);
            SideOverlay.Instance.ShowBuildingCards();

            // Highlight HQ card
            SideOverlay.Instance.HighlightButton(SideOverlay.SelectedCard.UnitCards, true);

            SideOverlay.Instance.HighlightUnitCard(UnitType.Infantry, true);

            currentInitState = InitGameState.WaitingForUnitPlacement;

            // Define the handler using the correct delegate type
            UnitPlacedHandler onUnitPlaced = null;

            onUnitPlaced = (Vector2Int gridPos, UnitType unit) =>
            {
                if (unit == UnitType.Infantry)
                {
                    SideOverlay.Instance.HighlightUnitCard(UnitType.Infantry, false);
                    SideOverlay.Instance.HighlightButton(SideOverlay.SelectedCard.UnitCards, false);

                    SideOverlay.Instance.buildingButton.enabled = true;

                    SideOverlay.Instance.FlipOutlines(SideOverlay.SelectedCard.UnitCards);

                    currentInitState = InitGameState.Idle;
                    // Unsubscribe from event
                    WorldUI.Instance.UnitPlaced -= onUnitPlaced;
                }
            };

            // Subscribe to the event
            WorldUI.Instance.UnitPlaced += onUnitPlaced;
        }
        private IEnumerator StartGameSequenceCoroutine()
        {
            yield return new WaitForSeconds(1f);

            MessageData welcomeMsg = messageManager.GetMessage(InitMessage.Welcome.ToString());

            // Show box 1
            MessageBox box1 = MessageBox.Show(welcomeMsg);

            // Wait until box1 is destroyed (closed)
            yield return new WaitUntil(() => box1 == null);
            ///////////////////////////////        

            // Wait a short delay
            yield return new WaitForSeconds(1f);

            MessageData spawnHq = messageManager.GetMessage(InitMessage.SpawnHQ.ToString());
            // Show box 2
            MessageBox box2 = MessageBox.Show(spawnHq);

            // Wait for box2 to close too (optional)
            yield return new WaitUntil(() => box2 == null);

            SpawnHQ_Step();

            yield return new WaitUntil(() => currentInitState == InitGameState.Idle);

            MessageData placeUnit = messageManager.GetMessage(InitMessage.PlaceUnits.ToString());

            // Show box 2
            MessageBox box3 = MessageBox.Show(placeUnit);

            // Wait for box2 to close too (optional)
            yield return new WaitUntil(() => box3 == null);

            SpawnUnit_Step();

            yield return new WaitUntil(() => currentInitState == InitGameState.Idle);

            MessageData msg = new MessageData();

            msg.title = "Done";
            msg.message = "Let Us Begin!";

            // Show box 2
            MessageBox box4 = MessageBox.Show(msg);


        }

    }
}
