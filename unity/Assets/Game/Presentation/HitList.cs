using System.Collections.Generic;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The power up tags under the status corner (docs/03_rules.md, "Earning" and "Using"; decisions/0017, D145), in the
    /// power up's colour with a player's token:
    /// <list type="bullet">
    /// <item>Reveal, Scores and Attacks: who drew a power up and why ("Аня получает: Слизь", "За верный ответ"), while they
    /// still hold it, then every hit used in the pick window that lands at the next Answer.</item>
    /// <item>Answer: every hit that landed, "Аня → Слизь → Соня", one tag per hit.</item>
    /// </list>
    /// It owns the TvLayout side zone; tags that do not fit are left out rather than spill over.
    /// </summary>
    public sealed class HitList : MonoBehaviour
    {
        private const float HitHeight = 44f;
        private const float DrawHeight = 58f;
        private const float TagGap = 8f;

        private readonly List<GameObject> _tags = new List<GameObject>();
        private readonly List<string> _shown = new List<string>();
        private TvContext _context;

        private struct Entry
        {
            public PlayerState TokenOf;
            public string PowerUp;
            public string Line1;
            public string Line2;
        }

        public static HitList Create(Transform parent, TvContext context)
        {
            var rect = TvLayout.Put(UiBuilder.Rect("Hits", parent), TvLayout.Side);
            var list = rect.gameObject.AddComponent<HitList>();
            list._context = context;
            return list;
        }

        /// <summary>The draws and hits of this phase, redrawn only when they changed.</summary>
        public void Refresh(GameState state, GamePhase phase)
        {
            var entries = new List<Entry>();
            if (phase == GamePhase.Reveal || phase == GamePhase.Scores || phase == GamePhase.Attacks)
            {
                foreach (var d in _context.Draws)
                {
                    var p = state.FindById(d.PlayerId);
                    if (p == null || p.HeldPowerUp != d.PowerUp) continue; // used already: its hit tag says the rest
                    var (title, reason) = _context.DrawText(p.Name, d.PowerUp, d.Rank);
                    entries.Add(new Entry { TokenOf = p, PowerUp = d.PowerUp, Line1 = title, Line2 = reason });
                }
            }

            foreach (var p in state.Players)
            {
                foreach (var hit in PowerUpHit.All(p, phase))
                {
                    var attacker = FindByName(state, hit.AttackerName) ?? p;
                    entries.Add(new Entry { TokenOf = attacker, PowerUp = hit.PowerUp, Line1 = _context.HitText(hit) });
                }
            }

            var keys = new List<string>();
            foreach (var e in entries) keys.Add(e.TokenOf.PlayerId + ":" + e.Line1 + ":" + e.Line2);
            if (Same(keys)) return;
            _shown.Clear();
            _shown.AddRange(keys);
            Clear(false);

            var names = new string[state.Players.Count];
            for (int i = 0; i < names.Length; i++) names[i] = state.Players[i].Name;
            var labels = TokenLabels.For(names);
            float height = ((RectTransform)transform).rect.height;
            if (height <= 0f) height = TvLayout.Side.height;
            float y = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                bool two = e.Line2 != null;
                float tagHeight = two ? DrawHeight : HitHeight;
                if (y + tagHeight > height) break;
                var tag = TvPiece.Card("Tag " + i, transform, TvColors.PowerUp(e.PowerUp), 12f, 5f);
                var rect = tag.Root;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -y);
                rect.sizeDelta = new Vector2(0f, tagHeight);
                y += tagHeight + TagGap;

                var token = PlayerToken.Create(tag.Content, e.TokenOf.PlayerId);
                var t = token.Rect;
                t.anchorMin = t.anchorMax = new Vector2(0f, 0.5f);
                t.pivot = new Vector2(0f, 0.5f);
                t.anchoredPosition = new Vector2(6f, 0f);
                t.sizeDelta = new Vector2(36f, 36f);
                int seat = -1;
                for (int k = 0; k < state.Players.Count; k++) if (state.Players[k] == e.TokenOf) seat = k;
                token.Set(seat >= 0 ? labels[seat] : "?", _context.ChipColor(e.TokenOf.CharacterId), e.TokenOf.Connected);

                var line1 = UiBuilder.Text("Text", tag.Content, 24, TvColors.Ink, true, TextAlignmentOptions.Left);
                UiBuilder.Fit(line1, 14, 24, 1);
                UiBuilder.Anchor(line1.rectTransform, 0, two ? 0.48f : 0f, 1, 1);
                line1.rectTransform.offsetMin = new Vector2(50f, two ? 0f : 2f);
                line1.rectTransform.offsetMax = new Vector2(-10f, -2f);
                line1.text = e.Line1;
                if (two)
                {
                    var line2 = UiBuilder.Text("Why", tag.Content, 17, TvColors.Ink, false, TextAlignmentOptions.Left);
                    UiBuilder.Fit(line2, 11, 17, 1);
                    UiBuilder.Anchor(line2.rectTransform, 0, 0f, 1, 0.5f);
                    line2.rectTransform.offsetMin = new Vector2(50f, 3f);
                    line2.rectTransform.offsetMax = new Vector2(-10f, 0f);
                    line2.text = e.Line2;
                }

                TvTween.On(rect).Pop(0.05f * i, 0.35f);
                _tags.Add(rect.gameObject);
            }
        }

        public void Clear()
        {
            Clear(true);
        }

        private void Clear(bool forget)
        {
            if (forget) _shown.Clear();
            foreach (var t in _tags) Destroy(t);
            _tags.Clear();
        }

        private static PlayerState FindByName(GameState state, string name)
        {
            foreach (var p in state.Players) if (p.Name == name) return p;
            return null;
        }

        private bool Same(List<string> keys)
        {
            if (keys.Count != _shown.Count) return false;
            for (int i = 0; i < keys.Count; i++) if (keys[i] != _shown[i]) return false;
            return true;
        }
    }
}
