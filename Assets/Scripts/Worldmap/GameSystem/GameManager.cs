using UnityEngine;
using CaseMaroon.WorldMap;
using CaseMaroon.WorldMapUI;

namespace CaseMaroon.GameSystem
{
    public class GameManager : MonoBehaviour
    {
        public GameManager Instance { get; private set; }

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
        }

        bool canBegin = false;
        private void OnWorldGenerated(Worldmap map)
        {
            string title = "Welcome General";
            string message = "Your Objective is to Capture The Enemy Headquarters.\n" + "Good Luck";

            MessageBox.Show(title, message);

        }

    }
}
