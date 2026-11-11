using SandsOfLegends.Combat;
using UnityEngine;

namespace SandsOfLegends.Core
{
    /// <summary>Designer-editable skill asset. Converted to the engine-free SkillSpec for combat.</summary>
    [CreateAssetMenu(menuName = "SandsOfLegends/Skill", fileName = "Skill_")]
    public sealed class SkillDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [Min(0f)] public float cooldown = 5f;
        [Min(0f)] public float power = 1.5f;
        public TargetType target = TargetType.SingleEnemy;
        public bool isHeal;

        public SkillSpec ToSpec()
        {
            return new SkillSpec
            {
                Id = string.IsNullOrEmpty(id) ? name : id,
                Name = displayName,
                Cooldown = cooldown,
                Power = power,
                Target = target,
                IsHeal = isHeal
            };
        }
    }

    /// <summary>Designer-editable hero asset (one asset per hero; never hardcode hero stats in code).</summary>
    [CreateAssetMenu(menuName = "SandsOfLegends/Hero", fileName = "Hero_")]
    public sealed class HeroDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public Faction faction;
        public Role role;
        public Rarity rarity;

        [Header("Base stats (level 1)")]
        [Min(1)] public int hp = 1000;
        [Min(1)] public int atk = 100;
        [Min(0)] public int def = 50;
        [Min(1)] public int spd = 100;
        [Range(0f, 1f)] public float critChance = 0.1f;

        [Header("Per-level growth (multiplier per level above 1)")]
        [Range(0f, 0.5f)] public float growth = 0.08f;

        [Header("Skills")]
        public SkillDefinition skill;
        public SkillDefinition ultimate;

        public UnitSpec ToSpec(int level = 1)
        {
            float m = 1f + growth * Mathf.Max(0, level - 1);
            return new UnitSpec
            {
                Id = string.IsNullOrEmpty(id) ? name : id,
                Name = displayName,
                Faction = faction,
                Role = role,
                Hp = Mathf.RoundToInt(hp * m),
                Atk = Mathf.RoundToInt(atk * m),
                Def = Mathf.RoundToInt(def * m),
                Spd = spd,
                CritChance = critChance,
                Skill = skill != null ? skill.ToSpec() : null,
                Ultimate = ultimate != null ? ultimate.ToSpec() : null
            };
        }
    }
}
