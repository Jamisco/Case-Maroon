using CaseMaroon.GameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaseMaroon.WorldMapUI
{
    public class MessageBox : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleObj;

        [SerializeField]
        private TextMeshProUGUI messageObj;

        [SerializeField]
        private Button buttonObj;

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
        public string Message
        {
            get
            {
                return messageObj.text;
            }
            set
            {
                messageObj.text = value;
            }
        }
        private void Awake()
        {
        }

        private void Start()
        {
            buttonObj.onClick.AddListener(CloseBox);
        }

        private void CloseBox()
        {
            Destroy(this.gameObject);
        }

        /// <summary>
        /// Will create, show in screen space and return a message box
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MessageBox Show(string title, string message)
        {
            MessageBox prefab = WorldUI.Instance.UIManager.messageBox;
            ScreenSpaceUI screenCanvas = FindAnyObjectByType<ScreenSpaceUI>();

            MessageBox box = Instantiate(prefab, screenCanvas.transform);

            box.Title = title;
            box.Message = message;

            return box;

        }

        public static MessageBox Show(MessageData data)
        {
            return Show(data.title, data.message);
        }
    }
}
