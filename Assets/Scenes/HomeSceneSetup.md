# Home Hub — Castle Stage

## Added in this branch
- Fixed left/right menu anchors so fixed-width rails are not collapsed to zero width.
- Creates an EventSystem + StandaloneInputModule only when the scene has none.
- Keeps the portrait HUD and adds a live idle reward preview based on the existing 12-hour capped reward calculation.
- Claim calls `GameState.ClaimIdle(TimeService.NowUnix)`, saves with `GameBootstrap.SaveNow()`, and refreshes the displayed values.
- Replaces the castle facade's primitive wall/gate/tower blocks with modular Quaternius FBX meshes (CC0): arch gate, wall modules, decorative wall pieces, columns, banners, stairs, floor and torches.
- Keeps the hero as a clearly temporary silhouette; hero replacement is a separate art task.

## Import the castle meshes (one-time)
1. Open the project in Unity Editor with internet access.
2. From the top menu choose **Legends Reborn → Art → Install Home Castle CC0 Models**.
3. Wait for Unity to finish importing the FBX files. The installer skips files already downloaded and writes the source/license note into `Assets/Resources/HomeCastle/LICENSE.txt`.
4. Enter Play mode again. `HomeWorldStage` loads the imported FBX meshes from `Resources/HomeCastle`.
5. If the installer cannot reach GitHub, the scene uses temporary geometric fallbacks for missing meshes; rerun the menu command after network access returns.

Source pack: [Quaternius Modular Dungeon Pack](https://quaternius.itch.io/lowpoly-modular-dungeon-pack), listed as CC0 by the official pack page. The installer fetches only the named FBX parts used by this home scene, not the entire source pack.

## Scene setup
1. Boot must initialize `GameBootstrap` before Home is shown.
2. Add an empty GameObject named `HomeScreen` to the Home scene and attach `HomeScreen.cs`.
3. The HUD Canvas is generated at runtime (1080×1920 reference resolution) if none is assigned.
4. `HomeWorldStage` builds the courtyard and loads the modular castle meshes.
5. Connect the public UnityEvents to real screen controllers in the Inspector as those screens are implemented.

## Not final / verification still required
- The castle is now assembled from real CC0 modular meshes, but it is not yet a bespoke final castle model; proportions, mesh pivots, materials and camera framing must be checked in Unity.
- The hero remains primitive placeholder geometry, not one of the approved five hero models.
- Built-in Arial does not provide Arabic shaping; current labels remain English.
- Navigation events without listeners remain hooks, not working destination screens.
- This repository update has not been compiled or run in Unity Editor, and no Android build or touch/aspect-ratio test has been performed.
