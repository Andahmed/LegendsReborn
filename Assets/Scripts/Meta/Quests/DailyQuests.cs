// =============================================================================
// FILE: Assets/Scripts/Meta/Quests/DailyQuests.cs
// OWNER ROLE: Systems Designer
// RESPONSIBILITY: Five daily quests with progress/claim and daily reset by day key (GDD 4.1).
// WORK ON: Real quest list + rewards from Systems Designer; weekly quests; event quests.
// STATUS: Implemented (placeholder quests)
// =============================================================================
using System;

namespace SandsOfLegends.Meta
{
    public sealed class DailyQuests
    {
        public static readonly string[] Ids = { "battle_5", "spend_energy_30", "summon_1", "level_up_hero", "claim_idle" };
        public static readonly int[] Targets = { 5, 30, 1, 1, 1 };
        public const int RewardGemsEach = 20;

        public string DayKey = "";
        public int[] Progress = new int[5];
        public bool[] Claimed = new bool[5];

        public void ResetIfNewDay(string dayKey)
        {
            if (DayKey == dayKey) return;
            DayKey = dayKey;
            Progress = new int[Ids.Length];
            Claimed = new bool[Ids.Length];
        }

        public void Report(string id, int amount)
        {
            int i = Array.IndexOf(Ids, id);
            if (i < 0 || amount <= 0) return;
            Progress[i] = Math.Min(Targets[i], Progress[i] + amount);
        }

        public bool TryClaim(int index, Wallet wallet)
        {
            if (index < 0 || index >= Ids.Length || Claimed[index] || Progress[index] < Targets[index]) return false;
            Claimed[index] = true;
            wallet.Add(Currency.Gems, RewardGemsEach);
            return true;
        }
    }
}
