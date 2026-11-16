using System.IO;
using SandsOfLegends.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SandsOfLegends.Editor
{
    /// <summary>
    /// Creates a separate visual test scene without modifying the existing Home scene or gameplay flow.
    /// </summary>
    public static class OpenWorldPrototypeInstaller
    {
        private const string ScenePath = "Assets/Scenes/OpenWorldPrototype.unity";

        [MenuItem("Sands/World/Generate Open World Prototype")]
        public static void Generate()
        {
            if (!EditorUtility.DisplayDialog(
                "Generate Open World Prototype",
                "This creates a separate preview scene. It does not change Home.unity, the story, or gameplay systems.",
                "Generate Scene",
                "Cancel"))
                return;

            if (!Directory.Exists("Assets/Scenes"))
                Directory.CreateDirectory("Assets/Scenes");

            // Build additively so generating a preview never replaces the scene the developer is editing.
            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                GameObject world = new GameObject("LegendsReborn Open World Preview");
                OpenWorldTerrainPrototype.Build(world.transform);

                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new IOException("Unity could not save the preview scene at " + ScenePath);

                AssetDatabase.SaveAssets();
                Debug.Log("Open World prototype scene generated: " + ScenePath);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.CloseScene(scene, true);
                if (previousScene.IsValid() && previousScene.isLoaded)
                    SceneManager.SetActiveScene(previousScene);
            }
            EditorUtility.DisplayDialog(
                "Prototype Created",
                "Open Assets/Scenes/OpenWorldPrototype.unity and press Play to inspect the terrain, road, river, trees and landmarks.",
                "OK");
        }
    }
}