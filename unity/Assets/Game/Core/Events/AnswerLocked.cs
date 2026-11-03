namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A player locked an answer in Answer or RaceQuestion (TV chip pops; log event input). Kind is the input kind:
    /// choice (Choice is the display index), value (Value is the guess) or trueFalse (TrueFalse). Ms is clamped to 0..T.
    /// Correct and Points are what the answer scores at the reveal, except the closest value bonus, which only the
    /// reveal knows (Points is then the distance score alone). Race answers score no points.
    /// </summary>
    public sealed class AnswerLocked : IGameEvent
    {
        public AnswerLocked(string playerId, long phaseId, string kind, int choice, long ms, bool correct, int points,
            double? value = null, bool? trueFalse = null)
        {
            PlayerId = playerId;
            PhaseId = phaseId;
            Kind = kind;
            Choice = choice;
            Ms = ms;
            Correct = correct;
            Points = points;
            Value = value;
            TrueFalse = trueFalse;
        }

        public string PlayerId { get; }
        public long PhaseId { get; }
        public string Kind { get; }

        /// <summary>The display index for a choice, else -1.</summary>
        public int Choice { get; }

        public long Ms { get; }
        public bool Correct { get; }
        public int Points { get; }

        /// <summary>The guess for a value, else null.</summary>
        public double? Value { get; }

        /// <summary>The answer for a trueFalse, else null.</summary>
        public bool? TrueFalse { get; }
    }
}
