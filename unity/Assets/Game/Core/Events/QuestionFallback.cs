namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A warning (docs/02_show_flow.md, "Questions"): the chosen topic had no unseen question for a slot, so the question
    /// came from group 2, 3 or 4. Telemetry logs it as an error line with where "content".
    /// </summary>
    public sealed class QuestionFallback : IGameEvent
    {
        public QuestionFallback(int round, int slot, string topicId, string questionId, int group)
        {
            Round = round;
            Slot = slot;
            TopicId = topicId;
            QuestionId = questionId;
            Group = group;
        }

        /// <summary>1 based round and slot.</summary>
        public int Round { get; }

        public int Slot { get; }

        /// <summary>The chosen topic, or the mixed topic id.</summary>
        public string TopicId { get; }

        public string QuestionId { get; }

        /// <summary>The group the question came from: 2 unseen any topic, 3 seen chosen topic, 4 seen any topic.</summary>
        public int Group { get; }
    }
}
