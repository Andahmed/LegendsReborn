using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SandsOfLegends.Combat;
using UnityEngine.TestTools;

namespace SandsOfLegends.Tests
{
    public sealed class MvpBattlePlayModeTests
    {
        private static UnitSpec Unit(string id, Faction faction, int attack)
        {
            return new UnitSpec
            {
                Id = id,
                Name = id,
                Faction = faction,
                Role = Role.Dps,
                Hp = 1200,
                Atk = attack,
                Def = 55,
                Spd = 100,
                CritChance = .05f
            };
        }

        [UnityTest]
        public IEnumerator FiveAgainstFiveSimulation_ReachesAValidResult()
        {
            IList<UnitSpec> attackers = new[]
            {
                Unit("a0", Faction.Fortress, 112),
                Unit("a1", Faction.Wild, 108),
                Unit("a2", Faction.Forest, 104),
                Unit("a3", Faction.Shadow, 116),
                Unit("a4", Faction.Fortress, 110)
            };
            IList<UnitSpec> defenders = new[]
            {
                Unit("d0", Faction.Wild, 107),
                Unit("d1", Faction.Forest, 109),
                Unit("d2", Faction.Shadow, 105),
                Unit("d3", Faction.Fortress, 111),
                Unit("d4", Faction.Wild, 106)
            };

            BattleSim battle = new BattleSim(attackers, defenders, 20261111);
            Assert.AreEqual(10, battle.Units.Count);

            int maxSteps = (int)(BattleSim.MaxTime / BattleSim.Dt) + 2;
            for (int i = 0; i < maxSteps && !battle.Finished; i++)
            {
                battle.Step();
                if (i % 100 == 0) yield return null;
            }

            Assert.IsTrue(battle.Finished, "The 5v5 battle must reach its terminal state before the timeout.");
            Assert.That(battle.Winner, Is.EqualTo(0).Or.EqualTo(1));
        }
    }
}
