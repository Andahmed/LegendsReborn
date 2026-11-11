// =============================================================================
// FILE: Assets/Editor/ArtPrototypeSetup.cs
// OWNER ROLE: Technical Artist
// RESPONSIBILITY: Generate original low-poly prototype heroes, faction VFX, and biome prefabs without third-party assets.
// WORK ON: Replace primitive prototypes with approved rigged FBX/GLB assets and authored animation clips.
// STATUS: Prototype generator; not production character art
// =============================================================================
using SandsOfLegends.Core;
using SandsOfLegends.Presentation;
using UnityEditor;
using UnityEngine;

namespace SandsOfLegends.EditorTools
{
    public static class ArtPrototypeSetup
    {
        private const string ArtRoot = "Assets/Art";
        private static readonly string[] Ids = { "fortress", "wild", "forest", "shadow" };
        private static readonly Color[] Colors =
        {
            new Color(0.34f, 0.55f, 0.78f),
            new Color(0.82f, 0.53f, 0.25f),
            new Color(0.27f, 0.68f, 0.43f),
            new Color(0.48f, 0.35f, 0.72f)
        };

        [MenuItem("Sands/Setup 3D Prototype")]
        public static void Setup3DPrototype()
        {
            string[] heroGuids = AssetDatabase.FindAssets("t:HeroDefinition", new[] { "Assets/Data/Heroes" });
            HeroDefinition[] heroes = new HeroDefinition[heroGuids.Length];
            for (int i = 0; i < heroGuids.Length; i++)
                heroes[i] = AssetDatabase.LoadAssetAtPath<HeroDefinition>(AssetDatabase.GUIDToAssetPath(heroGuids[i]));
            GeneratePrototypeArt(heroes);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated procedural 3D prototype assets. These are placeholders, not rigged production characters.");
        }

        public static void GeneratePrototypeArt(HeroDefinition[] heroes)
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder(ArtRoot, "Characters");
            EnsureFolder(ArtRoot, "Environments");
            EnsureFolder(ArtRoot, "VFX");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "Heroes");
            EnsureFolder("Assets/Prefabs", "UI");
            EnsureFolder(ArtRoot, "Materials");

            for (int i = 0; i < Ids.Length; i++)
            {
                string id = Ids[i];
                EnsureFolder(ArtRoot + "/Characters", id);
                EnsureFolder(ArtRoot + "/VFX", id);
                EnsureFolder("Assets/Prefabs/Heroes", id);
                Material material = GetOrCreateMaterial(id, Colors[i]);
                ParticleSystem skillFx = CreateVfx(id, "skill", Colors[i]);
                ParticleSystem ultimateFx = CreateVfx(id, "ultimate", Color.Lerp(Colors[i], Color.white, 0.35f));
                string prefabPath = "Assets/Prefabs/Heroes/" + id + "/hero_" + id + ".prefab";
                GameObject heroPrefab = CreateHeroPrefab(id, material, skillFx, ultimateFx);
                PrefabUtility.SaveAsPrefabAsset(heroPrefab, prefabPath);
                Object.DestroyImmediate(heroPrefab);

                if (heroes != null)
                {
                    for (int h = 0; h < heroes.Length; h++)
                    {
                        if (heroes[h] == null || heroes[h].id != "hero_" + id) continue;
                        heroes[h].prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        EditorUtility.SetDirty(heroes[h]);
                    }
                }
            }

            string[] biomes = { "forest", "desert", "mountain", "sea", "ruins" };
            Color[] palette =
            {
                new Color(0.20f, 0.42f, 0.25f), new Color(0.72f, 0.55f, 0.30f),
                new Color(0.45f, 0.50f, 0.58f), new Color(0.18f, 0.42f, 0.58f),
                new Color(0.43f, 0.39f, 0.34f)
            };
            for (int i = 0; i < biomes.Length; i++)
            {
                EnsureFolder(ArtRoot + "/Environments", biomes[i]);
                string path = ArtRoot + "/Environments/" + biomes[i] + "/env_" + biomes[i] + ".prefab";
                GameObject environment = CreateEnvironment(biomes[i], palette[i]);
                PrefabUtility.SaveAsPrefabAsset(environment, path);
                Object.DestroyImmediate(environment);
            }
            AssetDatabase.SaveAssets();
        }

        private static GameObject CreateHeroPrefab(string id, Material material, ParticleSystem skillFx, ParticleSystem ultimateFx)
        {
            GameObject root = new GameObject("hero_" + id);
            GameObject visual = new GameObject("visual_root");
            visual.transform.SetParent(root.transform, false);
            AddPrimitive(visual.transform, PrimitiveType.Capsule, "body", new Vector3(0f, 0.98f, 0f), new Vector3(0.48f, 0.78f, 0.34f), material);
            AddPrimitive(visual.transform, PrimitiveType.Sphere, "head", new Vector3(0f, 1.62f, 0f), new Vector3(0.34f, 0.34f, 0.34f), material);
            AddPrimitive(visual.transform, PrimitiveType.Capsule, "left_arm", new Vector3(-0.36f, 0.98f, 0f), new Vector3(0.19f, 0.58f, 0.19f), material);
            AddPrimitive(visual.transform, PrimitiveType.Capsule, "right_arm", new Vector3(0.36f, 0.98f, 0f), new Vector3(0.19f, 0.58f, 0.19f), material);
            AddPrimitive(visual.transform, PrimitiveType.Capsule, "left_leg", new Vector3(-0.14f, 0.34f, 0f), new Vector3(0.19f, 0.58f, 0.19f), material);
            AddPrimitive(visual.transform, PrimitiveType.Capsule, "right_leg", new Vector3(0.14f, 0.34f, 0f), new Vector3(0.19f, 0.58f, 0.19f), material);

            GameObject hpAnchor = new GameObject("hp_bar_anchor");
            hpAnchor.transform.SetParent(root.transform, false);
            hpAnchor.transform.localPosition = new Vector3(0f, 2.05f, 0f);

            Animator animator = root.AddComponent<Animator>();
            HeroView view = root.AddComponent<HeroView>();
            view.animator = animator;
            view.visualRoot = visual.transform;
            view.hpBarAnchor = hpAnchor.transform;
            view.skillVfx = skillFx;
            view.ultimateVfx = ultimateFx;
            return root;
        }

        private static GameObject CreateEnvironment(string biome, Color color)
        {
            Material material = GetOrCreateMaterial("env_" + biome, color);
            GameObject root = new GameObject("env_" + biome);
            AddPrimitive(root.transform, PrimitiveType.Cube, "ground", new Vector3(0f, -0.18f, 0f), new Vector3(14f, 0.35f, 10f), material);
            AddPrimitive(root.transform, PrimitiveType.Cube, "left_marker", new Vector3(-5.5f, 0.7f, 2.5f), new Vector3(0.8f, 1.8f, 0.8f), material);
            AddPrimitive(root.transform, PrimitiveType.Cube, "right_marker", new Vector3(5.5f, 0.7f, 2.5f), new Vector3(0.8f, 1.8f, 0.8f), material);
            return root;
        }

        private static ParticleSystem CreateVfx(string faction, string kind, Color color)
        {
            GameObject root = new GameObject("vfx_" + faction + "_" + kind);
            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.7f; main.loop = false; main.startLifetime = 0.45f;
            main.startSpeed = 2.2f; main.startSize = 0.22f; main.startColor = color; main.maxParticles = 48;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(kind == "ultimate" ? 28 : 12)) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = kind == "ultimate" ? 0.7f : 0.35f;
            string path = ArtRoot + "/VFX/" + faction + "/vfx_" + faction + "_" + kind + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<ParticleSystem>();
        }

        private static Material GetOrCreateMaterial(string id, Color color)
        {
            string path = ArtRoot + "/Materials/" + id + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
                material = new Material(shader);
                material.color = color;
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }

        private static GameObject AddPrimitive(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            return part;
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
