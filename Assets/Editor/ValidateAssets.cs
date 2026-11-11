// =============================================================================
// FILE: Assets/Editor/ValidateAssets.cs
// OWNER ROLE: Build Engineer
// RESPONSIBILITY: Enforce art registration, hero polygon budgets, Resources restrictions, and required hero prefab components.
// WORK ON: Extend validation to imported Humanoid rigs, animation clips, enemy/stage references and environment budgets.
// STATUS: Prototype validation
// =============================================================================
using System;
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
            if (!System.IO.File.Exists("docs/ASSETS.md"))
                throw new Exception("Missing docs/ASSETS.md asset register.");
            string[] heroGuids = AssetDatabase.FindAssets("t:HeroDefinition", new[] { "Assets/Data/Heroes" });
            if (heroGuids.Length < 4)
                throw new Exception("Expected at least four prototype HeroDefinition assets.");

            for (int i = 0; i < heroGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(heroGuids[i]);
                HeroDefinition hero = AssetDatabase.LoadAssetAtPath<HeroDefinition>(path);
                if (hero == null || hero.prefab == null)
                    throw new Exception("Hero has no prefab reference: " + path);
                if (hero.prefab.GetComponent<Animator>() == null || hero.prefab.GetComponent<HeroView>() == null)
                    throw new Exception("Hero prefab must contain Animator and HeroView: " + AssetDatabase.GetAssetPath(hero.prefab));
                int triangles = CountTriangles(hero.prefab);
                if (triangles > 8000)
                    throw new Exception("Hero exceeds 8k triangle budget (" + triangles + "): " + AssetDatabase.GetAssetPath(hero.prefab));
            }

            string[] allGuids = AssetDatabase.FindAssets("", new[] { "Assets" });
            for (int i = 0; i < allGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(allGuids[i]);
                if (path.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase) || path.Contains("/Resources/"))
                    throw new Exception("Unapproved asset under Resources/: " + path);
            }
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
