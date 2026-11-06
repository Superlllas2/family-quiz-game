using UnityEditor;
using UnityEngine;

namespace FamilyQuiz.EditorTools
{
    /// <summary>
    /// Import settings for the background music (decisions/0019): long tracks stream from disk as Vorbis instead of
    /// being decompressed into memory (five tracks would take about 250 MB decompressed).
    /// </summary>
    public sealed class MusicImport : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Resources/Music/")) return;
            var importer = (AudioImporter)assetImporter;
            importer.forceToMono = false;
            importer.loadInBackground = true;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.6f;
            settings.preloadAudioData = false;
            importer.defaultSampleSettings = settings;
        }
    }
}
