// =============================================================================
// FILE: Assets/Scripts/Meta/Economy/EnergyState.cs
// OWNER ROLE: Economy Designer
// RESPONSIBILITY: Energy (stamina): cap 120, +1 per 6 minutes, consumed by farming/events only (GDD 4.1). Time is injected (unix seconds).
// WORK ON: Tune cap/regen; add purchase refill; server time validation.
// STATUS: Implemented
// =============================================================================
using System;

namespace SandsOfLegends.Meta
{
    public sealed class EnergyState
    {
        public const int Cap = 120;
        public const int SecondsPerPoint = 360;

        public int Value;
        public long LastTickUnix;

        public void Refresh(long nowUnix)
        {
            if (Value >= Cap) { Value = Cap; LastTickUnix = nowUnix; return; }
            long gained = (nowUnix - LastTickUnix) / SecondsPerPoint;
            if (gained <= 0) return;
            Value = (int)Math.Min(Cap, Value + gained);
            LastTickUnix = Value >= Cap ? nowUnix : LastTickUnix + gained * SecondsPerPoint;
        }

        public bool TryConsume(int amount, long nowUnix)
        {
            Refresh(nowUnix);
            if (amount < 0 || Value < amount) return false;
            if (Value >= Cap) LastTickUnix = nowUnix;   // regen starts when leaving the cap
            Value -= amount;
            return true;
        }
    }
}
