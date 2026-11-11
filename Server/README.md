# Sands of Legends API

Python 3.10+ / FastAPI. Start from this directory:

\`\`\`bash
pip install -r requirements.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000
\`\`\`

The default database is Server/data/sands.sqlite3. Set SANDS_DB_PATH to use a different persistent location.

## Implemented endpoints

| Endpoint | Purpose |
|---|---|
| GET /health | Service health |
| POST /auth/guest | Creates a guest account and one-time bearer token |
| GET /profile | Reads the authenticated profile |
| POST /summon | Server RNG, gem debit, pity, duplicate shards and audit log |
| GET /summon/history | Last 1-100 audited pulls |
| GET /arena/status | Explicitly reports the PvP feature as locked |
| GET /boss/status | Explicitly reports the boss feature as locked |

## Security and release gates

- Summon results are generated with Python secrets, not client-provided randomness.
- `POST /summon` accepts only `count` (1 or 10); extra fields such as a client seed are rejected.
- The summon response includes `profile_state` (wallet and a list of hero DTOs) plus `pity`; this is the Unity client's JsonUtility-safe authoritative snapshot.
- Bearer tokens are stored as SHA-256 hashes, not in plaintext.
- Gem debit, pity, roster changes and audit inserts run in one SQLite write transaction.
- This is a development backend. Before public release, add TLS termination, account recovery/provider linking, abuse controls, operational monitoring, database backups and server deployment.
- Unity's Live Summon path calls `POST /auth/guest` and `POST /summon` via a configurable `serverBaseUrl`; the bearer token is memory-only in this batch. `SummonScreen.TryPull` remains preview/test-only.
- No Unity or Android verification is claimed for this integration.
- Arena and boss attacks are intentionally unavailable until the server can re-simulate and verify battle events.
