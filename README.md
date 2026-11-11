# Legends Reborn

Unity mobile Idle RPG for Android. Working title: **Sands of Legends**.

- Unity Editor: 2022.3.62f3
- Android target: IL2CPP / ARM64
- Combat simulation: engine-free deterministic C#
- Runtime UI: generated from code so the initial Home scene has no hand-wired UnityEvents

## First launch in Unity

1. Open this repository with Unity 2022.3.62f3.
2. Run **Sands → Setup MVP Home Scene**. This creates Assets/Scenes/Home.unity and registers it in Build Settings.
3. The Home castle is assembled from the 11 Quaternius Modular Dungeon CC0 FBX modules bundled in Assets/Resources/HomeCastle.
4. Run **Legends Reborn → Art → Install All CC0 3D Models** only if models or portraits are missing. The installer checks bundled files first, asks for a Quaternius source folder only when required FBX files are absent, and copies the five approved portrait PNGs from `ArtSource/Heroes/images/` into `Assets/Resources/HeroPortraits/`.
5. Wait for FBX import to complete, then open Home.unity and press Play.

The HomeWorldStage assembles the courtyard, gate, walls, towers, banners, stairs, torches and nature props from imported FBX meshes. It has no primitive-cube castle fallback. Missing required FBX assets produce explicit errors instead of silently substituting greybox geometry. The Heroes screen loads the five approved portraits (`warrior`, `paladin`, `berserker`, `healer`, `sharpshooter`) from `Resources/HeroPortraits`.

The Android build menu also creates/registers the Home scene before calling BuildPlayer. Android SDK/NDK, signing and device settings still need to be configured in the local Unity installation.

## Current MVP systems

- Five starter heroes and a 20-hero catalog with faction, role, rarity, combat stats, skills and ultimates.
- 200 generated campaign stages: five biomes × 40 stages, with boss encounters every ten stages.
- 5v5 deterministic auto-battle, 2 front + 3 back positions, x1/x2/x4 speed and optional manual ultimates.
- Battle presentation loads imported character/monster FBX models, shows combat numbers and updates the result/rewards.
- Local save includes roster, five-slot formation, six equipment slots per hero, currencies, pity, energy, campaign and daily quests.
- Home menu routes to Campaign, Heroes, Formation, Summon, Equipment, Backpack and feature-status screens.
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

This change has not been opened in Unity Editor in this environment. Unity compilation, FBX import, materials, pivots, camera framing, Play Mode, touch/layout testing, Android APK generation and device testing remain mandatory before calling the MVP playable. The Home castle is a modular FBX assembly, not a bespoke sculpted single-mesh castle. Generated content and combat balance are development values, not final economy tuning.
