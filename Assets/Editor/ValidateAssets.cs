// =============================================================================
// FILE: Assets/Editor/ValidateAssets.cs
// OWNER ROLE: Build Engineer
// RESPONSIBILITY: Validate registered 3D assets, polygon budgets, Humanoid rigs, seven animation clips, Resources restrictions and Android sprite compression.
// WORK ON: Extend validation to every campaign stage as stage data is added.
// STATUS: Prototype art validation
// =============================================================================
using System;
using System.IO;
using SandsOfLegends.Core;
using SandsOfLegends.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SandsOfLegends.EditorTools
{
    public static class ValidateAssets
    {
        private static readonly string[] RequiredClips =
            { "Idle", "Run", "Attack", "Skill", "Ultimate", "Hit", "Death" };

        [MenuItem("Sands/Validate Assets")]
        public static void Validate()
        {
            ValidateAssetsOrThrow();
            Debug.Log("Sands 3D asset validation passed.");
        }

        public static void ValidateAssetsOrThrow()
        {
            const string registerPath = "docs/ASSETS.md";
            if (!File.Exists(registerPath))
                throw new Exception("Missing docs/ASSETS.md.");
            string register = File.ReadAllText(registerPath);

            string[] heroGuids = AssetDatabase.FindAssets("t:HeroDefinition", new[] { "Assets/Data/Heroes" });
            if (heroGuids.Length < 4)
                throw new Exception("Expected four registered HeroDefinition assets.");

            for (int i = 0; i < heroGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(heroGuids[i]);
                HeroDefinition hero = AssetDatabase.LoadAssetAtPath<HeroDefinition>(path);
                if (hero == null || hero.prefab == null)
                    throw new Exception("HeroDefinition has no prefab reference: " + path);
                if (hero.portrait == null || hero.factionIcon == null)
                    throw new Exception("HeroDefinition is missing its portrait or faction icon: " + path);
                ValidateCharacterPrefab(hero.prefab, 8000, register, true);
                string heroId = hero.id.StartsWith("hero_", StringComparison.Ordinal) ? hero.id.Substring(5) : hero.id;
                string heroFolder = "Assets/Art/Characters/" + heroId + "/";
                GameObject artPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(heroFolder + "hero_" + heroId + ".prefab");
                if (artPrefab == null) throw new Exception("Missing character-folder prefab: " + heroFolder);
                ValidateCharacterPrefab(artPrefab, 8000, register, true);
                ValidateFbxModel(heroFolder + "hero_" + heroId + ".fbx", 8000, register, true);
                ValidateAnimationSet(heroFolder + "anims/", register);
            }

            GameObject enemy = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Art/Characters/Enemies/gloom_raider/enemy_gloom_raider.prefab");
            if (enemy == null) throw new Exception("Missing enemy_gloom_raider.prefab.");
            ValidateCharacterPrefab(enemy, 6000, register, true);
            string enemyFolder = "Assets/Art/Characters/Enemies/gloom_raider/";
            ValidateFbxModel(enemyFolder + "enemy_gloom_raider.fbx", 6000, register, true);
            ValidateAnimationSet(enemyFolder + "anims/", register);
            string[] biomes = { "forest", "desert", "mountain", "sea", "ruins" };
            for (int i = 0; i < biomes.Length; i++)
            {
                string root = "Assets/Art/Environments/" + biomes[i] + "/";
                ValidateFbxModel(root + "env_" + biomes[i] + ".fbx", 40000, register);
                ValidateFbxModel(root + "props/prop_ancient_gate.fbx", 1500, register);
                ValidateFbxModel(root + "props/prop_forest_crystal.fbx", 1500, register);
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Art", "Assets/Prefabs/Heroes" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                if (path.Contains("/anims/")) continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (!register.Contains(fileName))
                    throw new Exception("Prefab is not registered in docs/ASSETS.md: " + fileName);
                int budget = BudgetFor(path);
                if (budget > 0)
                {
                    int triangles = CountTriangles(prefab);
                    if (triangles > budget)
                        throw new Exception(path + " exceeds " + budget + " triangles: " + triangles);
                }
                if (!HasRequiredPrefix(fileName))
                    throw new Exception("Invalid asset name prefix: " + path);
            }

            ValidateEnvironmentPrefabs();
            ValidateVfxPrefabs();
            ValidateStageData();
            ValidatePortraits();

            string[] allGuids = AssetDatabase.FindAssets("", new[] { "Assets" });
            for (int i = 0; i < allGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(allGuids[i]);
                if (path.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase) ||
                    path.Contains("/Resources/"))
                    throw new Exception("Unapproved asset under Resources/: " + path);
            }
        }

        private static void ValidateCharacterPrefab(GameObject prefab, int budget, string register, bool requireHumanoid)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            if (!register.Contains(Path.GetFileNameWithoutExtension(path)))
                throw new Exception("Character prefab is not registered: " + path);
            Animator animator = prefab.GetComponent<Animator>();
            HeroView view = prefab.GetComponent<HeroView>();
            if (animator == null || view == null)
                throw new Exception("Character prefab needs Animator and HeroView: " + path);
            if (requireHumanoid && (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman))
                throw new Exception("Character prefab needs a valid Humanoid Avatar: " + path);
            if (animator.runtimeAnimatorController == null)
                throw new Exception("Character prefab has no Animator Controller: " + path);
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < RequiredClips.Length; i++)
            {
                bool found = false;
                for (int c = 0; c < clips.Length; c++)
                    if (clips[c] != null && clips[c].name == RequiredClips[i]) { found = true; break; }
                if (!found) throw new Exception("Missing " + RequiredClips[i] + " clip on " + path);
            }
            int triangles = CountTriangles(prefab);
            if (triangles > budget)
                throw new Exception(path + " exceeds " + budget + " triangles: " + triangles);
        }



        private static void ValidateAnimationSet(string folder, string register)
        {
            string[] clips = { "idle", "run", "attack", "skill", "ultimate", "hit", "death" };
            for (int i = 0; i < clips.Length; i++)
            {
                string path = folder + clips[i] + ".fbx";
                if (!File.Exists(path))
                    throw new Exception("Missing animation FBX: " + path);
                if (!register.Contains(clips[i]))
                    throw new Exception("Animation FBX is not registered: " + clips[i]);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null || importer.animationType != ModelImporterAnimationType.Human ||
                    importer.avatarSetup != ModelImporterAvatarSetup.CopyFromOther ||
                    importer.sourceAvatar == null || !importer.importAnimation)
                    throw new Exception("Animation FBX must use the character's Humanoid Avatar: " + path);
                UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                bool hasClip = false;
                for (int a = 0; a < subAssets.Length; a++)
                    if (subAssets[a] is AnimationClip) { hasClip = true; break; }
                if (!hasClip) throw new Exception("Animation FBX contains no imported AnimationClip: " + path);
            }
        }

        private static void ValidateFbxModel(string path, int budget, string register, bool requireHumanoid = false)
        {
            if (!File.Exists(path))
                throw new Exception("Missing required FBX model: " + path);
            string name = Path.GetFileNameWithoutExtension(path);
            if (!register.Contains(name))
                throw new Exception("FBX model is not registered in docs/ASSETS.md: " + name);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                throw new Exception("Unity did not import FBX model: " + path);
            if (requireHumanoid)
            {
                if (importer.animationType != ModelImporterAnimationType.Human ||
                    importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
                    throw new Exception("Character FBX is not configured as a Humanoid rig: " + path);
                UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                bool validAvatar = false;
                for (int i = 0; i < subAssets.Length; i++)
                {
                    Avatar avatar = subAssets[i] as Avatar;
                    if (avatar != null && avatar.isValid && avatar.isHuman) { validAvatar = true; break; }
                }
                if (!validAvatar) throw new Exception("Character FBX has no valid imported Humanoid Avatar: " + path);
            }
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null)
                throw new Exception("Unity could not import FBX model: " + path);
            int triangles = CountTriangles(model);
            if (triangles > budget)
                throw new Exception(path + " exceeds " + budget + " triangles: " + triangles);
        }


        private static void ValidateEnvironmentPrefabs()
        {
            string[] biomes = { "forest", "desert", "mountain", "sea", "ruins" };
            for (int i = 0; i < biomes.Length; i++)
            {
                string root = "Assets/Art/Environments/" + biomes[i] + "/";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(root + "env_" + biomes[i] + ".prefab") == null)
                    throw new Exception("Missing biome prefab: " + biomes[i]);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(root + "props/prop_ancient_gate.prefab") == null ||
                    AssetDatabase.LoadAssetAtPath<GameObject>(root + "props/prop_forest_crystal.prefab") == null)
                    throw new Exception("Missing registered props for biome: " + biomes[i]);
            }
        }

        private static void ValidateVfxPrefabs()
        {
            string[] factions = { "fortress", "wild", "forest", "shadow", "raider" };
            string[] types = { "skill", "ultimate" };
            for (int f = 0; f < factions.Length; f++)
            for (int t = 0; t < types.Length; t++)
            {
                string path = "Assets/Art/VFX/" + factions[f] + "/vfx_" + factions[f] + "_" + types[t] + ".prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null || prefab.GetComponent<ParticleSystem>() == null)
                    throw new Exception("Missing Particle System VFX prefab: " + path);
            }
        }

        private static void ValidateStageData()
        {
            StageDefinition stage = AssetDatabase.LoadAssetAtPath<StageDefinition>("Assets/Data/Stages/Stage_001.asset");
            if (stage == null || stage.enemyPrefabs == null || stage.enemyPrefabs.Length < 4)
                throw new Exception("Stage_001 must reference four enemy prefabs.");
            for (int i = 0; i < stage.enemyPrefabs.Length; i++)
                if (stage.enemyPrefabs[i] == null)
                    throw new Exception("Stage_001 has a missing enemy prefab at slot " + i + ".");
            if (stage.environmentPrefab == null)
                throw new Exception("Stage_001 has no environment prefab.");
        }

        private static void ValidatePortraits()
        {
            string[] ids = { "fortress", "wild", "forest", "shadow" };
            for (int i = 0; i < ids.Length; i++)
            {
                string path = "Assets/Prefabs/UI/hero_" + ids[i] + "_portrait.png";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType != TextureImporterType.Sprite)
                    throw new Exception("Missing Sprite portrait: " + path);
                if (!register.Contains(Path.GetFileNameWithoutExtension(path)))
                    throw new Exception("Portrait is not registered: " + path);
                ValidateAndroidAstc(importer, path);
                string iconPath = "Assets/Prefabs/UI/faction_" + ids[i] + "_icon.png";
                TextureImporter iconImporter = AssetImporter.GetAtPath(iconPath) as TextureImporter;
                if (iconImporter == null || iconImporter.textureType != TextureImporterType.Sprite)
                    throw new Exception("Missing faction icon Sprite: " + iconPath);
                if (!register.Contains(Path.GetFileNameWithoutExtension(iconPath)))
                    throw new Exception("Faction icon is not registered: " + iconPath);
                ValidateAndroidAstc(iconImporter, iconPath);

            }
            const string framePath = "Assets/Prefabs/UI/ui_frame_gold.png";
            TextureImporter frameImporter = AssetImporter.GetAtPath(framePath) as TextureImporter;
            if (frameImporter == null || frameImporter.textureType != TextureImporterType.Sprite ||
                !register.Contains("ui_frame_gold"))
                throw new Exception("Missing or unregistered UI frame Sprite.");
            ValidateAndroidAstc(frameImporter, framePath);
        }


        private static void ValidateAndroidAstc(TextureImporter importer, string path)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Android");
            if (!settings.overridden || settings.format != TextureImporterFormat.ASTC_6x6)
                throw new Exception("Texture must use ASTC 6x6 on Android: " + path);
        }

        private static int BudgetFor(string path)
        {
            if (path.Contains("/Characters/Enemies/")) return 6000;
            if (path.Contains("/Characters/")) return 8000;
            if (path.Contains("/props/")) return 1500;
            if (path.Contains("/Environments/")) return 40000;
            if (path.Contains("/Prefabs/Heroes/")) return 8000;
            return 0;
        }

        private static bool HasRequiredPrefix(string name)
        {
            return name.StartsWith("hero_", StringComparison.Ordinal) ||
                   name.StartsWith("enemy_", StringComparison.Ordinal) ||
                   name.StartsWith("prop_", StringComparison.Ordinal) ||
                   name.StartsWith("env_", StringComparison.Ordinal) ||
                   name.StartsWith("vfx_", StringComparison.Ordinal);
        }

        private static int CountTriangles(GameObject prefab)
        {
            int total = 0;
            MeshFilter[] filters = prefab.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
                if (filters[i].sharedMesh != null) total += filters[i].sharedMesh.triangles.Length / 3;
            SkinnedMeshRenderer[] skinned = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinned.Length; i++)
                if (skinned[i].sharedMesh != null) total += skinned[i].sharedMesh.triangles.Length / 3;
            return total;
        }
    }
}
