## 2026-11-11 - Procedural 3D prototype pipeline (ChatGPT)
**Changed**
- Added docs/ASSETS.md with source/license register, polygon budgets, naming, scale, texture and folder rules. All prototype assets are generated from repository scripts; no third-party or commercial-game assets were imported.
- Added Assets/Editor/ArtPrototypeSetup.cs to generate four original low-poly capsule/sphere hero prefabs, faction skill/ultimate particle prefabs, and five placeholder biome prefabs.
- Added a GameObject prefab reference to HeroDefinition and implemented HeroView event handling for Attack, Skill, Ultimate, Hit and Death using prototype motion/VFX.
- Updated PrototypeBattleController to instantiate each hero prefab into the battle presentation instead of leaving the placeholder cube visible.
- Added five named battle slot transforms per side in the generated Battle scene (the prototype simulation still uses four distinct heroes per team; the fifth slot is reserved).
- Added Sands > Validate Assets; Android build entry point runs the validator before BuildPlayer.

**Not verified / uncertainty**
- Unity Editor was not run. Generated prefabs, materials, particle assets, and biome prefabs have not been created on disk in this session; they are produced by Sands > Setup Prototype / Sands > Setup 3D Prototype.
- The four character prototypes are primitive low-poly meshes, not FBX/GLB Humanoid rigs. They do not contain authored Idle/Run/Attack/Skill/Ultimate/Hit/Death clips; HeroView currently uses procedural placeholder motion and has optional Animator trigger forwarding.
- Five biome prefabs are placeholders and are not yet selected by CampaignProgress.BiomeOf(stage); no stage-environment binding was added.
- Asset validation has not been executed in Unity or CI. No Battle scene visual test has been performed; therefore 3D is NOT marked complete.

**Next**
1. Open Unity 2022.3.62f3 and run Sands > Setup Prototype.
2. Inspect generated hero prefabs and Battle scene, then run Sands > Validate Assets.
3. Run PlayMode tests and verify that the four generated hero prefabs visibly animate in Battle.
4. Replace primitive characters with licensed/registered Humanoid FBX/GLB rigs and all seven production animation clips; connect biome selection and stage enemies.

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
