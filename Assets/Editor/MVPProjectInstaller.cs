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
            // Preserve any existing Boot scene and its authored objects.
            if (File.Exists(BootScenePath)) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Game Systems").AddComponent<GameBootstrap>();
            if (!EditorSceneManager.SaveScene(scene, BootScenePath))
                throw new IOException("Failed to save generated scene: " + BootScenePath);
            Debug.Log("[MVPProjectInstaller] Generated scene: " + BootScenePath);
        }

        private static void EnsureHomeScene()
        {
            // Preserve any existing Home scene and its authored objects.
            if (File.Exists(HomeScenePath)) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Home Screen").AddComponent<HomeScreen>();
            GameObject lightObject = new GameObject("Fallback Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(45f, -32f, 0f);
            if (!EditorSceneManager.SaveScene(scene, HomeScenePath))
                throw new IOException("Failed to save generated scene: " + HomeScenePath);
            Debug.Log("[MVPProjectInstaller] Generated scene: " + HomeScenePath);
        }
    }
}
