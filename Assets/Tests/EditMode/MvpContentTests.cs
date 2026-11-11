using NUnit.Framework;
using SandsOfLegends.Combat;
using SandsOfLegends.Core;
using SandsOfLegends.Meta;

namespace SandsOfLegends.Tests
{
    public sealed class MvpContentTests
    {
        [Test]
        public void HeroCatalogContainsTwentyHeroesAcrossAllRarities()
        {
            Assert.AreEqual(20, HeroCatalog.All.Count);
            var pool = HeroCatalog.BuildGachaPool();
            Assert.AreEqual(20, pool[Rarity.Common].Count + pool[Rarity.Rare].Count +
                pool[Rarity.Epic].Count + pool[Rarity.Legendary].Count);
            Assert.AreEqual(7, pool[Rarity.Common].Count);
            Assert.AreEqual(7, pool[Rarity.Rare].Count);
            Assert.AreEqual(3, pool[Rarity.Epic].Count);
            Assert.AreEqual(3, pool[Rarity.Legendary].Count);
        }

        [Test]
        public void CampaignGeneratesAllTwoHundredStagesAcrossFiveBiomes()
        {
            Assert.AreEqual(200, CampaignProgress.MaxStage);
            Assert.AreEqual(5, CampaignContent.Biomes.Length);
            for (int stage = 1; stage <= CampaignProgress.MaxStage; stage++)
            {
                CampaignStageData data = CampaignContent.Get(stage);
                Assert.AreEqual(stage, data.Number);
                Assert.IsNotNull(data.Biome);
                Assert.AreEqual(5, CampaignContent.BuildEnemies(stage).Length);
            }
            Assert.IsTrue(CampaignContent.Get(10).IsBoss);
            Assert.IsTrue(CampaignContent.Get(200).IsBoss);
        }

        [Test]
        public void StarterFormationSurvivesSaveRoundTrip()
        {
            GameState state = GameState.NewGame(1000);
            string[] starters = HeroCatalog.StartingHeroes;
            for (int i = 0; i < starters.Length; i++)
            {
                int shards;
                state.Roster.AddHeroOrShards(starters[i], HeroCatalog.Get(starters[i]).Rarity, out shards);
                state.Roster.SetFormation(i, starters[i]);
            }
            state.Equipment.Equip("warrior", EquipSlot.Weapon, "starter_blade");
            GameState restored = GameState.FromSave(state.ToSave(2000));
            CollectionAssert.AreEqual(starters, restored.Roster.GetFormation());
            Assert.AreEqual("starter_blade", restored.Equipment.Find("warrior", EquipSlot.Weapon).ItemId);
        }

        [Test]
        public void AssigningAnAlreadySelectedHeroSwapsFormationSlots()
        {
            GameState state = GameState.NewGame(1000);
            string[] starters = HeroCatalog.StartingHeroes;
            for (int i = 0; i < starters.Length; i++)
            {
                int shards;
                state.Roster.AddHeroOrShards(starters[i], HeroCatalog.Get(starters[i]).Rarity, out shards);
                state.Roster.SetFormation(i, starters[i]);
            }

            state.Roster.SetFormation(1, starters[0]);
            string[] formation = state.Roster.GetFormation();
            Assert.AreEqual(starters[1], formation[0]);
            Assert.AreEqual(starters[0], formation[1]);
            CollectionAssert.AllItemsAreUnique(formation);
        }

        [Test]
        public void HeroStarUpAndAscensionConsumeShardsAndMaterials()
        {
            GameState state = GameState.NewGame(1000);
            int shards;
            state.Roster.AddHeroOrShards("warrior", Rarity.Common, out shards);
            HeroInstance hero = state.Roster.Find("warrior");
            hero.Shards = 30;
            state.Wallet.Set(Currency.Gold, 10000);
            state.Wallet.Set(Currency.GearMaterials, 100);

            Assert.IsTrue(state.Roster.TryStarUp("warrior"));
            Assert.AreEqual(1, hero.Stars);
            Assert.IsTrue(state.Roster.TryAscend("warrior", state.Wallet));
            Assert.AreEqual(1, hero.Ascension);
            Assert.AreEqual(40, Progression.LevelCap(hero.Ascension));
            Assert.AreEqual(20, hero.Shards);
        }

        [Test]
        public void LevelUpAndEquipmentUpgradeSpendTheirResources()
        {
            GameState state = GameState.NewGame(1000);
            int shards;
            state.Roster.AddHeroOrShards("warrior", Rarity.Common, out shards);
            state.Wallet.Set(Currency.Gold, 1000);
            state.Wallet.Set(Currency.Exp, 1000);
            state.Wallet.Set(Currency.GearMaterials, 20);
            Assert.IsTrue(state.Roster.TryLevelUp("warrior", state.Wallet));
            Assert.AreEqual(2, state.Roster.Find("warrior").Level);
            state.Equipment.Equip("warrior", EquipSlot.Weapon, "starter_blade");
            Assert.IsTrue(state.Equipment.TryUpgrade("warrior", EquipSlot.Weapon, state.Wallet));
            Assert.AreEqual(2, state.Equipment.Find("warrior", EquipSlot.Weapon).Level);
        }
    }
}
