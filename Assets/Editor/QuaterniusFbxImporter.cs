using System;
using UnityEditor;

namespace SandsOfLegends.Editor
{
    /// <summary>
    /// Imports the Quaternius humanoid and monster FBXs as Generic animated rigs.
    /// Adapted from the legacy project's QuaterniusFbxImporter; scoped to this project's Resources folders.
    /// </summary>
    public sealed class QuaterniusFbxImporter : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            string path = assetPath ?? string.Empty;
            bool isCharacterModel =
                path.StartsWith("Assets/Resources/HomeHeroes/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("Assets/Resources/BattleMonsters/", StringComparison.OrdinalIgnoreCase);
            if (!isCharacterModel) return;

            ModelImporter importer = (ModelImporter)assetImporter;
            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.isReadable = false;
        }
    }
}
