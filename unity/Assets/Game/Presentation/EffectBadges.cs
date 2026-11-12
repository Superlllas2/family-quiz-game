using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The power ups a player is hit with, as a row of small discs in each power up's colour with its glyph (D149): ice,
    /// slime, fog, in the order they were used. Sits centred on a point of its parent (place Root); a new badge pops in.
    /// </summary>
    public sealed class EffectBadges : MonoBehaviour
    {
        private const float Overlap = 0.25f;

        private readonly List<GameObject> _badges = new List<GameObject>();
        private readonly List<string> _shown = new List<string>();
        private float _size;

        public RectTransform Root => (RectTransform)transform;

        /// <summary>An empty row of badges of the given size, anchored at (x, y) of the parent.</summary>
        public static EffectBadges Create(Transform parent, float size, float x, float y)
        {
            var rect = UiBuilder.Rect("Effects", parent);
            rect.anchorMin = rect.anchorMax = new Vector2(x, y);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(size, size);
            var badges = rect.gameObject.AddComponent<EffectBadges>();
            badges._size = size;
            return badges;
        }

        /// <summary>The badges for these power ups; redrawn only when they changed. Null or empty clears them.</summary>
        public void Set(IReadOnlyList<string> powerUps, bool animate = true)
        {
            int count = powerUps != null ? powerUps.Count : 0;
            bool same = count == _shown.Count;
            for (int i = 0; same && i < count; i++) same = powerUps[i] == _shown[i];
            if (same) return;

            int kept = 0;
            while (kept < count && kept < _shown.Count && powerUps[kept] == _shown[kept]) kept++;
            for (int i = _badges.Count - 1; i >= kept; i--)
            {
                Destroy(_badges[i]);
                _badges.RemoveAt(i);
                _shown.RemoveAt(i);
            }

            for (int i = kept; i < count; i++)
            {
                var disc = TvPiece.Disc("Badge " + i, transform, TvColors.PowerUp(powerUps[i]), 3f);
                var glyph = UiBuilder.Image("Glyph", disc.Content, TvColors.Ink);
                glyph.sprite = TvIcons.Effect(powerUps[i]);
                glyph.preserveAspect = true;
                glyph.enabled = glyph.sprite != null;
                UiBuilder.Anchor(glyph.rectTransform, 0.16f, 0.16f, 0.84f, 0.84f);
                _badges.Add(disc.Root.gameObject);
                _shown.Add(powerUps[i]);
                if (animate) TvTween.On(disc.Root).Slam(0.05f * (i - kept), 0.35f);
            }

            float step = _size * (1f - Overlap);
            float width = _size + step * (count - 1);
            for (int i = 0; i < _badges.Count; i++)
            {
                var r = (RectTransform)_badges[i].transform;
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.anchoredPosition = new Vector2(-width / 2f + _size / 2f + i * step, 0f);
                r.sizeDelta = new Vector2(_size, _size);
            }
        }

        public void Clear()
        {
            Set(null, false);
        }
    }
}
