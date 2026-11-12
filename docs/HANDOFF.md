# HANDOFF - implementation and verification status (2026-11-11)

Working title: **Sands of Legends**. Unity 2022.3.62f3, Android IL2CPP/ARM64 target.

## Implemented in the current MVP foundation branch

### Gameplay and data
- 20 hero records in HeroCatalog with faction, role, rarity, combat stats, skill/ultimate data and source-model mapping.
- Five starter heroes and a first-run profile with test currency, energy, gear and saved five-slot formation.
- Roster level-up and star-up operations; duplicate summons convert into rarity-based shards.
- 200 generated campaign stage definitions across five biomes, boss stages every ten stages and stage-scaled 5-unit enemy teams.
- Deterministic 5v5 combat core remains engine-free. Battle flow now consumes campaign energy, plays BattleSim events, supports x1/x2/x4 and manual or automatic ultimates, awards win rewards and updates campaign progress.
- Six equipment slots per hero; upgrades spend Gold and Gear Materials and gear levels add combat stats. The Equipment screen now lets the player select any owned hero before upgrading gear. Hero progression includes shard-based star-up and resource-gated ascension.
- Five approved hero portrait PNGs are bundled and shown in the roster; these are concept/portrait art, not the missing rigged 3D hero models.
- Save schema version 2 persists formation and equipment in addition to resources, roster, campaign, pity, energy and quests.

### Runtime UI and art pipeline
- GameFlowController connects the Home HUD UnityEvents to Campaign, Heroes, Formation, Summon, Equipment, Backpack, Quests and feature-status screens. Long screens and the battle HUD now use a scrollable viewport.
- Heroes, Campaign and Summon screen classes now expose validated gameplay operations; BattleHud controls speed/ultimates.
- HeroView loads Quaternius FBX models when imported, with forward attack lunges, hit reactions and a defeat fall; combat feedback uses floating damage numbers. Summon results reuse the approved hero portraits.
- 27 Quaternius FBX files are bundled in Assets/Resources: 11 modular dungeon pieces, four animated monster types, six nature props, and six temporary humanoid home hero models.
- HomeWorldStage loads an original editor-generated low-poly castle Mesh prefab (bevelled stone blocks, arched gate, polygonal towers, crenellations, banners and courtyard) and places the committed Quaternius nature meshes around it. It does not instantiate Unity primitive cubes for the castle.
- QuaterniusAssetImporter can copy any missing character rigs from a locally selected Free3D folder. The six MVP home hero FBXs are now committed under `Assets/Resources/HomeHeroes/`; the source project is public at https://github.com/Andahmed/LegendsStyleRPG.
- MVPProjectInstaller generates Assets/Scenes/Home.unity, adds GameBootstrap + HomeScreen and registers the scene in Build Settings. BuildScript calls it before Android builds.

### Development backend
- FastAPI guest account creation with opaque bearer token; only SHA-256 token hashes are stored.
- SQLite profile storage and transactional server-side summons with cryptographic RNG, gem debit, pity, duplicate shards and per-pull audit rows.
- Health, profile, summon-history, arena-status and boss-status routes.
- Python unittest coverage and a GitHub Actions workflow for backend tests.

## Setup

1. Open with Unity 2022.3.62f3.
2. Run **Sands → Setup MVP Home Scene**. This generates the original castle Mesh assets and its Resources prefab, then creates/updates Home.unity.
3. The Quaternius castle, monster and nature FBXs are already inside Assets/Resources and will import automatically.
4. The six temporary humanoid home hero FBXs are already committed under `Assets/Resources/HomeHeroes/`. For any additional legacy art, use the public `Andahmed/LegendsStyleRPG` checkout and the **Legends Reborn → Art → Install MVP Art** menu.
5. Wait for asset import, then open Home.unity and press Play.
6. From Server, install requirements, run uvicorn and python -m unittest discover -s tests -v.
7. Run **Sands → Validate MVP Assets** and inspect `Reports/MVP_ASSET_READINESS.md` for missing files and import status.

## Explicitly not complete or not verified

- No Unity Editor compile, Play Mode run, Android APK, mobile touch or aspect-ratio test has been performed in this environment. The GameCI Android APK workflow is configured, but it skips the build until the three Unity Actions secrets are present.
- Unity will generate .meta files when it first imports the new scripts/FBX assets; they must be committed after editor import.
- The original home castle Mesh generator is committed, but its generated prefab is materialized only when the user runs the Editor setup menu. Castle scale, Mesh normals, materials, silhouette and camera framing still require visual review inside Unity.
- The five approved hero archetypes map to the available Quaternius rigged character FBX models as temporary MVP models. Final bespoke hero models, animations and skill VFX are still outstanding.
- Arabic font shaping/RTL presentation is not complete; the current runtime labels are English.
- The Unity client summon screen still uses local preview RNG. It is not yet connected to the FastAPI guest/profile/summon endpoints.
- Arena, world boss, alliance, shop billing, Battle Pass and live events remain feature-status screens or locked endpoints, not playable systems.
- Individual stages are generated by a scaling formula; enemy lineup variety, encounter scripting, final reward/economy balance and all five biome art sets are not authored.
- No server deployment, TLS, account recovery, anti-abuse controls or production database operations are included.

## Design rules

- Do not copy names, heroes, art, audio or text from Legends Reborn: Last Battle / Magnum Quest. Mechanics only.
- Keep combat simulation independent from UnityEngine. UI and presentation consume battle events.
- Keep one save system and one currency set. Server owns production summon RNG and currency validation.
- Do not represent a local preview as a server-authoritative result or an untested build as ready for players.
