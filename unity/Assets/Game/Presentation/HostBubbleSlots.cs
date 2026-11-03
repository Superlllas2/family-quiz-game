using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Which host line bubble is up (docs/15_schemas.md, "Selection"): at most one per host, a new line for the same host
    /// replaces it, and it shows until its end time on the core clock. The text is kept as a function of the language so
    /// a language change re-renders it. Plain C#, so it is tested without the engine.
    /// </summary>
    public sealed class HostBubbleSlots
    {
        private readonly Dictionary<string, Slot> _slots = new Dictionary<string, Slot>();

        /// <summary>Puts up a bubble for the host until endsAtMs, replacing that host's bubble. False for an unknown host or no text.</summary>
        public bool Show(string host, Func<string, string> textInLang, long endsAtMs)
        {
            if (!HostIds.IsValid(host) || textInLang == null) return false;
            _slots[host] = new Slot(textInLang, endsAtMs);
            return true;
        }

        /// <summary>True while the host has a bubble whose end time is still ahead of now.</summary>
        public bool Visible(string host, long nowMs)
        {
            return host != null && _slots.TryGetValue(host, out var slot) && nowMs < slot.EndsAtMs;
        }

        /// <summary>The host's bubble text in the language, or null without a bubble.</summary>
        public string Text(string host, string lang)
        {
            if (host == null || !_slots.TryGetValue(host, out var slot)) return null;
            return slot.TextInLang(lang) ?? string.Empty;
        }

        /// <summary>The end time of the host's bubble on the core clock, or null without one.</summary>
        public long? EndsAtMs(string host)
        {
            return host != null && _slots.TryGetValue(host, out var slot) ? slot.EndsAtMs : (long?)null;
        }

        public void Clear()
        {
            _slots.Clear();
        }

        private readonly struct Slot
        {
            public Slot(Func<string, string> textInLang, long endsAtMs)
            {
                TextInLang = textInLang;
                EndsAtMs = endsAtMs;
            }

            public Func<string, string> TextInLang { get; }
            public long EndsAtMs { get; }
        }
    }
}
