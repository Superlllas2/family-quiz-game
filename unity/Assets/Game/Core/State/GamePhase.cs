namespace FamilyQuiz.Core.State
{
    /// <summary>
    /// The show phases (docs/02_show_flow.md). Day 2 runs Lobby, StartCountdown, then questions x
    /// (QuestionShow, Answer, Reveal, Scores), Results, then back to the Lobby (D76). Reveal and Scores share one phaseId (D28).
    /// </summary>
    public enum GamePhase
    {
        Lobby,
        StartCountdown,
        QuestionShow,
        Answer,
        Reveal,
        Scores,
        Results
    }
}
