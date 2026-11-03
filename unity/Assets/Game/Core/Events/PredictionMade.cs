namespace FamilyQuiz.Core.Events
{
    /// <summary>A player tapped who they think wins tonight during the Intro (log event input with kind prediction).</summary>
    public sealed class PredictionMade : IGameEvent
    {
        public PredictionMade(string playerId, long phaseId, string predictedPlayerId)
        {
            PlayerId = playerId;
            PhaseId = phaseId;
            PredictedPlayerId = predictedPlayerId;
        }

        public string PlayerId { get; }
        public long PhaseId { get; }
        public string PredictedPlayerId { get; }
    }
}
