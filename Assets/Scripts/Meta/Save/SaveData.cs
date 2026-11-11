// Versioned JSON save model. Increment CurrentVersion and add migrations when fields change.
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
        public string[] Formation = new string[5];
        public List<EquipmentInstance> Equipment = new List<EquipmentInstance>();
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
            if (d.Formation == null || d.Formation.Length != 5) d.Formation = new string[5];
            if (d.Equipment == null) d.Equipment = new List<EquipmentInstance>();
            if (d.QuestProgress == null || d.QuestProgress.Length != 5) d.QuestProgress = new int[5];
            if (d.QuestClaimed == null || d.QuestClaimed.Length != 5) d.QuestClaimed = new bool[5];
            if (d.QuestDay == null) d.QuestDay = "";
            d.Version = CurrentVersion;
            return d;
        }
    }
}
