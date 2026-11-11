// Home hub castle assembled exclusively from bundled Quaternius CC0 FBX meshes.
// No procedural block/cube castle or primitive-geometry fallback is used.
using UnityEngine;

namespace SandsOfLegends.UI
{
    public sealed class HomeWorldStage : MonoBehaviour
    {
        private Transform root;

        private void Awake()
        {
            root = new GameObject("Home World - FBX Castle Stage").transform;
            root.SetParent(transform, false);
            BuildCourtyard();
            BuildCastle();
            BuildNature();
            PlaceHero();
            ConfigureCamera();
            CreateWarmLights();
        }

        private void BuildCourtyard()
        {
            // Nine imported floor modules form the courtyard; each remains a real FBX mesh.
            for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
                PlaceAsset("Floor_Modular",
                    new Vector3(x * 5.15f, -.08f, z * 5.15f + 1.5f),
                    new Vector3(5.25f, .28f, 5.25f), Quaternion.identity,
                    "Courtyard Floor " + x + " " + z);

            PlaceAsset("Stairs_Modular", new Vector3(0f, .02f, -1.15f),
                new Vector3(4.4f, .9f, 2.8f), Quaternion.identity, "Castle Approach Stairs");
        }

        private void BuildCastle()
        {
            PlaceAsset("Arch_Door", new Vector3(0f, .02f, 6.15f),
                new Vector3(4.5f, 5.7f, 1.5f), Quaternion.identity, "Main Gate");
            PlaceAsset("Wall_Modular", new Vector3(-4.75f, 1.9f, 6.15f),
                new Vector3(5.2f, 4.1f, 1.35f), Quaternion.identity, "Left Curtain Wall");
            PlaceAsset("Wall_Modular", new Vector3(4.75f, 1.9f, 6.15f),
                new Vector3(5.2f, 4.1f, 1.35f), Quaternion.identity, "Right Curtain Wall");
            PlaceAsset("Decorative_Wall", new Vector3(-4.75f, 4.1f, 6.12f),
                new Vector3(5.2f, .85f, 1.45f), Quaternion.identity, "Left Battlements");
            PlaceAsset("Decorative_Wall", new Vector3(4.75f, 4.1f, 6.12f),
                new Vector3(5.2f, .85f, 1.45f), Quaternion.identity, "Right Battlements");

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 6.8f;
                PlaceAsset("Column2", new Vector3(x, 2.9f, 6.0f),
                    new Vector3(2.5f, 6.2f, 2.5f), Quaternion.identity,
                    side < 0 ? "West Tower" : "East Tower");
                PlaceAsset("Column", new Vector3(x, 6.0f, 6.0f),
                    new Vector3(2.9f, .95f, 2.9f), Quaternion.identity,
                    side < 0 ? "West Tower Crown" : "East Tower Crown");
                PlaceAsset("Banner", new Vector3(x - side * .78f, 5.2f, 4.75f),
                    new Vector3(1.35f, 2.55f, .28f),
                    Quaternion.Euler(0f, side < 0 ? 12f : -12f, 0f),
                    side < 0 ? "West Tower Banner" : "East Tower Banner");
                PlaceAsset("Banner_wall", new Vector3(x - side * .8f, 5.15f, 4.78f),
                    new Vector3(1.45f, 2.7f, .3f), Quaternion.identity,
                    side < 0 ? "West Banner Mount" : "East Banner Mount");
            }

            PlaceAsset("Arch", new Vector3(0f, 3.65f, 8.0f),
                new Vector3(4.1f, 3.8f, 1.65f), Quaternion.identity, "Inner Keep Arch");

            for (int i = 0; i < 3; i++)
            {
                float x = -3.35f + i * 3.35f;
                PlaceAsset("Decorative_Wall", new Vector3(x, 4.75f, 6.0f),
                    new Vector3(1.3f, .8f, 1.35f), Quaternion.identity,
                    "Upper Wall Detail " + i);
            }

            for (int side = -1; side <= 1; side += 2)
                PlaceAsset("Torch", new Vector3(side * 3.05f, .08f, 1.55f),
                    new Vector3(.5f, 1.8f, .5f), Quaternion.identity,
                    side < 0 ? "Left Gate Torch" : "Right Gate Torch");
        }

        private void BuildNature()
        {
            PlaceNatureAsset("PalmTree_1", new Vector3(-8.5f, 0f, 2.6f), new Vector3(2.8f, 5.6f, 2.8f));
            PlaceNatureAsset("PalmTree_2", new Vector3(8.4f, 0f, 2.3f), new Vector3(2.5f, 5.1f, 2.5f));
            PlaceNatureAsset("Cactus_1", new Vector3(-7.2f, 0f, -4.1f), new Vector3(1.05f, 1.8f, 1.05f));
            PlaceNatureAsset("Bush_1", new Vector3(6.7f, 0f, -4.0f), new Vector3(1.65f, 1.1f, 1.65f));
            PlaceNatureAsset("Rock_1", new Vector3(-5.8f, 0f, 5.1f), new Vector3(1.4f, .8f, 1.25f));
            PlaceNatureAsset("Grass", new Vector3(5.8f, 0f, 5.0f), new Vector3(1.5f, .8f, 1.5f));
        }

        private void PlaceNatureAsset(string assetName, Vector3 position, Vector3 targetSize)
        {
            PlaceAsset(assetName, position, targetSize, Quaternion.identity, assetName, "HomeNature/");
        }

        private void PlaceHero()
        {
            PlaceAsset("Warrior", new Vector3(0f, .05f, -2.35f),
                new Vector3(1.45f, 2.7f, 1.1f), Quaternion.Euler(0f, 180f, 0f),
                "Home Hero - Warrior", "HomeHeroes/");
        }

        private bool PlaceAsset(string assetName, Vector3 position, Vector3 targetSize,
            Quaternion rotation, string instanceName, string resourceFolder = "HomeCastle/")
        {
            GameObject prefab = Resources.Load<GameObject>(resourceFolder + assetName);
            if (prefab == null)
            {
                Debug.LogError("Required CC0 FBX model is missing: Resources/" +
                    resourceFolder + assetName + ". Run the matching Legends Reborn art installer.");
                return false;
            }

            GameObject instance = Instantiate(prefab, root);
            instance.name = instanceName;
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = Vector3.one;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogError("Imported model has no Renderer: " + resourceFolder + assetName);
                Destroy(instance);
                return false;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            Vector3 sourceSize = bounds.size;
            Vector3 scale = new Vector3(
                sourceSize.x > .0001f ? targetSize.x / sourceSize.x : 1f,
                sourceSize.y > .0001f ? targetSize.y / sourceSize.y : 1f,
                sourceSize.z > .0001f ? targetSize.z / sourceSize.z : 1f);
            instance.transform.localScale = scale;

            // Correct FBX pivots so each imported mesh rests on its intended placement height.
            bounds = CalculateBounds(instance);
            instance.transform.localPosition += Vector3.up * (position.y - bounds.min.y);
            return true;
        }

        private static Bounds CalculateBounds(GameObject instance)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private void CreateWarmLights()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject go = new GameObject("Castle Warm Point Light");
                go.transform.SetParent(root, false);
                go.transform.localPosition = new Vector3(side * 3.1f, 2.1f, 1.4f);
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

            cam.transform.position = new Vector3(0f, 6.2f, -14.5f);
            cam.transform.LookAt(new Vector3(0f, 3.0f, 4.0f));
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