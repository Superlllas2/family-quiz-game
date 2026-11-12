namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>One moment of the Lobby tour (<see cref="LobbyTour"/>).</summary>
    public readonly struct LobbyTourFrame
    {
        public LobbyTourFrame(long cut, int move, double progress, double alpha)
        {
            Cut = cut;
            Move = move;
            Progress = progress;
            Alpha = alpha;
        }

        /// <summary>How many cuts came before this one since the Lobby began.</summary>
        public long Cut { get; }

        /// <summary>Which move of the tour plays.</summary>
        public int Move { get; }

        /// <summary>How far through the move, 0 at its start to 1 at its end.</summary>
        public double Progress { get; }

        /// <summary>How much of the picture shows: 0 black (the fades), 1 the studio in full.</summary>
        public double Alpha { get; }
    }
}
