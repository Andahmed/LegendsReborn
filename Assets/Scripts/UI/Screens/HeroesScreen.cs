// =============================================================================
// FILE: Assets/Scripts/UI/Screens/HeroesScreen.cs
// OWNER ROLE: Client Programmer - UI
// RESPONSIBILITY: Hero list/details: level, stars, ascension, equipment, team formation (front/back).
// WORK ON: Hook to Roster/Progression; formation drag-drop.
// STATUS: Stub with portrait binding
// =============================================================================
using SandsOfLegends.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SandsOfLegends.UI
{
    public sealed class HeroesScreen : UIScreen
    {
        public void BindHeroPortrait(Image target, HeroDefinition hero)
        {
            if (target == null || hero == null) return;
            target.sprite = hero.portrait;
            target.preserveAspect = true;
        }

        public void BindFactionIcon(Image target, HeroDefinition hero)
        {
            if (target == null || hero == null) return;
            target.sprite = hero.factionIcon;
            target.preserveAspect = true;
        }
    }
}
