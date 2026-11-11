using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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

        [MenuItem("Legends Reborn/Art/Install Quaternius 3D Models")]
        public static void InstallAll()
        {
            bool acceptCurrentQuaterniusLicense = EditorUtility.DisplayDialog(
                "Quaternius asset license",
                "The current Quaternius license is not CC0. It permits commercial use inside a completed game/product, " +
                "but prohibits redistributing the FBX files as a standalone asset pack.\n\n" +
                "Continue only if you explicitly approve using these six models under those terms.",
                "I approve — continue", "Cancel");
            if (!acceptCurrentQuaterniusLicense) return;

            int downloadedHeroes = DownloadPublicHeroModels();
            int alreadyPresent = CountPresentFiles();
            int required = CountRequiredFiles();
            string root = null;

            // The six humanoid rigs come directly from the public source repository.
            // Ask for a local pack folder only if one of the already-bundled environment/monster assets is missing.
            if (alreadyPresent < required)
            {
                root = EditorUtility.OpenFolderPanel(
                    "Select the parent folder containing any missing Quaternius pack folders",
                    Application.dataPath, "");
                if (string.IsNullOrEmpty(root)) return;
            }

            int copied = downloadedHeroes;
            int present = 0;
            List<string> missing = new List<string>();
            try
            {
                for (int p = 0; p < Packs.Length; p++)
                {
                    Pack pack = Packs[p];
                    Directory.CreateDirectory(pack.Destination);
                    string sourceFolder = string.IsNullOrEmpty(root)
                        ? string.Empty
                        : Path.Combine(root, pack.Folder, "FBX");

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
                        if (string.IsNullOrEmpty(root) || !File.Exists(source) || new FileInfo(source).Length <= 2048)
                        {
                            missing.Add(pack.Folder + "/FBX/" + file);
                            continue;
                        }

                        File.Copy(source, target, true);
                        copied++;
                    }

                    File.WriteAllText(Path.Combine(pack.Destination, "SOURCE.txt"),
                        "Quaternius " + pack.Folder + " pack.\n" +
                        "Official license reference: https://quaternius.com/license.html\n" +
                        "Keep this notice with the imported files.\n");
                }

                Directory.CreateDirectory("ArtSource/ThirdParty");
                File.WriteAllText("ArtSource/ThirdParty/Quaternius_LICENSE_NOTICE.md",
                    "Quaternius third-party 3D assets — license notice\n" +
                    "===============================\n\n" +
                    "Packs used: RPG Characters, Animated Monsters, Modular Dungeon and Nature.\n" +
                    "License reference: https://quaternius.com/license.html\n\n" +
                    "Official pack pages:\n" +
                    "- https://quaternius.itch.io/lowpoly-animated-monsters\n" +
                    "- https://quaternius.itch.io/150-lowpoly-nature-models\n" +
                    "- https://quaternius.itch.io/lowpoly-modular-dungeon-pack\n");

                AssetDatabase.Refresh();
                string summary = "Already present: " + present + "\nCopied from selected Quaternius source: " + copied;
                if (missing.Count > 0)
                {
                    summary += "\n\nMissing source files (" + missing.Count + "):\n" + string.Join("\n", missing.ToArray());
                    summary += "\n\nSelect the folder containing the Quaternius pack folders, not an individual FBX folder.";
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


        private static int DownloadPublicHeroModels()
        {
            const string destination = "Assets/Resources/HomeHeroes";
            const string sourceRoot = "https://raw.githubusercontent.com/Andahmed/LegendsStyleRPG/main/" +
                "GameClient/Assets/Resources/GameArt/Free3D/Quaternius_RPGCharacters/FBX/";
            string[] models = { "Warrior", "Cleric", "Monk", "Ranger", "Rogue", "Wizard" };
            Directory.CreateDirectory(destination);
            int ready = 0;

            for (int i = 0; i < models.Length; i++)
            {
                string fileName = models[i] + ".fbx";
                string target = Path.Combine(destination, fileName);
                if (File.Exists(target) && IsBinaryFbx(File.ReadAllBytes(target)))
                {
                    ready++;
                    continue;
                }

                byte[] bytes = DownloadBytes(sourceRoot + fileName);
                if (!IsBinaryFbx(bytes))
                    throw new InvalidDataException("Rejected " + fileName + ": expected a binary FBX larger than 2048 bytes.");
                File.WriteAllBytes(target, bytes);
                ready++;
            }

            File.WriteAllText(Path.Combine(destination, "SOURCE.txt"),
                "Quaternius RPG Characters — current Quaternius License (not CC0).\\n" +
                "Commercial use inside a completed game/product is permitted; standalone asset-pack redistribution is prohibited.\\n" +
                "Files: Warrior.fbx, Cleric.fbx, Monk.fbx, Ranger.fbx, Rogue.fbx, Wizard.fbx.\\n" +
                "Source repository: https://github.com/Andahmed/LegendsStyleRPG\\n" +
                "Source path: GameClient/Assets/Resources/GameArt/Free3D/Quaternius_RPGCharacters/FBX/\\n" +
                "License reference: https://quaternius.com/license.html\\n");
            AssetDatabase.Refresh();
            return ready;
        }

        private static byte[] DownloadBytes(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 30000;
            request.ReadWriteTimeout = 30000;
            request.UserAgent = "SandsOfLegends-UnityAssetImporter";
            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (MemoryStream buffer = new MemoryStream())
            {
                if (stream == null) throw new IOException("Empty download response: " + url);
                stream.CopyTo(buffer);
                return buffer.ToArray();
            }
        }

        private static bool IsBinaryFbx(byte[] bytes)
        {
            byte[] signature = Encoding.ASCII.GetBytes("Kaydara FBX Binary");
            if (bytes == null || bytes.Length <= 2048 || bytes.Length < signature.Length) return false;
            for (int i = 0; i < signature.Length; i++)
                if (bytes[i] != signature[i]) return false;
            return true;
        }

        private static int CountPresentFiles()
        {
            int count = 0;
            for (int p = 0; p < Packs.Length; p++)
            for (int i = 0; i < Packs[p].Files.Length; i++)
            {
                string path = Path.Combine(Packs[p].Destination, Packs[p].Files[i] + ".fbx");
                if (File.Exists(path) && new FileInfo(path).Length > 2048) count++;
            }
            return count;
        }

        private static int CountRequiredFiles()
        {
            int count = 0;
            for (int p = 0; p < Packs.Length; p++) count += Packs[p].Files.Length;
            return count;
        }
    }
}