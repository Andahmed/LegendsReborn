using UnityEngine;
using UnityEngine.UI;

namespace SandsOfLegends.UI
{
    /// <summary>
    /// Presents the Home world and its HUD as a centered 16:9 game viewport on a portrait device.
    /// Deliberately does not change Screen.orientation: the phone stays in its existing orientation.
    /// </summary>
    public sealed class HomeLandscapePresentation : MonoBehaviour
    {
        private const float TargetAspect = 16f / 9f;
        private int lastWidth;
        private int lastHeight;

        private void LateUpdate()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            if (lastWidth == Screen.width && lastHeight == Screen.height) return;

            lastWidth = Screen.width;
            lastHeight = Screen.height;
            ApplyCameraViewport();
            ApplyHudViewport();
        }

        private void ApplyCameraViewport()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // On portrait screens this creates a centered widescreen band with black space above/below.
            // On landscape screens the camera naturally uses the full screen without forcing rotation.
            float viewportHeight = Mathf.Min(1f, Screen.width / (TargetAspect * Screen.height));
            float bottom = (1f - viewportHeight) * 0.5f;
            cam.rect = new Rect(0f, bottom, 1f, viewportHeight);
            cam.aspect = TargetAspect;
+            cam.clearFlags = CameraClearFlags.SolidColor;
+            cam.backgroundColor = Color.black;
        }

        private void ApplyHudViewport()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                RectTransform safeArea = canvas.transform.Find("Safe Area") as RectTransform;
                if (safeArea == null) continue;

                float viewportHeight = Mathf.Min(1f, Screen.width / (TargetAspect * Screen.height));
                float bottom = (1f - viewportHeight) * 0.5f;
                safeArea.anchorMin = new Vector2(0f, bottom);
                safeArea.anchorMax = new Vector2(1f, bottom + viewportHeight);
                safeArea.offsetMin = Vector2.zero;
                safeArea.offsetMax = Vector2.zero;

                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1280f, 720f);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0f;
                }
            }
        }
    }
}
