// =============================================================================
// FILE: Assets/Scripts/Combat/Types.cs
// OWNER ROLE: Client Programmer - Combat
// RESPONSIBILITY: Combat data types: Faction/Role/Rarity enums, FactionChart (+25% advantage), SkillSpec, UnitSpec, BattleEvent, SeededRng.
// WORK ON: Add new enums/fields only with a GDD change. Tune FactionChart.AdvantageBonus. Keep this file free of UnityEngine.
// STATUS: Implemented
// =============================================================================
using System.Collections.Generic;

namespace SandsOfLegends.Combat
{
    public enum Faction { Fortress, Wild, Forest, Shadow, Divinity, Abyss }
    public enum Role { Tank, Support, Dps }
    public enum Rarity { Common, Rare, Epic, Legendary }
    public enum TargetType { SingleEnemy, AllEnemies, LowestHpAlly, AllAllies }

    /// <summary>
    /// Faction advantage chart. Core 4 factions form a cycle:
    /// Fortress &gt; Wild &gt; Forest &gt; Shadow &gt; Fortress.
    /// The two rare factions (Divinity / Abyss) counter each other.
    /// The advantage bonus is a design-tunable number (GDD 4.3: 25%).
    /// </summary>
    public static class FactionChart
    {
        public const float AdvantageBonus = 0.25f;

        public static bool HasAdvantage(Faction attacker, Faction defender)
        {
            switch (attacker)
            {
                case Faction.Fortress: return defender == Faction.Wild;
                case Faction.Wild: return defender == Faction.Forest;
                case Faction.Forest: return defender == Faction.Shadow;
                case Faction.Shadow: return defender == Faction.Fortress;
                case Faction.Divinity: return defender == Faction.Abyss;
                case Faction.Abyss: return defender == Faction.Divinity;
                default: return false;
            }
        }

        public static float Multiplier(Faction attacker, Faction defender)
        {
            return HasAdvantage(attacker, defender) ? 1f + AdvantageBonus : 1f;
        }
    }

    /// <summary>Immutable skill description (data-driven; no hardcoded hero logic).</summary>
    public sealed class SkillSpec
    {
        public string Id;
        public string Name;
        public float Cooldown = 5f;      // seconds (ignored for ultimates)
        public float Power = 1.5f;       // multiplier of ATK (damage or heal)
        public TargetType Target = TargetType.SingleEnemy;
        public bool IsHeal;
    }

    /// <summary>Immutable hero stats at a given level (data-driven).</summary>
    public sealed class UnitSpec
    {
        public string Id;
        public string Name;
        public Faction Faction;
        public Role Role;
        public int Hp = 1000;
        public int Atk = 100;
        public int Def = 50;
        public int Spd = 100;            // 100 = one action per second at x1
        public float CritChance = 0.1f;
        public SkillSpec Skill;          // timed active skill (optional)
        public SkillSpec Ultimate;       // energy-based ultimate (optional)
    }

    public enum BattleEventType { Attack, Skill, Ultimate, Heal, Death, End }

    public struct BattleEvent
    {
        public float Time;
        public BattleEventType Type;
        public int SourceTeam, SourceSlot;
        public int TargetTeam, TargetSlot;
        public int Amount;
        public bool Crit;
        public string SkillId;
    }

    /// <summary>Tiny deterministic RNG (xorshift) so battles are reproducible and server-verifiable.</summary>
    public sealed class SeededRng
    {
        private uint _s;
        public SeededRng(int seed) { _s = seed == 0 ? 0x9E3779B9u : unchecked((uint)seed); }
        public uint NextUInt()
        {
            _s ^= _s << 13; _s ^= _s >> 17; _s ^= _s << 5;
            return _s;
        }
        public float NextFloat() { return (NextUInt() & 0xFFFFFF) / (float)0x1000000; }
    }
}
