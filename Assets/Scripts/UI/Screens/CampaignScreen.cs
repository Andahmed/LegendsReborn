using UnityEngine;
using SandsOfLegends.Meta;

namespace SandsOfLegends.UI
{
    /// <summary>Campaign screen rules shared by the runtime flow and any future authored scene UI.</summary>
    public sealed class CampaignScreen : UIScreen
    {
        public CampaignStageData Current { get { return GameBootstrapState().Campaign.HighestCleared < CampaignProgress.MaxStage
            ? CampaignContent.Get(GameBootstrapState().Campaign.HighestCleared + 1) : CampaignContent.Get(CampaignProgress.MaxStage); } }

        public static bool TryEnter(GameState state, int stage, long nowUnix)
        {
            if (state == null || stage != state.Campaign.HighestCleared + 1 || stage > CampaignProgress.MaxStage) return false;
            CampaignStageData data = CampaignContent.Get(stage);
            if (!state.Energy.TryConsume(data.EnergyCost, nowUnix)) return false;
            state.Quests.Report("spend_energy_30", data.EnergyCost);
            return true;
        }

        public static bool ApplyVictory(GameState state, int stage)
        {
            if (state == null || !state.Campaign.TryClear(stage)) return false;
            state.Wallet.Add(Currency.Gold, 150 + stage * 18);
            state.Wallet.Add(Currency.Exp, 90 + stage * 12);
            state.Wallet.Add(Currency.GearMaterials, 2 + stage / 10);
            state.Quests.Report("battle_5", 1);
            return true;
        }

        private GameState GameBootstrapState()
        {
            return SandsOfLegends.Bootstrap.GameBootstrap.Instance != null
                ? SandsOfLegends.Bootstrap.GameBootstrap.Instance.State : GameState.NewGame(0);
        }
    }
}
