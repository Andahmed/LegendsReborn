// =============================================================================
// FILE: Assets/Editor/PrototypeSetup.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Idempotently create prototype hero/skill assets, Boot/Home/Battle scenes, and build scene entries.
// WORK ON: Replace prototype cubes and placeholder UI with approved art and production scene composition.
// STATUS: Prototype implementation
// =============================================================================
using System.Collections.Generic;
using System.IO;
using SandsOfLegends.Bootstrap;
using SandsOfLegends.Combat;
using SandsOfLegends.Core;
using SandsOfLegends.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SandsOfLegends.EditorTools
{
    public static class PrototypeSetup
    {
        private const string DataRoot = "Assets/Data";
        private const string HeroesRoot = DataRoot + "/Heroes";
        private const string SkillsRoot = DataRoot + "/Skills";
        private const string ScenesRoot = "Assets/Scenes";
        private const string BootPath = ScenesRoot + "/Boot.unity";
        private const string HomePath = ScenesRoot + "/Home.unity";
        private const string BattlePath = ScenesRoot + "/Battle.unity";

        [MenuItem("Sands/Setup Prototype")]
        public static void SetupPrototype()
        {
            EnsureFolder("Assets", "Data");
            EnsureFolder(DataRoot, "Heroes");
            EnsureFolder(DataRoot, "Skills");
            EnsureFolder("Assets", "Scenes");

            HeroDefinition[] heroes = CreateHeroAssets();

            if (!File.Exists(BootPath)) CreateBootScene();
            if (!File.Exists(HomePath)) CreateHomeScene();
            if (!File.Exists(BattlePath)) CreateBattleScene(heroes);

            var buildScenes = new List<EditorBuildSettingsScene>();
            AddOrEnableScene(buildScenes, BootPath);
            AddOrEnableScene(buildScenes, HomePath);
            AddOrEnableScene(buildScenes, BattlePath);
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                bool alreadyAdded = false;
                for (int i = 0; i < buildScenes.Count; i++)
                    if (buildScenes[i].path == existing.path) { alreadyAdded = true; break; }
                if (!alreadyAdded) buildScenes.Add(existing);
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(BootPath, OpenSceneMode.Single);
            Debug.Log("Sands of Legends prototype assets and scenes are ready. Existing scenes/assets were preserved.");
        }

        private static HeroDefinition[] CreateHeroAssets()
        {
            var factions = new[] { Faction.Fortress, Faction.Wild, Faction.Forest, Faction.Shadow };
            var names = new[] { "Aren", "Mira", "Thalen", "Neris" };
            var roles = new[] { Role.Tank, Role.Dps, Role.Support, Role.Dps };
            var heroes = new HeroDefinition[4];

            for (int i = 0; i < 4; i++)
            {
                string key = factions[i].ToString();
                SkillDefinition active = GetOrCreateSkill(
                    "Skill_" + key + "_Active", key + "_active", names[i] + " Technique",
                    4.5f, 1.35f, TargetType.SingleEnemy, false);

                bool heals = factions[i] == Faction.Forest;
                SkillDefinition ultimate = GetOrCreateSkill(
                    "Skill_" + key + "_Ultimate", key + "_ultimate", names[i] + " Resolve",
                    0f, 2.2f, heals ? TargetType.AllAllies : TargetType.AllEnemies, heals);

                string assetPath = HeroesRoot + "/Hero_" + key + ".asset";
                HeroDefinition hero = AssetDatabase.LoadAssetAtPath<HeroDefinition>(assetPath);
                if (hero == null)
                {
                    hero = ScriptableObject.CreateInstance<HeroDefinition>();
                    hero.id = "hero_" + key.ToLowerInvariant();
                    hero.displayName = names[i];
                    hero.faction = factions[i];
                    hero.role = roles[i];
                    hero.rarity = Rarity.Epic;
                    hero.hp = 1000;
                    hero.atk = 100;
                    hero.def = 50;
                    hero.spd = 100;
                    hero.critChance = 0.1f;
                    hero.growth = 0.08f;
                    hero.skill = active;
                    hero.ultimate = ultimate;
                    AssetDatabase.CreateAsset(hero, assetPath);
                }
                heroes[i] = hero;
            }

            AssetDatabase.SaveAssets();
            return heroes;
        }

        private static SkillDefinition GetOrCreateSkill(
            string fileName, string id, string displayName, float cooldown,
            float power, TargetType target, bool isHeal)
        {
            string path = SkillsRoot + "/" + fileName + ".asset";
            SkillDefinition skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<SkillDefinition>();
                skill.id = id;
                skill.displayName = displayName;
                skill.cooldown = cooldown;
                skill.power = power;
                skill.target = target;
                skill.isHeal = isHeal;
                AssetDatabase.CreateAsset(skill, path);
            }
            return skill;
        }

        private static void CreateBootScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();
            bootstrap.AddComponent<PrototypeBootRouter>();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), BootPath);
        }

        private static void CreateHomeScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateEventSystem();
            Canvas canvas = CreateCanvas("HomeCanvas");
            CreateText(canvas.transform, "Title", "SANDS OF LEGENDS", 34, new Vector2(0f, 150f), new Vector2(700f, 90f), TextAnchor.MiddleCenter);
            CreateText(canvas.transform, "Subtitle", "Prototype Home - Campaign / Heroes / Summon", 20, new Vector2(0f, 75f), new Vector2(800f, 55f), TextAnchor.MiddleCenter);
            Button startBattle = CreateButton(canvas.transform, "EnterBattle", "Enter Battle", new Vector2(0f, -35f));
            PrototypeHomeController homeController = canvas.gameObject.AddComponent<PrototypeHomeController>();
            homeController.battleButton = startBattle;
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), HomePath);
        }

        private static void CreateBattleScene(HeroDefinition[] heroes)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateLight();
            CreateEventSystem();

            var playerModels = new Transform[4];
            var enemyModels = new Transform[4];
            var playerHealth = new Image[4];
            var enemyHealth = new Image[4];
            var playerLabels = new Text[4];
            var enemyLabels = new Text[4];

            Color playerColor = new Color(0.18f, 0.72f, 0.82f);
            Color enemyColor = new Color(0.82f, 0.38f, 0.28f);

            for (int i = 0; i < 4; i++)
            {
                float y = 2.2f - i * 1.35f;
                playerModels[i] = CreateHeroCube("Player_" + heroes[i].displayName, new Vector3(-4.2f, y, 0f), playerColor).transform;
                enemyModels[i] = CreateHeroCube("Enemy_" + heroes[i].displayName, new Vector3(4.2f, y, 0f), enemyColor).transform;
            }

            Canvas canvas = CreateCanvas("BattleCanvas");
            CreateText(canvas.transform, "BattleTitle", "PROTOTYPE BATTLE 4 vs 4", 26, new Vector2(0f, 315f), new Vector2(650f, 48f), TextAnchor.MiddleCenter);
            Text status = CreateText(canvas.transform, "BattleStatus", "Preparing battle...", 20, new Vector2(0f, 275f), new Vector2(650f, 40f), TextAnchor.MiddleCenter);

            for (int i = 0; i < 4; i++)
            {
                float leftX = -510f + i * 132f;
                float rightX = 114f + i * 132f;
                playerHealth[i] = CreateHealthBar(canvas.transform, "PlayerHP_" + i, leftX, 220f);
                enemyHealth[i] = CreateHealthBar(canvas.transform, "EnemyHP_" + i, rightX, 220f);
                playerLabels[i] = CreateText(canvas.transform, "PlayerLabel_" + i, heroes[i].displayName, 14, new Vector2(leftX, 240f), new Vector2(124f, 22f), TextAnchor.MiddleCenter);
                enemyLabels[i] = CreateText(canvas.transform, "EnemyLabel_" + i, "Echo " + heroes[i].displayName, 14, new Vector2(rightX, 240f), new Vector2(124f, 22f), TextAnchor.MiddleCenter);
            }

            Button speed1 = CreateButton(canvas.transform, "Speed1", "x1", new Vector2(-155f, -300f));
            Button speed2 = CreateButton(canvas.transform, "Speed2", "x2", new Vector2(0f, -300f));
            Button speed4 = CreateButton(canvas.transform, "Speed4", "x4", new Vector2(155f, -300f));

            var runtime = new GameObject("PrototypeBattleRuntime");
            BattleView battleView = runtime.AddComponent<BattleView>();
            PrototypeBattleController controller = runtime.AddComponent<PrototypeBattleController>();
            controller.heroes = heroes;
            controller.battleView = battleView;
            controller.playerModels = playerModels;
            controller.enemyModels = enemyModels;
            controller.playerHealth = playerHealth;
            controller.enemyHealth = enemyHealth;
            controller.playerLabels = playerLabels;
            controller.enemyLabels = enemyLabels;
            controller.battleStatus = status;
            controller.speed1Button = speed1;
            controller.speed2Button = speed2;
            controller.speed4Button = speed4;

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), BattlePath);
        }

        private static GameObject CreateHeroCube(string name, Vector3 position, Color color)
        {
            GameObject actor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            actor.name = name;
            actor.transform.position = position;
            actor.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            var renderer = actor.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            renderer.sharedMaterial = material;
            return actor;
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -18f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.backgroundColor = new Color(0.075f, 0.09f, 0.13f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, Vector2 position, Vector2 size, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Image CreateHealthBar(Transform parent, string name, float x, float y)
        {
            var backgroundObject = new GameObject(name + "_Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(parent, false);
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
            backgroundRect.anchoredPosition = new Vector2(x, y);
            backgroundRect.sizeDelta = new Vector2(120f, 14f);
            backgroundObject.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f);

            var fillObject = new GameObject(name + "_Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(backgroundObject.transform, false);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fill = fillObject.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            fill.color = new Color(0.2f, 0.78f, 0.42f);
            return fill;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(125f, 52f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.15f, 0.48f, 0.58f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText(buttonObject.transform, name + "_Text", label, 22, Vector2.zero, new Vector2(125f, 52f), TextAnchor.MiddleCenter);
            text.raycastTarget = false;
            return button;
        }

        private static void AddOrEnableScene(List<EditorBuildSettingsScene> scenes, string path)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == path)
                {
                    scenes[i] = new EditorBuildSettingsScene(path, true);
                    return;
                }
            }
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
