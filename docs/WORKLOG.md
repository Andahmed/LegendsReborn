# Work Log (newest first)

## 2026-11-11 — Owner-approved five-layer build baseline
**Done**
- Added `docs/BUILD_STRUCTURE.md` recording the supplied core loop, modes, economy, world/factions, art, technical architecture, team and nine-month gated schedule.
- Updated GDD summary for four roles (Tank/Damage/Support/Control), 5 resources, 5 monetization channels, Seven Sands/Gray Flood, faction list, talent trees and server requirements.
- Expanded Economy and Style Guide with missing ledgers, art capacity and acceptance gates.
- Changes are on `feature/mvp-five-slot-formation`; no merge or main-branch push.

**Not verified**
- Documentation alignment only; missing systems are not thereby implemented.
- Full source PDF has not been extracted page-by-page.
- Unity, Android, server, analytics/crash reporting and art acceptance gates remain unverified.

**Next critical path**
1. Heroes formation UI → Campaign launch → BattleSim → rewards/results → save/load.
2. Prove vertical slice with four heroes and one environment on target Android.
3. Author data-driven hero/skill/stage content and economy ledger.
4. Implement server-authoritative summon audit, async PvP and weekly boss.
5. Pass evidence-based Alpha/Soft Launch/Release gates.

## 2026-11-11 — Five-slot team formation and battle data bridge
- Added engine-free five-slot formation rules, ownership/duplicate validation, move/swap/removal, persistence and version 2→3 save migration.
- Added Core bridge from saved formation to ordered five-slot UnitSpec[].
- Added regression tests. Unity compilation/EditMode execution not run; Heroes UI and Campaign launch wiring remain incomplete.

## 2026-11-11 — MVP equipment foundation
- PR #7 contains engine-free six-slot equipment foundation and save migration. Unity tests, item content, set bonuses, economy, drops and UI remain incomplete.

## 2026-11-11 — Foundations pass
- Added Combat, Meta, Services, Bootstrap, Presentation, UI framework, localization, editor build script, server skeleton and docs.
- Previous report records 16 engine-free tests passing under Mono; this is not Unity test execution.
- Unity compile, scenes, .meta files and Android build remain unverified.
