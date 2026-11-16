# LegendsReborn — Open World Visual Prototype

## Scope and safety
- This is an isolated visual prototype on `feature/open-world-visual-upgrade`.
- It does not replace the original story, Home hub, campaign stages, battle simulation, save data, or UI events.
- It creates a separate scene: `Assets/Scenes/OpenWorldPrototype.unity`.
- The current Home scene and existing game flow remain unchanged.

## Prototype contents
- A 320m procedural terrain mesh with rolling hills and two raised mountain ranges.
- Four visual terrain regions: meadow, forest grass, rock and snow.
- A winding kingdom trade road and a river ribbon.
- Scattered existing Quaternius nature prefabs from `Resources/HomeNature`, with procedural fallback trees.
- Five visual landmarks to establish exploration destinations: Old Kingdom Outpost, Forgotten Watchtower, Ancient Ruins, Northern Gate and World Boss Clearing.
- A separate directional light and third-person follow camera.
- A controllable Warrior preview character with a CharacterController, keyboard movement in the Editor, and a left-side touch joystick on mobile.
- Proximity markers that display the current named landmark in the exploration HUD.

## Generate in Unity
1. Open the project with Unity 2022.3.62f3.
2. Wait for scripts to compile.
3. Choose **Sands → World → Generate Open World Prototype**.
4. Open `Assets/Scenes/OpenWorldPrototype.unity` and press Play.

## Current limitations
- This is a visual terrain blockout, not yet a connected playable world.
- This remains a single-scene exploration slice: there is no world streaming, minimap, combat encounter spawning, NPC dialogue, quest rewards, save integration, or mobile performance profile yet.
- The Warrior preview uses a static imported model; locomotion animations are not wired.
- The landmark proximity HUD is informational only and does not alter campaign progression.
- Existing Quaternius assets are reused from the LegendsReborn project. NJ MMO assets have not been copied into this branch; each external model must be checked for its individual license and attribution before import.
- The prototype has not been compiled or rendered in Unity in this environment. Review it in the Unity Editor before treating the visuals or performance as approved.

## Next art pass
Replace placeholder landmark geometry with license-verified GLB/FBX environment packs, improve biome transitions, add proper terrain textures and foliage LODs, then connect exploration locations to the existing LegendsReborn story and stage progression.


## Follow-up hardening

- Water is positioned just above the terrain and uses transparent blending where supported by the active shader.
- Material creation checks several Built-in/URP shader names and reports a clear error if none are available.
- The terrain mesh uses Unity's default 16-bit index format; its 161 × 161 vertices fit within that limit, which is friendlier to mobile GPUs.
- The editor installer now creates and closes an additive preview scene, preserving whichever scene was already open.

## Validation status

These changes were reviewed against the repository source, but have **not** yet been compiled or opened in the Unity Editor. The generated scene, visual quality, and Android performance remain unverified. The preview now includes a basic player controller, third-person camera, and landmark proximity HUD. Landmarks remain placeholder geometry; animation, streaming, NPCs, quests, save integration, and story progression are not implemented.
