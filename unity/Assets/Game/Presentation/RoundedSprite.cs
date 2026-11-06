using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>A white rounded rectangle made in code, nine sliced, for chips and cards.</summary>
    public static class RoundedSprite
    {
        private const int Size = 64;
        private const int Radius = 24;
        private static Sprite _sprite;

        /// <summary>The game pieces' plain rounded shape (TvSprites.Solid) since the game night look (0017).</summary>
        public static Sprite Get()
        {
            return TvSprites.Solid;
        }

        /// <summary>The first, smaller rounded rectangle; kept for reference, unused.</summary>
        public static Sprite Original()
        {
            if (_sprite != null) return _sprite;
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "RoundedRect"
            };
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float alpha = Coverage(x + 0.5f, y + 0.5f);
                    pixels[y * Size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            _sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(Radius, Radius, Radius, Radius));
            return _sprite;
        }

        private static float Coverage(float px, float py)
        {
            float cx = Mathf.Clamp(px, Radius, Size - Radius);
            float cy = Mathf.Clamp(py, Radius, Size - Radius);
            float distance = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
            return Mathf.Clamp01(Radius - distance + 0.5f);
        }
    }
}
