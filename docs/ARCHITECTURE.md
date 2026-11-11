# Architecture / File Map

Dependency rule: `Combat` (engine-free) <- `Meta` (engine-free) <- `Services` / `Core` (Unity) <- `Bootstrap`, `Presentation`, `UI`.

Server (`Server/`) can re-run `Combat` + `Meta` logic for authority/anti-cheat.

| File | Owner role | Responsibility | Work on | Status |
|---|---|---|---|---|
| `Assets/Scripts/Combat/Types.cs` | Client Programmer - Combat | Combat data types and deterministic RNG. | Add new fields only with approved GDD change; remain engine-free. | Implemented |
| `Assets/Scripts/Combat/BattleSim.cs` | Client Programmer - Combat | Deterministic fixed-step 2-front/3-back auto battle. | Add approved status effects, team synergy and targeting rules; remain deterministic and engine-free. | Implemented (placeholder balance) |
| `Assets/Scripts/Core/HeroDefinition.cs` | Systems Designer | Designer-authored hero/skill ScriptableObjects converted to combat specs. | Populate 20 heroes and connect art references. | Implemented (definition types only) |
| `Assets/Scripts/Meta/Heroes/HeroInstance.cs` | Client Programmer - Systems | Owned hero state, progression and serialized equipment collection. | Replace progression placeholders with approved economy tables. | Implemented (placeholder curves) |
| `Assets/Scripts/Meta/Heroes/Roster.cs` | Client Programmer - Systems | Hero collection, Spirit Source ceiling and equipment operations. | Add approved hero progression and 5-slot team formation. | Implemented (equipment foundation) |
| `Assets/Scripts/Meta/Equipment/Equipment.cs` | Systems Designer | Six equipment slots, serializable item stats/set identity, equip/replace/unequip rules. | Add authored items, approved set effects, drops and upgrade costs. | Foundation implemented |
| `Assets/Scripts/Meta/Save/SaveData.cs` | Client Programmer - Systems | Single versioned save model and migrations. | Add a migration for each persisted schema change. | Version 2; equipment migration |
| `Assets/Scripts/Meta/GameState.cs` | Client Programmer - Systems | Aggregate wallet, roster, pity, energy, campaign, quests and idle state. | Add remaining systems with matching save migration. | Implemented (partial MVP) |
| `Assets/Scripts/Meta/Gacha/GachaSystem.cs` | Systems Designer | Deterministic summon and pity logic. | Add server authority, banner data and official rates display. | Implemented (test rates) |
| `Assets/Scripts/Meta/Idle/IdleRewards.cs` | Economy Designer | Offline rewards with 12-hour cap. | Replace placeholders with approved economy ledger. | Implemented (placeholder rates) |
| `Assets/Scripts/Meta/Campaign/CampaignProgress.cs` | Systems Designer | Sequential 200-stage progress model. | Connect 200 authored stage definitions and reward/difficulty data. | Progress model only |
| `Assets/Scripts/Meta/Quests/DailyQuests.cs` | Systems Designer | Five daily quest progress/claim model. | Add approved quest definitions and reward data. | Placeholder content |
| `Assets/Scripts/Meta/Arena/ArenaSystem.cs` | Systems Designer | Async PvP arena. | Implement server-authoritative matchmaking, results and season rewards. | Stub |
| `Assets/Scripts/Meta/Boss/WeeklyBoss.cs` | Systems Designer | Weekly boss mode. | Implement attempts, scoring and reward tiers from approved data. | Stub |
| `Assets/Scripts/Presentation/BattleView.cs` | Client Programmer - Combat | Plays deterministic battle events at x1/x2/x4. | Connect real hero views, VFX, HUD and manual ultimate. | Simulation playback only |
| `Assets/Scripts/Presentation/HeroView.cs` | Animation/VFX Artist | Model, animator, HP anchor and battle reactions. | Bind licensed models/animations and event reactions. | Stub |
| `Assets/Scripts/Presentation/DamageNumbers.cs` | Animation/VFX Artist | Pooled damage/heal/crit feedback. | Implement pooled TMP labels and camera-follow. | Stub |
| `Assets/Scripts/UI/Screens/HomeScreen.cs` | Client Programmer - UI | Main hub and idle/quest/energy entry points. | Wire live game state and navigation. | Stub |
| `Assets/Scripts/UI/Screens/HeroesScreen.cs` | Client Programmer - UI | Hero roster, details, equipment and formation. | Wire roster and equipment UI. | Stub |
| `Assets/Scripts/UI/Screens/SummonScreen.cs` | Client Programmer - UI | Single/x10 summon, pity and official rates. | Wire gacha and results presentation. | Stub |
| `Assets/Scripts/UI/Screens/CampaignScreen.cs` | Client Programmer - UI | Five-biome campaign map and battle launch. | Connect stage definitions, progress and rewards. | Stub |
| `Assets/Scripts/UI/Screens/BattleHud.cs` | Client Programmer - UI | Battle speeds, ultimates, HP and skip. | Bind to BattleView and BattleSim. | Stub |
| `Server/app/main.py` | Server Programmer | FastAPI server entry point. | Add authenticated account/profile/inventory/gacha/arena/boss endpoints. | Health-only stub |
| `docs/GDD.md` | Systems Designer | Condensed design reference derived from the project plan. | Complete review and freeze after reading full PDF. | Draft |
| `docs/ECONOMY.md` | Economy Designer | Currency sources/sinks, progression and summon economics. | Complete day 1-14 ledger and stage 1-40 curve; no tuning without test data. | Draft |
| `docs/STYLE_GUIDE.md` | Concept/UI Artist | Stylized 3D and Arabic-inspired visual direction. | Approve mood boards, palette, character/environment/VFX specifications. | Stub |
| `docs/QA_PLAN.md` | QA Engineer | Device matrix and release QA. | Define smoke tests and real-device coverage. | Stub |
