# Work Log (newest first)

## 2026-11-12 - Unity activation summary shell fix
**Done:** Removed shell command-substitution backticks from the missing-secret summary so the manual readiness workflow prints secret names and documentation path literally.
**Not verified:** The workflow must be run again after this correction; repository secrets are still missing and Unity license validity is unverified.

## 2026-11-12 - Unity Personal activation workflow correction
**Done:** Replaced the deprecated `unity-request-activation-file` action with a manual secret-readiness check. Updated the instructions to the currently supported Unity Hub Personal-license flow and documented the OS-specific `.ulf` locations.
**Not verified:** The actual Unity license remains unvalidated until GameCI runs the Android build with the repository secrets.

## 2026-11-12 - A3b Hero portrait and tint metadata
**Done:** Wired `HeroCardData.PortraitId` to the roster portrait resource lookup and `TintHex` to the portrait plate and hero-card accent. Missing portrait IDs safely omit the portrait; missing/invalid tints fall back to the existing teal accent.
**Not verified:** Unity Editor import, roster rendering and Android runtime presentation remain unverified. Engine-Free CI does not compile Unity UI code.

## 2026-11-12 - B3 Hero visual metadata (GPT Lane B)
- Added TintHex and PortraitId metadata for the 20 hero catalog entries. Engine-Free C# CI evidence is pending; no Unity verification.
## 2026-11-12 - A7 Boot to Home game flow
**Done:** Added a generated Boot scene with the persistent GameBootstrap, automatic Boot -> Home routing, HomeScreen binding after scene load, and Build Settings order Boot then Home. Battle remains the existing in-Home presentation; victory/defeat results return to Home or offer the next stage.
**Not verified:** لم يتم التحقق في Unity؛ generated scene contents, runtime sceneLoaded timing, Play Mode battle/result flow and Android build are pending CI/editor verification.

## 2026-11-12 - A6 Local summon screen
**Done:** Confirmed and documented the existing local x1/x10 summon UI, 300/2,700 Gem costs, pity display and result presentation through SummonScreen.TryPull/GachaSystem. Server integration remains intentionally deferred; no Server, Meta or HeroCatalog files changed.
**Not verified:** لم يتم التحقق في Unity؛ summon button interaction, results layout and local save persistence require Unity Play Mode verification.

## 2026-11-12 - A5 Roster details and progression UI
**Done:** Extended the existing Heroes screen cards with level-scaled HP/ATK/DEF/SPD details from the existing hero data; list, portraits, level-up, star-up and ascension continue to use the existing Meta-backed operations. No Combat, Meta, Server or HeroCatalog files changed.
**Not verified:** لم يتم التحقق في Unity؛ layout, portrait import and button interaction require Unity Play Mode/device verification.

## 2026-11-12 - A3 Battle event presentation
**Done:** Added world-space HP bars to `HeroView`, updated them from damage/heal/death `BattleEvent` values, and retained the existing basic attack lunge, hit shake, ultimate motion and defeat fall presentation.
**Not verified:** لم يتم التحقق في Unity؛ Battle scene rendering, Canvas layout, imported model presentation and Play Mode execution were not run in this environment.

## 2026-11-12 - A2 Home 3D hero showcase
**Done:** Updated `HomeWorldStage` to load Cleric, Monk, Ranger, Rogue, Warrior and Wizard from `Resources/HomeHeroes/` and place the six models across the Home castle dais.
**Not verified:** لم يتم التحقق في Unity؛ FBX import, generated `.meta` files, Home scene framing and runtime presentation were not run in this environment. The A2 branch expects the six binaries from A1 PR #33.

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
