# Work Log (newest first)

## 2026-11-11 - Unity readiness, Android CI, and prototype setup (ChatGPT)
**Changed**
- PR #1: reviewed all 35 tracked C# files and assembly definitions; added Unity Test Framework `1.1.33` for the existing EditMode test assembly.
- PR #2: added `.github/workflows/android-apk.yml` using GameCI Unity Builder v4, Unity `2022.3.62f3`, Android target, `workflow_dispatch` and `pull_request`; APK artifact upload is configured.
- PR #3: added `Sands > Setup Prototype`, runtime 4v4 battle presentation, prototype HeroDefinition/SkillDefinition generation, Boot/Home/Battle scene generation, Build Settings registration, and Boot-to-Home/Home-to-Battle prototype navigation.
- PR #4: added the separate PlayMode test assembly and a 4v4 BattleSim completion smoke test.
- Updated the Android build entry point to create missing prototype assets/scenes and ensure the APK output folder exists before building.
- Kept Combat and Meta engine-free; no combat rules or economy values changed. All code files added/changed include the required ownership headers.

**Not verified / uncertainty**
- No Unity Editor compile or Test Runner was executed in this session. Source inspection is not a Unity compile result.
- Read `docs/GDD.md`; the 605 KB `docs/Idle_RPG_Project_Plan.pdf` exists, but GitHub's PDF viewer returned `Unable to render code block` and the raw download failed, so its 18 pages could not be extracted.
- The prototype setup menu has not been executed; generated Unity assets/scenes, serialized references, and visual appearance are not yet observed.
- The PlayMode test and GameCI Android workflow have not run. Add repository Actions secrets `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD` before CI can authenticate.

**Next**
1. Review the four independent PRs and merge only after approval.
2. Open Unity `2022.3.62f3`, run **Sands > Setup Prototype**, inspect the three scenes, and run EditMode/PlayMode tests.
3. Add the three Actions secrets and run the Android APK workflow; inspect its artifact and logs.

## 2026-11-11 - Foundations pass (Ahmed + Claude)
**Done**
- Added all foundation folders and files per the project plan (see ARCHITECTURE.md): Meta (economy, roster, gacha, idle, campaign, quests, save), Services, Bootstrap, Presentation, UI framework + 5 screen stubs, Localization, Editor build script, Server skeleton, docs.
- Every source file has a header: owner role, responsibility, what to work on, status.
- Engine-free code (Combat + Meta) compiled and all tests run outside Unity with mono; 16 EditMode tests pass under mono (Combat + Meta).

**Not verified**
- Project never opened in Unity 2022.3.62f3 (MonoBehaviour/ScriptableObject/Editor files untested there).
- No `.meta` files committed; no scenes (Boot/Home/Battle must be created in the editor).

**Next**
1. Open in Unity, fix compile errors, run EditMode tests, commit `.meta`.
2. Create Boot/Home/Battle scenes; add GameBootstrap to Boot.
3. 4 prototype heroes (one per faction) + skills as ScriptableObjects; battle scene using BattleView.
4. CI build (secrets needed: UNITY_LICENSE/UNITY_EMAIL/UNITY_PASSWORD).

## 2026-11-11 - Combat core + scaffold (Ahmed + Claude)
- Repo created, scaffold pushed; deterministic `BattleSim`, hero/skill ScriptableObjects, 6 tests; HANDOFF.md and the plan PDF added.
