using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Everything loaded and validated at startup: config, characters, questions and the TV app's texts.</summary>
    public sealed class GameContent
    {
        public GameContent(GameConfig config, IReadOnlyList<CharacterDef> characters, IReadOnlyList<QuestionDef> questions,
            UiStrings strings)
        {
            Config = config;
            Characters = characters;
            Questions = questions;
            Strings = strings;
        }

        public GameConfig Config { get; }
        public IReadOnlyList<CharacterDef> Characters { get; }
        public IReadOnlyList<QuestionDef> Questions { get; }
        public UiStrings Strings { get; }
    }
}
