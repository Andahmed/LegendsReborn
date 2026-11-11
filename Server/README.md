# Server

Run: `pip install -r requirements.txt && uvicorn app.main:app --reload`

## Authority rules
- Server owns RNG for summons and keeps an audit log of every pull (user, time, rates version, result).
- Client never sends results, only intents; server validates costs and returns the new state.
- Battles for arena/boss are re-simulated on the server with `BattleSim` (deterministic, seeded).

## Planned endpoints
| Endpoint | Purpose | Status |
|---|---|---|
| GET /health | liveness | done |
| POST /auth/guest | guest account + token | todo |
| GET/PUT /profile | cloud save | todo |
| POST /summon | server-side gacha + audit log | todo |
| POST /arena/attack | verify replay, update rank | todo |
| POST /boss/attack | weekly boss damage | todo |

## Environments
dev / staging / prod (separate DBs). Daily backups + monthly restore test (GDD risk table).
