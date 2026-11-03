using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// Everything loaded and validated at startup: config, characters, questions, topics, race statements, facts, show scripts
    /// and the TV app's texts.
    /// </summary>
    public sealed class GameContent
    {
        public GameContent(GameConfig config, IReadOnlyList<CharacterDef> characters, IReadOnlyList<QuestionDef> questions,
            UiStrings strings, IReadOnlyList<TopicDef> topics, IReadOnlyList<RaceStatementDef> race, ShowScripts scripts,
            IReadOnlyList<FactDef> facts = null)
        {
            Config = config;
            Characters = characters;
            Questions = questions;
            Strings = strings;
            Topics = topics ?? new List<TopicDef>();
            Race = race ?? new List<RaceStatementDef>();
            Scripts = scripts;
            Facts = facts ?? new List<FactDef>();
        }

        public GameConfig Config { get; }
        public IReadOnlyList<CharacterDef> Characters { get; }
        public IReadOnlyList<QuestionDef> Questions { get; }
        public UiStrings Strings { get; }

        /// <summary>content/topics.json, the mixed topic included.</summary>
        public IReadOnlyList<TopicDef> Topics { get; }

        /// <summary>content/race.json: the true or false statements of the final race.</summary>
        public IReadOnlyList<RaceStatementDef> Race { get; }

        /// <summary>content/facts.json: the winner's reward facts (D87).</summary>
        public IReadOnlyList<FactDef> Facts { get; }

        /// <summary>content/show_scripts.json.</summary>
        public ShowScripts Scripts { get; }

        public TopicDef Topic(string id)
        {
            foreach (var t in Topics)
            {
                if (t.Id == id) return t;
            }

            return null;
        }
    }
}
