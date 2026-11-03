namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// The seen lists changed (docs/02_show_flow.md, "Seen questions"): a question or race statement was shown, or
    /// "Reset seen questions" cleared both. A listener saves GameState.SeenQuestionIds and SeenRaceIds to disk on it.
    /// </summary>
    public sealed class SeenChanged : IGameEvent
    {
        public SeenChanged(bool reset)
        {
            Reset = reset;
        }

        /// <summary>True when the lists were cleared.</summary>
        public bool Reset { get; }
    }
}
