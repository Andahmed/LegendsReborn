using SandsOfLegends.Meta;

namespace SandsOfLegends.UI
{
    /// <summary>Validated roster operations; presentation is hosted by GameFlowController.</summary>
    public sealed class HeroesScreen : UIScreen
    {
        public static bool TryLevelUp(GameState state, string heroId)
        {
            if (state == null || !state.Roster.TryLevelUp(heroId, state.Wallet)) return false;
            state.Quests.Report("level_up_hero", 1);
            return true;
        }

        public static bool TryStarUp(GameState state, string heroId)
        {
            return state != null && state.Roster.TryStarUp(heroId);
        }

        public static void SetFormation(GameState state, int slot, string heroId)
        {
            if (state != null) state.Roster.SetFormation(slot, heroId);
        }
    }
}
