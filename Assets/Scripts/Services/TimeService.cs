// =============================================================================
// FILE: Assets/Scripts/Services/TimeService.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Single source of 'now' (unix seconds, day key). Anti-cheat: later replaced/validated by server time.
// WORK ON: Server time sync, detect device clock tampering.
// STATUS: Implemented
// =============================================================================
using System;

namespace SandsOfLegends.Services
{
    public static class TimeService
    {
        public static long NowUnix { get { return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); } }
        public static string DayKey { get { return DateTime.UtcNow.ToString("yyyyMMdd"); } }
    }
}
