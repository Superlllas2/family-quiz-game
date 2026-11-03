namespace FamilyQuiz.Core.State
{
    /// <summary>
    /// What a player did in the current game, for the funny tags on Results (D86, <see cref="Rules.Tags"/>). Reset when a
    /// game starts. Only Core changes it.
    /// </summary>
    public sealed class PlayerStats
    {
        /// <summary>Questions and race statements answered (locked in).</summary>
        public int Answered { get; internal set; }

        /// <summary>Questions and race statements with no answer by the deadline.</summary>
        public int Unanswered { get; internal set; }

        /// <summary>Wrong answers: a wrong option, a closest value guess outside the tolerance, a wrong true or false.</summary>
        public int Wrong { get; internal set; }

        /// <summary>Wrong answers locked in faster than scoring.fastAnswerMs.</summary>
        public int HastyWrong { get; internal set; }

        /// <summary>Sum over answers of ms / the player's window: the share of the window used, 0 to 1 each.</summary>
        public double WindowShareSum { get; internal set; }

        /// <summary>Times this player was the last one left and got the last call.</summary>
        public int LastCalls { get; internal set; }

        /// <summary>The biggest closest value miss as distance / tolerance (above 1: outside the tolerance).</summary>
        public double WorstMiss { get; internal set; }

        /// <summary>Topic votes for a door that did not win.</summary>
        public int LostVotes { get; internal set; }

        /// <summary>Steps moved in the final race (head start not counted).</summary>
        public int RaceSteps { get; internal set; }

        /// <summary>The average share of the window used per answer, or 0 without answers.</summary>
        public double AverageWindowShare => Answered > 0 ? WindowShareSum / Answered : 0;
    }
}
