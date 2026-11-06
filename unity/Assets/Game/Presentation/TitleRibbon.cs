using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// A view's title as a printed ribbon in the TvLayout title zone at the top right (decisions/0017_game_night_ui.md):
    /// a coloured piece, a little turned, with the title and an optional smaller line under it. The hosts' bubbles own
    /// the top left, so a title never sits under a bubble.
    /// </summary>
    public sealed class TitleRibbon
    {
        private readonly TvPiece _piece;
        private readonly TextMeshProUGUI _title;
        private readonly TextMeshProUGUI _subtitle;

        private TitleRibbon(TvPiece piece, TextMeshProUGUI title, TextMeshProUGUI subtitle)
        {
            _piece = piece;
            _title = title;
            _subtitle = subtitle;
        }

        public RectTransform Root => _piece.Root;

        public static TitleRibbon Create(Transform parent, Color fill)
        {
            var zone = TvLayout.Put(UiBuilder.Rect("TitleZone", parent), TvLayout.Title);
            var piece = TvPiece.Card("Ribbon", zone, fill, 14f, 7f).Tilt(1.5f);
            TvTween.On(piece.Root).Tilt = 1.5f;
            UiBuilder.Anchor(piece.Root, 0.08f, 0.08f, 1f, 0.92f);
            var title = UiBuilder.Display("Title", piece.Content, 44, TvColors.TextOn(fill), TextAlignmentOptions.Right);
            UiBuilder.Fit(title, 22, 44, 1);
            UiBuilder.Anchor(title.rectTransform, 0.04f, 0.42f, 0.96f, 0.94f);
            var subtitle = UiBuilder.Text("Subtitle", piece.Content, 26, TvColors.TextOn(fill), true, TextAlignmentOptions.Right);
            UiBuilder.Fit(subtitle, 16, 26, 1);
            UiBuilder.Anchor(subtitle.rectTransform, 0.04f, 0.08f, 0.96f, 0.42f);
            return new TitleRibbon(piece, title, subtitle);
        }

        /// <summary>The title, and the line under it (null or empty centres the title alone).</summary>
        public void Set(string title, string subtitle)
        {
            _title.text = title ?? string.Empty;
            bool sub = !string.IsNullOrEmpty(subtitle);
            _subtitle.gameObject.SetActive(sub);
            _subtitle.text = subtitle ?? string.Empty;
            UiBuilder.Anchor(_title.rectTransform, 0.04f, sub ? 0.42f : 0.1f, 0.96f, sub ? 0.94f : 0.9f);
        }

        public void SetVisible(bool visible)
        {
            _piece.Root.parent.gameObject.SetActive(visible);
        }

        public void Rise()
        {
            TvTween.On(_piece.Root).Rise(0f);
        }
    }
}
