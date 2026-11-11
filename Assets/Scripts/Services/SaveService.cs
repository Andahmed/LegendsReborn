// =============================================================================
// FILE: Assets/Scripts/Services/SaveService.cs
// OWNER ROLE: Client Programmer - Systems
// RESPONSIBILITY: Reads/writes the JSON save in persistentDataPath with .tmp + .bak safety.
// WORK ON: Add local encryption/tamper check, cloud sync with the server, corrupted-save recovery UI.
// STATUS: Implemented
// =============================================================================
using System.IO;
using UnityEngine;
using SandsOfLegends.Meta;

namespace SandsOfLegends.Services
{
    public sealed class SaveService
    {
        private readonly string _path;

        public SaveService(string fileName = "save.json")
        {
            _path = Path.Combine(Application.persistentDataPath, fileName);
        }

        public SaveData Load()
        {
            foreach (var p in new[] { _path, _path + ".bak" })
            {
                try
                {
                    if (!File.Exists(p)) continue;
                    var d = JsonUtility.FromJson<SaveData>(File.ReadAllText(p));
                    if (d != null) return SaveMigrations.Migrate(d);
                }
                catch (System.Exception e) { Debug.LogWarning("[Save] failed to read " + p + ": " + e.Message); }
            }
            return null;
        }

        public void Save(SaveData data)
        {
            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data));
            if (File.Exists(_path)) { File.Copy(_path, _path + ".bak", true); File.Delete(_path); }
            File.Move(tmp, _path);
        }
    }
}
