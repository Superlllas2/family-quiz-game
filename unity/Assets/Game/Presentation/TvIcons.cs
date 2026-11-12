using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Small shapes drawn in code once, like TvSprites: the attack arrow's head and the leader's crown (white with the
    /// ink outline baked in, to be tinted), and the power up glyphs (plain white shapes, tinted ink) that sit on a disc
    /// in the power up's colour: ice for freeze, a blob for slime, wavy lines for fog (D149).
    /// </summary>
    public static class TvIcons
    {
        private const int Size = 128;
        private const float Outline = 7f;
        private static readonly Color32 InkPixel = new Color32(0x2B, 0x1D, 0x16, 0xFF);

        private static Sprite _arrowHead;
        private static Sprite _crown;
        private static Sprite _freeze;
        private static Sprite _slime;
        private static Sprite _fog;

        /// <summary>A triangle pointing right (+x), its tip at the right edge's middle.</summary>
        public static Sprite ArrowHead => _arrowHead != null ? _arrowHead : _arrowHead = Polygon("ArrowHead", new[]
        {
            new Vector2(10, 12), new Vector2(122, 64), new Vector2(10, 116), new Vector2(34, 64),
        }, true);

        /// <summary>A three pointed crown.</summary>
        public static Sprite Crown => _crown != null ? _crown : _crown = Polygon("Crown", new[]
        {
            new Vector2(14, 22), new Vector2(114, 22), new Vector2(116, 104), new Vector2(88, 66), new Vector2(64, 114),
            new Vector2(40, 66), new Vector2(12, 104),
        }, true);

        /// <summary>The glyph of a power up's effect; null for an unknown one.</summary>
        public static Sprite Effect(string powerUp)
        {
            switch (powerUp)
            {
                case PowerUpTypes.Freeze: return _freeze != null ? _freeze : _freeze = Glyph("Freeze", Snowflake);
                case PowerUpTypes.Slime: return _slime != null ? _slime : _slime = Glyph("Slime", Blob);
                case PowerUpTypes.Fog: return _fog != null ? _fog : _fog = Glyph("Fog", Waves);
                default: return null;
            }
        }

        private static float Snowflake(Vector2 p)
        {
            var c = new Vector2(64, 64);
            float d = float.MaxValue;
            for (int arm = 0; arm < 6; arm++)
            {
                float a = arm * Mathf.PI / 3f;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var tip = c + dir * 48f;
                d = Mathf.Min(d, Segment(p, c, tip));
                // A small V on each arm.
                var at = c + dir * 30f;
                var side = new Vector2(-dir.y, dir.x);
                d = Mathf.Min(d, Segment(p, at, at + (dir + side) * 11f));
                d = Mathf.Min(d, Segment(p, at, at + (dir - side) * 11f));
            }

            return d - 6f;
        }

        private static float Blob(Vector2 p)
        {
            float d = (p - new Vector2(64, 74)).magnitude - 36f;
            d = Mathf.Min(d, Segment(p, new Vector2(42, 60), new Vector2(40, 26)) - 11f);
            d = Mathf.Min(d, Segment(p, new Vector2(80, 58), new Vector2(82, 34)) - 9f);
            d = Mathf.Min(d, (p - new Vector2(40, 22)).magnitude - 13f);
            return d;
        }

        private static float Waves(Vector2 p)
        {
            float d = float.MaxValue;
            foreach (float y in new[] { 34f, 64f, 94f })
            {
                Vector2 last = new Vector2(16, y);
                for (int i = 1; i <= 12; i++)
                {
                    float x = 16 + 96f * i / 12f;
                    var next = new Vector2(x, y + Mathf.Sin(i / 12f * Mathf.PI * 2f) * 7f);
                    d = Mathf.Min(d, Segment(p, last, next));
                    last = next;
                }
            }

            return d - 6f;
        }

        private static float Segment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return (p - (a + ab * t)).magnitude;
        }

        /// <summary>Signed distance to a closed polygon: negative inside.</summary>
        private static float PolygonDistance(Vector2 p, IReadOnlyList<Vector2> poly)
        {
            float d = float.MaxValue;
            bool inside = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                d = Mathf.Min(d, Segment(p, poly[j], poly[i]));
                if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                    p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                    inside = !inside;
            }

            return inside ? -d : d;
        }

        private static Sprite Polygon(string name, Vector2[] poly, bool outline)
        {
            return Draw(name, p => PolygonDistance(p, poly), outline);
        }

        private static Sprite Glyph(string name, Func<Vector2, float> distance)
        {
            return Draw(name, distance, false);
        }

        private static Sprite Draw(string name, Func<Vector2, float> distance, bool outline)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = "Tv" + name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var px = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float d = distance(new Vector2(x + 0.5f, y + 0.5f));
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(0.5f - d) * 255f);
                bool ink = outline && d > -Outline;
                px[y * Size + x] = ink
                    ? new Color32(InkPixel.r, InkPixel.g, InkPixel.b, alpha)
                    : new Color32(255, 255, 255, alpha);
            }

            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
