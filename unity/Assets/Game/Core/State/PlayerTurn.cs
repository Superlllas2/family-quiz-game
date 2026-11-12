using System.Collections.Generic;
using System.Linq;

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

        /// <summary>Race: true once the player tapped Пас (D157): locked, no answer, no move.</summary>
        public bool Passed { get; internal set; }

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
        /// Right answer. Closest value: the guess is within the tolerance (D83; it may score points further out, D156,
        /// and the closest bonus alone never makes a guess correct). Race: the statement was judged right.
        /// </summary>
        public bool Correct { get; internal set; }

        /// <summary>Points earned on this question, multiplier and closest bonus included. Set at Reveal. 0 in the race.</summary>
        public int Points { get; internal set; }

        /// <summary>Closest value: |guess - answer|, or null without a guess.</summary>
        public double? Distance { get; internal set; }

        /// <summary>Closest value: one of the closest guesses (gets the closest bonus and the crown).</summary>
        public bool Closest { get; internal set; }

        /// <summary>
        /// The power ups that landed on this player when this Answer phase started, in the order they were used
        /// (up to powerUps.maxHitsPerQuestion). The phone clears them one after another (D145).
        /// </summary>
        public IReadOnlyList<TurnEffect> Effects => _effects;

        private readonly List<TurnEffect> _effects = new List<TurnEffect>();

        internal void AddEffect(string powerUp, string attackerName) => _effects.Add(new TurnEffect(powerUp, attackerName));

        /// <summary>The effect the phone shows now: the first one not cleared yet, or null.</summary>
        public TurnEffect CurrentEffect => _effects.FirstOrDefault(e => !e.Cleared);

        /// <summary>The power up showing now, or the last one once all are cleared; null when nothing landed.</summary>
        public string Effect => (CurrentEffect ?? _effects.LastOrDefault())?.PowerUp;

        /// <summary>The attacker name of <see cref="Effect"/>.</summary>
        public string EffectFrom => (CurrentEffect ?? _effects.LastOrDefault())?.AttackerName;

        /// <summary>True once the phone reported every effect cleared (input effectCleared).</summary>
        public bool EffectCleared => _effects.Count > 0 && CurrentEffect == null;

        /// <summary>Closest value: the closest bonus included in Points (multiplier applied), else 0.</summary>
        public int Bonus { get; internal set; }
    }
}
