namespace FamilyQuiz.Core.Events
{
    /// <summary>A player's score changed (log event scoreChanged). Rank is the shared rank after the change.</summary>
    public sealed class ScoreChanged : IGameEvent
    {
        public ScoreChanged(string playerId, int delta, int score, int rank)
        {
            PlayerId = playerId;
            Delta = delta;
            Score = score;
            Rank = rank;
        }

        public string PlayerId { get; }
        public int Delta { get; }
        public int Score { get; }
        public int Rank { get; }
    }
}
