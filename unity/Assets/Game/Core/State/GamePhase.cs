namespace FamilyQuiz.Core.State
{
    /// <summary>
    /// The show phases (docs/02_show_flow.md). A Standard game runs Intro (first game of the session only), then per
    /// round TopicVote, questions x (QuestionShow, Answer, Reveal, Scores) and RoundEnd, then FinalIntro, the race as
    /// RaceQuestion and RaceReveal per statement, Results, the Poll, and back to the Lobby (D76). Reveal and Scores share one
    /// phaseId (D28). The race's "RaceQuestion" of docs/02_show_flow.md is split in two phases, its answer part
    /// (RaceQuestion) and its 2 s reveal (RaceReveal), each with its own phaseId.
    /// </summary>
    public enum GamePhase
    {
        Lobby,
        StartCountdown,
        Intro,
        TopicVote,
        QuestionShow,
        Answer,
        Reveal,
        Scores,
        RoundEnd,
        FinalIntro,
        RaceQuestion,
        RaceReveal,
        Results,
        Poll,

        /// <summary>The spin after a tied topic vote, or one nobody joined (D153). Phones show watch.</summary>
        TopicTie,

        /// <summary>
        /// After Scores, only when a power up was used in that pick window (D149): every attack of the question at once.
        /// Keeps the phaseId of Reveal and Scores, so the pick window runs through it.
        /// </summary>
        Attacks
    }
}
