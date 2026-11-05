using System.IO;
using UnityEditor;
using UnityEngine;

namespace FamilyQuiz.EditorTools.Studio
{
    /// <summary>
    /// Prepares the photo scanned CC0 sets that tools/fetch_textures.mjs puts in Assets/Game/Art/Textures/Scanned:
    /// import settings per map, a packed mask per set (URP Lit's metallic map: R metallic 0, A smoothness = 1 - rough),
    /// a grey copy of an albedo that the set re-tints (velvet, wool), and one soft variation map that breaks the visible
    /// repeat of the floor and walls. The generated files are committed with the scans, since the scene uses them.
    /// </summary>
    public static class StudioScans
    {
        public const string Folder = "Assets/Game/Art/Textures/Scanned";
        public const string Variation = "Variation";
        /// <summary>A flat normal map: URP Lit only applies a detail normal when the material has a normal map.</summary>
        public const string FlatNormal = "Flat_nor";
        private const int MaskSize = 1024;

        /// <summary>The sets that get a grey albedo copy (their colour comes from the material tint).</summary>
        public static readonly string[] Greyed = { "velour_velvet", "poly_wool_herringbone", "dirty_carpet" };

        /// <summary>Mean brightness of a grey copy, so a tint of target / GreyMean gives the target colour.</summary>
        public const float GreyMean = 0.8f;

        public static bool Has(string set)
        {
            return File.Exists(Folder + "/" + set + "_diff.jpg");
        }

        public static void PrepareAll(bool force)
        {
            if (!Directory.Exists(Folder))
            {
                Debug.LogWarning("Studio: no scanned textures in " + Folder + "; run node tools/fetch_textures.mjs");
                return;
            }

            AssetDatabase.Refresh();
            foreach (var file in Directory.GetFiles(Folder, "*_diff.jpg"))
            {
                string set = Path.GetFileName(file).Replace("_diff.jpg", "");
                string mask = Folder + "/" + set + "_mask.png";
                if (force || !File.Exists(mask)) WriteMask(set, mask);
                if (System.Array.IndexOf(Greyed, set) >= 0)
                {
                    string grey = Folder + "/" + set + "_grey.jpg";
                    if (force || !File.Exists(grey)) WriteGrey(set, grey);
                }
            }

            string variation = Folder + "/" + Variation + ".png";
            if (force || !File.Exists(variation)) WriteVariation(variation);
            string flat = Folder + "/" + FlatNormal + ".png";
            if (!File.Exists(flat))
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGB24, false, true);
                var px = new Color32[16];
                for (int i = 0; i < 16; i++) px[i] = new Color32(128, 128, 255, 255);
                tex.SetPixels32(px);
                File.WriteAllBytes(flat, tex.EncodeToPNG());
            }
            AssetDatabase.Refresh();

            foreach (var file in Directory.GetFiles(Folder))
            {
                string path = file.Replace('\\', '/');
                if (path.EndsWith(".meta") || path.EndsWith(".md")) continue;
                Configure(path);
            }
        }

        private static Texture2D Read(string path)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            tex.LoadImage(File.ReadAllBytes(path));
            return tex;
        }

        private static Texture2D Resized(Texture2D source, int size)
        {
            if (source.width == size) return source;
            var rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(source, rt);
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var result = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            result.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        private static void WriteMask(string set, string path)
        {
            var rough = Resized(Read(Folder + "/" + set + "_rough.jpg"), MaskSize);
            var px = rough.GetPixels32();
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 255, 0, (byte)(255 - px[i].r));
            var mask = new Texture2D(MaskSize, MaskSize, TextureFormat.RGBA32, false, true);
            mask.SetPixels32(px);
            File.WriteAllBytes(path, mask.EncodeToPNG());
        }

        private static void WriteGrey(string set, string path)
        {
            var diff = Read(Folder + "/" + set + "_diff.jpg");
            var px = diff.GetPixels();
            double sum = 0;
            var lum = new float[px.Length];
            for (int i = 0; i < px.Length; i++)
            {
                // Luminance of the sRGB pixel, linearized so the tint math happens in the shader's space.
                var c = px[i].linear;
                lum[i] = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
                sum += lum[i];
            }

            float scale = GreyMean / Mathf.Max(0.001f, (float)(sum / px.Length));
            for (int i = 0; i < px.Length; i++)
            {
                float v = Mathf.Clamp01(lum[i] * scale);
                px[i] = new Color(v, v, v).gamma;
            }

            var grey = new Texture2D(diff.width, diff.height, TextureFormat.RGB24, false);
            grey.SetPixels(px);
            File.WriteAllBytes(path, grey.EncodeToJPG(92));
        }

        /// <summary>Soft blotches around mid grey (URP's detail albedo multiplies by twice the map), tiling.</summary>
        private static void WriteVariation(string path)
        {
            const int size = 512;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (float)x / size, v = (float)y / size;
                float n = 0f, amp = 0.5f, total = 0f;
                for (int o = 0; o < 4; o++)
                {
                    int f = 3 << o;
                    n += amp * TileNoise(u * f, v * f, f, 17 + o);
                    total += amp;
                    amp *= 0.5f;
                }

                n /= total;
                float g = Mathf.Lerp(0.40f, 0.58f, n);
                px[y * size + x] = new Color(g, g, g);   // stored linear: URP wants detail albedo with sRGB off
            }

            var tex = new Texture2D(size, size, TextureFormat.RGB24, false);
            tex.SetPixels(px);
            File.WriteAllBytes(path, tex.EncodeToPNG());
        }

        private static float TileNoise(float x, float y, int period, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3 - 2 * fx);
            fy = fy * fy * (3 - 2 * fy);
            float a = Hash(x0 % period, y0 % period, seed), b = Hash((x0 + 1) % period, y0 % period, seed);
            float c = Hash(x0 % period, (y0 + 1) % period, seed), d = Hash((x0 + 1) % period, (y0 + 1) % period, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        private static float Hash(int x, int y, int seed)
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 2147483647);
            h = (h ^ (h >> 13)) * 1274126177u;
            return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
        }

        /// <summary>Import settings by file name; reimports only when something differs.</summary>
        private static void Configure(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            string name = Path.GetFileNameWithoutExtension(path);
            bool normal = name.EndsWith("_nor");
            bool linear = name.EndsWith("_rough") || name.EndsWith("_mask") || name == Variation;
            bool big = name.StartsWith("wood_floor_worn") || name.StartsWith("dark_wood") || name.StartsWith("dark_wooden_planks")
                       || name == Variation;
            var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            bool srgb = !normal && !linear;
            int maxSize = name.EndsWith("_rough") ? 256 : 2048;
            int aniso = big ? 16 : 8;
            if (importer.textureType == type && importer.sRGBTexture == srgb && importer.maxTextureSize == maxSize
                && importer.anisoLevel == aniso && importer.mipmapEnabled
                && importer.textureCompression == TextureImporterCompression.CompressedHQ) return;
            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = maxSize;
            importer.anisoLevel = aniso;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
    }
}
