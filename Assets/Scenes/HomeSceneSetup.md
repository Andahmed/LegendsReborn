# Home Scene — Mobile Hub Setup

## Target composition
Build the Home scene as a portrait mobile hub matching the approved fantasy direction:
- A large 3D castle gate and walls as the center background, with faction banners and side buildings.
- One selected 3D hero standing on the foreground platform, facing the camera.
- Keep the center third of the screen visually clear so the hero remains readable behind the HUD.
- Turquoise, deep blue-green, warm gold and sand palette; restrained Arabic geometric decoration.

## HUD delivered in HomeScreen.cs
The runtime uGUI layer now creates:
- Player profile / level strip and live Gems, Gold and Energy values.
- Events, Daily Login, Battle Pass and Monthly Card rail.
- Heroes, Formation, Quests and Equipment navigation on the left.
- Summon, Arena, Alliance Boss and Shop navigation on the right.
- Idle Rewards card with a claim event.
- Campaign stage label and primary Campaign button.
- Bottom navigation for Home, Heroes, Bag, Guild, Arena and More.

All destinations are exposed as serialized UnityEvents on HomeScreen so they can be connected to actual screens in the Inspector. Until those screens are implemented and assigned, the corresponding event has no destination; do not treat the navigation as fully integrated.

## Unity scene setup
1. Create a scene named `Home` and add it to Build Settings after `Boot`.
2. Add the existing persistent GameBootstrap object through the project's boot flow; do not duplicate it in Home.
3. Add a GameObject named `HomeScreen` and attach `Assets/Scripts/UI/Screens/HomeScreen.cs`.
4. The script creates a portrait Canvas (1080×1920 reference) and safe-area root at runtime if none are assigned.
5. Add the approved castle/environment and selected hero prefab to the 3D scene. Add camera, lighting and ground; keep the hero centered in the open area.
6. Connect each HomeScreen UnityEvent to its matching screen controller when available.
7. Test at 720×1280, 1080×1920 and a narrow/tall Android aspect ratio, including devices with camera cutouts.

## Known limitations — not signed off
- The repository currently contains no committed Home scene, castle prefab, hero prefab, or imported environment art in this branch. The HUD code does not generate or fake those assets.
- Arabic text shaping is not enabled by the built-in Arial font. Current labels are English until a properly licensed Arabic-capable font and shaping/TMP setup are added.
- Idle Rewards claim and navigation events are integration hooks only; the claim event must be connected to the real reward flow.
- This change has not been compiled or run in Unity Editor and has not been tested on Android. A Unity compile, visual layout pass and device build are required before calling the screen complete.
