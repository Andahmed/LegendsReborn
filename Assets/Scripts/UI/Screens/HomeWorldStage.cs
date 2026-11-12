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
        private bool lastLandscape;

        private void Awake()
        {
            root = new GameObject("Home World - Castle Mesh Stage").transform;
            root.SetParent(transform, false);
            PlaceCastle();
            PlaceNature();
            CreateSnowyBackdrop();
            ConfigureCamera();
            CreateWarmLights();
        }

        private void Update()
        {
            bool landscape = Screen.width > Screen.height;
            if (landscape != lastLandscape) ConfigureCamera();
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
                Debug.LogError("Original 3D castle prefab is missing. Run Sands > Setup MVP Home Scene in the Unity Editor.");
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

        private void CreateSnowyBackdrop()
        {
            // A distant alpine ridge gives the home city the bright mountain skyline of the approved reference.
            GameObject ridge = new GameObject("Distant Snow Mountain Ridge");
            ridge.transform.SetParent(root, false);
            ridge.transform.localPosition = new Vector3(0f, 0f, 20f);
            Mesh mesh = new Mesh();
            Vector3[] vertices =
            {
                new Vector3(-28f,0f,0f), new Vector3(-21f,8f,0f), new Vector3(-15f,1f,0f),
                new Vector3(-8f,11f,0f), new Vector3(-1f,1f,0f), new Vector3(7f,9f,0f),
                new Vector3(14f,1f,0f), new Vector3(22f,10f,0f), new Vector3(29f,0f,0f),
                new Vector3(-28f,0f,2f), new Vector3(-21f,8f,2f), new Vector3(-15f,1f,2f),
                new Vector3(-8f,11f,2f), new Vector3(-1f,1f,2f), new Vector3(7f,9f,2f),
                new Vector3(14f,1f,2f), new Vector3(22f,10f,2f), new Vector3(29f,0f,2f)
            };
            int[] triangles = { 0,1,9, 1,10,9, 1,2,10, 2,11,10, 2,3,11, 3,12,11, 3,4,12, 4,13,12, 4,5,13, 5,14,13, 5,6,14, 6,15,14, 6,7,15, 7,16,15, 7,8,16, 8,17,16 };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            MeshFilter filter = ridge.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = ridge.AddComponent<MeshRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material mountain = new Material(shader);
            mountain.color = new Color(0.48f, 0.67f, 0.78f);
            renderer.sharedMaterial = mountain;
        }

        private void PlaceNature()
        {
            PlaceNatureAsset("PalmTree_1", new Vector3(-8.2f, 0f, 3.4f), new Vector3(2.8f, 5.6f, 2.8f));
            PlaceNatureAsset("PalmTree_2", new Vector3(8.1f, 0f, 2.6f), new Vector3(2.5f, 5.1f, 2.5f));
            PlaceNatureAsset("Cactus_1", new Vector3(-7.2f, 0f, -3.4f), new Vector3(1.05f, 1.8f, 1.05f));
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

            bool landscape = Screen.width > Screen.height;
            lastLandscape = landscape;
            cam.transform.position = landscape ? new Vector3(0f, 6.2f, -14.2f) : new Vector3(0f, 8.5f, -20.5f);
            cam.transform.LookAt(landscape ? new Vector3(0f, 3.2f, 4.2f) : new Vector3(0f, 4.2f, 4.8f));
            cam.fieldOfView = landscape ? 40f : 48f;
            cam.nearClipPlane = .1f;
            cam.farClipPlane = 120f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.48f, .76f, .94f);

            RenderSettings.ambientLight = new Color(.72f, .78f, .82f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.62f, .79f, .88f);
            RenderSettings.fogDensity = .006f;

            Light[] lights = FindObjectsOfType<Light>();
            bool hasDirectional = false;
            for (int i = 0; i < lights.Length; i++)
                if (lights[i].type == LightType.Directional) hasDirectional = true;

            if (!hasDirectional)
            {
                GameObject sun = new GameObject("Home Directional Light");
                Light light = sun.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.25f;
                sun.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            }
        }
    }
}
