using System;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The game pieces' sprites, drawn in code once (decisions/0017_game_night_ui.md): white shapes to be tinted, with
    /// the ink outline baked in (a tint multiplies it, and ink times any colour stays ink dark) and a faint paper grain
    /// in the fill. Card is nine sliced and drawn Tiled, so the grain repeats instead of stretching.
    /// </summary>
    public static class TvSprites
    {
        /// <summary>The corner radius of Card and Solid in sprite pixels; Image.pixelsPerUnitMultiplier scales it.</summary>
        public const float Radius = 28f;

        /// <summary>The ink outline of Card in sprite pixels (about 4 canvas units at multiplier 1).</summary>
        public const float Outline = 4.5f;

        private const int CardSize = 128;

        /// <summary>
        /// Clear pixels around the card shapes inside their texture. The straight edges then fade out in the texture
        /// instead of being cut by the mesh, so a tilted piece is smoothed by bilinear filtering. The UI layer gets no
        /// MSAA or SMAA (those smooth the 3D studio only), so a shape drawn to the texture's border showed stair steps.
        /// </summary>
        private const float EdgePad = 3f;
        private const int DiscSize = 160;
        private const int StarSize = 256;
        private static readonly Color32 InkPixel = new Color32(0x2B, 0x1D, 0x16, 0xFF);

        private static Sprite _card;
        private static Sprite _solid;
        private static Sprite _frame;
        private static Sprite _disc;
        private static Sprite _discSolid;
        private static Sprite _star;
        private static Sprite _check;
        private static Sprite _grain;

        /// <summary>A rounded card with an ink outline and paper grain; nine sliced, best drawn Tiled.</summary>
        public static Sprite Card => _card != null ? _card : _card = RoundedRect("Card", true, true);

        /// <summary>A plain white rounded rectangle of the same shape: the solid bottom edges and shadows.</summary>
        public static Sprite Solid => _solid != null ? _solid : _solid = RoundedRect("Solid", false, false);

        /// <summary>Only the outline of the rounded rectangle, a thicker one: the rubber stamp's frame.</summary>
        public static Sprite Frame => _frame != null ? _frame : _frame = MakeFrame();

        /// <summary>A disc with an ink outline and grain: tokens, coins, the clock.</summary>
        public static Sprite Disc => _disc != null ? _disc : _disc = MakeDisc("Disc", true);

        /// <summary>A plain white disc: the edges under discs, dots, the clock's sweep.</summary>
        public static Sprite DiscSolid => _discSolid != null ? _discSolid : _discSolid = MakeDisc("DiscSolid", false);

        /// <summary>A twelve point burst with an ink outline: stickers like "×2!".</summary>
        public static Sprite Star => _star != null ? _star : _star = MakeStar();

        /// <summary>A thick white check mark: tint it ink.</summary>
        public static Sprite Check => _check != null ? _check : _check = MakeCheck();

        /// <summary>A tiling paper grain, white with soft dark specks: an overlay at low alpha.</summary>
        public static Sprite Grain => _grain != null ? _grain : _grain = MakeGrain();

        private static Texture2D NewTexture(string name, int w, int h, TextureWrapMode wrap = TextureWrapMode.Clamp)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = wrap,
                filterMode = FilterMode.Bilinear,
                name = "Tv" + name
            };
        }

        /// <summary>Signed distance to a rounded box centred in a size x size square (negative inside).</summary>
        private static float RoundedBox(float px, float py, float size, float radius)
        {
            float half = size * 0.5f;
            float qx = Mathf.Abs(px - half) - (half - radius);
            float qy = Mathf.Abs(py - half) - (half - radius);
            float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        /// <summary>A deterministic hash noise in 0..1, the same on every run.</summary>
        private static float Noise(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
            }
        }

        private static Color32 Shade(float d, float outline, bool grain, int x, int y)
        {
            float alpha = Mathf.Clamp01(0.5f - d);
            float fill = 1f;
            if (grain) fill = 0.955f + 0.045f * Noise(x, y);
            var c = new Color(fill, fill, fill, 1f);
            if (outline > 0f)
            {
                // Ink where the inside distance is less than the outline width, anti aliased on its inner edge.
                float ink = Mathf.Clamp01(outline + 0.5f + d);
                c = Color.Lerp(c, (Color)InkPixel, ink);
            }

            c.a = alpha;
            return c;
        }

        private static Sprite RoundedRect(string name, bool outline, bool grain)
        {
            var tex = NewTexture(name, CardSize, CardSize);
            var px = new Color32[CardSize * CardSize];
            for (int y = 0; y < CardSize; y++)
            for (int x = 0; x < CardSize; x++)
            {
                float d = RoundedBox(x + 0.5f - EdgePad, y + 0.5f - EdgePad, CardSize - 2 * EdgePad, Radius);
                px[y * CardSize + x] = Shade(d, outline ? Outline : 0f, grain, x, y);
            }

            tex.SetPixels32(px);
            tex.Apply(false, true);
            float b = Radius + 4f + EdgePad;
            return Sprite.Create(tex, new Rect(0, 0, CardSize, CardSize), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }

        private static Sprite MakeFrame()
        {
            const float width = 9f;
            var tex = NewTexture("Frame", CardSize, CardSize);
            var px = new Color32[CardSize * CardSize];
            for (int y = 0; y < CardSize; y++)
            for (int x = 0; x < CardSize; x++)
            {
                float d = RoundedBox(x + 0.5f - EdgePad, y + 0.5f - EdgePad, CardSize - 2 * EdgePad, Radius * 0.6f);
                float a = Mathf.Clamp01(0.5f - d) * Mathf.Clamp01(width + 0.5f + d);
                px[y * CardSize + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
            }

            tex.SetPixels32(px);
            tex.Apply(false, true);
            float b = Radius * 0.6f + width + 2f + EdgePad;
            return Sprite.Create(tex, new Rect(0, 0, CardSize, CardSize), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }

        private static Sprite MakeDisc(string name, bool outline)
        {
            var tex = NewTexture(name, DiscSize, DiscSize);
            var px = new Color32[DiscSize * DiscSize];
            float r = DiscSize * 0.5f;
            for (int y = 0; y < DiscSize; y++)
            for (int x = 0; x < DiscSize; x++)
            {
                float dx = x + 0.5f - r, dy = y + 0.5f - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy) - (r - 1f);
                px[y * DiscSize + x] = Shade(d, outline ? 7f : 0f, outline, x, y);
            }

            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, DiscSize, DiscSize), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite MakeStar()
        {
            const int points = 12;
            var tex = NewTexture("Star", StarSize, StarSize);
            var px = new Color32[StarSize * StarSize];
            float c = StarSize * 0.5f;
            float outer = c - 3f, inner = outer * 0.74f;
            for (int y = 0; y < StarSize; y++)
            for (int x = 0; x < StarSize; x++)
            {
                float dx = x + 0.5f - c, dy = y + 0.5f - c;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);
                // The burst's edge radius at this angle: a zigzag between the inner and outer radius.
                float t = Mathf.Repeat(angle / (2f * Mathf.PI) * points, 1f);
                float edge = Mathf.Lerp(outer, inner, Mathf.Abs(t - 0.5f) * 2f);
                // The zigzag's slope makes the radial distance overstate the true one: scale it down.
                float d = (dist - edge) * 0.8f;
                px[y * StarSize + x] = Shade(d, 9f, true, x, y);
            }

            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, StarSize, StarSize), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return (p - (a + ab * t)).magnitude;
        }

        private static Sprite MakeCheck()
        {
            const int size = 128;
            var tex = NewTexture("Check", size, size);
            var px = new Color32[size * size];
            var a = new Vector2(24, 64);
            var b = new Vector2(52, 34);
            var c = new Vector2(106, 96);
            const float half = 11f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Mathf.Min(SegmentDistance(p, a, b), SegmentDistance(p, b, c)) - half;
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(0.5f - d) * 255f);
                px[y * size + x] = new Color32(255, 255, 255, alpha);
            }

            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite MakeGrain()
        {
            const int size = 128;
            var tex = NewTexture("Grain", size, size, TextureWrapMode.Repeat);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float n = Noise(x + 977, y + 331);
                byte v = (byte)(n > 0.5f ? 255 : 0);
                px[y * size + x] = new Color32(v, v, v, (byte)Mathf.RoundToInt(Mathf.Abs(n - 0.5f) * 2f * 255f));
            }

            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
