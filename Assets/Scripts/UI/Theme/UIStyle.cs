// =============================================================================
// FILE: Assets/Scripts/UI/Theme/UIStyle.cs
// OWNER ROLE: Concept/UI Artist
// RESPONSIBILITY: Single source of UI colors/spacing from the Style Guide (warm sand/gold/turquoise, Arabic geometric motifs).
// WORK ON: Replace placeholder palette with the approved Style Guide values (docs/STYLE_GUIDE.md).
// STATUS: Implemented (placeholder palette)
// =============================================================================
using UnityEngine;

namespace SandsOfLegends.UI
{
    public static class UIStyle
    {
        public static readonly Color Sand = new Color(0.86f, 0.74f, 0.55f);
        public static readonly Color Gold = new Color(0.93f, 0.76f, 0.25f);
        public static readonly Color Turquoise = new Color(0.16f, 0.65f, 0.65f);
        public static readonly Color Night = new Color(0.10f, 0.12f, 0.18f);
        public static readonly Color Crit = new Color(1f, 0.45f, 0.2f);
        public static readonly Color Heal = new Color(0.4f, 0.9f, 0.5f);
    }
}
