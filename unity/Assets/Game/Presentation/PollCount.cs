using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The count of submitted polls the TV shows in the Poll phase ("Polls: 2 of 5"). Plain C#, so it is tested without
    /// the engine.
    /// </summary>
    public static class PollCount
    {
        /// <summary>
        /// Submitted of those the poll waits for: connected players, plus anyone who submitted before losing the
        /// connection, so done never exceeds total and both match once every connected phone submitted.
        /// </summary>
        public static void Count(GameState state, out int done, out int total)
        {
            done = 0;
            total = 0;
            foreach (var p in state.Players)
            {
                bool submitted = state.SubmittedPoll(p.PlayerId);
                if (submitted) done++;
                if (submitted || p.Connected) total++;
            }
        }
    }
}
