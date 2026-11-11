// =============================================================================
// FILE: Assets/Scripts/Meta/Heroes/HeroInstance.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Owned hero state (level, stars, ascension, shards) + Progression curves.
// WORK ON: Replace placeholder curves with Economy Designer's tables (Data/Economy). Add equipment slots (6) per hero.
// STATUS: Implemented (placeholder curves)
// =============================================================================
using System;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Meta
{
    [Serializable]
    public sealed class HeroInstance
    {
        public string HeroId;
        public int Level = 1;
        public int Stars;        // 0..5
        public int Ascension;    // 0..4
        public int Shards;       // hero-specific shards for stars
    }

    /// <summary>Progression curves (placeholders - the real numbers come from the economy ledger).</summary>
    public static class Progression
    {
        public const int MaxStars = 5;
        public const int MaxAscension = 4;

        public static int LevelCap(int ascension) { return 20 + 20 * ascension; }        // 20,40,60,80,100
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
