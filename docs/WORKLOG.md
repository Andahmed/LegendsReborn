## 2026-11-11 - Server-authoritative idle and daily claims
**Changed**
- Added server UTC-based idle claim endpoint with a 12-hour cap and transactional gold/EXP updates.
- Added once-per-UTC-day login reward claim and server-clock energy regeneration (1 per 5 minutes, cap 20).
- Added focused unit tests for idle accrual caps, elapsed-time handling, stage clamping and UTC day keys.
- Updated Server README and HANDOFF; Unity client is not connected yet.

**Verification**
- Static source review only. Python unittest execution was not available in this session.
- Unity compilation, Play Mode and Android build: لم يتم التحقق في Unity.

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
