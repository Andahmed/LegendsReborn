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

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject world = new GameObject("LegendsReborn Open World Preview");
            OpenWorldTerrainPrototype.Build(world.transform);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Open World prototype scene generated: " + ScenePath);
            EditorUtility.DisplayDialog(
                "Prototype Created",
                "Open Assets/Scenes/OpenWorldPrototype.unity and press Play to inspect the terrain, road, river, trees and landmarks.",
                "OK");
        }
    }
}