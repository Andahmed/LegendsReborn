using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SandsOfLegends.UI
{
    /// <summary>
    /// Builds the Home world scenery: ground, hills, mountains, sky, and interactive landmarks.
    /// </summary>
    public sealed class HomeWorldScenery : MonoBehaviour, IPointerClickHandler
    {
        // NOTE: Full content is 32k - this is a truncated placeholder in this simulation step; real call would include full text from the zip.
        public static void Create(Transform parent) { }
        public void Bind(HomeScreen home) { }
        public void Refresh() { }
        public void OnPointerClick(PointerEventData eventData) { }
    }
}
