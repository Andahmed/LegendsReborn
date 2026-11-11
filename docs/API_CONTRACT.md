# API Contract

This document is the API contract decision register for the MVP. Endpoint payloads and server authority are specified alongside the implementation; unresolved product choices are listed below and must not be implemented until the owner decides.

## Open Decisions

These are documentation-only decisions. No behavior is changed by recording them.

### 1. New-hero starting level can exceed the base level cap

**Current behavior:** `Roster.AddHeroOrShards` assigns a newly acquired hero `max(1, SpiritCeiling - 5)`. The base cap is `Progression.LevelCap(0) = 20`; a sufficiently high shared Spirit Ceiling can therefore produce a new hero above level 20.

**Options:**
- **A — Clamp to base cap:** assign `min(20, max(1, SpiritCeiling - 5))`.
- **B — Preserve shared-ceiling rule:** allow the starting level to exceed the base cap and treat the cap as progression-gated only for subsequent level-ups.
- **C — Raise the base cap rule:** redefine the intended starting-level/cap relationship in the progression design.

**Recommendation:** Option A, unless the design explicitly intends newly acquired heroes to bypass their current ascension cap.

### 2. Campaign entry costs 5 Energy although the GDD says it is free

**Current behavior:** `CampaignContent.Get` returns `EnergyCost = 5`, while `docs/GDD.md` says normal campaign does not consume Energy; Energy is reserved for farming/events.

**Options:**
- **A — Follow the GDD:** normal campaign costs 0 Energy; reserve Energy for farming/events.
- **B — Keep the implementation:** campaign entry costs 5 Energy and update the GDD after approval.
- **C — Split costs by mode:** normal campaign is free, while replay/farming variants may cost Energy.

**Recommendation:** Option A to preserve the explicit GDD rule and avoid blocking the core campaign loop.

### 3. Daily quest Gems are credited locally, but the server is unaware

**Current behavior:** `DailyQuests.TryClaim` adds the 20-Gem reward directly to the local `Wallet`; server-side Gems authority does not currently receive that claim.

**Options:**
- **A — Server-authoritative claim:** send a claim request; the server validates eligibility and credits Gems.
- **B — Keep local Gems temporarily:** allow local claims during the prototype and explicitly treat them as non-production state.
- **C — Change quest rewards:** use a local-only reward until a server claim endpoint is approved.

**Recommendation:** Option A before production; retain the current local implementation until the endpoint and migration are explicitly approved.

### 4. SummonScreen still uses local summon logic rather than the server

**Current behavior:** `SummonScreen` has not been connected to the server summon flow; local `GachaSystem` remains available, while the server owns the MVP summon RNG, pity, cost, duplicate-shard conversion, and history contract.

**Options:**
- **A — Connect the screen to server summon endpoints:** the client submits count and displays the authoritative response/history.
- **B — Keep local summons for offline prototype only:** clearly gate this path from production accounts.
- **C — Disable summon UI until server integration is ready.**

**Recommendation:** Option A for production; do not change the screen or API behavior until the integration task is approved.

---

**Decision rule:** These four items are open product decisions. This PR records the current behavior, options, and recommendations only; it does not implement any option.
