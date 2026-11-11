# 3D Asset Register and Production Budgets

All assets must be original procedural Unity assets or explicitly licensed CC0 assets. No commercial-game assets, characters, names, meshes, textures, or animation data are permitted.

## Mandatory budgets and import rules

| Asset type | Triangle budget | Scale / format | Texture budget |
|---|---:|---|---|
| Hero | 8,000 | 1 Unity unit = 1 metre; 1.6–2.0 m tall; feet pivot; forward +Z; FBX/GLB or procedural Unity mesh | Flat colours or one 1024x1024 PNG atlas |
| Enemy | 6,000 | Same scale/pivot/orientation; FBX/GLB or procedural Unity mesh | Flat colours or one 1024x1024 PNG atlas |
| Prop | 1,500 | FBX/GLB or procedural Unity mesh | Flat colours or shared atlas |
| Complete battle environment | 40,000 | FBX/GLB or procedural Unity mesh | No 4K textures |
| VFX | Particle System prefab | Assets/Art/VFX/<faction>/ | No oversized textures |

Android texture compression target: ASTC 6x6. Unused assets must not be placed under Resources; use direct prefab references or Addressables.

Names use lowercase snake_case and type prefixes: hero_<id>, enemy_<id>, prop_<name>, env_<biome>, and vfx_<name>.

## Prototype assets (procedurally generated)

| Name | Source (link) | License | Usage |
|---|---|---|---|
| hero_fortress | [ArtPrototypeSetup.cs](../Assets/Editor/ArtPrototypeSetup.cs) | Original procedural work; no third-party source | Fortress prototype hero prefab |
| hero_wild | [ArtPrototypeSetup.cs](../Assets/Editor/ArtPrototypeSetup.cs) | Original procedural work; no third-party source | Wild prototype hero prefab |
| hero_forest | [ArtPrototypeSetup.cs](../Assets/Editor/ArtPrototypeSetup.cs) | Original procedural work; no third-party source | Forest prototype hero prefab |
| hero_shadow | [ArtPrototypeSetup.cs](../Assets/Editor/ArtPrototypeSetup.cs) | Original procedural work; no third-party source | Shadow prototype hero prefab |
| vfx_fortress_skill, vfx_fortress_ultimate, vfx_wild_skill, vfx_wild_ultimate, vfx_forest_skill, vfx_forest_ultimate, vfx_shadow_skill, vfx_shadow_ultimate | [ArtPrototypeSetup.cs](../Assets/Editor/ArtPrototypeSetup.cs) | Original procedural work; no third-party source | Faction skill and ultimate particles |
| env_forest, env_desert, env_mountain, env_sea, env_ruins | [ArtPrototypeSetup.cs](../Assets/Editor/ArtPrototypeSetup.cs) | Original procedural work; no third-party source | Placeholder biome environment prefabs |

These are low-poly primitive prototypes, not production FBX character assets. They do not claim to contain a Humanoid skeletal rig or authored Idle/Run/Attack/Skill/Ultimate/Hit/Death animation clips. Those remain required before production art acceptance.

## Required asset locations

- Assets/Art/Characters/<hero_id>/: hero prefab, generated mesh hierarchy, atlas and animation imports.
- Assets/Art/Characters/Enemies/<enemy_id>/: enemy assets.
- Assets/Art/Environments/<biome>/: biome prefab and props.
- Assets/Art/VFX/<faction>/: faction Particle System prefabs.
- Assets/Prefabs/Heroes/: battle-ready hero prefabs with Animator, HeroView and HP anchor.
- Assets/Prefabs/UI/: portraits, icons and frames.

Every added or downloaded asset must be added to this register before its PR is accepted.
