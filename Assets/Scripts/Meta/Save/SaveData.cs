// =============================================================================
// FILE: Assets/Scripts/Meta/Save/SaveData.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Versioned JsonUtility-friendly save model and migrations; one save system.
// WORK ON: Add an explicit migration whenever persisted gameplay fields change.
// STATUS: Version 3; equipment and five-slot formation migrate safely from legacy saves.
// =============================================================================
using System;
using System.Collections.Generic;

namespace SandsOfLegends.Meta
{
    [Serializable]
    public sealed class SaveData
    {
        public int Version = 3;
        public long Gems, Gold, Exp, GearMaterials;
        public List<HeroInstance> Heroes = new List<HeroInstance>();
        public string[] TeamSlots = new string[TeamFormation.SlotCount];
        public int PitySinceEpic, PitySinceLegendary;
        public int Energy;
        public long EnergyTickUnix;
        public int HighestStage;
        public long IdleLastClaimUnix;
        public string QuestDay = "";
        public int[] QuestProgress = new int[5];
        public bool[] QuestClaimed = new bool[5];
        public long SavedAtUnix;
    }

    public static class SaveMigrations
    {
        public const int CurrentVersion = 3;

        public static SaveData Migrate(SaveData d)
        {
            if (d == null) return null;
            if (d.Heroes == null) d.Heroes = new List<HeroInstance>();
            if (d.QuestProgress == null || d.QuestProgress.Length != 5) d.QuestProgress = new int[5];
            if (d.QuestClaimed == null || d.QuestClaimed.Length != 5) d.QuestClaimed = new bool[5];
            if (d.QuestDay == null) d.QuestDay = "";

            if (d.Version < 2)
            {
                for (int i = 0; i < d.Heroes.Count; i++)
                    if (d.Heroes[i] != null && d.Heroes[i].Equipment == null)
                        d.Heroes[i].Equipment = new List<EquipmentItem>();
                d.Version = 2;
            }

            if (d.Version < 3)
            {
                d.TeamSlots = NormalizeTeamSlots(d.TeamSlots);
                d.Version = 3;
            }

            // Defensive normalization also covers partially written/newer saves.
            for (int i = 0; i < d.Heroes.Count; i++)
                if (d.Heroes[i] != null && d.Heroes[i].Equipment == null)
                    d.Heroes[i].Equipment = new List<EquipmentItem>();
            d.TeamSlots = NormalizeTeamSlots(d.TeamSlots);

            d.Version = CurrentVersion;
            return d;
        }

        private static string[] NormalizeTeamSlots(string[] slots)
        {
            if (slots != null && slots.Length == TeamFormation.SlotCount) return slots;
            var normalized = new string[TeamFormation.SlotCount];
            if (slots != null)
                Array.Copy(slots, normalized, Math.Min(slots.Length, TeamFormation.SlotCount));
            return normalized;
        }
    }
}
