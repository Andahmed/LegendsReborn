# Legends Reborn

Unity mobile Idle RPG for Android. Working title: **Sands of Legends**.

- Unity Editor: 2022.3.62f3
- Android target: IL2CPP / ARM64
- Combat simulation: engine-free deterministic C#
- Runtime UI: generated from code so the initial Home scene has no hand-wired UnityEvents

## First launch in Unity

1. Open this repository with Unity 2022.3.62f3.
2. Run **Sands → Setup MVP Home Scene**. This creates Assets/Scenes/Home.unity and registers it in Build Settings.
3. The repository already contains 21 Quaternius CC0 FBX files for the modular castle, four monster types and six nature props; Unity imports them automatically.
4. For the six humanoid character rigs, run **Legends Reborn → Art → Install MVP Art** and select the local Free3D folder from the legacy LegendsStyleRPG checkout. This copies the character FBXs into Resources/HomeHeroes; the importer no longer downloads from a private raw GitHub URL.
5. Wait for the FBX import to complete, then open Home.unity and press Play.

The Android build menu also creates/registers the Home scene before calling BuildPlayer. Android SDK/NDK, signing and device settings still need to be configured in the local Unity installation.

## Current MVP systems

- Five starter heroes and a 20-hero catalog with faction, role, rarity, combat stats, skills and ultimates.
- 200 generated campaign stages: five biomes × 40 stages, with boss encounters every ten stages.
- 5v5 deterministic auto-battle, 2 front + 3 back positions, x1/x2/x4 speed and optional manual ultimates.
- Battle presentation loads imported character/monster FBX models, shows combat numbers and updates the result/rewards. Four monster FBXs and 17 environment FBXs are included; humanoid rigs are imported from the local legacy Free3D folder.
- Local save includes roster, five-slot formation, six equipment slots per hero, currencies, pity, energy, campaign and daily quests. Heroes have level, star-up and ascension progression.
- The five approved starter-hero portrait PNGs are bundled and shown in the roster; these are 2D portraits, not rigged 3D character models.
- Home menu routes to Campaign, Heroes, Formation, Summon, Equipment, Backpack and feature-status screens. Screen content uses a touch-scroll viewport for long rosters and battle controls.
- Idle rewards capped at 12 hours; daily quest progress and claims.
- FastAPI development backend with guest accounts, persistent SQLite profiles, server-side summon RNG and summon audit history.

## Run the development server

From the Server folder:

~~~bash
pip install -r requirements.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000
python -m unittest discover -s tests -v
~~~

Implemented routes: GET /health, POST /auth/guest, GET /profile, POST /summon, GET /summon/history. Arena and weekly boss status routes explicitly report that those systems are still locked.

The Unity client currently uses its local preview summon path; it is not yet connected to the server. Do not treat local client RNG as production-safe.

## Architecture

- Assets/Scripts/Combat: pure C# simulation and combat events.
- Assets/Scripts/Core: hero and skill definitions/catalog.
- Assets/Scripts/Meta: roster, economy, campaign, daily quests, idle rewards, gacha, equipment and save state.
- Assets/Scripts/Presentation: actor models, battle events and floating damage numbers.
- Assets/Scripts/UI: Home HUD and runtime screen/game-flow controller.
- Assets/Editor: Home scene and art installers, plus Android build entry point.
- Server: FastAPI + SQLite development backend.

See docs/HANDOFF.md for implemented work, limitations and verification status.

## Verification limits

This repository update has not been opened in Unity Editor in this environment. Unity compilation, FBX import, Play Mode, touch/layout testing, Android APK generation and device testing remain mandatory before calling the MVP playable. Generated content and combat balance are development values, not final economy tuning.
