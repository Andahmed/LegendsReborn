// Aggregate player state and convert to/from one versioned save model.
namespace SandsOfLegends.Meta
{
    public sealed class GameState
    {
        public readonly Wallet Wallet = new Wallet();
        public readonly Roster Roster = new Roster();
        public readonly EquipmentLoadout Equipment = new EquipmentLoadout();
        public readonly PityState Pity = new PityState();
        public readonly EnergyState Energy = new EnergyState();
        public readonly CampaignProgress Campaign = new CampaignProgress();
        public readonly DailyQuests Quests = new DailyQuests();
        public long IdleLastClaimUnix;

        public static GameState NewGame(long nowUnix)
        {
            GameState state = new GameState();
            state.Energy.Value = EnergyState.Cap;
            state.Energy.LastTickUnix = nowUnix;
            state.IdleLastClaimUnix = nowUnix;
            return state;
        }

        public IdleRewards.Reward ClaimIdle(long nowUnix)
        {
            IdleRewards.Reward reward = IdleRewards.Calculate(IdleLastClaimUnix, nowUnix, Campaign.HighestCleared);
            Wallet.Add(Currency.Gold, reward.Gold);
            Wallet.Add(Currency.Exp, reward.Exp);
            IdleLastClaimUnix = nowUnix;
            Quests.Report("claim_idle", 1);
            return reward;
        }

        public SaveData ToSave(long nowUnix)
        {
            return new SaveData
            {
                Version = SaveMigrations.CurrentVersion,
                Gems = Wallet.Get(Currency.Gems), Gold = Wallet.Get(Currency.Gold),
                Exp = Wallet.Get(Currency.Exp), GearMaterials = Wallet.Get(Currency.GearMaterials),
                Heroes = Roster.Heroes,
                Formation = (string[])Roster.GetFormation().Clone(),
                Equipment = Equipment.Items,
                PitySinceEpic = Pity.SinceEpic, PitySinceLegendary = Pity.SinceLegendary,
                Energy = Energy.Value, EnergyTickUnix = Energy.LastTickUnix,
                HighestStage = Campaign.HighestCleared,
                IdleLastClaimUnix = IdleLastClaimUnix,
                QuestDay = Quests.DayKey, QuestProgress = (int[])Quests.Progress.Clone(),
                QuestClaimed = (bool[])Quests.Claimed.Clone(), SavedAtUnix = nowUnix
            };
        }

        public static GameState FromSave(SaveData data)
        {
            data = SaveMigrations.Migrate(data);
            GameState state = new GameState();
            state.Wallet.Set(Currency.Gems, data.Gems);
            state.Wallet.Set(Currency.Gold, data.Gold);
            state.Wallet.Set(Currency.Exp, data.Exp);
            state.Wallet.Set(Currency.GearMaterials, data.GearMaterials);
            state.Roster.Heroes = data.Heroes;
            state.Roster.Formation = data.Formation;
            state.Equipment.Items = data.Equipment;
            state.Pity.SinceEpic = data.PitySinceEpic;
            state.Pity.SinceLegendary = data.PitySinceLegendary;
            state.Energy.Value = data.Energy;
            state.Energy.LastTickUnix = data.EnergyTickUnix;
            state.Campaign.HighestCleared = data.HighestStage;
            state.IdleLastClaimUnix = data.IdleLastClaimUnix;
            state.Quests.DayKey = data.QuestDay;
            state.Quests.Progress = data.QuestProgress;
            state.Quests.Claimed = data.QuestClaimed;
            return state;
        }
    }
}
