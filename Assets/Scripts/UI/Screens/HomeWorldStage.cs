// Home hub environment assembled from Quaternius Modular Dungeon CC0 FBX assets.
// Run: Legends Reborn > Art > Install Home Castle CC0 Models.
using UnityEngine;

namespace SandsOfLegends.UI
{
    public sealed class HomeWorldStage : MonoBehaviour
    {
        private Transform root;
        private Material stone, shadow, sand, gold, teal, red, steel, skin;

        private void Awake()
        {
            stone = Mat(new Color(.62f, .53f, .39f));
            shadow = Mat(new Color(.17f, .23f, .24f));
            sand = Mat(new Color(.72f, .56f, .34f));
            gold = Mat(new Color(.92f, .68f, .25f), .25f);
            teal = Mat(new Color(.035f, .39f, .38f));
            red = Mat(new Color(.48f, .12f, .13f));
            steel = Mat(new Color(.30f, .40f, .43f), .45f);
            skin = Mat(new Color(.72f, .49f, .34f));

            root = new GameObject("Home Castle Environment").transform;
            root.SetParent(transform, false);
            Ground();
            Castle();
            Hero();
            Torches();
            CameraSetup();
        }

        private void OnEnable()
        {
            if (root != null) CameraSetup();
        }

        private Material Mat(Color c, float metallic = 0f)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            Material m = new Material(shader);
            m.color = c;
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", .28f);
            return m;
        }

        private void Part(PrimitiveType type, string name, Vector3 p, Vector3 s, Material m, Vector3 rotation = default(Vector3))
        {
            GameObject o = GameObject.CreatePrimitive(type);
            o.name = name;
            o.transform.SetParent(root, false);
            o.transform.localPosition = p;
            o.transform.localScale = s;
            o.transform.localEulerAngles = rotation;
            Renderer r = o.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = m;
            Collider c = o.GetComponent<Collider>();
            if (c != null) Destroy(c);
        }

        private void PlaceAsset(string assetName, Vector3 position, Vector3 targetSize, Material material, Vector3 rotation = default(Vector3))
        {
            GameObject prefab = Resources.Load<GameObject>("HomeCastle/" + assetName);
            if (prefab == null)
            {
                Debug.LogWarning("Missing HomeCastle model: " + assetName + ". Run Legends Reborn > Art > Install MVP Art.");
                return;
            }

            GameObject instance = Instantiate(prefab, root);
            instance.name = assetName;
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(rotation);
            instance.transform.localScale = Vector3.one;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            Vector3 size = bounds.size;
            Vector3 scale = new Vector3(
                size.x > .0001f ? targetSize.x / size.x : 1f,
                size.y > .0001f ? targetSize.y / size.y : 1f,
                size.z > .0001f ? targetSize.z / size.z : 1f);
            instance.transform.localScale = scale;

            foreach (Renderer renderer in renderers)
            {
                Material[] mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = material;
                renderer.sharedMaterials = mats;
            }
        }

        private void Ground()
        {
            Part(PrimitiveType.Plane, "Sandstone Courtyard", new Vector3(0, -.035f, 0), new Vector3(1.8f, 1f, 2.2f), sand);
            Part(PrimitiveType.Cylinder, "Hero Dais", new Vector3(0, .02f, -2.2f), new Vector3(3, .18f, 3), gold);
            Part(PrimitiveType.Cylinder, "Dais Top", new Vector3(0, .14f, -2.2f), new Vector3(2.72f, .12f, 2.72f), shadow);
            PlaceAsset("Floor_Modular", new Vector3(0, -.015f, 1.6f), new Vector3(13f, .16f, 7f), stone);
            for (int i = 0; i < 6; i++)
                Part(PrimitiveType.Cylinder, "Courtyard Inlay", new Vector3(-6 + i * 2.4f, .005f, 1.4f), new Vector3(.82f, .012f, .045f), gold);
        }

        private void Castle()
        {
            // Main gate and flanking modular stone walls.
            PlaceAsset("Arch_Door", new Vector3(0, 0, 5.35f), new Vector3(4.2f, 5.4f, 1.25f), gold);
            PlaceAsset("Wall_Modular", new Vector3(-4.65f, 2.0f, 5.45f), new Vector3(5.2f, 4.1f, 1.1f), stone);
            PlaceAsset("Wall_Modular", new Vector3(4.65f, 2.0f, 5.45f), new Vector3(5.2f, 4.1f, 1.1f), stone);
            PlaceAsset("Decorative_Wall", new Vector3(-4.65f, 4.2f, 5.42f), new Vector3(5.2f, .65f, 1.2f), gold);
            PlaceAsset("Decorative_Wall", new Vector3(4.65f, 4.2f, 5.42f), new Vector3(5.2f, .65f, 1.2f), gold);

            // Tall corner keeps made from real modular column meshes.
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 6.7f;
                PlaceAsset("Column2", new Vector3(x, 3.1f, 5.15f), new Vector3(2.5f, 6.6f, 2.5f), shadow);
                PlaceAsset("Column", new Vector3(x, 6.5f, 5.15f), new Vector3(2.9f, .9f, 2.9f), gold);
                PlaceAsset("Banner", new Vector3(x - side * .8f, 5.5f, 3.95f), new Vector3(1.3f, 2.5f, .2f), side < 0 ? teal : red);
                PlaceAsset("Banner_wall", new Vector3(x - side * .8f, 5.45f, 4.0f), new Vector3(1.4f, 2.7f, .25f), gold);
            }

            // Raised central keep silhouette behind the gate.
            PlaceAsset("Arch", new Vector3(0, 4.6f, 6.25f), new Vector3(3.8f, 3.4f, 1.5f), teal);
            PlaceAsset("Stairs_Modular", new Vector3(0, .12f, 2.65f), new Vector3(4.2f, .75f, 2.2f), stone);
            for (int i = 0; i < 3; i++)
            {
                float x = -3.4f + i * 3.4f;
                PlaceAsset("Decorative_Wall", new Vector3(x, 4.75f, 5.25f), new Vector3(1.25f, .65f, 1.2f), gold);
            }
        }

        private void Hero()
        {
            GameObject prefab = Resources.Load<GameObject>("HomeHeroes/Warrior");
            if (prefab == null)
            {
                Debug.LogWarning("Hero FBX is not installed yet. Run Legends Reborn > Art > Install MVP Art.");
                return;
            }

            GameObject hero = Instantiate(prefab, root);
            hero.name = "Home Hero - Warrior FBX";
            hero.transform.localPosition = new Vector3(0, 0, -2.2f);
            hero.transform.localRotation = Quaternion.Euler(0, 180, 0);
            Renderer[] renderers = hero.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                float height = Mathf.Max(.01f, bounds.size.y);
                hero.transform.localScale = Vector3.one * (2.7f / height);
                hero.transform.position += Vector3.up * .05f;
            }
        }

        private void Torches()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 p = new Vector3(side * 3f, .15f, .9f);
                PlaceAsset("Torch", p, new Vector3(.45f, 1.7f, .45f), gold);
                Part(PrimitiveType.Sphere, "Torch Flame", p + Vector3.up * 1.25f, new Vector3(.3f, .42f, .3f), gold);
                GameObject go = new GameObject("Torch Point Light");
                go.transform.SetParent(root, false);
                go.transform.localPosition = p + Vector3.up * 1.35f;
                Light l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1f, .62f, .28f);
                l.intensity = 2f;
                l.range = 7f;
            }
        }

        private void CameraSetup()
        {
            // Remove legacy greybox placeholders: the hero and castle must come from imported meshes.
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject go = new GameObject("Home World Camera", typeof(Camera));
                cam = go.GetComponent<Camera>();
                go.tag = "MainCamera";
            }
            cam.transform.position = new Vector3(0, 5.2f, -12.5f);
            cam.transform.LookAt(new Vector3(0, 3.4f, 3.2f));
            cam.fieldOfView = 42f;
            cam.nearClipPlane = .1f;
            cam.farClipPlane = 100f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.055f, .12f, .15f);
            RenderSettings.ambientLight = new Color(.48f, .49f, .46f);

            Light[] lights = FindObjectsOfType<Light>();
            bool hasDirectional = false;
            for (int i = 0; i < lights.Length; i++)
                if (lights[i].type == LightType.Directional) hasDirectional = true;
            if (!hasDirectional)
            {
                GameObject sun = new GameObject("Home Directional Light");
                Light l = sun.AddComponent<Light>();
                l.type = LightType.Directional;
                l.intensity = 1.15f;
                sun.transform.rotation = Quaternion.Euler(42, -28, 0);
            }
        }
    }
}