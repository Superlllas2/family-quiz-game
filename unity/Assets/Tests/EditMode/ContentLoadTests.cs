using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// Content loading and validation (docs/15_schemas.md, tools/validate_content.mjs): the real content/ loads, and a
    /// broken file gives a clear error naming the file, the item id and the field.
    /// </summary>
    public sealed class ContentLoadTests
    {
        // ---------------------------------------------------------------- the real files

        [Test]
        public void TheRealContentLoads()
        {
            var content = new ContentLoader(TestContent.Read).Load();
            Assert.That(content.Config.Players.Max, Is.GreaterThanOrEqualTo(content.Config.Players.Min));
            Assert.That(content.Characters.Count, Is.GreaterThanOrEqualTo(content.Config.Players.Max));
            Assert.That(content.Questions.Count, Is.GreaterThan(0));
            Assert.That(content.Topics.Count(t => t.Mixed), Is.EqualTo(1));
            Assert.That(content.Strings.Has("tv.lobby.roomCode"), Is.True);
            var cv = content.Questions.FirstOrDefault(q => q.Format == QuestionDef.ClosestValue);
            if (cv != null)
            {
                Assert.That(cv.Range.Length, Is.EqualTo(2));
                Assert.That(cv.Answer, Is.Not.Null);
                Assert.That(cv.Unit, Is.Not.Null);
            }
        }

        [Test]
        public void TheLoaderReadsEachFileByName()
        {
            var read = new List<string>();
            new ContentLoader(name =>
            {
                read.Add(name);
                return TestContent.Read(name);
            }).Load();
            Assert.That(read, Is.EquivalentTo(new[]
            {
                ConfigLoader.FileName, TopicLoader.FileName, QuestionLoader.FileName, RaceLoader.FileName, ShowScripts.FileName,
                CharacterLoader.FileName, UiStrings.FileName
            }));
        }

        [Test]
        public void ABrokenFileStopsTheLoader()
        {
            var ex = Assert.Throws<ContentException>(() => new ContentLoader(name =>
                name == QuestionLoader.FileName ? "[{" : TestContent.Read(name)).Load());
            Assert.That(ex.File, Is.EqualTo(QuestionLoader.FileName));
            Assert.That(ex.Message, Does.Contain(QuestionLoader.FileName));
        }

        // ---------------------------------------------------------------- config.json

        private static string Config(Action<JObject> change)
        {
            var root = JObject.Parse(TestContent.Read(ConfigLoader.FileName));
            change(root);
            return root.ToString();
        }

        private static ContentException ConfigError(Action<JObject> change)
        {
            return Assert.Throws<ContentException>(() => ConfigLoader.Parse(Config(change)));
        }

        [Test]
        public void AMissingConfigKeyIsNamed()
        {
            var ex = ConfigError(c => ((JObject)c["players"]).Remove("max"));
            Assert.That(ex.File, Is.EqualTo(ConfigLoader.FileName));
            Assert.That(ex.Errors, Has.Some.EqualTo("config.json players.max: missing key"));
        }

        [Test]
        public void AMissingKeyTheGameDoesNotUseYetIsStillAnError()
        {
            var ex = ConfigError(c => ((JObject)c["hosts"]).Remove("streakTrigger"));
            Assert.That(ex.Message, Does.Contain("hosts.streakTrigger: missing key"));
        }

        [TestCase("reveal")]
        [TestCase("questionShow")]
        [TestCase("lastCall")]
        [TestCase("results")]
        public void TimersMustBePositive(string timer)
        {
            var ex = ConfigError(c => c["timers"][timer] = 0);
            Assert.That(ex.Message, Does.Contain("config.json timers." + timer + ": must be more than 0"));
        }

        [Test]
        public void TheAnswerWindowMustBePositive()
        {
            var ex = ConfigError(c => c["timers"]["answer"]["multipleChoice"] = -1);
            Assert.That(ex.Message, Does.Contain("timers.answer.multipleChoice: must be more than 0"));
        }

        [Test]
        public void MultipliersMustMatchTheRoundCount()
        {
            var ex = ConfigError(c => c["rounds"]["multipliers"] = new JArray(1, 1));
            Assert.That(ex.Message, Does.Contain("rounds.multipliers"));
        }

        [Test]
        public void SlotOrdersMustMatchQuestionsPerRound()
        {
            var ex = ConfigError(c =>
            {
                c["rounds"]["difficultyOrder"] = new JArray("easy", "medium");
                c["rounds"]["formatOrder"] = new JArray("multipleChoice", "riddle", "multipleChoice", "closestValue");
            });
            Assert.That(ex.Message, Does.Contain("rounds.difficultyOrder: must have one entry per question slot"));
            Assert.That(ex.Message, Does.Contain("rounds.formatOrder: \"riddle\""));
        }

        [Test]
        public void ABadDifficultyInTheOrderIsNamed()
        {
            var ex = ConfigError(c => c["rounds"]["difficultyOrder"][1] = "extreme");
            Assert.That(ex.Message, Does.Contain("rounds.difficultyOrder: \"extreme\""));
        }

        [Test]
        public void ScoringValuesAreChecked()
        {
            var ex = ConfigError(c =>
            {
                c["scoring"]["roundTo"] = 0;
                c["scoring"]["speedPenalty"] = 1.5;
            });
            Assert.That(ex.Errors.Count, Is.EqualTo(2), ex.Message);
            Assert.That(ex.Message, Does.Contain("scoring.roundTo"));
            Assert.That(ex.Message, Does.Contain("scoring.speedPenalty"));
        }

        [Test]
        public void TimerMultiplierOptionsMustBePositive()
        {
            var ex = ConfigError(c => c["players"]["timerMultiplierOptions"] = new JArray(1, 0));
            Assert.That(ex.Message, Does.Contain("players.timerMultiplierOptions"));
        }

        [Test]
        public void StagingMustFitInsideItsPhase()
        {
            var ex = ConfigError(c => c["staging"]["reveal"]["reactionSeconds"] = 30);
            Assert.That(ex.Message, Does.Contain("config.json staging.reveal: cards up plus reactions must fit inside Reveal"));
        }

        [Test]
        public void TheRaceMustLetLastPlaceWin()
        {
            var ex = ConfigError(c => c["race"]["finishSteps"] = 40);
            Assert.That(ex.Message, Does.Contain("finishSteps must be at most maxQuestions"));
        }

        [Test]
        public void AWrongTypeIsAContentError()
        {
            var ex = ConfigError(c => c["players"]["min"] = "two");
            Assert.That(ex.File, Is.EqualTo(ConfigLoader.FileName));
        }

        [Test]
        public void BrokenJsonIsAContentError()
        {
            Assert.Throws<ContentException>(() => ConfigLoader.Parse("{ not json"));
            Assert.Throws<ContentException>(() => ConfigLoader.Parse(""));
            Assert.Throws<ContentException>(() => ConfigLoader.Parse("[]"));
        }

        [Test]
        public void TheMixedTopicIdMustNameTheMixedTopic()
        {
            var ex = Assert.Throws<ContentException>(() => new ContentLoader(name => name == ConfigLoader.FileName
                ? Config(c => c["topics"]["mixedTopicId"] = "geography")
                : TestContent.Read(name)).Load());
            Assert.That(ex.Message, Does.Contain("config.json topics.mixedTopicId"));
        }

        // ---------------------------------------------------------------- topics.json

        [Test]
        public void TopicsNeedExactlyOneMixedTopic()
        {
            var ex = Assert.Throws<ContentException>(() => TopicLoader.Parse(
                "[{\"id\":\"geo\",\"title\":{\"ru\":\"Гео\",\"en\":\"Geo\"}}]"));
            Assert.That(ex.Message, Does.Contain("topics.json -: exactly one topic must be mixed"));
        }

        [Test]
        public void ATopicTitleNeedsBothLanguages()
        {
            var ex = Assert.Throws<ContentException>(() => TopicLoader.Parse(
                "[{\"id\":\"geo\",\"title\":{\"ru\":\"Гео\"}},{\"id\":\"mixed\",\"title\":{\"ru\":\"Всё\",\"en\":\"All\"},\"mixed\":true}]"));
            Assert.That(ex.Message, Does.StartWith("topics.json geo: "));
            Assert.That(ex.Message, Does.Contain("en"));
        }

        // ---------------------------------------------------------------- questions.json

        private static readonly IReadOnlyList<TopicDef> Topics = TopicLoader.Parse(
            "[{\"id\":\"geography\",\"title\":{\"ru\":\"География\",\"en\":\"Geography\"}}," +
            "{\"id\":\"mixed\",\"title\":{\"ru\":\"Обо всём\",\"en\":\"A bit of everything\"},\"mixed\":true}]");

        private const string GoodQuestion =
            "{\"id\":\"geo_001\",\"topic\":\"geography\",\"format\":\"multipleChoice\",\"difficulty\":\"easy\"," +
            "\"text\":{\"ru\":\"Какая река самая длинная в Европе?\",\"en\":\"Which is the longest river in Europe?\"}," +
            "\"options\":[{\"ru\":\"Волга\",\"en\":\"Volga\"},{\"ru\":\"Дунай\",\"en\":\"Danube\"},{\"ru\":\"Рейн\",\"en\":\"Rhine\"},{\"ru\":\"Днепр\",\"en\":\"Dnieper\"}]," +
            "\"correct\":0,\"source\":\"https://www.wikidata.org/wiki/Q626\"}";

        private const string GoodClosest =
            "{\"id\":\"geo_005\",\"topic\":\"geography\",\"format\":\"closestValue\",\"difficulty\":\"hard\"," +
            "\"text\":{\"ru\":\"Какова высота Эвереста в метрах?\",\"en\":\"How tall is Mount Everest in metres?\"}," +
            "\"answer\":8849,\"range\":[6000,11000],\"step\":10,\"tolerance\":1500,\"unit\":{\"ru\":\"м\",\"en\":\"m\"}," +
            "\"source\":\"https://www.wikidata.org/wiki/Q513\"}";

        private static string Question(string json, Action<JObject> change)
        {
            var q = JObject.Parse(json);
            change(q);
            return q.ToString();
        }

        private static ContentException QuestionError(params string[] items)
        {
            return Assert.Throws<ContentException>(() => QuestionLoader.Parse("[" + string.Join(",", items) + "]", Topics));
        }

        [Test]
        public void TheSchemaExamplesLoad()
        {
            var list = QuestionLoader.Parse("[" + GoodQuestion + "," + GoodClosest + "]", Topics);
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[1].Range, Is.EqualTo(new double[] { 6000, 11000 }));
            Assert.That(list[1].Tolerance, Is.EqualTo(1500));
        }

        [Test]
        public void ADuplicateQuestionIdIsNamed()
        {
            var ex = QuestionError(GoodQuestion, GoodQuestion);
            Assert.That(ex.Errors, Has.Some.EqualTo("questions.json geo_001: id is used twice"));
        }

        [TestCase("space")]
        [TestCase("mixed")]
        public void AnUnknownOrMixedTopicIsNamed(string topic)
        {
            var ex = QuestionError(Question(GoodQuestion, q => q["topic"] = topic));
            Assert.That(ex.Message, Does.StartWith("questions.json geo_001: topic"));
        }

        [Test]
        public void ABadDifficultyIsNamed()
        {
            var ex = QuestionError(Question(GoodQuestion, q => q["difficulty"] = "brutal"));
            Assert.That(ex.Message, Is.EqualTo("questions.json geo_001: difficulty must be easy, medium or hard"));
        }

        [Test]
        public void TextOver120CharactersIsNamed()
        {
            var ex = QuestionError(Question(GoodQuestion, q => q["text"]["en"] = new string('a', ContentLimits.QuestionTextMax + 1)));
            Assert.That(ex.Message, Does.StartWith("questions.json geo_001: text must be at most 120"));
        }

        [Test]
        public void ExactlyFourOptionsOfAtMost40Characters()
        {
            var three = QuestionError(Question(GoodQuestion, q => ((JArray)q["options"]).RemoveAt(3)));
            Assert.That(three.Message, Does.Contain("geo_001: options must be a list of 4"));
            var longOne = QuestionError(Question(GoodQuestion, q => q["options"][2]["ru"] = new string('я', 41)));
            Assert.That(longOne.Message, Does.Contain("geo_001: options[2] must be at most 40"));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void CorrectMustBeZeroToThree(int correct)
        {
            var ex = QuestionError(Question(GoodQuestion, q => q["correct"] = correct));
            Assert.That(ex.Message, Does.Contain("geo_001: correct must be 0 to 3"));
        }

        [Test]
        public void AMissingCorrectIsNamed()
        {
            var ex = QuestionError(Question(GoodQuestion, q => q.Remove("correct")));
            Assert.That(ex.Message, Does.Contain("geo_001: correct"));
        }

        [Test]
        public void AMissingSourceIsNamed()
        {
            var ex = QuestionError(Question(GoodQuestion, q => q.Remove("source")));
            Assert.That(ex.Message, Does.StartWith("questions.json geo_001: "));
            Assert.That(ex.Message, Does.Contain("source"));
            var empty = QuestionError(Question(GoodQuestion, q => q["source"] = ""));
            Assert.That(empty.Message, Is.EqualTo("questions.json geo_001: source is missing"));
        }

        [Test]
        public void ABadFormatIsNamed()
        {
            var ex = QuestionError(Question(GoodQuestion, q => q["format"] = "riddle"));
            Assert.That(ex.Message, Does.Contain("geo_001: format must be multipleChoice or closestValue"));
        }

        [Test]
        public void ClosestValueFieldsAreChecked()
        {
            var outside = QuestionError(Question(GoodClosest, q => q["answer"] = 6000));
            Assert.That(outside.Message, Does.Contain("geo_005: answer must lie strictly inside range"));
            var step = QuestionError(Question(GoodClosest, q => q["step"] = 0));
            Assert.That(step.Message, Does.Contain("geo_005: step must be more than 0"));
            var tolerance = QuestionError(Question(GoodClosest, q => q.Remove("tolerance")));
            Assert.That(tolerance.Message, Does.Contain("geo_005: tolerance must be more than 0"));
            var unit = QuestionError(Question(GoodClosest, q => q.Remove("unit")));
            Assert.That(unit.Message, Does.Contain("geo_005: unit must have ru and en"));
            Assert.DoesNotThrow(() => QuestionLoader.Parse("[" + Question(GoodClosest, q => q["unit"] = JObject.Parse("{\"ru\":\"\",\"en\":\"\"}")) + "]", Topics),
                "an empty unit is allowed");
        }

        [Test]
        public void AnItemWithoutAnIdIsNamedByItsIndex()
        {
            var ex = QuestionError(GoodQuestion, Question(GoodClosest, q => q.Remove("id")));
            Assert.That(ex.Message, Does.StartWith("questions.json #1: "));
        }

        [Test]
        public void EveryErrorOfTheFileIsListed()
        {
            var ex = QuestionError(Question(GoodQuestion, q =>
            {
                q["difficulty"] = "x";
                q["correct"] = 9;
            }));
            Assert.That(ex.Errors.Count, Is.EqualTo(2));
        }

        [Test]
        public void TheBankMustHoldOneGame()
        {
            var ex = Assert.Throws<ContentException>(() => new ContentLoader(name => name == QuestionLoader.FileName
                ? "[" + GoodQuestion + "]"
                : TestContent.Read(name)).Load());
            Assert.That(ex.Message, Does.Contain("multipleChoice questions of difficulty medium"));
        }

        // ---------------------------------------------------------------- characters.json

        private static string Character(int id, string name, string color, Action<JObject> change = null)
        {
            var c = new JObject
            {
                ["id"] = id,
                ["name"] = new JObject { ["ru"] = name + "ру", ["en"] = name },
                ["personality"] = new JObject { ["ru"] = "Строка", ["en"] = "A line" },
                ["look"] = "look",
                ["color"] = color,
                ["entrance"] = "entrance",
                ["idlePose"] = "idle",
                ["happyReaction"] = "happy",
                ["sadReaction"] = "sad",
                ["cardHold"] = "card",
                ["victory"] = "victory"
            };
            change?.Invoke(c);
            return c.ToString();
        }

        private static GameConfig TwoPlayerConfig()
        {
            var config = ConfigLoader.Parse(TestContent.Read(ConfigLoader.FileName));
            config.Players.Max = 2;
            return config;
        }

        private static ContentException CharacterError(params string[] items)
        {
            return Assert.Throws<ContentException>(() => CharacterLoader.Parse("[" + string.Join(",", items) + "]", TwoPlayerConfig()));
        }

        [Test]
        public void GoodCharactersLoad()
        {
            var list = CharacterLoader.Parse("[" + Character(0, "Robot", "#5B9BD5") + "," + Character(1, "Diva", "#E57A9A") + "]",
                TwoPlayerConfig());
            Assert.That(list.Count, Is.EqualTo(2));
        }

        [Test]
        public void CharacterIdsMustRunInOrder()
        {
            var ex = CharacterError(Character(0, "Robot", "#5B9BD5"), Character(2, "Diva", "#E57A9A"));
            Assert.That(ex.Message, Is.EqualTo("characters.json 2: id must be 0, 1, 2, ... in file order"));
        }

        [Test]
        public void ThereMustBeAtLeastPlayersMaxCharacters()
        {
            var ex = CharacterError(Character(0, "Robot", "#5B9BD5"));
            Assert.That(ex.Message, Does.Contain("needs at least players.max characters"));
        }

        [Test]
        public void CharacterColorsAreChecked()
        {
            var bad = CharacterError(Character(0, "Robot", "blue"), Character(1, "Diva", "#E57A9A"));
            Assert.That(bad.Message, Is.EqualTo("characters.json 0: color must be #RRGGBB"));
            var twice = CharacterError(Character(0, "Robot", "#5B9BD5"), Character(1, "Diva", "#5b9bd5"));
            Assert.That(twice.Message, Is.EqualTo("characters.json 1: color is used twice"));
            var dark = CharacterError(Character(0, "Robot", "#202020"), Character(1, "Diva", "#E57A9A"));
            Assert.That(dark.Message, Does.StartWith("characters.json 0: color needs a contrast of 4.5"));
        }

        [Test]
        public void CharacterNamesAreChecked()
        {
            var twice = CharacterError(Character(0, "Robot", "#5B9BD5"), Character(1, "robot", "#E57A9A"));
            Assert.That(twice.Message, Does.Contain("characters.json 1: name.ru is used twice"));
            var longName = CharacterError(Character(0, new string('a', ContentLimits.CharacterNameMax + 1), "#5B9BD5"),
                Character(1, "Diva", "#E57A9A"));
            Assert.That(longName.Message, Does.Contain("characters.json 0: name must be at most 16"));
        }

        [Test]
        public void TheAnimatorFieldsMustBeThere()
        {
            var ex = CharacterError(Character(0, "Robot", "#5B9BD5", c => c.Remove("victory")), Character(1, "Diva", "#E57A9A"));
            Assert.That(ex.Message, Is.EqualTo("characters.json 0: victory is missing"));
        }

        [Test]
        public void APersonalityOver60CharactersIsNamed()
        {
            var ex = CharacterError(Character(0, "Robot", "#5B9BD5", c => c["personality"]["en"] = new string('a', 61)),
                Character(1, "Diva", "#E57A9A"));
            Assert.That(ex.Message, Does.Contain("characters.json 0: personality must be at most 60"));
        }

        // ---------------------------------------------------------------- ui_strings.json

        [Test]
        public void UiStringsNeedBothLanguages()
        {
            var ex = Assert.Throws<ContentException>(() => UiStrings.Parse("{\"tv.a\": {\"ru\": \"\", \"en\": \"x\"}}"));
            Assert.That(ex.Message, Is.EqualTo("ui_strings.json tv.a: must have non empty ru and en"));
            Assert.Throws<ContentException>(() => UiStrings.Parse("{\"tv.a\": {\"ru\": \"x\"}}"));
            Assert.Throws<ContentException>(() => UiStrings.Parse("{ not json"));
        }

        [TestCase("lobby.title")]
        [TestCase("tv.Lobby")]
        [TestCase("tv")]
        [TestCase("screen.lobby_title")]
        public void UiStringKeysMustBeDottedLowerCamelCase(string key)
        {
            var ex = Assert.Throws<ContentException>(() => UiStrings.Parse("{\"" + key + "\": {\"ru\": \"x\", \"en\": \"x\"}}"));
            Assert.That(ex.Message, Does.StartWith("ui_strings.json " + key + ": key must start with tv. or screen."));
        }

        [Test]
        public void UiStringPlaceholdersMustMatch()
        {
            var ex = Assert.Throws<ContentException>(() =>
                UiStrings.Parse("{\"tv.results.winner\": {\"ru\": \"Победитель: {name}\", \"en\": \"Winner: {player}\"}}"));
            Assert.That(ex.Message, Is.EqualTo("ui_strings.json tv.results.winner: placeholders differ between ru and en"));
        }

        [Test]
        public void UiStringsFillPlaceholders()
        {
            var ui = UiStrings.Parse("{\"tv.results.winner\": {\"ru\": \"Победитель: {name}\", \"en\": \"Winner: {name}\"}}");
            var values = new Dictionary<string, string> { { "name", "Mama" } };
            Assert.That(ui.Format("tv.results.winner", Languages.English, values), Is.EqualTo("Winner: Mama"));
            Assert.That(ui.Get("tv.results.winner", Languages.Russian), Is.EqualTo("Победитель: {name}"));
            Assert.That(ui.Get("tv.missing", Languages.Russian), Is.EqualTo("tv.missing"));
        }

        // ---------------------------------------------------------------- clock

        [Test]
        public void PausableClockStandsStillWhilePaused()
        {
            long source = 1000;
            var clock = new PausableClock(() => source);
            source += 500;
            Assert.That(clock.NowMs, Is.EqualTo(500));
            clock.Pause();
            source += 10000;
            Assert.That(clock.NowMs, Is.EqualTo(500));
            clock.Resume();
            source += 250;
            Assert.That(clock.NowMs, Is.EqualTo(750));
        }
    }
}
