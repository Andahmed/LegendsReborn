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

        private void Awake()
        {
            root = new GameObject("Home World - Castle Mesh Stage").transform;
            root.SetParent(transform, false);
            PlaceCastle();
            PlaceNature();
            PlaceHero();
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
            instance.transform.localScale = new Vector3(
                size.x > .0001f ? targetSize.x / size.x : 1f,
                size.y > .0001f ? targetSize.y / size.y : 1f,
                size.z > .0001f ? targetSize.z / size.z : 1f);
        }

        private void PlaceHero()
        {
            GameObject prefab = Resources.Load<GameObject>("HomeHeroes/Warrior");
            if (prefab == null)
            {
                Debug.LogWarning("Hero FBX is not installed yet. Run Legends Reborn > Art > Install MVP Art.");
                return;
            }

            GameObject hero = Instantiate(prefab, root);
            hero.name = "Home Hero - Warrior";
            hero.transform.localPosition = new Vector3(0f, .72f, -2.15f);
            hero.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            Renderer[] renderers = hero.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            hero.transform.localScale = Vector3.one * (2.65f / Mathf.Max(.01f, bounds.size.y));
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

            cam.transform.position = new Vector3(0f, 6.1f, -14.2f);
            cam.transform.LookAt(new Vector3(0f, 3f, 3.3f));
            cam.fieldOfView = 40f;
            cam.nearClipPlane = .1f;
            cam.farClipPlane = 120f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.035f, .085f, .105f);

            RenderSettings.ambientLight = new Color(.48f, .49f, .46f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.18f, .24f, .24f);
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
