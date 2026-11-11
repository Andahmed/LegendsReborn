// =============================================================================
// FILE: Assets/Scripts/Meta/Campaign/CampaignProgress.cs
// OWNER ROLE: Systems Designer
// RESPONSIBILITY: Campaign progress: 200 stages for MVP = 5 biomes x 40 stages. Stages unlock sequentially.
// WORK ON: Link to StageDefinition data (enemy teams, rewards). Boss stages every 10. Difficulty curve from Economy Designer.
// STATUS: Implemented
// =============================================================================
namespace SandsOfLegends.Meta
{
    public sealed class CampaignProgress
    {
        public const int Biomes = 5;
        public const int StagesPerBiome = 40;
        public const int MaxStage = Biomes * StagesPerBiome;

        public int HighestCleared;   // 0 = nothing cleared

        public static int BiomeOf(int stage) { return (stage - 1) / StagesPerBiome; }
        public static bool IsBoss(int stage) { return stage > 0 && stage % 10 == 0; }

        /// <summary>Only the next stage can be cleared. Returns true when progress advanced.</summary>
        public bool TryClear(int stage)
        {
            if (stage != HighestCleared + 1 || stage > MaxStage) return false;
            HighestCleared = stage;
            return true;
        }
    }
}
