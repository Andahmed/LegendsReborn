// =============================================================================
// FILE: Assets/Scripts/UI/Screens/HomeScreen.cs
// OWNER ROLE: Client Programmer - UI
// RESPONSIBILITY: Mobile-first main hub HUD and navigation over the 3D home-world scene.
// WORK ON: Bind navigation events to unlocked screens; connect live server-backed player data.
// STATUS: Foundation - runtime UI scaffold
// =============================================================================
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SandsOfLegends.Bootstrap;
using SandsOfLegends.Meta;

namespace SandsOfLegends.UI
{
    public sealed class HomeScreen : UIScreen
    {
        [Header("Scene")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform safeAreaRoot;

        [Header("Navigation Events")]
        public UnityEvent onCampaign;
        public UnityEvent onHeroes;
        public UnityEvent onSummon;
        public UnityEvent onQuests;
        public UnityEvent onShop;
        public UnityEvent onFormation;

        [Header("Palette")]
        [SerializeField] private Color panelColor = new Color(0.035f, 0.075f, 0.09f, 0.88f);
        [SerializeField] private Color accentColor = new Color(0.12f, 0.72f, 0.70f, 1f);
        [SerializeField] private Color goldColor = new Color(0.92f, 0.70f, 0.30f, 1f);

        private Text _gems;
        private Text _gold;
        private Text _energy;
        private Text _stage;
        private bool _built;

        private void Start()
        {
            BuildIfNeeded();
            RefreshPlayerData();
        }

        public override void OnShow()
        {
            base.OnShow();
            BuildIfNeeded();
            RefreshPlayerData();
        }

        private void BuildIfNeeded()
        {
            if (_built) return;
            if (canvas == null)
            {
                var canvasObject = new GameObject("Home HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 20;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (safeAreaRoot == null)
            {
                var rootObject = new GameObject("Safe Area", typeof(RectTransform));
                safeAreaRoot = rootObject.GetComponent<RectTransform>();
                safeAreaRoot.SetParent(canvas.transform, false);
                safeAreaRoot.anchorMin = Vector2.zero;
                safeAreaRoot.anchorMax = Vector2.one;
                safeAreaRoot.offsetMin = new Vector2(28, 28);
                safeAreaRoot.offsetMax = new Vector2(-28, -28);
            }

            BuildTopBar();
            BuildSideNavigation();
            BuildCampaignButton();
            BuildWorldTitle();
            _built = true;
        }

        private void BuildTopBar()
        {
            var bar = Panel("Top Resource Bar", safeAreaRoot, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 112), Vector2.zero, panelColor);
            var layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 10, 10);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            _gems = ResourceLabel(bar, "GEMS", goldColor);
            _gold = ResourceLabel(bar, "GOLD", goldColor);
            _energy = ResourceLabel(bar, "ENERGY", accentColor);
        }

        private Text ResourceLabel(Transform parent, string label, Color color)
        {
            var text = MakeText(label + "  --", parent, 24, color, TextAnchor.MiddleCenter);
            text.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            return text;
        }

        private void BuildSideNavigation()
        {
            var left = Panel("Left Navigation", safeAreaRoot, new Vector2(0, 0.34f), new Vector2(0, 0.34f), new Vector2(0, 390), Vector2.zero, Color.clear);
            var leftLayout = left.gameObject.AddComponent<VerticalLayoutGroup>();
            leftLayout.spacing = 14;
            leftLayout.childControlWidth = true;
            leftLayout.childControlHeight = true;
            leftLayout.childForceExpandHeight = false;
            leftLayout.childForceExpandWidth = true;
            AddNavButton(left, "HEROES", onHeroes);
            AddNavButton(left, "FORMATION", onFormation);
            AddNavButton(left, "QUESTS", onQuests);

            var right = Panel("Right Navigation", safeAreaRoot, new Vector2(1, 0.34f), new Vector2(1, 0.34f), new Vector2(0, 270), Vector2.zero, Color.clear);
            var rightLayout = right.gameObject.AddComponent<VerticalLayoutGroup>();
            rightLayout.spacing = 14;
            rightLayout.childControlWidth = true;
            rightLayout.childControlHeight = true;
            rightLayout.childForceExpandHeight = false;
            rightLayout.childForceExpandWidth = true;
            AddNavButton(right, "SUMMON", onSummon);
            AddNavButton(right, "SHOP", onShop);
        }

        private void AddNavButton(Transform parent, string label, UnityEvent action)
        {
            var button = MakeButton(label, parent, accentColor, 190, 76);
            button.onClick.AddListener(() => { if (action != null) action.Invoke(); });
        }

        private void BuildCampaignButton()
        {
            var area = Panel("Campaign Action Area", safeAreaRoot, new Vector2(0.08f, 0), new Vector2(0.92f, 0), new Vector2(0, 174), Vector2.zero, panelColor);
            var vertical = area.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(18, 18, 12, 12);
            vertical.spacing = 4;
            vertical.childAlignment = TextAnchor.MiddleCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;
            vertical.childForceExpandHeight = true;
            _stage = MakeText("CAMPAIGN  •  STAGE 1", area, 24, goldColor, TextAnchor.MiddleCenter);
            var enter = MakeButton("ENTER CAMPAIGN", area, goldColor, 0, 82);
            enter.GetComponent<LayoutElement>().flexibleWidth = 1;
            enter.onClick.AddListener(() => { if (onCampaign != null) onCampaign.Invoke(); });
        }

        private void BuildWorldTitle()
        {
            var title = MakeText("THE SEVEN SANDS", safeAreaRoot, 34, Color.white, TextAnchor.MiddleCenter);
            var rect = title.rectTransform;
            rect.anchorMin = new Vector2(0.16f, 0.68f);
            rect.anchorMax = new Vector2(0.84f, 0.76f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            title.fontStyle = FontStyle.Bold;
            title.characterSpacing = 3;
        }

        private void RefreshPlayerData()
        {
            if (_gems == null || GameBootstrap.Instance == null || GameBootstrap.Instance.State == null) return;
            GameState state = GameBootstrap.Instance.State;
            _gems.text = "GEMS  " + state.Wallet.Get(Currency.Gems).ToString("N0");
            _gold.text = "GOLD  " + state.Wallet.Get(Currency.Gold).ToString("N0");
            _energy.text = "ENERGY  " + state.Energy.Value + "/" + EnergyState.Cap;
            if (_stage != null) _stage.text = "CAMPAIGN  •  STAGE " + Mathf.Max(1, state.Campaign.HighestCleared + 1);
        }

        private RectTransform Panel(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0.01f;
            return rect;
        }

        private Text MakeText(string value, Transform parent, int size, Color color, TextAnchor alignment)
        {
            var go = new GameObject(value + " Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private Button MakeButton(string label, Transform parent, Color color, float width, float height)
        {
            var go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0.96f);
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(color.r, color.g, color.b, 0.22f);
            colors.pressedColor = new Color(color.r, color.g, color.b, 0.42f);
            button.colors = colors;
            var layout = go.GetComponent<LayoutElement>();
            layout.minWidth = width;
            layout.minHeight = height;
            layout.preferredHeight = height;
            var labelText = MakeText(label, go.transform, 22, color, TextAnchor.MiddleCenter);
            var rect = labelText.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8, 4);
            rect.offsetMax = new Vector2(-8, -4);
            return button;
        }
    }
}
