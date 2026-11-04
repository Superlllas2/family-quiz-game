using UnityEditor;

namespace FamilyQuiz.EditorTools.Studio
{
    /// <summary>
    /// Import settings for the Blender models in Assets/Game/Art/Models (0015): real scale, the normals Blender
    /// wrote (hardened bevels), lightmap UVs for the bake, no animation.
    /// </summary>
    public sealed class StudioModelImport : AssetPostprocessor
    {
        private const string ModelFolder = "Assets/Game/Art/Models/";

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ModelFolder)) return;
            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.bakeAxisConversion = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.generateSecondaryUV = true;
            importer.secondaryUVHardAngle = 70f;
            importer.secondaryUVPackMargin = 8f;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
        }
    }
}
