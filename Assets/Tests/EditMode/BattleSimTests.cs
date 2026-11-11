using NUnit.Framework;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Tests
{
    public class BattleSimTests
    {
        private static UnitSpec U(string id, Faction f, int hp = 1000, int atk = 100, int def = 50, int spd = 100)
        {
            return new UnitSpec { Id = id, Name = id, Faction = f, Hp = hp, Atk = atk, Def = def, Spd = spd, CritChance = 0f };
        }

        [Test]
        public void FactionCycle_IsCorrect()
        {
            Assert.IsTrue(FactionChart.HasAdvantage(Faction.Fortress, Faction.Wild));
            Assert.IsTrue(FactionChart.HasAdvantage(Faction.Wild, Faction.Forest));
            Assert.IsTrue(FactionChart.HasAdvantage(Faction.Forest, Faction.Shadow));
            Assert.IsTrue(FactionChart.HasAdvantage(Faction.Shadow, Faction.Fortress));
            Assert.IsTrue(FactionChart.HasAdvantage(Faction.Divinity, Faction.Abyss));
            Assert.IsTrue(FactionChart.HasAdvantage(Faction.Abyss, Faction.Divinity));
            Assert.IsFalse(FactionChart.HasAdvantage(Faction.Wild, Faction.Fortress));
            Assert.AreEqual(1.25f, FactionChart.Multiplier(Faction.Fortress, Faction.Wild), 1e-5f);
        }

        [Test]
        public void SameSeed_GivesIdenticalBattle()
        {
            var a = new[] { U("a1", Faction.Fortress), U("a2", Faction.Wild), U("a3", Faction.Forest) };
            var b = new[] { U("b1", Faction.Shadow), U("b2", Faction.Wild), U("b3", Faction.Forest) };
            var s1 = new BattleSim(a, b, 42); s1.Run();
            var s2 = new BattleSim(a, b, 42); s2.Run();
            Assert.AreEqual(s1.Winner, s2.Winner);
            Assert.AreEqual(s1.Events.Count, s2.Events.Count);
            Assert.AreEqual(s1.Time, s2.Time, 1e-5f);
        }

        [Test]
        public void StrongerTeam_Wins()
        {
            var a = new[] { U("a1", Faction.Fortress, 5000, 400), U("a2", Faction.Fortress, 5000, 400) };
            var b = new[] { U("b1", Faction.Wild, 500, 50), U("b2", Faction.Wild, 500, 50) };
            var s = new BattleSim(a, b, 1); s.Run();
            Assert.AreEqual(0, s.Winner);
        }

        [Test]
        public void FrontRow_IsTargetedFirst()
        {
            var a = new[] { U("a1", Faction.Fortress, 1000, 100) };
            var b = new[] { U("front", Faction.Wild, 100000), U("front2", Faction.Wild, 100000), U("back", Faction.Wild, 100) };
            var s = new BattleSim(a, b, 1);
            for (int i = 0; i < 10; i++) s.Step();
            Assert.IsTrue(s.Get(1, 2).Alive, "back row must be untouched while front row lives");
            Assert.Less(s.Get(1, 0).Hp, s.Get(1, 0).MaxHp);
        }

        [Test]
        public void Timeout_GivesDefenderWin()
        {
            var a = new[] { U("a", Faction.Fortress, 1000000, 1) };
            var b = new[] { U("b", Faction.Wild, 1000000, 1, 100000) };
            var s = new BattleSim(a, b, 1); s.Run();
            Assert.AreEqual(1, s.Winner);
            Assert.GreaterOrEqual(s.Time, BattleSim.MaxTime - 0.2f);
        }

        [Test]
        public void Ultimate_FiresAtFullEnergy_AndHealCapsAtMaxHp()
        {
            var healer = U("h", Faction.Forest, 1000, 100);
            healer.Ultimate = new SkillSpec { Id = "u", Power = 5f, Target = TargetType.AllAllies, IsHeal = true };
            var a = new[] { healer, U("ally", Faction.Forest, 1000, 10) };
            var b = new[] { U("e", Faction.Shadow, 100000, 1) };
            var s = new BattleSim(a, b, 3);
            for (int i = 0; i < 300 && !s.Events.Exists(e => e.Type == BattleEventType.Heal); i++) s.Step();
            Assert.IsTrue(s.Events.Exists(e => e.Type == BattleEventType.Heal));
            foreach (var u in s.Units) Assert.LessOrEqual(u.Hp, u.MaxHp);
        }
    }
}
