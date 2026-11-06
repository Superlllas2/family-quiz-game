using System.Collections.Generic;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The power up tags under the status corner (docs/03_rules.md, "Using"; decisions/0017_game_night_ui.md): one tag
    /// per hit player, in the power up's colour, with the target's token and "Dad → Freeze". It owns the TvLayout side
    /// zone; tags that do not fit are left out rather than spill over.
    /// </summary>
    public sealed class HitList : MonoBehaviour
    {
        private const float TagHeight = 52f;
        private const float TagGap = 10f;

        private readonly List<GameObject> _tags = new List<GameObject>();
        private readonly List<string> _shown = new List<string>();
        private TvContext _context;

        public static HitList Create(Transform parent, TvContext context)
        {
            var rect = TvLayout.Put(UiBuilder.Rect("Hits", parent), TvLayout.Side);
            var list = rect.gameObject.AddComponent<HitList>();
            list._context = context;
            return list;
        }

        /// <summary>The hits of this phase (PowerUpHit.For), redrawn only when they changed.</summary>
        public void Refresh(GameState state, GamePhase phase)
        {
            var hits = new List<(PlayerState Player, PowerUpHit Hit)>();
            var keys = new List<string>();
            foreach (var p in state.Players)
            {
                var hit = PowerUpHit.For(p, phase);
                if (hit == null) continue;
                hits.Add((p, hit));
                keys.Add(p.PlayerId + ":" + _context.HitText(hit) + ":" + _context.Lang);
            }

            if (Same(keys)) return;
            _shown.Clear();
            _shown.AddRange(keys);
            foreach (var t in _tags) Destroy(t);
            _tags.Clear();

            var names = new string[state.Players.Count];
            for (int i = 0; i < names.Length; i++) names[i] = state.Players[i].Name;
            var labels = TokenLabels.For(names);
            float height = ((RectTransform)transform).rect.height;
            if (height <= 0f) height = TvLayout.Side.height;
            int fit = Mathf.Max(0, (int)((height + TagGap) / (TagHeight + TagGap)));
            for (int i = 0; i < hits.Count && i < fit; i++)
            {
                var (player, hit) = hits[i];
                var color = TvColors.PowerUp(hit.PowerUp);
                var tag = TvPiece.Card("Hit " + player.PlayerId, transform, color, 12f, 5f);
                var rect = tag.Root;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -i * (TagHeight + TagGap));
                rect.sizeDelta = new Vector2(0f, TagHeight);

                var token = PlayerToken.Create(tag.Content, player.PlayerId);
                var t = token.Rect;
                t.anchorMin = t.anchorMax = new Vector2(0f, 0.5f);
                t.pivot = new Vector2(0f, 0.5f);
                t.anchoredPosition = new Vector2(6f, 0f);
                t.sizeDelta = new Vector2(40f, 40f);
                int seat = -1;
                for (int k = 0; k < state.Players.Count; k++) if (state.Players[k].PlayerId == player.PlayerId) seat = k;
                token.Set(seat >= 0 ? labels[seat] : "?", _context.ChipColor(player.CharacterId), player.Connected);

                var text = UiBuilder.Text("Text", tag.Content, 26, TvColors.Ink, true, TextAlignmentOptions.Left);
                UiBuilder.Fit(text, 16, 26, 1);
                UiBuilder.Anchor(text.rectTransform, 0, 0, 1, 1);
                text.rectTransform.offsetMin = new Vector2(56f, 2f);
                text.rectTransform.offsetMax = new Vector2(-12f, -2f);
                text.text = _context.HitText(hit);
                TvTween.On(rect).Pop(0.05f * i, 0.35f);
                _tags.Add(rect.gameObject);
            }
        }

        public void Clear()
        {
            _shown.Clear();
            foreach (var t in _tags) Destroy(t);
            _tags.Clear();
        }

        private bool Same(List<string> keys)
        {
            if (keys.Count != _shown.Count) return false;
            for (int i = 0; i < keys.Count; i++) if (keys[i] != _shown[i]) return false;
            return true;
        }
    }
}
