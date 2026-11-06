using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The hosts' comic bubbles until the 3D hosts exist (docs/15_schemas.md, "Selection"; decisions/0017_game_night_ui.md):
    /// in the TvLayout hosts zone at the top left, which no view draws in, one row per speaking host (the first to speak
    /// on top), each a cube badge with the host's name and a speech bubble. At most one bubble per host, a new line
    /// replaces it. The text is drawn in the game language and redrawn when it changes. An overlay above the views; a
    /// listener only: it never changes state and never holds a phase.
    /// </summary>
    public sealed class HostBubblesView : MonoBehaviour
    {
        private readonly HostBubbleSlots _slots = new HostBubbleSlots();
        private readonly Dictionary<string, HostCorner> _corners = new Dictionary<string, HostCorner>();

        /// <summary>The hosts in the order their current lines started: the earlier line keeps the top row.</summary>
        private readonly List<string> _order = new List<string>();
        private TvContext _context;

        public static HostBubblesView Create(Transform parent, TvContext context)
        {
            var rect = TvLayout.Put(UiBuilder.Rect("HostBubbles", parent), TvLayout.Hosts);
            var view = rect.gameObject.AddComponent<HostBubblesView>();
            view._context = context;
            view._corners[HostIds.Konstantin] = HostCorner.Create(rect, HostIds.Konstantin, TvColors.HostKonstantin);
            view._corners[HostIds.Ilja] = HostCorner.Create(rect, HostIds.Ilja, TvColors.HostIlja);
            return view;
        }

        /// <summary>Puts up the host's bubble until endsAtMs on the core clock, replacing that host's bubble.</summary>
        public void Show(string host, Func<string, string> textInLang, long endsAtMs)
        {
            if (!_slots.Show(host, textInLang, endsAtMs)) return;
            _corners[host].ShownLang = null; // a new line: drawn now
            _order.Remove(host);
            _order.Add(host);
            Tick();
        }

        /// <summary>Every frame: shows and hides the bubbles on the core clock and redraws them on a language change.</summary>
        public void Tick()
        {
            var game = _context.Game;
            string lang = _context.Lang;
            int row = 0;
            foreach (var host in _order)
            {
                var corner = _corners[host];
                bool visible = game != null && _slots.Visible(host, game.NowMs);
                if (visible) corner.SetRow(row++);
            }

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
