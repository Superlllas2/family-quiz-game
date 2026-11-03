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
    /// Core raises every host line trigger of docs/15_schemas.md from the right moment (HostLineRequested); Dialogue picks
    /// the lines. The Reveal triggers come in priority order with the cycle number.
    /// </summary>
    public sealed class HostTriggerTests
    {
        private static List<HostLineRequested> Triggers(IEnumerable<IGameEvent> events)
        {
            return events.OfType<HostLineRequested>().ToList();
        }

        private static List<string> TriggerNames(IEnumerable<IGameEvent> events)
        {
            return Triggers(events).Select(t => t.Trigger).ToList();
        }

        [Test]
        public void ANewPlayerIsGreetedButNotARejoin()
        {
            var f = new LobbyFixture();
            var joined = Triggers(f.Join("c1", "Mama"));
            Assert.That(joined.Single().Trigger, Is.EqualTo(HostTriggers.PlayerJoined));
            Assert.That(joined.Single().Name, Is.EqualTo("Mama"));
            Assert.That(joined.Single().Cycle, Is.EqualTo(0));

            var token = f.State.Players[0].Token;
            f.Send(new ConnectionClosed("c1"));
            f.Send(new ConnectionOpened("c9"));
            Assert.That(Triggers(f.Send(new PlayerRejoin("c9", token))), Is.Empty);
        }

        [Test]
        public void PlayersKeptAfterAGameAreNotGreetedAgain()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            var events = f.RunToLobby();
            Assert.That(TriggerNames(events), Has.No.Member(HostTriggers.PlayerJoined));
        }

        [Test]
        public void TheCountdownTheIntroAndTheRoundsRaiseTheirTriggers()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            var ready = f.ReadyAll();
            Assert.That(TriggerNames(ready), Is.EqualTo(new[] { HostTriggers.StartCountdown }));
            Assert.That(TriggerNames(f.Advance(f.CountdownMs)), Is.EqualTo(new[] { HostTriggers.GameStart }));
            Assert.That(TriggerNames(f.Advance(f.IntroMs)), Is.EqualTo(new[] { HostTriggers.RoundStart }));

            var game = f.RunToLobby();
            var names = TriggerNames(game);
            Assert.That(names.Count(n => n == HostTriggers.RoundStart), Is.EqualTo(f.Config.Rounds.Count - 1));
            Assert.That(names.Count(n => n == HostTriggers.RoundEnd), Is.EqualTo(f.Config.Rounds.Count));
            Assert.That(names.Count(n => n == HostTriggers.FinalStart), Is.EqualTo(1));
            Assert.That(names.Count(n => n == HostTriggers.RaceWinner), Is.EqualTo(1));
            Assert.That(names.Count(n => n == HostTriggers.NobodyCorrect), Is.EqualTo(f.Config.Rounds.Count * f.Config.Rounds.QuestionsPerRound),
                "nobody answered any question");
            var winner = Triggers(game).Single(t => t.Trigger == HostTriggers.RaceWinner);
            Assert.That(winner.Name, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void LastCallRaisesItsTrigger()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 1000);
            Assert.That(TriggerNames(f.Choice("c2", f.Right, 1000)), Is.EqualTo(new[] { HostTriggers.LastCall }));
        }

        [Test]
        public void TheSingleFastestCorrectPlayerUnderTheLimitFiresFastAnswer()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 1500);
            f.Choice("c2", f.Right, 1900);
            var events = f.Choice("c3", f.Wrong, 300);
            var fast = Triggers(events).Single(t => t.Trigger == HostTriggers.FastAnswer);
            Assert.That(fast.Name, Is.EqualTo("P1"), "the fastest correct, not the fastest overall");
            Assert.That(fast.Cycle, Is.EqualTo(1));
        }

        [Test]
        public void NoFastAnswerAtTheLimitOrOnATie()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            long limit = f.Config.Scoring.FastAnswerMs;
            f.Choice("c1", f.Right, limit);
            Assert.That(TriggerNames(f.Choice("c2", f.Wrong, 100)), Has.No.Member(HostTriggers.FastAnswer), "not below the limit");

            var g = new LobbyFixture();
            g.ToFirstAnswer(2);
            g.Choice("c1", g.Right, 800);
            Assert.That(TriggerNames(g.Choice("c2", g.Right, 800)), Has.No.Member(HostTriggers.FastAnswer), "a tie: no single fastest");
        }

        [Test]
        public void EveryoneCorrectAndTheOrderOfTheRevealTriggers()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            f.Choice("c1", f.Right, 900);
            var events = f.Choice("c2", f.Right, 3000);
            // Everyone shares rank 1 before question 1: nobody is in last place and nobody is a new leader.
            Assert.That(TriggerNames(events), Is.EqualTo(new[] { HostTriggers.EveryoneCorrect, HostTriggers.FastAnswer }));
            Assert.That(Triggers(events).All(t => t.Cycle == 1), Is.True);
        }

        [Test]
        public void NewLeaderAndLastPlaceCorrect()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(3);
            // Q1: P1 leads alone; P2 and P3 share last place (rank 2).
            f.Choice("c1", f.Right, 3000);
            f.Choice("c2", f.Wrong, 3000);
            f.Choice("c3", f.Wrong, 3000);
            f.RunUntilPhase(GamePhase.Answer, 60000);
            // Q2: P2 (last place before) answers fast, takes the lead.
            f.Choice("c1", f.Wrong, 3000);
            f.Choice("c2", f.Right, 2500);
            var events = f.Choice("c3", f.Wrong, 3000);
            var triggers = Triggers(events);
            Assert.That(triggers.Select(t => t.Trigger).Take(2), Is.EqualTo(new[] { HostTriggers.LastPlaceCorrect, HostTriggers.NewLeader }));
            Assert.That(triggers[0].Name, Is.EqualTo("P2"));
            Assert.That(triggers[1].Name, Is.EqualTo("P2"));
            Assert.That(triggers.All(t => t.Cycle == 2), Is.True);
        }

        [Test]
        public void Streak3FiresWhenTheStreakReachesTheTrigger()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            int trigger = f.Config.Hosts.StreakTrigger;
            List<HostLineRequested> last = null;
            for (int q = 1; q <= trigger + 1; q++)
            {
                f.Choice("c1", f.Right, 3000);
                last = Triggers(f.Choice("c2", f.Wrong, 3000));
                bool expected = q == trigger;
                Assert.That(last.Any(t => t.Trigger == HostTriggers.Streak3 && t.Name == "P1"), Is.EqualTo(expected), "question " + q);
                f.RunUntilPhase(GamePhase.Answer, 600000);
            }
        }

        [Test]
        public void ClosestExactFiresForAGuessEqualToTheAnswer()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            long waited = 0;
            while (!(f.State.Phase == GamePhase.Answer && f.State.Question.IsClosestValue))
            {
                f.Advance(50);
                waited += 50;
                Assert.That(waited, Is.LessThan(600000));
            }

            var answer = f.State.Question.Def.Answer.Value;
            f.Value("c1", answer, 3000);
            var events = f.Value("c2", f.State.Question.Def.Range[0], 3000);
            var exact = Triggers(events).Single(t => t.Trigger == HostTriggers.ClosestExact);
            Assert.That(exact.Name, Is.EqualTo("P1"));
        }

        [Test]
        public void EveryTriggerHasLinesForAtLeastOneHost()
        {
            var lines = TestContent.Content().HostLines;
            foreach (var trigger in HostTriggers.All)
                Assert.That(lines.Any(l => l.Trigger == trigger), Is.True, trigger);
        }
    }
}
