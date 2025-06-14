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

        private void Start()
        {
            statItemPrefab = WorldUI.Instance.uiManager.starItemCard;
            statItemPrefab.gameObject.SetActive(true);

            SetData();
        }

        public void SetData()
        {
            // Logictics Point
            // Production
            // Drain
            // Hexes Captured

            ClearObjects();

            StatItemCard logisticsPoint = Instantiate(statItemPrefab, leftParent.transform);

            logisticsPoint.Label = "Logistics Point:";
            logisticsPoint.Value = "0"; // Replace with actual value

            StatItemCard production = Instantiate(statItemPrefab, leftParent.transform);

            production.Label = "Production:";
            production.Value = "0"; // Replace with actual value

            StatItemCard drain = Instantiate(statItemPrefab, leftParent.transform);
            drain.Label = "Drain:";
            drain.Value = "0"; // Replace with actual value

            StatItemCard hexesCaptured = Instantiate(statItemPrefab, leftParent.transform);

            hexesCaptured.Label = "Hexes Captured:";
            hexesCaptured.Value = "0"; // Replace with actual value

            logisticsPoint.transform.SetParent(leftParent.transform, false);
            production.transform.SetParent(leftParent.transform, false);
            drain.transform.SetParent(leftParent.transform, false);
            hexesCaptured.transform.SetParent(leftParent.transform, false);
        }

        public void ClearObjects()
        {
            foreach (Transform child in leftParent.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in rightParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
