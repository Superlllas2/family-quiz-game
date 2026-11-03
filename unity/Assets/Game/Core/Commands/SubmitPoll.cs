using System.Collections.Generic;

namespace FamilyQuiz.Core.Commands
{
    /// <summary>
    /// Phone message "poll" (PROTOCOL.md section 3): the answers as sent, question id to a string, a whole number or
    /// another JSON value (Net converts it; Core keeps only what fits content/poll.json). Null when answers was not an object.
    /// </summary>
    public sealed class SubmitPoll : IGameCommand
    {
        public SubmitPoll(string conn, string inputId, IReadOnlyDictionary<string, object> answers)
        {
            Conn = conn;
            InputId = inputId;
            Answers = answers;
        }

        public string Conn { get; }
        public string InputId { get; }
        public IReadOnlyDictionary<string, object> Answers { get; }
    }
}
