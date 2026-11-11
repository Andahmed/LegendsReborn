using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SandsOfLegends.Bootstrap;
using SandsOfLegends.Combat;
using SandsOfLegends.Core;
using SandsOfLegends.Meta;
using SandsOfLegends.Presentation;
using SandsOfLegends.Services;

namespace SandsOfLegends.UI
{
    /// <summary>
    /// Runtime navigation and MVP game loop. It is attached automatically by GameBootstrap,
    /// so the generated Home scene does not require manual UnityEvent wiring.
    /// </summary>
    public sealed class GameFlowController : MonoBehaviour
    {
        private HomeScreen home;
        private Canvas overlayCanvas;
        private RectTransform overlayRoot;
        private RectTransform panel;
        private RectTransform contentRoot;
        private Text battleStatus;
        private Text battleLog;
        private Text battleSummary;
        private BattleSim battle;
        private BattleView battleView;
        private BattleHud battleHud;
        private GameObject battleWorld;
        private HomeWorldStage homeWorldStage;
        private DamageNumbers damageNumbers;
        private readonly Dictionary<string, HeroView> actors = new Dictionary<string, HeroView>();
        private readonly Dictionary<string, Sprite> portraitCache = new Dictionary<string, Sprite>();
        private int activeStage;
        private bool bound;
        private bool battleResolved;

        private readonly Color ink = new Color(.025f, .065f, .085f, .97f);
        private readonly Color panelColor = new Color(.035f, .11f, .13f, .98f);
        private readonly Color teal = new Color(.12f, .72f, .69f, 1f);
        private readonly Color gold = new Color(.96f, .75f, .32f, 1f);
        private readonly Color cream = new Color(1f, .94f, .79f, 1f);

        public void Bind(HomeScreen target)
        {
            if (bound || target == null) return;
            home = target;
            bound = true;
            CreateOverlay();
            home.onCampaign.AddListener(OpenCampaign);
            home.onHeroes.AddListener(OpenHeroes);
            home.onFormation.AddListener(OpenFormation);
            home.onEquipment.AddListener(OpenEquipment);
            home.onQuests.AddListener(OpenQuests);
            home.onBackpack.AddListener(OpenBackpack);
            home.onAllies.AddListener(OpenAllies);
            home.onMore.AddListener(OpenMore);
            home.onSummon.AddListener(OpenSummon);
            home.onArena.AddListener(OpenArena);
            home.onShop.AddListener(OpenShop);
            home.onEvents.AddListener(OpenEvents);
            home.onDailyLogin.AddListener(OpenDailyLogin);
            home.onBattlePass.AddListener(OpenBattlePass);
            home.onMonthlyCard.AddListener(OpenMonthlyCard);
            home.onAllianceBoss.AddListener(OpenAllianceBoss);
            home.onClaimIdle.AddListener(RefreshHome);
        }

        private void CreateOverlay()
        {
            GameObject canvasObject = new GameObject("Game Flow Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            overlayCanvas = canvasObject.GetComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 80;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = .45f;

            GameObject root = new GameObject("Overlay Root", typeof(RectTransform));
            root.transform.SetParent(canvasObject.transform, false);
            overlayRoot = root.GetComponent<RectTransform>();
            overlayRoot.anchorMin = Vector2.zero;
            overlayRoot.anchorMax = Vector2.one;
            overlayRoot.offsetMin = Vector2.zero;
            overlayRoot.offsetMax = Vector2.zero;
        }

        private GameState State
        {
            get { return GameBootstrap.Instance != null ? GameBootstrap.Instance.State : null; }
        }

        private void CreatePanel(string title, string subtitle, bool withBack)
        {
            contentRoot = null;
            if (panel != null && panel.parent != null) Destroy(panel.parent.gameObject);
            GameObject backdrop = new GameObject("Screen Backdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(overlayRoot, false);
            RectTransform backdropRect = backdrop.GetComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            backdrop.GetComponent<Image>().color = new Color(.008f, .025f, .032f, .84f);

            GameObject panelObject = new GameObject("Screen Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            panelObject.transform.SetParent(backdrop.transform, false);
            panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(.045f, .055f);
            panel.anchorMax = new Vector2(.955f, .945f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            panelObject.GetComponent<Image>().color = panelColor;
            VerticalLayoutGroup layout = panelObject.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(26, 26, 28, 24);
            layout.spacing = 13;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddText(panel, title, 40, gold, 84);
            AddText(panel, subtitle, 21, cream, 76);
            AddText(panel, " ", 8, cream, 8);
            if (withBack) AddButton(panel, "RETURN TO CITY", ReturnToCity, 66, teal);

            GameObject viewportObject = new GameObject("Scrollable Content Viewport",
                typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect), typeof(LayoutElement));
            viewportObject.transform.SetParent(panel, false);
            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, .04f);
            viewportImage.raycastTarget = true;
            LayoutElement viewportLayout = viewportObject.GetComponent<LayoutElement>();
            viewportLayout.minHeight = 240f;
            viewportLayout.flexibleHeight = 1f;

            GameObject contentObject = new GameObject("Scrollable Content",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportObject.transform, false);
            contentRoot = contentObject.GetComponent<RectTransform>();
            contentRoot.anchorMin = new Vector2(0f, 1f);
            contentRoot.anchorMax = new Vector2(1f, 1f);
            contentRoot.pivot = new Vector2(.5f, 1f);
            contentRoot.anchoredPosition = Vector2.zero;
            contentRoot.sizeDelta = Vector2.zero;
            VerticalLayoutGroup contentLayout = contentObject.GetComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(4, 4, 8, 20);
            contentLayout.spacing = 10f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = viewportObject.GetComponent<ScrollRect>();
            scroll.content = contentRoot;
            scroll.viewport = viewportRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.inertia = true;
            scroll.scrollSensitivity = 36f;
        }

        private Text AddText(Transform parent, string value, int size, Color color, float height)
        {
            GameObject go = new GameObject("Text - " + value, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            Transform actualParent = parent == panel && contentRoot != null ? contentRoot : parent;
            go.transform.SetParent(actualParent, false);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            LayoutElement element = go.GetComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
            return text;
        }

        private Button AddButton(Transform parent, string label, Action action, float height, Color accent)
        {
            GameObject go = new GameObject("Button - " + label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            Transform actualParent = parent == panel && contentRoot != null ? contentRoot : parent;
            go.transform.SetParent(actualParent, false);
            Image image = go.GetComponent<Image>();
            image.color = ink;
            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = ink;
            colors.highlightedColor = new Color(accent.r, accent.g, accent.b, .32f);
            colors.pressedColor = new Color(accent.r, accent.g, accent.b, .58f);
            button.colors = colors;
            LayoutElement element = go.GetComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
            Text text = AddText(go.transform, label, 21, accent, height - 8);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 2);
            textRect.offsetMax = new Vector2(-10, -2);
            if (action != null) button.onClick.AddListener(() => action());
            return button;
        }

        private void AddInfo(string text, float height = 70f)
        {
            if (panel != null) AddText(panel, text, 20, cream, height);
        }

        private void OpenCampaign()
        {
            if (State == null) return;
            ReturnToMenuWorld();
            int stage = Mathf.Min(CampaignProgress.MaxStage, State.Campaign.HighestCleared + 1);
            CampaignStageData data = CampaignContent.Get(stage);
            CreatePanel("CAMPAIGN", data.Biome + "  •  " + CampaignProgress.MaxStage + " stages across five biomes", true);
            AddInfo("NEXT: STAGE " + stage + (data.IsBoss ? "  •  BOSS ENCOUNTER" : "") +
                    "\nRECOMMENDED POWER  " + data.RecommendedPower + "\nENERGY COST  " + data.EnergyCost, 190);
            AddInfo("PROGRESS  " + State.Campaign.HighestCleared + " / " + CampaignProgress.MaxStage +
                    "\nCURRENT TEAM  " + State.Roster.Heroes.Count + " / 20 heroes", 100);
            for (int i = 0; i < CampaignContent.Biomes.Length; i++)
            {
                int first = i * CampaignProgress.StagesPerBiome + 1;
                int last = first + CampaignProgress.StagesPerBiome - 1;
                int cleared = Mathf.Clamp(State.Campaign.HighestCleared - first + 1, 0, CampaignProgress.StagesPerBiome);
                AddInfo(CampaignContent.Biomes[i] + "  •  " + cleared + " / 40  •  Stages " + first + "-" + last, 48);
            }
            AddButton(panel, "START STAGE " + stage, StartCampaignBattle, 78, gold);
        }

        private void OpenHeroes()
        {
            if (State == null) return;
            ReturnToMenuWorld();
            CreatePanel("HEROES", "Your roster  •  Spirit Source ceiling " + State.Roster.SpiritCeiling, true);
            AddInfo("Owned heroes: " + State.Roster.Heroes.Count + " / 20   •   Formation: 5 slots", 64);
            for (int i = 0; i < State.Roster.Heroes.Count; i++)
            {
                HeroInstance instance = State.Roster.Heroes[i];
                HeroCardData card = HeroCatalog.Get(instance.HeroId);
                if (card == null) continue;
                string id = instance.HeroId;
                int nextStarCost = instance.Stars < Progression.MaxStars
                    ? Progression.ShardsForNextStar(instance.Stars) : 0;
                string line = card.DisplayName + "  •  " + HeroCatalog.RarityName(card.Rarity) +
                    "\nLV " + instance.Level + "  •  " + card.Role + "  •  " + card.Faction +
                    "\nSTARS " + instance.Stars + "/5  •  ASCENSION " + instance.Ascension + "/4" +
                    "  •  " + instance.Shards + " shards";
                AddHeroPortrait(id);
                AddButton(panel, line + "\nTAP TO LEVEL UP", () => LevelUpHero(id), 116, teal);
                if (instance.Stars < Progression.MaxStars)
                    AddButton(panel, "STAR UP  •  " + instance.Shards + " / " + nextStarCost + " SHARDS",
                        () => StarUpHero(id), 58, gold);
                if (instance.Ascension < Progression.MaxAscension)
                {
                    int requiredStars = instance.Ascension + 1;
                    AddButton(panel, "ASCEND  •  NEED " + requiredStars + " STARS",
                        () => AscendHero(id), 58, teal);
                }
            }
        }

        private void AddHeroPortrait(string heroId)
        {
            if (contentRoot == null || string.IsNullOrEmpty(heroId)) return;
            Sprite sprite;
            if (!portraitCache.TryGetValue(heroId, out sprite))
            {
                Texture2D texture = Resources.Load<Texture2D>("HeroPortraits/hero_" + heroId);
                if (texture == null) return;
                sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
                portraitCache[heroId] = sprite;
            }

            GameObject go = new GameObject("Hero Portrait - " + heroId,
                typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(contentRoot, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            LayoutElement layout = go.GetComponent<LayoutElement>();
            layout.minHeight = 210f;
            layout.preferredHeight = 210f;
            layout.flexibleWidth = 1f;
        }

        private void LevelUpHero(string heroId)
        {
            if (State == null) return;
            if (HeroesScreen.TryLevelUp(State, heroId))
            {
                Save();
                OpenHeroes();
            }
            else
            {
                AddInfo("Not enough Gold or Experience, or this hero reached the current level cap.", 68);
            }
        }

        private void StarUpHero(string heroId)
        {
            if (State == null) return;
            if (HeroesScreen.TryStarUp(State, heroId))
            {
                Save();
                OpenHeroes();
            }
            else AddInfo("Not enough hero shards, or this hero already has five stars.", 68);
        }

        private void AscendHero(string heroId)
        {
            if (State == null) return;
            if (HeroesScreen.TryAscend(State, heroId))
            {
                Save();
                OpenHeroes();
            }
            else AddInfo("Ascension needs the required star level, Gold and Gear Materials.", 68);
        }

        private void OpenFormation()
        {
            if (State == null) return;
            ReturnToMenuWorld();
            CreatePanel("FORMATION", "Select five owned heroes. Slots 1-2 are front row; 3-5 are back row.", true);
            string[] formation = State.Roster.GetFormation();
            for (int slot = 0; slot < 5; slot++)
            {
                int index = slot;
                string current = formation[slot];
                AddInfo((slot < 2 ? "FRONT " : "BACK ") + (slot + 1) + "  •  " +
                    (HeroCatalog.Get(current) != null ? HeroCatalog.Get(current).DisplayName : "Empty"), 52);
                AddButton(panel, "CYCLE SLOT " + (slot + 1), () => CycleFormation(index), 56, teal);
            }
        }

        private void CycleFormation(int slot)
        {
            if (State == null || State.Roster.Heroes.Count == 0) return;
            string[] formation = State.Roster.GetFormation();
            string current = formation[slot];
            int start = 0;
            for (int i = 0; i < State.Roster.Heroes.Count; i++)
                if (State.Roster.Heroes[i].HeroId == current) start = i + 1;
            string next = State.Roster.Heroes[start % State.Roster.Heroes.Count].HeroId;
            HeroesScreen.SetFormation(State, slot, next);
            Save();
            OpenFormation();
        }

        private void OpenSummon()
        {
            if (State == null) return;
            ReturnToMenuWorld();
            CreatePanel("SUMMON ALTAR", "A 20-hero pool • client prototype only; production pulls must use server RNG.", true);
            AddInfo("GEMS  " + State.Wallet.Get(Currency.Gems).ToString("N0") +
                "\nCOMMON 40%   RARE 35%   EPIC 20%   LEGENDARY 5%\nEPIC PITY 30  •  LEGENDARY PITY 90", 164);
            AddButton(panel, "SUMMON x1  •  300 GEMS", () => PullHeroes(1), 72, teal);
            AddButton(panel, "SUMMON x10  •  2,700 GEMS", () => PullHeroes(10), 72, gold);
        }

        private void PullHeroes(int amount)
        {
            if (State == null) return;
            // SummonScreen.TryPull owns the single gem debit. Do not pre-spend here.
            PullResult[] pulls;
            if (!SummonScreen.TryPull(State, amount,
                Environment.TickCount ^ (int)(Time.realtimeSinceStartup * 7919f), out pulls))
            {
                AddInfo("Not enough Gems.", 64);
                return;
            }
            List<string> results = new List<string>();
            for (int i = 0; i < pulls.Length; i++)
            {
                PullResult result = pulls[i];
                HeroCardData card = HeroCatalog.Get(result.HeroId);
                results.Add((card != null ? card.DisplayName : result.HeroId) + "  •  " +
                    HeroCatalog.RarityName(result.Rarity) + (result.IsNew ? "  •  NEW" : "  •  +" + result.ShardsGained + " SHARDS"));
            }
            Save();
            CreatePanel("SUMMON RESULTS", "Pulls complete. Pity counters have been saved.", true);
            AddInfo(string.Join("\n", results.ToArray()), Mathf.Min(520, 56f * results.Count + 36f));
            AddInfo("EPIC PITY  " + State.Pity.SinceEpic + " / " + GachaSystem.EpicPity +
                "     LEGENDARY PITY  " + State.Pity.SinceLegendary + " / " + GachaSystem.LegendaryPity, 70);
            AddButton(panel, "OPEN HERO ROSTER", OpenHeroes, 70, teal);
        }

        private void OpenQuests()
        {
            if (State == null) return;
            ReturnToMenuWorld();
            CreatePanel("DAILY QUESTS", "Complete objectives to earn Gems. Resets daily.", true);
            for (int i = 0; i < DailyQuests.Ids.Length; i++)
            {
                int index = i;
                string label = DailyQuests.Ids[i].Replace("_", " ").ToUpperInvariant();
                bool claimed = State.Quests.Claimed[i];
                bool ready = State.Quests.Progress[i] >= DailyQuests.Targets[i];
                string progress = State.Quests.Progress[i] + " / " + DailyQuests.Targets[i];
                AddButton(panel, label + "  •  " + progress + "\n" +
                    (claimed ? "CLAIMED" : ready ? "CLAIM 20 GEMS" : "IN PROGRESS"),
                    ready && !claimed ? (Action)(() => ClaimQuest(index)) : null, 88, ready && !claimed ? gold : teal);
            }
        }

        private void ClaimQuest(int index)
        {
            if (State == null) return;
            if (State.Quests.TryClaim(index, State.Wallet)) Save();
            OpenQuests();
        }

        private void OpenEquipment()
        {
            if (State == null) return;
            ReturnToMenuWorld();
            CreatePanel("EQUIPMENT", "Six equipment slots. Upgrades consume Gold and Gear Materials.", true);
            if (State.Roster.Heroes.Count == 0) { AddInfo("Recruit a hero first.", 64); return; }
            HeroInstance hero = State.Roster.Heroes[0];
            HeroCardData card = HeroCatalog.Get(hero.HeroId);
            AddInfo((card != null ? card.DisplayName : hero.HeroId) + "  •  Gear level increases stats in the next battle.", 80);
            for (int i = 0; i < 6; i++)
            {
                EquipSlot slot = (EquipSlot)i;
                EquipmentInstance item = State.Equipment.Find(hero.HeroId, slot);
                if (item == null)
                {
                    State.Equipment.Equip(hero.HeroId, slot, "starter_" + slot.ToString().ToLowerInvariant());
                    item = State.Equipment.Find(hero.HeroId, slot);
                }
                string label = slot + "  •  " + item.ItemId + "  •  Lv " + item.Level;
                AddButton(panel, label + "\nUPGRADE", () => UpgradeEquipment(hero.HeroId, slot), 78, teal);
            }
            Save();
        }

        private void UpgradeEquipment(string heroId, EquipSlot slot)
        {
            if (State == null) return;
            if (!State.Equipment.TryUpgrade(heroId, slot, State.Wallet))
                AddInfo("Upgrade needs more Gold or Gear Materials.", 64);
            Save();
            OpenEquipment();
        }

        private void OpenBackpack()
        {
            if (State == null) return;
            ReturnToMenuWorld();
            CreatePanel("BACKPACK", "Player resources", true);
            AddInfo("GEMS  " + State.Wallet.Get(Currency.Gems).ToString("N0") +
                "\nGOLD  " + State.Wallet.Get(Currency.Gold).ToString("N0") +
                "\nEXPERIENCE  " + State.Wallet.Get(Currency.Exp).ToString("N0") +
                "\nGEAR MATERIALS  " + State.Wallet.Get(Currency.GearMaterials).ToString("N0"), 220);
        }

        private void OpenAllies()
        {
            ReturnToMenuWorld();
            CreatePanel("ALLIANCE", "Guild foundations", true);
            AddInfo("Guild creation, member chat, guild donations and guild wars are planned after the MVP core loop. No fake join action is shown here.", 220);
        }

        private void OpenMore()
        {
            ReturnToMenuWorld();
            CreatePanel("MORE SYSTEMS", "Additional game features", true);
            AddButton(panel, "DAILY QUESTS", OpenQuests, 64, teal);
            AddButton(panel, "EQUIPMENT", OpenEquipment, 64, teal);
            AddButton(panel, "BATTLE PASS", OpenBattlePass, 64, gold);
            AddButton(panel, "MONTHLY CARD", OpenMonthlyCard, 64, gold);
        }

        private void OpenArena()
        {
            ReturnToMenuWorld();
            CreatePanel("ARENA", "Asynchronous PvP", true);
            AddInfo("Arena battles require server-side matchmaking, opponent defense snapshots and replay verification. The server endpoints are not connected to this client yet.", 230);
        }

        private void OpenShop()
        {
            ReturnToMenuWorld();
            CreatePanel("MERCHANT", "Store and offers", true);
            AddInfo("The MVP economy is playable with earned Gems and Gold. Real-money billing and rewarded ads are intentionally not enabled in this development build.", 230);
        }

        private void OpenEvents()
        {
            ReturnToMenuWorld();
            CreatePanel("EVENTS", "Live operations", true);
            AddInfo("Event framework is reserved. No time-limited offer will be displayed until server time and event configuration are connected.", 190);
        }

        private void OpenDailyLogin()
        {
            ReturnToMenuWorld();
            CreatePanel("DAILY LOGIN", "Daily attendance", true);
            AddInfo("Daily quest reset is active. The seven-day login reward track is a remaining feature.", 150);
        }

        private void OpenBattlePass()
        {
            ReturnToMenuWorld();
            CreatePanel("BATTLE PASS", "Season progression", true);
            AddInfo("Season XP, reward tiers and paid track are not enabled in this MVP slice.", 150);
        }

        private void OpenMonthlyCard()
        {
            ReturnToMenuWorld();
            CreatePanel("MONTHLY CARD", "Optional purchase", true);
            AddInfo("Store billing is not connected. No purchase button is exposed in this development build.", 150);
        }

        private void OpenAllianceBoss()
        {
            ReturnToMenuWorld();
            CreatePanel("WORLD BOSS", "Weekly boss encounter", true);
            AddInfo("Boss UI is reserved until server-side attempts, damage validation and ranking rewards are implemented.", 170);
        }

        private void StartCampaignBattle()
        {
            if (State == null || GameBootstrap.Instance == null) return;
            activeStage = State.Campaign.HighestCleared + 1;
            if (activeStage > CampaignProgress.MaxStage) { AddInfo("Campaign completed.", 60); return; }
            CampaignStageData stageData = CampaignContent.Get(activeStage);
            if (battleView != null)
            {
                battleView.EventPlayed -= OnBattleEvent;
                Destroy(battleView.gameObject);
                battleView = null;
            }
            if (battleHud != null) battleHud = null;
            if (!CampaignScreen.TryEnter(State, activeStage, TimeService.NowUnix))
            {
                AddInfo("Not enough Energy or this stage is not unlocked.", 70);
                return;
            }
            Save();
            BuildBattleWorld();
            List<UnitSpec> player = new List<UnitSpec>();
            string[] formation = State.Roster.GetFormation();
            for (int i = 0; i < 5; i++)
            {
                HeroCardData card = HeroCatalog.Get(formation[i]);
                if (card == null) continue;
                HeroInstance instance = State.Roster.Find(card.Id);
                player.Add(BuildPlayerUnit(card, instance));
            }
            while (player.Count < 5)
            {
                HeroCardData card = HeroCatalog.Get(HeroCatalog.StartingHeroes[player.Count]);
                HeroInstance instance = State.Roster.Find(card.Id);
                player.Add(BuildPlayerUnit(card, instance));
            }
            UnitSpec[] enemies = CampaignContent.BuildEnemies(activeStage);
            battle = new BattleSim(player, enemies, activeStage * 7919 + (int)(Time.realtimeSinceStartup * 1000f));
            battle.AutoUltimate = true;
            battleResolved = false;
            GameObject battleObject = new GameObject("Battle Simulation");
            battleView = battleObject.AddComponent<BattleView>();
            battleHud = battleObject.AddComponent<BattleHud>();
            battleHud.Bind(battle, battleView);
            battleView.EventPlayed += OnBattleEvent;
            battleView.Begin(battle);
            for (int i = 0; i < battle.Units.Count; i++)
            {
                BattleUnit unit = battle.Units[i];
                string model = unit.Team == 0 ? HeroCatalog.Get(unit.Spec.Id).ModelId : EnemyModel(unit.Spec.Id);
                CreateBattleActor(unit, model);
            }
            OpenBattleHud(stageData);
        }

        private UnitSpec BuildPlayerUnit(HeroCardData card, HeroInstance instance)
        {
            UnitSpec spec = card.ToSpec(instance != null ? instance.Level : 1);
            int hpBonus = 0, atkBonus = 0, defBonus = 0;
            for (int i = 0; i < 6; i++)
            {
                EquipmentInstance item = State.Equipment.Find(card.Id, (EquipSlot)i);
                if (item == null) continue;
                int level = Mathf.Max(1, item.Level);
                if (item.Slot == EquipSlot.Weapon) atkBonus += level * 5;
                else if (item.Slot == EquipSlot.Armor || item.Slot == EquipSlot.Helmet) defBonus += level * 4;
                else if (item.Slot == EquipSlot.Boots) hpBonus += level * 18;
                else { atkBonus += level * 2; hpBonus += level * 8; }
            }
            spec.Hp += hpBonus;
            spec.Atk += atkBonus;
            spec.Def += defBonus;
            return spec;
        }

        private string EnemyModel(string id)
        {
            if (id.IndexOf("dragon", StringComparison.OrdinalIgnoreCase) >= 0) return "Dragon";
            if (id.IndexOf("slime", StringComparison.OrdinalIgnoreCase) >= 0) return "Slime";
            if (id.IndexOf("bat", StringComparison.OrdinalIgnoreCase) >= 0) return "Bat";
            return "Skeleton";
        }

        private void BuildBattleWorld()
        {
            if (homeWorldStage == null) homeWorldStage = FindObjectOfType<HomeWorldStage>();
            if (homeWorldStage != null) homeWorldStage.gameObject.SetActive(false);
            if (battleWorld != null) Destroy(battleWorld);
            actors.Clear();
            battleWorld = new GameObject("Battle Arena 3D");
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Arena Sand Floor";
            floor.transform.SetParent(battleWorld.transform, false);
            floor.transform.localScale = new Vector3(2.4f, 1f, 1.4f);
            Renderer renderer = floor.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = new Color(.58f, .43f, .27f);
            renderer.sharedMaterial = material;
            Destroy(floor.GetComponent<Collider>());

            GameObject archPrefab = Resources.Load<GameObject>("HomeCastle/Arch");
            if (archPrefab != null)
            {
                GameObject arch = Instantiate(archPrefab, battleWorld.transform);
                arch.transform.localPosition = new Vector3(0f, 0f, 7f);
                arch.transform.localScale = Vector3.one * 2.6f;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Battle Camera", typeof(Camera));
                camera = cameraObject.GetComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }
            camera.transform.position = new Vector3(0f, 7.5f, -15.5f);
            camera.transform.LookAt(new Vector3(0f, 1.1f, 0f));
            camera.fieldOfView = 48f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.06f, .13f, .16f);
            damageNumbers = battleWorld.AddComponent<DamageNumbers>();
            GameObject sunObject = new GameObject("Arena Key Light");
            sunObject.transform.SetParent(battleWorld.transform, false);
            sunObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.25f;
        }

        private void CreateBattleActor(BattleUnit unit, string modelId)
        {
            float[] x = { -1.7f, 1.7f, -4.2f, 0f, 4.2f };
            float z = unit.Team == 0 ? (unit.Slot < 2 ? -2.8f : -5.2f) : (unit.Slot < 2 ? 2.8f : 5.2f);
            GameObject actorObject = new GameObject("Team " + unit.Team + " - " + unit.Spec.Name);
            actorObject.transform.SetParent(battleWorld.transform, false);
            actorObject.transform.localPosition = new Vector3(x[unit.Slot], 0f, z);
            HeroView view = actorObject.AddComponent<HeroView>();
            view.Initialize(unit.Spec, modelId, unit.Team, unit.Slot, unit.Team == 1);
            actors[unit.Team + ":" + unit.Slot] = view;
        }

        private void OpenBattleHud(CampaignStageData stageData)
        {
            CreatePanel("STAGE " + activeStage + "  •  " + stageData.Biome,
                "AUTO BATTLE 5v5  •  x1 / x2 / x4  •  Manual Ultimate optional", false);
            battleStatus = AddText(panel, "BATTLE IN PROGRESS", 24, cream, 56);
            battleLog = AddText(panel, "The teams are entering the arena...", 18, teal, 100);
            battleSummary = AddText(panel, "Your team 5  •  Enemy team 5", 18, cream, 70);
            AddButton(panel, "SPEED x1", () => SetBattleSpeed(1), 54, teal);
            AddButton(panel, "SPEED x2", () => SetBattleSpeed(2), 54, teal);
            AddButton(panel, "SPEED x4", () => SetBattleSpeed(4), 54, teal);
            AddButton(panel, "TOGGLE AUTO ULTIMATE", ToggleAutoUltimate, 58, gold);
            for (int i = 0; i < 5; i++)
            {
                int slot = i;
                AddButton(panel, "USE ULTIMATE  •  HERO " + (i + 1), () => RequestUltimate(slot), 48, gold);
            }
            AddButton(panel, "RETREAT TO CITY", RetreatBattle, 54, new Color(1f, .42f, .34f));
        }

        private void SetBattleSpeed(int speed)
        {
            if (battleHud != null) battleHud.SetSpeed(speed);
            else if (battleView != null) battleView.SetSpeed(speed);
            if (battleStatus != null) battleStatus.text = "BATTLE IN PROGRESS  •  SPEED x" + speed;
        }

        private void ToggleAutoUltimate()
        {
            if (battle == null) return;
            if (battleHud != null) battleHud.ToggleAutoUltimate();
            else battle.AutoUltimate = !battle.AutoUltimate;
            if (battleStatus != null) battleStatus.text = battle.AutoUltimate ? "AUTO ULTIMATE: ON" : "AUTO ULTIMATE: OFF";
        }

        private void RequestUltimate(int slot)
        {
            if (battle != null && !(battleHud != null ? battleHud.RequestUltimate(slot) : battle.RequestUltimate(0, slot)) && battleLog != null)
                battleLog.text = "Ultimate is not charged yet for hero " + (slot + 1) + ".";
        }

        private void OnBattleEvent(BattleEvent battleEvent)
        {
            if (battleEvent.Type == BattleEventType.Attack || battleEvent.Type == BattleEventType.Skill ||
                battleEvent.Type == BattleEventType.Ultimate)
            {
                HeroView sourceActor;
                string sourceKey = battleEvent.SourceTeam + ":" + battleEvent.SourceSlot;
                if (actors.TryGetValue(sourceKey, out sourceActor)) sourceActor.PlayEvent(battleEvent);
            }

            string actorKey = battleEvent.TargetTeam + ":" + battleEvent.TargetSlot;
            HeroView target;
            if (actors.TryGetValue(actorKey, out target))
            {
                target.PlayEvent(battleEvent);
                if (battleEvent.Type == BattleEventType.Attack || battleEvent.Type == BattleEventType.Skill ||
                    battleEvent.Type == BattleEventType.Ultimate || battleEvent.Type == BattleEventType.Heal)
                {
                    if (damageNumbers != null)
                        damageNumbers.Spawn(target.transform.position, battleEvent.Amount,
                            battleEvent.Type == BattleEventType.Heal, battleEvent.Crit);
                }
            }
            if (battleEvent.Type == BattleEventType.Attack || battleEvent.Type == BattleEventType.Skill ||
                battleEvent.Type == BattleEventType.Ultimate || battleEvent.Type == BattleEventType.Heal ||
                battleEvent.Type == BattleEventType.Death)
            {
                if (battleLog != null)
                    battleLog.text = battleEvent.Type + "  •  Team " + battleEvent.TargetTeam +
                        "  •  Slot " + (battleEvent.TargetSlot + 1) + "  •  " + battleEvent.Amount;
            }
            if (battleEvent.Type == BattleEventType.End && !battleResolved) FinishBattle();
            RefreshBattleHealth();
        }

        private void RefreshBattleHealth()
        {
            if (battle == null || battleSummary == null) return;
            int aliveA = 0, aliveB = 0, hpA = 0, hpB = 0;
            for (int i = 0; i < battle.Units.Count; i++)
            {
                BattleUnit unit = battle.Units[i];
                if (unit.Team == 0) { if (unit.Alive) aliveA++; hpA += unit.Hp; }
                else { if (unit.Alive) aliveB++; hpB += unit.Hp; }
            }
            battleSummary.text = "ALLY  " + aliveA + "/5  •  HP " + hpA +
                "\nENEMY  " + aliveB + "/5  •  HP " + hpB;
        }

        private void FinishBattle()
        {
            battleResolved = true;
            bool won = battle != null && battle.Winner == 0;
            if (State != null)
            {
                if (won) CampaignScreen.ApplyVictory(State, activeStage);
                else State.Quests.Report("battle_5", 1);
                Save();
            }
            if (battleStatus != null) battleStatus.text = won ? "VICTORY  •  STAGE CLEARED" : "DEFEAT  •  RETRY TO CONTINUE";
            if (battleLog != null) battleLog.text = won
                ? "Rewards collected. Your campaign has advanced."
                : "Your formation was defeated. Improve heroes and try again.";
            AddButton(panel, "RETURN TO CITY", ReturnToCity, 70, gold);
            AddButton(panel, won ? "NEXT STAGE" : "RETRY STAGE", StartCampaignBattle, 70, teal);
        }

        private void RetreatBattle()
        {
            if (battleView != null) battleView.EventPlayed -= OnBattleEvent;
            ReturnToCity();
        }

        private void ReturnToCity()
        {
            if (battleView != null) battleView.EventPlayed -= OnBattleEvent;
            if (battleView != null) Destroy(battleView.gameObject);
            battleView = null;
            battle = null;
            if (battleWorld != null) Destroy(battleWorld);
            battleWorld = null;
            actors.Clear();
            if (homeWorldStage == null) homeWorldStage = FindObjectOfType<HomeWorldStage>();
            if (homeWorldStage != null) homeWorldStage.gameObject.SetActive(true);
            if (panel != null) Destroy(panel.parent.gameObject);
            panel = null;
            RefreshHome();
        }

        private void ReturnToMenuWorld()
        {
            if (battle != null) ReturnToCity();
        }

        private void RefreshHome()
        {
            if (home != null) home.SendMessage("RefreshData", SendMessageOptions.DontRequireReceiver);
        }

        private void Save()
        {
            if (GameBootstrap.Instance != null) GameBootstrap.Instance.SaveNow();
            RefreshHome();
        }

        private void OnDestroy()
        {
            if (home == null) return;
            home.onCampaign.RemoveListener(OpenCampaign);
            home.onHeroes.RemoveListener(OpenHeroes);
            home.onFormation.RemoveListener(OpenFormation);
            home.onEquipment.RemoveListener(OpenEquipment);
            home.onQuests.RemoveListener(OpenQuests);
            home.onBackpack.RemoveListener(OpenBackpack);
            home.onAllies.RemoveListener(OpenAllies);
            home.onMore.RemoveListener(OpenMore);
            home.onSummon.RemoveListener(OpenSummon);
            home.onArena.RemoveListener(OpenArena);
            home.onShop.RemoveListener(OpenShop);
            home.onEvents.RemoveListener(OpenEvents);
            home.onDailyLogin.RemoveListener(OpenDailyLogin);
            home.onBattlePass.RemoveListener(OpenBattlePass);
            home.onMonthlyCard.RemoveListener(OpenMonthlyCard);
            home.onAllianceBoss.RemoveListener(OpenAllianceBoss);
            home.onClaimIdle.RemoveListener(RefreshHome);
        }
    }
}
