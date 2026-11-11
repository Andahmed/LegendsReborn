# FILE: Server/app/main.py
# OWNER ROLE: Server Programmer
# RESPONSIBILITY: FastAPI entry: health check + router registration. Server is the authority for accounts, profile, inventory and gacha (GDD 7.2).
# WORK ON: Implement routers in app/routers: auth, profile, summon (RNG on server + audit log), arena, boss. Client only displays results.
# STATUS: Stub (health only)

from fastapi import FastAPI

app = FastAPI(title="Sands of Legends Server")


@app.get("/health")
def health():
    return {"status": "ok"}

# TODO: include routers: auth, profile, summon, arena, boss (see Server/README.md)
