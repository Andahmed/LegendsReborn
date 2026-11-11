# Economy ledger (implementation audit)

This document records the economy that is actually implemented in the engine-free Meta code. It distinguishes design intent in `docs/GDD.md` from implemented values. Amounts below are code constants/formulas, not a claim that the economy is balanced.

## Currency inventory and authority

| Resource | Role | Implemented source / sink |
|---|---|---|
| Gems | Premium currency | `Wallet` stores and spends Gems; `GachaSystem` declares summon costs; `DailyQuests.TryClaim` grants local Gems. No general production ledger, ad/IAP, event, or drop source is implemented in Meta. |
| Gold | Hero and gear progression | `GameState.ClaimIdle` credits idle Gold; `Roster.TryLevelUp`, `Roster.TryAscend`, and `EquipmentLoadout.TryUpgrade` spend it. |
| Exp | Hero leveling | `GameState.ClaimIdle` credits idle Exp; `Roster.TryLevelUp` spends it. |
| GearMaterials | Ascension and equipment upgrades | `Roster.TryAscend` and `EquipmentLoadout.TryUpgrade` spend it. A Meta income source is not implemented. |
| Hero shards | Hero star progression | Duplicate pulls grant shards through `Roster.AddHeroOrShards`; `Roster.TryStarUp` spends them. |
| Energy | Farming/event stamina | `EnergyState` regenerates and consumes it. A refill purchase implementation is not present. |

The wallet enum and storage are in `Assets/Scripts/Meta/Economy/Wallet.cs` (`Currency`, `Wallet.Get/Add/TrySpend/Set`). `Wallet.Add` rejects negative amounts and saturates at `long.MaxValue / 2`; `TrySpend` rejects negative or unaffordable spends.

## Gems and summoning

- One summon costs **300 Gems** and a ten-pull costs **2,700 Gems**: `GachaSystem.GemCostSingle` and `GemCostTen` in `Assets/Scripts/Meta/Gacha/GachaSystem.cs`.
- Test rarity weights are **40% Common / 35% Rare / 20% Epic / 5% Legendary** (cumulative roll cutoffs 40, 75, 95, 100): `GachaSystem.Roll`.
- Pity guarantees Epic-or-better by pull **30** and Legendary by pull **90**: `EpicPity`, `LegendaryPity`, and `GachaSystem.Pull`.
- Duplicate conversion is **5 / 10 / 20 / 40 shards** for Common / Rare / Epic / Legendary: `Progression.DuplicateShards` in `Assets/Scripts/Meta/Heroes/HeroInstance.cs`.
- Daily quest claims grant **20 Gems each**: `DailyQuests.RewardGemsEach` and `TryClaim` in `Assets/Scripts/Meta/Quests/DailyQuests.cs`. The grant currently uses the local wallet; server authority/synchronization is not part of this Meta implementation.

## Hero progression costs

All current curves are explicitly placeholders in `Assets/Scripts/Meta/Heroes/HeroInstance.cs`:

| Operation | Cost / rule | Code location |
|---|---|---|
| Level up | `ExpToNext(level) = 100 + 40 × level + 5 × level²` Exp and `75 × current level` Gold | `Progression.ExpToNext`; `Roster.TryLevelUp` |
| Level cap | `20 + 20 × ascension` (20, 40, 60, 80, 100) | `Progression.LevelCap` |
| Star up | `10 × (current stars + 1)` hero-specific shards | `Progression.ShardsForNextStar`; `Roster.TryStarUp` |
| Ascend | Requires stars >= next ascension; costs `1,000 × next ascension` Gold and `15 × next ascension` GearMaterials | `Roster.TryAscend` |
| New hero | Starts at `max(1, SpiritCeiling - 5)` | `Roster.AddHeroOrShards` |

The six equipment slots are Weapon, Helmet, Armor, Boots, Ring, and Amulet: `EquipSlot` in `Assets/Scripts/Meta/Equipment/Equipment.cs`. Each upgrade costs `100 × current item level` Gold and `3 × current item level` GearMaterials; maximum equipment level is 20 (`EquipmentLoadout.TryUpgrade`). The code currently has no set bonuses or equipment-stat application.

## Energy

- Capacity: **120** (`EnergyState.Cap`).
- Regeneration: **1 point per 360 seconds / 6 minutes** (`EnergyState.SecondsPerPoint`).
- `EnergyState.Refresh` caps the value at 120 and advances the injected Unix timestamp; `TryConsume` refreshes before validating and deducting the requested amount.
- No energy refill purchase or premium refill price is implemented in the Meta code.

## Idle income

Idle rewards are capped at **12 hours** (`IdleRewards.CapSeconds = 12 × 3600`). Rewards are paid in whole elapsed minutes using the highest cleared campaign stage:

- Gold per minute = **10 + 2 × highest cleared stage**.
- Exp per minute = **5 + highest cleared stage**.

These formulas are `IdleRewards.GoldPerMinute`, `ExpPerMinute`, and `Calculate` in `Assets/Scripts/Meta/Idle/IdleRewards.cs`. `GameState.ClaimIdle` credits Gold and Exp, advances `IdleLastClaimUnix`, and reports the `claim_idle` daily quest. No idle Gems, shards, or GearMaterials are awarded by this implementation.

## Income and sinks not yet implemented

- GDD describes campaign rewards (Gold, Exp, shards, GearMaterials), farming, events, weekly boss, shop, ads, and IAP. The current Meta code does not implement a complete source ledger for those channels. In particular, `CampaignContent.Get` describes stage metadata/enemies and sets `EnergyCost = 5`; it does not grant campaign rewards.
- The economy table previously listed events/drops/ads/IAP, exp farms, selling, weekly shop, and weekly boss as sources. These are design placeholders, not implemented Meta income paths.
- No sale/salvage sink, summon UI purchase flow, energy refill price, or daily/weekly economy budget is implemented in these files.

## Discrepancies (GDD vs code)

1. **Campaign energy:** GDD says normal campaign consumes no Energy and Energy is only for farming/events. However, `CampaignContent.Get` currently returns `EnergyCost = 5`. This is a data/code discrepancy; this document does not change it.
2. **Gear:** GDD calls for six gear slots, sets, and gear-material upgrades. Six slots and upgrades exist, but set bonuses and stat integration do not.
3. **New hero level:** GDD says a new hero is raised to shared ceiling minus five. `Roster.AddHeroOrShards` follows that formula but does not clamp the result to `Progression.LevelCap(0)`; this can exceed the base level cap.
4. **Quest Gems:** Daily quest Gems are added to the local `Wallet`; server-side Gems authority/sync is not implemented in this Meta path.

## Design references

- `docs/GDD.md`: core loop, 12-hour idle cap, Energy cap/regen and intended campaign exclusion, progression, summon rates/pity, gear and free-to-play principles.
- Implementation sources are linked inline above. Values should be treated as placeholders where the source comments say so; do not tune prices without the GDD/soft-launch process.
