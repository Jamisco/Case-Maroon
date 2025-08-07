
using UnityEngine;
using CaseMaroon.Units;
using System.Collections.Generic;
using CaseMaroon.Backend;
using CaseMaroon.WorldMap;
using System.Linq;

namespace CaseMaroon.GameSystem
{
    public enum GamePlayer
    {
        PlayerOne = 0,
        PlayerTwo = 1
    }
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }
        public Player PlayerOne { get; private set; }
        public Player PlayerTwo{ get; private set; }

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
            PlayerOne = new Player(gsr.players[0]);

            WorldmapOverlay.Instance.UpdateReconOverlay();

            // as u spawn buildings add hexes to owned buildings
        }

        public void SetInitialWorldData()
        {

        }

    }
}
