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
    /// The question bank test and last call off (decisions/0020, D116 to D119): two banks, the bank a game plays, a
    /// curated question and its full bank question seen as one, and no last call with the shipped config.
    /// </summary>
    public sealed class BankAndLastCallTests
    {
        [Test]
        public void TheShippedContentHasTheFullAndTheCuratedBank()
        {
            var content = TestContent.Shipped();
            Assert.That(content.Banks.Select(b => b.Id), Is.EqualTo(new[] { BankIds.Full, BankIds.Curated }));
            var curated = content.Bank(BankIds.Curated);
            Assert.That(curated.Questions.Count, Is.EqualTo(120));
            Assert.That(curated.Topics.Count(t => !t.Mixed), Is.EqualTo(12));
            Assert.That(curated.Topics.Any(t => t.Mixed), Is.True, "the mixed door stays");
            Assert.That(curated.Questions.All(q => q.Id.StartsWith("cur_")), Is.True);
            Assert.That(content.Bank(BankIds.Full).Questions.Count, Is.EqualTo(3000));
            Assert.That(content.Config.Banks.Default, Is.EqualTo(BankIds.Full));
            Assert.That(content.Config.Banks.ChooseOnStart, Is.True, "the test tool asks on start");
        }

        [Test]
        public void EveryCuratedTopicHoldsTwoRounds()
        {
            var curated = TestContent.Shipped().Bank(BankIds.Curated);
            foreach (var topic in curated.Topics.Where(t => !t.Mixed))
            {
                var qs = curated.Questions.Where(q => q.Topic == topic.Id).ToList();
                Assert.That(qs.Count(q => q.Difficulty == "easy"), Is.EqualTo(3), topic.Id);
                Assert.That(qs.Count(q => q.Difficulty == "medium"), Is.EqualTo(5), topic.Id);
                Assert.That(qs.Count(q => q.Format == QuestionDef.ClosestValue), Is.EqualTo(2), topic.Id);
            }
        }

        [Test]
        public void TheBankIsChosenOnTheTvAndAppliesFromTheNextGame()
        {
            var f = new LobbyFixture();
            Assert.That(f.State.PendingSettings.Bank, Is.EqualTo(BankIds.Full));
            var events = f.Send(new ChangeTvSettings(bank: BankIds.Curated));
            Assert.That(f.State.PendingSettings.Bank, Is.EqualTo(BankIds.Curated));
            var changed = events.OfType<SettingsChanged>().Single();
            Assert.That(changed.Setting, Is.EqualTo(SettingsChanged.SettingBank));
            Assert.That(changed.Value, Is.EqualTo(BankIds.Curated));
            Assert.That(f.Send(new ChangeTvSettings(bank: "nope")).OfType<SettingsChanged>(), Is.Empty, "unknown banks are ignored");

            f.SeatPlayers(3);
            var started = f.ReadyAll();
            started.AddRange(f.RunUntilPhase(GamePhase.TopicVote));
            Assert.That(f.State.Settings.Bank, Is.EqualTo(BankIds.Curated));
            var gameStarted = started.Concat(f.Advance(0)).OfType<GameStarted>().FirstOrDefault()
                              ?? throw new AssertionException("no GameStarted");
            Assert.That(gameStarted.Bank, Is.EqualTo(BankIds.Curated));
            var curatedTopics = new HashSet<string>(f.Content.Bank(BankIds.Curated).Topics.Select(t => t.Id));
            Assert.That(f.State.Round.Doors.All(d => curatedTopics.Contains(d.Id)), Is.True, "only curated topics behind the doors");

            f.RunUntilPhase(GamePhase.Answer);
            Assert.That(f.State.Question.Def.Id, Does.StartWith("cur_"));
        }

        [Test]
        public void ACuratedQuestionAndItsFullBankQuestionAreSeenAsOne()
        {
            var curated = TestContent.Shipped().Bank(BankIds.Curated).Questions.First(q => q.BasedOn != null);
            var full = TestContent.Shipped().Questions.First(q => q.Id == curated.BasedOn);
            Assert.That(QuestionPicker.IsSeen(curated, new HashSet<string> { full.Id }), Is.True, "its full bank question was played");
            Assert.That(QuestionPicker.IsSeen(curated, new HashSet<string>()), Is.False);
            Assert.That(QuestionPicker.IsSeen(full, new HashSet<string> { curated.Id, curated.BasedOn }), Is.True,
                "showing a curated question marks its basedOn seen too");
        }

        [Test]
        public void ShowingACuratedQuestionMarksItsFullBankQuestionSeen()
        {
            var f = new LobbyFixture();
            f.Send(new ChangeTvSettings(bank: BankIds.Curated));
            f.SeatPlayers(2);
            f.ReadyAll();
            for (int i = 0; i < 6; i++)
            {
                f.RunUntilPhase(GamePhase.Answer);
                var q = f.State.Question.Def;
                Assert.That(f.State.SeenQuestionIds, Does.Contain(q.Id));
                if (q.BasedOn != null) Assert.That(f.State.SeenQuestionIds, Does.Contain(q.BasedOn));
                f.RunUntilPhase(GamePhase.Reveal);
            }
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
