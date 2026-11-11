using System.Collections.Generic;

namespace SandsOfLegends.Combat
{
    public enum TeamSynergyKind
    {
        None,
        ThreePlusTwo,
        MonoFaction
    }

    public struct TeamSynergyResult
    {
        public readonly TeamSynergyKind Kind;
        public readonly float HpMultiplier;
        public readonly float AtkMultiplier;

        public TeamSynergyResult(TeamSynergyKind kind, float hpMultiplier, float atkMultiplier)
        {
            Kind = kind;
            HpMultiplier = hpMultiplier;
            AtkMultiplier = atkMultiplier;
        }

        public bool HasBonus { get { return Kind != TeamSynergyKind.None; } }
    }

    /// <summary>Pure faction-composition rules for a complete five-unit team.</summary>
    public static class TeamSynergy
    {
        public static TeamSynergyResult Calculate(IList<UnitSpec> team)
        {
            if (team == null || team.Count != BattleSim.TeamSize)
                return None();

            var counts = new Dictionary<Faction, int>();
            for (int i = 0; i < team.Count; i++)
            {
                if (team[i] == null) return None();
                int count;
                counts.TryGetValue(team[i].Faction, out count);
                counts[team[i].Faction] = count + 1;
            }

            if (counts.Count == 1)
                return new TeamSynergyResult(TeamSynergyKind.MonoFaction, 1.25f, 1.25f);

            if (counts.Count == 2)
            {
                bool hasThree = false;
                bool hasTwo = false;
                foreach (int count in counts.Values)
                {
                    if (count == 3) hasThree = true;
                    else if (count == 2) hasTwo = true;
                }
                if (hasThree && hasTwo)
                    return new TeamSynergyResult(TeamSynergyKind.ThreePlusTwo, 1.15f, 1f);
            }

            return None();
        }

        private static TeamSynergyResult None()
        {
            return new TeamSynergyResult(TeamSynergyKind.None, 1f, 1f);
        }
    }
}
