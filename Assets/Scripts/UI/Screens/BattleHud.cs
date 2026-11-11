using SandsOfLegends.Combat;
using SandsOfLegends.Presentation;

namespace SandsOfLegends.UI
{
    /// <summary>Battle HUD controller. Visual buttons are created by GameFlowController at runtime.</summary>
    public sealed class BattleHud : UIScreen
    {
        private BattleSim simulation;
        private BattleView view;

        public void Bind(BattleSim sim, BattleView battleView)
        {
            simulation = sim;
            view = battleView;
        }

        public void SetSpeed(int speed)
        {
            if (view != null) view.SetSpeed(speed);
        }

        public bool RequestUltimate(int slot)
        {
            return simulation != null && simulation.RequestUltimate(0, slot);
        }

        public void ToggleAutoUltimate()
        {
            if (simulation != null) simulation.AutoUltimate = !simulation.AutoUltimate;
        }
    }
}
