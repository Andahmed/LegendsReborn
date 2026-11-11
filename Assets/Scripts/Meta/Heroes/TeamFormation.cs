// =============================================================================
// FILE: Assets/Scripts/Meta/Heroes/TeamFormation.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Engine-free five-slot team formation (2 front + 3 back) for owned heroes.
// WORK ON: Connect drag/drop UI and map selected HeroDefinitions to ordered UnitSpecs.
// STATUS: Assignment, movement, uniqueness checks and save-safe normalization implemented.
// =============================================================================
using System;

namespace SandsOfLegends.Meta
{
    [Serializable]
    public sealed class TeamFormation
    {
        public const int SlotCount = 5;
        public const int FrontSlotCount = 2;

        // Slot order is authoritative: 0-1 front row, 2-4 back row.
        private string[] _heroIds = new string[SlotCount];

        public static bool IsValidSlot(int slot)
        {
            return slot >= 0 && slot < SlotCount;
        }

        public static bool IsFrontSlot(int slot)
        {
            return IsValidSlot(slot) && slot < FrontSlotCount;
        }

        public string GetHeroId(int slot)
        {
            if (!IsValidSlot(slot)) return null;
            EnsureStorage();
            return _heroIds[slot];
        }

        public bool TryAssign(int slot, string heroId, Roster roster, out string replacedHeroId)
        {
            replacedHeroId = null;
            if (!IsValidSlot(slot) || string.IsNullOrEmpty(heroId) || roster == null)
                return false;
            if (roster.Find(heroId) == null) return false;

            EnsureStorage();
            for (int i = 0; i < SlotCount; i++)
                if (i != slot && _heroIds[i] == heroId) return false;

            if (_heroIds[slot] == heroId) return true;
            replacedHeroId = _heroIds[slot];
            _heroIds[slot] = heroId;
            return true;
        }

        // Moves a hero and swaps with the target slot when it is occupied.
        public bool TryMove(int fromSlot, int toSlot, out string displacedHeroId)
        {
            displacedHeroId = null;
            if (!IsValidSlot(fromSlot) || !IsValidSlot(toSlot)) return false;
            EnsureStorage();
            if (fromSlot == toSlot) return true;
            if (string.IsNullOrEmpty(_heroIds[fromSlot])) return false;

            displacedHeroId = _heroIds[toSlot];
            _heroIds[toSlot] = _heroIds[fromSlot];
            _heroIds[fromSlot] = displacedHeroId;
            return true;
        }

        public bool TryRemove(int slot, out string removedHeroId)
        {
            removedHeroId = null;
            if (!IsValidSlot(slot)) return false;
            EnsureStorage();
            if (string.IsNullOrEmpty(_heroIds[slot])) return false;
            removedHeroId = _heroIds[slot];
            _heroIds[slot] = null;
            return true;
        }

        public string[] ToArray()
        {
            EnsureStorage();
            var copy = new string[SlotCount];
            Array.Copy(_heroIds, copy, SlotCount);
            return copy;
        }

        // Restore only owned heroes and discard duplicates or invalid references.
        public void LoadSlots(string[] savedSlots, Roster roster)
        {
            _heroIds = new string[SlotCount];
            if (savedSlots == null || roster == null) return;

            int count = Math.Min(savedSlots.Length, SlotCount);
            for (int slot = 0; slot < count; slot++)
            {
                string heroId = savedSlots[slot];
                if (string.IsNullOrEmpty(heroId) || roster.Find(heroId) == null) continue;

                bool alreadyAssigned = false;
                for (int previous = 0; previous < slot; previous++)
                    if (_heroIds[previous] == heroId) { alreadyAssigned = true; break; }

                if (!alreadyAssigned) _heroIds[slot] = heroId;
            }
        }

        private void EnsureStorage()
        {
            if (_heroIds != null && _heroIds.Length == SlotCount) return;
            var normalized = new string[SlotCount];
            if (_heroIds != null)
                Array.Copy(_heroIds, normalized, Math.Min(_heroIds.Length, SlotCount));
            _heroIds = normalized;
        }
    }
}
