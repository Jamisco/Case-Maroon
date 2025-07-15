using CaseMaroon.WorldMap;
using CaseMaroon.WorldMapUI;
using System;
using System.Collections;
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

        public Unit unit;

        public Vector2Int GridPosition
        {
            get
            {
                return unit.GridPosition;
            }
            set
            {
                unit.GridPosition = value;
            }
        }

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

        public void Initiliaze(Unit data)
        {
            this.unit = data;
            this.name = data.UnitName;

            image.sprite = data.Image;
            unitName.text = data.UnitName;

            healthPoints.text = data.HealthPoints.ToString();
            readyPoints.text = data.EnergyPoints.ToString();
        }

        public void UpdateValues()
        {
            healthPoints.text = unit.HealthPoints.ToString();
            readyPoints.text = unit.EnergyPoints.ToString();
        }

        public void MoveToPosition_Instant(Vector3 worldPos)
        {
            RectTransform rect = GetComponent<RectTransform>();

            // Preserve current world Z position
            float currentZ = rect.position.z;

            // Apply new position with preserved Z
            rect.position = new Vector3(worldPos.x, worldPos.y, currentZ);
        }
        public void MoveToPosition_Animate(List<Vector2Int> gridPositions)
        {
            List<Vector3> worldPath = gridPositions
                .Select(pos => Worldmap.Instance.gridManager.GridToWorldPostion(pos))
                .ToList();

            PsuedoDrain(gridPositions);

            StartCoroutine(MoveAlongPath(worldPath, 1f)); // 1 second total movement time
        }

        private IEnumerator MoveAlongPath(List<Vector3> path, float duration)
        {
            // Preserve current world Z position so we can reset it

            RectTransform rect = GetComponent<RectTransform>();

            float currentZ = rect.position.z;

            if (path.Count < 2)
                yield break;

            float totalDistance = path.Zip(path.Skip(1), Vector3.Distance).Sum();
            float timeSoFar = 0f;
            int currentIndex = 0;

            Vector3 start = path[0];
            Vector3 end = path[1];
            float segmentDistance = Vector3.Distance(start, end);
            float segmentTime = (segmentDistance / totalDistance) * duration;

            while (currentIndex < path.Count - 1)
            {
                start = path[currentIndex];
                end = path[currentIndex + 1];
                segmentDistance = Vector3.Distance(start, end);
                segmentTime = (segmentDistance / totalDistance) * duration;

                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / segmentTime;
                    transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(t));
                    yield return null;
                }

                currentIndex++;
            }

            transform.position = path[^1]; // Snap to final position
                                           // Preserve current world Z position

            Vector3 worldPos = rect.position;
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


        private void PsuedoDrain(List<Vector2Int> path)
        {
            int percentDrain = 2;

            // hex travelled * percent Drain
            int pReduction = 30;

            unit.MovementPoints = unit.MovementPoints - pReduction;
            ClampMove();
        }

        public void DisableOutline()
        {
            UIOutline.enabled = false;
        }

        GameObject u2Object = null;
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(u2Object != null)
            {
                return;
            }

            UnitInfoUI_2 u2Prefab = WorldMapUI.WorldUI.Instance.UIManager.unitInfo_2;
            UnitInfoUI_2 u2Obj = Instantiate(u2Prefab, this.transform);

            StatItemCard statPrefab = WorldUI.Instance.UIManager.starItemCard;
            StatItemCard statItem = Instantiate(statPrefab, u2Obj.transform);

            u2Object = u2Obj.gameObject;

            statItem.Label = "Move Points: ";
            statItem.Value = unit.MovementPoints.ToString();

            u2Obj.AddData(statItem);

            RectTransform u2Info = u2Object.GetComponent<RectTransform>();

            RectTransform cur = transform.GetComponent<RectTransform>();

            //u2Info.position = cur.rect.position;

            Vector3 offset = new Vector3(cur.rect.width * 2 + 10, 0);

            u2Info.localPosition = offset;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            Destroy(u2Object);
        }

        public void SupplyUnit(int move)
        {
            unit.MovementPoints += move;

            ClampMove();
        }

        private void ClampMove()
        {
            // clmapo movement to 0 and 100

            if (unit.MovementPoints < 0)
            {
                unit.MovementPoints = 0;
            }
            else if (unit.MovementPoints > 100)
            {
                unit.MovementPoints = 100;
            }
        }
    }
}
