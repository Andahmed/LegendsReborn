using UnityEditor;

namespace SandsOfLegends.Editor
{
    public static class MVPArtInstaller
    {
        [MenuItem("Legends Reborn/Art/Install MVP Art")]
        public static void Install()
        {
            QuaterniusAssetImporter.InstallAll();
        }
    }
}
