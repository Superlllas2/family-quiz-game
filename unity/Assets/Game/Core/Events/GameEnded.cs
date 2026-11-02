namespace FamilyQuiz.Core.Events
{
    /// <summary>A game ended (log event gameEnded): at the end of Results ("finished") or by Back to lobby ("backToLobby").</summary>
    public sealed class GameEnded : IGameEvent
    {
        public const string ReasonFinished = "finished";
        public const string ReasonBackToLobby = "backToLobby";

        public GameEnded(int game, string reason, string winnerId, long durationMs)
        {
            Game = game;
            Reason = reason;
            WinnerId = winnerId;
            DurationMs = durationMs;
        }

        public int Game { get; }
        public string Reason { get; }
        public string WinnerId { get; }
        public long DurationMs { get; }
    }
}
