// =============================================================================
// FILE: Assets/Scripts/UI/Screens/SummonScreen.cs
// OWNER ROLE: Client Programmer - UI
// RESPONSIBILITY: Gacha UI: single/x10 pull, pity counters, official rates screen, results animation.
// WORK ON: Call GachaSystem (later the server); show pity.
// STATUS: Stub with hero result portrait binding
// =============================================================================
using SandsOfLegends.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SandsOfLegends.UI
{
    public sealed class SummonScreen : UIScreen
    {
        public void BindSummonResultPortrait(Image target, HeroDefinition hero)
        {
            if (target == null || hero == null) return;
            target.sprite = hero.portrait;
            target.preserveAspect = true;
        }
    }
}
