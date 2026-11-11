// =============================================================================
// FILE: Assets/Scripts/Meta/Heroes/Roster.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Hero collection, shared Spirit Source ceiling, equipment and team formation.
// WORK ON: Connect five-slot formation to the Heroes UI and battle UnitSpec creation.
// STATUS: Collection, ceiling, six-slot equipment and five-slot formation rules implemented.
// =============================================================================
using System;
using System.Collections.Generic;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Meta
{
    public sealed class Roster
    {
        public List<HeroInstance> Heroes = new List<HeroInstance>();
        public readonly TeamFormation Formation = new TeamFormation();

        public HeroInstance Find(string heroId)
        {
            if (string.IsNullOrEmpty(heroId)) return null;
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

        public bool TryAssignFormationSlot(int slot, string heroId, out string replacedHeroId)
        {
            return Formation.TryAssign(slot, heroId, this, out replacedHeroId);
        }

        public bool TryMoveFormationSlot(int fromSlot, int toSlot, out string displacedHeroId)
        {
            return Formation.TryMove(fromSlot, toSlot, out displacedHeroId);
        }

        public bool TryRemoveFormationSlot(int slot, out string removedHeroId)
        {
            return Formation.TryRemove(slot, out removedHeroId);
        }
    }
}
