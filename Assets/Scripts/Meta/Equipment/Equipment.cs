// =============================================================================
// FILE: Assets/Scripts/Meta/Equipment/Equipment.cs
// OWNER ROLE: Systems Designer
// RESPONSIBILITY: Serializable six-slot hero equipment and engine-free equip rules.
// WORK ON: Connect authored equipment data, set bonuses and economy-approved upgrade costs.
// STATUS: Foundation implemented; balance and content data pending.
// =============================================================================
using System;
using System.Collections.Generic;

namespace SandsOfLegends.Meta
{
    public enum EquipSlot { Weapon, Helmet, Armor, Boots, Ring, Amulet }

    [Serializable]
    public sealed class EquipmentItem
    {
        public string Id;
        public EquipSlot Slot;
        public string SetId;
        public int Level = 1;
        public int HpBonus;
        public int AtkBonus;
        public int DefBonus;
    }

    /// <summary>Pure rules for the six equipment slots. No costs or balance values are invented here.</summary>
    public static class EquipmentRules
    {
        public const int SlotCount = 6;

        public static EquipmentItem Find(IList<EquipmentItem> items, EquipSlot slot)
        {
            if (items == null) return null;
            for (int i = 0; i < items.Count; i++)
                if (items[i] != null && items[i].Slot == slot) return items[i];
            return null;
        }

        public static bool TryEquip(HeroInstance hero, EquipmentItem item, out EquipmentItem replaced)
        {
            replaced = null;
            if (hero == null || item == null || string.IsNullOrEmpty(item.Id) || item.Level < 1)
                return false;
            if ((int)item.Slot < 0 || (int)item.Slot >= SlotCount)
                return false;

            if (hero.Equipment == null) hero.Equipment = new List<EquipmentItem>();
            for (int i = 0; i < hero.Equipment.Count; i++)
            {
                var current = hero.Equipment[i];
                if (current == null) continue;
                if (current.Id == item.Id) return false;
                if (current.Slot == item.Slot)
                {
                    replaced = current;
                    hero.Equipment[i] = item;
                    return true;
                }
            }
            hero.Equipment.Add(item);
            return true;
        }

        public static bool TryUnequip(HeroInstance hero, EquipSlot slot, out EquipmentItem removed)
        {
            removed = null;
            if (hero == null || hero.Equipment == null) return false;
            for (int i = 0; i < hero.Equipment.Count; i++)
            {
                if (hero.Equipment[i] == null || hero.Equipment[i].Slot != slot) continue;
                removed = hero.Equipment[i];
                hero.Equipment.RemoveAt(i);
                return true;
            }
            return false;
        }
    }
}
