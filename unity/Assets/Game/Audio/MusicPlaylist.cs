using System;
using System.Collections.Generic;
using FamilyQuiz.Core;

namespace FamilyQuiz.Audio
{
    /// <summary>
    /// The order of the background tracks (decisions/0019, D113): a shuffle bag. Every track plays once before any
    /// plays again, and a new round never starts with the track that just ended, so nothing repeats back to back.
    /// Plain C#, so it is tested without the engine.
    /// </summary>
    public sealed class MusicPlaylist
    {
        private readonly int _count;
        private readonly IRandom _random;
        private readonly List<int> _bag = new List<int>();
        private int _last = -1;

        public MusicPlaylist(int count, IRandom random)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), "needs at least one track");
            _count = count;
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>The index of the next track to play.</summary>
        public int Next()
        {
            if (_bag.Count == 0) Refill();
            int next = _bag[0];
            _bag.RemoveAt(0);
            _last = next;
            return next;
        }

        private void Refill()
        {
            for (int i = 0; i < _count; i++) _bag.Add(i);
            for (int i = _bag.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
            }

            // The round's first track is never the one that just ended.
            if (_count > 1 && _bag[0] == _last)
            {
                int j = 1 + _random.Next(_count - 1);
                (_bag[0], _bag[j]) = (_bag[j], _bag[0]);
            }
        }
    }
}
