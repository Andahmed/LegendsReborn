using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace SandsOfLegends.Editor
{
    public static class MVPArtInstaller
    {
        private const string HeroUrl = "https://raw.githubusercontent.com/Andahmed/LegendsStyleRPG/main/GameClient/Assets/Resources/GameArt/Free3D/Quaternius_RPGCharacters/FBX/";
        private const string MonsterUrl = "https://raw.githubusercontent.com/Andahmed/LegendsStyleRPG/main/GameClient/Assets/Resources/GameArt/Free3D/Quaternius_AnimatedMonsters/FBX/";
        private static readonly string[] Heroes = { "Warrior", "Cleric", "Monk", "Ranger", "Rogue", "Wizard" };
        private static readonly string[] Monsters = { "Skeleton", "Slime", "Bat", "Dragon" };

        [MenuItem("Legends Reborn/Art/Install MVP Art")]
        public static void Install()
        {
            try
            {
                int count = DownloadSet(HeroUrl, "Assets/Resources/HomeHeroes", Heroes);
                count += DownloadSet(MonsterUrl, "Assets/Resources/BattleMonsters", Monsters);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("MVP Art Installed",
                    "Imported " + count + " FBX models. Unity is processing the assets. Re-enter Play mode when import finishes.", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Art install failed",
                    ex.Message + "\nCheck network access, then run Install MVP Art again.", "Close");
            }
        }

        private static int DownloadSet(string baseUrl, string folder, string[] names)
        {
            Directory.CreateDirectory(folder);
            int count = 0;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent", "LegendsReborn-MVP-Art-Installer");
                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i] + ".fbx";
                    string target = Path.Combine(folder, name);
                    if (File.Exists(target) && new FileInfo(target).Length > 2048) continue;
                    string temp = target + ".download";
                    client.DownloadFile(baseUrl + name, temp);
                    if (!File.Exists(temp) || new FileInfo(temp).Length <= 2048)
                    {
                        if (File.Exists(temp)) File.Delete(temp);
                        throw new IOException("FBX download is incomplete: " + name);
                    }
                    if (File.Exists(target)) File.Delete(target);
                    File.Move(temp, target);
                    count++;
                }
            }
            File.WriteAllText(Path.Combine(folder, "SOURCE.txt"),
                "Quaternius low-poly character/monster models.\n" +
                "Source archive: https://github.com/Andahmed/LegendsStyleRPG\n" +
                "Character pack path: GameClient/Assets/Resources/GameArt/Free3D/Quaternius_RPGCharacters/FBX\n" +
                "Monster pack path: GameClient/Assets/Resources/GameArt/Free3D/Quaternius_AnimatedMonsters/FBX\n" +
                "Keep the source pack license record with any public distribution.\n");
            return count;
        }
    }
}
