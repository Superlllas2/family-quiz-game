namespace FamilyQuiz.Net.Protocol
{
    /// <summary>The screen ids the TV app sends (PROTOCOL.md section 4, docs/06_phone_screens.md).</summary>
    public static class ScreenIds
    {
        public const string Lobby = "lobby";
        public const string PickCharacter = "pickCharacter";
        public const string Countdown = "countdown";
        public const string Watch = "watch";
        public const string TopicVote = "topicVote";
        public const string AnswerChoice = "answerChoice";
        public const string AnswerSlider = "answerSlider";
        public const string AnswerTrueFalse = "answerTrueFalse";
        public const string Waiting = "waiting";
        public const string Result = "result";
        public const string Results = "results";
        public const string Poll = "poll";
        public const string PollDone = "pollDone";
    }
}
