// =============================================================================
// FILE: Assets/Scripts/Bootstrap/GameBootstrap.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Entry point (Boot scene): loads/creates GameState, refreshes energy, resets daily quests, saves on pause/quit.
// WORK ON: Add server login, idle-reward popup on start, scene routing (Boot -> Home), tutorial flag.
// STATUS: Implemented
// =============================================================================
using UnityEngine;
using SandsOfLegends.Meta;
using SandsOfLegends.Services;

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
            State.Energy.Refresh(now);
            State.Quests.ResetIfNewDay(TimeService.DayKey);
            Analytics.Track(AnalyticsEvents.SessionStart);
        }

        public void SaveNow() { if (State != null) Save.Save(State.ToSave(TimeService.NowUnix)); }
        private void OnApplicationPause(bool paused) { if (paused) SaveNow(); }
        private void OnApplicationQuit() { SaveNow(); }
    }
}
