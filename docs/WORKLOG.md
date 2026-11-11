## 2026-11-11 - Exact 3D asset implementation pass
**Done in this branch**
- Closed the previous 3D PR #5 without merging it. This replacement branch starts from the prototype setup branch.
- Added `CharacterArtGenerator.cs`: Unity Editor-generated low-poly humanoid mesh assets for four visually distinct factions and one separate enemy silhouette; Humanoid Avatar skeletons; Idle/Run/Attack/Skill/Ultimate/Hit/Death clips; Animator Controllers; FBX exports for every hero, enemy, biome and prop using Unity FBX Exporter 5.1.1; faction skill/ultimate Particle System prefabs; five biome environment prefabs; gate/crystal props; 128x128 portraits, faction icons and UI frame.
- Configured exported hero/enemy FBX files with ModelImporter Humanoid rig import settings; the validator checks imported Avatar subassets and the FBX triangle budgets.
- Added direct `HeroDefinition.prefab`, `portrait` and `factionIcon` references. Added `StageDefinition` with direct enemy/environment prefab references and Stage_001 setup.
- Updated `BattleView` to instantiate the character prefabs in the five formation slots per team and dispatch BattleEvents to each `HeroView`. Updated the prototype Battle scene to create two front-row and three back-row slots per team; Setup Prototype detects and rebuilds the older cube-based Battle scene.
- Connected biome selection through `CampaignProgress.BiomeOf(stageNumber)`, VFX colors through `UIStyle.FactionColor`, and portrait binding methods in HeroesScreen/SummonScreen.
- Added `docs/ASSETS.md` with source/license register, exact budgets, folder paths and import rules. Added `Sands > Validate Assets` and invoked it before Android BuildPlayer. Portrait/faction icon importers target Android ASTC 6x6.

**Not verified / uncertainty**
- Unity 2022.3.62f3 was not available in this session. The Editor generator has not been run, so the generated Mesh, Avatar, AnimationClip, AnimatorController, Prefab, Sprite and Scene assets have not yet been materialized in the Unity project.
- No Unity compile, Humanoid Avatar validation, animation playback, triangle-budget menu run, PlayMode test, Battle visual inspection or Android APK build has been performed. Therefore the 3D work is NOT marked complete.
- HeroesScreen and SummonScreen remain existing UI stubs; this pass adds portrait/icon data and binding methods but does not build their full card layouts.
- Stage_001 is prototype data with four instances of the same original enemy prefab. Production stage formations and five-biome campaign content remain to be authored.
- The generated character meshes use original Unity procedural geometry and flat-color materials, not third-party FBX/GLB downloads. No commercial-game assets or names were used.

**Next**
1. Open Unity 2022.3.62f3 and run `Sands > Setup Prototype`.
2. Inspect all four hero prefabs, the enemy, Humanoid Avatar validity, seven animation clips, VFX, portraits, props and five environments.
3. Run `Sands > Validate Assets`, then enter Battle and visually verify the 4v4 models, front/back formation and stage biome.
4. Run PlayMode tests and the Android build only after Unity compile and visual checks pass.

# Work Log (newest first)

## 2026-11-11 - Foundations pass (Ahmed + Claude)
**Done**
- Added all foundation folders and files per the project plan (see ARCHITECTURE.md): Meta (economy, roster, gacha, idle, campaign, quests, save), Services, Bootstrap, Presentation, UI framework + 5 screen stubs, Localization, Editor build script, Server skeleton, docs.
- Every source file now has a header: owner role, responsibility, what to work on, status.
- Engine-free code (Combat + Meta) compiled and all tests run outside Unity with mono; 16 EditMode tests pass under mono (Combat + Meta).

**Not verified**
- Project never opened in Unity 2022.3.62f3 (MonoBehaviour/ScriptableObject/Editor files untested there).
- No `.meta` files, no scenes (Boot/Home/Battle must be created in the editor).

**Next**
1. Open in Unity, fix compile errors, run EditMode tests, commit `.meta`.
2. Create Boot/Home/Battle scenes; add GameBootstrap to Boot.
3. 4 prototype heroes (one per faction) + skills as ScriptableObjects; battle scene using BattleView.
4. CI build (secrets needed: UNITY_LICENSE/EMAIL/PASSWORD).

## 2026-11-11 - Combat core + scaffold (Ahmed + Claude)
- Repo created, scaffold pushed; deterministic `BattleSim`, hero/skill ScriptableObjects, 6 tests; HANDOFF.md and the plan PDF added.
