namespace FamilyQuiz.Core.Events
{
    /// <summary>A player voted for a topic door in TopicVote (TV live vote counts; log event input with kind vote).</summary>
    public sealed class VoteCast : IGameEvent
    {
        public VoteCast(string playerId, long phaseId, int door)
        {
            PlayerId = playerId;
            PhaseId = phaseId;
            Door = door;
        }

        public string PlayerId { get; }
        public long PhaseId { get; }

        /// <summary>0 based door index; the last door is the mixed door.</summary>
        public int Door { get; }
    }
}
