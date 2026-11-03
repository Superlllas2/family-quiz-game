namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Durations and sizes of the TV's presentation only animations (pops, fades, row moves). They run on unscaled
    /// time and never hold a phase (pillar 4): when the phase changes, the view changes, finished or not. Not game
    /// timers, so not in content/config.json; the staging timings there (staging.*) come with the 3D set.
    /// </summary>
    public static class TvMotion
    {
        /// <summary>A chip's pop when its player locks in.</summary>
        public const float PopSeconds = 0.25f;

        /// <summary>How much bigger a chip gets at the top of its pop.</summary>
        public const float PopScale = 0.22f;

        /// <summary>How fast tiles grow, shrink and fade on Reveal (fraction of the way per second, exponential).</summary>
        public const float TileEaseSpeed = 10f;

        /// <summary>The correct tile's size on Reveal.</summary>
        public const float CorrectTileScale = 1.08f;

        /// <summary>Opacity of the wrong tiles on Reveal.</summary>
        public const float WrongTileAlpha = 0.25f;

        /// <summary>Leaderboard rows wait this long at their old place before moving.</summary>
        public const float RowMoveDelaySeconds = 0.5f;

        /// <summary>Leaderboard rows take this long to move to their new place.</summary>
        public const float RowMoveSeconds = 0.7f;

        /// <summary>Race chips wait this long on RaceReveal before they step forward (the right answer shows first).</summary>
        public const float RaceMoveDelaySeconds = 0.3f;

        /// <summary>Race chips take this long to slide to their new step; well inside the race reveal.</summary>
        public const float RaceMoveSeconds = 0.6f;

        /// <summary>Opacity of a disconnected player's chip.</summary>
        public const float DisconnectedAlpha = 0.35f;
    }
}
