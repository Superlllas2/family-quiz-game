namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The countdown line on Results (D78): it names what comes next, the Poll when it follows (poll.enabled and
    /// content/poll.json has questions), else the Lobby. Plain C#, so it is tested without the engine.
    /// </summary>
    public static class ResultsCountdown
    {
        public const string PollInKey = "tv.results.pollIn";
        public const string LobbyInKey = "tv.results.lobbyIn";

        public static bool PollFollows(bool pollEnabled, int pollQuestionCount)
        {
            return pollEnabled && pollQuestionCount > 0;
        }

        /// <summary>The ui_strings key of the line; it has a {seconds} placeholder.</summary>
        public static string Key(bool pollEnabled, int pollQuestionCount)
        {
            return PollFollows(pollEnabled, pollQuestionCount) ? PollInKey : LobbyInKey;
        }
    }
}
