using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaseMaroon.WorldMapUI
{

    /// <summary>
    /// Not currently in use
    /// </summary>
    public class BuildingInfoCard : MonoBehaviour
    {
        public string Title
        {
            get
            {
                return titleObj.text;
            }
            set
            {
                titleObj.text = value;
            }
        }
        public string Description
        {
            get
            {
                return descriptionObj.text;
            }
            set
            {
                descriptionObj.text = value;
            }
        }

        public Image BuildingImage;

        [SerializeField]
        private TextMeshProUGUI titleObj;

        [SerializeField]
        private TextMeshProUGUI descriptionObj;

    }
}
