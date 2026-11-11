// =============================================================================
// FILE: Assets/Scripts/Meta/Heroes/Roster.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: The player's hero collection + shared 'Spirit Source' level ceiling (new heroes are raised to ceiling-5, GDD 4.2).
// WORK ON: Add hero level-up/star-up/ascend operations (spend Wallet), team loadout (5 slots, front/back).
// STATUS: Implemented (collection + ceiling only)
// =============================================================================
using System;
using System.Collections.Generic;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Meta
{
    public sealed class Roster
    {
        public List<HeroInstance> Heroes = new List<HeroInstance>();

        public HeroInstance Find(string heroId)
        {
            for (int i = 0; i < Heroes.Count; i++)
                if (Heroes[i].HeroId == heroId) return Heroes[i];
            return null;
        }

        /// <summary>Shared level ceiling = highest level among owned heroes.</summary>
        public int SpiritCeiling
        {
            get
            {
                int m = 1;
                for (int i = 0; i < Heroes.Count; i++) m = Math.Max(m, Heroes[i].Level);
                return m;
            }
        }

        /// <summary>Adds a new hero (at ceiling-5) or converts a duplicate into shards. Returns true if new.</summary>
        public bool AddHeroOrShards(string heroId, Rarity rarity, out int shardsGained)
        {
            shardsGained = 0;
            var h = Find(heroId);
            if (h != null)
            {
                shardsGained = Progression.DuplicateShards(rarity);
                h.Shards += shardsGained;
                return false;
            }
            Heroes.Add(new HeroInstance { HeroId = heroId, Level = Math.Max(1, SpiritCeiling - 5) });
            return true;
        }
    }
}
