using System;
using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// Loads and validates the content the game uses (docs/15_schemas.md): config.json, topics.json, questions.json,
    /// characters.json and ui_strings.json. Files are read through the injected reader, so Core never touches the disk.
    /// Any error throws a <see cref="ContentException"/> naming the file, the item id and the field; the game must not
    /// start then.
    /// </summary>
    public sealed class ContentLoader
    {
        private readonly Func<string, string> _read;

        /// <param name="read">Returns the text of a content file by its file name, for example "config.json".</param>
        public ContentLoader(Func<string, string> read)
        {
            _read = read ?? throw new ArgumentNullException(nameof(read));
        }

        public GameContent Load()
        {
            var config = ConfigLoader.Parse(_read(ConfigLoader.FileName));
            var topics = TopicLoader.Parse(_read(TopicLoader.FileName));
            CheckMixedTopic(config, topics);
            var questions = QuestionLoader.Parse(_read(QuestionLoader.FileName), topics);
            CheckBankHoldsAGame(config, questions);
            var characters = CharacterLoader.Parse(_read(CharacterLoader.FileName), config);
            var strings = UiStrings.Parse(_read(UiStrings.FileName));
            return new GameContent(config, characters, questions, strings, topics);
        }

        /// <summary>topics.mixedTopicId must name the topic marked mixed (tools/validate_content.mjs).</summary>
        private static void CheckMixedTopic(GameConfig config, IReadOnlyList<TopicDef> topics)
        {
            var errors = new ContentErrors(ConfigLoader.FileName);
            bool found = false;
            foreach (var t in topics)
            {
                if (t.Mixed && t.Id == config.Topics.MixedTopicId) found = true;
            }

            errors.Check(found, "topics.mixedTopicId", "must name the mixed topic of topics.json");
            errors.ThrowIfAny();
        }

        /// <summary>
        /// The bank must hold one game without a repeat (D75, Day 2): for every slot difficulty, as many unused multiple
        /// choice questions as the slots that draw it.
        /// </summary>
        private static void CheckBankHoldsAGame(GameConfig config, IReadOnlyList<QuestionDef> questions)
        {
            var needed = new Dictionary<string, int>();
            for (int slot = 0; slot < config.Rounds.QuestionsPerRound; slot++)
            {
                var d = Rules.QuestionPicker.SlotDifficulty(config.Rounds, slot);
                needed[d] = needed.TryGetValue(d, out var n) ? n + 1 : 1;
            }

            var errors = new ContentErrors(QuestionLoader.FileName);
            foreach (var pair in needed)
            {
                int have = 0;
                foreach (var q in questions)
                {
                    if (q.Format == QuestionDef.MultipleChoice && q.Difficulty == pair.Key) have++;
                }

                errors.Check(have >= pair.Value, "-",
                    "needs at least " + pair.Value + " multipleChoice questions of difficulty " + pair.Key + " for one game, has " + have);
            }

            errors.ThrowIfAny();
        }
    }
}
