using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace SandsOfLegends.Editor
{
    public static class HomeCastleAssetInstaller
    {
        private const string BaseUrl = "https://raw.githubusercontent.com/Andahmed/LegendsStyleRPG/main/GameClient/Assets/Resources/GameArt/Free3D/Quaternius_ModularDungeon/FBX/";
        private const string TargetFolder = "Assets/Resources/HomeCastle";

        private static readonly string[] Files =
        {
            "Arch_Door.fbx",
            "Arch.fbx",
            "Wall_Modular.fbx",
            "Decorative_Wall.fbx",
            "Column.fbx",
            "Column2.fbx",
            "Banner.fbx",
            "Banner_wall.fbx",
            "Torch.fbx",
            "Stairs_Modular.fbx",
            "Floor_Modular.fbx"
        };

        [MenuItem("Legends Reborn/Art/Install Home Castle CC0 Models")]
        public static void Install()
        {
            Directory.CreateDirectory(TargetFolder);
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "LegendsReborn-Unity-Asset-Installer");
                    foreach (string file in Files)
                    {
                        string destination = Path.Combine(TargetFolder, file);
                        if (File.Exists(destination) && new FileInfo(destination).Length > 1024)
                            continue;

                        client.DownloadFile(BaseUrl + file, destination);
                        if (!File.Exists(destination) || new FileInfo(destination).Length <= 1024)
                            throw new IOException("Downloaded asset is missing or incomplete: " + file);
                    }
                }

                File.WriteAllText(Path.Combine(TargetFolder, "LICENSE.txt"),
                    "Quaternius Modular Dungeon Pack\n" +
                    "Source: https://quaternius.itch.io/lowpoly-modular-dungeon-pack\n" +
                    "License: CC0 1.0 (public domain dedication), as listed by the official Quaternius pack page.\n" +
                    "Original files are downloaded from the LegendsStyleRPG asset archive.\n");
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Home Castle Models",
                    "Downloaded " + Files.Length + " CC0 FBX models. Unity is importing them now. Re-enter Play mode after import.", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Home Castle Model Install Failed",
                    ex.Message + "\nCheck internet access and run the menu command again.", "Close");
                Debug.LogException(ex);
            }
        }
    }
}