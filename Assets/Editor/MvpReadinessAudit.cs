using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SandsOfLegends.Editor
{
    /// <summary>
    /// Creates a local inventory of MVP assets. It does not claim Unity compile or runtime tests passed.
    /// </summary>
    public static class MvpReadinessAudit
    {
        private sealed class Check
        {
            public string Name;
            public string Path;
            public bool Present;
            public string Detail;
        }

        [MenuItem("Sands/Validate MVP Assets")]
        public static void Run()
        {
            List<Check> checks = new List<Check>();
            AddFile(checks, "Generated Home scene", "Assets/Scenes/Home.unity", "Run Sands > Setup MVP Home Scene.");
            AddPrefab(checks, "Original castle Mesh prefab", "Assets/Resources/HomeCastle/LegendsRebornCastle.prefab", "Run Sands > Setup MVP Home Scene to generate the original castle Mesh.");
            AddFile(checks, "Android build entry point", "Assets/Editor/BuildScript.cs", "The GameCI build method is missing.");

            string[] castle = { "Arch", "Arch_Door", "Wall_Modular", "Decorative_Wall", "Column", "Column2", "Banner", "Banner_wall", "Torch", "Stairs_Modular", "Floor_Modular" };
            for (int i = 0; i < castle.Length; i++)
                AddModel(checks, "Castle part: " + castle[i], "Assets/Resources/HomeCastle/" + castle[i] + ".fbx", "Pull main and let Unity import the FBX.");

            string[] monsters = { "Bat", "Dragon", "Skeleton", "Slime" };
            for (int i = 0; i < monsters.Length; i++)
                AddModel(checks, "Battle monster: " + monsters[i], "Assets/Resources/BattleMonsters/" + monsters[i] + ".fbx", "Pull main and let Unity import the FBX.");

            string[] nature = { "Bush_1", "Cactus_1", "Grass", "PalmTree_1", "PalmTree_2", "Rock_1" };
            for (int i = 0; i < nature.Length; i++)
                AddModel(checks, "Nature prop: " + nature[i], "Assets/Resources/HomeNature/" + nature[i] + ".fbx", "Pull main and let Unity import the FBX.");

            string[] heroes = { "Warrior", "Cleric", "Monk", "Ranger", "Rogue", "Wizard" };
            for (int i = 0; i < heroes.Length; i++)
                AddModel(checks, "Humanoid rig: " + heroes[i], "Assets/Resources/HomeHeroes/" + heroes[i] + ".fbx", "Run Legends Reborn > Art > Install MVP Art and select the legacy Free3D folder.");

            string[] portraits = { "warrior", "paladin", "berserker", "healer", "sharpshooter" };
            for (int i = 0; i < portraits.Length; i++)
                AddTexture(checks, "Hero portrait: " + portraits[i], "Assets/Resources/HeroPortraits/hero_" + portraits[i] + ".png", "The approved portrait PNG is missing.");

            AddFile(checks, "5v5 PlayMode test", "Assets/Tests/PlayMode/MvpBattlePlayModeTests.cs", "The 5v5 PlayMode test source is missing.");
            AddFile(checks, "PlayMode test assembly", "Assets/Tests/PlayMode/SandsOfLegends.Tests.PlayMode.asmdef", "The PlayMode test assembly definition is missing.");
            AddFile(checks, "EditMode content tests", "Assets/Tests/EditMode/MvpContentTests.cs", "The EditMode test source is missing.");
            AddManifestPackage(checks, "Unity Test Framework", "com.unity.test-framework", "Add com.unity.test-framework to Packages/manifest.json.");

            int present = 0;
            for (int i = 0; i < checks.Count; i++) if (checks[i].Present) present++;
            int missing = checks.Count - present;
            StringBuilder report = new StringBuilder();
            report.AppendLine("# Legends Reborn - MVP asset readiness");
            report.AppendLine();
            report.AppendLine("Generated (UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            report.AppendLine("Unity version: " + Application.unityVersion);
            report.AppendLine("Asset/import checks: " + present + " / " + checks.Count + " present; " + missing + " missing.");
            report.AppendLine();
            report.AppendLine("| Check | Status | Path |");
            report.AppendLine("|---|---|---|");
            for (int i = 0; i < checks.Count; i++)
            {
                Check check = checks[i];
                report.Append("| ").Append(check.Name).Append(" | ")
                    .Append(check.Present ? "PRESENT" : "MISSING").Append(" | ")
                    .Append(check.Path).AppendLine(" |");
                if (!check.Present) report.AppendLine("> Next step: " + check.Detail);
            }
            report.AppendLine();
            report.AppendLine("## Verification boundary");
            report.AppendLine("This report checks files and Unity asset imports only. It does not prove scripts compile, scenes render correctly, Play Mode tests pass, mobile touch works, or an APK builds.");
            report.AppendLine("Run EditMode and PlayMode suites in Unity Test Runner, then run Sands > Build Android APK after configuring Android modules and Unity credentials.");

            Directory.CreateDirectory("Reports");
            string reportPath = "Reports/MVP_ASSET_READINESS.md";
            File.WriteAllText(reportPath, report.ToString(), Encoding.UTF8);
            string summary = present + " / " + checks.Count + " checks present; " + missing + " missing. Report: " + reportPath;
            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("MVP Asset Readiness", summary, "OK");
        }

        private static void AddFile(List<Check> checks, string name, string path, string detail)
        {
            checks.Add(new Check { Name = name, Path = path, Present = File.Exists(path), Detail = detail });
        }

        private static void AddModel(List<Check> checks, string name, string path, string detail)
        {
            checks.Add(new Check { Name = name, Path = path, Present = File.Exists(path) && AssetDatabase.LoadAssetAtPath<GameObject>(path) != null, Detail = detail });
        }

        private static void AddPrefab(List<Check> checks, string name, string path, string detail)
        {
            checks.Add(new Check { Name = name, Path = path, Present = File.Exists(path) && AssetDatabase.LoadAssetAtPath<GameObject>(path) != null, Detail = detail });
        }

        private static void AddTexture(List<Check> checks, string name, string path, string detail)
        {
            checks.Add(new Check { Name = name, Path = path, Present = File.Exists(path) && AssetDatabase.LoadAssetAtPath<Texture2D>(path) != null, Detail = detail });
        }

        private static void AddManifestPackage(List<Check> checks, string name, string packageId, string detail)
        {
            string path = "Packages/manifest.json";
            string content = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            checks.Add(new Check { Name = name, Path = path, Present = content.Contains(packageId), Detail = detail });
        }
    }
}
