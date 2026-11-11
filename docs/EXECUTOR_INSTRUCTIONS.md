# Executor Instructions — Sands of Legends

These instructions are mandatory for all implementation work in `Andahmed/LegendsReborn`. They combine the original execution rules with the mandatory legacy-repository research and asset-transfer addendum below. Follow the phases in order; do not claim completion without the stated evidence.

## 0. Goal, legal boundary, and project identity

- Target feel: a mobile idle RPG in the broad mechanical style of *Legends Reborn: Last Battle*: automatic 5v5 combat, Summon, campaign, idle rewards, equipment, and formation.
- Copy **mechanics only**. Do not copy names, heroes, images, audio, text, or other protected expression from the original game. Follow `docs/HANDOFF.md`.
- Allowed assets: Quaternius CC0 assets and original assets created for this project.
- Active project: `Andahmed/LegendsReborn`. Keep it strictly separate from `Andahmed/LegendsStyleRPG`. The latter is a **public legacy source repository** for permitted 3D assets and presentation techniques only; it is not the active game and its gameplay/data systems must not be migrated.
- The legacy source repository's Unity project is under `GameClient/`. Treat it as a source of assets and ideas, not as a verified working build. Its last recorded Unity credential/build status is not proof that it compiles.

## 1. Strict rules (a violation means the work is rejected)

1. Never say “ready”, “working”, or “built” without actually running the relevant verification. If Unity is unavailable, write exactly: `لم يتم التحقق في Unity`.
2. No capsules or cubes as character or castle substitutes in production. Any temporary fallback in `HeroView` is test-only and must be removed from the production path once the real FBX models are present.
3. Keep `Assets/Scripts/Combat` and `Assets/Scripts/Meta` independent of `UnityEngine`. Visual systems listen to `BattleEvent` and stay outside the simulation.
4. One save system and one currency system. Production Summon is server-authoritative; local RNG is preview-only.
5. Modify existing files and screens (`GameFlowController`, `HeroView`, etc.). Do not add pages or files unless necessary; justify every necessary new file in one sentence.
6. Do only the current requested task. Record out-of-scope issues in the report; do not implement them.
7. Every added FBX or PNG must have its Unity `.meta` file committed.
8. Commit complete usable code. No `TODO`, “finish this yourself”, or unfinished stubs.
9. Do not merge pull requests into `main` without explicit user approval.
10. Keep the two repositories' assets, code, settings, branches, and reports separate. Never use the legacy repository's hero/gameplay/catalog/campaign/tower/guild systems in the new project.

## 2. Mandatory research before any code changes

Before writing or changing implementation code:

1. Search the entire `LegendsReborn` repository for every class, path, asset name, and symbol that will be touched (use repository-wide search / `grep -rn` on a checkout). Read `docs/HANDOFF.md` and `docs/WORKLOG.md` first.
2. Research the public legacy repository `https://github.com/Andahmed/LegendsStyleRPG`. Use a shallow checkout with LFS smudging disabled:
   `GIT_LFS_SKIP_SMUDGE=1 git clone --depth 1 https://github.com/Andahmed/LegendsStyleRPG.git`
   The Unity project to inspect is `GameClient/`.
3. Do not write an implementation from scratch if a suitable implementation or source asset exists in either repository. Transfer and adapt it, then identify the exact source path in the report.
4. Inspect the legacy repository's `Art/CREDITS.md` before publishing transferred assets. Preserve required attribution and license notices.
5. Correct the stale statement in `docs/HANDOFF.md` that describes the legacy repository as private or requires selecting a local legacy checkout. The legacy repository is public; do not claim it is private. Update importer guidance accordingly when that file is in scope.

The following legacy findings are reported and must be independently confirmed against the current legacy repository checkout before relying on them:
- Six humanoid FBX models reportedly exist at `GameClient/Assets/Resources/GameArt/Free3D/Quaternius_RPGCharacters/FBX/`: Warrior, Cleric, Monk, Ranger, Rogue, Wizard. They are reported as real binary FBX files (about 2.1–2.8 MB), not LFS pointers.
- Four monster models reportedly exist there with matching file sizes to the copies in LegendsReborn.
- The legacy project reportedly contains `QuaterniusClipDriver`, `QuaterniusFbxImporter`, `NormalizeAndGround`, and `TintModel` implementations.
- The legacy project reportedly has 48 dungeon pieces and 150 nature models. Do not transfer these extra assets unless explicitly required by the current task.
- The legacy project's Unity/build state is unverified and must not be represented as a successful build.

## 3. Transfer map

| Requirement | Legacy source | LegendsReborn destination |
|---|---|---|
| Six hero FBX models | `GameClient/Assets/Resources/GameArt/Free3D/Quaternius_RPGCharacters/FBX/*.fbx` | `Assets/Resources/HomeHeroes/` |
| Clip playback | `QuaterniusClipDriver` | Existing `HeroView.cs` or one justified Presentation file, namespace `SandsOfLegends.Presentation` |
| Import settings | `QuaterniusFbxImporter` | `Assets/Editor/`, adapting its filter to `Assets/Resources/` |
| Grounding and normalization | `NormalizeAndGround` | `HeroView.Initialize` |
| Hero tint/variation | `TintModel` | `HeroView`, differentiated by each `HeroId` |

Transfer rules:
- Copy FBX files byte-for-byte using `cp` or git. Do not regenerate, convert, or re-export them.
- For every transferred FBX, verify its size is greater than 2048 bytes and its first bytes identify `Kaydara FBX Binary`. Reject LFS pointer text or invalid files.
- Add a `SOURCE.txt` in each relevant asset folder identifying the Quaternius CC0 source. Include/retain required credits from `Art/CREDITS.md`.
- Do not transfer the legacy 62-hero system, campaign, tower, guild, or their data models. Only permitted 3D assets and presentation logic may be transferred.
- Inspect actual animation clip names for **all six hero FBXs and all monster FBXs** (for example with `strings`) before hard-coding clip names.
- Fix the two known pitfalls in the legacy clip driver:
  - Do not select the first generic match for `Attack`: it can select `Idle_Attacking` before `Sword_Attack`. Select by the intended full/specific clip name, such as `Sword_Attack`.
  - Idle, walk, and flying clips loop. Attack and hit play once and return to idle. Death plays once and holds on its final pose.
- Report the exact legacy source paths used for each transferred implementation or asset.

## 4. Execution phases — do not advance until the previous phase passes

### Phase 0 — A real build baseline

- Add the six real hero FBXs to `Assets/Resources/HomeHeroes/`: Warrior, Cleric, Monk, Ranger, Rogue, Wizard.
- Open the project in Unity 2022.3.62f3 and fix all compile errors.
- Run the 16 existing EditMode tests; all must pass.
- Run **Sands → Setup MVP Home Scene** and commit `Assets/Scenes/Home.unity`.
- Commit Android settings (package name, minimum SDK, IL2CPP, ARM64) in `ProjectSettings`.
- Commit all generated `.meta` files for added assets.
- Configure GitHub Actions secrets: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`.
- Acceptance: the `Android APK` workflow succeeds, produces `SandsOfLegends.apk`, and the APK launches on a real device.

### Phase 1 — Characters render and animate correctly

In `HeroView.cs` and only the minimum justified supporting presentation/import files:

- Load animation clips embedded in FBXs with `Resources.LoadAll<AnimationClip>` and Playables API. FBXs do not include an Animator Controller, so an Animator trigger alone is insufficient.
- Idle loops. Attack and hit play once and return to idle. Death plays once and holds its final pose. Walk/flying loops where applicable.
- Select clips using verified actual names. Avoid generic substring selection that can pick `Idle_Attacking` instead of `Sword_Attack`.
- Ground models using renderer bounds (`bounds.min.y`) as `HomeWorldStage` does. Preserve the model's base/local rotation; do not reset `localRotation` to identity every frame.
- Distinguish heroes sharing a model using tint, scale, or accessories per `HeroId`; five heroes sharing a model must not look identical.
- Provide portraits for all 20 heroes, or an explicitly drawn placeholder portrait. Never silently skip a missing portrait.
- Remove the capsule fallback from the production path once real models are installed.
- Acceptance: a real-device video or screenshot shows a complete battle with genuine model animation. State explicitly whether this was tested in Unity and on Android.

### Phase 2 — Core loop, one item at a time, in this order

1. **Summon:** rarity-specific reveal screen animation/effects and clearly visible pity.
2. **Enemy variety:** different authored enemy teams per biome; bosses have their own model and skills.
3. **Skill and ultimate VFX:** visual Particle System effects driven by `BattleEvent`.
4. **Faction effects:** homogeneous-team bonuses or faction counters implemented inside `BattleSim` and covered by tests. Keep `Combat` independent of Unity.
5. **Idle rewards and daily quests:** return notification and clear claim button.
6. RTL and Arabic localization via `Localizer`, if required.

Each numbered item is a separate task. Do not bundle unrelated items.

### Phase 3 — Server integration

- Connect client Summon to FastAPI `/auth/guest` and `/summon`; remove local Summon RNG from production flow.
- Arena and Weekly Boss remain locked until actually implemented. Never show a screen that falsely implies they work.

## 5. Definition of done for every task

- Code compiles in Unity.
- An EditMode or PlayMode test covers the change.
- The change is tested on Android, or explicitly marked as not tested.
- One clear commit per task, ending with the approved `Co-Authored-By` convention.
- Update `docs/WORKLOG.md` with two concise lines: what changed and what remains unverified.
- Do not merge to `main` without explicit approval.

## 6. Required report format

1. **Changed:** exact file paths.
2. **Actually run and passed:** exact command/action and result.
3. **Not tested:** Unity, PlayMode, Android/device, CI, or any other relevant checks not run.
4. **Out-of-scope findings:** list issues discovered but not changed.
5. **Source provenance:** exact legacy repository paths for every transferred asset or implementation.
6. **Commit/PR:** exact commit SHA and PR URL; state clearly whether it is merged.

If Unity was unavailable, include the exact sentence: `لم يتم التحقق في Unity`.

## 7. Current project facts to re-check, not assume

- The current `docs/HANDOFF.md` reports 21 Quaternius FBX assets in `Assets/Resources`: 11 dungeon pieces, four monster types, and six nature props. It also says six humanoid FBXs are still missing and describes a local legacy-folder importer. Reconcile these statements against the actual repository after transferring assets.
- The five approved portrait PNGs are not substitutes for the six rigged FBX models. Confirm current asset presence and `.meta` files before copying anything.
- The Playables implementation in any open PR is not proof of runtime animation. Verify it against real imported clips and Unity Play Mode before claiming success.
- Do not assume any previous PR is merged. Inspect its current state and diff before continuing.
