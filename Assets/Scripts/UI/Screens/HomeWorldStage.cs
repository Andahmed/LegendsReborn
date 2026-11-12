using System.Collections.Generic;
using UnityEngine;

namespace SandsOfLegends.UI
{
    /// <summary>
    /// Loads the original generated castle mesh and Quaternius CC0 nature and hero models.
    /// The castle is a real multi-part Mesh prefab, not Unity primitive cubes.
    /// </summary>
    public sealed class HomeWorldStage : MonoBehaviour
    {
        private Transform root;
        private float castleDaisTop = 0.91f;

        private void Awake()
        {
            root = new GameObject("Home World - Castle Mesh Stage").transform;
            root.SetParent(transform, false);
            HomeWorldScenery.Create(root);
            PlaceCastle();
            PlaceNature();
            PlaceHeroes();
            ConfigureCamera();
            CreateWarmLights();
        }

        private void OnEnable()
        {
            if (root != null) ConfigureCamera();
        }

        private void PlaceCastle()
        {
            GameObject prefab = Resources.Load<GameObject>("HomeCastle/LegendsRebornCastle");
            if (prefab == null)
            {
                Debug.LogWarning("Generated castle prefab is missing; building the low-poly fallback castle from runtime meshes.");
                BuildFallbackCastle();
                return;
            }

            GameObject castle = Instantiate(prefab, root);
            castle.name = "Original Low-Poly Castle Mesh";
            castle.transform.localRotation = Quaternion.identity;
            castle.transform.localScale = Vector3.one;

            Renderer[] renderers = castle.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogError("Generated castle has no MeshRenderer components.");
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            float fit = Mathf.Min(
                15.5f / Mathf.Max(.01f, bounds.size.x),
                9.6f / Mathf.Max(.01f, bounds.size.y),
                17f / Mathf.Max(.01f, bounds.size.z));

            castle.transform.localScale = Vector3.one * fit;
            castle.transform.localPosition = new Vector3(
                -bounds.center.x * fit,
                -bounds.min.y * fit,
                0f);

            // The dais top is y=0.7075 in the generated mesh; account for the castle's ground offset.
            castleDaisTop = (0.7075f - bounds.min.y) * fit;
        }

        // Fallback keeps the Home hub readable even when the optional generated castle prefab
        // has not been exported into Resources yet. All parts are lightweight, unlit-shadow primitives.
        private void BuildFallbackCastle()
        {
            Transform castle = new GameObject("Fallback Blue-Roof Castle").transform;
            castle.SetParent(root, false);
            castleDaisTop = 1.05f;

            Color stone = new Color(.78f, .75f, .68f);
            Color lightStone = new Color(.91f, .86f, .73f);
            Color blue = new Color(.10f, .34f, .70f);
            Color blueLight = new Color(.20f, .53f, .88f);
            Color gold = new Color(.94f, .69f, .22f);
            Color wine = new Color(.40f, .055f, .12f);
            Color green = new Color(.16f, .42f, .20f);

            CastlePart(castle, PrimitiveType.Cube, new Vector3(0f, .42f, 0f), new Vector3(18f, .8f, 11f), stone, "Castle Foundation");
            CastlePart(castle, PrimitiveType.Cube, new Vector3(0f, .9f, 0f), new Vector3(15.8f, .22f, 9.2f), gold, "Foundation Gold Inlay");
            CastlePart(castle, PrimitiveType.Cube, new Vector3(0f, 4.1f, .4f), new Vector3(7.4f, 6.2f, 6.6f), lightStone, "Central Keep");
            CastlePart(castle, PrimitiveType.Cube, new Vector3(0f, 1.55f, -3.02f), new Vector3(2.0f, 2.7f, .16f), new Color(.25f,.14f,.10f), "Main Gate");
            CastlePart(castle, PrimitiveType.Cube, new Vector3(0f, 2.95f, -3.12f), new Vector3(2.5f, .22f, .2f), gold, "Gate Header");

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 6.0f;
                CastlePart(castle, PrimitiveType.Cube, new Vector3(x, 3.25f, .4f), new Vector3(5.0f, 4.5f, 6.0f), stone, "Side Keep");
                CastlePart(castle, PrimitiveType.Cube, new Vector3(x, 5.55f, .4f), new Vector3(5.25f, .24f, 6.25f), gold, "Side Keep Cornice");
            }

            CastleTower(castle, new Vector3(-6.0f, 0f, -3.2f), 2.15f, 4.6f, blue, gold);
            CastleTower(castle, new Vector3(6.0f, 0f, -3.2f), 2.15f, 4.6f, blue, gold);
            CastleTower(castle, new Vector3(-6.0f, 0f, 4.0f), 2.0f, 4.0f, blueLight, gold);
            CastleTower(castle, new Vector3(6.0f, 0f, 4.0f), 2.0f, 4.0f, blueLight, gold);
            CastleTower(castle, new Vector3(0f, 0f, 2.8f), 2.6f, 7.6f, blue, gold);

            for (int side = -1; side <= 1; side += 2)
            {
                CastlePart(castle, PrimitiveType.Cylinder, new Vector3(side * 3.0f, 5.3f, -3.45f), new Vector3(.13f, 2.0f, .13f), gold, "Banner Pole");
                CastlePart(castle, PrimitiveType.Cube, new Vector3(side * 3.0f, 5.0f, -3.45f), new Vector3(1.25f, 1.65f, .10f), wine, "Royal Wine Banner");
                CastlePart(castle, PrimitiveType.Cube, new Vector3(side * 3.0f, 5.0f, -3.52f), new Vector3(.22f, .95f, .05f), gold, "Banner Emblem");
            }

            // A small emerald guardian statue and a blue crystal echo the reference artwork.
            CastlePart(castle, PrimitiveType.Sphere, new Vector3(4.25f, 1.55f, -5.0f), new Vector3(1.15f, 1.05f, 1.1f), green, "Dragon Guardian Head");
            CastlePart(castle, PrimitiveType.Sphere, new Vector3(4.25f, 1.25f, -5.55f), new Vector3(.9f, .48f, .72f), green, "Dragon Guardian Snout");
            CastleCone(castle, new Vector3(3.75f, 2.15f, -4.95f), new Vector3(.38f, .8f, .38f), gold, "Guardian Horn L");
            CastleCone(castle, new Vector3(4.75f, 2.15f, -4.95f), new Vector3(.38f, .8f, .38f), gold, "Guardian Horn R");
            CastlePart(castle, PrimitiveType.Cylinder, new Vector3(-4.1f, 1.35f, -5.0f), new Vector3(1.2f, .22f, 1.2f), gold, "Crystal Pedestal");
            CastlePart(castle, PrimitiveType.Cube, new Vector3(-4.1f, 2.15f, -5.0f), new Vector3(.78f, 1.2f, .78f), new Color(.15f,.66f,.96f), "Blue Crystal");
        }

        private void CastleTower(Transform parent, Vector3 basePosition, float diameter, float height, Color roof, Color trim)
        {
            CastlePart(parent, PrimitiveType.Cylinder,
                basePosition + Vector3.up * (height * .5f),
                new Vector3(diameter, height, diameter), new Color(.84f,.80f,.72f), "Round Castle Tower");
            CastlePart(parent, PrimitiveType.Cylinder,
                basePosition + Vector3.up * (height - .12f),
                new Vector3(diameter * 1.12f, .18f, diameter * 1.12f), trim, "Tower Gold Band");
            CastleCone(parent,
                basePosition + Vector3.up * (height + .95f),
                new Vector3(diameter * 1.42f, 2.0f, diameter * 1.42f), roof, "Blue Tower Roof");
            CastlePart(parent, PrimitiveType.Sphere,
                basePosition + Vector3.up * (height + 2.0f),
                new Vector3(.28f,.34f,.28f), trim, "Roof Finial");
        }

        private void CastleCone(Transform parent, Vector3 position, Vector3 scale, Color color, string partName)
        {
            const int sides = 8;
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            Vector3 apex = new Vector3(0f, .5f, 0f);
            for (int i = 0; i < sides; i++)
            {
                float a0 = 2f * Mathf.PI * i / sides;
                float a1 = 2f * Mathf.PI * (i + 1) / sides;
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * .5f, -.5f, Mathf.Sin(a0) * .5f);
                Vector3 p1 = new Vector3(Mathf.Cos(a1) * .5f, -.5f, Mathf.Sin(a1) * .5f);
                int start = vertices.Count;
                vertices.Add(apex); vertices.Add(p1); vertices.Add(p0);
                triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
                int baseStart = vertices.Count;
                vertices.Add(new Vector3(0f, -.5f, 0f)); vertices.Add(p0); vertices.Add(p1);
                triangles.Add(baseStart); triangles.Add(baseStart + 1); triangles.Add(baseStart + 2);
            }
            Mesh mesh = new Mesh();
            mesh.name = "Castle Low Poly Cone";
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject part = new GameObject(partName);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = color;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void CastlePart(Transform parent, PrimitiveType type, Vector3 position, Vector3 scale, Color color, string partName)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Renderer renderer = part.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", color == new Color(.94f,.69f,.22f) ? .35f : .02f);
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void PlaceNature()
        {
            PlaceNatureAsset("PalmTree_1", new Vector3(-8.2f, 0f, 3.4f), new Vector3(2.8f, 5.6f, 2.8f));
            PlaceNatureAsset("PalmTree_2", new Vector3(8.1f, 0f, 2.6f), new Vector3(2.5f, 5.1f, 2.5f));
            PlaceNatureAsset("Bush_1", new Vector3(6.5f, 0f, -3.5f), new Vector3(1.65f, 1.1f, 1.65f));
            PlaceNatureAsset("Rock_1", new Vector3(-5.7f, 0f, 4.4f), new Vector3(1.4f, .8f, 1.25f));
            PlaceNatureAsset("Grass", new Vector3(5.7f, 0f, 4.3f), new Vector3(1.5f, .8f, 1.5f));
        }

        private void PlaceNatureAsset(string assetName, Vector3 position, Vector3 targetSize)
        {
            GameObject prefab = Resources.Load<GameObject>("HomeNature/" + assetName);
            if (prefab == null)
            {
                Debug.LogWarning("Missing HomeNature model: " + assetName);
                return;
            }

            GameObject instance = Instantiate(prefab, root);
            instance.name = assetName;
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            Vector3 size = bounds.size;
            float scaleY = size.y > .0001f ? targetSize.y / size.y : 1f;
            instance.transform.localScale = new Vector3(
                size.x > .0001f ? targetSize.x / size.x : 1f,
                scaleY,
                size.z > .0001f ? targetSize.z / size.z : 1f);
            // Align the bottom of the imported mesh with the courtyard instead of assuming its pivot is at the feet.
            instance.transform.localPosition = position + Vector3.up * (-bounds.min.y * scaleY);
        }

        private void PlaceHeroes()
        {
            string[] modelNames = { "Cleric", "Monk", "Ranger", "Rogue", "Warrior", "Wizard" };
            Vector3[] positions =
            {
                new Vector3(-5.25f, 0f, -1.3f),
                new Vector3(-3.15f, 0f, -1.85f),
                new Vector3(-1.05f, 0f, -2.15f),
                new Vector3(1.05f, 0f, -2.15f),
                new Vector3(3.15f, 0f, -1.85f),
                new Vector3(5.25f, 0f, -1.3f)
            };

            for (int i = 0; i < modelNames.Length; i++)
                PlaceHero(modelNames[i], positions[i]);
        }

        private void PlaceHero(string modelName, Vector3 position)
        {
            GameObject prefab = Resources.Load<GameObject>("HomeHeroes/" + modelName);
            if (prefab == null)
            {
                Debug.LogWarning("Missing HomeHeroes model: " + modelName + ". Merge the A1 asset PR or run the art installer.");
                return;
            }

            GameObject hero = Instantiate(prefab, root);
            hero.name = "Home Hero - " + modelName;
            hero.transform.localPosition = position;
            hero.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            Renderer[] renderers = hero.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            float scale = 2.65f / Mathf.Max(.01f, bounds.size.y);
            hero.transform.localScale = Vector3.one * scale;
            // The generated dais top sits just below one world unit after the castle fit.
            hero.transform.localPosition = new Vector3(
                position.x,
                castleDaisTop - bounds.min.y * scale,
                position.z);
        }

        private void CreateWarmLights()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject go = new GameObject("Castle Warm Point Light");
                go.transform.SetParent(root, false);
                go.transform.localPosition = new Vector3(side * 3.1f, 2.1f, .9f);
                Light light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, .63f, .31f);
                light.intensity = 1.35f;
                light.range = 7f;
            }
        }

        private void ConfigureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject go = new GameObject("Home World Camera", typeof(Camera));
                cam = go.GetComponent<Camera>();
                go.tag = "MainCamera";
            }

            // Pulled back and tilted slightly down so the castle, hills, mountains and sky all fit a portrait screen.
            cam.transform.position = new Vector3(0f, 15f, -34f);
            cam.transform.LookAt(new Vector3(0f, 5.5f, 6f));
            cam.fieldOfView = 50f;
            cam.nearClipPlane = .3f;
            cam.farClipPlane = 260f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            Color sky = new Color(.55f, .78f, .96f);
            cam.backgroundColor = sky;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.60f, .66f, .76f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = sky;
            RenderSettings.fogDensity = .0075f;

            Light sun = null;
            Light[] lights = FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; i++)
                if (lights[i].type == LightType.Directional) { sun = lights[i]; break; }

            if (sun == null)
            {
                GameObject sunObject = new GameObject("Home Directional Light");
                sun = sunObject.AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            sun.color = new Color(1f, .95f, .84f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.None;
            sun.transform.rotation = Quaternion.Euler(42f, -26f, 0f);
        }
    }
}
