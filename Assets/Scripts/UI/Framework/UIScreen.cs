// =============================================================================
// FILE: Assets/Scripts/UI/Framework/UIScreen.cs
// OWNER ROLE: Client Programmer - UI
// RESPONSIBILITY: Base class for every full-screen UI (Show/Hide hooks).
// WORK ON: Add transitions, back-button handling.
// STATUS: Implemented
// =============================================================================
using UnityEngine;

namespace SandsOfLegends.UI
{
    public abstract class UIScreen : MonoBehaviour
    {
        public virtual void OnShow() { gameObject.SetActive(true); }
        public virtual void OnHide() { gameObject.SetActive(false); }
    }
}
