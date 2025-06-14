using CaseMaroon.WorldMapUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CaseMaroon.Units
{
    public enum UnitLevel { One, Two, Three };
    public class UnitInfoUI_1 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image image;
        public TMP_Text unitName;
        public TMP_Text healthPoints;
        public TMP_Text readyPoints;

        public Outline UIOutline;

        [SerializeField]
        private GameObject QuestionMark;

        public bool EnableQuestionMark
        {
            get
            {
                return QuestionMark.activeSelf;
            }
            set
            {
                QuestionMark.SetActive(value);
            }
        }

        public UnitData data;
        public void Initiliaze(UnitData data)
        {
            this.data = data;
            this.name = data.UnitId;

            image.sprite = data.Image;
            unitName.text = data.UnitName;

            healthPoints.text = data.HealthPoints.ToString();
            readyPoints.text = data.EnergyPoints.ToString();
        }

        public void UpdateValues()
        {
            healthPoints.text = data.HealthPoints.ToString();
            readyPoints.text = data.EnergyPoints.ToString();
        }

        public Vector2Int gridPosition;

        public void MoveToPosition(Vector3 worldPos)
        {
            RectTransform rect = GetComponent<RectTransform>();

            // Preserve current world Z position
            float currentZ = rect.position.z;

            // Apply new position with preserved Z
            rect.position = new Vector3(worldPos.x, worldPos.y, currentZ);
        }

        public void StackUnit(int index, Vector3 worldPos)
        {
            RectTransform rect = transform.GetComponent<RectTransform>();

            float multiplier = .015f;
            float offset = (index * 2 * multiplier);

            float zpos = rect.position.z;

            Vector3 tVec = worldPos + (new Vector3(offset, -offset, zpos));

            float size = index * multiplier;
            Vector3 newSize = Vector3.one - new Vector3(size, size, size);

            rect.localScale = newSize;
            rect.position = tVec;

            transform.SetSiblingIndex(index);
        }

        public void IsSelected()
        {
            // use its movement to highlight the radius of hexes around it.


        }
        public void EnableOutline()
        {
            UIOutline.enabled = true;
        }

        public void DisableOutline()
        {
            UIOutline.enabled = false;
        }

        GameObject remove = null;
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(remove != null)
            {
                return;
            }

            UnitInfoUI_2 u2 = WorldMapUI.WorldUI.Instance.uiManager.unitInfo_2;

            remove = Instantiate(u2.gameObject, this.transform);

            RectTransform u2Info = remove.GetComponent<RectTransform>();

            RectTransform cur = transform.GetComponent<RectTransform>();

            //u2Info.position = cur.rect.position;

            Vector3 offset = new Vector3(cur.rect.width * 2 + 10, 0);

            u2Info.localPosition = offset;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            Destroy(remove);
        }


    }
}
