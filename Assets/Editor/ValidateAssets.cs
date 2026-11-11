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
                ValidateCharacterPrefab(hero.prefab, 8000, register, true);
            }

            GameObject enemy = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Art/Characters/Enemies/gloom_raider/enemy_gloom_raider.prefab");
            if (enemy == null) throw new Exception("Missing enemy_gloom_raider.prefab.");
            ValidateCharacterPrefab(enemy, 6000, register, true);

            string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Art", "Assets/Prefabs/Heroes" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
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
                ValidateAndroidAstc(importer, path);
                string iconPath = "Assets/Prefabs/UI/faction_" + ids[i] + "_icon.png";
                TextureImporter iconImporter = AssetImporter.GetAtPath(iconPath) as TextureImporter;
                if (iconImporter == null || iconImporter.textureType != TextureImporterType.Sprite)
                    throw new Exception("Missing faction icon Sprite: " + iconPath);
                ValidateAndroidAstc(iconImporter, iconPath);
                TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
                if (!android.overridden || android.format != TextureImporterFormat.ASTC_6x6)
                    throw new Exception("Portrait must use ASTC 6x6 on Android: " + path);
            }
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
