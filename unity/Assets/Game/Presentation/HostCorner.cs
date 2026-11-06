using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One host's line on the TV until the 3D hosts exist (decisions/0017_game_night_ui.md): the host's cube badge (a
    /// rounded square in their colour with two eyes and their name on a tag) and a comic speech bubble beside it, sized
    /// to the line. It lives in the TvLayout hosts zone, one row per speaking host; hidden until shown.
    /// </summary>
    public sealed class HostCorner
    {
        public const float RowHeight = 118f;
        private const float BadgeSize = 88f;
        private const float BubbleGap = 26f;
        private const float BubbleMaxWidth = TvLayoutHostsWidth - BadgeSize - BubbleGap;
        private const float TvLayoutHostsWidth = 960f;
        private const float PadX = 26f;
        private const float PadY = 14f;
        private const float LineSize = 32f;
        private const float LineMinSize = 24f;

        private readonly RectTransform _root;
        private readonly TvPiece _bubble;
        private readonly TextMeshProUGUI _name;
        private readonly TextMeshProUGUI _line;

        private HostCorner(RectTransform root, TvPiece bubble, TextMeshProUGUI name, TextMeshProUGUI line)
        {
            _root = root;
            _bubble = bubble;
            _name = name;
            _line = line;
        }

        public bool Visible => _root.gameObject.activeSelf;

        /// <summary>The language the texts were last drawn in; null to redraw.</summary>
        public string ShownLang { get; set; }

        public static HostCorner Create(Transform zone, string host, Color color)
        {
            var root = UiBuilder.Rect("Host " + host, zone);
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(1f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            root.sizeDelta = new Vector2(0f, RowHeight);

            var badge = TvPiece.Card("Badge", root, color, 16f, 6f).Tilt(-3f);
            var b = badge.Root;
            b.anchorMin = b.anchorMax = new Vector2(0f, 1f);
            b.pivot = new Vector2(0f, 1f);
            b.anchoredPosition = new Vector2(0f, -2f);
            b.sizeDelta = new Vector2(BadgeSize, BadgeSize);
            for (int i = 0; i < 2; i++)
            {
                var eye = UiBuilder.Image("Eye " + i, badge.Content, TvColors.Cream);
                eye.sprite = TvSprites.DiscSolid;
                var e = eye.rectTransform;
                e.anchorMin = e.anchorMax = new Vector2(i == 0 ? 0.36f : 0.64f, 0.58f);
                e.sizeDelta = new Vector2(16f, 16f);
            }

            var tag = TvPiece.Card("NameTag", root, TvColors.Mustard, 8f, 3f).Tilt(2f);
            var t = tag.Root;
            t.anchorMin = t.anchorMax = new Vector2(0f, 1f);
            t.pivot = new Vector2(0.5f, 0.5f);
            t.anchoredPosition = new Vector2(BadgeSize / 2f, -BadgeSize + 2f);
            t.sizeDelta = new Vector2(BadgeSize + 24f, 30f);
            var name = UiBuilder.Text("Name", tag.Content, 18, TvColors.Ink, true, TextAlignmentOptions.Center);
            UiBuilder.Fit(name, 11, 18, 1);
            UiBuilder.Anchor(name.rectTransform, 0.05f, 0f, 0.95f, 1f);

            var bubble = TvPiece.Card("Bubble", root, TvColors.Cream, 26f, 6f);
            var r = bubble.Root;
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(BadgeSize + BubbleGap, -4f);
            // The tail: a small cream square turned 45 degrees, drawn before the body so only its point shows.
            var tail = TvPiece.Card("Tail", r, TvColors.Cream, 4f, 0f).Tilt(45f);
            tail.Root.SetAsFirstSibling();
            var tr = tail.Root;
            tr.anchorMin = tr.anchorMax = new Vector2(0f, 1f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = new Vector2(2f, -40f);
            tr.sizeDelta = new Vector2(28f, 28f);
            var line = UiBuilder.Text("Line", bubble.Content, LineSize, TvColors.Ink, false, TextAlignmentOptions.Left);
            UiBuilder.Fit(line, LineMinSize, LineSize, 3);
            UiBuilder.Anchor(line.rectTransform, 0, 0, 1, 1);
            line.rectTransform.offsetMin = new Vector2(PadX, PadY);
            line.rectTransform.offsetMax = new Vector2(-PadX, -PadY);

            root.gameObject.SetActive(false);
            return new HostCorner(root, bubble, name, line);
        }

        /// <summary>Shows or hides the row; a row that appears pops in.</summary>
        public void SetVisible(bool visible)
        {
            if (_root.gameObject.activeSelf == visible) return;
            _root.gameObject.SetActive(visible);
            if (visible) TvTween.On(_bubble.Root).Pop(0f, 0.35f);
        }

        /// <summary>The row's place in the zone: 0 is the top row.</summary>
        public void SetRow(int row)
        {
            _root.anchoredPosition = new Vector2(0f, -row * (RowHeight + 10f));
        }

        public void SetTexts(string hostName, string line)
        {
            _name.text = hostName ?? string.Empty;
            _line.text = line ?? string.Empty;
            // The bubble hugs the line: as wide as it needs up to the zone, as tall as two or three lines need.
            float inner = BubbleMaxWidth - 2 * PadX;
            var size = _line.GetPreferredValues(_line.text, inner, 0f);
            float width = Mathf.Clamp(size.x + 2 * PadX + 4f, 200f, BubbleMaxWidth);
            float lines = Mathf.Clamp(Mathf.Ceil(size.y / (LineSize * 1.25f)), 1f, 3f);
            float height = Mathf.Clamp(lines * LineSize * 1.25f + 2 * PadY, 64f, RowHeight - 8f);
            _bubble.Root.sizeDelta = new Vector2(width, height);
        }
    }
}
