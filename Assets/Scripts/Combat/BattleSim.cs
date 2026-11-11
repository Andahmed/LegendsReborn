using System;
using System.Collections.Generic;

namespace SandsOfLegends.Combat
{
    /// <summary>Runtime state of one hero inside a battle.</summary>
    public sealed class BattleUnit
    {
        public readonly UnitSpec Spec;
        public readonly int Team;      // 0 = attacker/player, 1 = defender
        public readonly int Slot;      // 0-1 front row, 2-4 back row
        public int Hp;
        public readonly int MaxHp;
        public float Energy;           // 0..100, ultimate at 100
        public float ActionMeter;      // acts when >= 1
        public float SkillCd;
        public bool UltQueued;         // manual ultimate request

        public bool Alive { get { return Hp > 0; } }
        public bool IsFront { get { return Slot < BattleSim.FrontSlots; } }

        public BattleUnit(UnitSpec spec, int team, int slot)
        {
            Spec = spec; Team = team; Slot = slot;
            Hp = MaxHp = spec.Hp;
            SkillCd = spec.Skill != null ? spec.Skill.Cooldown * 0.5f : 0f; // first cast earlier
        }
    }

    /// <summary>
    /// Deterministic fixed-step auto-battle simulation. Pure C# (no UnityEngine) so it can be
    /// unit-tested and re-run on the server for verification. The view layer only reads
    /// <see cref="Events"/> / unit state and plays animations at x1/x2/x4 by calling Step() more often.
    /// </summary>
    public sealed class BattleSim
    {
        public const int TeamSize = 5;
        public const int FrontSlots = 2;          // 2 front + 3 back
        public const float Dt = 0.1f;
        public const float MaxTime = 120f;        // timeout => defender wins
        public const float EnergyOnAttack = 20f;
        public const float EnergyOnHit = 10f;
        public const float CritMultiplier = 1.5f;

        public readonly List<BattleUnit> Units = new List<BattleUnit>();
        public readonly List<BattleEvent> Events = new List<BattleEvent>();
        public float Time { get; private set; }
        public bool Finished { get; private set; }
        public int Winner { get; private set; } = -1;   // 0 = team A, 1 = team B
        public bool AutoUltimate = true;

        private readonly SeededRng _rng;

        public BattleSim(IList<UnitSpec> teamA, IList<UnitSpec> teamB, int seed)
        {
            _rng = new SeededRng(seed);
            for (int i = 0; i < teamA.Count && i < TeamSize; i++) if (teamA[i] != null) Units.Add(new BattleUnit(teamA[i], 0, i));
            for (int i = 0; i < teamB.Count && i < TeamSize; i++) if (teamB[i] != null) Units.Add(new BattleUnit(teamB[i], 1, i));
            CheckEnd();
        }

        public BattleUnit Get(int team, int slot)
        {
            for (int i = 0; i < Units.Count; i++)
                if (Units[i].Team == team && Units[i].Slot == slot) return Units[i];
            return null;
        }

        /// <summary>Manual ultimate (used when AutoUltimate is off). Returns false if not ready.</summary>
        public bool RequestUltimate(int team, int slot)
        {
            var u = Get(team, slot);
            if (u == null || !u.Alive || u.Spec.Ultimate == null || u.Energy < 100f) return false;
            u.UltQueued = true;
            return true;
        }

        public void Run()
        {
            while (!Finished) Step();
        }

        public void Step()
        {
            if (Finished) return;
            Time += Dt;

            for (int i = 0; i < Units.Count; i++)
            {
                var u = Units[i];
                if (!u.Alive) continue;
                if (u.SkillCd > 0f) u.SkillCd -= Dt;
                u.ActionMeter += Dt * u.Spec.Spd / 100f;
                if (u.ActionMeter >= 1f)
                {
                    u.ActionMeter -= 1f;
                    Act(u);
                    if (CheckEnd()) return;
                }
            }

            if (Time >= MaxTime && !Finished)
            {
                Finished = true; Winner = 1;
                Events.Add(new BattleEvent { Time = Time, Type = BattleEventType.End });
            }
        }

        private void Act(BattleUnit u)
        {
            var spec = u.Spec;
            if (spec.Ultimate != null && u.Energy >= 100f && (AutoUltimate || u.UltQueued))
            {
                u.UltQueued = false;
                u.Energy = 0f;
                Execute(u, spec.Ultimate, BattleEventType.Ultimate);
            }
            else if (spec.Skill != null && u.SkillCd <= 0f)
            {
                u.SkillCd = spec.Skill.Cooldown;
                Execute(u, spec.Skill, BattleEventType.Skill);
                u.Energy = Math.Min(100f, u.Energy + EnergyOnAttack * 0.5f);
            }
            else
            {
                Execute(u, BasicAttack, BattleEventType.Attack);
                u.Energy = Math.Min(100f, u.Energy + EnergyOnAttack);
            }
        }

        private static readonly SkillSpec BasicAttack = new SkillSpec
        {
            Id = "basic", Name = "Attack", Power = 1f, Target = TargetType.SingleEnemy
        };

        private void Execute(BattleUnit src, SkillSpec skill, BattleEventType evType)
        {
            var targets = ResolveTargets(src, skill);
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (skill.IsHeal)
                {
                    int heal = Math.Max(1, (int)Math.Round(src.Spec.Atk * skill.Power));
                    int before = t.Hp;
                    t.Hp = Math.Min(t.MaxHp, t.Hp + heal);
                    Events.Add(new BattleEvent
                    {
                        Time = Time, Type = BattleEventType.Heal, SkillId = skill.Id,
                        SourceTeam = src.Team, SourceSlot = src.Slot,
                        TargetTeam = t.Team, TargetSlot = t.Slot, Amount = t.Hp - before
                    });
                }
                else
                {
                    bool crit = _rng.NextFloat() < src.Spec.CritChance;
                    float raw = src.Spec.Atk * skill.Power * (100f / (100f + t.Spec.Def));
                    raw *= FactionChart.Multiplier(src.Spec.Faction, t.Spec.Faction);
                    if (crit) raw *= CritMultiplier;
                    int dmg = Math.Max(1, (int)Math.Round(raw));
                    t.Hp = Math.Max(0, t.Hp - dmg);
                    t.Energy = Math.Min(100f, t.Energy + EnergyOnHit);
                    Events.Add(new BattleEvent
                    {
                        Time = Time, Type = evType, SkillId = skill.Id, Crit = crit,
                        SourceTeam = src.Team, SourceSlot = src.Slot,
                        TargetTeam = t.Team, TargetSlot = t.Slot, Amount = dmg
                    });
                    if (!t.Alive)
                        Events.Add(new BattleEvent
                        {
                            Time = Time, Type = BattleEventType.Death,
                            TargetTeam = t.Team, TargetSlot = t.Slot
                        });
                }
            }
        }

        private List<BattleUnit> ResolveTargets(BattleUnit src, SkillSpec skill)
        {
            var list = new List<BattleUnit>(TeamSize);
            int enemy = 1 - src.Team;
            switch (skill.Target)
            {
                case TargetType.SingleEnemy:
                {
                    BattleUnit pick = null;
                    for (int i = 0; i < Units.Count && pick == null; i++)
                        if (Units[i].Team == enemy && Units[i].Alive && Units[i].IsFront) pick = Units[i];
                    for (int i = 0; i < Units.Count && pick == null; i++)
                        if (Units[i].Team == enemy && Units[i].Alive) pick = Units[i];
                    if (pick != null) list.Add(pick);
                    break;
                }
                case TargetType.AllEnemies:
                    for (int i = 0; i < Units.Count; i++)
                        if (Units[i].Team == enemy && Units[i].Alive) list.Add(Units[i]);
                    break;
                case TargetType.LowestHpAlly:
                {
                    BattleUnit pick = null;
                    for (int i = 0; i < Units.Count; i++)
                    {
                        var u = Units[i];
                        if (u.Team != src.Team || !u.Alive) continue;
                        if (pick == null || (float)u.Hp / u.MaxHp < (float)pick.Hp / pick.MaxHp) pick = u;
                    }
                    if (pick != null) list.Add(pick);
                    break;
                }
                case TargetType.AllAllies:
                    for (int i = 0; i < Units.Count; i++)
                        if (Units[i].Team == src.Team && Units[i].Alive) list.Add(Units[i]);
                    break;
            }
            return list;
        }

        private bool CheckEnd()
        {
            if (Finished) return true;
            bool a = false, b = false;
            for (int i = 0; i < Units.Count; i++)
            {
                if (!Units[i].Alive) continue;
                if (Units[i].Team == 0) a = true; else b = true;
            }
            if (a && b) return false;
            Finished = true;
            Winner = a ? 0 : 1;   // both dead / team A empty => B wins
            Events.Add(new BattleEvent { Time = Time, Type = BattleEventType.End, SourceTeam = Winner });
            return true;
        }
    }
}
