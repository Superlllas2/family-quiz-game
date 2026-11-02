using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// The show order and timers (docs/02_show_flow.md, D75), Ready and the start countdown, and the return to the
    /// Lobby after a game (D76; PROTOCOL.md "lobby actions", docs/18_edge_cases.md).
    /// </summary>
    public sealed class ShowFlowTests
    {
        private static List<string> Names(IEnumerable<IGameEvent> events)
        {
            return events.Select(LobbyFixture.Describe).ToList();
        }

        private static string[] ReadyNoStart => new[] { "InputAcked(True)", "LobbyActionHandled", "ReadyChanged" };

        [Test]
        public void TheLastReadyRunsTheCountdownFromTheConfig()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.Pick(seats[0]);
            f.Pick(seats[1]);
            long lobbyId = f.State.PhaseId;

            Assert.That(Names(f.Lobby("c1", LobbyActions.Ready)), Is.EqualTo(ReadyNoStart));
            Assert.That(seats[0].Ready, Is.True);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby), "one of two ready: no countdown");

            var events = f.Lobby("c2", LobbyActions.Ready);
            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "InputAcked(True)", "LobbyActionHandled", "ReadyChanged", "PhaseEnded(Lobby, allDone)", "PhaseStarted(StartCountdown)"
            }));
            var ready = events.OfType<ReadyChanged>().Single();
            Assert.That(ready.PlayerId, Is.EqualTo(seats[1].PlayerId));
            Assert.That(ready.Ready, Is.True);
            Assert.That(events.OfType<PhaseEnded>().Single().PhaseId, Is.EqualTo(lobbyId));
            var started = events.OfType<PhaseStarted>().Single();
            Assert.That(started.PhaseId, Is.EqualTo(lobbyId + 1));
            Assert.That(started.BudgetMs, Is.EqualTo(f.CountdownMs));
            Assert.That(f.State.PhaseEndsAtMs, Is.EqualTo(f.Clock.NowMs + f.CountdownMs));
            Assert.That(f.Game.EndsInMs(f.State.Players[0]), Is.EqualTo(f.CountdownMs));
        }

        [Test]
        public void ReadyAloneNeedsPlayersMinConnected()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(f.Config.Players.Min - 1);
            Assert.That(f.Game.EnoughPlayers, Is.False);
            f.ReadyAll();
            f.Advance(10000);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            Assert.That(f.State.Players.All(p => p.Ready), Is.True);
        }

        [Test]
        public void ReadyNeedsAPickedCharacter()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            var events = f.Lobby("c1", LobbyActions.Ready);
            Assert.That(Names(events), Is.EqualTo(new[] { "InputAcked(False, invalid)", "LobbyActionHandled" }));
            Assert.That(events.OfType<LobbyActionHandled>().Single().Ok, Is.False);
            Assert.That(seats[0].Ready, Is.False);
        }

        [Test]
        public void APlayerStillPickingACharacterHoldsTheStart()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(3);
            f.Pick(seats[0]);
            f.Pick(seats[1]);
            f.Lobby("c1", LobbyActions.Ready);
            f.Lobby("c2", LobbyActions.Ready);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby), "P3 is connected and not ready");
            f.Pick(seats[2]);
            f.Lobby("c3", LobbyActions.Ready);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
        }

        [Test]
        public void ADisconnectedPlayerNeverHoldsTheStart()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(3);
            f.Send(new ConnectionClosed("c3"));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            f.ReadyAll();
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            Assert.That(seats[2].Ready, Is.False);
        }

        [Test]
        public void TheLastNotReadyPlayerDisconnectingStartsTheCountdown()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(3);
            f.Pick(seats[0]);
            f.Pick(seats[1]);
            f.Lobby("c1", LobbyActions.Ready);
            f.Lobby("c2", LobbyActions.Ready);
            var events = f.Send(new ConnectionClosed("c3"));
            Assert.That(Names(events).Skip(Names(events).Count - 2), Is.EqualTo(new[] { "PhaseEnded(Lobby, allDone)", "PhaseStarted(StartCountdown)" }));
        }

        [Test]
        public void ReadyAndNotReadyWithNoChangeAreOkWithoutAnEvent()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(3);
            f.Pick(seats[0]);
            Assert.That(Names(f.Lobby("c1", LobbyActions.NotReady)), Is.EqualTo(new[] { "InputAcked(True)", "LobbyActionHandled" }));
            f.Lobby("c1", LobbyActions.Ready);
            Assert.That(Names(f.Lobby("c1", LobbyActions.Ready)), Is.EqualTo(new[] { "InputAcked(True)", "LobbyActionHandled" }));
            Assert.That(seats[0].Ready, Is.True);
        }

        [Test]
        public void TheLastTwoReadyAtOnceRunOneCountdown()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(3);
            f.ReadyAll();
            long id = f.State.PhaseId;
            var again = f.Lobby("c3", LobbyActions.Ready);
            Assert.That(LobbyFixture.AckOf(again).Ok, Is.True);
            Assert.That(f.State.PhaseId, Is.EqualTo(id));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
        }

        [TestCase("start")]
        [TestCase("cancelStart")]
        [TestCase("playAgain")]
        [TestCase("dance")]
        public void OtherLobbyActionsAreInvalid(string action)
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.Pick(seats[0]);
            Assert.That(LobbyFixture.AckOf(f.Lobby("c1", action)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
        }

        [TestCase(LobbyActions.Ready)]
        [TestCase(LobbyActions.NotReady)]
        public void LobbyActionsDuringAGameAreInvalid(string action)
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            Assert.That(LobbyFixture.AckOf(f.Lobby("c1", action)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer));
        }

        [Test]
        public void ALobbyActionResentGetsTheSameAckOnly()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.Pick(seats[0]);
            f.Send(new LobbyAction("c1", "s1", LobbyActions.Ready));
            var again = f.Send(new LobbyAction("c1", "s1", LobbyActions.Ready));
            Assert.That(Names(again), Is.EqualTo(new[] { "InputAcked(True, duplicate)" }));
        }

        [Test]
        public void NotReadyFromAnyPhoneStopsTheCountdown()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.ReadyAll();
            f.Advance(f.CountdownMs - 1);
            long countdownId = f.State.PhaseId;
            var events = f.Lobby("c2", LobbyActions.NotReady);
            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "InputAcked(True)", "LobbyActionHandled", "ReadyChanged", "PhaseEnded(StartCountdown, cancelled)", "PhaseStarted(Lobby)"
            }));
            Assert.That(events.OfType<PhaseEnded>().Single().PhaseId, Is.EqualTo(countdownId));
            Assert.That(events.OfType<PhaseEnded>().Single().DurationMs, Is.EqualTo(f.CountdownMs - 1));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            Assert.That(f.State.PhaseId, Is.EqualTo(countdownId + 1));
            Assert.That(f.State.PhaseEndsAtMs, Is.Null);
            Assert.That(f.State.GameNumber, Is.EqualTo(0));
            Assert.That(seats[0].Ready, Is.True, "the others stay ready");
            Assert.That(seats[1].Ready, Is.False);
            f.Advance(10000);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby), "nothing starts while someone is not ready");

            f.Lobby("c2", LobbyActions.Ready);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown), "Ready again restarts it");
            Assert.That(f.State.PhaseEndsAtMs, Is.EqualTo(f.Clock.NowMs + f.CountdownMs), "from the full countdown");
        }

        [Test]
        public void NotReadyAtTheScheduledEndIsTooLate()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            f.Clock.Advance(f.CountdownMs);
            var events = f.Lobby("c2", LobbyActions.NotReady);
            Assert.That(events.OfType<GameStarted>().Count(), Is.EqualTo(1));
            Assert.That(LobbyFixture.AckOf(events).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
        }

        [Test]
        public void AJoinDuringTheCountdownStopsIt()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            var events = f.Join("c3", "Late");
            Assert.That(events.OfType<PlayerJoined>().Count(), Is.EqualTo(1), "a join during the countdown is welcome");
            Assert.That(Names(events).Skip(Names(events).Count - 2), Is.EqualTo(new[] { "PhaseEnded(StartCountdown, cancelled)", "PhaseStarted(Lobby)" }));
            f.ReadyAll();
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown), "once the new player is ready it runs again");
        }

        [Test]
        public void ANotReadyPlayerReconnectingDuringTheCountdownStopsIt()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(3);
            f.Send(new ConnectionClosed("c3"));
            f.ReadyAll();
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            f.Send(new ConnectionOpened("c3b"));
            f.Send(new PlayerRejoin("c3b", seats[2].Token));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
        }

        [Test]
        public void LeaveDuringTheCountdownBelowPlayersMinCancelsIt()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            var events = f.Send(new PlayerLeave("c2", "bye"));
            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "InputAcked(True)", "PlayerLeft", "RosterChanged", "PhaseEnded(StartCountdown, cancelled)", "PhaseStarted(Lobby)"
            }));
        }

        [Test]
        public void LeaveDuringTheCountdownWithEnoughPlayersKeepsIt()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(3);
            f.ReadyAll();
            f.Send(new PlayerLeave("c2", "bye"));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            f.Advance(f.CountdownMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
        }

        [Test]
        public void ADisconnectDuringTheCountdownBelowPlayersMinConnectedCancelsIt()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            var events = f.Send(new ConnectionClosed("c2"));
            Assert.That(Names(events).Skip(Names(events).Count - 2), Is.EqualTo(new[] { "PhaseEnded(StartCountdown, cancelled)", "PhaseStarted(Lobby)" }));
        }

        [Test]
        public void ADisconnectDuringTheCountdownWithEnoughConnectedKeepsIt()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(3);
            f.ReadyAll();
            f.Send(new ConnectionClosed("c2"));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            f.Advance(f.CountdownMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
            Assert.That(f.State.Players.Count, Is.EqualTo(3), "the dropped seat is held and plays");
        }

        [Test]
        public void TheCountdownEndStartsTheGame()
        {
            var f = new LobbyFixture(seed: 777);
            var seats = f.SeatPlayers(3);
            f.ReadyAll();
            long countdownStart = f.Clock.NowMs;
            var events = f.Advance(f.CountdownMs + 3); // a late frame: the game starts at the scheduled time anyway

            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "PhaseEnded(StartCountdown, timeout)", "GameStarted", "PlayerUpdated", "PlayerUpdated", "PlayerUpdated",
                "PhaseStarted(QuestionShow)"
            }));
            var started = events.OfType<GameStarted>().Single();
            Assert.That(started.Game, Is.EqualTo(1));
            Assert.That(started.Seed, Is.EqualTo(777));
            Assert.That(started.Mode, Is.EqualTo(GameStarted.ModeStandard));
            Assert.That(started.PlayerIds, Is.EqualTo(seats.Select(p => p.PlayerId)));
            Assert.That(started.GameLang, Is.EqualTo(f.State.GameLanguage));
            Assert.That(f.State.GameNumber, Is.EqualTo(1));
            Assert.That(f.State.Seed, Is.EqualTo(777));
            Assert.That(f.State.GameRunning, Is.True);
            Assert.That(f.State.GameStartedAtMs, Is.EqualTo(countdownStart + f.CountdownMs));
            Assert.That(f.State.QuestionCount, Is.EqualTo(f.Config.Rounds.QuestionsPerRound));
            Assert.That(f.State.PhaseStartedAtMs, Is.EqualTo(countdownStart + f.CountdownMs));
            Assert.That(f.State.Question, Is.Not.Null);
            Assert.That(f.State.Question.Number, Is.EqualTo(1));
            Assert.That(f.State.Question.AnswerPhaseId, Is.EqualTo(0));
            Assert.That(seats.All(p => p.Turn == null), Is.True);
            Assert.That(seats.All(p => !p.Ready), Is.True, "ready marks are cleared when a game starts");
            var show = events.OfType<PhaseStarted>().Single();
            Assert.That(show.BudgetMs, Is.EqualTo(f.QuestionShowMs));
            Assert.That(show.StagingExtraMs, Is.EqualTo(LobbyFixture.Ms(f.Config.Staging.ExtraSeconds.QuestionShow)));
        }

        [Test]
        public void JoinsAreRefusedDuringAGameAndWelcomeAfterIt()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            f.Advance(f.CountdownMs);
            Assert.That(LobbyFixture.RefusalOf(f.Join("x2", "Later")), Is.EqualTo(RefusalReasons.GameStarted), "game");
            f.RunToLobby();
            Assert.That(f.Join("x3", "Latest").OfType<PlayerJoined>().Count(), Is.EqualTo(1), "back in the Lobby");
        }

        [Test]
        public void TheGameRunsEveryPhaseInOrderWithItsBudget()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            var events = f.RunToLobby();

            var expected = new List<GamePhase> { GamePhase.QuestionShow };
            for (int q = 0; q < f.Config.Rounds.QuestionsPerRound; q++)
            {
                if (q > 0) expected.Add(GamePhase.QuestionShow);
                expected.AddRange(new[] { GamePhase.Answer, GamePhase.Reveal, GamePhase.Scores });
            }

            expected.Add(GamePhase.Results);
            expected.Add(GamePhase.Lobby);
            var started = events.OfType<PhaseStarted>().ToList();
            Assert.That(started.Select(s => s.Phase), Is.EqualTo(expected));

            var budgets = new Dictionary<GamePhase, long>
            {
                { GamePhase.StartCountdown, f.CountdownMs },
                { GamePhase.QuestionShow, f.QuestionShowMs },
                { GamePhase.Answer, f.WindowMs + f.GraceMs },
                { GamePhase.Reveal, f.RevealMs },
                { GamePhase.Scores, f.ScoresMs },
                { GamePhase.Results, f.ResultsMs }
            };
            foreach (var ended in events.OfType<PhaseEnded>())
            {
                Assert.That(ended.Reason, Is.EqualTo(PhaseEndReasons.Timeout), ended.Phase.ToString());
                Assert.That(ended.DurationMs, Is.EqualTo(budgets[ended.Phase]), ended.Phase.ToString());
            }

            // Scores keeps Reveal's phaseId; every other phase gets a new one.
            for (int i = 1; i < started.Count; i++)
            {
                if (started[i].Phase == GamePhase.Scores) Assert.That(started[i].PhaseId, Is.EqualTo(started[i - 1].PhaseId));
                else Assert.That(started[i].PhaseId, Is.EqualTo(started[i - 1].PhaseId + 1));
            }

            var gameEnded = events.OfType<GameEnded>().Single();
            Assert.That(gameEnded.Reason, Is.EqualTo(GameEnded.ReasonFinished));
            Assert.That(gameEnded.Game, Is.EqualTo(1));
            Assert.That(gameEnded.DurationMs, Is.EqualTo(f.State.PhaseStartedAtMs - f.State.GameStartedAtMs));
            Assert.That(f.State.GameRunning, Is.False);
            Assert.That(f.State.PhaseEndsAtMs, Is.Null);
            Assert.That(f.State.Question, Is.Null);
        }

        [Test]
        public void TheEndOfResultsEndsTheGameThenOpensTheLobby()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            f.RunUntilPhase(GamePhase.Results, 3600000, 50);
            var events = f.Advance(f.ResultsMs);
            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "PhaseEnded(Results, timeout)", "GameEnded", "PlayerUpdated", "PlayerUpdated", "PhaseStarted(Lobby)"
            }));
            Assert.That(f.State.PhaseEndsAtMs, Is.Null);
            f.Advance(60000);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby), "the Lobby waits for everyone to be ready again");
        }

        [Test]
        public void TurnsStayThroughScoresAndAreReplacedByTheNextQuestion()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Wrong, 0);
            var first = seats[0].Turn;
            f.RunUntilPhase(GamePhase.Scores);
            Assert.That(seats[0].Turn, Is.SameAs(first));
            Assert.That(seats[0].Turn.Points, Is.EqualTo(1000));
            Assert.That(seats[0].Turn.Revealed, Is.True);
            f.RunUntilPhase(GamePhase.QuestionShow);
            Assert.That(seats[0].Turn, Is.Null);
            Assert.That(f.State.Question.Number, Is.EqualTo(2));
        }

        [Test]
        public void ResultsSetTheWinnerAndMostCorrect()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            // Question 1: P1 fast right, P2 slow right, P3 wrong. Then nobody answers.
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Right, f.WindowMs);
            f.Choice("c3", f.Wrong, 0);
            f.RunUntilPhase(GamePhase.Results, 3600000, 50);

            Assert.That(f.State.WinnerId, Is.EqualTo(seats[0].PlayerId));
            Assert.That(f.State.MostCorrectIds, Is.EquivalentTo(new[] { seats[0].PlayerId, seats[1].PlayerId }));
            Assert.That(seats[0].Turn, Is.Null);
            Assert.That(f.State.Question, Is.Null);
            Assert.That(seats[2].Streak, Is.EqualTo(0));
        }

        [Test]
        public void NobodyCorrectMeansNoMostCorrectAndASeededWinner()
        {
            var winners = new HashSet<string>();
            for (int seed = 1; seed <= 12; seed++)
            {
                var f = new LobbyFixture(seed);
                f.SeatPlayers(3);
                f.ReadyAll();
                f.RunUntilPhase(GamePhase.Results, 3600000, 50);
                Assert.That(f.State.MostCorrectIds, Is.Empty);
                Assert.That(f.State.WinnerId, Is.Not.Null);
                winners.Add(f.State.WinnerId);

                var again = new LobbyFixture(seed);
                again.SeatPlayers(3);
                again.ReadyAll();
                again.RunUntilPhase(GamePhase.Results, 3600000, 50);
                Assert.That(again.State.WinnerId, Is.EqualTo(f.State.WinnerId), "the same seed picks the same winner");
            }

            Assert.That(winners.Count, Is.GreaterThan(1), "ties are broken by the seed, not always the first player");
        }

        [Test]
        public void BackInTheLobbyScoresAreZeroAndSeatsCharactersAndSettingsStay()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.Send(new PickCharacter("c1", "k1", 5));
            f.Send(new ChangeSettings("c2", "en", f.Config.Players.TimerMultiplierOptions.Max()));
            f.ReadyAll();
            f.Advance(f.CountdownMs);
            f.Advance(f.QuestionShowMs);
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Right, 0);
            f.RunUntilPhase(GamePhase.Results, 3600000, 50);
            Assert.That(seats[0].Score, Is.GreaterThan(0));
            Assert.That(seats[0].CorrectCount, Is.EqualTo(1));
            var tokens = seats.Select(p => p.Token).ToList();

            f.RunToLobby();
            Assert.That(f.State.Players, Is.EqualTo(seats));
            Assert.That(seats.Select(p => p.Token), Is.EqualTo(tokens));
            Assert.That(seats.All(p => p.Score == 0 && p.Streak == 0 && p.CorrectCount == 0), Is.True);
            Assert.That(seats.All(p => !p.Ready && p.Turn == null), Is.True);
            Assert.That(seats.All(p => p.CharacterPicked), Is.True, "nobody picks a character again");
            Assert.That(seats[0].CharacterId, Is.EqualTo(5));
            Assert.That(seats[1].Lang, Is.EqualTo("en"));
            Assert.That(seats[1].TimerMultiplier, Is.EqualTo(f.Config.Players.TimerMultiplierOptions.Max()));

            f.ReadyAll();
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            var start = f.Advance(f.CountdownMs);
            Assert.That(start.OfType<GameStarted>().Single().Game, Is.EqualTo(2));
            Assert.That(f.State.GameNumber, Is.EqualTo(2));
            Assert.That(f.Seeds.Count, Is.EqualTo(2));
            Assert.That(f.State.Seed, Is.EqualTo(f.Seeds[1]));
            Assert.That(start.OfType<PlayerUpdated>().Count(), Is.EqualTo(2));
        }

        [Test]
        public void AfterAGameWithOnePlayerLeftTheLobbyWaits()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            f.Advance(f.CountdownMs);
            f.Send(new PlayerLeave("c2", "bye"));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow), "a game goes on with one player");
            f.RunToLobby();
            Assert.That(f.Game.EnoughPlayers, Is.False);
            f.ReadyAll();
            f.Advance(10000);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
        }

        [Test]
        public void ThePlayersOfALeftSeatAreOutOfRanksAndResults()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c3", f.Right, 0);
            f.Send(new PlayerLeave("c3", "bye"));
            f.RunUntilPhase(GamePhase.Results, 3600000, 50);
            Assert.That(f.State.Players.Count, Is.EqualTo(2));
            Assert.That(f.State.WinnerId, Is.Not.EqualTo(seats[2].PlayerId));
            Assert.That(f.State.MostCorrectIds, Is.Empty);
        }
    }
}
