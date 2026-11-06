using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace FamilyQuiz.EditorTools
{
    /// <summary>
    /// The game night fonts of the TV (decisions/0017_game_night_ui.md): Unbounded ExtraBold for headings, numbers and
    /// tile letters, Nunito ExtraBold and Black for everything else, Caveat Bold for the odd hand written word. All three
    /// are OFL Google Fonts with Cyrillic (licences beside the files in Assets/Fonts/GameNight). Each gets a static TMP
    /// font asset with Latin, Cyrillic and typographic marks baked in, under a Resources folder so the TV loads them by
    /// name (TvFonts), falling back to the Inter dynamic asset for anything else. Nunito ExtraBold becomes the default
    /// TMP font. Safe to run again. Run (no -quit; it exits by itself):
    /// Unity -batchmode -projectPath unity -executeMethod FamilyQuiz.EditorTools.GameNightFonts.Run -logFile fonts.log
    /// </summary>
    public static class GameNightFonts
    {
        private const string Folder = "Assets/Fonts/GameNight";
        private const string AssetFolder = Folder + "/Resources/Fonts";
        private const string FallbackPath = "Assets/Fonts/Inter/Inter-Regular Dynamic SDF.asset";
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        /// <summary>The TTF names; the asset of each is "&lt;name&gt; SDF".</summary>
        public static readonly string[] Fonts = { "Unbounded-ExtraBold", "Nunito-ExtraBold", "Nunito-Black", "Caveat-Bold" };

        private const string DefaultFont = "Nunito-ExtraBold";

        // Sampled at 56 pt with a 6 px padding (SDF scales up cleanly to the big headings) in one 2048 x 1024 atlas: about
        // 400 glyph cells, so every seeded character fits and each asset stays about 4 MB in git.
        private const int SamplingPointSize = 56;
        private const int AtlasPadding = 6;
        private const int AtlasWidth = 2048;
        private const int AtlasHeight = 1024;

        [MenuItem("Family Quiz/Make game night fonts")]
        public static void RunFromMenu()
        {
            Make();
        }

        public static void Run()
        {
            try
            {
                Make();
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Game night fonts failed: " + e);
                EditorApplication.Exit(1);
            }
        }

        private static void Make()
        {
            if (!AssetDatabase.IsValidFolder(Folder + "/Resources")) AssetDatabase.CreateFolder(Folder, "Resources");
            if (!AssetDatabase.IsValidFolder(AssetFolder)) AssetDatabase.CreateFolder(Folder + "/Resources", "Fonts");
            var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackPath);
            TMP_FontAsset defaultAsset = null;
            foreach (var name in Fonts)
            {
                var asset = MakeStatic(Folder + "/" + name + ".ttf", AssetFolder + "/" + name + " SDF.asset");
                if (asset == null) throw new System.InvalidOperationException("Could not make the font asset of " + name);
                if (fallback != null)
                {
                    if (asset.fallbackFontAssetTable == null) asset.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    if (!asset.fallbackFontAssetTable.Contains(fallback)) asset.fallbackFontAssetTable.Add(fallback);
                    EditorUtility.SetDirty(asset);
                }

                if (name == DefaultFont) defaultAsset = asset;
            }

            var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
            var so = new SerializedObject(settings);
            so.FindProperty("m_defaultFontAsset").objectReferenceValue = defaultAsset;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("Game night fonts done; default TMP font is " + DefaultFont);
        }

        private static TMP_FontAsset MakeStatic(string fontPath, string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) AssetDatabase.DeleteAsset(assetPath);
            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                Debug.LogError("Font file not found: " + fontPath);
                return null;
            }

            var asset = TMP_FontAsset.CreateFontAsset(font, SamplingPointSize, AtlasPadding, GlyphRenderMode.SDFAA,
                AtlasWidth, AtlasHeight, AtlasPopulationMode.Dynamic, false);
            if (asset == null) return null;
            var baseName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            asset.name = baseName;
            asset.atlasTextures[0].name = baseName + " Atlas";
            asset.material.name = baseName + " Material";
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            asset.TryAddCharacters(CharacterSet(), out var notAdded);
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(asset.atlasTextures[0]);
            AssetDatabase.SaveAssets();
            if (!string.IsNullOrEmpty(notAdded))
                Debug.Log(assetPath + " lacks " + string.Join(" ", notAdded.Select(c => "U+" + ((int)c).ToString("X4"))) + " (the Inter fallback draws them)");
            Debug.Log("Made " + assetPath + " with " + asset.characterTable.Count + " characters");
            return asset;
        }

        /// <summary>Latin with its accents, Cyrillic, digits, punctuation and the marks the TV draws.</summary>
        private static string CharacterSet()
        {
            var sb = new StringBuilder();
            for (int c = 0x20; c <= 0x7E; c++) sb.Append((char)c);
            for (int c = 0xA0; c <= 0xFF; c++) sb.Append((char)c);
            for (int c = 0x0400; c <= 0x045F; c++) sb.Append((char)c);
            sb.Append("«»—–…·№“”„‘’°×−→←↑↓•₽€");
            return sb.ToString();
        }
    }
}
