# Work Log (newest first)

## 2026-11-11 - Prototype assets, scenes, and PlayMode smoke test (ChatGPT)
**Changed**
- Added `Assets/Editor/PrototypeSetup.cs` with **Sands > Setup Prototype**; it creates missing hero/skill ScriptableObjects, Boot/Home/Battle scenes, and Build Settings entries while preserving existing assets/scenes.
- Added four original prototype HeroDefinition assets at setup time (Fortress/Wild/Forest/Shadow), with active and ultimate SkillDefinition assets under `Assets/Data/Skills`.
- Added `Assets/Scripts/Presentation/PrototypeBattleController.cs`; generated Battle scene wires BattleView to a 4v4 cube battle, HP bars, status, and x1/x2/x4 buttons.
- Added `Assets/Tests/PlayMode/PrototypeBattlePlayModeTests.cs` and its PlayMode asmdef; the smoke test steps a 4v4 BattleSim until it ends.
- Updated `Assets/Editor/BuildScript.cs` so the GameCI build entry point ensures missing prototype scenes/assets before collecting Build Settings scenes.
- Added Unity Test Framework `1.1.33` to `Packages/manifest.json`; updated Data/Scenes readmes and Architecture map.

**Not verified / uncertainty**
- Unity Editor was not available in this session. The menu command has not been executed, so generated .asset/.unity files, serialized references, visual layout, and Build Settings have not been observed.
- The PlayMode test has not been run in Unity Test Runner.
- Android APK has not been built; CI secrets and the workflow run are still pending.
- The prototype uses primitive cubes and placeholder UI only; no production art or combat/economy rule changes were introduced.

**Next**
1. Open the project in Unity 2022.3.62f3 and run **Sands > Setup Prototype**.
2. Inspect Boot/Home/Battle, verify references and controls, then run EditMode and PlayMode tests.
3. Merge the independent review/CI PRs as appropriate, add Unity Actions secrets, and run the Android workflow.

## 2026-11-11 - Foundations pass (Ahmed + Claude)
**Done**
- Added all foundation folders and files per the project plan (see ARCHITECTURE.md): Meta (economy, roster, gacha, idle, campaign, quests, save), Services, Bootstrap, Presentation, UI framework + 5 screen stubs, Localization, Editor build script, Server skeleton, docs.
- Every source file has a header: owner role, responsibility, what to work on, status.
- Engine-free code (Combat + Meta) compiled and all tests run outside Unity with mono; 16 EditMode tests pass under mono (Combat + Meta).

**Not verified**
- Project never opened in Unity 2022.3.62f3 (MonoBehaviour/ScriptableObject/Editor files untested there).
- No `.meta` files, no scenes (Boot/Home/Battle must be created in the editor).

**Next**
1. Open in Unity, fix compile errors, run EditMode tests, commit `.meta`.
2. Create Boot/Home/Battle scenes; add GameBootstrap to Boot.
3. 4 prototype heroes (one per faction) + skills as ScriptableObjects; battle scene using BattleView.
4. CI build (secrets needed: UNITY_LICENSE/UNITY_EMAIL/UNITY_PASSWORD).

## 2026-11-11 - Combat core + scaffold (Ahmed + Claude)
- Repo created, scaffold pushed; deterministic `BattleSim`, hero/skill ScriptableObjects, 6 tests; HANDOFF.md and the plan PDF added.
