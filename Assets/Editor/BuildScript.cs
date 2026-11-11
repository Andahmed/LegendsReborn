// =============================================================================
// FILE: Assets/Editor/BuildScript.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Menu 'Sands > Build Android APK' + CI entry point; ensures the prototype scenes exist before building.
// WORK ON: Configure keystore, IL2CPP/ARM64, versionCode bump, and AAB output for Play Store.
// STATUS: Implemented
// =============================================================================
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace SandsOfLegends.EditorTools
{
    public static class BuildScript
    {
        [MenuItem("Sands/Build Android APK")]
        public static void BuildAndroid()
        {
            PrototypeSetup.SetupPrototype();
            Directory.CreateDirectory("Builds");
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/SandsOfLegends.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Build failed: " + report.summary.result);
        }
    }
}
