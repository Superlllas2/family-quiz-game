using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// Headless Standard games on a fake clock with the six simulated players of docs/13_architecture.md (2 fast, 2
    /// normal, 2 slow), driving the real Game from everyone tapping Ready through Results and back to the Lobby, twice per
    /// session (D76). Checks docs/19_acceptance.md, Day 3. The clock jumps exactly to the next phase end or the next
    /// simulated input, so every phase ends at its exact budget, never a frame later.
    /// </summary>
    public sealed class SimulatedGameTests
    {
        /// <summary>Sessions of two games each, every one from an empty seen list: 100 Standard games.</summary>
        private const int Sessions = 50;

        private const long MinGameMs = 8 * 60 * 1000;
        private const long MaxGameMs = 12 * 60 * 1000;

        /// <summary>
        /// The idle limit of docs/19_acceptance.md: a starting value set again with the timers (D56). The current timers
        /// give exactly 11 s (QuestionShow + Reveal + Scores, and Reveal + Scores + RoundEnd).
        /// </summary>
        private const long IdleLimitMs = 11000;

        private const long MaxEndAfterLastLockMs = 200;

        private static readonly GamePhase[] InputPhases = { GamePhase.TopicVote, GamePhase.Answer, GamePhase.RaceQuestion };
        private static readonly GamePhase[] LeftOutOfIdle = { GamePhase.Intro, GamePhase.FinalIntro, GamePhase.Results };

        private sealed class PhaseRecord
        {
            public GamePhase Phase;
            public long PhaseId;
            public long StartMs;
            public long EndMs = -1;
            public string Reason;
            public long BudgetMs;
            public long ScheduledEndMs;
        }

        private sealed class GameRecord
        {
            public int Game;
            public readonly List<PhaseRecord> Phases = new List<PhaseRecord>();
            public readonly List<string> Questions = new List<string>();
            public readonly List<string> Statements = new List<string>();
            public readonly Dictionary<long, long> LastLockMs = new Dictionary<long, long>();
            public readonly Dictionary<string, int> ScoreDeltas = new Dictionary<string, int>();
            public RaceStarted Race;
            public Dictionary<string, int> ExpectedHeadStarts;
            public GameEnded Ended;
            public string WinnerId;
            public long DurationMs;
            public long IdleMs;
            public long LongestIdleMs;
            public int Drawn;
            public int Used;
            public int Applied;
            public int Cleared;
            public int Polls;
            public double IdleShare => DurationMs > 0 ? (double)IdleMs / DurationMs : 0;
        }

        // ---------------------------------------------------------------- the driver

        /// <summary>Runs one game from the Lobby (everyone Ready) back to the Lobby.</summary>
        private static GameRecord PlayGame(LobbyFixture f, SimulatedPlayer[] sims)
        {
            var record = new GameRecord();
            Record(f, record, f.ReadyAll());
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            long limit = f.Clock.NowMs + 3600000;
            while (f.State.Phase != GamePhase.Lobby)
            {
                long now = f.Clock.NowMs;
                long next = f.State.PhaseEndsAtMs ?? long.MaxValue;
                foreach (var s in sims)
                {
                    var at = s.PlannedAtMs(f.Game);
                    if (at.HasValue) next = Math.Min(next, at.Value);
                }

                Assert.That(next, Is.LessThan(limit), "the game never ended");
                if (next > now) f.Clock.Advance(next - now);
                Record(f, record, f.Game.Tick());
                foreach (var s in sims)
                {
                    var input = s.Act(f.Game);
                    if (input == null) continue;
                    var events = f.Send(input);
                    Assert.That(LobbyFixture.AckOf(events).Ok, Is.True,
                        s.Profile + " " + input.GetType().Name + " refused in " + f.State.Phase + ": " + LobbyFixture.Describe(events));
                    Record(f, record, events);
                }
            }

            Measure(record);
            return record;
        }

        private static void Record(LobbyFixture f, GameRecord r, IReadOnlyList<IGameEvent> events)
        {
            long now = f.Clock.NowMs;
            foreach (var e in events)
            {
                switch (e)
                {
                    case GameStarted g:
                        r.Game = g.Game;
                        break;
                    case PhaseEnded ended:
                    {
                        var last = r.Phases.LastOrDefault();
                        if (last == null || last.Phase != ended.Phase) break; // the Lobby or the countdown before the game
                        last.EndMs = last.StartMs + ended.DurationMs;
                        last.Reason = ended.Reason;
                        Assert.That(last.EndMs, Is.LessThanOrEqualTo(now), "a phase never ends in the future");
                        break;
                    }
                    case PhaseStarted started:
                    {
                        if (started.Phase == GamePhase.StartCountdown || (started.Phase == GamePhase.Lobby && r.Phases.Count == 0)) break;
                        // A phase starts where the previous one ended (its scheduled end).
                        long start = r.Phases.Count > 0 ? r.Phases.Last().EndMs : f.State.GameStartedAtMs;
                        Assert.That(start, Is.GreaterThanOrEqualTo(0), started.Phase + " starts before the last phase ended");
                        r.Phases.Add(new PhaseRecord
                        {
                            Phase = started.Phase, PhaseId = started.PhaseId, StartMs = start, BudgetMs = started.BudgetMs,
                            ScheduledEndMs = started.EndsAtMs ?? -1
                        });
                        if (started.Phase == GamePhase.QuestionShow) r.Questions.Add(f.State.Question.Def.Id);
                        if (started.Phase == GamePhase.RaceQuestion) r.Statements.Add(f.State.Race.Statement.Id);
                        if (started.Phase == GamePhase.Results) CheckResults(f, r);
                        break;
                    }
                    case AnswerLocked locked:
                        r.LastLockMs[locked.PhaseId] = now;
                        break;
                    case PowerUpDrawn _:
                        r.Drawn++;
                        Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal), "draws happen at Reveal");
                        break;
                    case PowerUpUsed _:
                        r.Used++;
                        Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal).Or.EqualTo(GamePhase.Scores));
                        break;
                    case PowerUpApplied applied:
                        r.Applied++;
                        Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer), "effects land when Answer starts");
                        Assert.That(applied.PhaseId, Is.EqualTo(f.State.PhaseId));
                        break;
                    case EffectCleared _:
                        r.Cleared++;
                        break;
                    case PollSubmitted _:
                        r.Polls++;
                        break;
                    case ScoreChanged score:
                        r.ScoreDeltas[score.PlayerId] = (r.ScoreDeltas.TryGetValue(score.PlayerId, out var sum) ? sum : 0) + score.Delta;
                        break;
                    case RaceStarted race:
                        r.Race = race;
                        int players = f.State.Players.Count;
                        r.ExpectedHeadStarts = f.State.Players.ToDictionary(p => p.PlayerId,
                            p => RaceRules.HeadStart(Ranking.SharedRank(f.State, p), players, f.Config.Race.MaxHeadStart));
                        break;
                    case GameEnded ended:
                        r.Ended = ended;
                        Assert.That(ended.WinnerId, Is.EqualTo(r.WinnerId), "the game winner is the race winner");
                        break;
                }
            }
        }

        /// <summary>When Results starts: the final scores, the race result and the awards.</summary>
        private static void CheckResults(LobbyFixture f, GameRecord r)
        {
            foreach (var p in f.State.Players)
            {
                r.ScoreDeltas.TryGetValue(p.PlayerId, out var sum);
                Assert.That(p.Score, Is.EqualTo(sum), "the score of " + p.Name + " is the sum of its ScoreChanged deltas");
            }

            var race = f.State.Race;
            Assert.That(race.Finished, Is.True);
            bool someoneFinished = race.Positions.Values.Any(v => v >= f.Config.Race.FinishSteps);
            Assert.That(race.StatementNumber, Is.LessThanOrEqualTo(f.Config.Race.MaxQuestions));
            Assert.That(someoneFinished || race.StatementNumber == f.Config.Race.MaxQuestions, Is.True,
                "the race ends at the finish or after race.maxQuestions");
            if (someoneFinished && race.StatementNumber > 1)
            {
                // Nobody had finished one statement earlier, or the race would have ended there.
                Assert.That(race.Positions.Values.Max(), Is.LessThanOrEqualTo(f.Config.Race.FinishSteps));
            }

            Assert.That(f.State.WinnerId, Is.EqualTo(f.State.Placement[0]));
            Assert.That(race.Positions[f.State.WinnerId], Is.EqualTo(race.Positions.Values.Max()), "the winner is the furthest");
            r.WinnerId = f.State.WinnerId;
            Assert.That(f.State.MostCorrectIds, Is.EquivalentTo(Awards.MostCorrect(f.State.Players)));
        }

        /// <summary>
        /// Idle (docs/15_schemas.md): time no connected phone can send an input. Every input phase ends the moment nobody
        /// can answer any more, so idle time is the time outside TopicVote, Answer and RaceQuestion. The longest stretch
        /// leaves out Intro, FinalIntro and Results, which cut it.
        /// </summary>
        private static void Measure(GameRecord r)
        {
            // The poll comes after the game ended (gameEnded at the end of Results): not game time.
            var game = r.Phases.Where(p => p.Phase != GamePhase.Lobby && p.Phase != GamePhase.Poll).ToList();
            r.DurationMs = r.Ended.DurationMs;
            long stretch = 0;
            foreach (var p in game)
            {
                long d = p.EndMs - p.StartMs;
                if (InputPhases.Contains(p.Phase))
                {
                    stretch = 0;
                    continue;
                }

                r.IdleMs += d;
                if (LeftOutOfIdle.Contains(p.Phase))
                {
                    stretch = 0;
                    continue;
                }

                stretch += d;
                r.LongestIdleMs = Math.Max(r.LongestIdleMs, stretch);
            }
        }

        // ---------------------------------------------------------------- checks

        private static List<GamePhase> ExpectedOrder(LobbyFixture f, GameRecord r, bool intro)
        {
            var expected = new List<GamePhase>();
            if (intro) expected.Add(GamePhase.Intro);
            for (int round = 0; round < f.Config.Rounds.Count; round++)
            {
                expected.Add(GamePhase.TopicVote);
                for (int q = 0; q < f.Config.Rounds.QuestionsPerRound; q++)
                    expected.AddRange(new[] { GamePhase.QuestionShow, GamePhase.Answer, GamePhase.Reveal, GamePhase.Scores });
                expected.Add(GamePhase.RoundEnd);
            }

            expected.Add(GamePhase.FinalIntro);
            for (int s = 0; s < r.Statements.Count; s++) expected.AddRange(new[] { GamePhase.RaceQuestion, GamePhase.RaceReveal });
            expected.Add(GamePhase.Results);
            if (f.Config.Poll.Enabled) expected.Add(GamePhase.Poll);
            expected.Add(GamePhase.Lobby);
            return expected;
        }

        private static void CheckGame(LobbyFixture f, GameRecord r, bool intro)
        {
            Assert.That(r.Phases.Select(p => p.Phase), Is.EqualTo(ExpectedOrder(f, r, intro)), "game " + r.Game + " phase order");

            for (int i = 1; i < r.Phases.Count; i++)
            {
                var p = r.Phases[i];
                long expectedId = p.Phase == GamePhase.Scores ? r.Phases[i - 1].PhaseId : r.Phases[i - 1].PhaseId + 1;
                Assert.That(p.PhaseId, Is.EqualTo(expectedId), "phaseId of " + p.Phase);
            }

            foreach (var p in r.Phases.Where(x => x.EndMs >= 0))
            {
                long duration = p.EndMs - p.StartMs;
                if (p.Phase == GamePhase.Answer || p.Phase == GamePhase.RaceQuestion)
                {
                    Assert.That(p.EndMs, Is.LessThanOrEqualTo(p.StartMs + p.BudgetMs * 2 + f.GraceMs), "never past the longest window");
                    if (p.Reason == PhaseEndReasons.AllDone)
                        Assert.That(p.EndMs - r.LastLockMs[p.PhaseId], Is.InRange(0, MaxEndAfterLastLockMs));
                    else
                        Assert.That(p.EndMs, Is.EqualTo(p.ScheduledEndMs).Or.LessThan(p.ScheduledEndMs), "a timeout ends on time");
                }
                else if (p.Phase == GamePhase.TopicVote)
                {
                    Assert.That(duration, Is.LessThanOrEqualTo(f.VoteEndMs));
                    if (p.Reason == PhaseEndReasons.Timeout) Assert.That(duration, Is.EqualTo(f.VoteEndMs));
                }
                else if (p.Phase == GamePhase.Poll)
                {
                    // Ends when every connected phone submitted, or after poll.maxSeconds.
                    Assert.That(duration, Is.LessThanOrEqualTo(f.PollMs));
                    if (p.Reason == PhaseEndReasons.Timeout) Assert.That(duration, Is.EqualTo(f.PollMs));
                }
                else
                {
                    Assert.That(duration, Is.EqualTo(p.BudgetMs), p.Phase + " lasts its budget exactly");
                }
            }

            Assert.That(r.Questions.Distinct().Count(), Is.EqualTo(r.Questions.Count), "no question twice in a game");
            Assert.That(r.Statements.Distinct().Count(), Is.EqualTo(r.Statements.Count), "no statement twice in a race");
            Assert.That(r.Race, Is.Not.Null);
            Assert.That(r.Race.LastPlaceCanWin, Is.True, "lastPlaceCanWin is true in every run");
            Assert.That(r.Race.HeadStarts, Is.EquivalentTo(r.ExpectedHeadStarts), "head starts follow the formula");
            Assert.That(r.LongestIdleMs, Is.LessThanOrEqualTo(IdleLimitMs), "no stretch without input over the idle limit");
            Assert.That(r.DurationMs, Is.InRange(MinGameMs, MaxGameMs), "game " + r.Game + " length");
        }

        [Test]
        public void HundredStandardGamesWithTheSixProfiles()
        {
            var all = new List<GameRecord>();
            for (int session = 1; session <= Sessions; session++)
            {
                // A fresh session: an empty seen list.
                var f = new LobbyFixture(session * 7919);
                var sims = SimulatedPlayer.SixProfiles(session);
                foreach (var s in sims) f.Seat(s.Conn, s.Name);

                var first = PlayGame(f, sims);
                CheckGame(f, first, intro: true);
                var second = PlayGame(f, sims);
                CheckGame(f, second, intro: false);

                Assert.That(second.Questions.Intersect(first.Questions), Is.Empty, "session " + session + ": no question repeated within 2 games");
                Assert.That(second.Statements.Intersect(first.Statements), Is.Empty, "session " + session + ": no statement repeated within 2 games");
                all.Add(first);
                all.Add(second);
            }

            Assert.That(all.Count, Is.EqualTo(2 * Sessions));
            // Power ups run in the simulation: drawn, used, landed and cleared; every phone answers the poll.
            Assert.That(all.Sum(g => g.Drawn), Is.GreaterThan(0));
            Assert.That(all.Sum(g => g.Used), Is.GreaterThan(0));
            foreach (var g in all)
            {
                Assert.That(g.Used, Is.LessThanOrEqualTo(g.Drawn), "a power up is used at most once");
                Assert.That(g.Applied, Is.EqualTo(g.Used), "every used power up lands on the next question");
                Assert.That(g.Cleared, Is.EqualTo(g.Applied), "every landed effect is cleared");
                Assert.That(g.Polls, Is.EqualTo(6), "every phone answers the poll");
            }

            Print("Six profiles, " + all.Count + " Standard games", all);
        }

        [TestCase(SimulatedPlayer.Style.FullWindow)]
        [TestCase(SimulatedPlayer.Style.Never)]
        public void UsingTheFullWindowNeverGoesOverTwelveMinutes(SimulatedPlayer.Style style)
        {
            var all = new List<GameRecord>();
            for (int session = 1; session <= 5; session++)
            {
                var f = new LobbyFixture(session);
                var sims = SimulatedPlayer.SixProfiles(session, style);
                foreach (var s in sims) f.Seat(s.Conn, s.Name);
                var first = PlayGame(f, sims);
                CheckGame(f, first, intro: true);
                all.Add(first);
                Assert.That(first.DurationMs, Is.LessThanOrEqualTo(MaxGameMs));
            }

            Print(style + ": every player uses the full window", all);
        }

        [Test]
        public void EveryoneLockingInEndsTheQuestionAtTheLastLockIn()
        {
            // Six players who always answer within 2 s: every Answer must end allDone right at the last lock in.
            var f = new LobbyFixture(5);
            var sims = Enumerable.Range(1, 6)
                .Select(i => new SimulatedPlayer("fast", "c" + i, "P" + i, 500, 2000, 0.5, 10 + i)).ToArray();
            foreach (var s in sims) f.Seat(s.Conn, s.Name);
            var r = PlayGame(f, sims);
            var answers = r.Phases.Where(p => p.Phase == GamePhase.Answer || p.Phase == GamePhase.RaceQuestion).ToList();
            foreach (var a in answers)
            {
                Assert.That(a.Reason, Is.EqualTo(PhaseEndReasons.AllDone));
                Assert.That(a.EndMs - r.LastLockMs[a.PhaseId], Is.InRange(0, MaxEndAfterLastLockMs));
            }

            Assert.That(r.Phases.Where(p => p.Phase == GamePhase.TopicVote).All(p => p.Reason == PhaseEndReasons.AllDone), Is.True);
            Print("Six fast players", new List<GameRecord> { r });
        }

        // ---------------------------------------------------------------- the summary table

        private static void Print(string title, List<GameRecord> games)
        {
            TestContext.WriteLine(title);
            TestContext.WriteLine("phase          count   budgetMs(avg)  durationMs min / avg / max");
            foreach (var phase in Enum.GetValues(typeof(GamePhase)).Cast<GamePhase>())
            {
                var list = games.SelectMany(g => g.Phases).Where(p => p.Phase == phase && p.EndMs >= 0).ToList();
                if (list.Count == 0) continue;
                var d = list.Select(p => p.EndMs - p.StartMs).ToList();
                TestContext.WriteLine(string.Format("{0,-13}  {1,5}  {2,13:0}  {3,10} / {4,7:0} / {5,6}", phase, list.Count,
                    list.Average(p => p.BudgetMs), d.Min(), d.Average(), d.Max()));
            }

            var lengths = games.Select(g => g.DurationMs / 1000.0).ToList();
            var shares = games.Select(g => g.IdleShare * 100).ToList();
            var idle = games.Select(g => g.LongestIdleMs).ToList();
            TestContext.WriteLine(string.Format("game length s         min {0:0.0}  avg {1:0.0}  max {2:0.0}", lengths.Min(), lengths.Average(), lengths.Max()));
            TestContext.WriteLine(string.Format("idle share % (report) min {0:0.0}  avg {1:0.0}  max {2:0.0}", shares.Min(), shares.Average(), shares.Max()));
            TestContext.WriteLine(string.Format("longest idle ms       min {0}  max {1} (limit {2}, outside Intro, FinalIntro, Results)",
                idle.Min(), idle.Max(), IdleLimitMs));
            TestContext.WriteLine(string.Format("power ups per game    drawn {0:0.0}  used {1:0.0}  landed {2:0.0}  cleared {3:0.0}",
                games.Average(g => g.Drawn), games.Average(g => g.Used), games.Average(g => g.Applied), games.Average(g => g.Cleared)));
            var races = games.Select(g => g.Statements.Count).ToList();
            TestContext.WriteLine(string.Format("race statements       min {0}  avg {1:0.0}  max {2}", races.Min(), races.Average(), races.Max()));
        }
    }
}
