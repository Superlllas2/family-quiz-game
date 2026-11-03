namespace FamilyQuiz.Core.State
{
    /// <summary>
    /// The show phases (docs/02_show_flow.md). A Standard game runs Intro (first game of the session only), then per
    /// round TopicVote, questions x (QuestionShow, Answer, Reveal, Scores) and RoundEnd, then FinalIntro, the race as
    /// RaceQuestion and RaceReveal per statement, Results, and back to the Lobby (D76). Reveal and Scores share one
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
        Results
    }
}
