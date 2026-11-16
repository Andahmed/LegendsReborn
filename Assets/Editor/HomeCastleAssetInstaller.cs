using UnityEditor;

namespace SandsOfLegends.Editor
{
    public static class HomeCastleAssetInstaller
    {
        [MenuItem("Legends Reborn/Art/Install Home Castle Models")]
        public static void Install()
        {
            QuaterniusAssetImporter.InstallAll();
        }
    }
}
