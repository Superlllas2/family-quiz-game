using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>A flat horizontal bar filled from the left: the read time bar and the answer countdown.</summary>
    public sealed class TimerBar
    {
        private readonly Image _fill;

        private TimerBar(RectTransform root, Image fill)
        {
            Root = root;
            _fill = fill;
        }

        public RectTransform Root { get; }

        public static TimerBar Create(Transform parent)
        {
            var track = UiBuilder.Image("TimerBar", parent, TvColors.BarTrack, RoundedSprite.Get());
            var fill = UiBuilder.Image("Fill", track.transform, TvColors.Text, RoundedSprite.Get());
            UiBuilder.Anchor(fill.rectTransform, 0, 0, 1, 1);
            return new TimerBar(track.rectTransform, fill);
        }

        /// <summary>Fill from 0 (empty) to 1 (full).</summary>
        public void Set(float fraction, Color color)
        {
            fraction = Mathf.Clamp01(fraction);
            _fill.rectTransform.anchorMax = new Vector2(fraction, 1f);
            _fill.enabled = fraction > 0f;
            _fill.color = color;
        }

        public void SetVisible(bool visible)
        {
            Root.gameObject.SetActive(visible);
        }
    }
}
