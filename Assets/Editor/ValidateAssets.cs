// =============================================================================
// FILE: Assets/Editor/ValidateAssets.cs
// OWNER ROLE: Build Engineer
// RESPONSIBILITY: Enforce art registration, polygon budgets, Resources restrictions, and required hero prefab components.
// WORK ON: Extend validation to imported Humanoid rigs, animation clips, enemy/stage references and environment budgets.
// STATUS: Prototype validation
// =============================================================================
using System;
using System.IO;
using SandsOfLegends.Core;
using SandsOfLegends.Presentation;
using UnityEditor;
using UnityEngine;

namespace SandsOfLegends.EditorTools
{
    public static class ValidateAssets
    {
        [MenuItem("Sands/Validate Assets")]
        public static void Validate()
        {
            ValidateAssetsOrThrow();
            Debug.Log("Sands asset validation passed.");
        }

        public static void ValidateAssetsOrThrow()
        {
            const string registerPath = "docs/ASSETS.md";
            if (!File.Exists(registerPath))
                throw new Exception("Missing docs/ASSETS.md asset register.");
            string register = File.ReadAllText(registerPath);

            string[] heroGuids = AssetDatabase.FindAssets("t:HeroDefinition", new[] { "Assets/Data/Heroes" });
            if (heroGuids.Length < 4)
                throw new Exception("Expected at least four prototype HeroDefinition assets.");

            for (int i = 0; i < heroGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(heroGuids[i]);
                HeroDefinition hero = AssetDatabase.LoadAssetAtPath<HeroDefinition>(path);
                if (hero == null || hero.prefab == null)
                    throw new Exception("Hero has no prefab reference: " + path);
                if (string.IsNullOrEmpty(hero.id) || !register.Contains(hero.id))
                    throw new Exception("Hero is missing from docs/ASSETS.md: " + hero.id);
                if (hero.prefab.GetComponent<Animator>() == null || hero.prefab.GetComponent<HeroView>() == null)
                    throw new Exception("Hero prefab must contain Animator and HeroView: " + AssetDatabase.GetAssetPath(hero.prefab));
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Art", "Assets/Prefabs/Heroes" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                int budget = BudgetFor(path);
                if (budget <= 0) continue;
                int triangles = CountTriangles(prefab);
                if (triangles > budget)
                    throw new Exception("Prefab exceeds " + budget + " triangle budget (" + triangles + "): " + path);
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (!HasRequiredPrefix(fileName))
                    throw new Exception("Prefab name must use an approved type prefix: " + path);
            }

            string[] allGuids = AssetDatabase.FindAssets("", new[] { "Assets" });
            for (int i = 0; i < allGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(allGuids[i]);
                if (path.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase) || path.Contains("/Resources/"))
                    throw new Exception("Unapproved asset under Resources/: " + path);
            }
        }

        private static int BudgetFor(string path)
        {
            if (path.Contains("/Characters/Enemies/")) return 6000;
            if (path.Contains("/Characters/")) return 8000;
            if (path.Contains("/Environments/")) return 40000;
            if (path.Contains("/props/") || path.Contains("/Props/")) return 1500;
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
