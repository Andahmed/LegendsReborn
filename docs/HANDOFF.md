# HANDOFF - where the project stands (2026-11-11)

Working title: **Sands of Legends** (repo: LegendsReborn). Idle RPG for Android (Unity 2022.3.62f3).
Plan source: "Idle RPG project plan" PDF (GDD summary below). Owner: Ahmed.

## Done
- Repo scaffold: README, .gitignore, .gitattributes, folders, `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`.
- `Assets/Scripts/Combat` (engine-free, asmdef `noEngineReferences`):
  - `Types.cs`: Faction/Role/Rarity/TargetType, `FactionChart` (+25% advantage), `SkillSpec`, `UnitSpec`, `BattleEvent`, `SeededRng`.
  - `BattleSim.cs`: deterministic fixed-step (Dt 0.1s) auto-battle. 2 front + 3 back slots, front row targeted first,
    timed skill, energy ultimate (auto or manual via `RequestUltimate`), crit via seeded RNG, 120s timeout = defender wins.
- `Assets/Scripts/Core`: `HeroDefinition` / `SkillDefinition` ScriptableObjects -> `ToSpec()`.
- `Assets/Tests/EditMode/BattleSimTests.cs`: 6 NUnit tests.
- Combat logic was compiled and checked outside Unity with mono (all checks passed).

## NOT verified yet
- Project has never been opened in Unity. `HeroDefinition.cs`, tests and asmdefs are untested in the editor.
- No `.meta` files committed; Unity will generate them on first open (commit them after).
- No Unity Cloud Build / CI workflow yet (needs UNITY_LICENSE/EMAIL/PASSWORD secrets).
- Balance numbers (damage formula `ATK*power*100/(100+DEF)`, energy gain 20/10) are placeholders.

## Next steps (suggested order, per GDD weeks 1-4)
1. Open in Unity 2022.3.62f3, fix any compile errors, run EditMode tests, commit `.meta` files.
2. Create 4 hero assets (one per faction: Fortress/Wild/Forest/Shadow) + skills under `Assets/Resources/Data`.
3. Minimal battle scene: read `BattleSim.Events`, show units, HP bars, speed button x1/x2/x4.
4. Campaign stage data (ScriptableObject) + auto-progress; save system (one save system only).
5. Idle rewards (12h cap), then gacha/summon with pity (30 = Epic, 90 = Legendary; rates 40/35/20/5 are test values).
6. Android build pipeline (Cloud Build or GitHub Actions), APK size budget.

## Design rules (from GDD - keep!)
- Combat logic stays engine-free; view layer only reads events. No hero stats hardcoded - ScriptableObjects only.
- Factions: Fortress > Wild > Forest > Shadow > Fortress; Divinity <-> Abyss (rare). Advantage = +25% damage.
- Roles: Tank / Support / DPS. Speeds x1/x2/x4. Battle target 60-90s at x1.
- **Legal:** do NOT copy names, heroes, art, audio or text from Legends Reborn: Last Battle / Magnum Quest. Mechanics only.
- Lessons from the old repo (LegendsStyleRPG): one save system, one currency set, one UI system, keep APK small
  (don't put unused art packs in `Resources/`), keep CI Unity version equal to ProjectVersion.txt.

## Old repo (separate)
`Andahmed/LegendsStyleRPG` is the previous attempt. Local fixes (mirrored labels, camera focus, gravity, SafeArea) were
prepared but intentionally NOT pushed by request; ChatGPT pushed similar changes to its own branches.
