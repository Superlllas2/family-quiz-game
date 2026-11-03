using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>Shared ranks (D22), the option shuffle (docs/04_formats.md) and the Day 3 question rules are in QuestionPickerTests.</summary>
    public sealed class RankingAndQuestionTests
    {
        [Test]
        public void EqualScoresShareTheBetterRank()
        {
            var state = new GameState();
            var scores = new[] { 900, 900, 400 };
            for (int i = 0; i < scores.Length; i++)
                state.MutablePlayers.Add(new PlayerState("p" + i, "P" + i, "ru", i, "t" + i, 1) { Score = scores[i] });
            Assert.That(state.Players.Select(p => Ranking.SharedRank(state, p)), Is.EqualTo(new[] { 1, 1, 3 }));
        }

        [Test]
        public void EveryoneHasRankOneAtTheStart()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            Assert.That(seats.Select(p => p.Turn.RankBefore), Is.EqualTo(new[] { 1, 1, 1 }));
        }

        [Test]
        public void RankBeforeIsTheRankBeforeTheQuestion()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Right, 0);
            f.Choice("c3", f.Wrong, 0);
            f.RunUntilPhase(GamePhase.Answer);
            Assert.That(seats.Select(p => p.Turn.RankBefore), Is.EqualTo(new[] { 1, 1, 3 }));
        }

        [Test]
        public void TheShuffleIsAPermutationAndTheSameForTheSameSeed()
        {
            var a = OptionShuffle.DisplayOrder(4, new SeededRandom(5));
            var b = OptionShuffle.DisplayOrder(4, new SeededRandom(5));
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.OrderBy(x => x), Is.EqualTo(new[] { 0, 1, 2, 3 }));
            var orders = new HashSet<string>();
            for (int seed = 0; seed < 50; seed++) orders.Add(string.Join(",", OptionShuffle.DisplayOrder(4, new SeededRandom(seed))));
            Assert.That(orders.Count, Is.GreaterThan(10), "the order really changes");
        }

        [Test]
        public void TheSameSeedReplaysTheSameQuestionsAndOrders()
        {
            var one = PlayOneGame(31);
            var two = PlayOneGame(31);
            var other = PlayOneGame(32);
            Assert.That(two, Is.EqualTo(one));
            Assert.That(other, Is.Not.EqualTo(one));
        }

        private static List<string> PlayOneGame(int seed)
        {
            var f = new LobbyFixture(seed);
            f.SeatPlayers(2);
            f.ReadyAll();
            var seen = new List<string>();
            for (int q = 0; q < f.Config.Rounds.QuestionsPerRound; q++)
            {
                f.RunUntilPhase(GamePhase.Answer, 3600000, 50);
                var question = f.State.Question;
                seen.Add(question.Def.Id + ":" + string.Join("", question.DisplayOrder));
                f.RunUntilPhase(GamePhase.Reveal, 3600000, 50);
            }

            return seen;
        }

        [Test]
        public void TheShuffledOrderKeepsTheRightAnswer()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var f = new LobbyFixture(seed);
                f.ToFirstAnswer(2);
                var q = f.State.Question;
                Assert.That(q.DisplayOrder.Length, Is.EqualTo(q.Def.Options.Count));
                Assert.That(q.DisplayOrder[q.CorrectDisplayIndex], Is.EqualTo(q.Def.Correct));
                Assert.That(q.OptionText(q.CorrectDisplayIndex, Languages.English), Is.EqualTo(q.Def.Options[q.Def.Correct].En));

                // A tap on the right display index scores; the file index of the right option does not, unless they match.
                var locked = f.Choice("c1", q.CorrectDisplayIndex, 0).OfType<FamilyQuiz.Core.Events.AnswerLocked>().Single();
                Assert.That(locked.Correct, Is.True);
            }
        }
    }
}
