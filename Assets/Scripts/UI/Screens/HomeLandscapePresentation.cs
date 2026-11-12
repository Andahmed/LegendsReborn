using UnityEngine;
using UnityEngine.UI;

namespace SandsOfLegends.UI
{
    /// <summary>
    /// Landscape presentation for the Home world. Keeps the 3D kingdom full-screen and arranges
    /// the HUD into the familiar RPG layout: player/resources at top, event shortcuts above,
    /// tappable features at the sides, and chat/navigation along the bottom.
    /// </summary>
    public sealed class HomeLandscapePresentation : MonoBehaviour
    {
        private int lastWidth;
        private int lastHeight;
        private RectTransform safeArea;
        private Canvas homeCanvas;
        private bool auxiliaryHudBuilt;

        private void Awake()
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

        private void LateUpdate()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            if (lastWidth == Screen.width && lastHeight == Screen.height) return;

            lastWidth = Screen.width;
            lastHeight = Screen.height;
            ApplyLandscapeCanvasScaling();
            ApplyCamera();
            ArrangeHud();
        }

        private void ApplyLandscapeCanvasScaling()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null) continue;
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null) continue;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        private void ApplyCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            cam.rect = new Rect(0f, 0f, 1f, 1f);
            cam.aspect = (float)Screen.width / Screen.height;
        }

        private void ArrangeHud()
        {
            if (safeArea == null)
            {
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                for (int i = 0; i < canvases.Length; i++)
                {
                    if (canvases[i] == null || canvases[i].renderMode != RenderMode.ScreenSpaceOverlay) continue;
                    Transform found = canvases[i].transform.Find("Safe Area");
                    if (found == null) continue;
                    homeCanvas = canvases[i];
                    safeArea = found as RectTransform;
                    break;
                }
            }
            if (safeArea == null) return;

            safeArea.anchorMin = Vector2.zero;
            safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = new Vector2(18f, 10f);
            safeArea.offsetMax = new Vector2(-18f, -10f);

            // Profile/resource bars share the top edge without covering the central castle.
            Set(safeArea, "Player Profile", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(410f, 84f), new Vector2(0f, -4f), new Vector2(0f, 1f));
            Set(safeArea, "Resource Strip", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(650f, 66f), new Vector2(0f, -4f), new Vector2(1f, 1f));
            Set(safeArea, "Limited Events Rail", new Vector2(0.50f, 1f), new Vector2(0.50f, 1f), new Vector2(650f, 66f), new Vector2(0f, -76f), new Vector2(0.5f, 1f));

            // Vertical side rails keep the center of the world clear and the labels legible.
            Set(safeArea, "Left Menu", new Vector2(0f, 0.48f), new Vector2(0f, 0.48f), new Vector2(148f, 300f), new Vector2(0f, 0f), new Vector2(0f, 0.5f));
            Set(safeArea, "Right Menu", new Vector2(1f, 0.48f), new Vector2(1f, 0.48f), new Vector2(158f, 300f), new Vector2(0f, 0f), new Vector2(1f, 0.5f));

            // Bottom rewards and campaign actions sit above the chat/navigation strip.
            Set(safeArea, "Idle Rewards Card", new Vector2(0.50f, 0f), new Vector2(0.50f, 0f), new Vector2(300f, 92f), new Vector2(-158f, 80f), new Vector2(0.5f, 0f));
            Set(safeArea, "Campaign CTA", new Vector2(0.50f, 0f), new Vector2(0.50f, 0f), new Vector2(300f, 92f), new Vector2(158f, 80f), new Vector2(0.5f, 0f));
            Set(safeArea, "Bottom Navigation", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(940f, 66f), new Vector2(0f, 0f), new Vector2(1f, 0f));

            ArrangeResources();
            ArrangeRail("Limited Events Rail", false, 4);
            ArrangeRail("Left Menu", true, 4);
            ArrangeRail("Right Menu", true, 4);
            ArrangeBottomNavigation();
            ArrangeCompactCard("Idle Rewards Card", "CLAIM REWARDS Button");
            ArrangeCompactCard("Campaign CTA", "⚔  CAMPAIGN Button");
            BuildAuxiliaryHud();
        }

        private void ArrangeResources()
        {
            Transform strip = safeArea.Find("Resource Strip");
            if (strip == null) return;
            float[] edges = { 0f, 0.33f, 0.66f, 1f };
            for (int i = 0; i < strip.childCount; i++)
            {
                RectTransform item = strip.GetChild(i) as RectTransform;
                if (item == null) continue;
                int index = Mathf.Clamp(i, 0, 2);
                item.anchorMin = new Vector2(edges[index], 0f);
                item.anchorMax = new Vector2(edges[index + 1], 1f);
                item.offsetMin = new Vector2(2f, 0f);
                item.offsetMax = new Vector2(-2f, 0f);
                Text text = item.GetComponent<Text>();
                if (text != null) text.fontSize = 22;
            }
        }

        private void ArrangeRail(string name, bool verticalButtons, int expectedCount)
        {
            Transform rail = safeArea.Find(name);
            if (rail == null) return;
            VerticalLayoutGroup vertical = rail.GetComponent<VerticalLayoutGroup>();
            if (vertical != null) vertical.enabled = false;
            HorizontalLayoutGroup horizontal = rail.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null) horizontal.enabled = false;

            int count = Mathf.Max(1, rail.childCount);
            for (int i = 0; i < count; i++)
            {
                RectTransform button = rail.GetChild(i) as RectTransform;
                if (button == null) continue;
                if (verticalButtons)
                {
                    float top = 1f - (float)i / count;
                    float bottom = 1f - (float)(i + 1) / count;
                    button.anchorMin = new Vector2(0f, bottom);
                    button.anchorMax = new Vector2(1f, top);
                }
                else
                {
                    button.anchorMin = new Vector2((float)i / count, 0f);
                    button.anchorMax = new Vector2((float)(i + 1) / count, 1f);
                }
                button.offsetMin = new Vector2(3f, 3f);
                button.offsetMax = new Vector2(-3f, -3f);

                Image image = button.GetComponent<Image>();
                if (image != null) image.color = new Color(0.055f, 0.045f, 0.035f, 0.90f);
                LayoutElement element = button.GetComponent<LayoutElement>();
                if (element != null)
                {
                    element.minHeight = verticalButtons ? 64f : 58f;
                    element.preferredHeight = verticalButtons ? 64f : 58f;
                    element.minWidth = verticalButtons ? 140f : 150f;
                    element.preferredWidth = verticalButtons ? 140f : 150f;
                }
                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.fontSize = verticalButtons ? 15 : 15;
                    label.color = (i % 2 == 0) ? new Color(1f, 0.84f, 0.46f) : new Color(0.94f, 0.90f, 0.78f);
                }
            }
        }

        private void ArrangeBottomNavigation()
        {
            Transform nav = safeArea.Find("Bottom Navigation");
            if (nav == null) return;
            // Preserve the actual HomeScreen event bindings; do not rename a button to an unrelated feature.
            string[] labels = { "HOME", "HEROES", "BAG", "GUILD", "ARENA", "MORE" };
            for (int i = 0; i < nav.childCount; i++)
            {
                RectTransform button = nav.GetChild(i) as RectTransform;
                if (button == null) continue;
                button.anchorMin = new Vector2((float)i / nav.childCount, 0f);
                button.anchorMax = new Vector2((float)(i + 1) / nav.childCount, 1f);
                button.offsetMin = new Vector2(2f, 2f);
                button.offsetMax = new Vector2(-2f, -2f);
                Image image = button.GetComponent<Image>();
                if (image != null) image.color = new Color(0.025f, 0.035f, 0.042f, 0.96f);
                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    if (i < labels.Length) label.text = labels[i];
                    label.fontSize = 14;
                    label.color = i == 0 ? new Color(1f, 0.82f, 0.39f) : Color.white;
                }
            }
        }

        private void ArrangeCompactCard(string name, string buttonName)
        {
            Transform card = safeArea.Find(name);
            if (card == null) return;
            Image background = card.GetComponent<Image>();
            if (background != null) background.color = new Color(0.035f, 0.04f, 0.045f, 0.91f);

            for (int i = 0; i < card.childCount; i++)
            {
                Transform child = card.GetChild(i);
                RectTransform rect = child as RectTransform;
                if (rect == null) continue;
                if (child.name == buttonName)
                {
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.sizeDelta = new Vector2(168f, 32f);
                    rect.anchoredPosition = new Vector2(0f, 3f);
                    continue;
                }
                Text label = child.GetComponent<Text>();
                if (label == null) continue;
                label.fontSize = 15;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(6f, child.name.Contains("+0 GOLD") ? 29f : 48f);
                rect.offsetMax = new Vector2(-6f, child.name.Contains("+0 GOLD") ? -28f : -2f);
            }
        }

        private void BuildAuxiliaryHud()
        {
            if (auxiliaryHudBuilt || homeCanvas == null) return;
            auxiliaryHudBuilt = true;
            CreateHudPanel("Home Chat Panel", new Vector2(0f, 0f), new Vector2(0.31f, 0f), new Vector2(0f, 66f), Vector2.zero,
                new Color(0.025f, 0.035f, 0.045f, 0.88f), "No chat yet", 16, TextAnchor.MiddleLeft);
        }

        private void CreateHudPanel(string name, Vector2 min, Vector2 max, Vector2 size, Vector2 position, Color color, string text, int fontSize, TextAnchor anchor)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safeArea, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(min.x == max.x ? min.x : 0f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = color;
            panelImage.raycastTarget = false;

            GameObject labelObject = new GameObject("Chat Preview", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(panel.transform, false);
            Text label = labelObject.GetComponent<Text>();
            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch (System.ArgumentException) { }
            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch (System.ArgumentException) { }
            }
            label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.color = new Color(0.88f, 0.88f, 0.82f, 1f);
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12f, 6f);
            labelRect.offsetMax = new Vector2(-8f, -6f);
        }

        private static void Set(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, Vector2 pivot)
        {
            Transform child = parent.Find(name);
            RectTransform rect = child as RectTransform;
            if (rect == null) return;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
