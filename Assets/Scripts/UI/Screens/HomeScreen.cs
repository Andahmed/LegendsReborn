// Mobile-first fantasy home hub HUD. Scene art is supplied by the Home scene behind this canvas.
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using SandsOfLegends.Services;
using SandsOfLegends.Bootstrap;
using SandsOfLegends.Meta;

namespace SandsOfLegends.UI
{
    public sealed class HomeScreen : UIScreen
    {
        [Header("Scene")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform safeAreaRoot;

        [Header("Primary navigation")]
        public UnityEvent onCampaign = new UnityEvent();
        public UnityEvent onHeroes = new UnityEvent();
        public UnityEvent onFormation = new UnityEvent();
        public UnityEvent onEquipment = new UnityEvent();
        public UnityEvent onQuests = new UnityEvent();
        public UnityEvent onBackpack = new UnityEvent();
        public UnityEvent onAllies = new UnityEvent();
        public UnityEvent onMore = new UnityEvent();
        public UnityEvent onSummon = new UnityEvent();
        public UnityEvent onArena = new UnityEvent();
        public UnityEvent onShop = new UnityEvent();
        public UnityEvent onEvents = new UnityEvent();
        public UnityEvent onDailyLogin = new UnityEvent();
        public UnityEvent onBattlePass = new UnityEvent();
        public UnityEvent onMonthlyCard = new UnityEvent();
        public UnityEvent onAllianceBoss = new UnityEvent();
        public UnityEvent onClaimIdle = new UnityEvent();

        [Header("Visual palette")]
        [SerializeField] private Color deep = new Color(0.025f, 0.065f, 0.085f, 0.94f);
        [SerializeField] private Color panel = new Color(0.035f, 0.11f, 0.13f, 0.91f);
        [SerializeField] private Color turquoise = new Color(0.12f, 0.72f, 0.69f, 1f);
        [SerializeField] private Color gold = new Color(0.96f, 0.75f, 0.32f, 1f);
        [SerializeField] private Color cream = new Color(1f, 0.94f, 0.79f, 1f);

        private Text gemsText, goldText, energyText, stageText, powerText, idleText, idleAmountText;
        private Button idleClaimButton;
        private float refreshTimer;
        private bool built;

        private void Start() { Build(); RefreshData(); }
        private void Update() { if (!built) return; refreshTimer += Time.unscaledDeltaTime; if (refreshTimer >= 1f) { refreshTimer = 0f; RefreshData(); } }
        public override void OnShow() { base.OnShow(); Build(); RefreshData(); }

        private void Build()
        {
            if (built) return;
            if (FindObjectOfType<HomeWorldStage>() == null) new GameObject("Home World Castle Stage").AddComponent<HomeWorldStage>();
            if (FindObjectOfType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            if (canvas == null)
            {
                GameObject c = new GameObject("Home HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = c.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 20;
                CanvasScaler scaler = c.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.45f;
            }
            if (safeAreaRoot == null)
            {
                GameObject root = new GameObject("Safe Area", typeof(RectTransform));
                safeAreaRoot = root.GetComponent<RectTransform>();
                safeAreaRoot.SetParent(canvas.transform, false);
                safeAreaRoot.anchorMin = Vector2.zero; safeAreaRoot.anchorMax = Vector2.one;
                safeAreaRoot.offsetMin = new Vector2(22, 24); safeAreaRoot.offsetMax = new Vector2(-22, -24);
            }

            BuildProfileAndResources();
            BuildEventRail();
            BuildSideMenus();
            BuildIdleRewards();
            BuildCampaign();
            BuildBottomNavigation();
            built = true;
        }

        private void BuildProfileAndResources()
        {
            RectTransform profile = Box("Player Profile", safeAreaRoot, new Vector2(0,1), new Vector2(0.43f,1), new Vector2(0,116), new Vector2(0,-4), panel);
            AddText("PLAYER  •  LEVEL 1", profile, 25, cream, TextAnchor.MiddleLeft, new Vector2(18,0), new Vector2(-12,0));
            powerText = AddText("POWER  1,250", profile, 20, gold, TextAnchor.LowerLeft, new Vector2(18,8), new Vector2(-12,0));

            RectTransform resources = Box("Resource Strip", safeAreaRoot, new Vector2(0.45f,1), new Vector2(1,1), new Vector2(0,116), new Vector2(0,-4), deep);
            gemsText = AddText("◆  0", resources, 23, cream, TextAnchor.MiddleCenter, new Vector2(0,0), new Vector2(0,0));
            gemsText.rectTransform.anchorMin = new Vector2(0,0); gemsText.rectTransform.anchorMax = new Vector2(0.34f,1);
            goldText = AddText("●  0", resources, 23, gold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            goldText.rectTransform.anchorMin = new Vector2(0.34f,0); goldText.rectTransform.anchorMax = new Vector2(0.68f,1);
            energyText = AddText("⚡  0/100", resources, 22, turquoise, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            energyText.rectTransform.anchorMin = new Vector2(0.68f,0); energyText.rectTransform.anchorMax = new Vector2(1,1);
        }

        private void BuildEventRail()
        {
            RectTransform rail = Box("Limited Events Rail", safeAreaRoot, new Vector2(0,0.69f), new Vector2(0.22f,0.69f), new Vector2(0,420), new Vector2(0,0), Color.clear);
            AddNav(rail,"EVENTS",onEvents, gold, 68);
            AddNav(rail,"DAILY LOGIN",onDailyLogin,turquoise,68);
            AddNav(rail,"BATTLE PASS",onBattlePass,gold,68);
            AddNav(rail,"MONTHLY CARD",onMonthlyCard,turquoise,68);
        }

        private void BuildSideMenus()
        {
            RectTransform left = Box("Left Menu", safeAreaRoot, new Vector2(0,0.34f), new Vector2(0,0.34f), new Vector2(190,360), Vector2.zero, Color.clear);
            left.pivot = new Vector2(0f,0.5f);
            AddNav(left,"HEROES",onHeroes,turquoise,72);
            AddNav(left,"FORMATION",onFormation,gold,72);
            AddNav(left,"QUESTS",onQuests,turquoise,72);
            AddNav(left,"EQUIPMENT",onEquipment,gold,72);

            RectTransform right = Box("Right Menu", safeAreaRoot, new Vector2(1,0.34f), new Vector2(1,0.34f), new Vector2(190,360), Vector2.zero, Color.clear);
            right.pivot = new Vector2(1f,0.5f);
            AddNav(right,"SUMMON",onSummon,gold,72);
            AddNav(right,"ARENA",onArena,turquoise,72);
            AddNav(right,"ALLIANCE BOSS",onAllianceBoss,gold,72);
            AddNav(right,"SHOP",onShop,turquoise,72);
        }

        private void BuildIdleRewards()
        {
            RectTransform idle = Box("Idle Rewards Card", safeAreaRoot, new Vector2(0.24f,0.20f), new Vector2(0.76f,0.20f), new Vector2(0,142), Vector2.zero, panel);
            idleText = AddText("IDLE REWARDS  •  00:00:00", idle, 20, cream, TextAnchor.MiddleCenter, new Vector2(8,62), new Vector2(-8,-2));
            idleAmountText = AddText("+0 GOLD  •  +0 XP", idle, 17, turquoise, TextAnchor.MiddleCenter, new Vector2(8,36), new Vector2(-8,-26));
            idleClaimButton = MakeButton("CLAIM REWARDS", idle, gold, 210, 48);
            idleClaimButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f,0);
            idleClaimButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f,0);
            idleClaimButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,9);
            idleClaimButton.onClick.AddListener(ClaimIdleRewards);
        }

        private void BuildCampaign()
        {
            RectTransform area = Box("Campaign CTA", safeAreaRoot, new Vector2(0.24f,0.105f), new Vector2(0.76f,0.105f), new Vector2(0,128), Vector2.zero, deep);
            stageText = AddText("THE SEVEN SANDS  •  STAGE 1", area, 22, cream, TextAnchor.MiddleCenter, new Vector2(8,54), new Vector2(-8,0));
            Button enter = MakeButton("⚔  CAMPAIGN", area, gold, 0, 68);
            RectTransform er = enter.GetComponent<RectTransform>();
            er.anchorMin = new Vector2(0.08f,0); er.anchorMax = new Vector2(0.92f,0);
            er.anchoredPosition = new Vector2(0,12); er.sizeDelta = new Vector2(0,66);
            enter.onClick.AddListener(() => onCampaign.Invoke());
        }

        private void BuildBottomNavigation()
        {
            RectTransform nav = Box("Bottom Navigation", safeAreaRoot, new Vector2(0,0), new Vector2(1,0), new Vector2(0,116), new Vector2(0,0), deep);
            string[] labels = { "HOME", "HEROES", "BAG", "GUILD", "ARENA", "MORE" };
            UnityEvent[] actions = { null, onHeroes, onBackpack, onAllies, onArena, onMore };
            for (int i=0;i<labels.Length;i++)
            {
                int index=i;
                Button b = MakeButton(labels[i],nav,i==0?gold:turquoise,0,82);
                RectTransform r=b.GetComponent<RectTransform>();
                r.anchorMin=new Vector2(i/6f,0.12f); r.anchorMax=new Vector2((i+1)/6f,0.88f);
                r.offsetMin=new Vector2(4,0); r.offsetMax=new Vector2(-4,0);
                if (actions[index]!=null) b.onClick.AddListener(()=>actions[index].Invoke());
                else b.interactable=false;
            }
        }

        private void AddNav(RectTransform parent,string label,UnityEvent action,Color accent,float height)
        {
            VerticalLayoutGroup layout=parent.GetComponent<VerticalLayoutGroup>();
            if(layout==null)
            {
                layout=parent.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing=9;
                layout.padding=new RectOffset(2,2,2,2);
                layout.childAlignment=TextAnchor.MiddleCenter;
                layout.childControlWidth=true;
                layout.childControlHeight=true;
                layout.childForceExpandWidth=true;
                layout.childForceExpandHeight=false;
            }
            Button b=MakeButton(label,parent,accent,0,height);
            b.onClick.AddListener(()=>action.Invoke());
        }

        private RectTransform Box(string name,Transform parent,Vector2 min,Vector2 max,Vector2 size,Vector2 pos,Color color)
        {
            GameObject go=new GameObject(name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(parent,false);
            RectTransform r=go.GetComponent<RectTransform>();
            r.anchorMin=min; r.anchorMax=max; r.sizeDelta=size; r.anchoredPosition=pos;
            Image image=go.GetComponent<Image>(); image.color=color; image.raycastTarget=color.a>0.01f;
            return r;
        }

        private Text AddText(string value,Transform parent,int size,Color color,TextAnchor align,Vector2 minOffset,Vector2 maxOffset)
        {
            GameObject go=new GameObject(value+" Label",typeof(RectTransform),typeof(Text));
            go.transform.SetParent(parent,false);
            Text t=go.GetComponent<Text>();
            Font font=null;
            try { font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch (ArgumentException) { }
            if (font==null)
            {
                try { font=Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch (ArgumentException) { }
            }
            t.font=font; t.text=value; t.fontSize=size;
            t.color=color; t.alignment=align; t.fontStyle=FontStyle.Bold;
            t.horizontalOverflow=HorizontalWrapMode.Overflow; t.verticalOverflow=VerticalWrapMode.Overflow;
            t.raycastTarget=false;
            RectTransform r=t.rectTransform; r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one;
            r.offsetMin=minOffset; r.offsetMax=maxOffset;
            return t;
        }

        private Button MakeButton(string label,Transform parent,Color accent,float width,float height)
        {
            GameObject go=new GameObject(label+" Button",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            go.transform.SetParent(parent,false);
            Image image=go.GetComponent<Image>(); image.color=new Color(deep.r,deep.g,deep.b,0.97f);
            Button button=go.GetComponent<Button>();
            ColorBlock cb=button.colors; cb.normalColor=image.color;
            cb.highlightedColor=new Color(accent.r,accent.g,accent.b,0.34f);
            cb.pressedColor=new Color(accent.r,accent.g,accent.b,0.58f); button.colors=cb;
            LayoutElement le=go.GetComponent<LayoutElement>(); le.minHeight=height; le.preferredHeight=height;
            if(width>0) le.minWidth=width;
            Text labelText=AddText(label,go.transform,19,accent,TextAnchor.MiddleCenter,new Vector2(5,3),new Vector2(-5,-3));
            return button;
        }

        private void ClaimIdleRewards() { if (GameBootstrap.Instance == null || GameBootstrap.Instance.State == null) return; GameBootstrap.Instance.State.ClaimIdle(TimeService.NowUnix); GameBootstrap.Instance.SaveNow(); RefreshData(); onClaimIdle.Invoke(); }

        private void RefreshData()
        {
            if (GameBootstrap.Instance==null || GameBootstrap.Instance.State==null) return;
            GameState state=GameBootstrap.Instance.State;
            if(gemsText!=null) gemsText.text="◆  "+state.Wallet.Get(Currency.Gems).ToString("N0");
            if(goldText!=null) goldText.text="●  "+state.Wallet.Get(Currency.Gold).ToString("N0");
            if(energyText!=null) energyText.text="⚡  "+state.Energy.Value+"/"+EnergyState.Cap;
            if(stageText!=null) stageText.text="THE SEVEN SANDS  •  STAGE "+Mathf.Max(1,state.Campaign.HighestCleared+1);
            if (idleText != null) { IdleRewards.Reward reward = IdleRewards.Calculate(state.IdleLastClaimUnix, TimeService.NowUnix, state.Campaign.HighestCleared); TimeSpan elapsed = TimeSpan.FromSeconds(reward.Seconds); idleText.text = "IDLE REWARDS  •  " + ((int)elapsed.TotalHours).ToString("00") + ":" + elapsed.Minutes.ToString("00") + ":" + elapsed.Seconds.ToString("00"); idleAmountText.text = "+" + reward.Gold.ToString("N0") + " GOLD  •  +" + reward.Exp.ToString("N0") + " XP"; if (idleClaimButton != null) idleClaimButton.interactable = reward.Seconds >= 60; }
        }
    }
}
