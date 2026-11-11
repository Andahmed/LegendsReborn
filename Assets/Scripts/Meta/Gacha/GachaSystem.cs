// =============================================================================
// FILE: Assets/Scripts/Meta/Gacha/GachaSystem.cs
// OWNER ROLE: Systems Designer
// RESPONSIBILITY: Summon logic: rarity rates 40/35/20/5, pity (Epic every 30, Legendary every 90), duplicates -> shards. Deterministic via SeededRng.
// WORK ON: Tune rates (test values, GDD 4.2), add banners/rate-up, publish official rates screen data. Final version runs on the SERVER (audit log) - client only shows results.
// STATUS: Implemented (test rates)
// =============================================================================
using System;
using System.Collections.Generic;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Meta
{
    [Serializable]
    public sealed class PityState
    {
        public int SinceEpic;
        public int SinceLegendary;
    }

    public struct PullResult
    {
        public string HeroId;
        public Rarity Rarity;
        public bool IsNew;
        public int ShardsGained;
    }

    public static class GachaSystem
    {
        public const int EpicPity = 30;
        public const int LegendaryPity = 90;
        public const int GemCostSingle = 300;
        public const int GemCostTen = 2700;

        // cumulative thresholds out of 100: Common 40, Rare 35, Epic 20, Legendary 5
        private static Rarity Roll(float r100)
        {
            if (r100 < 40f) return Rarity.Common;
            if (r100 < 75f) return Rarity.Rare;
            if (r100 < 95f) return Rarity.Epic;
            return Rarity.Legendary;
        }

        public static PullResult Pull(IDictionary<Rarity, List<string>> pool, PityState pity, SeededRng rng, Roster roster)
        {
            Rarity rarity = Roll(rng.NextFloat() * 100f);
            if (pity.SinceLegendary + 1 >= LegendaryPity) rarity = Rarity.Legendary;
            else if (pity.SinceEpic + 1 >= EpicPity && rarity < Rarity.Epic) rarity = Rarity.Epic;

            rarity = NearestNonEmpty(pool, rarity);

            if (rarity == Rarity.Legendary) { pity.SinceLegendary = 0; pity.SinceEpic = 0; }
            else if (rarity == Rarity.Epic) { pity.SinceEpic = 0; pity.SinceLegendary++; }
            else { pity.SinceEpic++; pity.SinceLegendary++; }

            var list = pool[rarity];
            string id = list[(int)(rng.NextUInt() % (uint)list.Count)];
            int shards;
            bool isNew = roster.AddHeroOrShards(id, rarity, out shards);
            return new PullResult { HeroId = id, Rarity = rarity, IsNew = isNew, ShardsGained = shards };
        }

        private static Rarity NearestNonEmpty(IDictionary<Rarity, List<string>> pool, Rarity want)
        {
            for (int d = 0; d < 4; d++)
            {
                int down = (int)want - d, up = (int)want + d;
                if (down >= 0 && Has(pool, (Rarity)down)) return (Rarity)down;
                if (up <= 3 && Has(pool, (Rarity)up)) return (Rarity)up;
            }
            throw new InvalidOperationException("Gacha pool is empty");
        }

        private static bool Has(IDictionary<Rarity, List<string>> pool, Rarity r)
        {
            List<string> l;
            return pool.TryGetValue(r, out l) && l != null && l.Count > 0;
        }
    }
}
