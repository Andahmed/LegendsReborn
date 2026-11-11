// =============================================================================
// FILE: Assets/Scripts/Bootstrap/PrototypeBootRouter.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Route the prototype startup flow from Boot to Home after GameBootstrap initializes.
// WORK ON: Replace the prototype one-frame handoff with the production loading/login flow.
// STATUS: Prototype implementation
// =============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SandsOfLegends.Bootstrap
{
    public sealed class PrototypeBootRouter : MonoBehaviour
    {
        private void Start()
        {
            if (SceneManager.GetActiveScene().name == "Boot")
                SceneManager.LoadScene("Home");
        }
    }
}
