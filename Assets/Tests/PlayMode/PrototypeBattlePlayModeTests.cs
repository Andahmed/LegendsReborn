// =============================================================================
// FILE: Assets/Tests/PlayMode/PrototypeBattlePlayModeTests.cs
// OWNER ROLE: QA Engineer
// RESPONSIBILITY: PlayMode smoke test proving a deterministic 4v4 BattleSim reaches a finished state.
// WORK ON: Extend runtime smoke coverage when the battle presentation and scene flow gain features.
// STATUS: Implemented
// =============================================================================
using System.Collections;
using NUnit.Framework;
using SandsOfLegends.Combat;
using UnityEngine.TestTools;

namespace SandsOfLegends.Tests
{
    public class PrototypeBattlePlayModeTests
    {
        private static UnitSpec Hero(string id, Faction faction)
        {
            return new UnitSpec
            {
                Id = id,
                Name = id,
                Faction = faction,
                Role = Role.Dps,
                Hp = 900,
                Atk = 180,
                Def = 45,
                Spd = 100,
                CritChance = 0f
            };
        }

        [UnityTest]
        public IEnumerator FourHeroesPerTeam_BattleFinishes()
        {
            var playerTeam = new[]
            {
                Hero("Aren", Faction.Fortress),
                Hero("Mira", Faction.Wild),
                Hero("Thalen", Faction.Forest),
                Hero("Neris", Faction.Shadow)
            };
            var enemyTeam = new[]
            {
                Hero("Echo_Aren", Faction.Fortress),
                Hero("Echo_Mira", Faction.Wild),
                Hero("Echo_Thalen", Faction.Forest),
                Hero("Echo_Neris", Faction.Shadow)
            };

            var battle = new BattleSim(playerTeam, enemyTeam, 20261111);
            int maxSteps = (int)(BattleSim.MaxTime / BattleSim.Dt) + 2;

            for (int step = 0; step < maxSteps && !battle.Finished; step++)
            {
                battle.Step();
                if (step % 100 == 0) yield return null;
            }

            Assert.IsTrue(battle.Finished, "The 4v4 prototype battle must finish before the simulation timeout.");
            Assert.That(battle.Winner, Is.EqualTo(0).Or.EqualTo(1));
        }
    }
}
