// =============================================================================
// FILE: Assets/Tests/EditMode/MetaTests.cs
// OWNER ROLE: QA Engineer
// RESPONSIBILITY: EditMode tests for economy, progression, equipment, campaign, gacha and save migrations.
// WORK ON: Add regression coverage for every gameplay rule and persistence change.
// STATUS: Existing tests retained; equipment and version-1 migration tests added.
// =============================================================================
using System.Collections.Generic;
using NUnit.Framework;
using SandsOfLegends.Combat;
using SandsOfLegends.Meta;

namespace SandsOfLegends.Tests
{
    public class MetaTests
    {
        private static Dictionary<Rarity, List<string>> Pool()
        {
            return new Dictionary<Rarity, List<string>>
            {
                { Rarity.Common, new List<string> { "c1", "c2" } },
                { Rarity.Rare, new List<string> { "r1", "r2" } },
                { Rarity.Epic, new List<string> { "e1", "e2" } },
                { Rarity.Legendary, new List<string> { "l1" } },
            };
        }

        [Test]
        public void Gacha_LegendaryGuaranteedWithin90()
        {
            var pity = new PityState(); var roster = new Roster(); var rng = new SeededRng(7);
            bool got = false;
            for (int i = 0; i < GachaSystem.LegendaryPity && !got; i++)
                got = GachaSystem.Pull(Pool(), pity, rng, roster).Rarity == Rarity.Legendary;
            Assert.IsTrue(got);
        }

        [Test]
        public void Gacha_EpicOrBetterWithin30()
        {
            var pity = new PityState(); var roster = new Roster(); var rng = new SeededRng(11);
            bool got = false;
            for (int i = 0; i < GachaSystem.EpicPity && !got; i++)
                got = GachaSystem.Pull(Pool(), pity, rng, roster).Rarity >= Rarity.Epic;
            Assert.IsTrue(got);
        }

        [Test]
        public void Roster_DuplicateBecomesShards()
        {
            var r = new Roster(); int sh;
            Assert.IsTrue(r.AddHeroOrShards("h", Rarity.Rare, out sh));
            Assert.IsFalse(r.AddHeroOrShards("h", Rarity.Rare, out sh));
            Assert.AreEqual(Progression.DuplicateShards(Rarity.Rare), r.Find("h").Shards);
        }

        [Test]
        public void Roster_NewHeroStartsAtCeilingMinus5()
        {
            var r = new Roster(); int sh;
            r.AddHeroOrShards("a", Rarity.Epic, out sh);
            r.Find("a").Level = 30;
            r.AddHeroOrShards("b", Rarity.Epic, out sh);
            Assert.AreEqual(25, r.Find("b").Level);
        }

        [Test]
        public void Equipment_EquipReplacesOnlyMatchingSlot()
        {
            var hero = new HeroInstance { HeroId = "hero" };
            EquipmentItem replaced;
            Assert.IsTrue(EquipmentRules.TryEquip(hero, new EquipmentItem { Id = "sword-a", Slot = EquipSlot.Weapon }, out replaced));
            Assert.IsNull(replaced);
            Assert.IsTrue(EquipmentRules.TryEquip(hero, new EquipmentItem { Id = "sword-b", Slot = EquipSlot.Weapon }, out replaced));
            Assert.AreEqual("sword-a", replaced.Id);
            Assert.AreEqual(1, hero.Equipment.Count);
            Assert.AreEqual("sword-b", EquipmentRules.Find(hero.Equipment, EquipSlot.Weapon).Id);
        }

        [Test]
        public void Equipment_RejectsSameItemTwiceAndUnequips()
        {
            var hero = new HeroInstance { HeroId = "hero" };
            var item = new EquipmentItem { Id = "ring-a", Slot = EquipSlot.Ring };
            EquipmentItem result;
            Assert.IsTrue(EquipmentRules.TryEquip(hero, item, out result));
            Assert.IsFalse(EquipmentRules.TryEquip(hero, item, out result));
            Assert.IsTrue(EquipmentRules.TryUnequip(hero, EquipSlot.Ring, out result));
            Assert.AreEqual("ring-a", result.Id);
            Assert.IsFalse(EquipmentRules.TryUnequip(hero, EquipSlot.Ring, out result));
        }

        [Test]
        public void Equipment_SaveRoundTripAndVersionOneMigration()
        {
            var old = new SaveData { Version = 1 };
            old.Heroes.Add(new HeroInstance { HeroId = "legacy", Equipment = null });
            var migrated = SaveMigrations.Migrate(old);
            Assert.AreEqual(2, migrated.Version);
            Assert.IsNotNull(migrated.Heroes[0].Equipment);

            var state = GameState.NewGame(1000);
            int shards;
            state.Roster.AddHeroOrShards("equipped", Rarity.Epic, out shards);
            EquipmentItem replaced;
            state.Roster.TryEquip("equipped", new EquipmentItem { Id = "helm-1", Slot = EquipSlot.Helmet, SetId = "sand" }, out replaced);
            var loaded = GameState.FromSave(state.ToSave(2000));
            Assert.AreEqual("helm-1", loaded.Roster.Find("equipped").Equipment[0].Id);
            Assert.AreEqual(2, loaded.ToSave(3000).Version);
        }

        [Test]
        public void Energy_RegeneratesAndCaps()
        {
            var e = new EnergyState { Value = 100, LastTickUnix = 0 };
            e.Refresh(360 * 5);
            Assert.AreEqual(105, e.Value);
            e.Refresh(360 * 1000);
            Assert.AreEqual(EnergyState.Cap, e.Value);
        }

        [Test]
        public void Energy_ConsumeFailsWhenShort()
        {
            var e = new EnergyState { Value = 5, LastTickUnix = 0 };
            Assert.IsFalse(e.TryConsume(10, 0));
            Assert.IsTrue(e.TryConsume(5, 0));
        }

        [Test]
        public void Idle_CapsAt12Hours()
        {
            var r = IdleRewards.Calculate(0, 100 * 3600, 10);
            Assert.AreEqual(IdleRewards.CapSeconds, r.Seconds);
            Assert.AreEqual(12 * 60 * IdleRewards.GoldPerMinute(10), r.Gold);
        }

        [Test]
        public void Campaign_ClearsInOrderOnly()
        {
            var c = new CampaignProgress();
            Assert.IsFalse(c.TryClear(2));
            Assert.IsTrue(c.TryClear(1));
            Assert.IsTrue(c.TryClear(2));
            Assert.AreEqual(0, CampaignProgress.BiomeOf(40));
            Assert.AreEqual(1, CampaignProgress.BiomeOf(41));
        }

        [Test]
        public void SaveRoundTrip_KeepsState()
        {
            var s = GameState.NewGame(1000);
            s.Wallet.Add(Currency.Gold, 500);
            s.Campaign.TryClear(1);
            int sh; s.Roster.AddHeroOrShards("h", Rarity.Epic, out sh);
            var back = GameState.FromSave(s.ToSave(2000));
            Assert.AreEqual(500, back.Wallet.Get(Currency.Gold));
            Assert.AreEqual(1, back.Campaign.HighestCleared);
            Assert.IsNotNull(back.Roster.Find("h"));
        }

        [Test]
        public void DailyQuests_ClaimOnlyWhenComplete()
        {
            var q = new DailyQuests(); q.ResetIfNewDay("20261111"); var w = new Wallet();
            Assert.IsFalse(q.TryClaim(2, w));
            q.Report("summon_1", 1);
            Assert.IsTrue(q.TryClaim(2, w));
            Assert.IsFalse(q.TryClaim(2, w));
            Assert.AreEqual(DailyQuests.RewardGemsEach, w.Get(Currency.Gems));
        }
    }
}
