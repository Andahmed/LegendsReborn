# Home Hub — Landscape Presentation

The Home scene uses landscape orientation. `HomeWorldStage` adds `HomeLandscapePresentation` at runtime; it locks the Home game to landscape, frames the 3D world edge-to-edge, and lays out the HUD for a wide screen.

## Home HUD layout
- Player profile at upper left; live gold, gems and energy values at upper right.
- Events and login/pass shortcuts across the upper edge.
- Feature buttons at both sides so the castle/world remains visible in the middle.
- Idle rewards and Campaign actions above the bottom bar.
- Chat preview at lower left; existing Home/Heroes/Bag/Guild/Arena/More navigation along the lower edge.

Landmark labels on the 3D map use the project destinations: Summon, PVP, Guild, Shop, Continent and Dungeon. Tapping them invokes the existing HomeScreen events.

## Apply and verify
1. Open the project with Unity 2022.3.62f3 and allow scripts/assets to import.
2. Open `Assets/Scenes/Home.unity` and enter Play mode. If the scene has not been generated, run **Sands → Setup MVP Home Scene** first.
3. Test a landscape Android device and verify that the profile/resource strips, side controls, landmark names, chat strip, and bottom navigation do not overlap at the target resolution.
4. Unity Editor Play Mode, Android rendering, device orientation, text/font appearance, and touch hit areas still require verification; source changes alone do not prove a successful device build.
