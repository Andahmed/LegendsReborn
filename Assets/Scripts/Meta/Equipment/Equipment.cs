using System;
using System.Collections.Generic;

namespace SandsOfLegends.Meta
{
    public enum EquipSlot { Weapon, Helmet, Armor, Boots, Ring, Amulet }

    [Serializable]
    public sealed class EquipmentInstance
    {
        public string HeroId;
        public EquipSlot Slot;
        public string ItemId;
        public int Level = 1;
    }

    [Serializable]
    public sealed class EquipmentLoadout
    {
        public List<EquipmentInstance> Items = new List<EquipmentInstance>();

        public EquipmentInstance Find(string heroId, EquipSlot slot)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].HeroId == heroId && Items[i].Slot == slot) return Items[i];
            return null;
        }

        public bool Equip(string heroId, EquipSlot slot, string itemId)
        {
            if (string.IsNullOrEmpty(heroId) || string.IsNullOrEmpty(itemId)) return false;
            EquipmentInstance item = Find(heroId, slot);
            if (item == null)
            {
                item = new EquipmentInstance { HeroId = heroId, Slot = slot, ItemId = itemId, Level = 1 };
                Items.Add(item);
            }
            else item.ItemId = itemId;
            return true;
        }

        public bool TryUpgrade(string heroId, EquipSlot slot, Wallet wallet)
        {
            EquipmentInstance item = Find(heroId, slot);
            if (item == null) return false;
            long goldCost = 100L * item.Level;
            long materialCost = 3L * item.Level;
            if (!wallet.TrySpend(Currency.Gold, goldCost)) return false;
            if (!wallet.TrySpend(Currency.GearMaterials, materialCost))
            {
                wallet.Add(Currency.Gold, goldCost);
                return false;
            }
            item.Level = Math.Min(20, item.Level + 1);
            return true;
        }
    }
}
