# Work Log (newest first)

## 2026-11-12 - Server-authoritative summon client wiring
**Done**
- Added `Assets/Scripts/Services/SummonService.cs`: UnityWebRequest coroutine client for `POST /auth/guest` and `POST /summon`, configurable `serverBaseUrl`, memory-only bearer token, and JsonUtility-safe DTOs.
- Changed the live `GameFlowController` summon path to avoid `GachaSystem.Pull`, `SeededRng`, and local gem spending; it applies server results plus wallet, pity, and roster snapshot.
- Kept `SummonScreen.TryPull` explicitly preview/test-only and unused by the live path.
- Added `profile_state` DTO projection in the FastAPI summon response and a server test proving extra client seed input is rejected and the response snapshot is authoritative.
- Updated `Server/README.md` with the contract and verification limits.

**Not verified**
- لم يتم التحقق في Unity.
- Android, network deployment, TLS, and a running Unity scene were not tested.

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
