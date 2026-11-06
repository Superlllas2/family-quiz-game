using System;
using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// Loads and validates the content the game uses (docs/15_schemas.md): config.json, topics.json, questions.json,
    /// race.json, facts.json, show_scripts.json, characters.json, ui_strings.json, host_lines.json and poll.json. Files are read through the injected reader,
    /// so Core never touches the disk. Any error throws a <see cref="ContentException"/> naming the file, the item id
    /// and the field; the game must not start then.
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
            // The curated test bank (decisions/0020) is optional: without its file there is only the full bank.
            var curatedText = ReadOptional(CuratedFileName);
            IReadOnlyList<QuestionDef> curated = null;
            if (curatedText != null)
            {
                curated = QuestionLoader.Parse(curatedText, topics, CuratedFileName);
                CheckBankHoldsAGame(config, curated);
                CheckCurated(curated, questions);
            }
            var race = RaceLoader.Parse(_read(RaceLoader.FileName), config);
            var scripts = ShowScripts.Parse(_read(ShowScripts.FileName), config);
            var characters = CharacterLoader.Parse(_read(CharacterLoader.FileName), config);
            var strings = UiStrings.Parse(_read(UiStrings.FileName));
            var facts = FactLoader.Parse(_read(FactLoader.FileName));
            var hostLines = HostLineLoader.Parse(_read(HostLineLoader.FileName));
            var poll = PollLoader.Parse(_read(PollLoader.FileName));
            return new GameContent(config, characters, questions, strings, topics, race, scripts, facts, hostLines, poll, curated);
        }

        /// <summary>The curated test bank (decisions/0020).</summary>
        public const string CuratedFileName = "questions_refined.json";

        private string ReadOptional(string name)
        {
            try
            {
                return _read(name);
            }
            catch (System.IO.IOException)
            {
                return null;
            }
        }

        /// <summary>
        /// Curated ids never repeat a full bank id; basedOn names a full bank question for same and rewrite and is null
        /// for new; a same question is an exact copy of its full bank question's text and answer.
        /// </summary>
        private static void CheckCurated(IReadOnlyList<QuestionDef> curated, IReadOnlyList<QuestionDef> full)
        {
            var errors = new ContentErrors(CuratedFileName);
            var byId = new Dictionary<string, QuestionDef>();
            foreach (var q in full) byId[q.Id] = q;
            foreach (var q in curated)
            {
                errors.Check(!byId.ContainsKey(q.Id), q.Id, "id is also a full bank id");
                errors.Check(q.Refine == "same" || q.Refine == "rewrite" || q.Refine == "new", q.Id, "refine must be same, rewrite or new");
                if (q.Refine == "new")
                {
                    errors.Check(q.BasedOn == null, q.Id, "a new question has basedOn null");
                    continue;
                }

                if (!byId.TryGetValue(q.BasedOn ?? "", out var original))
                {
                    errors.Add(q.Id, "basedOn \"" + q.BasedOn + "\" is not a full bank question");
                    continue;
                }

                errors.Check(original.Topic == q.Topic, q.Id, "basedOn is a question of another topic");
                if (q.Refine == "same")
                {
                    errors.Check(original.Text.Ru == q.Text.Ru && original.Text.En == q.Text.En && original.Correct == q.Correct
                                 && original.Answer == q.Answer, q.Id, "a same question must be an exact copy of " + q.BasedOn);
                }
            }

            errors.ThrowIfAny();
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
        /// The bank must hold one Standard game without a repeat (docs/02_show_flow.md, "Questions"): for every slot kind
        /// (rounds.formatOrder and rounds.difficultyOrder), rounds.count times as many questions as the slots of that kind.
        /// </summary>
        private static void CheckBankHoldsAGame(GameConfig config, IReadOnlyList<QuestionDef> questions)
        {
            var needed = new Dictionary<(string Format, string Difficulty), int>();
            for (int slot = 0; slot < config.Rounds.QuestionsPerRound; slot++)
            {
                var kind = (config.Rounds.FormatOrder[slot], config.Rounds.DifficultyOrder[slot]);
                needed[kind] = (needed.TryGetValue(kind, out var n) ? n : 0) + config.Rounds.Count;
            }

            var errors = new ContentErrors(QuestionLoader.FileName);
            foreach (var pair in needed)
            {
                int have = 0;
                foreach (var q in questions)
                {
                    if (q.Format == pair.Key.Format && q.Difficulty == pair.Key.Difficulty) have++;
                }

                errors.Check(have >= pair.Value, "-",
                    "needs at least " + pair.Value + " " + pair.Key.Format + " questions of difficulty " + pair.Key.Difficulty
                    + " for one game, has " + have);
            }

            errors.ThrowIfAny();
        }
    }
}
