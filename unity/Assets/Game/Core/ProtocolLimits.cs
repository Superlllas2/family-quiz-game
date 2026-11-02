namespace FamilyQuiz.Core
{
    /// <summary>Values fixed by docs/PROTOCOL.md (not tunable, so not in config.json).</summary>
    public static class ProtocolLimits
    {
        /// <summary>PROTOCOL.md section 3, "Names": 1 to 16 characters after trimming.</summary>
        public const int MaxNameLength = 16;

        /// <summary>PROTOCOL.md section 4: "token: random 24 characters".</summary>
        public const int TokenLength = 24;

        /// <summary>PROTOCOL.md section 3, "input kinds": choice is 0 to 3.</summary>
        public const int ChoiceCount = 4;
    }
}
