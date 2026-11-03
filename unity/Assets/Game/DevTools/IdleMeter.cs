using System;

namespace FamilyQuiz.DevTools
{
    /// <summary>
    /// The live idle share of the current game (docs/15_schemas.md, "Idle"): a moment is idle when no connected phone
    /// can send an input. Fed once per frame with the core clock; each interval counts with the state seen at its
    /// start. Also keeps the longest idle stretch outside the phases left out of it. Plain C#, no engine.
    /// </summary>
    public sealed class IdleMeter
    {
        private long _lastMs;
        private bool _lastIdle;
        private bool _lastCountsForStretch;

        public bool Running { get; private set; }

        /// <summary>True once a game has been measured to its end.</summary>
        public bool Finished { get; private set; }

        public long GameMs { get; private set; }

        public long IdleMs { get; private set; }

        public long CurrentStretchMs { get; private set; }

        public long LongestStretchMs { get; private set; }

        /// <summary>Idle time divided by game time, 0 before any game time.</summary>
        public double IdleShare => GameMs > 0 ? (double)IdleMs / GameMs : 0d;

        /// <summary>A new game started at startMs (GameStarted): clears everything.</summary>
        /// <param name="idle">Whether that moment is idle.</param>
        /// <param name="countsForStretch">False in phases left out of the longest stretch (Intro, FinalIntro, Results).</param>
        public void Start(long startMs, bool idle, bool countsForStretch)
        {
            Running = true;
            Finished = false;
            GameMs = 0;
            IdleMs = 0;
            CurrentStretchMs = 0;
            LongestStretchMs = 0;
            _lastMs = startMs;
            _lastIdle = idle;
            _lastCountsForStretch = countsForStretch;
        }

        /// <summary>Adds the time since the last sample with the state seen then, and remembers the state now.</summary>
        public void Sample(long nowMs, bool idle, bool countsForStretch)
        {
            if (!Running) return;
            long delta = Math.Max(0, nowMs - _lastMs);
            if (nowMs > _lastMs) _lastMs = nowMs;
            GameMs += delta;
            if (_lastIdle)
            {
                IdleMs += delta;
                if (_lastCountsForStretch)
                {
                    CurrentStretchMs += delta;
                    if (CurrentStretchMs > LongestStretchMs) LongestStretchMs = CurrentStretchMs;
                }
            }

            _lastIdle = idle;
            _lastCountsForStretch = countsForStretch;
            if (!idle || !countsForStretch) CurrentStretchMs = 0;
        }

        /// <summary>The game ended at endMs (GameEnded): count up to it, then freeze the numbers.</summary>
        public void Stop(long endMs)
        {
            if (!Running) return;
            Sample(endMs, _lastIdle, _lastCountsForStretch);
            Running = false;
            Finished = true;
        }
    }
}
