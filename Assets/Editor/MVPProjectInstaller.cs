using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SandsOfLegends.Bootstrap;
using SandsOfLegends.UI;

namespace SandsOfLegends.Editor
{
    public static class MVPProjectInstaller
    {
        public const string BootScenePath = "Assets/Scenes/Boot.unity";
        public const string HomeScenePath = "Assets/Scenes/Home.unity";

        [MenuItem("Sands/Setup MVP Home Scene")]
        public static void SetupMenu()
        {
            EnsureMvpScene();
            EditorUtility.DisplayDialog("Sands of Legends", "Boot and Home scenes are ready. Boot initializes the save and routes into Home.", "OK");
        }

        public static void EnsureMvpScene()
        {
            HomeCastleMeshGenerator.GenerateIfMissing();
            Directory.CreateDirectory("Assets/Scenes");
            EnsureBootScene();
            EnsureHomeScene();

            // Boot must be first so GameBootstrap owns state before Home UI binds.
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(HomeScenePath, true)
            };
        }

        private static void EnsureBootScene()
        {
            Scene scene;
            if (File.Exists(BootScenePath))
                scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            else
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (Object.FindObjectOfType<GameBootstrap>() == null)
                new GameObject("Game Systems").AddComponent<GameBootstrap>();
            EditorSceneManager.SaveScene(scene, BootScenePath);
        }

        private static void EnsureHomeScene()
        {
            Scene scene;
            if (File.Exists(HomeScenePath))
                scene = EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single);
            else
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (Object.FindObjectOfType<HomeScreen>() == null)
                new GameObject("Home Screen").AddComponent<HomeScreen>();

            if (Object.FindObjectOfType<Light>() == null)
            {
                GameObject lightObject = new GameObject("Fallback Directional Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.1f;
                lightObject.transform.rotation = Quaternion.Euler(45f, -32f, 0f);
            }
            EditorSceneManager.SaveScene(scene, HomeScenePath);
        }
    }
}
