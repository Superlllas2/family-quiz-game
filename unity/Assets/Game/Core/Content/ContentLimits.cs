namespace FamilyQuiz.Core.Content
{
    /// <summary>Limits fixed by docs/15_schemas.md (schema rules, not tuning values, so not in config.json).</summary>
    public static class ContentLimits
    {
        /// <summary>questions.json "text": at most 120 characters per language.</summary>
        public const int QuestionTextMax = 120;

        /// <summary>questions.json "options": at most 40 characters each.</summary>
        public const int OptionTextMax = 40;

        /// <summary>questions.json "options": a list of 4.</summary>
        public const int OptionCount = 4;

        /// <summary>topics.json "title": at most 30 characters (tools/validate_content.mjs).</summary>
        public const int TopicTitleMax = 30;

        /// <summary>characters.json "name": at most 16 characters per language.</summary>
        public const int CharacterNameMax = 16;

        /// <summary>characters.json "personality": at most 60 characters.</summary>
        public const int CharacterPersonalityMax = 60;

        /// <summary>characters.json "color": the chip text color the signature color must contrast with.</summary>
        public const string ChipTextColor = "#14161C";

        /// <summary>characters.json "color": a contrast ratio of at least 4.5 against <see cref="ChipTextColor"/>.</summary>
        public const double MinChipContrast = 4.5;
    }
}
