using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// Headless games on a fake clock with 3 simulated players (fast, normal, slow; docs/13_architecture.md), driving the
    /// real Game from everyone tapping Ready through Results, back to the Lobby, Ready again and a second game
    /// (docs/19_acceptance.md, Day 2; D76).
    /// </summary>
    public sealed class SimulatedGameTests
    {
        private const long StepMs = 5;
        private const long MaxEndAfterLastLockMs = 200;

        private sealed class PhaseRecord
        {
            public GamePhase Phase;
            public long PhaseId;
            public long StartMs;
            public long EndMs = -1;
            public string Reason;
            public long BudgetMs;
            public int Game;
        }

        private sealed class Run
        {
            public readonly List<PhaseRecord> Phases = new List<PhaseRecord>();
            public readonly List<IGameEvent> Events = new List<IGameEvent>();
            public readonly Dictionary<long, long> LastLockMsByAnswer = new Dictionary<long, long>();
            public readonly Dictionary<long, long> LatestDeadlineByAnswer = new Dictionary<long, long>();
            public readonly Dictionary<string, int> PointsThisGame = new Dictionary<string, int>();
            public int Game;
        }

        private static List<SimulatedPlayer> Players(int seed)
        {
            return new List<SimulatedPlayer>
            {
                new SimulatedPlayer("fast", "c1", "Fast", 1000, 3000, 0.9, seed * 10 + 1),
                new SimulatedPlayer("normal", "c2", "Normal", 3000, 8000, 0.6, seed * 10 + 2),
                new SimulatedPlayer("slow", "c3", "Slow", 7000, 14000, 0.3, seed * 10 + 3)
            };
        }

        /// <summary>A run that starts in the current (untimed) phase, the Lobby.</summary>
        private static Run NewRun(LobbyFixture f)
        {
            var run = new Run();
            run.Phases.Add(new PhaseRecord { Phase = f.State.Phase, PhaseId = f.State.PhaseId, StartMs = f.State.PhaseStartedAtMs });
            return run;
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void TwoGamesRunInOrderOnTime(int seed)
        {
            var f = new LobbyFixture(seed);
            var sims = Players(seed);
            foreach (var s in sims) f.Seat(s.Conn, s.Name);
            var run = NewRun(f);

            Record(f, run, f.ReadyAll());
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            Drive(f, sims, run, GamePhase.Results);
            CheckGame(f, run, 1);
            Drive(f, sims, run, GamePhase.Lobby);
            CheckBackInTheLobby(f, run, 1);

            run.PointsThisGame.Clear();
            Record(f, run, f.ReadyAll());
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            Drive(f, sims, run, GamePhase.Results);
            CheckGame(f, run, 2);
            Drive(f, sims, run, GamePhase.Lobby);
            CheckBackInTheLobby(f, run, 2);

            CheckOrder(f, run);
            CheckTiming(f, run);
            Print(run);
        }

        /// <summary>Steps the clock in small frames, letting the simulated players act, until the phase is reached again.</summary>
        private static void Drive(LobbyFixture f, List<SimulatedPlayer> sims, Run run, GamePhase until)
        {
            long limit = f.Clock.NowMs + 3600000;
            do
            {
                f.Clock.Advance(StepMs);
                Record(f, run, f.Game.Tick());
                foreach (var s in sims)
                {
                    var input = s.Act(f.Game);
                    if (input == null) continue;
                    var events = f.Send(input);
                    Assert.That(LobbyFixture.AckOf(events).Ok, Is.True, s.Profile + " input refused: " + LobbyFixture.Describe(events));
                    Record(f, run, events);
                }

                Assert.That(f.Clock.NowMs, Is.LessThan(limit), "the game never reached " + until);
            } while (f.State.Phase != until);
        }

        private static void Record(LobbyFixture f, Run run, IReadOnlyList<IGameEvent> events)
        {
            long now = f.Clock.NowMs;
            foreach (var e in events)
            {
                run.Events.Add(e);
                switch (e)
                {
                    case GameStarted g:
                        run.Game = g.Game;
                        break;
                    case PhaseEnded ended:
                    {
                        var last = run.Phases.Last();
                        Assert.That(ended.Phase, Is.EqualTo(last.Phase));
                        last.EndMs = last.StartMs + ended.DurationMs;
                        last.Reason = ended.Reason;
                        Assert.That(last.EndMs, Is.LessThanOrEqualTo(now), "a phase never ends in the future");
                        break;
                    }
                    case PhaseStarted started:
                    {
                        // A phase starts where the previous one ended (its scheduled end), or now for the first one.
                        long start = run.Phases.Count > 0 && run.Phases.Last().EndMs >= 0 ? run.Phases.Last().EndMs : now;
                        run.Phases.Add(new PhaseRecord
                        {
                            Phase = started.Phase, PhaseId = started.PhaseId, StartMs = start, BudgetMs = started.BudgetMs, Game = run.Game
                        });
                        if (started.Phase == GamePhase.Answer)
                            run.LatestDeadlineByAnswer[started.PhaseId] = f.State.Players.Max(p => p.Turn.DeadlineMs);
                        break;
                    }
                    case AnswerLocked locked:
                        run.LastLockMsByAnswer[locked.PhaseId] = now;
                        run.PointsThisGame[locked.PlayerId] = (run.PointsThisGame.TryGetValue(locked.PlayerId, out var p) ? p : 0) + locked.Points;
                        break;
                }
            }
        }

        private static void CheckGame(LobbyFixture f, Run run, int game)
        {
            Assert.That(f.State.GameNumber, Is.EqualTo(game));
            foreach (var player in f.State.Players)
            {
                run.PointsThisGame.TryGetValue(player.PlayerId, out var sum);
                Assert.That(player.Score, Is.EqualTo(sum), "score of " + player.Name + " is the sum of its AnswerLocked points");
            }

            Assert.That(f.State.WinnerId, Is.Not.Null);
            int best = f.State.Players.Max(p => p.Score);
            Assert.That(f.State.FindById(f.State.WinnerId).Score, Is.EqualTo(best));
            int most = f.State.Players.Max(p => p.CorrectCount);
            var mostIds = most == 0 ? new List<string>() : f.State.Players.Where(p => p.CorrectCount == most).Select(p => p.PlayerId).ToList();
            Assert.That(f.State.MostCorrectIds, Is.EquivalentTo(mostIds));
        }

        /// <summary>After Results: the game ended with the Results winner, and everyone is back in the Lobby at 0 (D76).</summary>
        private static void CheckBackInTheLobby(LobbyFixture f, Run run, int game)
        {
            var ended = run.Events.OfType<GameEnded>().Last();
            Assert.That(ended.Game, Is.EqualTo(game));
            Assert.That(ended.Reason, Is.EqualTo(GameEnded.ReasonFinished));
            Assert.That(ended.WinnerId, Is.EqualTo(f.State.WinnerId));
            Assert.That(f.State.Players.All(p => p.Score == 0 && !p.Ready && p.CharacterPicked), Is.True);
        }

        private static void CheckOrder(LobbyFixture f, Run run)
        {
            var expected = new List<GamePhase> { GamePhase.Lobby };
            for (int game = 0; game < 2; game++)
            {
                expected.Add(GamePhase.StartCountdown);
                for (int q = 0; q < f.Config.Rounds.QuestionsPerRound; q++)
                    expected.AddRange(new[] { GamePhase.QuestionShow, GamePhase.Answer, GamePhase.Reveal, GamePhase.Scores });
                expected.Add(GamePhase.Results);
                expected.Add(GamePhase.Lobby);
            }

            Assert.That(run.Phases.Select(p => p.Phase), Is.EqualTo(expected));
            for (int i = 1; i < run.Phases.Count; i++)
            {
                var p = run.Phases[i];
                long expectedId = p.Phase == GamePhase.Scores ? run.Phases[i - 1].PhaseId : run.Phases[i - 1].PhaseId + 1;
                Assert.That(p.PhaseId, Is.EqualTo(expectedId), "phaseId of " + p.Phase);
            }
        }

        private static void CheckTiming(LobbyFixture f, Run run)
        {
            int allLockedQuestions = 0;
            foreach (var p in run.Phases.Where(x => x.EndMs >= 0))
            {
                long duration = p.EndMs - p.StartMs;
                switch (p.Phase)
                {
                    case GamePhase.StartCountdown:
                        Assert.That(duration, Is.EqualTo(f.CountdownMs));
                        break;
                    case GamePhase.QuestionShow:
                        Assert.That(duration, Is.EqualTo(f.QuestionShowMs));
                        break;
                    case GamePhase.Reveal:
                        Assert.That(duration, Is.EqualTo(f.RevealMs));
                        break;
                    case GamePhase.Scores:
                        Assert.That(duration, Is.EqualTo(f.ScoresMs));
                        break;
                    case GamePhase.Results:
                        Assert.That(duration, Is.EqualTo(f.ResultsMs));
                        break;
                    case GamePhase.Answer:
                        long budgetEnd = run.LatestDeadlineByAnswer[p.PhaseId] + f.GraceMs;
                        Assert.That(p.EndMs, Is.LessThanOrEqualTo(budgetEnd), "Answer never outlasts the latest deadline plus grace");
                        if (p.Reason == PhaseEndReasons.AllDone)
                        {
                            allLockedQuestions++;
                            long lastLock = run.LastLockMsByAnswer[p.PhaseId];
                            Assert.That(p.EndMs - lastLock, Is.InRange(0, MaxEndAfterLastLockMs),
                                "the question ends within 200 ms of the last lock in");
                        }

                        break;
                }
            }

            TestContext.WriteLine("Questions where everyone locked in: " + allLockedQuestions);
        }

        private static void Print(Run run)
        {
            TestContext.WriteLine("game  phase           phaseId  budgetMs  durationMs  reason");
            foreach (var p in run.Phases)
            {
                string duration = p.EndMs >= 0 ? (p.EndMs - p.StartMs).ToString() : "-";
                TestContext.WriteLine(string.Format("{0,4}  {1,-14}  {2,7}  {3,8}  {4,10}  {5}", p.Game, p.Phase, p.PhaseId, p.BudgetMs,
                    duration, p.Reason ?? ""));
            }
        }

        [Test]
        public void EveryoneLockingInEndsTheQuestionAtTheLastLockIn()
        {
            // Three players who always answer within 2 s: every Answer must end allDone right at the last lock in.
            var f = new LobbyFixture(5);
            var sims = new List<SimulatedPlayer>
            {
                new SimulatedPlayer("fast", "c1", "A", 500, 2000, 1, 11),
                new SimulatedPlayer("fast", "c2", "B", 500, 2000, 0.5, 12),
                new SimulatedPlayer("fast", "c3", "C", 500, 2000, 0, 13)
            };
            foreach (var s in sims) f.Seat(s.Conn, s.Name);
            var run = NewRun(f);
            Record(f, run, f.ReadyAll());
            Drive(f, sims, run, GamePhase.Results);

            var answers = run.Phases.Where(p => p.Phase == GamePhase.Answer).ToList();
            Assert.That(answers.Count, Is.EqualTo(f.Config.Rounds.QuestionsPerRound));
            foreach (var a in answers)
            {
                Assert.That(a.Reason, Is.EqualTo(PhaseEndReasons.AllDone));
                Assert.That(a.EndMs - run.LastLockMsByAnswer[a.PhaseId], Is.InRange(0, MaxEndAfterLastLockMs));
            }

            CheckGame(f, run, 1);
            Print(run);
        }
    }
}
