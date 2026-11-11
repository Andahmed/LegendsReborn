// =============================================================================
// FILE: Assets/Editor/CharacterArtGenerator.cs
// OWNER ROLE: Technical Artist
// RESPONSIBILITY: Generate original low-poly humanoid character meshes, human rigs, seven animation clips, faction VFX, biome environments and UI portraits.
// WORK ON: Replace generated prototype art only with separately registered CC0 or original production assets.
// STATUS: Procedural 3D asset generator; run Sands > Setup Prototype in Unity 2022.3.62f3
// =============================================================================
using System;
using System.Collections.Generic;
using System.IO;
using SandsOfLegends.Combat;
using SandsOfLegends.Core;
using SandsOfLegends.Presentation;
using SandsOfLegends.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SandsOfLegends.EditorTools
{
    public static class CharacterArtGenerator
    {
        private const string Art = "Assets/Art";
        private static readonly string[] HeroIds = { "fortress", "wild", "forest", "shadow" };
        private static readonly Color[] Armor =
        {
            new Color(0.22f, 0.37f, 0.56f), new Color(0.56f, 0.28f, 0.13f),
            new Color(0.16f, 0.42f, 0.27f), new Color(0.23f, 0.19f, 0.34f)
        };
        private static readonly Color[] Accent =
        {
            new Color(0.88f, 0.67f, 0.24f), new Color(0.94f, 0.61f, 0.19f),
            new Color(0.57f, 0.78f, 0.34f), new Color(0.72f, 0.24f, 0.48f)
        };

        private sealed class MeshPart
        {
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<int> Triangles = new List<int>();
            public readonly List<BoneWeight> Weights = new List<BoneWeight>();

            public void AddBox(Vector3 center, Vector3 size, int bone)
            {
                Vector3 h = size * 0.5f;
                Vector3[] v =
                {
                    center + new Vector3(-h.x,-h.y,-h.z), center + new Vector3(h.x,-h.y,-h.z),
                    center + new Vector3(h.x,h.y,-h.z), center + new Vector3(-h.x,h.y,-h.z),
                    center + new Vector3(-h.x,-h.y,h.z), center + new Vector3(h.x,-h.y,h.z),
                    center + new Vector3(h.x,h.y,h.z), center + new Vector3(-h.x,h.y,h.z)
                };
                Add(v, new[] { 0,2,1, 0,3,2, 4,5,6, 4,6,7, 0,1,5, 0,5,4,
                    1,2,6, 1,6,5, 2,3,7, 2,7,6, 3,0,4, 3,4,7 }, bone);
            }

            public void AddOcta(Vector3 center, Vector3 scale, int bone)
            {
                Vector3[] v =
                {
                    center + Vector3.up * scale.y, center + Vector3.down * scale.y,
                    center + Vector3.left * scale.x, center + Vector3.right * scale.x,
                    center + Vector3.forward * scale.z, center + Vector3.back * scale.z
                };
                Add(v, new[] { 0,4,3, 0,2,4, 0,5,2, 0,3,5, 1,3,4, 1,4,2, 1,2,5, 1,5,3 }, bone);
            }

            public void AddCylinder(Vector3 from, Vector3 to, float radius, int bone, int sides = 8)
            {
                Vector3 axis = (to - from).normalized;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, axis);
                int start = Vertices.Count;
                for (int ring = 0; ring < 2; ring++)
                {
                    Vector3 origin = ring == 0 ? from : to;
                    for (int i = 0; i < sides; i++)
                    {
                        float angle = i * Mathf.PI * 2f / sides;
                        Vector3 local = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                        Vertices.Add(origin + rotation * local);
                        Weights.Add(Weight(bone));
                    }
                }
                for (int i = 0; i < sides; i++)
                {
                    int next = (i + 1) % sides;
                    Triangles.AddRange(new[] { start + i, start + next, start + sides + next,
                        start + i, start + sides + next, start + sides + i });
                }
            }

            private void Add(Vector3[] vertices, int[] indices, int bone)
            {
                int start = Vertices.Count;
                Vertices.AddRange(vertices);
                for (int i = 0; i < vertices.Length; i++) Weights.Add(Weight(bone));
                for (int i = 0; i < indices.Length; i++) Triangles.Add(start + indices[i]);
            }

            private static BoneWeight Weight(int bone)
            {
                return new BoneWeight { boneIndex0 = bone, weight0 = 1f };
            }

            public Mesh Build(string name, Matrix4x4[] bindposes)
            {
                Mesh mesh = new Mesh();
                mesh.name = name;
                mesh.SetVertices(Vertices);
                mesh.SetTriangles(Triangles, 0);
                mesh.boneWeights = Weights.ToArray();
                mesh.bindposes = bindposes;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }

            public Mesh BuildStatic(string name)
            {
                Mesh mesh = new Mesh();
                mesh.name = name;
                mesh.SetVertices(Vertices);
                mesh.SetTriangles(Triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }
        }

        public static void Generate(HeroDefinition[] heroes)
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder(Art, "Characters");
            EnsureFolder(Art + "/Characters", "Enemies");
            EnsureFolder(Art, "Environments");
            EnsureFolder(Art, "VFX");
            EnsureFolder(Art, "UI");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "Heroes");
            EnsureFolder("Assets/Prefabs", "UI");

            EnsureFolder(Art + "/Characters", "Enemies");
            EnsureFolder(Art + "/Characters/Enemies", "gloom_raider");
            for (int i = 0; i < HeroIds.Length; i++)
            {
                string id = HeroIds[i];
                CreateFactionVfx(id, UIStyle.FactionColor((Faction)i));
                GameObject prefab = CreateCharacter(id, id, Armor[i], Accent[i], false);
                string prefabPath = "Assets/Prefabs/Heroes/hero_" + id + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                UnityEngine.Object.DestroyImmediate(prefab);
                if (heroes != null)
                {
                    for (int h = 0; h < heroes.Length; h++)
                    {
                        if (heroes[h] == null || heroes[h].id != "hero_" + id) continue;
                        heroes[h].prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        EditorUtility.SetDirty(heroes[h]);
                    }
                }
                CreatePortrait(id, Armor[i], Accent[i]);
            }

            CreateFactionVfx("raider", UIStyle.FactionColor(Faction.Shadow));
            GameObject enemy = CreateCharacter("gloom_raider", "raider", new Color(0.18f, 0.17f, 0.20f),
                new Color(0.78f, 0.18f, 0.12f), true);
            string enemyPath = "Assets/Art/Characters/Enemies/gloom_raider/enemy_gloom_raider.prefab";
            EnsureFolder(Art + "/Characters/Enemies", "gloom_raider");
            PrefabUtility.SaveAsPrefabAsset(enemy, enemyPath);
            UnityEngine.Object.DestroyImmediate(enemy);

            CreateBiome("forest", new Color(0.15f, 0.34f, 0.19f));
            CreateBiome("desert", new Color(0.66f, 0.47f, 0.25f));
            CreateBiome("mountain", new Color(0.34f, 0.39f, 0.47f));
            CreateBiome("sea", new Color(0.10f, 0.32f, 0.48f));
            CreateBiome("ruins", new Color(0.39f, 0.32f, 0.25f));
            CreateUiFrame();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateCharacter(string id, string style, Color armor, Color accent, bool enemy)
        {
            string folder = enemy ? Art + "/Characters/Enemies/" + id : Art + "/Characters/" + id;
            EnsureFolder(Art + "/Characters", enemy ? "Enemies" : id);
            EnsureFolder(folder, "meshes");
            EnsureFolder(folder, "anims");

            GameObject root = new GameObject(enemy ? "enemy_" + id : "hero_" + id);
            root.transform.position = Vector3.zero;
            Transform[] bones = CreateHumanoidBones(root.transform);
            Matrix4x4[] bindposes = new Matrix4x4[bones.Length];
            for (int i = 0; i < bones.Length; i++) bindposes[i] = bones[i].worldToLocalMatrix;

            Color skin = new Color(0.68f, 0.46f, 0.32f);
            Color metal = new Color(0.25f, 0.27f, 0.31f);
            var groups = new Dictionary<string, MeshPart>();
            Add(groups, "armor", armor).AddBox(new Vector3(0f, 1.18f, 0f), new Vector3(.42f,.54f,.25f), 2);
            Add(groups, "armor", armor).AddBox(new Vector3(0f, 1.42f, .01f), new Vector3(.52f,.30f,.30f), 3);
            Add(groups, "accent", accent).AddBox(new Vector3(0f, .94f, .01f), new Vector3(.44f,.13f,.28f), 1);
            Add(groups, "skin", skin).AddOcta(new Vector3(0f, 1.72f, 0f), new Vector3(.17f,.21f,.15f), 5);
            Add(groups, "metal", metal).AddOcta(new Vector3(0f, 1.88f, 0f), new Vector3(.21f,.08f,.18f), 5);

            Add(groups, "armor", armor).AddOcta(new Vector3(-.29f,1.48f,0f), new Vector3(.19f,.16f,.19f), 6);
            Add(groups, "armor", armor).AddOcta(new Vector3(.29f,1.48f,0f), new Vector3(.19f,.16f,.19f), 9);
            Add(groups, "skin", skin).AddCylinder(new Vector3(-.34f,1.40f,0f),new Vector3(-.53f,1.20f,0f),.075f,8);
            Add(groups, "skin", skin).AddCylinder(new Vector3(.34f,1.40f,0f),new Vector3(.53f,1.20f,0f),.075f,11);
            Add(groups, "armor", armor).AddCylinder(new Vector3(-.25f,1.48f,0f),new Vector3(-.36f,1.38f,0f),.105f,7);
            Add(groups, "armor", armor).AddCylinder(new Vector3(.25f,1.48f,0f),new Vector3(.36f,1.38f,0f),.105f,10);
            Add(groups, "armor", armor).AddCylinder(new Vector3(-.13f,.79f,0f),new Vector3(-.13f,.40f,0f),.12f,12);
            Add(groups, "armor", armor).AddCylinder(new Vector3(.13f,.79f,0f),new Vector3(.13f,.40f,0f),.12f,15);
            Add(groups, "metal", metal).AddBox(new Vector3(-.13f,.13f,.06f),new Vector3(.25f,.20f,.38f),14);
            Add(groups, "metal", metal).AddBox(new Vector3(.13f,.13f,.06f),new Vector3(.25f,.20f,.38f),17);
            Add(groups, "skin", skin).AddCylinder(new Vector3(-.13f,.40f,0f),new Vector3(-.13f,.12f,.04f),.07f,13);
            Add(groups, "skin", skin).AddCylinder(new Vector3(.13f,.40f,0f),new Vector3(.13f,.12f,.04f),.07f,16);

            if (style == "fortress")
            {
                Add(groups, "metal", metal).AddBox(new Vector3(.68f,1.03f,.03f),new Vector3(.31f,.52f,.13f),11);
                Add(groups, "accent", accent).AddOcta(new Vector3(.68f,1.04f,.12f),new Vector3(.12f,.17f,.04f),11);
                Add(groups, "metal", metal).AddCylinder(new Vector3(.57f,1.18f,0f),new Vector3(.62f,.66f,0f),.035f,11);
                Add(groups, "accent", accent).AddOcta(new Vector3(.62f,.65f,0f),new Vector3(.09f,.16f,.07f),11);
            }
            else if (style == "wild")
            {
                Add(groups, "accent", accent).AddOcta(new Vector3(-.29f,1.48f,0f),new Vector3(.24f,.27f,.22f),6);
                Add(groups, "accent", accent).AddOcta(new Vector3(.29f,1.48f,0f),new Vector3(.24f,.27f,.22f),9);
                Add(groups, "metal", metal).AddCylinder(new Vector3(.55f,1.20f,0f),new Vector3(.83f,.95f,0f),.025f,11);
                Add(groups, "metal", metal).AddCylinder(new Vector3(-.55f,1.20f,0f),new Vector3(-.83f,.95f,0f),.025f,8);
            }
            else if (style == "forest")
            {
                Add(groups, "accent", accent).AddOcta(new Vector3(-.27f,1.52f,0f),new Vector3(.16f,.22f,.15f),3);
                Add(groups, "accent", accent).AddOcta(new Vector3(.27f,1.52f,0f),new Vector3(.16f,.22f,.15f),3);
                Add(groups, "metal", metal).AddCylinder(new Vector3(.57f,1.20f,0f),new Vector3(.63f,.50f,0f),.028f,11);
                Add(groups, "accent", accent).AddOcta(new Vector3(.63f,.47f,0f),new Vector3(.13f,.17f,.13f),11);
            }
            else if (style == "raider")
            {
                Add(groups, "accent", accent).AddOcta(new Vector3(0f,1.42f,.05f),new Vector3(.31f,.23f,.20f),3);
                Add(groups, "metal", metal).AddOcta(new Vector3(-.32f,1.55f,0f),new Vector3(.18f,.25f,.18f),6);
                Add(groups, "metal", metal).AddOcta(new Vector3(.32f,1.55f,0f),new Vector3(.18f,.25f,.18f),9);
                Add(groups, "metal", metal).AddCylinder(new Vector3(.54f,1.20f,0f),new Vector3(.80f,.84f,0f),.04f,11);
                Add(groups, "accent", accent).AddOcta(new Vector3(.80f,.80f,0f),new Vector3(.12f,.20f,.09f),11);
            }
            else
            {
                Add(groups, "accent", accent).AddOcta(new Vector3(0f,1.84f,-.02f),new Vector3(.25f,.25f,.22f),5);
                Add(groups, "metal", metal).AddOcta(new Vector3(-.17f,1.87f,0f),new Vector3(.06f,.11f,.06f),5);
                Add(groups, "metal", metal).AddOcta(new Vector3(.17f,1.87f,0f),new Vector3(.06f,.11f,.06f),5);
                Add(groups, "metal", metal).AddCylinder(new Vector3(.54f,1.20f,0f),new Vector3(.77f,.91f,0f),.025f,11);
                Add(groups, "metal", metal).AddCylinder(new Vector3(-.54f,1.20f,0f),new Vector3(-.77f,.91f,0f),.025f,8);
            }

            foreach (KeyValuePair<string, MeshPart> pair in groups)
            {
                Mesh mesh = pair.Value.Build("mesh_" + id + "_" + pair.Key, bindposes);
                string meshPath = folder + "/meshes/" + id + "_" + pair.Key + ".asset";
                if (File.Exists(meshPath)) AssetDatabase.DeleteAsset(meshPath);
                AssetDatabase.CreateAsset(mesh, meshPath);
                GameObject part = new GameObject(pair.Key + "_mesh");
                part.transform.SetParent(root.transform, false);
                SkinnedMeshRenderer renderer = part.AddComponent<SkinnedMeshRenderer>();
                renderer.sharedMesh = mesh;
                renderer.bones = bones;
                renderer.rootBone = bones[1];
                renderer.sharedMaterial = CreateMaterial(id + "_" + pair.Key, ColorFor(pair.Key, armor, accent, skin, metal));
                renderer.updateWhenOffscreen = true;
            }

            Animator animator = root.AddComponent<Animator>();
            Avatar avatar = BuildHumanoidAvatar(root, bones);
            avatar.name = id + "_humanoid_avatar";
            string avatarPath = folder + "/" + id + "_avatar.asset";
            if (File.Exists(avatarPath)) AssetDatabase.DeleteAsset(avatarPath);
            AssetDatabase.CreateAsset(avatar, avatarPath);
            animator.avatar = avatar;
            animator.runtimeAnimatorController = CreateAnimatorController(root, bones, folder, id);
            HeroView view = root.AddComponent<HeroView>();
            view.animator = animator;
            Transform hp = new GameObject("hp_bar_anchor").transform;
            hp.SetParent(root.transform, false);
            hp.localPosition = new Vector3(0f, 2.05f, 0f);
            view.hpBarAnchor = hp;
            view.skillVfx = AssetDatabase.LoadAssetAtPath<GameObject>(Art + "/VFX/" + style + "/vfx_" + style + "_skill.prefab").GetComponent<ParticleSystem>();
            view.ultimateVfx = AssetDatabase.LoadAssetAtPath<GameObject>(Art + "/VFX/" + style + "/vfx_" + style + "_ultimate.prefab").GetComponent<ParticleSystem>();
            return root;
        }

        private static MeshPart Add(Dictionary<string, MeshPart> groups, string key, Color color)
        {
            if (!groups.ContainsKey(key)) groups.Add(key, new MeshPart());
            return groups[key];
        }

        private static Color ColorFor(string key, Color armor, Color accent, Color skin, Color metal)
        {
            if (key == "accent") return accent;
            if (key == "skin") return skin;
            if (key == "metal") return metal;
            return armor;
        }

        private static Transform[] CreateHumanoidBones(Transform root)
        {
            Transform[] b = new Transform[18];
            b[0] = root;
            b[0].name = "Root";
            b[1] = Bone("Hips", b[0], new Vector3(0f,.88f,0f));
            b[2] = Bone("Spine", b[1], new Vector3(0f,.22f,0f));
            b[3] = Bone("Chest", b[2], new Vector3(0f,.20f,0f));
            b[4] = Bone("Neck", b[3], new Vector3(0f,.20f,0f));
            b[5] = Bone("Head", b[4], new Vector3(0f,.10f,0f));
            b[6] = Bone("LeftShoulder", b[3], new Vector3(-.22f,.10f,0f));
            b[7] = Bone("LeftUpperArm", b[6], new Vector3(-.12f,-.05f,0f));
            b[8] = Bone("LeftLowerArm", b[7], new Vector3(-.17f,-.08f,0f));
            b[9] = Bone("RightShoulder", b[3], new Vector3(.22f,.10f,0f));
            b[10] = Bone("RightUpperArm", b[9], new Vector3(.12f,-.05f,0f));
            b[11] = Bone("RightLowerArm", b[10], new Vector3(.17f,-.08f,0f));
            b[12] = Bone("LeftUpperLeg", b[1], new Vector3(-.13f,-.09f,0f));
            b[13] = Bone("LeftLowerLeg", b[12], new Vector3(0f,-.38f,0f));
            b[14] = Bone("LeftFoot", b[13], new Vector3(0f,-.34f,.06f));
            b[15] = Bone("RightUpperLeg", b[1], new Vector3(.13f,-.09f,0f));
            b[16] = Bone("RightLowerLeg", b[15], new Vector3(0f,-.38f,0f));
            b[17] = Bone("RightFoot", b[16], new Vector3(0f,-.34f,.06f));
            return b;
        }

        private static Transform Bone(string name, Transform parent, Vector3 local)
        {
            Transform t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = local;
            return t;
        }

        private static Avatar BuildHumanoidAvatar(GameObject root, Transform[] bones)
        {
            string[] humanNames =
            {
                "Hips","Spine","Chest","Neck","Head","LeftShoulder","LeftUpperArm","LeftLowerArm",
                "RightShoulder","RightUpperArm","RightLowerArm","LeftUpperLeg","LeftLowerLeg","LeftFoot",
                "RightUpperLeg","RightLowerLeg","RightFoot"
            };
            var human = new HumanBone[humanNames.Length];
            for (int i = 0; i < humanNames.Length; i++)
                human[i] = new HumanBone
                {
                    humanName = humanNames[i],
                    boneName = humanNames[i],
                    limit = new HumanLimit { useDefaultValues = true }
                };
            var skeleton = new SkeletonBone[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                skeleton[i] = new SkeletonBone
                {
                    name = bones[i].name,
                    position = bones[i].localPosition,
                    rotation = bones[i].localRotation,
                    scale = bones[i].localScale
                };
            var description = new HumanDescription
            {
                human = human,
                skeleton = skeleton,
                upperArmTwist = .5f, lowerArmTwist = .5f,
                upperLegTwist = .5f, lowerLegTwist = .5f,
                armStretch = .05f, legStretch = .05f,
                feetSpacing = 0f, hasTranslationDoF = false
            };
            Avatar avatar = AvatarBuilder.BuildHumanAvatar(root, description);
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                throw new InvalidOperationException("Could not build a valid Humanoid Avatar for " + root.name);
            return avatar;
        }

        private static RuntimeAnimatorController CreateAnimatorController(GameObject root, Transform[] bones, string folder, string id)
        {
            string controllerPath = folder + "/" + id + "_animator.controller";
            if (File.Exists(controllerPath)) AssetDatabase.DeleteAsset(controllerPath);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = machine.AddState("Idle");
            idle.motion = CreateClip(root, bones, folder, "Idle", true);
            machine.defaultState = idle;
            string[] names = { "Run", "Attack", "Skill", "Ultimate", "Hit", "Death" };
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                AnimatorState state = machine.AddState(name);
                state.motion = CreateClip(root, bones, folder, name, name == "Run");
                controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
                AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
                enter.hasExitTime = false;
                enter.duration = .05f;
                enter.canTransitionToSelf = false;
                enter.AddCondition(AnimatorConditionMode.If, 0f, name);
                if (name != "Death")
                {
                    AnimatorStateTransition exit = state.AddTransition(idle);
                    exit.hasExitTime = true;
                    exit.exitTime = 1f;
                    exit.duration = .08f;
                }
            }
            return controller;
        }

        private static AnimationClip CreateClip(GameObject root, Transform[] bones, string folder, string name, bool loop)
        {
            AnimationClip clip = new AnimationClip();
            clip.name = name;
            clip.frameRate = 30f;
            clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            float[] times = { 0f, .5f, 1f };
            if (name == "Idle")
                SetPositionY(clip, root.transform, bones[1], times, new[] { .88f, .90f, .88f });
            if (name == "Run")
            {
                SetRotation(clip, root.transform, bones[7], times, new[] { 25f, -25f, 25f });
                SetRotation(clip, root.transform, bones[10], times, new[] { -25f, 25f, -25f });
                SetRotation(clip, root.transform, bones[12], times, new[] { -28f, 28f, -28f });
                SetRotation(clip, root.transform, bones[15], times, new[] { 28f, -28f, 28f });
            }
            if (name == "Attack") SetRotation(clip, root.transform, bones[10], times, new[] { 0f, -100f, 0f });
            if (name == "Skill")
            {
                SetRotation(clip, root.transform, bones[3], times, new[] { 0f, -20f, 0f });
                SetRotation(clip, root.transform, bones[10], times, new[] { 0f, -75f, 0f });
            }
            if (name == "Ultimate")
            {
                SetRotation(clip, root.transform, bones[7], times, new[] { 0f, 70f, 0f });
                SetRotation(clip, root.transform, bones[10], times, new[] { 0f, -70f, 0f });
            }
            if (name == "Hit") SetRotation(clip, root.transform, bones[3], times, new[] { 0f, 22f, 0f });
            if (name == "Death")
            {
                SetRotation(clip, root.transform, bones[1], times, new[] { 0f, 75f, 88f });
                SetRotation(clip, root.transform, bones[3], times, new[] { 0f, 25f, 40f });
            }
            string clipPath = folder + "/anims/" + name.ToLowerInvariant() + ".anim";
            if (File.Exists(clipPath)) AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);
            return clip;
        }

        private static void SetPositionY(AnimationClip clip, Transform root, Transform bone, float[] times, float[] values)
        {
            string path = AnimationUtility.CalculateTransformPath(bone, root.transform);
            var curve = new AnimationCurve();
            for (int i = 0; i < times.Length; i++) curve.AddKey(new Keyframe(times[i], values[i]));
            clip.SetCurve(path, typeof(Transform), "localPosition.y", curve);
        }

        private static void SetRotation(AnimationClip clip, Transform root, Transform bone, float[] times, float[] angles)
        {
            string path = AnimationUtility.CalculateTransformPath(bone, root.transform);
            Quaternion[] rotations = new Quaternion[angles.Length];
            for (int i = 0; i < angles.Length; i++) rotations[i] = Quaternion.Euler(0f, 0f, angles[i]);
            string[] properties = { "localRotation.x", "localRotation.y", "localRotation.z", "localRotation.w" };
            for (int p = 0; p < properties.Length; p++)
            {
                var curve = new AnimationCurve();
                for (int i = 0; i < times.Length; i++)
                {
                    float value = p == 0 ? rotations[i].x : p == 1 ? rotations[i].y : p == 2 ? rotations[i].z : rotations[i].w;
                    curve.AddKey(new Keyframe(times[i], value));
                }
                clip.SetCurve(path, typeof(Transform), properties[p], curve);
            }
        }

        private static void CreateFactionVfx(string faction, Color color)
        {
            EnsureFolder(Art, "VFX");
            EnsureFolder(Art + "/VFX", faction);
            CreateVfxPrefab(faction, "skill", color, 16);
            CreateVfxPrefab(faction, "ultimate", Color.Lerp(color, UIStyle.Gold, .35f), 42);
        }

        private static void CreateVfxPrefab(string faction, string type, Color color, short particles)
        {
            string path = Art + "/VFX/" + faction + "/vfx_" + faction + "_" + type + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            GameObject go = new GameObject("vfx_" + faction + "_" + type);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = .7f; main.loop = false; main.startLifetime = .55f;
            main.startSpeed = 2.1f; main.startSize = .18f; main.startColor = color;
            main.maxParticles = particles;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, particles) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = type == "ultimate" ? .8f : .35f;
            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void CreateBiome(string biome, Color color)
        {
            string folder = Art + "/Environments/" + biome;
            EnsureFolder(Art + "/Environments", biome);
            EnsureFolder(folder, "props");
            string path = folder + "/env_" + biome + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            GameObject root = new GameObject("env_" + biome);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "ground";
            ground.transform.SetParent(root.transform, false);
            ground.transform.localPosition = new Vector3(0f, -.18f, 0f);
            ground.transform.localScale = new Vector3(14f, .35f, 10f);
            UnityEngine.Object.DestroyImmediate(ground.GetComponent<Collider>());
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("env_" + biome, color);
            for (int i = 0; i < 4; i++)
            {
                GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                landmark.name = "landmark_" + i;
                landmark.transform.SetParent(root.transform, false);
                landmark.transform.localPosition = new Vector3(i < 2 ? -5f : 5f, .8f + (i % 2) * .4f, i % 2 == 0 ? -2.5f : 2.5f);
                landmark.transform.localScale = new Vector3(.7f, 1.6f + (i % 2) * .8f, .7f);
                UnityEngine.Object.DestroyImmediate(landmark.GetComponent<Collider>());
                landmark.GetComponent<Renderer>().sharedMaterial = CreateMaterial("env_" + biome + "_landmark", Color.Lerp(color, UIStyle.Gold, .22f));
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            CreatePropPrefab(folder + "/props/prop_ancient_gate.prefab", "prop_ancient_gate", new Color(.38f,.32f,.24f));
            CreatePropPrefab(folder + "/props/prop_forest_crystal.prefab", "prop_forest_crystal", UIStyle.Turquoise);
        }

        private static void CreatePropPrefab(string path, string name, Color color)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            GameObject go = new GameObject(name);
            GameObject meshObject = GameObject.CreatePrimitive(name.Contains("crystal") ? PrimitiveType.Sphere : PrimitiveType.Cube);
            meshObject.name = "mesh";
            meshObject.transform.SetParent(go.transform, false);
            UnityEngine.Object.DestroyImmediate(meshObject.GetComponent<Collider>());
            meshObject.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name, color);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void CreatePortrait(string id, Color background, Color accent)
        {
            string path = "Assets/Prefabs/UI/hero_" + id + "_portrait.png";
            if (File.Exists(path)) return;
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x - 64f) / 64f, dy = (y - 64f) / 64f;
                Color c = Color.Lerp(background * .45f, background, Mathf.Clamp01(.7f + dy * .25f));
                if (dx * dx + dy * dy < .74f) c = Color.Lerp(c, accent, Mathf.Clamp01((.22f - Mathf.Abs(dx)) * 2f));
                if (y > 40 && y < 92 && Mathf.Abs(x - 64) < 22) c = Color.Lerp(c, new Color(.72f,.48f,.32f), .7f);
                if (y > 83 && y < 115 && Mathf.Abs(x - 64) < 34) c = Color.Lerp(c, accent, .8f);
                pixels[y * size + x] = c;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            ConfigureSprite(path);
        }

        private static void CreateUiFrame()
        {
            const string path = "Assets/Prefabs/UI/ui_frame_gold.png";
            if (File.Exists(path)) return;
            Texture2D texture = new Texture2D(128,128,TextureFormat.RGBA32,false);
            Color[] pixels = new Color[128*128];
            for (int y=0;y<128;y++) for(int x=0;x<128;x++)
            {
                bool edge = x < 6 || x > 121 || y < 6 || y > 121;
                pixels[y*128+x] = edge ? UIStyle.Gold : new Color(0f,0f,0f,0f);
            }
            texture.SetPixels(pixels); texture.Apply(); File.WriteAllBytes(path,texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            ConfigureSprite(path);
        }


        private static void ConfigureSprite(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.textureCompression = TextureImporterCompression.Compressed;
            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.name = "Android";
            android.overridden = true;
            android.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
        }

        private static Material CreateMaterial(string id, Color color)
        {
            string folder = Art + "/Materials";
            EnsureFolder(Art, "Materials");
            string path = folder + "/" + id + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader);
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
