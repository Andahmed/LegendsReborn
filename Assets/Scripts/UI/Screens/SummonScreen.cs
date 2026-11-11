using System.Collections.Generic;
using SandsOfLegends.Combat;
using SandsOfLegends.Core;
using SandsOfLegends.Meta;

namespace SandsOfLegends.UI
{
    /// <summary>Preview/test-only local pull. Live UI must use SummonService and never call this method.</summary>
    public sealed class SummonScreen : UIScreen
    {
        public static bool TryPull(GameState state, int count, int seed, out PullResult[] results)
        {
            results = new PullResult[0];
            if (state == null || (count != 1 && count != 10)) return false;
            long cost = count == 10 ? GachaSystem.GemCostTen : GachaSystem.GemCostSingle;
            if (!state.Wallet.TrySpend(Currency.Gems, cost)) return false;

            Dictionary<Rarity, List<string>> pool = HeroCatalog.BuildGachaPool();
            SeededRng rng = new SeededRng(seed);
            results = new PullResult[count];
            for (int i = 0; i < count; i++)
                results[i] = GachaSystem.Pull(pool, state.Pity, rng, state.Roster);
            state.Quests.Report("summon_1", count);
            return true;
        }
    }
}
