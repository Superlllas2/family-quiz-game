using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FamilyQuiz.Bootstrap;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace FamilyQuiz.EditorTools
{
    /// <summary>
    /// One time project setup, safe to run again. Run from the command line (no -quit; it exits by itself):
    /// Unity -batchmode -projectPath unity -executeMethod FamilyQuiz.EditorTools.ProjectSetup.Run -logFile setup.log
    /// The first run on a fresh project only imports the TextMeshPro Essential Resources and exits;
    /// run it a second time for the fonts, the TMP settings and the scene.
    ///
    /// Fonts: "Inter-Regular SDF" and "Inter-Bold SDF" are static assets with Latin, digits, punctuation and
    /// Cyrillic (А to я, Ё, ё) baked into their atlas, so they never change in git. Both fall back to
    /// "Inter-Regular Dynamic SDF", a dynamic asset that adds any other character at runtime and is cleared
    /// by TextMeshPro when the editor quits.
    /// </summary>
    public static class ProjectSetup
    {
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        private const string FontFolder = "Assets/Fonts/Inter";
        private const string RegularFontPath = FontFolder + "/Inter-Regular.ttf";
        private const string BoldFontPath = FontFolder + "/Inter-Bold.ttf";
        private const string RegularAssetPath = FontFolder + "/Inter-Regular SDF.asset";
        private const string BoldAssetPath = FontFolder + "/Inter-Bold SDF.asset";
        private const string FallbackAssetPath = FontFolder + "/Inter-Regular Dynamic SDF.asset";
        private const string ScenePath = "Assets/Scenes/Main.unity";

        // SDF sampling 64 pt with padding 6 (the same 10 % ratio as TextMeshPro's default 90/9) in one 1024 atlas.
        private const int SamplingPointSize = 64;
        private const int AtlasPadding = 6;
        private const int AtlasSize = 1024;

        /// <summary>Index of the Bold (700) entry in a TMP font weight table.</summary>
        private const int BoldWeightIndex = 7;

        private static readonly Color CameraBackground = new Color32(0x14, 0x16, 0x1C, 0xFF);

        [MenuItem("Family Quiz/Run project setup")]
        public static void RunFromMenu()
        {
            if (!ImportTmpEssentialsIfMissing(false)) RunRest();
        }

        /// <summary>Entry point for -executeMethod. Exits Unity when done.</summary>
        public static void Run()
        {
            if (ImportTmpEssentialsIfMissing(true)) return;
            try
            {
                RunRest();
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Project setup failed: " + e);
                EditorApplication.Exit(1);
            }
        }

        private static void RunRest()
        {
            var fallback = EnsureDynamicFontAsset(RegularFontPath, FallbackAssetPath);
            var regular = EnsureStaticFontAsset(RegularFontPath, RegularAssetPath);
            var bold = EnsureStaticFontAsset(BoldFontPath, BoldAssetPath);
            if (regular == null) throw new System.InvalidOperationException("Could not create the Inter font asset");
            SetFallback(regular, fallback);
            if (bold != null)
            {
                SetFallback(bold, fallback);
                LinkBold(regular, bold);
            }

            SetDefaultFont(regular);
            CreateScene();
            AssetDatabase.SaveAssets();
            Debug.Log("Project setup done.");
        }

        /// <summary>True when the import was started (the run ends when it completes).</summary>
        private static bool ImportTmpEssentialsIfMissing(bool exitWhenDone)
        {
            if (File.Exists(TmpSettingsPath)) return false;
            Debug.Log("Importing the TextMeshPro Essential Resources. Run the setup again when this run ends.");
            AssetDatabase.importPackageCompleted += name =>
            {
                Debug.Log("Imported " + name + ". Run the setup again for the font, TMP settings and scene.");
                if (exitWhenDone) EditorApplication.Exit(0);
            };
            AssetDatabase.importPackageFailed += (name, error) =>
            {
                Debug.LogError("Import of " + name + " failed: " + error);
                if (exitWhenDone) EditorApplication.Exit(1);
            };
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            return true;
        }

        private static TMP_FontAsset EnsureStaticFontAsset(string fontPath, string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null && existing.atlasPopulationMode == AtlasPopulationMode.Static && existing.atlasWidth == AtlasSize)
            {
                ReportMissing(existing, assetPath, !existing.HasCharacters(CharacterSet(), out List<char> missing) ? new string(missing.ToArray()) : null);
                return existing;
            }

            if (existing != null)
            {
                Debug.Log("Replacing font asset " + assetPath + " (old settings)");
                AssetDatabase.DeleteAsset(assetPath);
            }

            var asset = NewFontAsset(fontPath, assetPath);
            if (asset == null) return null;
            asset.TryAddCharacters(CharacterSet(), out var notAdded);
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(asset.atlasTextures[0]);
            AssetDatabase.SaveAssets();
            ReportMissing(asset, assetPath, notAdded);
            Debug.Log("Created static font asset " + assetPath + " with " + asset.characterTable.Count + " characters");
            return asset;
        }

        private static TMP_FontAsset EnsureDynamicFontAsset(string fontPath, string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) return existing;
            var asset = NewFontAsset(fontPath, assetPath);
            if (asset == null) return null;
            AssetDatabase.SaveAssets();
            Debug.Log("Created dynamic fallback font asset " + assetPath);
            return asset;
        }

        private static TMP_FontAsset NewFontAsset(string fontPath, string assetPath)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                Debug.LogError("Font file not found: " + fontPath);
                return null;
            }

            var asset = TMP_FontAsset.CreateFontAsset(font, SamplingPointSize, AtlasPadding, GlyphRenderMode.SDFAA,
                AtlasSize, AtlasSize, AtlasPopulationMode.Dynamic, false);
            if (asset == null)
            {
                Debug.LogError("TextMeshPro could not load " + fontPath);
                return null;
            }

            var baseName = Path.GetFileNameWithoutExtension(assetPath);
            asset.name = baseName;
            asset.atlasTextures[0].name = baseName + " Atlas";
            asset.material.name = baseName + " Material";
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            return asset;
        }

        private static void ReportMissing(TMP_FontAsset asset, string assetPath, string missing)
        {
            if (string.IsNullOrEmpty(missing))
            {
                Debug.Log("Font asset " + assetPath + " has every seeded character");
                return;
            }

            var codes = string.Join(" ", missing.Select(c => "U+" + ((int)c).ToString("X4")));
            Debug.LogWarning("Font asset " + assetPath + " is missing characters: " + codes);
        }

        /// <summary>Latin, digits and punctuation, Cyrillic А to я with Ё and ё, and typographic marks.</summary>
        private static string CharacterSet()
        {
            var sb = new StringBuilder();
            for (int c = 0x20; c <= 0x7E; c++) sb.Append((char)c);
            for (int c = 0x0410; c <= 0x044F; c++) sb.Append((char)c);
            sb.Append('Ё').Append('ё'); // Ё ё
            sb.Append("«»—–…·№“”„‘’°× ");
            return sb.ToString();
        }

        private static void SetFallback(TMP_FontAsset asset, TMP_FontAsset fallback)
        {
            if (fallback == null) return;
            if (asset.fallbackFontAssetTable == null) asset.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (asset.fallbackFontAssetTable.Contains(fallback)) return;
            asset.fallbackFontAssetTable.Add(fallback);
            EditorUtility.SetDirty(asset);
        }

        private static void LinkBold(TMP_FontAsset regular, TMP_FontAsset bold)
        {
            var table = regular.fontWeightTable;
            if (table == null || table.Length <= BoldWeightIndex) return;
            if (table[BoldWeightIndex].regularTypeface == bold) return;
            // The property returns the asset's own array, so the element is changed in place.
            table[BoldWeightIndex].regularTypeface = bold;
            EditorUtility.SetDirty(regular);
        }

        private static void SetDefaultFont(TMP_FontAsset font)
        {
            var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
            if (settings == null) throw new System.InvalidOperationException("TMP Settings not found at " + TmpSettingsPath);
            var so = new SerializedObject(settings);
            var property = so.FindProperty("m_defaultFontAsset");
            if (property == null) throw new System.InvalidOperationException("TMP Settings has no m_defaultFontAsset");
            if (property.objectReferenceValue == font)
            {
                Debug.Log("Default TMP font is already Inter");
                return;
            }

            property.objectReferenceValue = font;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            Debug.Log("Default TMP font set to " + font.name);
        }

        private static void CreateScene()
        {
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                var camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = CameraBackground;
                cameraObject.AddComponent<AudioListener>();
                new GameObject("Bootstrap").AddComponent<GameBootstrap>();
                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log("Created " + ScenePath);
            }

            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 1 && scenes[0].path == ScenePath) return;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("Build settings: " + ScenePath + " is the only scene");
        }
    }
}
