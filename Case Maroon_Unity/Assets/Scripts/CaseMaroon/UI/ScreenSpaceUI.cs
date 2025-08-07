using UnityEngine;

namespace CaseMaroon.WorldMapUI
{
    public class ScreenSpaceUI : MonoBehaviour
    {
        public static ScreenSpaceUI Instance { get; private set; }

        public SplashScreen splashOverlay;
        public TopOverlay topOverlay;
        public BotOverlay botOverlay;
        public SideOverlay sideOverlay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            splashOverlay = GetComponentInChildren<SplashScreen>(true);
            topOverlay = GetComponentInChildren<TopOverlay>(true);
            botOverlay = GetComponentInChildren<BotOverlay>(true);
            sideOverlay = GetComponentInChildren<SideOverlay>(true);

            ShowSplashOnly();
        }

        public void ShowSplashOnly()
        {
            splashOverlay.gameObject.SetActive(true);
            topOverlay.gameObject.SetActive(false);
            botOverlay.gameObject.SetActive(false);
            sideOverlay.gameObject.SetActive(false);
        }

        public void HideSplashShowRest()
        {
            splashOverlay.gameObject.SetActive(false);
            topOverlay.gameObject.SetActive(true);
            botOverlay.gameObject.SetActive(true);
            sideOverlay.gameObject.SetActive(true);
        }
    }
}
