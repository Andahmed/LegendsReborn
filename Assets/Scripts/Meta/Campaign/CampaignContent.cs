using SandsOfLegends.Combat;

namespace SandsOfLegends.Meta
{
    public sealed class CampaignStageData
    {
        public int Number;
        public string Biome;
        public bool IsBoss;
        public int RecommendedPower;
        public int EnergyCost;
    }

    public static class CampaignContent
    {
        public static readonly string[] Biomes =
        {
            "The Seven Sands", "Ashen Frontier", "Emerald Ruins", "Obsidian Reach", "The Gray Flood"
        };

        public static CampaignStageData Get(int stage)
        {
            if (stage < 1) stage = 1;
            if (stage > CampaignProgress.MaxStage) stage = CampaignProgress.MaxStage;
            int biome = (stage - 1) / CampaignProgress.StagesPerBiome;
            return new CampaignStageData
            {
                Number = stage,
                Biome = Biomes[biome],
                IsBoss = CampaignProgress.IsBoss(stage),
                RecommendedPower = 550 + stage * 95 + biome * 240,
                EnergyCost = 5
            };
        }

        public static UnitSpec[] BuildEnemies(int stage)
        {
            string[] models = { "Skeleton", "Slime", "Bat", "Skeleton", "Dragon" };
            string[] names = { "Bone Guard", "Dune Ooze", "Night Flier", "Ash Raider", "Wasteland Drake" };
            UnitSpec[] units = new UnitSpec[5];
            float scale = 1f + (stage - 1) * 0.075f;
            for (int i = 0; i < units.Length; i++)
            {
                bool boss = CampaignProgress.IsBoss(stage) && i == 4;
                units[i] = new UnitSpec
                {
                    Id = "enemy_" + models[i].ToLowerInvariant() + "_" + i,
                    Name = boss ? "Stage " + stage + " Guardian" : names[i],
                    Faction = (Faction)(i % 4),
                    Role = i == 0 || i == 3 || boss ? Role.Tank : Role.Dps,
                    Hp = (int)((boss ? 1900 : 850 + i * 65) * scale),
                    Atk = (int)((boss ? 175 : 88 + i * 9) * scale),
                    Def = (int)((boss ? 88 : 38 + i * 5) * scale),
                    Spd = boss ? 82 : 92 + i * 5,
                    CritChance = boss ? .15f : .08f
                };
            }
            return units;
        }
    }
}
