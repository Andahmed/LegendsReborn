using UnityEngine;
using UnityEngine.UI;

namespace SandsOfLegends.UI
{
    /// <summary>
    /// Landscape-only home game HUD: keeps the whole 3D world visible, with player/resources above,
    /// feature shortcuts at the sides, and chat/navigation along the bottom edge.
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
            ApplyCamera();
            ArrangeHud();
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

            CanvasScaler scaler = homeCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            safeArea.anchorMin = Vector2.zero;
            safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = new Vector2(18f, 12f);
            safeArea.offsetMax = new Vector2(-18f, -12f);

            Set(safeArea, "Player Profile", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(370f, 88f), new Vector2(0f, -4f), new Vector2(0f, 1f));
            Set(safeArea, "Resource Strip", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(620f, 76f), new Vector2(0f, -4f), new Vector2(1f, 1f));
            Set(safeArea, "Limited Events Rail", new Vector2(0.50f, 1f), new Vector2(0.50f, 1f), new Vector2(650f, 76f), new Vector2(0f, -86f), new Vector2(0.5f, 1f));
            Set(safeArea, "Left Menu", new Vector2(0f, 0.48f), new Vector2(0f, 0.48f), new Vector2(138f, 300f), new Vector2(0f, 0f), new Vector2(0f, 0.5f));
            Set(safeArea, "Right Menu", new Vector2(1f, 0.48f), new Vector2(1f, 0.48f), new Vector2(150f, 300f), new Vector2(0f, 0f), new Vector2(1f, 0.5f));
            Set(safeArea, "Idle Rewards Card", new Vector2(0.50f, 0f), new Vector2(0.50f, 0f), new Vector2(300f, 94f), new Vector2(-170f, 90f), new Vector2(0.5f, 0f));
            Set(safeArea, "Campaign CTA", new Vector2(0.50f, 0f), new Vector2(0.50f, 0f), new Vector2(300f, 94f), new Vector2(170f, 90f), new Vector2(0.5f, 0f));
            Set(safeArea, "Bottom Navigation", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(930f, 70f), new Vector2(0f, 0f), new Vector2(1f, 0f));

            ArrangeResources();
            ArrangeRail("Limited Events Rail", 4, 144f, 64f);
            ArrangeRail("Left Menu", 4, 130f, 62f);
            ArrangeRail("Right Menu", 4, 140f, 62f);
            ArrangeBottomNavigation();
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
            }
        }

        private void ArrangeRail(string name, int count, float width, float height)
        {
            Transform rail = safeArea.Find(name);
            if (rail == null) return;
            VerticalLayoutGroup vertical = rail.GetComponent<VerticalLayoutGroup>();
            if (vertical != null) vertical.enabled = false;
            HorizontalLayoutGroup horizontal = rail.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null) horizontal.enabled = false;
            for (int i = 0; i < rail.childCount; i++)
            {
                RectTransform button = rail.GetChild(i) as RectTransform;
                if (button == null) continue;
                button.anchorMin = new Vector2((float)i / count, 0f);
                button.anchorMax = new Vector2((float)(i + 1) / count, 1f);
                button.offsetMin = new Vector2(3f, 3f);
                button.offsetMax = new Vector2(-3f, -3f);
                LayoutElement element = button.GetComponent<LayoutElement>();
                if (element != null)
                {
                    element.minHeight = height;
                    element.preferredHeight = height;
                    element.minWidth = width / count;
                    element.preferredWidth = width / count;
                }
                Text label = button.GetComponentInChildren<Text>();
                if (label != null) label.fontSize = name == "Limited Events Rail" ? 16 : 15;
            }
        }

        private void ArrangeBottomNavigation()
        {
            Transform nav = safeArea.Find("Bottom Navigation");
            if (nav == null) return;
            string[] labels = { "MISSIONS", "CHAT", "TECH", "SHOP", "BOUNTY", "FORGE", "HEROES", "BAG" };
            for (int i = 0; i < nav.childCount; i++)
            {
                RectTransform button = nav.GetChild(i) as RectTransform;
                if (button == null) continue;
                button.anchorMin = new Vector2((float)i / nav.childCount, 0f);
                button.anchorMax = new Vector2((float)(i + 1) / nav.childCount, 1f);
                button.offsetMin = new Vector2(2f, 2f);
                button.offsetMax = new Vector2(-2f, -2f);
                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    if (i < labels.Length) label.text = labels[i];
                    label.fontSize = 14;
                }
            }
        }

        private void BuildAuxiliaryHud()
        {
            if (auxiliaryHudBuilt || homeCanvas == null) return;
            auxiliaryHudBuilt = true;
            CreateHudPanel("Home Chat Panel", new Vector2(0f, 0f), new Vector2(0.31f, 0f), new Vector2(0f, 70f), new Vector2(0f, 0f), new Color(0.025f, 0.035f, 0.045f, 0.82f),
                "[K581] Alliance: Welcome to the kingdom\n[K580] Your heroes are ready for battle", 16, TextAnchor.MiddleLeft);
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
            panel.GetComponent<Image>().color = color;
            panel.GetComponent<Image>().raycastTarget = false;

            GameObject labelObject = new GameObject("Chat Preview", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(panel.transform, false);
            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.text = text;
            label.fontSize = fontSize;
            label.color = new Color(0.82f, 0.9f, 0.79f, 1f);
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
