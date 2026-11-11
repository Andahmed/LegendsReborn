# Work Log (newest first)

## 2026-11-12 - Architecture status correction (GPT Lane B)
- Updated Server/app/main.py, Equipment.cs, and HeroView.cs status descriptions to reflect implemented MVP behavior and explicitly list remaining gaps.
- Not verified: no Unity Editor compile/Play Mode/Android build; no production deployment verification. Engine-Free Tests run requested on this documentation branch.

## 2026-11-12 - A0 Unity font crash guard
**Done:** Replaced direct Arial loading with LegacyRuntime.ttf plus guarded Arial fallback in GameFlowController and HomeScreen; assigned the TextMesh font and font material in DamageNumbers.
**Not verified:** لم يتم التحقق في Unity؛ CI/Android workflow was not run in this environment.

## 2026-11-12 - B0 Engine-Free CI (GPT Lane B)
- Engine-Free Tests workflow added; GitHub Actions run #3 passed (22 tests, 0 failures). Unity Editor/Play Mode/Android build not run.

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
