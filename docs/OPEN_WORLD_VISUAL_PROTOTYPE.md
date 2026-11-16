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
- A separate directional light and wide camera framing.

## Generate in Unity
1. Open the project with Unity 2022.3.62f3.
2. Wait for scripts to compile.
3. Choose **Sands → World → Generate Open World Prototype**.
4. Open `Assets/Scenes/OpenWorldPrototype.unity` and press Play.

## Current limitations
- This is a visual terrain blockout, not yet a connected playable world.
- There is no player controller, streaming, minimap, quest triggers, NPC logic, collision tuning, or mobile performance profile yet.
- Existing Quaternius assets are reused from the LegendsReborn project. NJ MMO assets have not been copied into this branch; each external model must be checked for its individual license and attribution before import.
- The prototype has not been compiled or rendered in Unity in this environment. Review it in the Unity Editor before treating the visuals or performance as approved.

## Next art pass
Replace placeholder landmark geometry with license-verified GLB/FBX environment packs, improve biome transitions, add proper terrain textures and foliage LODs, then connect exploration locations to the existing LegendsReborn story and stage progression.
