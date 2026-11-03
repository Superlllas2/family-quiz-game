using System.Collections.Generic;

namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A phone submitted the poll. PlayerId is for the phone screens only (pollDone); the log line (event poll) never
    /// carries it (docs/15_schemas.md). Answers: question id to a string (text) or an int option index (choice), checked
    /// against content/poll.json, in its order.
    /// </summary>
    public sealed class PollSubmitted : IGameEvent
    {
        public PollSubmitted(string playerId, IReadOnlyList<KeyValuePair<string, object>> answers)
        {
            PlayerId = playerId;
            Answers = answers;
        }

        public string PlayerId { get; }
        public IReadOnlyList<KeyValuePair<string, object>> Answers { get; }
    }
}
