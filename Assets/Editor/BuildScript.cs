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

            // Match GameCI's configured output location exactly.
            const string outputPath = "build/Android/Android.apk";
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Android APK build failed: " + report.summary.result +
                    ". Errors: " + report.summary.totalErrors);

            if (!File.Exists(options.locationPathName) || new FileInfo(options.locationPathName).Length == 0)
                throw new IOException("Unity reported success but the APK is missing or empty.");

            Debug.Log("Android APK created: " + Path.GetFullPath(options.locationPathName) +
                " (" + report.summary.totalSize + " bytes)");
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
