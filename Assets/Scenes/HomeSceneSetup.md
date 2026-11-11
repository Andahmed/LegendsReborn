# Home Hub — Castle Stage

## Added in this branch
- Fixed left/right menu anchors so fixed-width rails are not collapsed to zero width.
- Creates an EventSystem + StandaloneInputModule only when the scene has none.
- Keeps the portrait HUD and adds a live idle reward preview based on the existing 12-hour capped reward calculation.
- Claim now calls `GameState.ClaimIdle(TimeService.NowUnix)`, saves with `GameBootstrap.SaveNow()`, and refreshes the displayed values.
- Adds a procedural castle courtyard with gate, round towers, faction banners, torches and an armored hero silhouette.

## Scene setup
1. Boot must initialize `GameBootstrap` before Home is shown.
2. Add an empty GameObject named `HomeScreen` to the Home scene and attach `HomeScreen.cs`.
3. The HUD Canvas is generated at runtime (1080×1920 reference resolution) if none is assigned.
4. The castle preview is generated automatically by `HomeWorldStage`. Remove/disable this preview when the approved licensed castle and hero prefabs are imported.
5. Connect the public UnityEvents to real screen controllers in the Inspector as those screens are implemented.

## Not final / verification still required
- The hero and castle are primitive greybox geometry, not the approved five heroes or final 3D art.
- Built-in Arial does not provide Arabic shaping; current labels remain English.
- Navigation events without listeners remain hooks, not working destination screens.
- This repository update has not been compiled or run in Unity Editor, and no Android build or touch/aspect-ratio test has been performed.
