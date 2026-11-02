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
    /// <summary>Shared ranks (D22), the option shuffle (docs/04_formats.md) and question picking (D75).</summary>
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
            f.Lobby("c1", LobbyActions.Start);
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

        [Test]
        public void SlotsFollowTheDifficultyOrderWithMediumForNonMultipleChoiceSlots()
        {
            var f = new LobbyFixture();
            var rounds = f.Config.Rounds;
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            for (int slot = 0; slot < rounds.QuestionsPerRound; slot++)
            {
                f.RunUntilPhase(GamePhase.QuestionShow, 3600000, 50);
                var q = f.State.Question;
                Assert.That(q.Slot, Is.EqualTo(slot));
                Assert.That(q.Number, Is.EqualTo(slot + 1));
                Assert.That(q.RoundIndex, Is.EqualTo(0));
                Assert.That(q.Multiplier, Is.EqualTo(rounds.Multipliers[0]));
                Assert.That(q.Def.Format, Is.EqualTo(QuestionDef.MultipleChoice));
                string expected = rounds.FormatOrder[slot] == QuestionDef.MultipleChoice ? rounds.DifficultyOrder[slot] : Difficulties.Medium;
                Assert.That(q.Def.Difficulty, Is.EqualTo(expected), "slot " + slot);
                f.RunUntilPhase(GamePhase.Answer, 3600000, 50);
            }
        }

        [Test]
        public void NoQuestionTwiceInAGameAndUnseenFirstAcrossGames()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            var first = QuestionsOf(f);
            Assert.That(first.Distinct().Count(), Is.EqualTo(first.Count));
            Assert.That(f.State.SeenQuestionIds, Is.EquivalentTo(first));

            f.Lobby("c1", LobbyActions.PlayAgain);
            var second = QuestionsOf(f);
            Assert.That(second.Intersect(first), Is.Empty, "the bank still has unseen ones, so none repeats");
            Assert.That(f.State.SeenQuestionIds.Count, Is.EqualTo(first.Count + second.Count));
        }

        private static List<string> QuestionsOf(LobbyFixture f)
        {
            var ids = new List<string>();
            while (true)
            {
                f.RunUntilPhase(GamePhase.QuestionShow, 3600000, 50);
                ids.Add(f.State.Question.Def.Id);
                f.RunUntilPhase(GamePhase.Scores, 3600000, 50);
                if (ids.Count == f.State.QuestionCount) break;
            }

            f.RunToPlayAgain();
            return ids;
        }

        [Test]
        public void SeenQuestionsAreUsedWhenNoUnseenOneIsLeft()
        {
            var bank = TestContent.Content().Questions;
            var rounds = TestContent.Content().Config.Rounds;
            var seen = new HashSet<string>(bank.Where(q => q.Format == QuestionDef.MultipleChoice).Select(q => q.Id));
            var used = new HashSet<string>();
            var q1 = QuestionPicker.Pick(bank, rounds, 0, seen, used, new SeededRandom(1));
            Assert.That(q1.Difficulty, Is.EqualTo(rounds.DifficultyOrder[0]));
            Assert.That(seen.Contains(q1.Id), Is.True);

            seen.Remove(q1.Id);
            var unseenOne = bank.First(q => q.Format == QuestionDef.MultipleChoice && q.Difficulty == rounds.DifficultyOrder[0] && q.Id != q1.Id);
            seen.Remove(unseenOne.Id);
            used.Add(q1.Id);
            Assert.That(QuestionPicker.Pick(bank, rounds, 0, seen, used, new SeededRandom(9)).Id, Is.EqualTo(unseenOne.Id),
                "the one unseen and unused question wins");
        }
    }
}
