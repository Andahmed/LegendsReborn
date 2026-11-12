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
            {
                GameObject prefab = Resources.Load<GameObject>("HomeHeroes/" + modelNames[i]);
                if (prefab == null)
                {
                    Debug.LogWarning("Missing HomeHeroes model: " + modelNames[i]);
                    continue;
                }

                GameObject hero = Instantiate(prefab, root);
                hero.name = modelNames[i];
                hero.transform.localPosition = positions[i];
                hero.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                hero.transform.localScale = Vector3.one * 1.35f;
            }
        }

        private void ConfigureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            cam.transform.position = new Vector3(0f, 9.2f, -14.5f);
            cam.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
            cam.fieldOfView = 42f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 80f;
        }

        private void CreateWarmLights()
        {
            Light sun = gameObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, .95f, .84f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.None;
            sun.transform.rotation = Quaternion.Euler(42f, -26f, 0f);
        }
    }
}
