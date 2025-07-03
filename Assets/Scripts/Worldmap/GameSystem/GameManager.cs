using UnityEngine;
using CaseMaroon.WorldMap;

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

            
        }



    }
}
