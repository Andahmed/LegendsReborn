// =============================================================================
// FILE: Assets/Scripts/Bootstrap/GameBootstrap.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Entry point (Boot scene): loads/creates GameState, refreshes energy, resets daily quests, saves on pause/quit.
// WORK ON: Add server login, idle-reward popup on start, scene routing (Boot -> Home), tutorial flag.
// STATUS: Implemented
// =============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;
using SandsOfLegends.Meta;
using SandsOfLegends.Services;
using SandsOfLegends.Core;
using SandsOfLegends.Combat;
using SandsOfLegends.UI;

namespace SandsOfLegends.Bootstrap
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }
        public GameState State { get; private set; }
        public SaveService Save { get; private set; }
        public IAnalytics Analytics { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Save = new SaveService();
            Analytics = new NullAnalytics();
            long now = TimeService.NowUnix;
            var data = Save.Load();
            State = data != null ? GameState.FromSave(data) : GameState.NewGame(now);
            if (data == null) CreateStarterProfile();
            State.Energy.Refresh(now);
            State.Quests.ResetIfNewDay(TimeService.DayKey);
            Analytics.Track(AnalyticsEvents.SessionStart);
            if (data == null) SaveNow();
        }

        private void CreateStarterProfile()
        {
            State.Wallet.Set(Currency.Gems, 3000);
            State.Wallet.Set(Currency.Gold, 15000);
            State.Wallet.Set(Currency.Exp, 5000);
            State.Wallet.Set(Currency.GearMaterials, 100);
            string[] starters = HeroCatalog.StartingHeroes;
            for (int i = 0; i < starters.Length; i++)
            {
                HeroCardData card = HeroCatalog.Get(starters[i]);
                int shards;
                State.Roster.AddHeroOrShards(starters[i], card.Rarity, out shards);
                State.Roster.SetFormation(i, starters[i]);
                for (int slot = 0; slot < 6; slot++)
                    State.Equipment.Equip(starters[i], (EquipSlot)slot, "starter_" + ((EquipSlot)slot).ToString().ToLowerInvariant());
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().name != "Home" &&
                Application.CanStreamedLevelBeLoaded("Home"))
            {
                SceneManager.LoadScene("Home");
                return;
            }
            BindHomeScreen();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Home") BindHomeScreen();
        }

        private void BindHomeScreen()
        {
            HomeScreen home = FindObjectOfType<HomeScreen>();
            if (home == null) return;
            GameFlowController flow = home.GetComponent<GameFlowController>();
            if (flow == null) flow = home.gameObject.AddComponent<GameFlowController>();
            flow.Bind(home);
        }

        public void SaveNow() { if (State != null) Save.Save(State.ToSave(TimeService.NowUnix)); }
        private void OnApplicationPause(bool paused) { if (paused) SaveNow(); }
        private void OnApplicationQuit() { SaveNow(); }
    }
}
