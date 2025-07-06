using System;
using UnityEngine;
using CaseMaroon.WorldMap;
using CaseMaroon.WorldMapUI;
using CaseMaroon.Miscellaneous;
using System.Collections;
using CaseMaroon.Units;

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
        public enum InitMessage { Welcome, SpawnHQ, PlaceUnits}

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
            Worldmap.Instance.OnWorldGenerated += OnWorldGenerated;
            WorldUI.Instance.BuildingPlaced += BuildingPlaced;
        }

        bool hqPlaced = false;
        private void BuildingPlaced(Vector2Int gridPos, Building building)
        {
            if(building.buildingType == GlobalData.BuildingType.Headquarters)
            {
                hqPlaced = true;
            }
        }

        private void OnWorldGenerated(Worldmap map)
        {
            StartCoroutine(StartGameSequenceCoroutine());
        }

        private IEnumerator StartGameSequenceCoroutine()
        {
            yield return new WaitForSeconds(1f);

            MessageData welcomeMsg = messageManager.GetMessage(InitMessage.Welcome.ToString());


            // Show box 1
            MessageBox box1 = MessageBox.Show(welcomeMsg);

            // Wait until box1 is destroyed (closed)
            yield return new WaitUntil(() => box1 == null);

            // Wait a short delay
            yield return new WaitForSeconds(1f);

            MessageData spawnHq = messageManager.GetMessage(InitMessage.SpawnHQ.ToString());
            // Show box 2
            MessageBox box2 = MessageBox.Show(spawnHq);

            // Wait for box2 to close too (optional)
            yield return new WaitUntil(() => box2 == null);

            // Highlight HQ card
            SideOverlay.Instance.HighlightCard(GlobalData.BuildingType.Headquarters, true);

            // wait for hq placed event

            yield return new WaitUntil(() => hqPlaced);

            SideOverlay.Instance.HighlightCard(GlobalData.BuildingType.Headquarters, false);

            MessageData placeUnit = messageManager.GetMessage(InitMessage.PlaceUnits.ToString());
            // Show box 2
            MessageBox box3 = MessageBox.Show(placeUnit);

        }


    }
}
