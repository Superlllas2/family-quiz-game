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
        private readonly Dictionary<string, Func<string, string>> _texts = new Dictionary<string, Func<string, string>>();
        private readonly Dictionary<string, long> _endsAtMs = new Dictionary<string, long>();

        /// <summary>Puts up a bubble for the host until endsAtMs, replacing that host's bubble. False for an unknown host or no text.</summary>
        public bool Show(string host, Func<string, string> textInLang, long endsAtMs)
        {
            if (!HostIds.IsValid(host) || textInLang == null) return false;
            _texts[host] = textInLang;
            _endsAtMs[host] = endsAtMs;
            return true;
        }

        /// <summary>True while the host has a bubble whose end time is still ahead of now.</summary>
        public bool Visible(string host, long nowMs)
        {
            return host != null && _endsAtMs.TryGetValue(host, out long end) && nowMs < end;
        }

        /// <summary>The host's bubble text in the language, or null without a bubble.</summary>
        public string Text(string host, string lang)
        {
            if (host == null || !_texts.TryGetValue(host, out var text)) return null;
            return text(lang) ?? string.Empty;
        }

        /// <summary>The end time of the host's bubble on the core clock, or null without one.</summary>
        public long? EndsAtMs(string host)
        {
            return host != null && _endsAtMs.TryGetValue(host, out long end) ? end : (long?)null;
        }

        public void Clear()
        {
            _texts.Clear();
            _endsAtMs.Clear();
        }
    }
}
