// =============================================================================
// FILE: Assets/Scripts/Meta/Idle/IdleRewards.cs
// OWNER ROLE: Economy Designer
// RESPONSIBILITY: Offline/idle rewards: accumulate up to 12h, rate depends on the highest cleared stage (GDD 4.1).
// WORK ON: Replace placeholder rate formula with the economy ledger; add multi-site farming (resource nodes).
// STATUS: Implemented (placeholder rates)
// =============================================================================
using System;

namespace SandsOfLegends.Meta
{
    public static class IdleRewards
    {
        public const int CapSeconds = 12 * 3600;

        public struct Reward { public long Gold; public long Exp; public int Seconds; }

        public static long GoldPerMinute(int stage) { return 10 + 2L * stage; }
        public static long ExpPerMinute(int stage) { return 5 + 1L * stage; }

        public static Reward Calculate(long lastClaimUnix, long nowUnix, int highestStage)
        {
            long sec = Math.Max(0, Math.Min(CapSeconds, nowUnix - lastClaimUnix));
            long min = sec / 60;
            return new Reward { Seconds = (int)sec, Gold = min * GoldPerMinute(highestStage), Exp = min * ExpPerMinute(highestStage) };
        }
    }
}
