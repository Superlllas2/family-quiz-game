using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// Questions per slot (docs/02_show_flow.md, "Rules inside a round", 2): the slot order, the four fallback groups,
    /// never twice in a game, and the warning when the chosen topic had nothing unseen (docs/18_edge_cases.md).
    /// </summary>
    public sealed class QuestionPickerTests
    {
        private const string Mc = QuestionDef.MultipleChoice;

        private static QuestionDef Q(string id, string topic, string difficulty = "medium", string format = Mc)
        {
            return new QuestionDef { Id = id, Topic = topic, Format = format, Difficulty = difficulty };
        }

        private static readonly List<QuestionDef> Bank = new List<QuestionDef>
        {
            Q("geo_1", "geo"), Q("geo_2", "geo"), Q("his_1", "his"), Q("his_2", "his"),
            Q("geo_e", "geo", "easy"), Q("geo_cv", "geo", "hard", QuestionDef.ClosestValue)
        };

        private static QuestionDef Pick(string topic, HashSet<string> seen, HashSet<string> used, int seed, out int group)
        {
            return QuestionPicker.Pick(Bank, Mc, "medium", topic, seen, used, new SeededRandom(seed), out group);
        }

        [Test]
        public void Group1UnseenFromTheChosenTopic()
        {
            for (int seed = 0; seed < 10; seed++)
            {
                var q = Pick("geo", new HashSet<string> { "geo_1" }, new HashSet<string>(), seed, out int group);
                Assert.That(q.Id, Is.EqualTo("geo_2"));
                Assert.That(group, Is.EqualTo(1));
            }
        }

        [Test]
        public void Group2UnseenFromAnyTopic()
        {
            var q = Pick("geo", new HashSet<string> { "geo_1", "geo_2", "his_1" }, new HashSet<string>(), 1, out int group);
            Assert.That(q.Id, Is.EqualTo("his_2"));
            Assert.That(group, Is.EqualTo(2));
        }

        [Test]
        public void Group3SeenFromTheChosenTopic()
        {
            var seen = new HashSet<string> { "geo_1", "geo_2", "his_1", "his_2" };
            var q = Pick("geo", seen, new HashSet<string> { "geo_1" }, 1, out int group);
            Assert.That(q.Id, Is.EqualTo("geo_2"));
            Assert.That(group, Is.EqualTo(3));
        }

        [Test]
        public void Group4SeenFromAnyTopic()
        {
            var seen = new HashSet<string> { "geo_1", "geo_2", "his_1", "his_2" };
            var q = Pick("geo", seen, new HashSet<string> { "geo_1", "geo_2", "his_1" }, 1, out int group);
            Assert.That(q.Id, Is.EqualTo("his_2"));
            Assert.That(group, Is.EqualTo(4));
        }

        [Test]
        public void TheMixedDoorDrawsUnseenFromAnyTopicAsGroup1()
        {
            var picked = new HashSet<string>();
            for (int seed = 0; seed < 40; seed++)
            {
                var q = Pick(null, new HashSet<string> { "geo_1" }, new HashSet<string>(), seed, out int group);
                Assert.That(group, Is.EqualTo(1));
                picked.Add(q.Id);
            }

            Assert.That(picked, Is.EquivalentTo(new[] { "geo_2", "his_1", "his_2" }));
        }

        [Test]
        public void TheSlotFormatAndDifficultyAlwaysHold()
        {
            var q = QuestionPicker.Pick(Bank, QuestionDef.ClosestValue, "hard", "his", new HashSet<string>(), new HashSet<string>(),
                new SeededRandom(1), out int group);
            Assert.That(q.Id, Is.EqualTo("geo_cv"), "no closest value question in his: any topic");
            Assert.That(group, Is.EqualTo(2));
        }

        [Test]
        public void ANeverUsedQuestionIsNeverRepeatedInAGame()
        {
            var used = new HashSet<string> { "geo_1", "geo_2", "his_1", "his_2" };
            Assert.Throws<System.InvalidOperationException>(() => Pick("geo", new HashSet<string>(), used, 1, out _));
        }

        // ---------------------------------------------------------------- in a real game

        [Test]
        public void EveryRoundPlaysTheSlotsInOrderWithItsMultiplier()
        {
            var f = new LobbyFixture();
            var rounds = f.Config.Rounds;
            f.ToFirstVote(2);
            for (int r = 0; r < rounds.Count; r++)
            {
                Assert.That(f.State.Phase, Is.EqualTo(GamePhase.TopicVote));
                Assert.That(f.State.Round.Index, Is.EqualTo(r));
                Assert.That(f.State.Round.Multiplier, Is.EqualTo(rounds.Multipliers[r]));
                for (int slot = 0; slot < rounds.QuestionsPerRound; slot++)
                {
                    f.SkipTo(GamePhase.QuestionShow);
                    var q = f.State.Question;
                    Assert.That(q.Slot, Is.EqualTo(slot));
                    Assert.That(q.RoundIndex, Is.EqualTo(r));
                    Assert.That(q.Number, Is.EqualTo(r * rounds.QuestionsPerRound + slot + 1));
                    Assert.That(q.Multiplier, Is.EqualTo(rounds.Multipliers[r]));
                    Assert.That(q.Def.Format, Is.EqualTo(rounds.FormatOrder[slot]), "round " + r + " slot " + slot);
                    Assert.That(q.Def.Difficulty, Is.EqualTo(rounds.DifficultyOrder[slot]), "round " + r + " slot " + slot);
                    var topic = f.State.Round.Topic;
                    if (!topic.Mixed && q.PickGroup == 1) Assert.That(q.Def.Topic, Is.EqualTo(topic.Id));
                    f.SkipTo(GamePhase.Answer);
                }

                f.SkipTo(GamePhase.RoundEnd);
                if (r + 1 < rounds.Count) f.SkipTo(GamePhase.TopicVote);
            }
        }

        [Test]
        public void TheRoundMultiplierAppliesToEveryPointOfTheRound()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstVote(2);
            var rounds = f.Config.Rounds;
            for (int r = 0; r < rounds.Count; r++)
            {
                f.SkipTo(GamePhase.Answer);
                Assert.That(f.State.Question.IsClosestValue, Is.False);
                int before = seats[0].Score;
                f.Choice("c1", f.Right, 0);
                f.Choice("c2", f.Wrong, 0);
                f.SkipTo(GamePhase.Reveal);
                Assert.That(seats[0].Score - before, Is.EqualTo((int)(f.Config.Scoring.MaxPoints * rounds.Multipliers[r])), "round " + (r + 1));
                if (r + 1 < rounds.Count) f.SkipTo(GamePhase.TopicVote);
            }
        }

        [Test]
        public void AnEmptyChosenTopicRaisesTheFallbackWarning()
        {
            // Every question seen before the game: group 1 is empty for every slot.
            var content = TestContent.Content();
            var f = new LobbyFixture(7, seenQuestions: content.Questions.Select(q => q.Id));
            f.ToFirstVote(2);
            var events = f.SkipTo(GamePhase.Answer);
            var warning = events.OfType<QuestionFallback>().Single();
            Assert.That(warning.Round, Is.EqualTo(1));
            Assert.That(warning.Slot, Is.EqualTo(1));
            // Group 3 (seen from the chosen topic; for the mixed door that is any topic).
            Assert.That(warning.Group, Is.EqualTo(QuestionPicker.GroupSeenTopic));
            Assert.That(warning.QuestionId, Is.EqualTo(f.State.Question.Def.Id));
        }

        [Test]
        public void NoFallbackWarningWithAFreshBankAndTopicDoors()
        {
            // Everyone votes for door 1 (never the mixed door): four different topics, each with unseen questions.
            var f = new LobbyFixture(7);
            f.ToFirstVote(2);
            var events = new List<IGameEvent>();
            for (int r = 0; r < f.Config.Rounds.Count; r++)
            {
                Assert.That(f.State.Phase, Is.EqualTo(GamePhase.TopicVote));
                events.AddRange(f.Vote("c1", 0));
                events.AddRange(f.Vote("c2", 0));
                Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow), "everyone voted: the vote ends at once");
                events.AddRange(f.SkipTo(r + 1 < f.Config.Rounds.Count ? GamePhase.TopicVote : GamePhase.FinalIntro));
            }

            Assert.That(events.OfType<QuestionFallback>(), Is.Empty);
            Assert.That(events.OfType<TopicChosen>().Select(t => t.TopicId).Distinct().Count(), Is.EqualTo(f.Config.Rounds.Count));
        }
    }
}
