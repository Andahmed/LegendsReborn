// =============================================================================
// FILE: Assets/Tests/EditMode/MetaTests.cs
// OWNER ROLE: QA Engineer
// RESPONSIBILITY: EditMode tests for meta systems: gacha pity, energy regen, idle cap, campaign order, roster duplicates, save round-trip.
// WORK ON: Add a test whenever a Meta rule changes.
// STATUS: Implemented
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

        [Test]
        public void Gacha_GuaranteedEpicResetsEpicPityAndAdvancesLegendaryPity()
        {
            var pool = new Dictionary<Rarity, List<string>>
            {
                { Rarity.Epic, new List<string> { "epic_only" } }
            };
            var pity = new PityState { SinceEpic = GachaSystem.EpicPity - 1, SinceLegendary = 8 };
            PullResult result = GachaSystem.Pull(pool, pity, new SeededRng(31), new Roster());

            Assert.AreEqual(Rarity.Epic, result.Rarity);
            Assert.AreEqual(0, pity.SinceEpic);
            Assert.AreEqual(9, pity.SinceLegendary);
        }

        [Test]
        public void Gacha_GuaranteedLegendaryResetsBothPityCounters()
        {
            var pool = new Dictionary<Rarity, List<string>>
            {
                { Rarity.Legendary, new List<string> { "legendary_only" } }
            };
            var pity = new PityState { SinceEpic = 22, SinceLegendary = GachaSystem.LegendaryPity - 1 };
            PullResult result = GachaSystem.Pull(pool, pity, new SeededRng(91), new Roster());

            Assert.AreEqual(Rarity.Legendary, result.Rarity);
            Assert.AreEqual(0, pity.SinceEpic);
            Assert.AreEqual(0, pity.SinceLegendary);
        }

        [Test]
        public void Gacha_SameSeedProducesSamePullSequenceAndPityState()
        {
            var firstPity = new PityState();
            var secondPity = new PityState();
            var firstRoster = new Roster();
            var secondRoster = new Roster();
            var firstRng = new SeededRng(20261112);
            var secondRng = new SeededRng(20261112);

            for (int i = 0; i < 100; i++)
            {
                PullResult first = GachaSystem.Pull(Pool(), firstPity, firstRng, firstRoster);
                PullResult second = GachaSystem.Pull(Pool(), secondPity, secondRng, secondRoster);
                Assert.AreEqual(first.Rarity, second.Rarity, "rarity at pull " + i);
                Assert.AreEqual(first.HeroId, second.HeroId, "hero at pull " + i);
                Assert.AreEqual(first.IsNew, second.IsNew, "duplicate state at pull " + i);
                Assert.AreEqual(first.ShardsGained, second.ShardsGained, "shards at pull " + i);
                Assert.AreEqual(firstPity.SinceEpic, secondPity.SinceEpic);
                Assert.AreEqual(firstPity.SinceLegendary, secondPity.SinceLegendary);
            }
        }

        [Test]
        public void Campaign_BossStagesAndBiomeBoundariesAreExact()
        {
            for (int stage = 1; stage <= CampaignProgress.MaxStage; stage++)
                Assert.AreEqual(stage % 10 == 0, CampaignProgress.IsBoss(stage), "boss flag at stage " + stage);

            Assert.IsFalse(CampaignProgress.IsBoss(0));
            Assert.IsFalse(CampaignProgress.IsBoss(201));
            Assert.AreEqual(0, CampaignProgress.BiomeOf(1));
            Assert.AreEqual(0, CampaignProgress.BiomeOf(40));
            Assert.AreEqual(1, CampaignProgress.BiomeOf(41));
            Assert.AreEqual(1, CampaignProgress.BiomeOf(80));
            Assert.AreEqual(2, CampaignProgress.BiomeOf(81));
            Assert.AreEqual(2, CampaignProgress.BiomeOf(120));
            Assert.AreEqual(3, CampaignProgress.BiomeOf(121));
            Assert.AreEqual(3, CampaignProgress.BiomeOf(160));
            Assert.AreEqual(4, CampaignProgress.BiomeOf(161));
            Assert.AreEqual(4, CampaignProgress.BiomeOf(200));
        }

        [Test]
        public void Campaign_ProgressUnlocksSequentiallyThroughStage200Only()
        {
            var campaign = new CampaignProgress();
            Assert.IsFalse(campaign.TryClear(0));
            for (int stage = 1; stage <= CampaignProgress.MaxStage; stage++)
            {
                Assert.IsTrue(campaign.TryClear(stage), "stage " + stage);
                Assert.AreEqual(stage, campaign.HighestCleared);
            }
            Assert.IsFalse(campaign.TryClear(200));
            Assert.IsFalse(campaign.TryClear(201));
            Assert.AreEqual(200, campaign.HighestCleared);
        }

        [Test]
        public void DailyQuests_NewDayResetsProgressAndClaimsButSameDayDoesNot()
        {
            var quests = new DailyQuests();
            var wallet = new Wallet();
            quests.ResetIfNewDay("2026-11-11");
            quests.Report("summon_1", 1);
            Assert.IsTrue(quests.TryClaim(2, wallet));
            Assert.AreEqual(DailyQuests.RewardGemsEach, wallet.Get(Currency.Gems));

            quests.ResetIfNewDay("2026-11-11");
            Assert.AreEqual(1, quests.Progress[2]);
            Assert.IsTrue(quests.Claimed[2]);
            Assert.IsFalse(quests.TryClaim(2, wallet));
            Assert.AreEqual(DailyQuests.RewardGemsEach, wallet.Get(Currency.Gems));

            quests.ResetIfNewDay("2026-11-12");
            Assert.AreEqual(0, quests.Progress[2]);
            Assert.IsFalse(quests.Claimed[2]);
            Assert.IsFalse(quests.TryClaim(2, wallet));
            Assert.AreEqual(DailyQuests.RewardGemsEach, wallet.Get(Currency.Gems));
        }

    }
}
