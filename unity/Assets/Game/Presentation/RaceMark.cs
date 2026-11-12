using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// What a race turn did to its runner on RaceReveal (D157): a right answer one step forward, a wrong one a step back
    /// (never below the start), Пас and no answer stay. Read from the revealed turn; the step itself always comes from
    /// RaceState. Plain C#, tested without the engine.
    /// </summary>
    public static class RaceMark
    {
        public enum Kind
        {
            None,
            Forward,
            Back,
            Pass,
        }

        public static Kind For(PlayerTurn turn)
        {
            if (turn == null || !turn.Locked) return Kind.None;
            if (turn.Passed) return Kind.Pass;
            if (!turn.Revealed) return Kind.None;
            return turn.Correct ? Kind.Forward : Kind.Back;
        }
    }
}
