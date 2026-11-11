# C0 — Game Systems Freeze (Prototype 4v4)

Status: **Design proposal / pending acceptance**  
Scope: Freeze the first playable client prototype. This document does not implement gameplay code and does not change `Server/`, Unity scenes, or existing combat files.

## 1. Play loop

1. Launch `Boot`; load the local player state.
2. Route to `Home`.
3. Open `Heroes` / `Formation` and select the four prototype heroes.
4. Open `Campaign`; select the single authored test encounter, **Stage 1 — The Broken Gate**.
5. Enter a real-time auto-battle: **4 player units vs 4 enemy units**.
6. Battle ends when one team has no living units, or at the 120-second simulation limit.
7. On victory, show the result and the Stage 1 test reward; on defeat, show defeat and do not advance campaign progress or grant the clear reward.
8. Return to Home and show updated progression/resources.

The production target remains **5v5**. The 4v4 lineup is a temporary vertical-slice fixture, not a change to the final formation.

## 2. Prototype heroes

Use existing records in `Assets/Scripts/Core/HeroCatalog.cs`; do not invent new records or copy external IP.

| ID | Display name | Faction | Role | Model ID | Skill / Ultimate |
|---|---|---|---|---|---|
| `warrior` | Iron Vanguard | Fortress | Tank | Warrior | Shield Bash / Fortress Wall |
| `tracker` | Dust Tracker | Wild | DPS | Ranger | Quick Shot / Marked Prey |
| `healer` | Oasis Healer | Forest | Support | Cleric | Healing Light / Life Bloom |
| `scout` | Night Scout | Shadow | DPS | Rogue | Ambush / Shadow Volley |

This gives the prototype one representative from each of the four core factions and distinct temporary model mappings. These are existing catalog mappings, not confirmation that the corresponding FBX files are installed or Unity-imported.

## 3. Prototype enemies and boss

Use the existing model IDs referenced by `Assets/Scripts/Meta/Campaign/CampaignContent.cs`.

| Prototype enemy | Model ID | Prototype role |
|---|---|---|
| Bone Guard | `Skeleton` | Front-line tank |
| Dune Ooze | `Slime` | Front-line bruiser |
| Night Flier | `Bat` | Back-line DPS |
| Ash Raider | `Skeleton` | Back-line DPS |

**Boss fixture:** Wasteland Drake / Stage Guardian, model ID `Dragon`, based on the existing Stage 10 boss placeholder. The boss is a separate fixture for boss presentation/testing; it does not replace the four-enemy Stage 1 encounter. Do not claim a bespoke boss model, authored boss skill, or boss gameplay is complete until verified in Unity.

The current campaign generator creates five enemies per stage and reuses a small model/name list. The four-enemy lineup above is a new prototype data requirement; it is not currently implemented as an authored Stage 1 definition.

## 4. Combat rules

### Team and positions
- Prototype: four units per team; slots 0–1 are front row and slots 2–3 are back row.
- Final target: five units per team; slots 0–1 front and 2–4 back.
- A null or dead unit cannot act or be selected as a target.

### Actions and damage
- Simulation fixed step: 0.1 seconds; maximum battle time: 120 seconds.
- Each unit's action meter advances using its SPD stat; one action occurs per full meter.
- Use the unit's active skill when its cooldown reaches zero; otherwise use a basic attack.
- Single-enemy targeting selects the first living front-row enemy, then the first living enemy in the opposing team.
- Existing target types remain: SingleEnemy, AllEnemies, LowestHpAlly, AllAllies.
- Damage formula in the current engine: ATK × skill power × (100 / (100 + target DEF)); apply faction advantage and then critical multiplier.
- Critical multiplier: 1.5×. Core faction advantage: +25% damage according to Fortress > Wild > Forest > Shadow > Fortress; Divinity and Abyss counter each other.
- Healing is based on healer ATK × skill power and cannot exceed target MaxHP.
- Attacking grants 20 energy; being hit grants 10 energy; using an active skill grants 10 energy. Ultimate becomes available at 100 energy and consumes all 100.
- Auto Ultimate is on by default. If disabled, the player may request an Ultimate for a living unit only when it has 100 energy.
- x1/x2/x4 changes presentation/simulation stepping speed only; it must not change the battle result for the same seed.

### End conditions
- Victory: opposing team has no living units.
- Defeat: player team has no living units.
- Timeout at 120 seconds: defender/team B wins, matching current `BattleSim`.
- The result screen must show Victory/Defeat, elapsed battle time, and reward only when a campaign victory is accepted.

### Not in the first 4v4 slice
Buffs, debuffs, shields, damage-over-time, faction team synergy, bespoke boss mechanics, and PvP are not currently implemented in `BattleSim`. They remain later combat-system work; do not present them as working in the prototype. The GDD does specify team synergy (+15% HP for a 3+2 faction split; +25% ATK/HP for mono-faction), so it must be implemented and tested before claiming the full combat design is complete.

## 5. Stage and rewards

**Stage 1 — The Broken Gate**
- First and only authored campaign stage required for the vertical slice.
- Four enemies from the table above.
- Recommended power and enemy stats must be playtested; current generated scaling is placeholder data.
- On victory, use the existing `CampaignScreen.ApplyVictory` formula as the initial test reward:
  - Gold: 150 + 18 × stage = **168**
  - EXP: 90 + 12 × stage = **102**
  - Gear Materials: 2 + floor(stage / 10) = **2**
- Victory advances `HighestCleared` only when the stage is the next sequential stage.
- Defeat grants no clear reward and does not advance progress.

Boss fixture:
- Use the existing Stage 10 Guardian / Dragon mapping for a separate boss test fixture.
- Do not unlock Stage 10 directly in normal progression; campaign stages remain sequential.
- Stage 10 reward under the current formula would be 330 Gold, 210 EXP, and 3 Gear Materials, but it is not part of the Stage 1 prototype reward.

## 6. Source compatibility review

| Source | Current finding | C0 implication |
|---|---|---|
| `Assets/Scripts/Combat/BattleSim.cs` | Accepts up to five units per team; four-unit teams are representable. | 4v4 is structurally supported; add a dedicated 4v4 test. |
| `Assets/Scripts/Combat/Types.cs` | Has six factions, three roles, four target types, attack/skill/ultimate/heal/death/end events. | Current simple combat rules above map to existing types. |
| `Assets/Scripts/Core/HeroCatalog.cs` | Contains the four selected hero records and model IDs. | No new hero records required for the prototype. |
| `Assets/Scripts/Meta/Campaign/CampaignContent.cs` | Generates five enemies per stage; Stage 10 has a Dragon placeholder. | Must add authored four-enemy Stage 1 data; the proposed lineup is not currently present. |
| `Assets/Scripts/UI/Screens/CampaignScreen.cs` | Victory reward formula matches the Stage 1 test reward above. `TryEnter` currently consumes 5 energy. | There is a design conflict that must be resolved before C0 can be marked fully compatible. |
| `Assets/Tests/PlayMode/MvpBattlePlayModeTests.cs` | Existing PlayMode test covers 5v5 only. | Add a separate 4v4 completion/result test; current test does not prove 4v4. |

### Blocking design conflict: campaign energy

`docs/GDD.md` says energy is spent only on farming/events and **never on the normal campaign**, while `CampaignScreen.TryEnter` currently consumes 5 energy for every stage. C0 proposes that the normal Stage 1 campaign encounter costs **0 energy**, following the GDD. This requires a later client-code correction; no code was changed in C0. Product owner must confirm this rule before implementation.

## 7. C0 acceptance checklist

- [x] Existing combat types and deterministic battle rules inspected.
- [x] Four existing hero catalog records selected.
- [x] Existing enemy model IDs and Stage 10 boss placeholder inspected.
- [x] Existing Stage 1 reward values calculated from the current formula.
- [ ] Product owner confirms the campaign-energy rule (GDD says 0; current code charges 5).
- [ ] Manus confirms no conflict with M0 and the planned Unity scene flow.
- [ ] Dedicated 4v4 EditMode/PlayMode coverage is added and actually run in Unity (not part of this documentation-only task).

**Verification statement:** This is a source-based design review only. No Unity Editor, Unity Compile, Play Mode, or Android build was run. It does not close or replace M0.
