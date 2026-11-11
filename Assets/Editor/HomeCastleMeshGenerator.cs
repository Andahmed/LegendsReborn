// Original, editor-generated low-poly castle mesh. No Unity primitive cubes are used.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandsOfLegends.Editor
{
    public static class HomeCastleMeshGenerator
    {
        private const string ResourceRoot = "Assets/Resources/HomeCastle";
        private const string MeshRoot = ResourceRoot + "/Meshes";
        private const string MaterialRoot = ResourceRoot + "/Materials";
        private const string PrefabPath = ResourceRoot + "/LegendsRebornCastle.prefab";

        private sealed class MeshBuilder
        {
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<int> Triangles = new List<int>();

            public void Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
            {
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0f)
                {
                    Vector3 swap = b;
                    b = c;
                    c = swap;
                }
                int start = Vertices.Count;
                Vertices.Add(a);
                Vertices.Add(b);
                Vertices.Add(c);
                Triangles.Add(start);
                Triangles.Add(start + 1);
                Triangles.Add(start + 2);
            }

            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
            {
                Triangle(a, b, c, normal);
                Triangle(a, c, d, normal);
            }

            public Mesh Build(string meshName)
            {
                Mesh mesh = new Mesh();
                mesh.name = meshName;
                if (Vertices.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(Vertices);
                mesh.SetTriangles(Triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }
        }

        private static readonly string[] MaterialIds =
            { "sandstone", "lightstone", "darkstone", "gold", "teal", "crimson", "shadow" };

        private static readonly Color[] MaterialColors =
        {
            new Color(.68f, .53f, .36f),
            new Color(.84f, .70f, .49f),
            new Color(.30f, .34f, .34f),
            new Color(.96f, .70f, .25f),
            new Color(.035f, .39f, .39f),
            new Color(.53f, .13f, .16f),
            new Color(.075f, .12f, .14f)
        };

        [MenuItem("Sands/Generate Original 3D Home Castle")]
        public static void GenerateMenu()
        {
            Generate();
            EditorUtility.DisplayDialog("Legends Reborn", "Original low-poly castle mesh and prefab generated under Assets/Resources/HomeCastle.", "OK");
        }

        public static void GenerateIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
                Generate();
        }

        public static void Generate()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets/Resources", "HomeCastle");
            EnsureFolder(ResourceRoot, "Meshes");
            EnsureFolder(ResourceRoot, "Materials");

            AssetDatabase.DeleteAsset(PrefabPath);
            for (int i = 0; i < MaterialIds.Length; i++)
            {
                AssetDatabase.DeleteAsset(MeshRoot + "/castle_" + MaterialIds[i] + ".asset");
                AssetDatabase.DeleteAsset(MaterialRoot + "/" + MaterialIds[i] + ".mat");
            }

            var materials = new Dictionary<string, Material>();
            var builders = new Dictionary<string, MeshBuilder>();
            for (int i = 0; i < MaterialIds.Length; i++)
            {
                string id = MaterialIds[i];
                materials[id] = CreateMaterial(id, MaterialColors[i], id == "gold" ? .62f : 0f);
                builders[id] = new MeshBuilder();
            }

            BuildCourtyard(builders);
            BuildGate(builders);
            BuildWalls(builders);
            BuildTowers(builders);
            BuildKeep(builders);
            BuildBanners(builders);
            BuildStairsAndDais(builders);
            BuildTorches(builders);

            GameObject root = new GameObject("Legends Reborn - Original Castle Mesh");
            root.transform.position = Vector3.zero;
            foreach (string id in MaterialIds)
            {
                Mesh mesh = builders[id].Build("castle_" + id);
                string meshPath = MeshRoot + "/castle_" + id + ".asset";
                AssetDatabase.CreateAsset(mesh, meshPath);

                GameObject part = new GameObject("Castle Mesh - " + id);
                part.transform.SetParent(root.transform, false);
                MeshFilter filter = part.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                MeshRenderer renderer = part.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = materials[id];
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject saved = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (saved == null || saved.GetComponentsInChildren<MeshRenderer>(true).Length < 5)
                throw new System.InvalidOperationException("Generated castle prefab is incomplete.");
        }

        private static Material CreateMaterial(string id, Color color, float metallic)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.name = "Castle " + id;
            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", .32f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .32f);
            AssetDatabase.CreateAsset(material, MaterialRoot + "/" + id + ".mat");
            return material;
        }

        private static MeshBuilder B(Dictionary<string, MeshBuilder> builders, string id)
        {
            return builders[id];
        }

        private static void BuildCourtyard(Dictionary<string, MeshBuilder> b)
        {
            Cylinder(B(b, "sandstone"), new Vector3(0f, -.16f, 2.1f), 8.8f, 8.8f, .36f, 36);
            Cylinder(B(b, "gold"), new Vector3(0f, .045f, 2.1f), 8.84f, 8.84f, .07f, 36);
            Cylinder(B(b, "darkstone"), new Vector3(0f, .09f, 2.1f), 8.34f, 8.34f, .12f, 36);
            Cylinder(B(b, "sandstone"), new Vector3(0f, .17f, 2.1f), 8.17f, 8.17f, .12f, 36);

            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2f / 12f;
                float x = Mathf.Cos(a) * 6.8f;
                float z = 2.1f + Mathf.Sin(a) * 6.8f;
                Cylinder(B(b, "lightstone"), new Vector3(x, .27f, z), .16f, .16f, .12f, 8);
                Cylinder(B(b, "gold"), new Vector3(x, .34f, z), .09f, .09f, .06f, 8);
            }
        }

        private static void BuildGate(Dictionary<string, MeshBuilder> b)
        {
            MeshBuilder stone = B(b, "lightstone");
            Block(stone, new Vector3(-1.95f, 1.20f, 5.25f), new Vector3(.62f, 2.55f, 1.05f), .12f);
            Block(stone, new Vector3(1.95f, 1.20f, 5.25f), new Vector3(.62f, 2.55f, 1.05f), .12f);
            Arch(B(b, "gold"), new Vector3(0f, 2.22f, 5.25f), 2.35f, 1.65f, 1.05f, 18);
            Block(B(b, "gold"), new Vector3(0f, 4.53f, 5.25f), new Vector3(.58f, .48f, 1.16f), .12f);

            Block(B(b, "shadow"), new Vector3(-.76f, 1.12f, 5.82f), new Vector3(1.38f, 2.18f, .16f), .06f);
            Block(B(b, "shadow"), new Vector3(.76f, 1.12f, 5.82f), new Vector3(1.38f, 2.18f, .16f), .06f);
            for (int i = 0; i < 5; i++)
            {
                float x = -1.3f + i * .65f;
                Block(B(b, "gold"), new Vector3(x, 1.15f, 5.70f), new Vector3(.055f, 2.12f, .06f), .018f);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                Cylinder(B(b, "gold"), new Vector3(side * 2.65f, 2.25f, 5.25f), .16f, .16f, 4.5f, 8);
                Cylinder(B(b, "teal"), new Vector3(side * 2.65f, 4.58f, 5.25f), .24f, .24f, .24f, 8);
            }
        }

        private static void BuildWalls(Dictionary<string, MeshBuilder> b)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 4.15f;
                Block(B(b, "sandstone"), new Vector3(x, 2.15f, 5.55f), new Vector3(4.55f, 4.25f, .82f), .16f);
                Block(B(b, "darkstone"), new Vector3(x, .38f, 5.55f), new Vector3(4.65f, .32f, .98f), .08f);
                Block(B(b, "gold"), new Vector3(x, 4.23f, 5.55f), new Vector3(4.58f, .16f, .90f), .045f);
                for (int i = 0; i < 7; i++)
                {
                    float merlonX = x - 1.92f + i * .64f;
                    Block(B(b, "lightstone"), new Vector3(merlonX, 4.53f, 5.55f),
                        new Vector3(.40f, .48f, .84f), .075f);
                }
            }
        }

        private static void BuildTowers(Dictionary<string, MeshBuilder> b)
        {
            Tower(b, -6.65f, 5.55f, 5.55f, .98f, "teal");
            Tower(b, 6.65f, 5.55f, 5.55f, .98f, "crimson");
            Tower(b, -3.65f, 7.55f, 4.55f, .64f, "gold");
            Tower(b, 3.65f, 7.55f, 4.55f, .64f, "gold");
        }

        private static void Tower(Dictionary<string, MeshBuilder> b, float x, float z, float height, float radius, string pennant)
        {
            MeshBuilder stone = B(b, "sandstone");
            Cylinder(stone, new Vector3(x, height * .43f, z), radius * 1.18f, radius * .98f, height * .86f, 10);
            Cylinder(B(b, "darkstone"), new Vector3(x, .20f, z), radius * 1.25f, radius * 1.25f, .34f, 10);
            Cylinder(B(b, "gold"), new Vector3(x, height * .81f, z), radius * 1.05f, radius * 1.05f, .14f, 10);
            Cylinder(B(b, "lightstone"), new Vector3(x, height * .90f, z), radius * 1.10f, radius * .88f, .34f, 10);
            Cone(B(b, "teal"), new Vector3(x, height + .98f, z), radius * 1.22f, 1.95f, 10);
            Cone(B(b, "gold"), new Vector3(x, height + 2.02f, z), radius * .17f, .54f, 8);

            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f;
                float bx = x + Mathf.Cos(a) * radius * .82f;
                float bz = z + Mathf.Sin(a) * radius * .82f;
                Block(B(b, "lightstone"), new Vector3(bx, height * .91f, bz),
                    new Vector3(.25f, .43f, .25f), .05f);
            }
            Flag(b, x - radius * .92f, height * .78f + .65f, z - .10f, pennant);
        }

        private static void BuildKeep(Dictionary<string, MeshBuilder> b)
        {
            Block(B(b, "darkstone"), new Vector3(0f, 2.4f, 8.0f), new Vector3(5.9f, 4.8f, 1.55f), .22f);
            Block(B(b, "sandstone"), new Vector3(0f, 4.75f, 8.0f), new Vector3(5.5f, 2.15f, 1.42f), .20f);
            Block(B(b, "lightstone"), new Vector3(0f, 5.87f, 8.0f), new Vector3(5.72f, .20f, 1.60f), .07f);
            Block(B(b, "gold"), new Vector3(0f, 4.1f, 7.18f), new Vector3(2.2f, 2.7f, .10f), .035f);
            Block(B(b, "teal"), new Vector3(0f, 4.05f, 7.10f), new Vector3(1.32f, 1.78f, .12f), .06f);

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 1.86f;
                Cylinder(B(b, "gold"), new Vector3(x, 6.55f, 8.0f), .62f, .52f, .36f, 8);
                Cylinder(B(b, "sandstone"), new Vector3(x, 7.20f, 8.0f), .48f, .34f, 1.05f, 8);
                Cone(B(b, "crimson"), new Vector3(x, 8.02f, 8.0f), .72f, 1.25f, 8);
            }
            Block(B(b, "gold"), new Vector3(0f, 6.12f, 8.0f), new Vector3(.46f, .5f, 1.0f), .10f);
        }

        private static void BuildBanners(Dictionary<string, MeshBuilder> b)
        {
            Flag(b, -6.65f, 6.65f, 4.48f, "teal");
            Flag(b, 6.65f, 6.65f, 4.48f, "crimson");
            Flag(b, 0f, 7.65f, 7.1f, "gold");
        }

        private static void BuildStairsAndDais(Dictionary<string, MeshBuilder> b)
        {
            Cylinder(B(b, "gold"), new Vector3(0f, .25f, -2.15f), 1.92f, 1.92f, .50f, 24);
            Cylinder(B(b, "darkstone"), new Vector3(0f, .53f, -2.15f), 1.66f, 1.66f, .12f, 24);
            Cylinder(B(b, "teal"), new Vector3(0f, .61f, -2.15f), 1.48f, 1.48f, .08f, 24);
            Cylinder(B(b, "gold"), new Vector3(0f, .68f, -2.15f), 1.18f, 1.18f, .055f, 24);

            for (int i = 0; i < 6; i++)
            {
                float z = -.72f + i * .37f;
                float y = .16f + i * .065f;
                Block(B(b, "lightstone"), new Vector3(0f, y, z),
                    new Vector3(3.35f - i * .10f, .22f, .46f), .055f);
                Block(B(b, "gold"), new Vector3(0f, y + .12f, z - .19f),
                    new Vector3(3.25f - i * .10f, .045f, .035f), .012f);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                Column(b, side * 3.15f, .0f, 1.0f);
                Torch(b, side * 3.15f, .0f, 1.92f);
            }
        }

        private static void Column(Dictionary<string, MeshBuilder> b, float x, float y, float z)
        {
            Cylinder(B(b, "darkstone"), new Vector3(x, y + .18f, z), .42f, .42f, .36f, 10);
            Cylinder(B(b, "gold"), new Vector3(x, y + .40f, z), .35f, .29f, .12f, 10);
            Cylinder(B(b, "lightstone"), new Vector3(x, y + 1.18f, z), .28f, .23f, 1.48f, 10);
            Cylinder(B(b, "gold"), new Vector3(x, y + 1.96f, z), .39f, .39f, .18f, 10);
        }

        private static void Torch(Dictionary<string, MeshBuilder> b, float x, float y, float z)
        {
            Cylinder(B(b, "darkstone"), new Vector3(x, y + .25f, z), .12f, .12f, .50f, 8);
            Cylinder(B(b, "gold"), new Vector3(x, y + .56f, z), .18f, .14f, .18f, 8);
            Cone(B(b, "crimson"), new Vector3(x, y + .94f, z), .22f, .62f, 8);
            Cone(B(b, "gold"), new Vector3(x, y + 1.08f, z - .025f), .11f, .48f, 8);
        }

        private static void Flag(Dictionary<string, MeshBuilder> b, float x, float y, float z, string color)
        {
            Cylinder(B(b, "gold"), new Vector3(x, y + .42f, z), .045f, .045f, 2.35f, 8);
            float w = .92f;
            float h = 1.42f;
            Vector3 a = new Vector3(x, y + 1.50f, z);
            Vector3 c = new Vector3(x + w, y + 1.48f, z + .08f);
            Vector3 d = new Vector3(x + w * .92f, y + .12f, z + .22f);
            Vector3 e = new Vector3(x + w * .18f, y + .24f, z + .18f);
            B(b, color).Quad(a, c, d, e, Vector3.back);
            B(b, color).Quad(e + Vector3.back * .018f, d + Vector3.back * .018f,
                c + Vector3.back * .018f, a + Vector3.back * .018f, Vector3.forward);

            Vector3 center = new Vector3(x + .47f, y + .86f, z - .025f);
            Vector3 top = center + Vector3.up * .24f;
            Vector3 right = center + Vector3.right * .16f;
            Vector3 bottom = center + Vector3.down * .24f;
            Vector3 left = center + Vector3.left * .16f;
            B(b, "gold").Quad(top, right, bottom, left, Vector3.back);
        }

        private static void Arch(MeshBuilder builder, Vector3 center, float outerRadius, float innerRadius, float depth, int segments)
        {
            float frontZ = center.z - depth * .5f;
            float backZ = center.z + depth * .5f;
            for (int i = 0; i < segments; i++)
            {
                float a0 = Mathf.PI * i / segments;
                float a1 = Mathf.PI * (i + 1) / segments;
                float mid = (a0 + a1) * .5f;
                Vector3 if0 = ArcPoint(center, innerRadius, a0, frontZ);
                Vector3 if1 = ArcPoint(center, innerRadius, a1, frontZ);
                Vector3 of0 = ArcPoint(center, outerRadius, a0, frontZ);
                Vector3 of1 = ArcPoint(center, outerRadius, a1, frontZ);
                Vector3 ib0 = ArcPoint(center, innerRadius, a0, backZ);
                Vector3 ib1 = ArcPoint(center, innerRadius, a1, backZ);
                Vector3 ob0 = ArcPoint(center, outerRadius, a0, backZ);
                Vector3 ob1 = ArcPoint(center, outerRadius, a1, backZ);
                Vector3 radial = new Vector3(Mathf.Cos(mid), Mathf.Sin(mid), 0f);

                builder.Quad(if0, of0, of1, if1, Vector3.back);
                builder.Quad(ib1, ob1, ob0, ib0, Vector3.forward);
                builder.Quad(of0, ob0, ob1, of1, radial);
                builder.Quad(ib0, ib1, if1, if0, -radial);
                if (i == 0) builder.Quad(if0, ib0, ob0, of0, new Vector3(1f, 0f, 0f));
                if (i == segments - 1) builder.Quad(of1, ob1, ib1, if1, new Vector3(-1f, 0f, 0f));
            }
        }

        private static Vector3 ArcPoint(Vector3 center, float radius, float angle, float z)
        {
            return new Vector3(center.x + Mathf.Cos(angle) * radius,
                center.y + Mathf.Sin(angle) * radius, z);
        }

        private static void Block(MeshBuilder b, Vector3 center, Vector3 size, float bevel)
        {
            float hx = size.x * .5f;
            float hy = size.y * .5f;
            float hz = size.z * .5f;
            float cut = Mathf.Clamp(bevel, .005f, Mathf.Min(hx, hz) * .35f);
            float[] ys = { -hy, -hy + cut, hy - cut, hy };
            float[] xs = { hx - cut, hx, hx, hx - cut };
            float[] zs = { hz - cut, hz, hz, hz - cut };
            Vector3[][] rings = new Vector3[4][];
            for (int r = 0; r < 4; r++)
            {
                float x = xs[r];
                float z = zs[r];
                float y = ys[r];
                rings[r] = new[]
                {
                    center + new Vector3(-x, y, -z),
                    center + new Vector3(x, y, -z),
                    center + new Vector3(x, y, z),
                    center + new Vector3(-x, y, z)
                };
            }

            Vector3[] normals = { Vector3.back, Vector3.right, Vector3.forward, Vector3.left };
            for (int r = 0; r < 3; r++)
                for (int side = 0; side < 4; side++)
                {
                    int next = (side + 1) % 4;
                    b.Quad(rings[r][side], rings[r][next], rings[r + 1][next], rings[r + 1][side], normals[side]);
                }

            Vector3 bottom = center + Vector3.down * hy;
            Vector3 top = center + Vector3.up * hy;
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                b.Triangle(bottom, rings[0][next], rings[0][i], Vector3.down);
                b.Triangle(top, rings[3][i], rings[3][next], Vector3.up);
            }
        }

        private static void Cylinder(MeshBuilder b, Vector3 center, float bottomRadius, float topRadius, float height, int sides)
        {
            float half = height * .5f;
            Vector3 bottomCenter = center + Vector3.down * half;
            Vector3 topCenter = center + Vector3.up * half;
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides;
                float a1 = (i + 1) * Mathf.PI * 2f / sides;
                Vector3 b0 = new Vector3(center.x + Mathf.Cos(a0) * bottomRadius, bottomCenter.y,
                    center.z + Mathf.Sin(a0) * bottomRadius);
                Vector3 b1 = new Vector3(center.x + Mathf.Cos(a1) * bottomRadius, bottomCenter.y,
                    center.z + Mathf.Sin(a1) * bottomRadius);
                Vector3 t0 = new Vector3(center.x + Mathf.Cos(a0) * topRadius, topCenter.y,
                    center.z + Mathf.Sin(a0) * topRadius);
                Vector3 t1 = new Vector3(center.x + Mathf.Cos(a1) * topRadius, topCenter.y,
                    center.z + Mathf.Sin(a1) * topRadius);
                Vector3 radial = new Vector3(Mathf.Cos((a0 + a1) * .5f), 0f,
                    Mathf.Sin((a0 + a1) * .5f));
                if (topRadius <= .001f)
                    b.Triangle(b0, b1, topCenter, radial + Vector3.up * .25f);
                else
                    b.Quad(b0, b1, t1, t0, radial);
                b.Triangle(bottomCenter, b1, b0, Vector3.down);
                if (topRadius > .001f) b.Triangle(topCenter, t0, t1, Vector3.up);
            }
        }

        private static void Cone(MeshBuilder b, Vector3 center, float radius, float height, int sides)
        {
            Cylinder(b, center, radius, 0f, height, sides);
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
