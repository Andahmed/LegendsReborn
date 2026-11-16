using UnityEngine;

namespace SandsOfLegends.World
{
    /// <summary>Proximity marker used by the prototype HUD; no campaign state is changed.</summary>
    public sealed class OpenWorldLandmark : MonoBehaviour
    {
        [SerializeField] private string displayName;
        public string DisplayName { get { return string.IsNullOrEmpty(displayName) ? gameObject.name : displayName; } }
        public void Initialize(string value) { displayName = value; }
    }
}