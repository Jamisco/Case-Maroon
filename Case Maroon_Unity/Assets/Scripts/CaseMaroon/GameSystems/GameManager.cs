using CaseMaroon.Backend;
using CaseMaroon.Miscellaneous;
using CaseMaroon.Units;
using CaseMaroon.WorldMap;
using CaseMaroon.WorldMapUI;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using static CaseMaroon.Miscellaneous.GlobalData;

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
        public static GameManager Instance { get; private set; }
        public MessageManager messageManager;

        public float messageDelay = .5f;
        public bool StartSequence = false;
        public enum InitMessage { Welcome, SpawnHQ, PlaceUnits}
        public enum PlayerState
        {
            Loading,
            Idle, 
            WaitingForHQPlacement,
            WaitingForUnitPlacement,
            WaitingForNext
        }

        public PlayerState playerState;
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
            playerState = PlayerState.Loading;
            Worldmap.Instance.OnWorldGenerated += OnWorldGenerated;
        }
        public void RestartGame()
        {
            Worldmap.Instance.Start();
            BuildingOverlay.Instance.buildings.Clear();
            WorldUI.Instance.Clear();

            StopCoroutine(StartGameSequenceCoroutine());
            StartCoroutine(StartGameSequenceCoroutine());
        }
        private void OnWorldGenerated(Worldmap map)
        {
            BackendMessenger.Instance.GetGameState();

            BackendMessenger.Instance.OnPlayerStatesResponse += OnPlayerStatesResponse;
            BackendMessenger.Instance.PollPlayerStates();
        }

        private void OnPlayerStatesResponse(BackendResponses.PlayersStatusResponse response)
        {
            
        }

        public void StartGameSequence()
        {
            if (StartSequence)
            {
                ScreenSpaceUI.Instance.HideSplashShowRest();

                DisableButtons();
                StartCoroutine(StartGameSequenceCoroutine());
            }
            else
            {
                ScreenSpaceUI.Instance.HideSplashShowRest();

                SideOverlay.Instance.CreateAllCards();
                SideOverlay.Instance.FlipOutlines(SideOverlay.SelectedCard.BuildingCards);
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
            SideOverlay.Instance.RemoveAllCards();

            SideOverlay.Instance.FlipOutlines(SideOverlay.SelectedCard.BuildingCards);

            SideOverlay.Instance.AddBuildingCard(BuildingType.Headquarters);

            SideOverlay.Instance.HighlightBuildingCard(BuildingType.Headquarters, true);

            playerState = PlayerState.WaitingForHQPlacement;

            // Define the handler using the correct delegate type
            BuildingPlacedHandler onBuildingPlaced = null;

            onBuildingPlaced = (Vector2Int gridPos, Building building) =>
            {
                if (building.buildingType == BuildingType.Headquarters)
                {
                    SideOverlay.Instance.HighlightBuildingCard(BuildingType.Headquarters, false);

                    playerState = PlayerState.Idle;
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

            playerState = PlayerState.WaitingForUnitPlacement;

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

                    playerState = PlayerState.Idle;
                    // Unsubscribe from event
                    WorldUI.Instance.UnitPlaced -= onUnitPlaced;
                }
            };

            // Subscribe to the event
            WorldUI.Instance.UnitPlaced += onUnitPlaced;
        }
        private IEnumerator StartGameSequenceCoroutine()
        {
            yield return new WaitForSeconds(messageDelay);

            MessageData welcomeMsg = messageManager.GetMessage(InitMessage.Welcome.ToString());

            // Show box 1
            MessageBox box1 = MessageBox.Show(welcomeMsg);

            // Wait until box1 is destroyed (closed)
            yield return new WaitUntil(() => box1 == null);
            ///////////////////////////////        

            // Wait a short delay
            yield return new WaitForSeconds(messageDelay);

            MessageData spawnHq = messageManager.GetMessage(InitMessage.SpawnHQ.ToString());
            // Show box 2
            MessageBox box2 = MessageBox.Show(spawnHq);

            // Wait for box2 to close too (optional)
            yield return new WaitUntil(() => box2 == null);

            SpawnHQ_Step();

            yield return new WaitUntil(() => playerState == PlayerState.Idle);

            MessageData placeUnit = messageManager.GetMessage(InitMessage.PlaceUnits.ToString());

            // Show box 2
            MessageBox box3 = MessageBox.Show(placeUnit);

            // Wait for box2 to close too (optional)
            yield return new WaitUntil(() => box3 == null);

            SpawnUnit_Step();

            yield return new WaitUntil(() => playerState == PlayerState.Idle);

            MessageData msg = new MessageData();

            msg.title = "Done";
            msg.message = "Let Us Begin!";

            // Show box 2
            MessageBox box4 = MessageBox.Show(msg);


        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GameManager exampleScript = (GameManager)target;

            if (GUILayout.Button("Restart Game"))
            {
                exampleScript.RestartGame();
            }
        }
    }

#endif
}
