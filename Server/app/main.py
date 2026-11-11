import hashlib
import json
import os
import secrets
import sqlite3
import time
import uuid
from pathlib import Path
from typing import Dict, List, Optional

from fastapi import FastAPI, Header, HTTPException
from pydantic import BaseModel, Field

app = FastAPI(title="Sands of Legends Server", version="0.2.0")

DB_PATH = Path(os.environ.get("SANDS_DB_PATH", Path(__file__).resolve().parent.parent / "data" / "sands.sqlite3"))
STARTERS = ["warrior", "paladin", "berserker", "healer", "sharpshooter"]
HERO_POOLS = {
    "COMMON": ["warrior", "healer", "militia", "tracker", "acolyte", "scout", "guardian"],
    "RARE": ["paladin", "sharpshooter", "monk", "shieldbearer", "stormcaller", "sandstalker", "oracle"],
    "EPIC": ["berserker", "sunwarden", "dragonknight"],
    "LEGENDARY": ["phoenix", "voidreaper", "titan"],
}
RARITY_RATES = {"COMMON": 40, "RARE": 35, "EPIC": 20, "LEGENDARY": 5}
RARITY_WEIGHTS = [("COMMON", 4000), ("RARE", 3500), ("EPIC", 2000), ("LEGENDARY", 500)]
EPIC_PITY = 30
LEGENDARY_PITY = 90
EPIC_PITY_GUARANTEE_CUTOFF = 9500
SUMMON_COST_SINGLE = 300
SUMMON_COST_TEN = 2700
DUPLICATE_SHARDS = {"COMMON": 5, "RARE": 10, "EPIC": 20, "LEGENDARY": 40}


def connect() -> sqlite3.Connection:
    DB_PATH.parent.mkdir(parents=True, exist_ok=True)
    connection = sqlite3.connect(str(DB_PATH), timeout=15)
    connection.row_factory = sqlite3.Row
    connection.execute("PRAGMA foreign_keys = ON")
    return connection


def initialize_database() -> None:
    with connect() as db:
        db.executescript(
            """
            CREATE TABLE IF NOT EXISTS accounts (
                account_id TEXT PRIMARY KEY,
                token_hash TEXT NOT NULL UNIQUE,
                created_at INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS profiles (
                account_id TEXT PRIMARY KEY REFERENCES accounts(account_id) ON DELETE CASCADE,
                payload TEXT NOT NULL,
                updated_at INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS summon_audit (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                account_id TEXT NOT NULL REFERENCES accounts(account_id),
                created_at INTEGER NOT NULL,
                rates_version INTEGER NOT NULL,
                rarity TEXT NOT NULL,
                hero_id TEXT NOT NULL,
                is_new INTEGER NOT NULL,
                shards_gained INTEGER NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_summon_audit_account_time
                ON summon_audit(account_id, created_at);
            """
        )


initialize_database()


def default_profile() -> Dict:
    return {
        "version": 1,
        "gems": 3000,
        "gold": 15000,
        "exp": 5000,
        "gear_materials": 100,
        "heroes": {hero_id: {"level": 1, "stars": 0, "shards": 0} for hero_id in STARTERS},
        "formation": STARTERS[:],
        "pity_epic": 0,
        "pity_legendary": 0,
        "highest_stage": 0,
    }


def require_account(authorization: Optional[str]) -> str:
    if not authorization or not authorization.lower().startswith("bearer "):
        raise HTTPException(status_code=401, detail="Bearer access token required")
    token = authorization.split(" ", 1)[1].strip()
    if not token:
        raise HTTPException(status_code=401, detail="Bearer access token required")
    token_hash = hashlib.sha256(token.encode("utf-8")).hexdigest()
    with connect() as db:
        row = db.execute("SELECT account_id FROM accounts WHERE token_hash = ?", (token_hash,)).fetchone()
    if row is None:
        raise HTTPException(status_code=401, detail="Invalid or expired access token")
    return str(row["account_id"])


def read_profile(db: sqlite3.Connection, account_id: str) -> Dict:
    row = db.execute("SELECT payload FROM profiles WHERE account_id = ?", (account_id,)).fetchone()
    if row is None:
        raise HTTPException(status_code=404, detail="Profile not found")
    try:
        value = json.loads(row["payload"])
    except (TypeError, json.JSONDecodeError):
        raise HTTPException(status_code=500, detail="Stored profile is invalid")
    if not isinstance(value, dict):
        raise HTTPException(status_code=500, detail="Stored profile is invalid")
    return value


def write_profile(db: sqlite3.Connection, account_id: str, profile: Dict) -> None:
    db.execute(
        "UPDATE profiles SET payload = ?, updated_at = ? WHERE account_id = ?",
        (json.dumps(profile, separators=(",", ":"), sort_keys=True), int(time.time()), account_id),
    )


class GuestLoginResponse(BaseModel):
    account_id: str
    access_token: str
    token_type: str = "bearer"
    profile: Dict


class SummonRequest(BaseModel):
    count: int = Field(default=1, description="Only 1 or 10 pulls are supported")


def roll_rarity(profile: Dict) -> str:
    roll = secrets.randbelow(10000)
    if int(profile.get("pity_legendary", 0)) >= LEGENDARY_PITY - 1:
        return "LEGENDARY"
    if int(profile.get("pity_epic", 0)) >= EPIC_PITY - 1 and roll < EPIC_PITY_GUARANTEE_CUTOFF:
        return "EPIC"
    cursor = 0
    for rarity, weight in RARITY_WEIGHTS:
        cursor += weight
        if roll < cursor:
            return rarity
    return "LEGENDARY"


@app.get("/health")
def health():
    return {"status": "ok", "service": "sands-of-legends", "time": int(time.time())}


@app.post("/auth/guest", response_model=GuestLoginResponse)
def guest_login():
    account_id = uuid.uuid4().hex
    token = secrets.token_urlsafe(40)
    token_hash = hashlib.sha256(token.encode("utf-8")).hexdigest()
    profile = default_profile()
    now = int(time.time())
    with connect() as db:
        db.execute("BEGIN IMMEDIATE")
        db.execute(
            "INSERT INTO accounts(account_id, token_hash, created_at) VALUES(?, ?, ?)",
            (account_id, token_hash, now),
        )
        db.execute(
            "INSERT INTO profiles(account_id, payload, updated_at) VALUES(?, ?, ?)",
            (account_id, json.dumps(profile, separators=(",", ":")), now),
        )
    return GuestLoginResponse(account_id=account_id, access_token=token, profile=profile)


@app.get("/profile")
def get_profile(authorization: Optional[str] = Header(default=None)):
    account_id = require_account(authorization)
    with connect() as db:
        return {"account_id": account_id, "profile": read_profile(db, account_id)}


@app.post("/summon")
def summon(payload: SummonRequest, authorization: Optional[str] = Header(default=None)):
    account_id = require_account(authorization)
    if payload.count not in (1, 10):
        raise HTTPException(status_code=400, detail="count must be 1 or 10")
    cost = SUMMON_COST_TEN if payload.count == 10 else SUMMON_COST_SINGLE
    now = int(time.time())
    results: List[Dict] = []

    with connect() as db:
        db.execute("BEGIN IMMEDIATE")
        profile = read_profile(db, account_id)
        gems = int(profile.get("gems", 0))
        if gems < cost:
            raise HTTPException(status_code=409, detail="Not enough Gems")
        profile["gems"] = gems - cost
        heroes = profile.setdefault("heroes", {})
        for _ in range(payload.count):
            rarity = roll_rarity(profile)
            pool = HERO_POOLS[rarity]
            hero_id = pool[secrets.randbelow(len(pool))]
            is_new = hero_id not in heroes
            shards = 0
            if is_new:
                heroes[hero_id] = {"level": max(1, int(profile.get("highest_hero_level", 1)) - 5), "stars": 0, "shards": 0}
            else:
                shards = DUPLICATE_SHARDS[rarity]
                heroes[hero_id]["shards"] = int(heroes[hero_id].get("shards", 0)) + shards

            if rarity == "LEGENDARY":
                profile["pity_epic"] = 0
                profile["pity_legendary"] = 0
            elif rarity == "EPIC":
                profile["pity_epic"] = 0
                profile["pity_legendary"] = int(profile.get("pity_legendary", 0)) + 1
            else:
                profile["pity_epic"] = int(profile.get("pity_epic", 0)) + 1
                profile["pity_legendary"] = int(profile.get("pity_legendary", 0)) + 1

            results.append({
                "hero_id": hero_id,
                "rarity": rarity,
                "is_new": is_new,
                "shards_gained": shards,
            })
            db.execute(
                """INSERT INTO summon_audit
                   (account_id, created_at, rates_version, rarity, hero_id, is_new, shards_gained)
                   VALUES(?, ?, ?, ?, ?, ?, ?)""",
                (account_id, now, 1, rarity, hero_id, 1 if is_new else 0, shards),
            )

        write_profile(db, account_id, profile)

    return {
        "results": results,
        "profile": profile,
        "rates_version": 1,
        "rates": RARITY_RATES.copy(),
        "pity": {"epic": profile["pity_epic"], "legendary": profile["pity_legendary"]},
    }


@app.get("/summon/history")
def summon_history(limit: int = 20, authorization: Optional[str] = Header(default=None)):
    account_id = require_account(authorization)
    limit = max(1, min(100, limit))
    with connect() as db:
        rows = db.execute(
            """SELECT created_at, rarity, hero_id, is_new, shards_gained
               FROM summon_audit WHERE account_id = ?
               ORDER BY id DESC LIMIT ?""",
            (account_id, limit),
        ).fetchall()
    return {"items": [dict(row) for row in rows]}


@app.get("/arena/status")
def arena_status():
    return {
        "status": "locked",
        "reason": "Arena requires server-side opponent snapshots and verified BattleSim replay integration.",
    }


@app.get("/boss/status")
def boss_status():
    return {
        "status": "locked",
        "reason": "Weekly boss requires server-side damage validation, attempts and reward ranking.",
    }
