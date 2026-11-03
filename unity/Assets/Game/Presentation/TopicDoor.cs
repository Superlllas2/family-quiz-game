using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>One topic door of the TopicVote on the TV: a rounded card in the door's color with the topic title and its live vote count.</summary>
    public sealed class TopicDoor : MonoBehaviour
    {
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _votes;

        public RectTransform Rect => (RectTransform)transform;

        public static TopicDoor Create(Transform parent, int index)
        {
            var rect = UiBuilder.Rect("Door " + index, parent);
            var door = rect.gameObject.AddComponent<TopicDoor>();
            door.Build(index);
            return door;
        }

        public void Set(string title, string votes)
        {
            _title.text = title;
            _votes.text = votes;
        }

        private void Build(int index)
        {
            var card = UiBuilder.Image("Card", transform, TvColors.Answer(index), RoundedSprite.Get());
            UiBuilder.Anchor(card.rectTransform, 0, 0, 1, 1);

            _title = UiBuilder.Text("Title", card.transform, 60, TvColors.Background, true, TextAlignmentOptions.Center);
            // Three lines at most; long titles shrink before they are cut.
            _title.enableAutoSizing = true;
            _title.fontSizeMin = 36;
            _title.fontSizeMax = 60;
            _title.maxVisibleLines = 3;
            UiBuilder.Anchor(_title.rectTransform, 0.06f, 0.35f, 0.94f, 0.94f);

            _votes = UiBuilder.Text("Votes", card.transform, 52, TvColors.Background, true, TextAlignmentOptions.Center);
            _votes.textWrappingMode = TextWrappingModes.NoWrap;
            _votes.enableAutoSizing = true;
            _votes.fontSizeMin = 32;
            _votes.fontSizeMax = 52;
            UiBuilder.Anchor(_votes.rectTransform, 0.06f, 0.06f, 0.94f, 0.28f);
        }
    }
}
