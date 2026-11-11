// =============================================================================
// FILE: Assets/Scripts/Localization/Localizer.cs
// OWNER ROLE: Client Programmer - UI
// RESPONSIBILITY: Arabic + English text lookup with RTL flag (Arabic first-class from day one, GDD 3.4).
// WORK ON: Load tables from Resources/Localization, Arabic text shaping/RTL for TextMeshPro, font fallback, no hardcoded UI strings.
// STATUS: Implemented (in-memory)
// =============================================================================
using System.Collections.Generic;

namespace SandsOfLegends.Localization
{
    public enum Language { Arabic, English }

    public static class Localizer
    {
        private static readonly Dictionary<string, string> _ar = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _en = new Dictionary<string, string>();

        public static Language Current = Language.Arabic;
        public static bool IsRtl { get { return Current == Language.Arabic; } }

        public static void Register(string key, string arabic, string english)
        {
            _ar[key] = arabic; _en[key] = english;
        }

        public static string T(string key)
        {
            var d = Current == Language.Arabic ? _ar : _en;
            string v;
            return d.TryGetValue(key, out v) ? v : key;
        }
    }
}
