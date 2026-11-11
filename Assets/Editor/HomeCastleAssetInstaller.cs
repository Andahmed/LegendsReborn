using UnityEditor;

namespace SandsOfLegends.Editor
{
    public static class HomeCastleAssetInstaller
    {
        [MenuItem("Legends Reborn/Art/Install Home Castle CC0 Models")]
        public static void Install()
        {
            QuaterniusAssetImporter.InstallAll();
        }
    }
}
