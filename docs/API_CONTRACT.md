# API Contract — Sands of Legends Server

Version: 0.2.0  
Base URL: the deployed server origin (development default: `http://localhost:8000`)  
Authentication: `Authorization: Bearer <access_token>` for authenticated routes.

This contract describes the endpoints that exist in `Server/app/main.py`. It does not imply that the Unity client is connected to them.

## Endpoints

### GET /health

No authentication.

Response:

```json
{
  "status": "ok",
  "service": "sands-of-legends",
  "time": 1730000000
}
```

`time` is the server's Unix timestamp.

### POST /auth/guest

Creates a guest account and a bearer token. No request body is required.

Response:

```json
{
  "account_id": "hex-account-id",
  "access_token": "opaque-one-time-token",
  "token_type": "bearer",
  "profile": {
    "version": 1,
    "gems": 3000,
    "gold": 15000,
    "exp": 5000,
    "gear_materials": 100,
    "heroes": {
      "warrior": { "level": 1, "stars": 0, "shards": 0 }
    },
    "formation": ["warrior", "paladin", "berserker", "healer", "sharpshooter"],
    "pity_epic": 0,
    "pity_legendary": 0,
    "highest_stage": 0
  }
}
```

The `heroes` object contains all five starter records; the example shows one entry for brevity.

### GET /profile

Requires a bearer token.

Response:

```json
{
  "account_id": "hex-account-id",
  "profile": {
    "version": 1,
    "gems": 3000,
    "gold": 15000,
    "exp": 5000,
    "gear_materials": 100,
    "heroes": {},
    "formation": [],
    "pity_epic": 0,
    "pity_legendary": 0,
    "highest_stage": 0
  }
}
```

The profile is a JSON snapshot. Its presence does not mean every field has a server-side mutation endpoint.

### POST /summon

Requires a bearer token.

Request:

```json
{ "count": 1 }
```

Only `count=1` and `count=10` are accepted. Single summon costs 300 Gems; ten summons cost 2700 Gems.

Response:

```json
{
  "results": [
    {
      "hero_id": "hero-id",
      "rarity": "COMMON",
      "is_new": true,
      "shards_gained": 0
    }
  ],
  "profile": {},
  "rates_version": 1,
  "rates": {
    "COMMON": 40,
    "RARE": 35,
    "EPIC": 20,
    "LEGENDARY": 5
  },
  "pity": {
    "epic": 0,
    "legendary": 0
  }
}
```

The `results` array contains one entry per pull. The `profile` field is the updated profile snapshot. A duplicate converts to shards: Common 5, Rare 10, Epic 20, Legendary 40. Pity guarantees Epic-or-better by the 30th pull and Legendary by the 90th pull. RNG, gem debit, hero acquisition, shard credit, pity update and audit insert are server-side.

### GET /summon/history?limit=20

Requires a bearer token. `limit` defaults to 20 and is clamped to 1..100.

Response:

```json
{
  "items": [
    {
      "created_at": 1730000000,
      "rarity": "COMMON",
      "hero_id": "hero-id",
      "is_new": 1,
      "shards_gained": 0
    }
  ]
}
```

`is_new` is returned as an integer from SQLite (0 or 1).

### GET /arena/status

No authentication.

Response while unavailable:

```json
{
  "status": "locked",
  "reason": "Arena requires server-side opponent snapshots and verified BattleSim replay integration."
}
```

### GET /boss/status

No authentication.

Response while unavailable:

```json
{
  "status": "locked",
  "reason": "Weekly boss requires server-side damage validation, attempts and reward ranking."
}
```

## Error responses

FastAPI returns its standard `{"detail":"..."}` error shape.

| HTTP | Condition |
|---|---|
| 401 | Missing, malformed, or invalid bearer token on authenticated routes |
| 400 | `/summon` count is not 1 or 10 |
| 409 | Insufficient Gems for the requested summon |
| 404 | Authenticated account has no stored profile |
| 500 | Stored profile JSON is invalid |

## Data ownership today

| Data | Current authority | Notes |
|---|---|---|
| Account ID and bearer token hash | Server | Token is returned once; only its SHA-256 hash is stored |
| Gems spent on summons | Server | Summon checks and debits the server balance transactionally |
| Summon RNG, rarity rates, pity | Server | Client must not provide a seed or result |
| Hero acquisition from summons | Server | Server inserts newly acquired hero records |
| Duplicate hero shards | Server | Awarded and recorded in summon audit |
| Summon history | Server | Append-only audit records |
| Gold, EXP, Gear Materials | Client-local for now | Initial values are present in the server's profile snapshot, but there are no update endpoints |
| Energy and regeneration | Client-local for now | No server profile mutation/sync endpoint exists |
| Campaign progress and stage rewards | Client-local for now | No verified server battle result endpoint exists |
| Formation | Client-local for now | No formation write endpoint exists |
| Hero level, stars and ascension | Client-local for now | The server stores basic hero records but does not expose progression mutation endpoints |
| Equipment and inventory progression | Client-local for now | No server equipment/inventory API exists |
| Daily/weekly quest progress and rewards | Client-local for now | No quest endpoint or reward reconciliation exists |

### Known Gems ownership conflict — deferred

The local `DailyQuests.TryClaim` implementation credits quest Gems directly to the client Wallet. The server does not know about that reward and treats its own Gems balance as authoritative for summons. Therefore, locally earned quest Gems can diverge from the server balance and may be rejected or overwritten when the client next consumes a server summon/profile response.

**No synchronization endpoint or workaround is implemented in this PR.** The ownership and reward-credit flow requires a product decision before adding a sync contract.

## Compatibility guarantees

- Existing response shapes for `/auth/guest`, `/profile`, `/summon`, and `/summon/history` remain unchanged.
- This document and the tests do not connect or modify the Unity client.
- Arena and weekly boss remain explicitly locked.
