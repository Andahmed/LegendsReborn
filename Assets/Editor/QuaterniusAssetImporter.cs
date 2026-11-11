using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SandsOfLegends.Editor
{
    public static class QuaterniusAssetImporter
    {
        private sealed class Pack
        {
            public string Folder;
            public string Destination;
            public string[] Files;
            public Pack(string folder, string destination, string[] files)
            {
                Folder = folder;
                Destination = destination;
                Files = files;
            }
        }

        private static readonly Pack[] Packs =
        {
            new Pack("Quaternius_RPGCharacters", "Assets/Resources/HomeHeroes",
                new[] { "Warrior", "Cleric", "Monk", "Ranger", "Rogue", "Wizard" }),
            new Pack("Quaternius_AnimatedMonsters", "Assets/Resources/BattleMonsters",
                new[] { "Bat", "Dragon", "Skeleton", "Slime" }),
            new Pack("Quaternius_ModularDungeon", "Assets/Resources/HomeCastle",
                new[] { "Arch", "Arch_Door", "Wall_Modular", "Decorative_Wall", "Column", "Column2",
                        "Banner", "Banner_wall", "Torch", "Stairs_Modular", "Floor_Modular" }),
            new Pack("Quaternius_Nature", "Assets/Resources/HomeNature",
                new[] { "Bush_1", "Cactus_1", "Grass", "PalmTree_1", "PalmTree_2", "Rock_1" })
        };

        [MenuItem("Legends Reborn/Art/Install All CC0 3D Models")]
        public static void InstallAll()
        {
            string root = FindLocalRoot();
            if (string.IsNullOrEmpty(root))
            {
                root = EditorUtility.OpenFolderPanel(
                    "Select the Free3D folder inside LegendsStyleRPG/GameClient/Assets/Resources/GameArt",
                    Application.dataPath, "");
            }
            if (string.IsNullOrEmpty(root)) return;

            int copied = 0;
            int present = 0;
            List<string> missing = new List<string>();
            try
            {
                for (int p = 0; p < Packs.Length; p++)
                {
                    Pack pack = Packs[p];
                    string sourceFolder = Path.Combine(root, pack.Folder, "FBX");
                    Directory.CreateDirectory(pack.Destination);
                    for (int i = 0; i < pack.Files.Length; i++)
                    {
                        string file = pack.Files[i] + ".fbx";
                        string target = Path.Combine(pack.Destination, file);
                        if (File.Exists(target) && new FileInfo(target).Length > 2048)
                        {
                            present++;
                            continue;
                        }

                        string source = Path.Combine(sourceFolder, file);
                        if (!File.Exists(source) || new FileInfo(source).Length <= 2048)
                        {
                            missing.Add(pack.Folder + "/FBX/" + file);
                            continue;
                        }
                        File.Copy(source, target, true);
                        copied++;
                    }

                    File.WriteAllText(Path.Combine(pack.Destination, "SOURCE.txt"),
                        "Quaternius " + pack.Folder + " pack.\n" +
                        "Original archive: https://github.com/Andahmed/LegendsStyleRPG\n" +
                        "Source folder: GameClient/Assets/Resources/GameArt/Free3D/" + pack.Folder + "/FBX\n" +
                        "License reference: https://quaternius.com/license.html\n" +
                        "The project asset inventory records this pack as CC0. Keep this notice with the imported files.\n");
                }

                Directory.CreateDirectory("ArtSource/ThirdParty");
                File.WriteAllText("ArtSource/ThirdParty/Quaternius_CC0_NOTICE.md",
                    "Quaternius third-party 3D assets\n" +
                    "===============================\n\n" +
                    "Packs used: RPG Characters, Animated Monsters, Modular Dungeon and Nature.\n" +
                    "Project asset inventory identifies the Quaternius packs as CC0. License reference:\n" +
                    "https://quaternius.com/license.html\n\n" +
                    "Official pack pages:\n" +
                    "- https://quaternius.itch.io/lowpoly-animated-monsters\n" +
                    "- https://quaternius.itch.io/150-lowpoly-nature-models\n" +
                    "- https://quaternius.itch.io/lowpoly-modular-dungeon-pack\n\n" +
                    "Source archive: https://github.com/Andahmed/LegendsStyleRPG\n" +
                    "The character and environment FBX files are used as game assets, not redistributed as a standalone asset pack.\n");

                AssetDatabase.Refresh();
                string summary = "Already present: " + present + "\nCopied from local source: " + copied;
                if (missing.Count > 0)
                {
                    summary += "\n\nMissing source files (" + missing.Count + "):\n" + string.Join("\n", missing.ToArray());
                    summary += "\n\nChoose the Free3D parent folder, not an individual FBX folder.";
                }
                else summary += "\n\nAll requested FBX files are available in Assets/Resources.";
                EditorUtility.DisplayDialog("Quaternius 3D Models", summary, "OK");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("3D model import failed", ex.Message, "Close");
            }
        }

        private static string FindLocalRoot()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string[] candidates =
            {
                Path.GetFullPath(Path.Combine(projectRoot, "..", "LegendsStyleRPG", "GameClient", "Assets", "Resources", "GameArt", "Free3D")),
                Path.GetFullPath(Path.Combine(projectRoot, "..", "..", "LegendsStyleRPG", "GameClient", "Assets", "Resources", "GameArt", "Free3D"))
            };
            for (int i = 0; i < candidates.Length; i++)
                if (Directory.Exists(Path.Combine(candidates[i], "Quaternius_RPGCharacters", "FBX")))
                    return candidates[i];
            return null;
        }
    }
}
