# Work Log (newest first)

## 2026-11-11 - Android APK CI workflow (ChatGPT)
**Changed**
- Added `.github/workflows/android-apk.yml`: GameCI Unity Builder v4, Unity `2022.3.62f3`, Android target, manual `workflow_dispatch` and `pull_request` triggers.
- Workflow calls `SandsOfLegends.EditorTools.BuildScript.BuildAndroid` and uploads `Builds/SandsOfLegends.apk` as a 14-day artifact.

**Not verified / uncertainty**
- Workflow has not run yet. Ahmed must add repository Actions secrets: `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD`.
- This PR is based on main, where the Boot/Home/Battle scenes have not yet been created; an APK build may fail until the separate prototype-setup PR adds build scenes.
- Unity licensing and GameCI container compatibility are not verified in this session.

**Next**
1. Add the three repository secrets in Settings > Secrets and variables > Actions.
2. Merge the prototype setup PR when reviewed, then run this workflow manually and inspect the APK artifact.

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
