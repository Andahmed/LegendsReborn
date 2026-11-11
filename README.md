# Legends Reborn

Unity mobile Idle RPG (Android), working title **Sands of Legends**. Clean rebuild.

- Unity: **2022.3.62f3** (matches Unity Cloud Build)
- Target: Android (IL2CPP, ARM64)

## Structure
```
Assets/
  Scripts/  Core | Characters | Combat | World | UI | Editor
  Resources/  Scenes/  Art/
Packages/manifest.json
ProjectSettings/ProjectVersion.txt
```

## Rules
- One save system, one currency set, one UI system (no parallel legacy copies).
- Build must pass locally before pushing to `main`.
- Large art packs go outside `Resources/` unless used at runtime.

## Architecture (GDD 7.1)
- `Scripts/Combat` - engine-free C# (asmdef `noEngineReferences`): `BattleSim` is a deterministic fixed-step auto-battle
  (seeded RNG, 2 front + 3 back slots, faction cycle +25%, timed skills, energy ultimates). View layer only reads
  `BattleSim.Events` and speeds up by calling `Step()` more often (x1/x2/x4).
- `Scripts/Core` - ScriptableObjects (`HeroDefinition`, `SkillDefinition`) -> `UnitSpec`/`SkillSpec`. No hero stats in code.
- `Tests/EditMode` - NUnit tests for combat.

## Factions
Fortress > Wild > Forest > Shadow > Fortress. Divinity <-> Abyss (rare, mutual).
