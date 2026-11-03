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
    /// The end of game poll (docs/20_playtest.md, docs/19_acceptance.md Day 4) and Back to lobby from the TV settings
    /// panel (docs/02_show_flow.md).
    /// </summary>
    public sealed class PollAndLobbyTests
    {
        /// <summary>A game of count players run to the Poll with nobody answering.</summary>
        private static (LobbyFixture F, PlayerState[] Seats) AtThePoll(int count = 3)
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(count);
            f.ReadyAll();
            f.RunUntilPhase(GamePhase.Poll, 3600000, 50);
            return (f, seats);
        }

        private static IReadOnlyList<IGameEvent> Poll(LobbyFixture f, string conn, Dictionary<string, object> answers, string inputId = null)
        {
            return f.Send(new SubmitPoll(conn, inputId ?? f.NextInputId(), answers));
        }

        [Test]
        public void ThePollFollowsResultsWithItsOwnTimer()
        {
            var (f, _) = AtThePoll();
            Assert.That(f.State.GameRunning, Is.False, "the game ended with Results");
            Assert.That(f.State.PhaseBudgetMs, Is.EqualTo(f.PollMs));
            Assert.That(f.State.PhaseEndsAtMs, Is.EqualTo(f.State.PhaseStartedAtMs + f.PollMs));
            Assert.That(f.Game.AnyoneCanAnswer, Is.True, "phones can answer the poll");
        }

        [Test]
        public void AnswersAreKeptAnonymouslyAndCleaned()
        {
            var (f, seats) = AtThePoll();
            var events = Poll(f, "c1", new Dictionary<string, object>
            {
                { "fun", "  The freeze on Dad  " },
                { "frustrating", "   " },
                { "waiting", 1L },
                { "time", 7L },
                { "tvMissed", "often" },
                { "unfair", 2.0 },
                { "nope", "x" }
            });
            Assert.That(LobbyFixture.AckOf(events).Ok, Is.True);
            var poll = events.OfType<PollSubmitted>().Single();
            Assert.That(poll.PlayerId, Is.EqualTo(seats[0].PlayerId), "for the pollDone screen only");
            Assert.That(poll.Answers.Select(a => a.Key), Is.EqualTo(new[] { "fun", "waiting", "unfair" }), "poll.json order, bad ones left out");
            Assert.That(poll.Answers[0].Value, Is.EqualTo("The freeze on Dad"));
            Assert.That(poll.Answers[1].Value, Is.EqualTo(1));
            Assert.That(poll.Answers[2].Value, Is.EqualTo(2));
        }

        [Test]
        public void ATextAnswerIsCutTo200Characters()
        {
            var clean = PollAnswers.Clean(TestContent.Content().Poll,
                new Dictionary<string, object> { { "fun", new string('a', 250) } });
            Assert.That(((string)clean.Single().Value).Length, Is.EqualTo(ContentLimits.PollTextAnswerMax));
        }

        [Test]
        public void OnePollPerPlayer()
        {
            var (f, _) = AtThePoll();
            var answers = new Dictionary<string, object> { { "again", 0L } };
            Assert.That(LobbyFixture.AckOf(Poll(f, "c1", answers, "p1")).Ok, Is.True);
            Assert.That(LobbyFixture.AckOf(Poll(f, "c1", answers, "p1")).Reason, Is.EqualTo(AckReasons.Duplicate));
            var second = Poll(f, "c1", answers);
            Assert.That(LobbyFixture.AckOf(second).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(second.OfType<PollSubmitted>(), Is.Empty);
            Assert.That(LobbyFixture.AckOf(Poll(f, "c2", null)).Reason, Is.EqualTo(AckReasons.Invalid), "answers must be an object");
        }

        [Test]
        public void ThePollEndsWhenEveryConnectedPhoneSubmitted()
        {
            var (f, seats) = AtThePoll();
            f.Send(new ConnectionClosed("c3"));
            Poll(f, "c1", new Dictionary<string, object>());
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Poll));
            var events = Poll(f, "c2", new Dictionary<string, object> { { "again", 0L } });
            Assert.That(LobbyFixture.FlowEvents(events).Select(LobbyFixture.Describe),
                Does.Contain("PhaseEnded(Poll, allDone)").And.Contain("PhaseStarted(Lobby)"));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            Assert.That(seats.All(s => !s.Ready && s.Score == 0), Is.True, "ready cleared and scores at 0 (D76)");
        }

        [Test]
        public void ThePollEndsAfterMaxSeconds()
        {
            var (f, _) = AtThePoll();
            Poll(f, "c1", new Dictionary<string, object>());
            f.Advance(f.PollMs - 1);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Poll));
            var events = f.Advance(1);
            Assert.That(LobbyFixture.FlowEvents(events).Select(LobbyFixture.Describe), Does.Contain("PhaseEnded(Poll, timeout)"));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
        }

        [Test]
        public void APollOutsideThePollPhaseIsClosed()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            Assert.That(LobbyFixture.AckOf(Poll(f, "c1", new Dictionary<string, object>())).Reason, Is.EqualTo(AckReasons.Closed));
        }

        [Test]
        public void WithThePollOffResultsGoesStraightToTheLobby()
        {
            var content = TestContent.WithConfig(c => c["poll"]["enabled"] = false);
            var f = new LobbyFixture(content: content);
            f.SeatPlayers(2);
            f.ReadyAll();
            var events = f.RunToLobby();
            Assert.That(events.OfType<PhaseStarted>().Any(p => p.Phase == GamePhase.Poll), Is.False);
        }

        [Test]
        public void TheNextGameStartsFromTheLobbyAfterThePoll()
        {
            var (f, _) = AtThePoll(2);
            f.Advance(f.PollMs);
            f.StartNextGame();
            Assert.That(f.State.GameNumber, Is.EqualTo(2));
        }

        // ---------------------------------------------------------------- back to lobby

        [Test]
        public void BackToLobbyAbandonsARunningGame()
        {
            var f = new LobbyFixture(content: TestContent.WithConfig(c => c["powerUps"]["drawChanceLeader"] = 1.0));
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 500);
            f.Choice("c2", f.Right, 600);
            f.Choice("c3", f.Wrong, 600);
            Assert.That(seats[0].HeldPowerUp, Is.Not.Null);
            f.UsePowerUp("c1", seats[0].HeldPowerUp, seats[2].PlayerId);
            int characterBefore = seats[1].CharacterId;

            var events = f.Send(new BackToLobby());
            var names = LobbyFixture.FlowEvents(events).Select(LobbyFixture.Describe).ToList();
            Assert.That(names.First(), Is.EqualTo("SettingsChanged"));
            Assert.That(names, Does.Contain("PhaseEnded(Reveal, abandoned)"));
            var ended = events.OfType<GameEnded>().Single();
            Assert.That(ended.Reason, Is.EqualTo(GameEnded.ReasonBackToLobby));
            Assert.That(ended.WinnerId, Is.Null);
            Assert.That(events.OfType<SettingsChanged>().Single().Setting, Is.EqualTo(SettingsChanged.SettingBackToLobby));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            Assert.That(f.State.GameRunning, Is.False);
            Assert.That(f.State.Players.Count, Is.EqualTo(3), "everyone keeps their seat");
            Assert.That(seats[1].CharacterId, Is.EqualTo(characterBefore), "and their character");
            Assert.That(seats.All(s => s.CharacterPicked && !s.Ready && s.Score == 0 && s.HeldPowerUp == null), Is.True);
            Assert.That(seats[2].PendingEffects, Is.Empty);
            Assert.That(events.OfType<HostLineRequested>().Any(h => h.Trigger == HostTriggers.PlayerJoined), Is.False,
                "players kept after Back to lobby are not greeted again");
        }

        [Test]
        public void BackToLobbyDuringTheCountdownStopsIt()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            var events = f.Send(new BackToLobby());
            Assert.That(events.OfType<GameEnded>(), Is.Empty, "no game was running");
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            f.Advance(f.CountdownMs * 2);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby), "ready marks were cleared");
        }

        [Test]
        public void BackToLobbyInTheLobbyOnlyLogs()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            long phaseId = f.State.PhaseId;
            var events = f.Send(new BackToLobby());
            Assert.That(LobbyFixture.FlowEvents(events).Select(LobbyFixture.Describe), Is.EqualTo(new[] { "SettingsChanged" }));
            Assert.That(f.State.PhaseId, Is.EqualTo(phaseId));
        }

        [Test]
        public void BackToLobbyDuringThePollEndsIt()
        {
            var (f, _) = AtThePoll(2);
            var events = f.Send(new BackToLobby());
            Assert.That(LobbyFixture.FlowEvents(events).Select(LobbyFixture.Describe), Does.Contain("PhaseEnded(Poll, abandoned)"));
            Assert.That(events.OfType<GameEnded>(), Is.Empty, "the game had already ended with Results");
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
        }

        [Test]
        public void AfterBackToLobbyANewGameCanStart()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            f.Send(new BackToLobby());
            f.StartNextGame();
            Assert.That(f.State.GameNumber, Is.EqualTo(2));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.TopicVote), "no Intro in the next game");
        }
    }
}
