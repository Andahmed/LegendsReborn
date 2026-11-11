# Legends Reborn

Unity mobile RPG (Android), clean rebuild.

- Unity: **2022.3.62f3** (matches Unity Cloud Build)
- Target: Android (IL2CPP, ARM64)

## Structure
```
Assets/
  Scripts/  Core | Characters | Combat | World | UI | Editor
  Resources/  Scenes/  Art/
Packages/manifest.json
ProjectSettings/ProjectVersion.txt
```

## Rules
- One save system, one currency set, one UI system (no parallel legacy copies).
- Build must pass locally before pushing to `main`.
- Large art packs go outside `Resources/` unless used at runtime.
