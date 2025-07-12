
using UnityEngine;
using CaseMaroon.Units;
using System.Collections.Generic;
using CaseMaroon.Backend;
using CaseMaroon.WorldMap;
using System.Linq;

namespace CaseMaroon.GameSystem
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public Player MainPlayer { get; private set; }

        public Dictionary<Vector2Int, Building> buildings = new();
        public Dictionary<Vector2Int, Unit> units = new();

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
            BackendTester.Instance.GameStateSynced += GameStateSynced;

        }
        private void GameStateSynced(BackendResponses.GameStateResponse gsr)
        {
            MainPlayer = new Player(gsr.players[0]);

            WorldmapOverlay.Instance.UpdateReconOverlay();

            // as u spawn buildings add hexes to owned buildings
        }

        public void SetInitialWorldData()
        {

        }

    }
}
