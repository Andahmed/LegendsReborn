// =============================================================================
// FILE: Assets/Scripts/Meta/Save/SaveData.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Versioned JsonUtility-friendly save model and migrations; one save system.
// WORK ON: Add an explicit migration whenever persisted gameplay fields change.
// STATUS: Version 2; equipment collections migrate safely from version 1.
// =============================================================================
using System;
using System.Collections.Generic;

namespace SandsOfLegends.Meta
{
    [Serializable]
    public sealed class SaveData
    {
        public int Version = 2;
        public long Gems, Gold, Exp, GearMaterials;
        public List<HeroInstance> Heroes = new List<HeroInstance>();
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
        public const int CurrentVersion = 2;

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

            // Defensive normalization also covers partially written/newer saves.
            for (int i = 0; i < d.Heroes.Count; i++)
                if (d.Heroes[i] != null && d.Heroes[i].Equipment == null)
                    d.Heroes[i].Equipment = new List<EquipmentItem>();

            d.Version = CurrentVersion;
            return d;
        }
    }
}
