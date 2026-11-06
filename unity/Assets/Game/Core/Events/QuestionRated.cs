namespace FamilyQuiz.Core.Events
{
    /// <summary>A player rated one item of the current question (decisions/0018, D109 and D111; log event questionRated).</summary>
    public sealed class QuestionRated : IGameEvent
    {
        public QuestionRated(string playerId, string questionId, string topic, string format, string difficulty, string item,
            int stars, bool? correct, long phaseId)
        {
            PlayerId = playerId;
            QuestionId = questionId;
            Topic = topic;
            Format = format;
            Difficulty = difficulty;
            Item = item;
            Stars = stars;
            Correct = correct;
            PhaseId = phaseId;
        }

        public string PlayerId { get; }
        public string QuestionId { get; }
        public string Topic { get; }
        public string Format { get; }
        public string Difficulty { get; }
        public string Item { get; }
        public int Stars { get; }

        /// <summary>Whether the player answered the question correctly; null when they sent no answer.</summary>
        public bool? Correct { get; }

        public long PhaseId { get; }
    }
}
