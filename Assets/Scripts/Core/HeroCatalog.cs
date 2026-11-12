// Data-driven MVP hero catalog. Art models are imported by MVPArtInstaller.
using System;
using System.Collections.Generic;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Core
{
    [Serializable]
    public sealed class HeroCardData
    {
        public string Id, DisplayName, ModelId;
        public string TintHex, PortraitId;
        public Faction Faction;
        public Role Role;
        public Rarity Rarity;
        public int Hp, Atk, Def, Spd;
        public float Crit;
        public string SkillName, UltimateName;
        public float SkillPower, UltimatePower;
        public bool SkillHeal, UltimateHeal;

        public UnitSpec ToSpec(int level)
        {
            float growth = 1f + 0.055f * Math.Max(0, level - 1);
            return new UnitSpec
            {
                Id = Id, Name = DisplayName, Faction = Faction, Role = Role,
                Hp = (int)Math.Round(Hp * growth), Atk = (int)Math.Round(Atk * growth),
                Def = (int)Math.Round(Def * growth), Spd = Spd, CritChance = Crit,
                Skill = new SkillSpec
                {
                    Id = Id + "_skill", Name = SkillName, Cooldown = 5f,
                    Power = SkillPower, Target = SkillHeal ? TargetType.LowestHpAlly : TargetType.SingleEnemy,
                    IsHeal = SkillHeal
                },
                Ultimate = new SkillSpec
                {
                    Id = Id + "_ultimate", Name = UltimateName, Cooldown = 0f,
                    Power = UltimatePower, Target = UltimateHeal ? TargetType.AllAllies : TargetType.AllEnemies,
                    IsHeal = UltimateHeal
                }
            };
        }
    }

    public static class HeroCatalog
    {
        private static readonly HeroCardData[] Cards =
        {
            new HeroCardData { Id="warrior", DisplayName="Iron Vanguard", ModelId="Warrior", TintHex="#C65D4A", PortraitId="hero_warrior", Faction=Faction.Fortress, Role=Role.Tank, Rarity=Rarity.Common, Hp=1450, Atk=92, Def=82, Spd=90, Crit=.05f, SkillName="Shield Bash", UltimateName="Fortress Wall", SkillPower=1.35f, UltimatePower=1.9f },
            new HeroCardData { Id="paladin", DisplayName="Dawn Paladin", ModelId="Cleric", TintHex="#E7C65A", PortraitId="hero_paladin", Faction=Faction.Fortress, Role=Role.Support, Rarity=Rarity.Rare, Hp=1180, Atk=110, Def=66, Spd=100, Crit=.08f, SkillName="Radiant Mend", UltimateName="Sanctuary", SkillPower=1.55f, UltimatePower=2.1f, SkillHeal=true, UltimateHeal=true },
            new HeroCardData { Id="berserker", DisplayName="Ash Berserker", ModelId="Monk", TintHex="#A84332", PortraitId="hero_berserker", Faction=Faction.Wild, Role=Role.Dps, Rarity=Rarity.Epic, Hp=1030, Atk=148, Def=45, Spd=115, Crit=.18f, SkillName="Rending Strike", UltimateName="Rage Tempest", SkillPower=2.0f, UltimatePower=1.25f },
            new HeroCardData { Id="healer", DisplayName="Oasis Healer", ModelId="Cleric", TintHex="#57A88B", PortraitId="hero_healer", Faction=Faction.Forest, Role=Role.Support, Rarity=Rarity.Common, Hp=1120, Atk=98, Def=58, Spd=100, Crit=.05f, SkillName="Healing Light", UltimateName="Life Bloom", SkillPower=1.8f, UltimatePower=1.15f, SkillHeal=true, UltimateHeal=true },
            new HeroCardData { Id="sharpshooter", DisplayName="Dune Sharpshooter", ModelId="Ranger", TintHex="#6386C7", PortraitId="hero_sharpshooter", Faction=Faction.Shadow, Role=Role.Dps, Rarity=Rarity.Rare, Hp=920, Atk=142, Def=42, Spd=125, Crit=.2f, SkillName="Piercing Arrow", UltimateName="Rain of Arrows", SkillPower=2.0f, UltimatePower=1.05f },
            new HeroCardData { Id="militia", DisplayName="Sand Militia", ModelId="Warrior", TintHex="#B47B43", PortraitId="", Faction=Faction.Fortress, Role=Role.Tank, Rarity=Rarity.Common, Hp=1300, Atk=86, Def=72, Spd=92, Crit=.06f, SkillName="Spear Thrust", UltimateName="Shield Line", SkillPower=1.25f, UltimatePower=1.8f },
            new HeroCardData { Id="tracker", DisplayName="Dust Tracker", ModelId="Ranger", TintHex="#7F9A45", PortraitId="", Faction=Faction.Wild, Role=Role.Dps, Rarity=Rarity.Common, Hp=900, Atk=118, Def=40, Spd=122, Crit=.16f, SkillName="Quick Shot", UltimateName="Marked Prey", SkillPower=1.6f, UltimatePower=2.0f },
            new HeroCardData { Id="acolyte", DisplayName="Temple Acolyte", ModelId="Cleric", TintHex="#58A6A6", PortraitId="", Faction=Faction.Forest, Role=Role.Support, Rarity=Rarity.Common, Hp=1080, Atk=92, Def=56, Spd=96, Crit=.04f, SkillName="Minor Grace", UltimateName="Blessing", SkillPower=1.5f, UltimatePower=1.0f, SkillHeal=true, UltimateHeal=true },
            new HeroCardData { Id="scout", DisplayName="Night Scout", ModelId="Rogue", TintHex="#7955A6", PortraitId="", Faction=Faction.Shadow, Role=Role.Dps, Rarity=Rarity.Common, Hp=880, Atk=120, Def=38, Spd=128, Crit=.18f, SkillName="Ambush", UltimateName="Shadow Volley", SkillPower=1.8f, UltimatePower=1.15f },
            new HeroCardData { Id="guardian", DisplayName="Granite Guardian", ModelId="Warrior", TintHex="#7C8795", PortraitId="", Faction=Faction.Fortress, Role=Role.Tank, Rarity=Rarity.Common, Hp=1520, Atk=82, Def=88, Spd=82, Crit=.04f, SkillName="Stone Guard", UltimateName="Earth Bastion", SkillPower=1.2f, UltimatePower=1.7f },
            new HeroCardData { Id="monk", DisplayName="Way of Dawn", ModelId="Monk", TintHex="#D58B48", PortraitId="", Faction=Faction.Wild, Role=Role.Support, Rarity=Rarity.Rare, Hp=1100, Atk=120, Def=58, Spd=112, Crit=.1f, SkillName="Palm Strike", UltimateName="Inner Calm", SkillPower=1.65f, UltimatePower=1.2f, UltimateHeal=true },
            new HeroCardData { Id="shieldbearer", DisplayName="Oath Shield", ModelId="Warrior", TintHex="#477A9B", PortraitId="", Faction=Faction.Fortress, Role=Role.Tank, Rarity=Rarity.Rare, Hp=1600, Atk=95, Def=94, Spd=86, Crit=.05f, SkillName="Bulwark", UltimateName="Unbroken Oath", SkillPower=1.3f, UltimatePower=1.8f },
            new HeroCardData { Id="stormcaller", DisplayName="Storm Caller", ModelId="Wizard", TintHex="#5576D9", PortraitId="", Faction=Faction.Forest, Role=Role.Dps, Rarity=Rarity.Rare, Hp=930, Atk=145, Def=43, Spd=108, Crit=.12f, SkillName="Chain Spark", UltimateName="Thunderfall", SkillPower=1.45f, UltimatePower=1.25f },
            new HeroCardData { Id="sandstalker", DisplayName="Sand Stalker", ModelId="Rogue", TintHex="#A65C83", PortraitId="", Faction=Faction.Shadow, Role=Role.Dps, Rarity=Rarity.Rare, Hp=950, Atk=138, Def=42, Spd=125, Crit=.19f, SkillName="Twin Blades", UltimateName="Mirage Rush", SkillPower=1.9f, UltimatePower=1.2f },
            new HeroCardData { Id="oracle", DisplayName="Veiled Oracle", ModelId="Wizard", TintHex="#76A88B", PortraitId="", Faction=Faction.Forest, Role=Role.Support, Rarity=Rarity.Rare, Hp=1070, Atk=112, Def=55, Spd=98, Crit=.07f, SkillName="Foresight", UltimateName="Second Dawn", SkillPower=1.6f, UltimatePower=1.4f, SkillHeal=true, UltimateHeal=true },
            new HeroCardData { Id="sunwarden", DisplayName="Sun Warden", ModelId="Warrior", TintHex="#E39A35", PortraitId="", Faction=Faction.Fortress, Role=Role.Tank, Rarity=Rarity.Epic, Hp=1750, Atk=128, Def=105, Spd=90, Crit=.08f, SkillName="Solar Edge", UltimateName="Sunwall", SkillPower=1.9f, UltimatePower=2.0f },
            new HeroCardData { Id="dragonknight", DisplayName="Dragon Knight", ModelId="Warrior", TintHex="#B85A37", PortraitId="", Faction=Faction.Wild, Role=Role.Dps, Rarity=Rarity.Epic, Hp=1320, Atk=165, Def=70, Spd=104, Crit=.14f, SkillName="Drake Cleave", UltimateName="Dragon's Wrath", SkillPower=2.0f, UltimatePower=1.3f },
            new HeroCardData { Id="phoenix", DisplayName="Phoenix Herald", ModelId="Wizard", TintHex="#E66D3D", PortraitId="", Faction=Faction.Divinity, Role=Role.Support, Rarity=Rarity.Legendary, Hp=1480, Atk=160, Def=78, Spd=112, Crit=.13f, SkillName="Ember Renewal", UltimateName="Rebirth Flame", SkillPower=1.9f, UltimatePower=1.55f, SkillHeal=true, UltimateHeal=true },
            new HeroCardData { Id="voidreaper", DisplayName="Void Reaper", ModelId="Rogue", TintHex="#65427E", PortraitId="", Faction=Faction.Abyss, Role=Role.Dps, Rarity=Rarity.Legendary, Hp=1220, Atk=198, Def=58, Spd=132, Crit=.24f, SkillName="Rift Cut", UltimateName="Black Sun", SkillPower=2.4f, UltimatePower=1.55f },
            new HeroCardData { Id="titan", DisplayName="Sand Titan", ModelId="Monk", TintHex="#6D8793", PortraitId="", Faction=Faction.Fortress, Role=Role.Tank, Rarity=Rarity.Legendary, Hp=2100, Atk=152, Def=125, Spd=78, Crit=.08f, SkillName="World Shaker", UltimateName="Titan's Stand", SkillPower=2.0f, UltimatePower=2.2f },
        };

        public static IList<HeroCardData> All { get { return Cards; } }
        public static string[] StartingHeroes { get { return new[] { "warrior", "paladin", "berserker", "healer", "sharpshooter" }; } }

        public static HeroCardData Get(string id)
        {
            for (int i = 0; i < Cards.Length; i++) if (Cards[i].Id == id) return Cards[i];
            return null;
        }

        public static UnitSpec BuildUnit(string id, int level)
        {
            HeroCardData card = Get(id);
            return card == null ? null : card.ToSpec(level);
        }

        public static Dictionary<Rarity, List<string>> BuildGachaPool()
        {
            var pool = new Dictionary<Rarity, List<string>>();
            for (int r = 0; r <= 3; r++) pool[(Rarity)r] = new List<string>();
            for (int i = 0; i < Cards.Length; i++) pool[Cards[i].Rarity].Add(Cards[i].Id);
            return pool;
        }

        public static string RarityName(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: return "COMMON";
                case Rarity.Rare: return "RARE";
                case Rarity.Epic: return "EPIC";
                default: return "LEGENDARY";
            }
        }
    }
}
