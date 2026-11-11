// =============================================================================
// FILE: Assets/Scripts/Services/AnalyticsService.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Analytics interface + event names for the KPIs (D1/D7/D30, tutorial completion, session length). Measured from the first build (GDD 7.1).
// WORK ON: Plug Firebase/Amplitude in a new IAnalytics implementation; fire the events from the flows.
// STATUS: Implemented (null sink)
// =============================================================================
using UnityEngine;

namespace SandsOfLegends.Services
{
    public interface IAnalytics
    {
        void Track(string eventName, string key = null, string value = null);
    }

    public static class AnalyticsEvents
    {
        public const string SessionStart = "session_start";
        public const string TutorialStep = "tutorial_step";
        public const string TutorialComplete = "tutorial_complete";
        public const string BattleStart = "battle_start";
        public const string BattleEnd = "battle_end";
        public const string Summon = "summon";
        public const string IdleClaim = "idle_claim";
        public const string Purchase = "purchase";
    }

    public sealed class NullAnalytics : IAnalytics
    {
        public void Track(string eventName, string key = null, string value = null)
        {
            Debug.Log("[Analytics] " + eventName + (key != null ? " " + key + "=" + value : ""));
        }
    }
}
