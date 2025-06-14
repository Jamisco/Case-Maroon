using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    public class ScreenSpaceUI : MonoBehaviour
    {
        public TopOverlay topOverlay;
        public BotOverlay botOverlay;

        private void Awake()
        {
            
        }

        private void Start()
        {
            
        }
        public void ClearOverlay()
        {
            botOverlay.ClearChilds();
        }
        

    }
}
