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
        public void CampaignBiomeLineups_AreDifferent()
        {
            string previousSignature = null;
            for (int biome = 0; biome < CampaignProgress.Biomes; biome++)
            {
                int stage = biome * CampaignProgress.StagesPerBiome + 1;
                UnitSpec[] enemies = CampaignContent.BuildEnemies(stage);
                string signature = "";
                for (int i = 0; i < enemies.Length; i++)
                    signature += enemies[i].Name + "|" + enemies[i].Faction + "|" + enemies[i].Role + ";";
                Assert.AreNotEqual(previousSignature, signature, "Biome " + biome + " must have a distinct lineup");
                previousSignature = signature;
            }
        }

        [Test]
        public void CampaignEnemyPower_DoesNotDecreaseInsideAnyBiome()
        {
            for (int biome = 0; biome < CampaignProgress.Biomes; biome++)
            {
                int first = biome * CampaignProgress.StagesPerBiome + 1;
                long previousPower = -1;
                for (int offset = 0; offset < CampaignProgress.StagesPerBiome; offset++)
                {
                    UnitSpec[] enemies = CampaignContent.BuildEnemies(first + offset);
                    long power = 0;
                    for (int i = 0; i < enemies.Length; i++)
                        power += enemies[i].Hp + enemies[i].Atk + enemies[i].Def;
                    Assert.GreaterOrEqual(power, previousPower, "Enemy power dropped at stage " + (first + offset));
                    previousPower = power;
                }
            }
        }

        [Test]
        public void CampaignEnemies_HaveSkillsAndAreDeterministic()
        {
            for (int stage = 1; stage <= CampaignProgress.MaxStage; stage++)
            {
                UnitSpec[] first = CampaignContent.BuildEnemies(stage);
                UnitSpec[] second = CampaignContent.BuildEnemies(stage);
                Assert.AreEqual(5, first.Length);
                for (int i = 0; i < first.Length; i++)
                {
                    Assert.IsTrue(first[i].Id.StartsWith("enemy_"));
                    Assert.IsNotNull(first[i].Skill, first[i].Id + " skill");
                    Assert.AreEqual(first[i].Id, second[i].Id);
                    Assert.AreEqual(first[i].Name, second[i].Name);
                    Assert.AreEqual(first[i].Faction, second[i].Faction);
                    Assert.AreEqual(first[i].Role, second[i].Role);
                    Assert.AreEqual(first[i].Hp, second[i].Hp);
                    Assert.AreEqual(first[i].Atk, second[i].Atk);
                    Assert.AreEqual(first[i].Def, second[i].Def);
                    Assert.AreEqual(first[i].Skill.Id, second[i].Skill.Id);
                    Assert.AreEqual(first[i].Skill.Power, second[i].Skill.Power);
                }
                bool boss = CampaignProgress.IsBoss(stage);
                Assert.AreEqual(boss, first[4].Ultimate != null, "Boss ultimate at stage " + stage);
                if (boss)
                {
                    Assert.AreEqual(TargetType.AllEnemies, first[4].Ultimate.Target);
                    Assert.IsFalse(first[4].Ultimate.IsHeal);
                }
            }
        }

        [Test]
        public void StarterTeam_WinsStageOneForAtLeastNinetyPercentOfSeeds()
        {
            var starters = new UnitSpec[HeroCatalog.StartingHeroes.Length];
            for (int i = 0; i < starters.Length; i++)
                starters[i] = HeroCatalog.BuildUnit(HeroCatalog.StartingHeroes[i], 1);

            int wins = 0;
            for (int seed = 1; seed <= 50; seed++)
            {
                var battle = new BattleSim(starters, CampaignContent.BuildEnemies(1), seed);
                battle.Run();
                if (battle.Winner == 0) wins++;
            }
            Assert.GreaterOrEqual(wins, 45, "Stage 1 must be winnable in at least 90% of seeds 1..50");
        }

        [Test]
        public void StarterTeam_LosesStageTwoHundredForEverySeed()
        {
            var starters = new UnitSpec[HeroCatalog.StartingHeroes.Length];
            for (int i = 0; i < starters.Length; i++)
                starters[i] = HeroCatalog.BuildUnit(HeroCatalog.StartingHeroes[i], 1);

            for (int seed = 1; seed <= 50; seed++)
            {
                var battle = new BattleSim(starters, CampaignContent.BuildEnemies(200), seed);
                battle.Run();
                Assert.AreEqual(1, battle.Winner, "Stage 200 should defeat the starter team for seed " + seed);
            }
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
