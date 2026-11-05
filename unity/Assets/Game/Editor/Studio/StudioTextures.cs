using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FamilyQuiz.EditorTools.Studio
{
    /// <summary>
    /// Procedural textures for the studio set (docs/21_staging.md, "Look"): wood, fabric, velvet, the rugs, the board
    /// and the invented evening city behind the windows (D93). Generated from a fixed seed into
    /// Assets/Game/Art/Textures, so no texture is downloaded and the set looks the same on every machine.
    /// </summary>
    public static class StudioTextures
    {
        public const string Folder = "Assets/Game/Art/Textures";

        public static void GenerateAll()
        {
            Directory.CreateDirectory(Folder);
            Floor();
            Wood("WoodPanel", 1024, new Color(0.30f, 0.17f, 0.10f), new Color(0.20f, 0.11f, 0.06f), 7f, 0.55f, 11);
            Wood("WoodDark", 512, new Color(0.22f, 0.13f, 0.08f), new Color(0.13f, 0.075f, 0.045f), 9f, 0.5f, 12);
            Wood("WoodWarm", 512, new Color(0.55f, 0.32f, 0.17f), new Color(0.38f, 0.2f, 0.1f), 14f, 0.45f, 13);
            Fabric("Fabric", 512, 64, 21);
            Velvet();
            Rug("Rug", 1024, 512, new Color(0.45f, 0.11f, 0.10f), new Color(0.10f, 0.14f, 0.26f), new Color(0.80f, 0.68f, 0.48f), 31);
            Rug("Rug2", 1024, 1024, new Color(0.20f, 0.24f, 0.30f), new Color(0.48f, 0.20f, 0.12f), new Color(0.74f, 0.62f, 0.44f), 32);
            City();
            Board();
            Noise("Leaf", 256, new Color(0.18f, 0.32f, 0.12f), new Color(0.10f, 0.22f, 0.07f), 41);
            AssetDatabase.Refresh();
        }

        // ---------------------------------------------------------------- generators

        /// <summary>Worn plank floor, 2 m x 2 m: planks 0.18 m wide with random lengths, dark seams.</summary>
        private static void Floor()
        {
            const int size = 2048;
            const float metres = 2f;
            var rng = new System.Random(1);
            var albedo = new Color[size * size];
            var height = new float[size * size];
            int rows = Mathf.RoundToInt(metres / 0.18f); // 11 planks across
            float rowW = size / (float)rows;
            var offsets = new float[rows];
            var lengths = new float[rows];
            for (int r = 0; r < rows; r++)
            {
                offsets[r] = (float)rng.NextDouble() * size;
                lengths[r] = size * (0.6f + (float)rng.NextDouble() * 0.5f);
            }

            var baseA = new Color(0.46f, 0.27f, 0.15f);
            var baseB = new Color(0.30f, 0.16f, 0.08f);
            for (int y = 0; y < size; y++)
            {
                int row = Mathf.Min(rows - 1, (int)(y / rowW));
                float inRow = y - row * rowW;
                for (int x = 0; x < size; x++)
                {
                    float along = (x + offsets[row]) % size;
                    int plank = (int)(along / lengths[row]) + row * 31;
                    float inPlank = along % lengths[row];
                    float tone = Hash(plank) * 0.8f + 0.1f;
                    float grain = Mathf.PerlinNoise(x * 0.004f + plank * 7.1f, y * 0.06f + plank) * 0.6f
                                  + Mathf.PerlinNoise(x * 0.02f, y * 0.25f + plank * 3f) * 0.4f;
                    float ring = Mathf.Abs(Mathf.Sin((y * 0.12f + Mathf.PerlinNoise(x * 0.003f, plank) * 9f)));
                    float t = Mathf.Clamp01(tone * 0.6f + grain * 0.35f + ring * 0.12f);
                    var c = Color.Lerp(baseB, baseA, t);
                    // Wear: lighter, flatter patches where people walk.
                    float wear = Mathf.PerlinNoise(x * 0.0012f + 5f, y * 0.0012f + 9f);
                    c = Color.Lerp(c, c * 1.18f, Mathf.Clamp01((wear - 0.55f) * 3f));
                    float seam = Mathf.Min(inRow, rowW - inRow) < 2.2f || Mathf.Min(inPlank, lengths[row] - inPlank) < 2.2f ? 1f : 0f;
                    c = Color.Lerp(c, new Color(0.07f, 0.04f, 0.025f), seam * 0.85f);
                    albedo[y * size + x] = c;
                    height[y * size + x] = 1f - seam + grain * 0.08f;
                }
            }

            Save("FloorWood", albedo, size, size, true);
            SaveNormal("FloorWood_N", height, size, size, 2.5f);
        }

        /// <summary>Wood with lengthwise grain (vertical in the texture), one texture per metre.</summary>
        private static void Wood(string name, int size, Color light, Color dark, float rings, float contrast, int seed)
        {
            var albedo = new Color[size * size];
            var height = new float[size * size];
            float s = seed * 13.7f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)size, v = y / (float)size;
                float warp = Mathf.PerlinNoise(u * 3f + s, v * 0.6f) * 2.5f;
                float ring = Mathf.Abs(Mathf.Sin((u * rings + warp) * Mathf.PI));
                float fine = Mathf.PerlinNoise(u * 180f + s, v * 6f);
                float t = Mathf.Clamp01(Mathf.Pow(ring, 0.7f) * contrast + fine * 0.35f + Mathf.PerlinNoise(u * 2f, v * 2f + s) * 0.25f);
                albedo[y * size + x] = Color.Lerp(dark, light, t);
                height[y * size + x] = fine * 0.5f + ring * 0.2f;
            }

            Save(name, albedo, size, size, true);
            SaveNormal(name + "_N", height, size, size, 1.2f);
        }

        /// <summary>A light grey woven fabric (tinted by each material), one texture per 0.25 m.</summary>
        private static void Fabric(string name, int size, int threads, int seed)
        {
            var albedo = new Color[size * size];
            var height = new float[size * size];
            float period = size / (float)threads;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float wx = Mathf.Sin(x / period * Mathf.PI * 2f);
                float wy = Mathf.Sin(y / period * Mathf.PI * 2f);
                bool over = ((int)(x / period) + (int)(y / period)) % 2 == 0;
                float h = over ? 0.5f + 0.5f * Mathf.Abs(wy) : 0.5f + 0.5f * Mathf.Abs(wx);
                float slub = Mathf.PerlinNoise(x * 0.03f + seed, y * 0.4f) * 0.5f + Mathf.PerlinNoise(x * 0.4f, y * 0.03f + seed) * 0.5f;
                float t = 0.72f + h * 0.18f + (slub - 0.5f) * 0.18f;
                albedo[y * size + x] = new Color(t, t, t);
                height[y * size + x] = h;
            }

            Save(name, albedo, size, size, true);
            SaveNormal(name + "_N", height, size, size, 1.5f);
        }

        /// <summary>Velvet: soft vertical sheen streaks, light grey (tinted deep green by the material).</summary>
        private static void Velvet()
        {
            const int size = 512;
            var albedo = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float streak = Mathf.PerlinNoise(x * 0.05f, y * 0.004f) * 0.6f + Mathf.PerlinNoise(x * 0.4f, y * 0.02f) * 0.4f;
                float t = 0.78f + (streak - 0.5f) * 0.25f;
                albedo[y * size + x] = new Color(t, t, t);
            }

            Save("Velvet", albedo, size, size, true);
        }

        /// <summary>
        /// An original medallion rug: a border of bands with a repeated motif, a field with a small lattice and a
        /// central medallion. Mapped once over the whole rug (the material sets tiling and offset to the rug's size).
        /// </summary>
        private static void Rug(string name, int w, int h, Color field, Color border, Color light, int seed)
        {
            var albedo = new Color[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)(w - 1), v = y / (float)(h - 1);
                float edge = Mathf.Min(Mathf.Min(u, 1 - u) * w, Mathf.Min(v, 1 - v) * h) / Mathf.Min(w, h); // 0 at the edge
                Color c;
                if (edge < 0.012f) c = light * 0.85f;                                    // fringe line
                else if (edge < 0.025f) c = border * 0.8f;
                else if (edge < 0.1f)
                {
                    // Border with a repeated diamond motif.
                    float along = (Mathf.Min(u, 1 - u) * w < Mathf.Min(v, 1 - v) * h ? v * h : u * w) / (Mathf.Min(w, h) * 0.075f);
                    float across = (edge - 0.025f) / 0.075f;
                    float d = Mathf.Abs(Frac(along) - 0.5f) + Mathf.Abs(across - 0.5f);
                    c = d < 0.28f ? light : d < 0.36f ? field * 1.1f : border;
                }
                else if (edge < 0.11f) c = light * 0.9f;
                else
                {
                    float cx = (u - 0.5f) * w / Mathf.Min(w, h), cy = (v - 0.5f) * h / Mathf.Min(w, h);
                    float r = Mathf.Sqrt(cx * cx * 0.45f + cy * cy);
                    float petals = Mathf.Cos(Mathf.Atan2(cy, cx) * 8f) * 0.04f;
                    if (r < 0.2f + petals) c = r < 0.08f ? light : r < 0.12f ? border : field * 1.25f;
                    else if (r < 0.215f + petals) c = light * 0.9f;
                    else
                    {
                        float lu = u * w / (Mathf.Min(w, h) * 0.12f), lv = v * h / (Mathf.Min(w, h) * 0.12f);
                        float lat = Mathf.Abs(Frac(lu + lv) - 0.5f) < 0.04f || Mathf.Abs(Frac(lu - lv) - 0.5f) < 0.04f ? 1 : 0;
                        c = Color.Lerp(field, border, lat * 0.6f);
                    }
                }

                // Wool: small noise and a soft wear.
                float n = Mathf.PerlinNoise(x * 0.35f + seed, y * 0.35f) * 0.16f - 0.08f;
                float wear = Mathf.PerlinNoise(u * 4f + seed, v * 4f) * 0.12f;
                albedo[y * w + x] = new Color(c.r + n - wear * 0.3f, c.g + n - wear * 0.3f, c.b + n - wear * 0.3f);
            }

            Save(name, albedo, w, h, true);
        }

        /// <summary>
        /// The invented evening city (D93): a dusk gradient, three layers of towers with lit windows, a few red
        /// aviation lights. No real skyline. Used as emission on the backdrop far behind the windows.
        /// </summary>
        private static void City()
        {
            const int w = 4096, h = 2048;
            var rng = new System.Random(77);
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = y / (float)h;
                // Dusk glow low on the horizon, deep blue above.
                Color sky = v < 0.3f
                    ? Color.Lerp(new Color(0.30f, 0.15f, 0.20f), new Color(0.10f, 0.08f, 0.20f), v / 0.3f)
                    : Color.Lerp(new Color(0.10f, 0.08f, 0.20f), new Color(0.015f, 0.025f, 0.07f), Mathf.Clamp01((v - 0.3f) / 0.5f));
                for (int x = 0; x < w; x++) px[y * w + x] = new Color(sky.r * 0.5f, sky.g * 0.5f, sky.b * 0.5f, 1f);
            }

            // Far layers small, dim and dense; the near layer a little taller. Windows are tiny so the city reads as far away.
            float[] dim = { 0.3f, 0.55f, 0.9f };
            int[] floorPx = { 6, 7, 9 };
            for (int layer = 0; layer < 3; layer++)
            {
                int x = -rng.Next(10, 60);
                while (x < w)
                {
                    int bw = rng.Next(40, 150) + layer * 25;
                    int top = (int)(h * (0.12f + 0.05f * layer + (float)rng.NextDouble() * (0.12f + layer * 0.1f)));
                    if (rng.NextDouble() < 0.08) top += (int)(h * 0.12f); // the odd tower
                    float shade = (float)rng.NextDouble();
                    var body = Color.Lerp(new Color(0.025f, 0.03f, 0.05f), new Color(0.05f, 0.045f, 0.07f), shade) * (0.7f + layer * 0.15f);
                    body.a = 1f;
                    int fp = floorPx[layer];
                    int cols = Math.Max(2, bw / (fp + 1)), floors = Math.Max(3, top / fp);
                    float lit = 0.18f + (float)rng.NextDouble() * 0.3f;
                    bool cool = rng.NextDouble() < 0.2;
                    for (int yy = 0; yy < top && yy < h; yy++)
                    for (int xx = Math.Max(0, x); xx < Math.Min(w, x + bw); xx++)
                        px[yy * w + xx] = body;

                    for (int f = 1; f < floors; f++)
                    for (int c = 0; c < cols; c++)
                    {
                        if (rng.NextDouble() > lit) continue;
                        float warmth = (float)rng.NextDouble();
                        Color win = cool && warmth < 0.6f
                            ? new Color(0.55f, 0.7f, 0.95f)
                            : Color.Lerp(new Color(1f, 0.6f, 0.28f), new Color(1f, 0.84f, 0.55f), warmth);
                        win *= (0.5f + (float)rng.NextDouble() * 0.7f) * dim[layer];
                        win.a = 1f;
                        int wx0 = x + c * bw / cols + 1, wy0 = top - f * fp + 2;
                        for (int yy = wy0; yy < wy0 + fp / 2; yy++)
                        for (int xx = wx0; xx < wx0 + Math.Max(2, bw / cols - 3); xx++)
                            if (xx >= 0 && xx < w && yy >= 0 && yy < h) px[yy * w + xx] = win;
                    }

                    if (layer == 2 && rng.NextDouble() < 0.25)
                        for (int yy = top; yy < top + 4 && yy < h; yy++)
                        for (int xx = x + bw / 2 - 2; xx < x + bw / 2 + 2; xx++)
                            if (xx >= 0 && xx < w) px[yy * w + xx] = new Color(1.4f, 0.1f, 0.08f, 1f);
                    x += bw + rng.Next(-10, 30);
                }
            }

            // A low haze over the city.
            for (int y = 0; y < h * 0.35f; y++)
            {
                float a = 0.18f * (1f - y / (h * 0.35f));
                for (int x = 0; x < w; x++)
                {
                    var c = px[y * w + x];
                    px[y * w + x] = new Color(c.r + 0.16f * a, c.g + 0.1f * a, c.b + 0.14f * a, 1f);
                }
            }

            Save("City", px, w, h, true);
        }

        /// <summary>
        /// Lit office and flat windows for the 3D city blocks behind the window wall: 16 bays of 1.2 m by 16 floors
        /// of 3 m (19.2 m x 48 m a tile), some floors dark, warm and a few cool lights, unlit glass faintly blue.
        /// Emission only: the blocks' albedo is black.
        /// </summary>
        public static void CityWindows()
        {
            Directory.CreateDirectory(Folder);
            const int cols = 16, floors = 16, cw = 32, fh = 64, w = cols * cw, h = floors * fh;
            var rng = new System.Random(91);
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = new Color(0.004f, 0.005f, 0.009f, 1f);
            for (int f = 0; f < floors; f++)
            {
                double floorLit = rng.NextDouble() < 0.2 ? 0.05 : 0.25 + rng.NextDouble() * 0.45;
                for (int c = 0; c < cols; c++)
                {
                    bool lit = rng.NextDouble() < floorLit;
                    float warmth = (float)rng.NextDouble();
                    Color glass = lit
                        ? (rng.NextDouble() < 0.18 ? new Color(0.5f, 0.66f, 0.95f) : Color.Lerp(new Color(1f, 0.55f, 0.24f), new Color(1f, 0.85f, 0.58f), warmth))
                          * (0.45f + (float)rng.NextDouble() * 0.6f)
                        : new Color(0.012f, 0.016f, 0.03f);
                    glass.a = 1f;
                    // A lit window has a soft falloff towards its bottom, like a room lit from the ceiling.
                    for (int y = 10; y < fh - 14; y++)
                    for (int x = 4; x < cw - 4; x++)
                    {
                        float k = lit ? Mathf.Lerp(0.65f, 1f, (y - 10f) / (fh - 24f)) : 1f;
                        px[(f * fh + y) * w + c * cw + x] = new Color(glass.r * k, glass.g * k, glass.b * k, 1f);
                    }
                }
            }

            Save("CityWindows", px, w, h, true);
        }

        /// <summary>The question board: a dark matte felt so the light question overlay reads on it (D92).</summary>
        private static void Board()
        {
            const int size = 512;
            var albedo = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.06f + Mathf.PerlinNoise(x * 0.6f, y * 0.6f) * 0.04f;
                albedo[y * size + x] = new Color(0.12f + n, 0.13f + n, 0.16f + n);
            }

            Save("Board", albedo, size, size, true);
        }

        private static void Noise(string name, int size, Color a, Color b, int seed)
        {
            var albedo = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                albedo[y * size + x] = Color.Lerp(a, b, Mathf.PerlinNoise(x * 0.05f + seed, y * 0.05f));
            Save(name, albedo, size, size, true);
        }

        // ---------------------------------------------------------------- helpers

        private static float Hash(int n)
        {
            n = (n << 13) ^ n;
            return 1f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824f * 0.5f;
        }

        private static float Frac(float f) => f - Mathf.Floor(f);

        private static void Save(string name, Color[] pixels, int w, int h, bool srgb)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, !srgb);
            tex.SetPixels(pixels);
            tex.Apply();
            string path = Folder + "/" + name + ".png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = srgb;
            importer.wrapMode = name == "City" || name.StartsWith("Rug") ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            importer.anisoLevel = 8;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = Math.Max(w, h);
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static void SaveNormal(string name, float[] height, int w, int h, float strength)
        {
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float l = height[y * w + (x + w - 1) % w], r = height[y * w + (x + 1) % w];
                float d = height[((y + h - 1) % h) * w + x], u = height[((y + 1) % h) * w + x];
                var n = new Vector3((l - r) * strength, (d - u) * strength, 1f).normalized;
                px[y * w + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
            }

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            tex.SetPixels(px);
            tex.Apply();
            string path = Folder + "/" + name + ".png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.NormalMap;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.anisoLevel = 8;
            importer.SaveAndReimport();
        }
    }
}
