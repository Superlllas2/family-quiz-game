namespace FamilyQuiz.Core.State
{
    /// <summary>
    /// One player's part of the current question or race statement: personal window and deadline (docs/03_rules.md,
    /// "Timer multiplier and personal deadlines"), the locked answer and, after the reveal, the result. A fresh one per
    /// question and per race statement.
    /// </summary>
    public sealed class PlayerTurn
    {
        /// <summary>T: the base window times the player's timer multiplier at the start of the phase. Last call never changes it.</summary>
        public long WindowMs { get; internal set; }

        /// <summary>Core clock time of the personal deadline; last call may move it earlier.</summary>
        public long DeadlineMs { get; internal set; }

        /// <summary>True once last call cut this player's deadline.</summary>
        public bool LastCall { get; internal set; }

        /// <summary>The input id that locked the answer, or null.</summary>
        public string InputId { get; internal set; }

        /// <summary>True once the player locked an answer of any kind.</summary>
        public bool Locked { get; internal set; }

        /// <summary>Multiple choice: display index (0 to 3) the player locked, or null.</summary>
        public int? Choice { get; internal set; }

        /// <summary>Closest value: the locked guess, or null.</summary>
        public double? Value { get; internal set; }

        /// <summary>Race: the locked true or false, or null.</summary>
        public bool? TrueFalse { get; internal set; }

        /// <summary>The phone's ms as sent.</summary>
        public long? RawMs { get; internal set; }

        /// <summary>The phone's ms clamped to 0..WindowMs: the t of the scoring formula.</summary>
        public long? Ms { get; internal set; }

        /// <summary>Core clock time the answer was applied.</summary>
        public long? LockedAtMs { get; internal set; }

        /// <summary>True once the player reached DeadlineMs plus net.inputGraceMs without locking in (PlayerTimedOut raised).</summary>
        public bool TimedOut { get; internal set; }

        /// <summary>Shared rank before this question.</summary>
        public int RankBefore { get; internal set; }

        /// <summary>True once the reveal scored this turn.</summary>
        public bool Revealed { get; internal set; }

        /// <summary>
        /// Right answer. Closest value: the distance score is above 0 (the closest bonus alone does not make a guess
        /// correct). Race: the statement was judged right.
        /// </summary>
        public bool Correct { get; internal set; }

        /// <summary>Points earned on this question, multiplier and closest bonus included. Set at Reveal. 0 in the race.</summary>
        public int Points { get; internal set; }

        /// <summary>Closest value: |guess - answer|, or null without a guess.</summary>
        public double? Distance { get; internal set; }

        /// <summary>Closest value: one of the closest guesses (gets the closest bonus and the crown).</summary>
        public bool Closest { get; internal set; }

        /// <summary>The power up that landed on this player when this Answer phase started, or null (one per question, maxHitsPerQuestion).</summary>
        public string Effect { get; internal set; }

        /// <summary>The attacker name of <see cref="Effect"/>.</summary>
        public string EffectFrom { get; internal set; }

        /// <summary>True once the phone reported the effect cleared (input effectCleared).</summary>
        public bool EffectCleared { get; internal set; }

        /// <summary>Closest value: the closest bonus included in Points (multiplier applied), else 0.</summary>
        public int Bonus { get; internal set; }
    }
}
