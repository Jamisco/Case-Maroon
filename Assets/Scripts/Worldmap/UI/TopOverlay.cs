using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaseMaroon.WorldMapUI
{
    public class TopOverlay : MonoBehaviour
    {
        public GameObject leftParent;
        public GameObject rightParent;

        public StatItemCard statItemPrefab;

        private TextMeshProUGUI gameTimeObj;
        private TextMeshProUGUI gameTurnObj;
        private Button buttonObj;

        public string GameTime
        {
            get
            {
                return gameTimeObj.text;
            }
            set
            {
                gameTimeObj.text = value;
            }
        }
        public string GameTurn
        {
            get
            {
                return gameTurnObj.text;
            }
            set
            {
                gameTurnObj.text = value;
            }
        }

    }
}
