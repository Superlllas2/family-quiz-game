namespace FamilyQuiz.Core.Rules
{
    /// <summary>What a player did on one race statement (D157).</summary>
    public enum RaceOutcome
    {
        /// <summary>No answer before the deadline: no move.</summary>
        None,

        /// <summary>The Пас button: no move.</summary>
        Pass,

        /// <summary>A right answer: race.stepsPerCorrect forward.</summary>
        Right,

        /// <summary>A wrong answer: race.stepsPerWrong back, never below 0.</summary>
        Wrong
    }
}
