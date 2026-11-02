using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Everything loaded and validated at startup: config, characters, questions, topics and the TV app's texts.</summary>
    public sealed class GameContent
    {
        public GameContent(GameConfig config, IReadOnlyList<CharacterDef> characters, IReadOnlyList<QuestionDef> questions,
            UiStrings strings)
            : this(config, characters, questions, strings, new List<TopicDef>())
        {
        }

        public GameContent(GameConfig config, IReadOnlyList<CharacterDef> characters, IReadOnlyList<QuestionDef> questions,
            UiStrings strings, IReadOnlyList<TopicDef> topics)
        {
            Config = config;
            Characters = characters;
            Questions = questions;
            Strings = strings;
            Topics = topics;
        }

        public GameConfig Config { get; }
        public IReadOnlyList<CharacterDef> Characters { get; }
        public IReadOnlyList<QuestionDef> Questions { get; }
        public UiStrings Strings { get; }

        /// <summary>content/topics.json (Day 2 only checks question topics against it; the topic vote comes on Day 3).</summary>
        public IReadOnlyList<TopicDef> Topics { get; }
    }
}
