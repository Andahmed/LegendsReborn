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
        private struct EnemyArchetype
        {
            public string Model;
            public string Name;
            public Faction Faction;
            public Role Role;
            public int Hp, Atk, Def, Spd;
            public float Crit;
            public float SkillPower;
            public bool SkillHeal;

            public EnemyArchetype(string model, string name, Faction faction, Role role,
                int hp, int atk, int def, int spd, float crit, float skillPower, bool skillHeal = false)
            {
                Model = model; Name = name; Faction = faction; Role = role;
                Hp = hp; Atk = atk; Def = def; Spd = spd; Crit = crit;
                SkillPower = skillPower; SkillHeal = skillHeal;
            }
        }

        public static readonly string[] Biomes =
        {
            "The Seven Sands", "Ashen Frontier", "Emerald Ruins", "Obsidian Reach", "The Gray Flood"
        };

        // Five authored enemy lineups, one per biome. Model IDs are restricted to the four
        // models currently supported by the client: skeleton, slime, bat, and dragon.
        private static readonly EnemyArchetype[][] BiomeLineups =
        {
            new[]
            {
                new EnemyArchetype("skeleton", "Bone Guard", Faction.Fortress, Role.Tank, 640, 58, 30, 88, .03f, 1.10f),
                new EnemyArchetype("slime", "Dune Ooze", Faction.Wild, Role.Tank, 720, 52, 34, 82, .02f, 1.05f),
                new EnemyArchetype("bat", "Night Flier", Faction.Shadow, Role.Dps, 520, 72, 24, 118, .08f, 1.15f),
                new EnemyArchetype("skeleton", "Ash Raider", Faction.Wild, Role.Dps, 600, 62, 28, 96, .05f, 1.10f),
                new EnemyArchetype("dragon", "Wasteland Drake", Faction.Forest, Role.Tank, 1200, 88, 48, 82, .08f, 1.20f)
            },
            new[]
            {
                new EnemyArchetype("skeleton", "Cinder Marshal", Faction.Shadow, Role.Tank, 680, 64, 33, 92, .04f, 1.12f),
                new EnemyArchetype("slime", "Magma Glob", Faction.Fortress, Role.Support, 760, 48, 38, 84, .02f, .95f, true),
                new EnemyArchetype("bat", "Ember Screecher", Faction.Wild, Role.Dps, 560, 76, 25, 120, .09f, 1.18f),
                new EnemyArchetype("skeleton", "Charred Skirmisher", Faction.Forest, Role.Dps, 620, 66, 29, 100, .06f, 1.12f),
                new EnemyArchetype("dragon", "Ashen Wyrm", Faction.Abyss, Role.Tank, 1260, 92, 52, 84, .09f, 1.22f)
            },
            new[]
            {
                new EnemyArchetype("skeleton", "Mossbound Keeper", Faction.Forest, Role.Tank, 700, 60, 36, 86, .03f, 1.08f),
                new EnemyArchetype("slime", "Verdant Spore", Faction.Wild, Role.Support, 790, 50, 40, 88, .03f, .98f, true),
                new EnemyArchetype("bat", "Canopy Stalker", Faction.Shadow, Role.Dps, 570, 78, 27, 122, .10f, 1.20f),
                new EnemyArchetype("skeleton", "Rootbound Archer", Faction.Divinity, Role.Dps, 640, 68, 31, 102, .06f, 1.14f),
                new EnemyArchetype("dragon", "Emerald Guardian", Faction.Forest, Role.Tank, 1320, 96, 56, 86, .10f, 1.24f)
            },
            new[]
            {
                new EnemyArchetype("skeleton", "Obsidian Watcher", Faction.Shadow, Role.Tank, 730, 66, 39, 90, .05f, 1.14f),
                new EnemyArchetype("slime", "Blackglass Ooze", Faction.Abyss, Role.Tank, 820, 58, 43, 86, .04f, 1.08f),
                new EnemyArchetype("bat", "Riftwing", Faction.Divinity, Role.Dps, 590, 82, 29, 124, .11f, 1.22f),
                new EnemyArchetype("skeleton", "Voidbound Raider", Faction.Forest, Role.Dps, 660, 72, 33, 104, .07f, 1.16f),
                new EnemyArchetype("dragon", "Obsidian Tyrant", Faction.Shadow, Role.Tank, 1380, 102, 60, 88, .11f, 1.26f)
            },
            new[]
            {
                new EnemyArchetype("skeleton", "Drowned Sentinel", Faction.Abyss, Role.Tank, 760, 70, 42, 92, .05f, 1.16f),
                new EnemyArchetype("slime", "Flood Mire", Faction.Forest, Role.Support, 850, 56, 46, 90, .04f, 1.02f, true),
                new EnemyArchetype("bat", "Gloom Skimmer", Faction.Shadow, Role.Dps, 620, 86, 31, 126, .12f, 1.24f),
                new EnemyArchetype("skeleton", "Tideworn Hunter", Faction.Wild, Role.Dps, 690, 76, 35, 106, .08f, 1.18f),
                new EnemyArchetype("dragon", "Gray Flood Leviathan", Faction.Abyss, Role.Tank, 1450, 108, 64, 90, .12f, 1.28f)
            }
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
            if (stage < 1) stage = 1;
            if (stage > CampaignProgress.MaxStage) stage = CampaignProgress.MaxStage;

            int biome = (stage - 1) / CampaignProgress.StagesPerBiome;
            bool bossStage = CampaignProgress.IsBoss(stage);
            float scale = 1f + (stage - 1) * 0.075f;
            EnemyArchetype[] lineup = BiomeLineups[biome];
            UnitSpec[] units = new UnitSpec[5];

            for (int i = 0; i < units.Length; i++)
            {
                EnemyArchetype archetype = lineup[i];
                bool boss = bossStage && i == 4;
                string skillId = "enemy_skill_" + biome + "_" + i;
                units[i] = new UnitSpec
                {
                    Id = "enemy_" + archetype.Model + "_" + i,
                    Name = boss ? Biomes[biome] + " Guardian" : archetype.Name,
                    Faction = archetype.Faction,
                    Role = archetype.Role,
                    Hp = (int)(archetype.Hp * scale),
                    Atk = (int)(archetype.Atk * scale),
                    Def = (int)(archetype.Def * scale),
                    Spd = archetype.Spd,
                    CritChance = archetype.Crit,
                    Skill = new SkillSpec
                    {
                        Id = skillId,
                        Name = archetype.Name + " Skill",
                        Cooldown = 5f,
                        Power = archetype.SkillPower,
                        Target = archetype.SkillHeal ? TargetType.LowestHpAlly : TargetType.SingleEnemy,
                        IsHeal = archetype.SkillHeal
                    },
                    Ultimate = boss ? new SkillSpec
                    {
                        Id = "enemy_boss_ultimate_" + biome,
                        Name = Biomes[biome] + " Cataclysm",
                        Cooldown = 0f,
                        Power = 0.85f,
                        Target = TargetType.AllEnemies,
                        IsHeal = false
                    } : null
                };
            }

            return units;
        }
    }
}
