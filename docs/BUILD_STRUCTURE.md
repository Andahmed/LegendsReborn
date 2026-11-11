# Game Build Structure — Owner-Approved Baseline
Date: 2026-11-11

This document records the project owner's five-layer build structure. It is the working delivery baseline; it does not claim that the complete source PDF has been extracted.

## 1. Core Loop
- Campaign: 200 stages = 5 environments × 40; automatic 5v5 combat.
- Battle target: 60–90 sec at x1; 15–30 sec at x4. Ultimate is optionally manual.
- Idle farming accumulates offline for at most 12 hours; daily collection and reinvestment.
- Formation: 2 front + 3 back. Faction advantage: 25%. Enemy archetypes must force roster/formation variation.
- MVP: 20 heroes; four roles: Tank / Damage / Support / Control; four rarities: Common / Rare / Epic / Legendary.
- Gacha test rates 40/35/20/5; Epic pity every 30, Legendary every 90; duplicates become shards for five star levels.
- Progression: shared Spirit Source ceiling, six gear slots, three-branch talent tree.
- Hero and skill content must be data-driven (JSON/ScriptableObject), not hard-coded.

## 2. Modes and Delivery Order
**MVP:** Campaign, idle rewards, asynchronous PvP arena, weekly boss raid, daily quests.
**Post-launch:** Mazes/dungeons, endless tower, alliances + alliance war, seasonal events.

## 3. Economy, World and Art
**Five resources:** Gems, Gold, Experience, Hero Shards, Equipment Materials. Each requires its own documented sources and sinks.

**Five revenue channels:** Summon banners, monthly card, Battle Pass, voluntary/rewarded ads, tiered offers.

**World:** The Seven Sands + the Gray Flood.
**Factions:** City Guardians, Plainsfolk, Oasisfolk, Shadowborn; rare factions Light and Abyss.

**Art identity:** Stylized 3D; turquoise × gold × sand/earth; Arabic geometric ornament; distinct voice/audio identity for each core hero. Art is a bottleneck and should receive about one-third of production capacity. Use original or properly licensed assets only.

## 4. Technical Architecture
- Unity LTS client, separated into Presentation / Logic / Data.
- Hero and skill definitions are JSON and/or ScriptableObjects.
- Combat and Meta remain engine-free; Unity adapters belong in Core/Services/Presentation.
- Server environments: dev / staging / production.
- Summon RNG and results are server-authoritative, with auditable records. Never trust client-generated results.
- Async PvP results/rankings are server-verified; add authentication, validation, idempotency, anti-replay and rate limits.
- Analytics and crash reporting from the first playable build.
- Production workflow: Jira, Git LFS, Notion. Provider/credentials must be configured and tested before claiming integration.

## 5. Team and Nine-Month Target
**12 people:** Management 1 | Design 2 | Client 3 | Server 2 | Art 3 | QA 1.

| Phase | Target | Exit criteria |
|---|---|---|
| Pre-production | M0–1 | GDD, architecture, art direction, economy assumptions, risk register and vertical-slice scope approved |
| Vertical slice | M1–3 | One playable end-to-end loop on target Android: heroes, 5v5, rewards, save/load, integrated art/VFX/audio, analytics/crash reporting |
| Alpha | M3–5 | MVP systems integrated, content pipeline working, server summon audit and basic async arena, internal QA pass |
| Soft launch | M5–7 | Production-like backend, economy/retention instrumentation, device/performance/crash targets, live-ops rehearsal |
| Release preparation | M7–8 | Store compliance, localization, security/load testing, final content, rollback/support plan and release candidate sign-off |
| Global launch + LiveOps | M8–9 | Release approval, monitoring/on-call, event calendar, incident response and first LiveOps cycle |

## Phase-gate rule
No phase starts until the previous phase's exit criteria are met and evidenced by a build/artifact, test report, reviewed content and named owner approval. Calendar dates alone do not pass a gate. Art is critical path and must not be deferred until the end.

## Current repository assessment (2026-11-11)
**Foundation present:** deterministic combat core; Meta foundations for roster/gacha/idle/campaign/quests/save; equipment foundation in PR #7; five-slot formation and Core battle-data bridge in PR #8.

**Still incomplete/unverified:** Heroes UI and Campaign launch wiring; 20 authored heroes and 200 stage records; 3-branch talents; enemy archetypes; complete environment/character/voice assets; economy ledger; weekly boss and async PvP; server dev/staging/prod and audited summon RNG; analytics/crash provider; Unity compilation, full EditMode run and Android vertical-slice build.

## Immediate critical path
1. Complete the vertical slice: Heroes formation UI → Campaign launch → BattleSim → rewards/results → save/load.
2. Prove the art pipeline with four representative heroes and one complete environment on target Android.
3. Author and balance data-driven hero/skill/stage content; complete economy source/sink ledger.
4. Implement server-authoritative summons with audit records and async PvP.
5. Add weekly boss, QA gates and release operations.

The nine-month schedule is a target, not a statement that the current project is on schedule. The full PDF must still be reconciled page-by-page before claiming complete plan coverage.
