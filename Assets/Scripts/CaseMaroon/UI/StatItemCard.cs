using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    public class StatItemCard : MonoBehaviour
    {
        public static string LabelText = "labelTxt";
        public static string ValueText = "valueTxt";
        public string Label
        {
            get
            {
                return labelObj.GetComponent<TMPro.TMP_Text>().text;
            }
            set
            {
                labelObj.GetComponent<TMPro.TMP_Text>().text = value;
            }
        }
        public string Value
        {
            get
            {
                return valueObj.GetComponent<TMPro.TMP_Text>().text;
            }
            set
            {
                valueObj.GetComponent<TMPro.TMP_Text>().text = value;
            }
        }

        private GameObject parent;

        [SerializeField]
        private GameObject labelObj;
        [SerializeField]
        private GameObject valueObj;

        private void Awake()
        {
            //labelObj = transform.Find(LabelText).gameObject;
            //valueObj = transform.Find(ValueText).gameObject;
        }

        //public StatItemCard Clone(string label, string value)
        //{
        //    GameObject newItem = Instantiate(parent);

        //    StatItemCard newStatItem = newItem.GetComponent<StatItemCard>();

        //    newItem.SetActive(true);

        //    newStatItem.Label = label;
        //    newStatItem.Value = value;

        //    return newStatItem;
        //}
    }
}
