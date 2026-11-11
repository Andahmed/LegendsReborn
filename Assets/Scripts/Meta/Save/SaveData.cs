// =============================================================================
// FILE: Assets/Scripts/Meta/Save/SaveData.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Versioned save model (JsonUtility-friendly POCO) + migrations. ONE save system for the whole game.
// WORK ON: Bump Version and add a migration step for every new/changed field. Later: cloud save (server) uses the same model.
// STATUS: Implemented
// =============================================================================
using System;
using System.Collections.Generic;

namespace SandsOfLegends.Meta
{
    [Serializable]
    public sealed class SaveData
    {
        public int Version = 1;
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
        public const int CurrentVersion = 1;

        public static SaveData Migrate(SaveData d)
        {
            if (d == null) return null;
            if (d.Heroes == null) d.Heroes = new List<HeroInstance>();
            if (d.QuestProgress == null || d.QuestProgress.Length != 5) d.QuestProgress = new int[5];
            if (d.QuestClaimed == null || d.QuestClaimed.Length != 5) d.QuestClaimed = new bool[5];
            if (d.QuestDay == null) d.QuestDay = "";
            // if (d.Version < 2) { ...upgrade...; d.Version = 2; }
            d.Version = CurrentVersion;
            return d;
        }
    }
}
