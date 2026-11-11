// =============================================================================
// FILE: Assets/Scripts/Meta/Heroes/Roster.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Hero collection, shared Spirit Source ceiling and equipment loadouts.
// WORK ON: Add economy-approved level/star/ascension operations and five-slot team formation.
// STATUS: Collection, ceiling and six-slot equip/unequip rules implemented.
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
                if (Heroes[i] != null && Heroes[i].HeroId == heroId) return Heroes[i];
            return null;
        }

        public int SpiritCeiling
        {
            get
            {
                int m = 1;
                for (int i = 0; i < Heroes.Count; i++)
                    if (Heroes[i] != null) m = Math.Max(m, Heroes[i].Level);
                return m;
            }
        }

        public bool AddHeroOrShards(string heroId, Rarity rarity, out int shardsGained)
        {
            shardsGained = 0;
            if (string.IsNullOrEmpty(heroId)) return false;
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

        public bool TryEquip(string heroId, EquipmentItem item, out EquipmentItem replaced)
        {
            replaced = null;
            return EquipmentRules.TryEquip(Find(heroId), item, out replaced);
        }

        public bool TryUnequip(string heroId, EquipSlot slot, out EquipmentItem removed)
        {
            removed = null;
            return EquipmentRules.TryUnequip(Find(heroId), slot, out removed);
        }
    }
}
