using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandsOfLegends.World
{
    /// <summary>
    /// Builds an isolated, mobile-conscious open-world visual prototype.
    /// This is deliberately separate from HomeWorldStage and does not change story or gameplay systems.
    /// </summary>
    public static class OpenWorldTerrainPrototype
    {
        private const int Resolution = 160;
        private const float WorldSize = 320f;
        private const float HalfSize = WorldSize * 0.5f;

        private static readonly Color Grass = new Color(0.31f, 0.55f, 0.27f);
        private static readonly Color Meadow = new Color(0.47f, 0.66f, 0.32f);
        private static readonly Color Rock = new Color(0.43f, 0.44f, 0.43f);
        private static readonly Color Snow = new Color(0.86f, 0.89f, 0.91f);
        private static readonly Color Sand = new Color(0.70f, 0.57f, 0.36f);

        public static void Build(Transform parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            Transform old = parent.Find("Open World - Prototype");
            if (old != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(old.gameObject);
                else UnityEngine.Object.DestroyImmediate(old.gameObject);
            }

            GameObject rootObject = new GameObject("Open World - Prototype");
            rootObject.transform.SetParent(parent, false);
            Transform root = rootObject.transform;

            BuildTerrain(root);
            BuildRoad(root);
            BuildRiver(root);
            ScatterNature(root);
            BuildLandmarks(root);
            ConfigureLightingAndCamera(root);
            BuildPlayer(root);
        }

        private static void BuildTerrain(Transform root)
        {
            int side = Resolution + 1;
            Vector3[] vertices = new Vector3[side * side];
            Vector2[] uv = new Vector2[vertices.Length];
            List<int>[] triangles = { new List<int>(), new List<int>(), new List<int>(), new List<int>() };

            for (int z = 0; z <= Resolution; z++)
            {
                for (int x = 0; x <= Resolution; x++)
                {
                    float wx = -HalfSize + WorldSize * x / Resolution;
                    float wz = -HalfSize + WorldSize * z / Resolution;
                    float h = HeightAt(wx, wz);
                    int index = z * side + x;
                    vertices[index] = new Vector3(wx, h, wz);
                    uv[index] = new Vector2(x / (float)Resolution * 16f, z / (float)Resolution * 16f);
                }
            }

            for (int z = 0; z < Resolution; z++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int a = z * side + x;
                    int b = a + 1;
                    int c = a + side;
                    int d = c + 1;
                    float cx = -HalfSize + WorldSize * (x + .5f) / Resolution;
                    float cz = -HalfSize + WorldSize * (z + .5f) / Resolution;
                    int biome = BiomeAt(cx, cz, HeightAt(cx, cz));
                    List<int> list = triangles[biome];
                    list.Add(a); list.Add(c); list.Add(b);
                    list.Add(b); list.Add(c); list.Add(d);
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "Open World Terrain Mesh";
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.subMeshCount = triangles.Length;
            for (int i = 0; i < triangles.Length; i++) mesh.SetTriangles(triangles[i], i, true);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject terrain = new GameObject("Sculpted Terrain - 320m");
            terrain.transform.SetParent(root, false);
            MeshFilter filter = terrain.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = terrain.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[]
            {
                MakeMaterial("Meadow", Meadow),
                MakeMaterial("Forest Grass", Grass),
                MakeMaterial("Mountain Stone", Rock),
                MakeMaterial("Snow", Snow)
            };
            terrain.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private static float HeightAt(float x, float z)
        {
            float broad = Mathf.PerlinNoise((x + 900f) * .012f, (z + 900f) * .012f);
            float detail = Mathf.PerlinNoise((x + 170f) * .045f, (z - 310f) * .045f);
            float ridgeNoise = Mathf.PerlinNoise((x - 400f) * .021f, (z + 120f) * .021f);

            float mountainDistance = Mathf.Min(
                Vector2.Distance(new Vector2(x, z), new Vector2(-112f, 108f)),
                Vector2.Distance(new Vector2(x, z), new Vector2(112f, 116f)));
            float mountain = Mathf.Clamp01(1f - mountainDistance / 105f);
            float height = (broad - .5f) * 9f + (detail - .5f) * 2.1f + ridgeNoise * 2.2f;
            height += mountain * mountain * (13f + detail * 9f);

            // A broad, gently rolling central valley keeps the hub and main route readable.
            float valley = Mathf.Clamp01(1f - Mathf.Abs(x) / 48f);
            height *= 1f - valley * .48f;
            return height;
        }

        private static int BiomeAt(float x, float z, float height)
        {
            if (height > 17.5f) return 3;
            if (height > 11.5f) return 2;
            if (Mathf.Abs(x) > 92f || z < -112f) return 0;
            return 1;
        }

        private static void BuildRoad(Transform root)
        {
            const int segments = 100;
            const float width = 5.8f;
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float z = -HalfSize + t * WorldSize;
                float x = Mathf.Sin(t * Mathf.PI * 2.1f) * 17f + Mathf.Sin(t * Mathf.PI * 5f) * 4f;
                Vector3 left = new Vector3(x - width * .5f, HeightAt(x - width * .5f, z) + .16f, z);
                Vector3 right = new Vector3(x + width * .5f, HeightAt(x + width * .5f, z) + .16f, z);
                vertices.Add(left); vertices.Add(right);
                if (i < segments)
                {
                    int a = i * 2;
                    indices.Add(a); indices.Add(a + 2); indices.Add(a + 1);
                    indices.Add(a + 1); indices.Add(a + 2); indices.Add(a + 3);
                }
            }
            CreateRibbon(root, "Kingdom Trade Road", vertices, indices, new Color(.48f, .36f, .24f));
        }

        private static void BuildRiver(Transform root)
        {
            const int segments = 100;
            const float width = 4.2f;
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float z = -HalfSize + t * WorldSize;
                float x = 76f + Mathf.Sin(t * Mathf.PI * 3f) * 13f;
                // Keep the water surface just above the terrain so it remains visible without z-fighting.
                float y = HeightAt(x, z) + .08f;
                vertices.Add(new Vector3(x - width * .5f, y, z));
                vertices.Add(new Vector3(x + width * .5f, y, z));
                if (i < segments)
                {
                    int a = i * 2;
                    // Match the terrain's upward-facing winding so the water is visible from above.
                    indices.Add(a); indices.Add(a + 2); indices.Add(a + 1);
                    indices.Add(a + 1); indices.Add(a + 2); indices.Add(a + 3);
                }
            }
            CreateRibbon(root, "Silver River", vertices, indices, new Color(.12f, .48f, .68f, .88f));
        }

        private static void CreateRibbon(Transform root, string name, List<Vector3> vertices, List<int> indices, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            Mesh mesh = new Mesh();
            mesh.name = name + " Mesh";
            mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateNormals();
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            Material material = MakeMaterial(name + " Material", color);
            if (color.a < .999f) ConfigureTransparentMaterial(material);
            renderer.sharedMaterial = material;
        }

        private static void ScatterNature(Transform root)
        {
            GameObject treeA = Resources.Load<GameObject>("HomeNature/PalmTree_1");
            GameObject treeB = Resources.Load<GameObject>("HomeNature/PalmTree_2");
            GameObject bush = Resources.Load<GameObject>("HomeNature/Bush_1");
            GameObject rock = Resources.Load<GameObject>("HomeNature/Rock_1");
            System.Random random = new System.Random(7419);

            for (int i = 0; i < 145; i++)
            {
                float x = (float)(random.NextDouble() * 270.0 - 135.0);
                float z = (float)(random.NextDouble() * 270.0 - 135.0);
                float roadX = Mathf.Sin(((z + HalfSize) / WorldSize) * Mathf.PI * 2.1f) * 17f;
                if (Mathf.Abs(x - roadX) < 10f || Mathf.Abs(x - 76f) < 9f) continue;
                float h = HeightAt(x, z);
                if (h > 17f) continue;

                GameObject prefab = i % 5 == 0 ? bush : (i % 3 == 0 ? treeB : treeA);
                if (prefab != null)
                {
                    GameObject instance = UnityEngine.Object.Instantiate(prefab, root);
                    instance.name = "World Nature - " + i;
                    instance.transform.localPosition = new Vector3(x, h, z);
                    float scale = .75f + (float)random.NextDouble() * 1.05f;
                    instance.transform.localScale = Vector3.one * scale;
                    instance.transform.localRotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
                }
                else if (i % 5 != 0)
                {
                    CreateFallbackTree(root, new Vector3(x, h, z), .8f + (float)random.NextDouble() * .7f);
                }

                if (rock != null && i % 9 == 0)
                {
                    GameObject stone = UnityEngine.Object.Instantiate(rock, root);
                    stone.name = "World Rock";
                    stone.transform.localPosition = new Vector3(x + 3f, h, z + 2f);
                    stone.transform.localScale = Vector3.one * (.7f + (float)random.NextDouble() * .8f);
                }
            }
        }

        private static void CreateFallbackTree(Transform root, Vector3 position, float scale)
        {
            GameObject tree = new GameObject("Fallback Pine");
            tree.transform.SetParent(root, false);
            tree.transform.localPosition = position;
            tree.transform.localScale = Vector3.one * scale;
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 1f, 0f);
            trunk.transform.localScale = new Vector3(.24f, 1f, .24f);
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            crown.transform.SetParent(tree.transform, false);
            crown.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            crown.transform.localScale = new Vector3(1.5f, 1.8f, 1.5f);
            crown.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Pine Green", new Color(.16f, .39f, .22f));
        }

        private static void BuildLandmarks(Transform root)
        {
            PlaceLandmark(root, "Old Kingdom Outpost", new Vector3(-48f, 0f, 34f), new Color(.75f, .59f, .36f));
            PlaceLandmark(root, "Forgotten Watchtower", new Vector3(44f, 0f, 92f), new Color(.48f, .49f, .55f));
            PlaceLandmark(root, "Ancient Ruins", new Vector3(-82f, 0f, -62f), new Color(.57f, .51f, .39f));
            PlaceLandmark(root, "Northern Gate", new Vector3(18f, 0f, 137f), new Color(.72f, .68f, .57f));
            PlaceLandmark(root, "World Boss Clearing", new Vector3(-96f, 0f, -126f), new Color(.42f, .31f, .30f));
        }

        private static void PlaceLandmark(Transform root, string name, Vector3 position, Color color)
        {
            position.y = HeightAt(position.x, position.z);
            GameObject group = new GameObject(name);
            group.transform.SetParent(root, false);
            group.transform.localPosition = position;
            GameObject baseStone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseStone.name = "Landmark Foundation";
            baseStone.transform.SetParent(group.transform, false);
            baseStone.transform.localPosition = new Vector3(0f, .45f, 0f);
            baseStone.transform.localScale = new Vector3(8f, .45f, 8f);
            baseStone.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + " Stone", color);
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = "Landmark Tower";
            tower.transform.SetParent(group.transform, false);
            tower.transform.localPosition = new Vector3(0f, 3.2f, 0f);
            tower.transform.localScale = new Vector3(2.5f, 6f, 2.5f);
            tower.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + " Tower", color * .82f);

            SphereCollider proximity = group.AddComponent<SphereCollider>();
            proximity.isTrigger = true;
            proximity.center = new Vector3(0f, 2f, 0f);
            proximity.radius = 9f;
            group.AddComponent<OpenWorldLandmark>().Initialize(name);
        }

        private static void BuildPlayer(Transform root)
        {
            float spawnZ = -126f;
            GameObject player = new GameObject("Player - Exploration");
            player.transform.SetParent(root, false);
            player.transform.position = new Vector3(0f, HeightAt(0f, spawnZ) + .2f, spawnZ);
            player.AddComponent<CharacterController>();
            OpenWorldPlayerController controller = player.AddComponent<OpenWorldPlayerController>();

            GameObject heroPrefab = Resources.Load<GameObject>("HomeHeroes/Warrior");
            if (heroPrefab != null)
            {
                GameObject hero = UnityEngine.Object.Instantiate(heroPrefab, player.transform);
                hero.name = "Warrior - Player Model";
                hero.transform.localPosition = Vector3.zero;
                hero.transform.localRotation = Quaternion.identity;
                Renderer[] renderers = hero.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                    float scale = 2.25f / Mathf.Max(.01f, bounds.size.y);
                    hero.transform.localScale = Vector3.one * scale;
                    hero.transform.localPosition = Vector3.up * (-bounds.min.y + player.transform.position.y) * scale;
                }
            }
            else
            {
                GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallback.name = "Temporary Player Visual";
                fallback.transform.SetParent(player.transform, false);
                fallback.transform.localPosition = Vector3.up;
                fallback.transform.localScale = new Vector3(.72f, 1f, .72f);
                Collider visualCollider = fallback.GetComponent<Collider>();
                if (visualCollider != null) UnityEngine.Object.Destroy(visualCollider);
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Open World Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }
            controller.SetCamera(camera);
        }

        private static void ConfigureLightingAndCamera(Transform root)
        {
            GameObject sunObject = new GameObject("Open World Sun");
            sunObject.transform.SetParent(root, false);
            sunObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, .93f, .82f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;

            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Open World Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }
            camera.transform.position = new Vector3(0f, 104f, -142f);
            camera.transform.LookAt(new Vector3(0f, 4f, 20f));
            camera.fieldOfView = 55f;
            camera.nearClipPlane = .3f;
            camera.farClipPlane = 500f;
            camera.clearFlags = CameraClearFlags.Skybox;
            RenderSettings.ambientLight = new Color(.55f, .59f, .66f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.68f, .78f, .86f);
            RenderSettings.fogDensity = .0028f;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            // Support both the Built-in renderer and URP projects.
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null)
                throw new InvalidOperationException("No compatible Unity shader was found. Check the project's render pipeline and included shaders.");

            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            return material;
        }

        private static void ConfigureTransparentMaterial(Material material)
        {
            if (material == null) return;

            // URP Lit transparency.
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            // Built-in Standard transparency.
            if (material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            material.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}