using UnityEngine;

namespace SandsOfLegends.UI
{
    /// <summary>
    /// Loads and displays the Home world stage (buildings, landmarks, background).
    /// Coordinates with HomeScreen for navigation and event routing.
    /// </summary>
    public sealed class HomeWorldStage : MonoBehaviour
    {
        [SerializeField] Transform stageRoot;
        [SerializeField] HomeWorldScenery scenery;
        [SerializeField] Camera worldCamera;

        void Awake()
        {
            if (scenery == null)
                scenery = GetComponentInChildren<HomeWorldScenery>(true);
        }

        public void Bind(HomeScreen home)
        {
            if (scenery != null)
                scenery.Bind(home);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (scenery != null)
                scenery.Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
