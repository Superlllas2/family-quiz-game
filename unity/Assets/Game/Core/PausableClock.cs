using System;

namespace FamilyQuiz.Core
{
    /// <summary>
    /// The core clock on top of a time source. It can be paused: while the TV app has lost the relay
    /// the core clock stands still (docs/18_edge_cases.md, "The laptop loses internet").
    /// </summary>
    public sealed class PausableClock : IClock
    {
        private readonly Func<long> _sourceMs;
        private long _offsetMs;
        private long _pausedAtSourceMs;
        private bool _paused;

        public PausableClock(Func<long> sourceMs)
        {
            _sourceMs = sourceMs ?? throw new ArgumentNullException(nameof(sourceMs));
            _offsetMs = _sourceMs();
        }

        public bool IsPaused => _paused;

        public long NowMs => (_paused ? _pausedAtSourceMs : _sourceMs()) - _offsetMs;

        public void Pause()
        {
            if (_paused) return;
            _pausedAtSourceMs = _sourceMs();
            _paused = true;
        }

        public void Resume()
        {
            if (!_paused) return;
            _offsetMs += _sourceMs() - _pausedAtSourceMs;
            _paused = false;
        }
    }
}
