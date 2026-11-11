// =============================================================================
// FILE: Assets/Scripts/Presentation/PrototypeHomeController.cs
// OWNER ROLE: Client Programmer - UI
// RESPONSIBILITY: Wire the prototype Home scene's Enter Battle button to the generated Battle scene.
// WORK ON: Replace the temporary scene jump with the production home navigation and unlock flow.
// STATUS: Prototype implementation
// =============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SandsOfLegends.Presentation
{
    public sealed class PrototypeHomeController : MonoBehaviour
    {
        public Button battleButton;

        private void Start()
        {
            if (battleButton != null) battleButton.onClick.AddListener(LoadBattle);
        }

        private void OnDestroy()
        {
            if (battleButton != null) battleButton.onClick.RemoveListener(LoadBattle);
        }

        private void LoadBattle()
        {
            SceneManager.LoadScene("Battle");
        }
    }
}
