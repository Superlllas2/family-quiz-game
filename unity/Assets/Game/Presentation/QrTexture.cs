using UnityEngine;
using QrCode = global::Net.Codecrete.QrCodeGenerator.QrCode;

namespace FamilyQuiz.Presentation
{
    /// <summary>A QR code as a texture: one pixel per module, point filtered, with a white quiet zone.</summary>
    public static class QrTexture
    {
        /// <summary>The QR standard asks for a quiet zone of 4 modules around the code.</summary>
        public const int QuietZoneModules = 4;

        public static Texture2D Create(string text)
        {
            var qr = QrCode.EncodeText(text, QrCode.Ecc.Medium);
            int size = qr.Size + 2 * QuietZoneModules;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "RoomQr"
            };
            var dark = new Color32(0, 0, 0, 255);
            var light = new Color32(255, 255, 255, 255);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int mx = x - QuietZoneModules;
                    int my = y - QuietZoneModules;
                    bool on = mx >= 0 && my >= 0 && mx < qr.Size && my < qr.Size && qr.GetModule(mx, my);
                    // Texture rows start at the bottom; QR rows start at the top.
                    pixels[(size - 1 - y) * size + x] = on ? dark : light;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
