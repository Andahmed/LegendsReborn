// =============================================================================
// FILE: Assets/Scripts/Meta/Equipment/Equipment.cs
// OWNER ROLE: Systems Designer
// RESPONSIBILITY: Equipment: 6 slots per hero, set bonuses, level via GearMaterials (GDD 2.3/4.4).
// WORK ON: Define EquipmentItem (slot, set, stats, level), set bonuses, equip/unequip/upgrade operations; add to HeroInstance + SaveData.
// STATUS: Stub
// =============================================================================
namespace SandsOfLegends.Meta
{
    public enum EquipSlot { Weapon, Helmet, Armor, Boots, Ring, Amulet }

    public sealed class EquipmentItem
    {
        public string Id;
        public EquipSlot Slot;
        public string SetId;
        public int Level = 1;
        // TODO: base stats + substats, set bonus lookup (Data/Economy).
    }
}
