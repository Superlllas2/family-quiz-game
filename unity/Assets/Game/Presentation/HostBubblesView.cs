using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The hosts' text bubbles until the 3D hosts exist (docs/15_schemas.md, "Selection"): Konstantin bottom left, Ilja
    /// bottom right, each a placeholder portrait box with the host's name and a bubble beside it. A host shows only while
    /// their bubble is up, so the boxes never cover the chips and rows for long; at most one bubble per host, a new line
    /// replaces it. The text is drawn in the game language and redrawn when it changes. An overlay above the views; a
    /// listener only: it never changes state and never holds a phase.
    /// </summary>
    public sealed class HostBubblesView : MonoBehaviour
    {
        private readonly HostBubbleSlots _slots = new HostBubbleSlots();
        private readonly Dictionary<string, HostCorner> _corners = new Dictionary<string, HostCorner>();
        private TvContext _context;

        public static HostBubblesView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("HostBubbles", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<HostBubblesView>();
            view._context = context;
            view._corners[HostIds.Konstantin] = HostCorner.Create(rect, HostIds.Konstantin, false);
            view._corners[HostIds.Ilja] = HostCorner.Create(rect, HostIds.Ilja, true);
            return view;
        }

        /// <summary>Puts up the host's bubble until endsAtMs on the core clock, replacing that host's bubble.</summary>
        public void Show(string host, Func<string, string> textInLang, long endsAtMs)
        {
            if (!_slots.Show(host, textInLang, endsAtMs)) return;
            _corners[host].ShownLang = null; // a new line: drawn now
            Tick();
        }

        /// <summary>Every frame: shows and hides the bubbles on the core clock and redraws them on a language change.</summary>
        public void Tick()
        {
            var game = _context.Game;
            string lang = _context.Lang;
            foreach (var pair in _corners)
            {
                var corner = pair.Value;
                bool visible = game != null && _slots.Visible(pair.Key, game.NowMs);
                corner.SetVisible(visible);
                if (!visible || corner.ShownLang == lang) continue;
                corner.ShownLang = lang;
                corner.SetTexts(_context.T("tv.hosts." + pair.Key), _slots.Text(pair.Key, lang));
            }
        }

        public void HideAll()
        {
            _slots.Clear();
            foreach (var corner in _corners.Values) corner.SetVisible(false);
        }
    }
}
