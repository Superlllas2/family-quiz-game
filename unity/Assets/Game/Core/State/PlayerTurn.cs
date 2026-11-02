namespace FamilyQuiz.Core.State
{
    /// <summary>
    /// One player's part of the current question: personal window and deadline (docs/03_rules.md, "Timer multiplier
    /// and personal deadlines"), the locked answer and, after Reveal, the result. A fresh one per question.
    /// </summary>
    public sealed class PlayerTurn
    {
        /// <summary>T: the base window times the player's timer multiplier at the start of Answer. Last call never changes it.</summary>
        public long WindowMs { get; internal set; }

        /// <summary>Core clock time of the personal deadline; last call may move it earlier.</summary>
        public long DeadlineMs { get; internal set; }

        /// <summary>True once last call cut this player's deadline.</summary>
        public bool LastCall { get; internal set; }

        /// <summary>The input id that locked the answer, or null.</summary>
        public string InputId { get; internal set; }

        /// <summary>Display index (0 to 3) the player locked, or null for no answer.</summary>
        public int? Choice { get; internal set; }

        /// <summary>The phone's ms as sent.</summary>
        public long? RawMs { get; internal set; }

        /// <summary>The phone's ms clamped to 0..WindowMs: the t of the scoring formula.</summary>
        public long? Ms { get; internal set; }

        /// <summary>Core clock time the answer was applied.</summary>
        public long? LockedAtMs { get; internal set; }

        public bool Locked => Choice.HasValue;

        /// <summary>True once the player reached DeadlineMs plus net.inputGraceMs without locking in (PlayerTimedOut raised).</summary>
        public bool TimedOut { get; internal set; }

        /// <summary>Shared rank before this question.</summary>
        public int RankBefore { get; internal set; }

        /// <summary>True once Reveal scored this turn.</summary>
        public bool Revealed { get; internal set; }

        public bool Correct { get; internal set; }

        /// <summary>Points earned on this question, multiplier included. Set at Reveal.</summary>
        public int Points { get; internal set; }
    }
}
