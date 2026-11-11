// =============================================================================
// FILE: Assets/Scripts/Core/TeamFormationBuilder.cs
// OWNER ROLE: Client Programmer - Combat
// RESPONSIBILITY: Convert the saved owned-hero formation into ordered engine-free UnitSpecs.
// WORK ON: Connect the resulting player team to BattleSim when the campaign battle launches.
// STATUS: Implemented; missing hero definitions fail with a clear error instead of silently shrinking the team.
// =============================================================================
using System;
using System.Collections.Generic;
using SandsOfLegends.Combat;
using SandsOfLegends.Meta;

namespace SandsOfLegends.Core
{
    public static class TeamFormationBuilder
    {
        public static bool TryBuildPlayerTeam(
            Roster roster,
            IList<HeroDefinition> definitions,
            out UnitSpec[] team,
            out string error)
        {
            team = new UnitSpec[TeamFormation.SlotCount];
            error = null;

            if (roster == null)
            {
                error = "Player roster is missing.";
                return false;
            }
            if (definitions == null)
            {
                error = "Hero definitions are missing.";
                return false;
            }

            int memberCount = 0;
            for (int slot = 0; slot < TeamFormation.SlotCount; slot++)
            {
                string heroId = roster.Formation.GetHeroId(slot);
                if (string.IsNullOrEmpty(heroId)) continue;

                HeroInstance ownedHero = roster.Find(heroId);
                if (ownedHero == null)
                {
                    error = "Formation references a hero that is not owned: " + heroId;
                    team = new UnitSpec[TeamFormation.SlotCount];
                    return false;
                }

                HeroDefinition definition = FindDefinition(definitions, heroId);
                if (definition == null)
                {
                    error = "Hero definition is missing for: " + heroId;
                    team = new UnitSpec[TeamFormation.SlotCount];
                    return false;
                }

                team[slot] = definition.ToSpec(Math.Max(1, ownedHero.Level));
                memberCount++;
            }

            if (memberCount == 0)
            {
                error = "Assign at least one hero before starting a battle.";
                return false;
            }

            return true;
        }

        private static HeroDefinition FindDefinition(IList<HeroDefinition> definitions, string heroId)
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                HeroDefinition definition = definitions[i];
                if (definition == null) continue;
                string definitionId = string.IsNullOrEmpty(definition.id) ? definition.name : definition.id;
                if (definitionId == heroId) return definition;
            }
            return null;
        }
    }
}
