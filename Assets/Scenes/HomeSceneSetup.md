# Home Scene Foundation

The Home screen is a mobile-first HUD layer over a 3D home-world scene.

## Scene composition
- Center: hero display point in front of the castle.
- Background: castle gate, faction flags, walls and supporting buildings.
- Foreground: clear ground plane and readable hero silhouette.
- HUD: resource strip at the top, Heroes/Formation/Quests on the left, Summon/Shop on the right, Campaign action at the bottom.
- Palette: turquoise, warm gold, sand and deep blue-green; restrained Arabic geometric trim.

## Unity setup
1. Create a scene named `Home` and add it to Build Settings after `Boot`.
2. Add a GameObject named `HomeScreen` and attach `HomeScreen.cs`.
3. The script creates its Canvas/HUD at runtime if no Canvas is assigned.
4. Assign the six navigation UnityEvents to the matching screen navigation once those screens are wired.
5. Place the approved castle, flags, gate/buildings and selected hero prefab in the 3D scene. Keep the hero display area unobstructed by HUD controls.

## Current scope
This is the Home HUD/navigation foundation only. It does not fabricate replacement character or environment art, and it does not claim that the 3D assets, scene file, animations or Android build are complete.
