// =============================================================================
// FILE: Assets/Scripts/Meta/Heroes/HeroInstance.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Owned hero state, progression and equipped item references.
// WORK ON: Replace placeholder progression curves with Economy Designer tables; implement approved level/star costs.
// STATUS: Implemented (placeholder curves); equipment slots persisted.
// =============================================================================
using System;
using System.Collections.Generic;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Meta
{
    [Serializable]
    public sealed class HeroInstance
    {
        public string HeroId;
        public int Level = 1;
        public int Stars;
        public int Ascension;
        public int Shards;
        public List<EquipmentItem> Equipment = new List<EquipmentItem>();
    }

    /// <summary>Progression curves remain placeholders until the economy ledger is approved.</summary>
    public static class Progression
    {
        public const int MaxStars = 5;
        public const int MaxAscension = 4;

        public static int LevelCap(int ascension) { return 20 + 20 * ascension; }
        public static int ExpToNext(int level) { return 100 + 40 * level + 5 * level * level; }
        public static int ShardsForNextStar(int currentStars) { return 10 * (currentStars + 1); }

        public static int DuplicateShards(Rarity r)
        {
            switch (r)
            {
                case Rarity.Common: return 5;
                case Rarity.Rare: return 10;
                case Rarity.Epic: return 20;
                default: return 40;
            }
        }
    }
}
