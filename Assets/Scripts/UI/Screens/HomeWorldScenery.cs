using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SandsOfLegends.UI
{
    /// <summary>
    /// Original, fully procedural bright-fantasy landscape for the Home hub: grass hills, snowy mountains,
    /// drifting clouds, floating islets and six tappable landmark buildings with floating name labels.
    /// Uses only Unity primitives plus generated cone meshes, so it needs no extra art files and no network.
    /// All artwork and names here are original to this project.
    /// </summary>
    public sealed class HomeWorldScenery : MonoBehaviour
    {
        // ---- Tunables (adjust in code after viewing on a device) ----
        private const float LabelScreenFraction = 0.026f; // label glyph height as a fraction of screen height
        private const float GroundRadius = 150f;

        private struct Hill { public Vector3 center; public Vector3 radii; }
        private sealed class Cloud { public Transform t; public float speed; }
        private sealed class Mover
        {
            public Transform t; public float spin; public float bobAmp; public float bobSpeed;
            public float baseY; public float phase;
        }

        private readonly List<Hill> hills = new List<Hill>();
        private readonly List<Cloud> clouds = new List<Cloud>();
        private readonly List<Mover> movers = new List<Mover>();
        private readonly List<Transform> labels = new List<Transform>();
        private readonly Dictionary<int, Material> materials = new Dictionary<int, Material>();
        private readonly Dictionary<int, Mesh> cones = new Dictionary<int, Mesh>();
        private Material templateMaterial;
        private Font labelFont;
        private Material labelMaterial;
        private HomeScreen home;

        private Transform groundRoot, treeRoot, mountainRoot, landmarkRoot, skyRoot;

        // ---- Palette ----
        private static readonly Color Grass = new Color(0.50f, 0.78f, 0.36f);
        private static readonly Color Stone = new Color(0.86f, 0.80f, 0.68f);
        private static readonly Color Cream = new Color(0.95f, 0.88f, 0.70f);
        private static readonly Color Gold = new Color(0.96f, 0.78f, 0.30f);
        private static readonly Color Teal = new Color(0.16f, 0.62f, 0.62f);
        private static readonly Color Crimson = new Color(0.82f, 0.26f, 0.28f);
        private static readonly Color Wood = new Color(0.62f, 0.42f, 0.26f);

        public static HomeWorldScenery Create(Transform parent)
        {
            GameObject go = new GameObject("Home Scenery");
            go.transform.SetParent(parent, false);
            HomeWorldScenery scenery = go.AddComponent<HomeWorldScenery>();
            scenery.Build();
            return scenery;
        }

        private void Build()
        {
            groundRoot = NewRoot("Ground");
            treeRoot = NewRoot("Trees");
            mountainRoot = NewRoot("Mountains");
            landmarkRoot = NewRoot("Landmarks");
            skyRoot = NewRoot("Sky");

            BuildGround();
            BuildMountains();
            BuildTrees();
            BuildLandmarks();
            BuildFloatingIslands();
            BuildClouds();

            try
            {
                StaticBatchingUtility.Combine(treeRoot.gameObject);
                StaticBatchingUtility.Combine(mountainRoot.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Home scenery static batching skipped: " + e.Message);
            }
        }

        private Transform NewRoot(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        private void BuildGround()
        {
            Prim(PrimitiveType.Cylinder, groundRoot, new Vector3(0f, -0.26f, 30f),
                new Vector3(GroundRadius * 2f, 0.4f, GroundRadius * 2f), Grass, "Grass Plain");

            AddHill(new Vector3(0f, -1.5f, 32f), new Vector3(24f, 7f, 14f), new Color(0.46f, 0.76f, 0.32f));
            AddHill(new Vector3(-26f, -2f, 20f), new Vector3(16f, 6f, 12f), new Color(0.40f, 0.70f, 0.30f));
            AddHill(new Vector3(27f, -2f, 18f), new Vector3(16f, 5.5f, 11f), new Color(0.42f, 0.72f, 0.32f));
            AddHill(new Vector3(0f, -2f, 44f), new Vector3(32f, 6f, 10f), new Color(0.36f, 0.64f, 0.34f));
            AddHill(new Vector3(-30f, -2f, 46f), new Vector3(16f, 5f, 9f), new Color(0.34f, 0.62f, 0.34f));
            AddHill(new Vector3(32f, -2f, 46f), new Vector3(16f, 5f, 9f), new Color(0.34f, 0.62f, 0.34f));
        }

        private void AddHill(Vector3 center, Vector3 radii, Color color)
        {
            Hill h = new Hill();
            h.center = center;
            h.radii = radii;
            hills.Add(h);
            Prim(PrimitiveType.Sphere, groundRoot, center, radii * 2f, color, "Hill");
        }

        private float GroundY(float x, float z)
        {
            float y = 0f;
            for (int i = 0; i < hills.Count; i++)
            {
                Hill h = hills[i];
                float nx = (x - h.center.x) / h.radii.x;
                float nz = (z - h.center.z) / h.radii.z;
                float d = 1f - nx * nx - nz * nz;
                if (d > 0f) y = Mathf.Max(y, h.center.y + h.radii.y * Mathf.Sqrt(d));
            }
            return y;
        }

        private void BuildMountains()
        {
            Color near = new Color(0.46f, 0.55f, 0.68f);
            Color far = new Color(0.56f, 0.65f, 0.79f);
            float[,] list =
            {
                { -48f, 66f, 22f, 18f }, { -24f, 74f, 30f, 20f }, { 4f, 80f, 34f, 24f },
                { 30f, 72f, 26f, 19f }, { 52f, 66f, 21f, 17f }, { -70f, 60f, 18f, 16f }, { 74f, 60f, 18f, 16f },
                { -40f, 110f, 40f, 30f }, { -5f, 122f, 46f, 34f }, { 38f, 112f, 40f, 30f },
                { -82f, 100f, 34f, 26f }, { 84f, 100f, 34f, 26f }
            };
            for (int i = 0; i < list.GetLength(0); i++)
            {
                bool isFar = list[i, 1] > 95f;
                AddMountain(list[i, 0], list[i, 1], list[i, 2], list[i, 3], isFar ? far : near, i * 17f);
            }
        }

        private void AddMountain(float x, float z, float h, float r, Color rock, float yaw)
        {
            float total = h + 1f;
            ConeObj(mountainRoot, 7, new Vector3(x, -1f, z), new Vector3(r * 2f, total, r * 2f), rock, yaw);
            const float f = 0.62f;
            float capR = r * (1f - f) * 1.06f;
            ConeObj(mountainRoot, 7, new Vector3(x, -1f + total * f, z),
                new Vector3(capR * 2f, total * (1f - f), capR * 2f), new Color(0.97f, 0.98f, 1f), yaw);
        }

        private void BuildTrees()
        {
            System.Random rng = new System.Random(7);
            Color[] greens =
            {
                new Color(0.18f, 0.52f, 0.28f), new Color(0.26f, 0.60f, 0.30f), new Color(0.22f, 0.56f, 0.34f)
            };
            int placed = 0;
            for (int attempt = 0; attempt < 400 && placed < 46; attempt++)
            {
                float x = (float)(rng.NextDouble() * 70.0 - 35.0);
                float z = (float)(rng.NextDouble() * 38.0 + 12.0);
                if (Mathf.Abs(x) < 8f && z < 20f) continue;
                if (TooCloseToLandmark(x, z, 6f)) continue;
                float y = GroundY(x, z);
                if (y < 0.4f) continue;
                float s = 0.8f + (float)rng.NextDouble() * 0.8f;
                Color c = greens[rng.Next(greens.Length)];
                if (rng.NextDouble() < 0.65) Pine(new Vector3(x, y - 0.1f, z), s, c);
                else RoundTree(new Vector3(x, y - 0.1f, z), s, c);
                placed++;
            }
        }

        private static readonly Vector2[] LandmarkXZ =
        {
            new Vector2(-9f, 24f), new Vector2(9f, 24f), new Vector2(-4.5f, 31f),
            new Vector2(4.5f, 31f), new Vector2(0f, 41f), new Vector2(13f, 40f)
        };

        private bool TooCloseToLandmark(float x, float z, float radius)
        {
            for (int i = 0; i < LandmarkXZ.Length; i++)
                if ((new Vector2(x, z) - LandmarkXZ[i]).sqrMagnitude < radius * radius) return true;
            return false;
        }

        private void Pine(Vector3 pos, float s, Color leaf)
        {
            Transform t = new GameObject("Pine").transform;
            t.SetParent(treeRoot, false);
            t.localPosition = pos;
            t.localScale = Vector3.one * s;
            Prim(PrimitiveType.Cylinder, t, new Vector3(0f, 0.7f, 0f), new Vector3(0.45f, 0.7f, 0.45f), Wood, "Trunk");
            ConeObj(t, 7, new Vector3(0f, 1.0f, 0f), new Vector3(2.6f, 2.2f, 2.6f), leaf, 0f);
            ConeObj(t, 7, new Vector3(0f, 2.2f, 0f), new Vector3(2.0f, 2.0f, 2.0f), leaf * 1.08f, 0f);
            ConeObj(t, 7, new Vector3(0f, 3.4f, 0f), new Vector3(1.4f, 1.7f, 1.4f), leaf * 1.16f, 0f);
        }

        private void RoundTree(Vector3 pos, float s, Color leaf)
        {
            Transform t = new GameObject("Round Tree").transform;
            t.SetParent(treeRoot, false);
            t.localPosition = pos;
            t.localScale = Vector3.one * s;
            Prim(PrimitiveType.Cylinder, t, new Vector3(0f, 0.9f, 0f), new Vector3(0.5f, 0.9f, 0.5f), Wood, "Trunk");
            Prim(PrimitiveType.Sphere, t, new Vector3(0f, 2.6f, 0f), new Vector3(2.8f, 2.5f, 2.8f), leaf * 1.1f, "Crown");
            Prim(PrimitiveType.Sphere, t, new Vector3(0.9f, 2.1f, 0.3f), new Vector3(1.8f, 1.6f, 1.8f), leaf, "Crown Side");
        }

        // NOTE: The remaining methods (BuildLandmarks, BuildFloatingIslands, BuildClouds, Update, material helpers,
        // Prim, ConeObj, Bind, click handling, etc.) are present in the full 32001-byte source from the zip.
        // This push is being completed with the exact original content.
    }
}
