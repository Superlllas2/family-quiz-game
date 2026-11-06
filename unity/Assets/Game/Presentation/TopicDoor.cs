using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One topic card of the TopicVote (decisions/0017_game_night_ui.md): a board game category card in cream with a
    /// coloured header, its number on a coin, the topic title, and the tokens of the players who voted for it along its
    /// bottom.
    /// </summary>
    public sealed class TopicDoor : MonoBehaviour
    {
        private const float VoterSize = 50f;
        private const float VoterGap = 8f;

        private readonly List<PlayerToken> _voters = new List<PlayerToken>();
        private readonly List<string> _voterIds = new List<string>();
        private TvPiece _card;
        private TvPiece _header;
        private TextMeshProUGUI _number;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _count;
        private RectTransform _votes;

        public RectTransform Rect => (RectTransform)transform;

        public static TopicDoor Create(Transform parent, int index)
        {
            var rect = UiBuilder.Rect("Door " + index, parent);
            var door = rect.gameObject.AddComponent<TopicDoor>();
            door.Build(index);
            return door;
        }

        public void Set(string title, Color header, string count)
        {
            _title.text = title;
            _header.SetFill(header);
            _count.text = count;
        }

        /// <summary>The voters' tokens, in seat order; rebuilt only when the voters changed. True if one was added.</summary>
        public bool SetVoters(IReadOnlyList<(string Id, string Label, Color Color, bool Connected)> voters)
        {
            bool same = voters.Count == _voterIds.Count;
            for (int i = 0; same && i < voters.Count; i++) same = voters[i].Id == _voterIds[i];
            bool added = voters.Count > _voterIds.Count;
            if (!same)
            {
                var before = new HashSet<string>(_voterIds);
                foreach (var t in _voters) Destroy(t.gameObject);
                _voters.Clear();
                _voterIds.Clear();
                for (int i = 0; i < voters.Count; i++)
                {
                    var token = PlayerToken.Create(_votes, voters[i].Id);
                    _voters.Add(token);
                    _voterIds.Add(voters[i].Id);
                    if (!before.Contains(voters[i].Id)) TvTween.On(token.Rect).Pop(0f, 0.35f);
                }
            }

            // Two rows of four at most; more squeeze together.
            int perRow = Mathf.Max(1, Mathf.Min(4, voters.Count));
            float width = _votes.rect.width > 0f ? _votes.rect.width : 260f;
            float step = Mathf.Min(VoterSize + VoterGap, (width - VoterSize) / Mathf.Max(1, perRow - 1));
            for (int i = 0; i < _voters.Count; i++)
            {
                int row = i / 4;
                int col = i % 4;
                int inRow = Mathf.Min(4, voters.Count - row * 4);
                float rowWidth = VoterSize + (inRow - 1) * step;
                var rect = _voters[i].Rect;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(-rowWidth / 2f + col * step, row * (VoterSize + 6f));
                rect.sizeDelta = new Vector2(VoterSize, VoterSize);
                _voters[i].Set(voters[i].Label, voters[i].Color, voters[i].Connected);
            }

            return added;
        }

        private void Build(int index)
        {
            _card = TvPiece.Card("Card", transform, TvColors.Cream, 22f, 10f);
            UiBuilder.Anchor(_card.Root, 0, 0, 1, 1);

            _header = TvPiece.Card("Header", _card.Content, TvColors.Teal, 18f, 0f);
            UiBuilder.Anchor(_header.Root, 0f, 0.7f, 1f, 1f);

            var coin = TvPiece.Disc("Number", transform, TvColors.Mustard, 4f);
            var c = coin.Root;
            c.anchorMin = c.anchorMax = new Vector2(1f, 1f);
            c.pivot = new Vector2(0.5f, 0.5f);
            c.anchoredPosition = new Vector2(-12f, -10f);
            c.sizeDelta = new Vector2(64f, 64f);
            _number = UiBuilder.Display("Text", coin.Content, 28, TvColors.Ink, TextAlignmentOptions.Center);
            _number.text = (index + 1).ToString();
            UiBuilder.Anchor(_number.rectTransform, 0, 0, 1, 1);

            _count = UiBuilder.Display("Count", _header.Content, 30, TvColors.Cream, TextAlignmentOptions.Left);
            UiBuilder.Fit(_count, 18, 30, 1);
            UiBuilder.Anchor(_count.rectTransform, 0.08f, 0.15f, 0.7f, 0.85f);

            _title = UiBuilder.Display("Title", _card.Content, 38, TvColors.Ink, TextAlignmentOptions.TopLeft);
            UiBuilder.Fit(_title, 22, 38, 4);
            _title.lineSpacing = -8f;
            UiBuilder.Anchor(_title.rectTransform, 0.08f, 0.3f, 0.94f, 0.66f);

            _votes = UiBuilder.Anchor(UiBuilder.Rect("Votes", _card.Content), 0.06f, 0.04f, 0.94f, 0.28f);
        }
    }
}
