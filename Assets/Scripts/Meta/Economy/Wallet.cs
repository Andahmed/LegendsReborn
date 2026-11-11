// =============================================================================
// FILE: Assets/Scripts/Meta/Economy/Wallet.cs
// OWNER ROLE: Economy Designer
// RESPONSIBILITY: Player currencies (Gems, Gold, Exp, GearMaterials): add/spend with validation and change event.
// WORK ON: Hook analytics (source/sink tracking per GDD 4.7). Server-authoritative validation later.
// STATUS: Implemented
// =============================================================================
using System;

namespace SandsOfLegends.Meta
{
    public enum Currency { Gems, Gold, Exp, GearMaterials }

    public sealed class Wallet
    {
        private readonly long[] _v = new long[4];
        public event Action<Currency, long> Changed;

        public long Get(Currency c) { return _v[(int)c]; }

        public void Add(Currency c, long amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException("amount");
            Set(c, Math.Min(long.MaxValue / 2, _v[(int)c] + amount));
        }

        public bool TrySpend(Currency c, long amount)
        {
            if (amount < 0 || _v[(int)c] < amount) return false;
            Set(c, _v[(int)c] - amount);
            return true;
        }

        public void Set(Currency c, long value)
        {
            _v[(int)c] = value;
            if (Changed != null) Changed(c, value);
        }
    }
}
