namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// The topic vote of a round is over (raised right before PhaseEnded(TopicVote)): GameState.Round.ChosenDoor and
    /// Topic are set. ByTieBreak is true when the votes were tied and the lowest ranked voter or the seeded random decided;
    /// NobodyVoted when the seeded random picked a door.
    /// </summary>
    public sealed class TopicChosen : IGameEvent
    {
        public TopicChosen(int round, int door, string topicId, bool byTieBreak, bool nobodyVoted)
        {
            Round = round;
            Door = door;
            TopicId = topicId;
            ByTieBreak = byTieBreak;
            NobodyVoted = nobodyVoted;
        }

        /// <summary>1 based round number.</summary>
        public int Round { get; }

        public int Door { get; }
        public string TopicId { get; }
        public bool ByTieBreak { get; }
        public bool NobodyVoted { get; }
    }
}
