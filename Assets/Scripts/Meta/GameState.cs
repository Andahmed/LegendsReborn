// =============================================================================
// FILE: Assets/Scripts/Meta/GameState.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Aggregate player state (wallet, roster, pity, energy, campaign, quests, idle timer) with ToSave/FromSave.
// WORK ON: Add remaining systems with matching save migrations; connect formation to the battle setup flow.
// STATUS: Implemented (partial MVP); equipment and team formation persist through the single save model.
// =============================================================================
using System;

namespace SandsOfLegends.Meta
{
    public sealed class GameState
    {
        public readonly Wallet Wallet = new Wallet();
        public readonly Roster Roster = new Roster();
        public readonly PityState Pity = new PityState();
        public readonly EnergyState Energy = new EnergyState();
        public readonly CampaignProgress Campaign = new CampaignProgress();
        public readonly DailyQuests Quests = new DailyQuests();
        public long IdleLastClaimUnix;

        public static GameState NewGame(long nowUnix)
        {
            var s = new GameState();
            s.Energy.Value = EnergyState.Cap;
            s.Energy.LastTickUnix = nowUnix;
            s.IdleLastClaimUnix = nowUnix;
            return s;
        }

        public IdleRewards.Reward ClaimIdle(long nowUnix)
        {
            var r = IdleRewards.Calculate(IdleLastClaimUnix, nowUnix, Campaign.HighestCleared);
            Wallet.Add(Currency.Gold, r.Gold);
            Wallet.Add(Currency.Exp, r.Exp);
            IdleLastClaimUnix = nowUnix;
            return r;
        }

        public SaveData ToSave(long nowUnix)
        {
            return new SaveData
            {
                Version = SaveMigrations.CurrentVersion,
                Gems = Wallet.Get(Currency.Gems), Gold = Wallet.Get(Currency.Gold),
                Exp = Wallet.Get(Currency.Exp), GearMaterials = Wallet.Get(Currency.GearMaterials),
                Heroes = Roster.Heroes,
                TeamSlots = Roster.Formation.ToArray(),
                PitySinceEpic = Pity.SinceEpic, PitySinceLegendary = Pity.SinceLegendary,
                Energy = Energy.Value, EnergyTickUnix = Energy.LastTickUnix,
                HighestStage = Campaign.HighestCleared,
                IdleLastClaimUnix = IdleLastClaimUnix,
                QuestDay = Quests.DayKey, QuestProgress = (int[])Quests.Progress.Clone(), QuestClaimed = (bool[])Quests.Claimed.Clone(),
                SavedAtUnix = nowUnix
            };
        }

        public static GameState FromSave(SaveData d)
        {
            d = SaveMigrations.Migrate(d);
            var s = new GameState();
            s.Wallet.Set(Currency.Gems, d.Gems); s.Wallet.Set(Currency.Gold, d.Gold);
            s.Wallet.Set(Currency.Exp, d.Exp); s.Wallet.Set(Currency.GearMaterials, d.GearMaterials);
            s.Roster.Heroes = d.Heroes;
            s.Roster.Formation.LoadSlots(d.TeamSlots, s.Roster);
            s.Pity.SinceEpic = d.PitySinceEpic; s.Pity.SinceLegendary = d.PitySinceLegendary;
            s.Energy.Value = d.Energy; s.Energy.LastTickUnix = d.EnergyTickUnix;
            s.Campaign.HighestCleared = d.HighestStage;
            s.IdleLastClaimUnix = d.IdleLastClaimUnix;
            s.Quests.DayKey = d.QuestDay; s.Quests.Progress = d.QuestProgress; s.Quests.Claimed = d.QuestClaimed;
            return s;
        }
    }
}
