// Player collection, Spirit Source ceiling and five-slot formation.
using System;
using System.Collections.Generic;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Meta
{
    public sealed class Roster
    {
        public List<HeroInstance> Heroes = new List<HeroInstance>();
        public string[] Formation = new string[5];

        public HeroInstance Find(string heroId)
        {
            for (int i = 0; i < Heroes.Count; i++)
                if (Heroes[i].HeroId == heroId) return Heroes[i];
            return null;
        }

        public int SpiritCeiling
        {
            get
            {
                int m = 1;
                for (int i = 0; i < Heroes.Count; i++) m = Math.Max(m, Heroes[i].Level);
                return m;
            }
        }

        public bool AddHeroOrShards(string heroId, Rarity rarity, out int shardsGained)
        {
            shardsGained = 0;
            HeroInstance h = Find(heroId);
            if (h != null)
            {
                shardsGained = Progression.DuplicateShards(rarity);
                h.Shards += shardsGained;
                return false;
            }
            Heroes.Add(new HeroInstance { HeroId = heroId, Level = Math.Max(1, SpiritCeiling - 5) });
            return true;
        }

        public void SetFormation(int slot, string heroId)
        {
            if (Formation == null || Formation.Length != 5) Formation = new string[5];
            if (slot < 0 || slot >= Formation.Length) return;
            if (!string.IsNullOrEmpty(heroId) && Find(heroId) == null) return;
            if (!string.IsNullOrEmpty(heroId))
            {
                for (int i = 0; i < Formation.Length; i++)
                {
                    if (i == slot || Formation[i] != heroId) continue;
                    Formation[i] = Formation[slot];
                    break;
                }
            }
            Formation[slot] = heroId;
        }

        public string[] GetFormation()
        {
            if (Formation == null || Formation.Length != 5) Formation = new string[5];
            string[] result = new string[5];
            for (int i = 0; i < 5; i++)
            {
                string id = Formation[i];
                result[i] = Find(id) != null ? id : (i < Heroes.Count ? Heroes[i].HeroId : null);
            }
            return result;
        }

        public bool TryLevelUp(string heroId, Wallet wallet)
        {
            HeroInstance hero = Find(heroId);
            if (hero == null || hero.Level >= Progression.LevelCap(hero.Ascension)) return false;
            int expCost = Progression.ExpToNext(hero.Level);
            long goldCost = 75L * hero.Level;
            if (!wallet.TrySpend(Currency.Gold, goldCost)) return false;
            if (!wallet.TrySpend(Currency.Exp, expCost))
            {
                wallet.Add(Currency.Gold, goldCost);
                return false;
            }
            hero.Level++;
            return true;
        }

        public bool TryStarUp(string heroId)
        {
            HeroInstance hero = Find(heroId);
            if (hero == null || hero.Stars >= Progression.MaxStars) return false;
            int cost = Progression.ShardsForNextStar(hero.Stars);
            if (hero.Shards < cost) return false;
            hero.Shards -= cost;
            hero.Stars++;
            return true;
        }

        public bool TryAscend(string heroId, Wallet wallet)
        {
            HeroInstance hero = Find(heroId);
            if (hero == null || hero.Ascension >= Progression.MaxAscension) return false;
            if (hero.Stars < hero.Ascension + 1) return false;

            long goldCost = 1000L * (hero.Ascension + 1);
            long materialCost = 15L * (hero.Ascension + 1);
            if (!wallet.TrySpend(Currency.Gold, goldCost)) return false;
            if (!wallet.TrySpend(Currency.GearMaterials, materialCost))
            {
                wallet.Add(Currency.Gold, goldCost);
                return false;
            }
            hero.Ascension++;
            return true;
        }
    }
}
