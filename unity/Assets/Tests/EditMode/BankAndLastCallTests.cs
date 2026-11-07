using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// One question bank (D130, replacing the bank test of decisions/0020, D116 to D118) and last call off (D119): the
    /// game loads and plays only the refined questions, offers no bank choice, and a question whose full bank question
    /// a family saw before still counts as seen.
    /// </summary>
    public sealed class BankAndLastCallTests
    {
        [Test]
        public void TheShippedContentIsTheRefinedBankOnly()
        {
            var content = TestContent.Shipped();
            var bank = content.Bank;
            Assert.That(bank.Questions, Is.SameAs(content.Questions));
            Assert.That(bank.Questions.Count, Is.EqualTo(120));
            Assert.That(bank.Questions.All(q => q.Id.StartsWith("cur_")), Is.True, "no full bank question is loaded");
            Assert.That(bank.Topics.Count(t => !t.Mixed), Is.EqualTo(12));
            Assert.That(bank.Topics.Any(t => t.Mixed), Is.True, "the mixed door stays");
        }

        [Test]
        public void EveryTopicHoldsTwoRounds()
        {
            var bank = TestContent.Shipped().Bank;
            foreach (var topic in bank.Topics.Where(t => !t.Mixed))
            {
                var qs = bank.Questions.Where(q => q.Topic == topic.Id).ToList();
                Assert.That(qs.Count(q => q.Difficulty == "easy"), Is.EqualTo(3), topic.Id);
                Assert.That(qs.Count(q => q.Difficulty == "medium"), Is.EqualTo(5), topic.Id);
                Assert.That(qs.Count(q => q.Format == QuestionDef.ClosestValue), Is.EqualTo(2), topic.Id);
            }
        }

        [Test]
        public void AGamePlaysTheRefinedQuestions()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(3);
            f.ReadyAll();
            f.RunUntilPhase(GamePhase.TopicVote);
            var topics = new HashSet<string>(f.Content.Bank.Topics.Select(t => t.Id));
            Assert.That(f.State.Round.Doors.All(d => topics.Contains(d.Id)), Is.True, "only topics of the bank behind the doors");

            for (int i = 0; i < 4; i++)
            {
                f.RunUntilPhase(GamePhase.Answer);
                var q = f.State.Question.Def;
                Assert.That(q.Id, Does.StartWith("cur_"));
                Assert.That(f.State.SeenQuestionIds, Does.Contain(q.Id));
                if (q.BasedOn != null) Assert.That(f.State.SeenQuestionIds, Does.Not.Contain(q.BasedOn), "only the question shown is marked");
                f.RunUntilPhase(GamePhase.Reveal);
            }
        }

        [Test]
        public void AQuestionWhoseFullBankQuestionWasSeenCountsAsSeen()
        {
            var q = TestContent.Shipped().Questions.First(x => x.BasedOn != null);
            Assert.That(QuestionPicker.IsSeen(q, new HashSet<string> { q.BasedOn }), Is.True, "seen.json from before D130 holds full bank ids");
            Assert.That(QuestionPicker.IsSeen(q, new HashSet<string> { q.Id }), Is.True);
            Assert.That(QuestionPicker.IsSeen(q, new HashSet<string>()), Is.False);
        }

        [Test]
        public void WithTheShippedConfigAnotherAnswerNeverShortensTheTimer()
        {
            var f = new LobbyFixture(content: TestContent.Shipped());
            Assert.That(f.Config.Timers.LastCallEnabled, Is.False);
            var seats = f.ToFirstAnswer(2);
            long before = seats[1].Turn.DeadlineMs;
            var events = f.Choice(seats[0].Conn, f.Right, 1000);
            Assert.That(events.OfType<LastCallStarted>(), Is.Empty);
            Assert.That(seats[1].Turn.DeadlineMs, Is.EqualTo(before), "the other player keeps the whole window");
            Assert.That(seats[1].Turn.LastCall, Is.False);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer));
            f.Choice(seats[1].Conn, f.Wrong, 2000);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal), "the phase still ends the moment everyone answered");
        }
    }
}
