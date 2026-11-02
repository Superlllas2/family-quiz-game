using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Events
{
    /// <summary>A new (not duplicate) input was applied and is now the player's last input.</summary>
    public sealed class AnswerReceived : IGameEvent
    {
        public AnswerReceived(string playerId, AnswerRecord answer)
        {
            PlayerId = playerId;
            Answer = answer;
        }

        public string PlayerId { get; }
        public AnswerRecord Answer { get; }
    }
}
