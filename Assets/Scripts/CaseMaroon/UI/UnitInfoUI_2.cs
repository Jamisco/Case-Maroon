using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    public class UnitInfoUI_2 : MonoBehaviour
    {
        // Start is called before the first frame update

        [SerializeField]
        private GameObject statParent;

        [SerializeField]
        private GameObject questionObj;

        public bool EnableQuestionMark
        {
            get
            {
                return questionObj.activeSelf;
            }
            set
            {
                questionObj.SetActive(value);
            }
        }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddData(StatItemCard stat)
        {
            questionObj.SetActive(false);

            stat.transform.SetParent(stat.transform);
            stat.gameObject.SetActive(true);
            // Optionally, you can reset the parent to the original prefab
            // statItemParent.gameObject.SetActive(false);
        }
    }
}