namespace FamilyQuiz.Core.Commands
{
    /// <summary>Phone input kind "rate" (PROTOCOL.md section 3, decisions/0018 D109): stars for one item of the rating card.</summary>
    public sealed class RateQuestion : IGameCommand
    {
        public RateQuestion(string conn, string inputId, long phaseId, string item, double? stars)
        {
            Conn = conn;
            InputId = inputId;
            PhaseId = phaseId;
            Item = item;
            Stars = stars;
        }

        public string Conn { get; }
        public string InputId { get; }
        public long PhaseId { get; }

        /// <summary>An id from config.json feedback.items.</summary>
        public string Item { get; }

        /// <summary>1 to feedback.stars; anything else, or not a whole number, is invalid.</summary>
        public double? Stars { get; }
    }
}
