// =============================================================================
// FILE: Assets/Editor/BuildScript.cs
// ROLE: Android APK build entry point used by the Editor menu and GameCI.
// =============================================================================
using System;
using System.IO;
using System.Linq;
using SandsOfLegends.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SandsOfLegends.EditorTools
{
    public static class BuildScript
    {
        [MenuItem("Sands/Build Android APK")]
        public static void BuildAndroid()
        {
            ConfigureAndroid();
            MVPProjectInstaller.EnsureMvpScene();

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes are registered in Build Settings.");

            string outputPath = Path.Combine("Builds", "Android", "SandsOfLegends.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Android APK build failed: " + summary.result +
                    ". Errors: " + summary.totalErrors);

            if (!File.Exists(outputPath) || new FileInfo(outputPath).Length == 0)
                throw new IOException("Unity reported success but the APK is missing or empty.");

            Debug.Log("Android APK created: " + Path.GetFullPath(outputPath) +
                " (" + new FileInfo(outputPath).Length + " bytes)");

            // GameCI parses this standard Build results block to determine whether
            // a custom BuildPlayer method completed successfully.
            Debug.Log(
                "###########################\n" +
                "#      Build results      #\n" +
                "###########################\n\n" +
                "Duration: " + summary.totalTime + "\n" +
                "Warnings: " + summary.totalWarnings + "\n" +
                "Errors: " + summary.totalErrors + "\n" +
                "Size: " + summary.totalSize + " bytes\n\n\n" +
                "Build succeeded!");
        }

        private static void ConfigureAndroid()
        {
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.productName = "Sands of Legends";
        }
    }
}
