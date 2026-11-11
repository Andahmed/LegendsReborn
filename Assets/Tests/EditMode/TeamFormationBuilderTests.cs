// =============================================================================
// FILE: Assets/Tests/EditMode/TeamFormationBuilderTests.cs
// OWNER ROLE: QA Engineer
// RESPONSIBILITY: Verify the Core bridge from persistent roster formation to battle UnitSpecs.
// WORK ON: Add regressions when hero data or battle team composition changes.
// STATUS: Slot order, progression level scaling and missing-definition handling covered.
// =============================================================================
using System.Collections.Generic;
using NUnit.Framework;
using SandsOfLegends.Combat;
using SandsOfLegends.Core;
using SandsOfLegends.Meta;
using UnityEngine;

namespace SandsOfLegends.Tests
{
    public sealed class TeamFormationBuilderTests
    {
        [Test]
        public void Builder_PreservesSlotOrderAndUsesOwnedHeroLevel()
        {
            HeroDefinition front = ScriptableObject.CreateInstance<HeroDefinition>();
            HeroDefinition back = ScriptableObject.CreateInstance<HeroDefinition>();
            try
            {
                front.id = "front";
                front.displayName = "Front Hero";
                front.hp = 100;
                front.atk = 50;
                front.def = 20;
                front.growth = 0.1f;

                back.id = "back";
                back.displayName = "Back Hero";
                back.hp = 200;

                var roster = new Roster();
                int shards;
                roster.AddHeroOrShards("front", Rarity.Epic, out shards);
                roster.AddHeroOrShards("back", Rarity.Rare, out shards);
                roster.Find("front").Level = 5;

                string replaced;
                Assert.IsTrue(roster.TryAssignFormationSlot(0, "front", out replaced));
                Assert.IsTrue(roster.TryAssignFormationSlot(4, "back", out replaced));

                UnitSpec[] team;
                string error;
                bool built = TeamFormationBuilder.TryBuildPlayerTeam(
                    roster, new List<HeroDefinition> { front, back }, out team, out error);

                Assert.IsTrue(built, error);
                Assert.AreEqual(TeamFormation.SlotCount, team.Length);
                Assert.AreEqual("front", team[0].Id);
                Assert.AreEqual(140, team[0].Hp);
                Assert.IsNull(team[1]);
                Assert.IsNull(team[3]);
                Assert.AreEqual("back", team[4].Id);
            }
            finally
            {
                Object.DestroyImmediate(front);
                Object.DestroyImmediate(back);
            }
        }

        [Test]
        public void Builder_FailsClearlyWhenSelectedHeroDefinitionIsMissing()
        {
            var roster = new Roster();
            int shards;
            roster.AddHeroOrShards("not-loaded", Rarity.Common, out shards);
            string replaced;
            Assert.IsTrue(roster.TryAssignFormationSlot(2, "not-loaded", out replaced));

            UnitSpec[] team;
            string error;
            bool built = TeamFormationBuilder.TryBuildPlayerTeam(
                roster, new List<HeroDefinition>(), out team, out error);

            Assert.IsFalse(built);
            StringAssert.Contains("not-loaded", error);
            Assert.AreEqual(TeamFormation.SlotCount, team.Length);
            for (int i = 0; i < team.Length; i++) Assert.IsNull(team[i]);
        }
    }
}
