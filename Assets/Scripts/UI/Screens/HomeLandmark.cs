using UnityEngine;

namespace SandsOfLegends.UI
{
    /// <summary>Marks a tappable building in the Home world. The key is mapped to a HomeScreen event.</summary>
    public sealed class HomeLandmark : MonoBehaviour
    {
        [SerializeField] string landmarkKey = "campaign";

        public string LandmarkKey => landmarkKey;
    }
}
