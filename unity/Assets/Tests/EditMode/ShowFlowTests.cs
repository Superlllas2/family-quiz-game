using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// The show order and timers (docs/02_show_flow.md, D75), the start countdown and Cancel, Play again
    /// (PROTOCOL.md "lobby actions", docs/18_edge_cases.md).
    /// </summary>
    public sealed class ShowFlowTests
    {
        private static List<string> Names(IEnumerable<IGameEvent> events)
        {
            return events.Select(LobbyFixture.Describe).ToList();
        }

        [Test]
        public void StartRunsTheCountdownFromTheConfig()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            Assert.That(f.Game.CanStart, Is.True);
            long lobbyId = f.State.PhaseId;
            var events = f.Lobby("c2", LobbyActions.Start);

            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "InputAcked(True)", "LobbyActionHandled", "PhaseEnded(Lobby, allDone)", "PhaseStarted(StartCountdown)"
            }));
            Assert.That(events.OfType<LobbyActionHandled>().Single().Ok, Is.True);
            Assert.That(events.OfType<PhaseEnded>().Single().PhaseId, Is.EqualTo(lobbyId));
            var started = events.OfType<PhaseStarted>().Single();
            Assert.That(started.PhaseId, Is.EqualTo(lobbyId + 1));
            Assert.That(started.BudgetMs, Is.EqualTo(f.CountdownMs));
            Assert.That(f.State.PhaseEndsAtMs, Is.EqualTo(f.Clock.NowMs + f.CountdownMs));
            Assert.That(f.State.CountdownReturnPhase, Is.EqualTo(GamePhase.Lobby));
            Assert.That(f.Game.EndsInMs(f.State.Players[0]), Is.EqualTo(f.CountdownMs));
            Assert.That(f.Game.CanStart, Is.False);
        }

        [Test]
        public void StartNeedsPlayersMin()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(f.Config.Players.Min - 1);
            Assert.That(f.Game.CanStart, Is.False);
            var events = f.Lobby("c1", LobbyActions.Start);
            Assert.That(Names(events), Is.EqualTo(new[] { "InputAcked(False, invalid)", "LobbyActionHandled" }));
            Assert.That(events.OfType<LobbyActionHandled>().Single().Ok, Is.False);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
        }

        [Test]
        public void StartOnTwoPhonesAtOnceRunsOneCountdown()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(3);
            f.Lobby("c1", LobbyActions.Start);
            long id = f.State.PhaseId;
            var second = f.Lobby("c3", LobbyActions.Start);
            Assert.That(LobbyFixture.AckOf(second).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(second.OfType<LobbyActionHandled>().Single().Ok, Is.False);
            Assert.That(f.State.PhaseId, Is.EqualTo(id));
        }

        [TestCase(LobbyActions.CancelStart)]
        [TestCase(LobbyActions.PlayAgain)]
        [TestCase("dance")]
        public void OtherLobbyActionsInTheLobbyAreInvalid(string action)
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            Assert.That(LobbyFixture.AckOf(f.Lobby("c1", action)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
        }

        [Test]
        public void ALobbyActionResentGetsTheSameAckOnly()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Send(new LobbyAction("c1", "s1", LobbyActions.Start));
            var again = f.Send(new LobbyAction("c1", "s1", LobbyActions.Start));
            Assert.That(Names(again), Is.EqualTo(new[] { "InputAcked(True, duplicate)" }));
        }

        [Test]
        public void CancelFromAnyPhoneReturnsToTheLobby()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            f.Advance(f.CountdownMs - 1);
            long countdownId = f.State.PhaseId;
            var events = f.Lobby("c2", LobbyActions.CancelStart);
            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "InputAcked(True)", "LobbyActionHandled", "PhaseEnded(StartCountdown, cancelled)", "PhaseStarted(Lobby)"
            }));
            Assert.That(events.OfType<PhaseEnded>().Single().PhaseId, Is.EqualTo(countdownId));
            Assert.That(events.OfType<PhaseEnded>().Single().DurationMs, Is.EqualTo(f.CountdownMs - 1));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            Assert.That(f.State.PhaseId, Is.EqualTo(countdownId + 1));
            Assert.That(f.State.PhaseEndsAtMs, Is.Null);
            Assert.That(f.State.GameNumber, Is.EqualTo(0));
            f.Advance(10000);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby), "nothing starts after a cancel");
            Assert.That(LobbyFixture.AckOf(f.Lobby("c1", LobbyActions.CancelStart)).Reason, Is.EqualTo(AckReasons.Invalid));
        }

        [Test]
        public void CancelAtTheScheduledEndIsTooLate()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            f.Clock.Advance(f.CountdownMs);
            var events = f.Lobby("c2", LobbyActions.CancelStart);
            Assert.That(events.OfType<GameStarted>().Count(), Is.EqualTo(1));
            Assert.That(LobbyFixture.AckOf(events).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
        }

        [Test]
        public void LeaveDuringTheCountdownBelowPlayersMinCancelsIt()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
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
            f.Lobby("c1", LobbyActions.Start);
            f.Send(new PlayerLeave("c2", "bye"));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            f.Advance(f.CountdownMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
        }

        [Test]
        public void ASeatExpiringDuringTheCountdownCancelsIt()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.Send(new ConnectionClosed("c2"));
            // The seat runs out just after Start, during the countdown.
            f.Advance(LobbyFixture.Ms(f.Config.Players.SeatHoldSeconds) - 1);
            f.Lobby("c1", LobbyActions.Start);
            var events = f.Advance(1);
            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "PlayerLeft", "RosterChanged", "PhaseEnded(StartCountdown, cancelled)", "PhaseStarted(Lobby)"
            }));
            Assert.That(f.State.FindById(seats[1].PlayerId), Is.Null);
        }

        [Test]
        public void ADisconnectDuringTheCountdownDoesNotCancelIt()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            f.Send(new ConnectionClosed("c2"));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
        }

        [Test]
        public void TheCountdownEndStartsTheGame()
        {
            var f = new LobbyFixture(seed: 777);
            var seats = f.SeatPlayers(3);
            f.Lobby("c1", LobbyActions.Start);
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
            var show = events.OfType<PhaseStarted>().Single();
            Assert.That(show.BudgetMs, Is.EqualTo(f.QuestionShowMs));
            Assert.That(show.StagingExtraMs, Is.EqualTo(LobbyFixture.Ms(f.Config.Staging.ExtraSeconds.QuestionShow)));
        }

        [Test]
        public void JoinsAreRefusedOutsideTheLobby()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            Assert.That(LobbyFixture.RefusalOf(f.Join("x1", "Late")), Is.EqualTo(RefusalReasons.GameStarted), "countdown");
            f.Advance(f.CountdownMs);
            Assert.That(LobbyFixture.RefusalOf(f.Join("x2", "Later")), Is.EqualTo(RefusalReasons.GameStarted), "game");
            f.RunToPlayAgain();
            Assert.That(LobbyFixture.RefusalOf(f.Join("x3", "Latest")), Is.EqualTo(RefusalReasons.GameStarted), "PlayAgain");
        }

        [Test]
        public void TheGameRunsEveryPhaseInOrderWithItsBudget()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            var events = f.RunToPlayAgain();

            var expected = new List<GamePhase> { GamePhase.QuestionShow };
            for (int q = 0; q < f.Config.Rounds.QuestionsPerRound; q++)
            {
                if (q > 0) expected.Add(GamePhase.QuestionShow);
                expected.AddRange(new[] { GamePhase.Answer, GamePhase.Reveal, GamePhase.Scores });
            }

            expected.Add(GamePhase.Results);
            expected.Add(GamePhase.PlayAgain);
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
        public void TheEndOfResultsEndsTheGameThenOpensPlayAgain()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            f.RunUntilPhase(GamePhase.Results, 3600000, 50);
            var events = f.Advance(f.ResultsMs);
            Assert.That(Names(events), Is.EqualTo(new[] { "PhaseEnded(Results, timeout)", "GameEnded", "PhaseStarted(PlayAgain)" }));
            Assert.That(f.Game.CanPlayAgain, Is.True);
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
                f.Lobby("c1", LobbyActions.Start);
                f.RunUntilPhase(GamePhase.Results, 3600000, 50);
                Assert.That(f.State.MostCorrectIds, Is.Empty);
                Assert.That(f.State.WinnerId, Is.Not.Null);
                winners.Add(f.State.WinnerId);

                var again = new LobbyFixture(seed);
                again.SeatPlayers(3);
                again.Lobby("c1", LobbyActions.Start);
                again.RunUntilPhase(GamePhase.Results, 3600000, 50);
                Assert.That(again.State.WinnerId, Is.EqualTo(f.State.WinnerId), "the same seed picks the same winner");
            }

            Assert.That(winners.Count, Is.GreaterThan(1), "ties are broken by the seed, not always the first player");
        }

        [Test]
        public void PlayAgainKeepsSeatsAndResetsScores()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.Send(new PickCharacter("c1", "k1", 5));
            f.Send(new ChangeSettings("c2", "en", f.Config.Players.TimerMultiplierOptions.Max()));
            f.Lobby("c1", LobbyActions.Start);
            f.Advance(f.CountdownMs);
            f.Advance(f.QuestionShowMs);
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Right, 0);
            f.RunToPlayAgain();
            Assert.That(seats[0].Score, Is.GreaterThan(0));
            Assert.That(seats[0].CorrectCount, Is.EqualTo(1));
            var tokens = seats.Select(p => p.Token).ToList();

            var events = f.Lobby("c2", LobbyActions.PlayAgain);
            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "InputAcked(True)", "LobbyActionHandled", "PhaseEnded(PlayAgain, allDone)", "PhaseStarted(StartCountdown)"
            }));
            Assert.That(f.State.CountdownReturnPhase, Is.EqualTo(GamePhase.PlayAgain));
            Assert.That(seats[0].Score, Is.GreaterThan(0), "scores reset when the new game starts");

            var start = f.Advance(f.CountdownMs);
            Assert.That(start.OfType<GameStarted>().Single().Game, Is.EqualTo(2));
            Assert.That(f.State.GameNumber, Is.EqualTo(2));
            Assert.That(f.Seeds.Count, Is.EqualTo(2));
            Assert.That(f.State.Seed, Is.EqualTo(f.Seeds[1]));
            Assert.That(f.State.Players, Is.EqualTo(seats));
            Assert.That(seats.Select(p => p.Token), Is.EqualTo(tokens));
            Assert.That(seats.All(p => p.Score == 0 && p.Streak == 0 && p.CorrectCount == 0), Is.True);
            Assert.That(seats[0].CharacterId, Is.EqualTo(5));
            Assert.That(seats.All(p => p.CharacterPicked), Is.True);
            Assert.That(seats[1].Lang, Is.EqualTo("en"));
            Assert.That(seats[1].TimerMultiplier, Is.EqualTo(f.Config.Players.TimerMultiplierOptions.Max()));
            Assert.That(start.OfType<PlayerUpdated>().Count(), Is.EqualTo(2));
        }

        [Test]
        public void CancelAfterPlayAgainReturnsToPlayAgain()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            f.RunToPlayAgain();
            f.Lobby("c1", LobbyActions.PlayAgain);
            var events = f.Lobby("c2", LobbyActions.CancelStart);
            Assert.That(Names(events).Skip(2), Is.EqualTo(new[] { "PhaseEnded(StartCountdown, cancelled)", "PhaseStarted(PlayAgain)" }));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.PlayAgain));
            Assert.That(f.Game.CanPlayAgain, Is.True);
            Assert.That(LobbyFixture.AckOf(f.Lobby("c1", LobbyActions.Start)).Reason, Is.EqualTo(AckReasons.Invalid));
        }

        [Test]
        public void PlayAgainNeedsPlayersMin()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.Lobby("c1", LobbyActions.Start);
            f.Advance(f.CountdownMs);
            f.Send(new PlayerLeave("c2", "bye"));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow), "a game goes on with one player");
            f.RunToPlayAgain();
            Assert.That(f.Game.CanPlayAgain, Is.False);
            Assert.That(LobbyFixture.AckOf(f.Lobby("c1", LobbyActions.PlayAgain)).Reason, Is.EqualTo(AckReasons.Invalid));
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
