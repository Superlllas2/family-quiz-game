using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The game night fonts (decisions/0017_game_night_ui.md), made by the editor's GameNightFonts and loaded from
    /// Resources: Unbounded ExtraBold for headings, numbers and tile letters, Nunito ExtraBold for text (the default TMP
    /// font), Nunito Black for strong text, Caveat Bold for a hand written word. Null when a font is missing, so TMP's
    /// default font draws instead.
    /// </summary>
    public static class TvFonts
    {
        private static TMP_FontAsset _display;
        private static TMP_FontAsset _body;
        private static TMP_FontAsset _black;
        private static TMP_FontAsset _hand;

        public static TMP_FontAsset Display => _display != null ? _display : _display = Load("Unbounded-ExtraBold");

        public static TMP_FontAsset Body => _body != null ? _body : _body = Load("Nunito-ExtraBold");

        public static TMP_FontAsset Black => _black != null ? _black : _black = Load("Nunito-Black");

        public static TMP_FontAsset Hand => _hand != null ? _hand : _hand = Load("Caveat-Bold");

        private static TMP_FontAsset Load(string name)
        {
            return Resources.Load<TMP_FontAsset>("Fonts/" + name + " SDF");
        }
    }
}
