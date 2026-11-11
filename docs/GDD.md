# Game Design Document — Sands of Legends

Working summary aligned with the owner-approved `docs/BUILD_STRUCTURE.md`. The full source PDF has not yet been extracted page-by-page.

## Core loop
- Campaign: 200 stages, 5 environments × 40; automatic 5v5 battles.
- Target duration: 60–90 seconds at x1; 15–30 seconds at x4. Ultimate may be manual or automatic.
- Offline farming capped at 12 hours; collect daily and reinvest in progression.
- Existing energy working rule: cap 120, +1 per 6 minutes, used for farming/events and not normal campaign. Reconfirm against the full source PDF.

## Heroes and progression
- 20 MVP heroes; four roles: Tank / Damage / Support / Control.
- Four rarities: Common / Rare / Epic / Legendary.
- Test gacha rates 40/35/20/5; Epic pity 30, Legendary pity 90; duplicates convert to shards; five star levels.
- Shared Spirit Source level ceiling; six equipment slots; three-branch talent tree.
- Hero and skill data authored in JSON and/or ScriptableObjects.

## Combat
- Five units per side; 2 front + 3 back.
- Faction advantage 25%; enemy archetypes must encourage varied formations.
- Deterministic engine-free combat; x1/x2/x4; optional manual Ultimate.

## World and factions
- Regions: The Seven Sands and the Gray Flood.
- Core factions: City Guardians, Plainsfolk, Oasisfolk, Shadowborn; rare: Light and Abyss.
- All names, lore, art, audio and UI are original or properly licensed.

## Modes
**MVP:** Campaign, idle rewards, asynchronous PvP arena, weekly boss, daily quests.
**Post-launch:** Mazes, endless tower, alliances and alliance war, seasonal events.

## Economy and monetization
Five resources: Gems, Gold, Experience, Hero Shards, Equipment Materials.
Five revenue channels: banners, monthly card, Battle Pass, voluntary/rewarded ads, tiered offers.
Define all sources, sinks and values in an approved ledger; do not invent prices or tune rates without evidence. Preserve a viable F2P path.

## Art and technical requirements
Stylized 3D; turquoise/gold/sand palette; Arabic geometric ornament; distinct voice/audio identity per core hero. Art/animation/VFX/audio receive approximately one-third of production capacity.
Unity LTS with Presentation / Logic / Data separation. Server dev/staging/prod; server-authoritative summon RNG with audit records; server-verified async PvP and rate limits. Analytics and crash reporting from first playable build. Jira, Git LFS and Notion.

## Delivery
Nine-month target: Pre-production M0–1; Vertical Slice M1–3; Alpha M3–5; Soft Launch M5–7; Release Prep M7–8; Global Launch + LiveOps M8–9. Each phase requires evidence-based exit approval. See `docs/BUILD_STRUCTURE.md`.
