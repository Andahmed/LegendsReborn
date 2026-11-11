// =============================================================================
// FILE: Assets/Editor/BuildScript.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Menu 'Sands > Build Android APK' + CI entry point (BuildScript.BuildAndroid) used by Cloud Build/GitHub Actions.
// WORK ON: Set keystore, IL2CPP/ARM64, versionCode bump, AAB output for Play Store.
// STATUS: Implemented
// =============================================================================
using System;
using System.Linq;
using SandsOfLegends.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace SandsOfLegends.EditorTools
{
    public static class BuildScript
    {
        [MenuItem("Sands/Build Android APK")]
        public static void BuildAndroid()
        {
            MVPProjectInstaller.EnsureMvpScene();
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
