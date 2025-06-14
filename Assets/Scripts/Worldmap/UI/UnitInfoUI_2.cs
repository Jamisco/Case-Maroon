using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    public class UnitInfoUI_2 : MonoBehaviour
    {
        public StatItemCard statItemParent;
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
            statItemParent.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}