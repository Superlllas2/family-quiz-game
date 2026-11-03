using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>The final race (docs/02_show_flow.md, "The final race"; docs/03_rules.md; docs/18_edge_cases.md).</summary>
    public sealed class RaceTests
    {
        private static readonly RaceConfig Race = TestContent.Config().Race;

        // ---------------------------------------------------------------- head starts

        [TestCase(2, 1, 3)]
        [TestCase(2, 2, 0)]
        [TestCase(3, 1, 3)]
        [TestCase(3, 2, 1)]
        [TestCase(3, 3, 0)]
        [TestCase(4, 2, 2)]
        [TestCase(4, 3, 1)]
        [TestCase(6, 2, 2)]
        [TestCase(6, 5, 0)]
        [TestCase(6, 6, 0)]
        [TestCase(8, 4, 1)]
        public void HeadStartFollowsTheFormula(int players, int rank, int expected)
        {
            Assert.That(RaceRules.HeadStart(rank, players, Race.MaxHeadStart), Is.EqualTo(expected));
        }

        [Test]
        public void TiedPlayersShareTheHigherHeadStartOfTheirGroup()
        {
            // Scores 900, 900, 400 give ranks 1, 1, 3: head starts 3, 3, 0.
            Assert.That(new[] { 1, 1, 3 }.Select(r => RaceRules.HeadStart(r, 3, Race.MaxHeadStart)), Is.EqualTo(new[] { 3, 3, 0 }));
            // 500, 400, 400, 0: ranks 1, 2, 2, 4: 3, 2, 2, 0.
            Assert.That(new[] { 1, 2, 2, 4 }.Select(r => RaceRules.HeadStart(r, 4, Race.MaxHeadStart)), Is.EqualTo(new[] { 3, 2, 2, 0 }));
        }

        [Test]
        public void APlayerAloneStartsOnTheLeadersStep()
        {
            Assert.That(RaceRules.HeadStart(1, 1, Race.MaxHeadStart), Is.EqualTo(Race.MaxHeadStart));
        }

        [Test]
        public void LastPlaceCanWinChecksBothConditions()
        {
            Assert.That(RaceRules.LastPlaceCanWin(new[] { 3, 1, 0 }, Race), Is.True);
            var far = new RaceConfig { MaxHeadStart = 3, FinishSteps = 13, MaxQuestions = 12, StepsPerCorrect = 1 };
            Assert.That(RaceRules.LastPlaceCanWin(new[] { 3, 0 }, far), Is.False, "finishSteps - 0 > maxQuestions");
            Assert.That(RaceRules.LastPlaceCanWin(new[] { 3, 3 }, far), Is.True, "13 - 3 <= 12");
            var gap = new RaceConfig { MaxHeadStart = 12, FinishSteps = 12, MaxQuestions = 12, StepsPerCorrect = 1 };
            Assert.That(RaceRules.LastPlaceCanWin(new[] { 12, 0 }, gap), Is.False, "a gap of maxQuestions");
        }

        // ---------------------------------------------------------------- order and winner

        private static List<string> Order(Dictionary<string, int> positions, Dictionary<string, int> scores,
            Dictionary<string, long> ms, int seed = 1)
        {
            return RaceRules.Order(positions.Keys.ToList(), positions, scores, ms, new SeededRandom(seed));
        }

        [Test]
        public void TheFurthestPlayerWins()
        {
            var order = Order(new Dictionary<string, int> { { "a", 7 }, { "b", 10 }, { "c", 9 } },
                new Dictionary<string, int> { { "a", 5000 }, { "b", 0 }, { "c", 100 } },
                new Dictionary<string, long> { { "a", 1 }, { "b", 99999 }, { "c", 2 } });
            Assert.That(order, Is.EqualTo(new[] { "b", "c", "a" }));
        }

        [Test]
        public void ATieGoesToTheHigherScoreBeforeTheRaceThenTheFasterRace()
        {
            var order = Order(new Dictionary<string, int> { { "a", 10 }, { "b", 10 }, { "c", 10 } },
                new Dictionary<string, int> { { "a", 900 }, { "b", 1200 }, { "c", 900 } },
                new Dictionary<string, long> { { "a", 30000 }, { "b", 50000 }, { "c", 20000 } });
            Assert.That(order, Is.EqualTo(new[] { "b", "c", "a" }));
        }

        [Test]
        public void AFullTieIsPickedBySeed()
        {
            var winners = new HashSet<string>();
            for (int seed = 0; seed < 30; seed++)
            {
                var positions = new Dictionary<string, int> { { "a", 10 }, { "b", 10 } };
                var scores = new Dictionary<string, int> { { "a", 500 }, { "b", 500 } };
                var ms = new Dictionary<string, long> { { "a", 9000 }, { "b", 9000 } };
                var order = Order(positions, scores, ms, seed);
                Assert.That(Order(positions, scores, ms, seed), Is.EqualTo(order), "the same seed, the same winner");
                winners.Add(order[0]);
            }

            Assert.That(winners.Count, Is.EqualTo(2));
        }

        [Test]
        public void TheRaceEndsAtTheFinishOrAfterMaxQuestions()
        {
            Assert.That(RaceRules.IsFinished(new[] { 9, 3 }, 6, Race), Is.False);
            Assert.That(RaceRules.IsFinished(new[] { Race.FinishSteps, 3 }, 6, Race), Is.True);
            Assert.That(RaceRules.IsFinished(new[] { 4, 3 }, Race.MaxQuestions, Race), Is.True);
            Assert.That(RaceRules.Move(Race.FinishSteps - 1, true, Race), Is.EqualTo(Race.FinishSteps));
            Assert.That(RaceRules.Move(Race.FinishSteps, true, Race), Is.EqualTo(Race.FinishSteps), "never past the finish");
            Assert.That(RaceRules.Move(4, false, Race), Is.EqualTo(4));
        }

        // ---------------------------------------------------------------- statements

        private static List<RaceStatementDef> Statements(int n)
        {
            return Enumerable.Range(1, n).Select(i => new RaceStatementDef { Id = "r" + i, Answer = i % 2 == 0 }).ToList();
        }

        [Test]
        public void UnseenStatementsComeFirst()
        {
            var all = Statements(6);
            var seen = new HashSet<string> { "r1", "r2", "r3", "r4" };
            var used = new HashSet<string>();
            var picked = new List<string>();
            for (int i = 0; i < 2; i++)
            {
                var s = RaceRules.PickStatement(all, seen, used, new SeededRandom(i));
                used.Add(s.Id);
                picked.Add(s.Id);
            }

            Assert.That(picked, Is.EquivalentTo(new[] { "r5", "r6" }));
        }

        [Test]
        public void WhenUnseenRunOutSeenOnesArePickedBySeedWithoutRepeats()
        {
            var all = Statements(6);
            var seen = new HashSet<string>(all.Select(s => s.Id));
            var used = new HashSet<string>();
            for (int i = 0; i < 6; i++)
            {
                var s = RaceRules.PickStatement(all, seen, used, new SeededRandom(i));
                Assert.That(used.Add(s.Id), Is.True);
            }

            Assert.That(RaceRules.PickStatement(all, seen, used, new SeededRandom(1)), Is.Null);
        }

        // ---------------------------------------------------------------- in a game

        /// <summary>
        /// count players; in round 1 question 1 the first rightAnswers players answer right (the first fastest), then
        /// nobody answers: runs to FinalIntro.
        /// </summary>
        private static (LobbyFixture, PlayerState[]) ToFinalIntro(int count, int rightAnswers, int seed = 42,
            IEnumerable<string> seenRace = null)
        {
            var f = new LobbyFixture(seed, seenRace: seenRace);
            var seats = f.ToFirstVote(count);
            f.SkipTo(GamePhase.Answer);
            for (int i = 0; i < count; i++) f.Choice(seats[i].Conn, i < rightAnswers ? f.Right : f.Wrong, 1000 * (i + 1));
            f.SkipTo(GamePhase.FinalIntro);
            return (f, seats);
        }

        [Test]
        public void TheRaceStartsWithHeadStartsByRankAndLogsLastPlaceCanWin()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstVote(3);
            f.SkipTo(GamePhase.Answer);
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Right, f.WindowMs);
            f.Choice("c3", f.Wrong, 0);
            var events = f.SkipTo(GamePhase.FinalIntro);
            var started = events.OfType<RaceStarted>().Single();
            Assert.That(started.HeadStarts[seats[0].PlayerId], Is.EqualTo(3));
            Assert.That(started.HeadStarts[seats[1].PlayerId], Is.EqualTo(1));
            Assert.That(started.HeadStarts[seats[2].PlayerId], Is.EqualTo(0));
            Assert.That(started.LastPlaceCanWin, Is.True);
            Assert.That(f.State.Race.Positions[seats[0].PlayerId], Is.EqualTo(3));
            Assert.That(f.State.PhaseEndsAtMs - f.State.PhaseStartedAtMs, Is.EqualTo(f.FinalIntroMs));
        }

        [Test]
        public void OnlyTwoPlayersLeaderThreeOtherZeroAndEqualScoresBothThree()
        {
            var (f, seats) = ToFinalIntro(2, 1);
            Assert.That(f.State.Race.HeadStarts.Values, Is.EqualTo(new[] { 3, 0 }));

            var (g, _) = ToFinalIntro(2, 0);
            Assert.That(g.State.Race.HeadStarts.Values, Is.EqualTo(new[] { 3, 3 }));
            Assert.That(g.State.Race.LastPlaceCanWin, Is.True);
        }

        [Test]
        public void TiedScoresAtTheRaceStartGetTheSameHeadStart()
        {
            // P1 and P2 right at the same ms (same points), P3 wrong: ranks 1, 1, 3.
            var f = new LobbyFixture();
            var seats = f.ToFirstVote(3);
            f.SkipTo(GamePhase.Answer);
            f.Choice("c1", f.Right, 500);
            f.Choice("c2", f.Right, 500);
            f.Choice("c3", f.Wrong, 0);
            f.SkipTo(GamePhase.FinalIntro);
            Assert.That(seats.Select(p => f.State.Race.HeadStarts[p.PlayerId]), Is.EqualTo(new[] { 3, 3, 0 }));
        }

        [Test]
        public void RaceAnswersUseTheRaceWindowTimesTheTimerMultiplierWithoutLastCall()
        {
            var (f, seats) = ToFinalIntro(2, 0);
            f.Send(new ChangeSettings("c2", null, f.Config.Players.TimerMultiplierOptions.Max()));
            f.Advance(f.FinalIntroMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.RaceQuestion));
            Assert.That(seats[0].Turn.WindowMs, Is.EqualTo(f.RaceWindowMs));
            Assert.That(seats[1].Turn.WindowMs, Is.EqualTo(f.RaceWindowMs * 2));
            Assert.That(f.State.PhaseEndsAtMs, Is.EqualTo(f.State.PhaseStartedAtMs + 2 * f.RaceWindowMs + f.GraceMs));

            var events = f.TrueFalse("c1", f.State.Race.Statement.Answer, 700);
            Assert.That(events.OfType<LastCallStarted>(), Is.Empty, "no last call in the race");
            Assert.That(seats[1].Turn.DeadlineMs, Is.EqualTo(f.State.PhaseStartedAtMs + 2 * f.RaceWindowMs));
            Assert.That(f.Game.EndsInMs(seats[1]), Is.EqualTo(2 * f.RaceWindowMs));
        }

        [Test]
        public void ARightAnswerMovesStepsPerCorrectAndAWrongOrNoAnswerDoesNot()
        {
            var (f, seats) = ToFinalIntro(3, 0);
            f.Advance(f.FinalIntroMs);
            int start = f.State.Race.Positions[seats[0].PlayerId];
            bool right = f.State.Race.Statement.Answer;
            f.TrueFalse("c1", right, 900);
            f.TrueFalse("c2", !right, 900);
            var events = f.SkipTo(GamePhase.RaceReveal);
            var revealed = events.OfType<RaceRevealed>().Single();
            Assert.That(revealed.Answer, Is.EqualTo(right));
            Assert.That(f.State.Race.Positions[seats[0].PlayerId], Is.EqualTo(start + f.Config.Race.StepsPerCorrect));
            Assert.That(f.State.Race.Positions[seats[1].PlayerId], Is.EqualTo(start));
            Assert.That(f.State.Race.Positions[seats[2].PlayerId], Is.EqualTo(start));
            Assert.That(seats[0].Turn.Correct, Is.True);
            Assert.That(seats.All(p => p.Score == 0), Is.True, "the race gives no points");
            Assert.That(f.State.PhaseEndsAtMs - f.State.PhaseStartedAtMs, Is.EqualTo(f.RaceRevealMs));
        }

        [Test]
        public void TheRaceEndsWhenAPlayerReachesTheFinishAndTheyWin()
        {
            var (f, seats) = ToFinalIntro(2, 1);
            f.Advance(f.FinalIntroMs);
            int needed = (f.Config.Race.FinishSteps - f.State.Race.HeadStarts[seats[0].PlayerId]) / f.Config.Race.StepsPerCorrect;
            for (int i = 0; i < needed; i++)
            {
                Assert.That(f.State.Phase, Is.EqualTo(GamePhase.RaceQuestion));
                bool right = f.State.Race.Statement.Answer;
                f.TrueFalse("c1", right, 900);
                f.TrueFalse("c2", right, 800);
                Assert.That(f.State.Phase, Is.EqualTo(GamePhase.RaceReveal), "everyone answered: the answer part ends at once");
                f.Advance(f.RaceRevealMs);
            }

            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Results));
            Assert.That(f.State.Race.StatementNumber, Is.EqualTo(needed));
            Assert.That(f.State.Race.Finished, Is.True);
            Assert.That(f.State.WinnerId, Is.EqualTo(seats[0].PlayerId));
            Assert.That(f.State.Placement, Is.EqualTo(new[] { seats[0].PlayerId, seats[1].PlayerId }));
        }

        [Test]
        public void LastPlaceCanWinByAnsweringRightEveryTime()
        {
            var (f, seats) = ToFinalIntro(2, 1);
            f.Advance(f.FinalIntroMs);
            Assert.That(f.State.Race.HeadStarts[seats[1].PlayerId], Is.EqualTo(0));
            while (f.State.Phase == GamePhase.RaceQuestion)
            {
                bool right = f.State.Race.Statement.Answer;
                f.TrueFalse("c1", !right, 900);
                f.TrueFalse("c2", right, 900);
                f.Advance(f.RaceRevealMs);
            }

            Assert.That(f.State.WinnerId, Is.EqualTo(seats[1].PlayerId));
            Assert.That(f.State.Race.StatementNumber, Is.EqualTo(f.Config.Race.FinishSteps), "10 right answers from step 0");
        }

        [Test]
        public void ATieAtTheEndGoesToTheFasterRaceWhenScoresAreEqual()
        {
            var (f, seats) = ToFinalIntro(2, 0);
            f.Advance(f.FinalIntroMs);
            while (f.State.Phase == GamePhase.RaceQuestion)
            {
                bool right = f.State.Race.Statement.Answer;
                f.TrueFalse("c1", right, 2000);
                f.TrueFalse("c2", right, 1500);
                f.Advance(f.RaceRevealMs);
            }

            Assert.That(f.State.Race.Positions.Values.Distinct().Single(), Is.EqualTo(f.Config.Race.FinishSteps), "they cross together");
            Assert.That(f.State.WinnerId, Is.EqualTo(seats[1].PlayerId));
            Assert.That(f.State.Race.AnswerMs[seats[1].PlayerId], Is.LessThan(f.State.Race.AnswerMs[seats[0].PlayerId]));
        }

        [Test]
        public void ARaceWithoutAWinnerOnTheTrackEndsAfterMaxQuestions()
        {
            var (f, seats) = ToFinalIntro(2, 1);
            var events = f.SkipTo(GamePhase.Results);
            Assert.That(events.OfType<PhaseStarted>().Count(p => p.Phase == GamePhase.RaceQuestion), Is.EqualTo(f.Config.Race.MaxQuestions));
            Assert.That(f.State.WinnerId, Is.EqualTo(seats[0].PlayerId), "nobody moved: the head start leader wins");
        }

        [Test]
        public void RaceInputsAreCheckedAndALateOneIsClosed()
        {
            var (f, _) = ToFinalIntro(2, 0);
            Assert.That(LobbyFixture.AckOf(f.TrueFalse("c1", true, 100)).Reason, Is.EqualTo(AckReasons.Invalid), "FinalIntro takes no input");
            f.Advance(f.FinalIntroMs);
            long raceId = f.State.PhaseId;
            Assert.That(LobbyFixture.AckOf(f.Choice("c1", 0, 100)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(LobbyFixture.AckOf(f.TrueFalse("c1", true, null)).Reason, Is.EqualTo(AckReasons.Invalid), "ms is needed");
            var noAnswer = f.Send(new SubmitAnswer("c1", "x", raceId, InputKinds.TrueFalse, null, 100));
            Assert.That(LobbyFixture.AckOf(noAnswer).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(LobbyFixture.AckOf(f.TrueFalse("c1", true, 100)).Ok, Is.True);
            Assert.That(LobbyFixture.AckOf(f.TrueFalse("c1", false, 100)).Reason, Is.EqualTo(AckReasons.Invalid), "one answer");
            f.Advance(f.RaceWindowMs + f.GraceMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.RaceReveal));
            var late = f.Send(new SubmitAnswer("c2", "late", raceId, InputKinds.TrueFalse, null, 100, answer: true));
            Assert.That(LobbyFixture.AckOf(late).Reason, Is.EqualTo(AckReasons.Closed));
        }

        [Test]
        public void EveryRaceStatementIsShownOnceAndMarkedSeen()
        {
            var (f, _) = ToFinalIntro(2, 0);
            var events = f.SkipTo(GamePhase.Results);
            Assert.That(f.State.SeenRaceIds.Count, Is.EqualTo(f.Config.Race.MaxQuestions));
            // One SeenChanged per statement, and one for the winner's fact when Results starts (D87).
            Assert.That(events.OfType<SeenChanged>().Count(), Is.EqualTo(f.Config.Race.MaxQuestions + 1));
        }

        [Test]
        public void WhenUnseenStatementsRunOutTheRaceUsesSeenOnes()
        {
            var all = TestContent.Content().Race.Select(s => s.Id).ToList();
            var (f, _) = ToFinalIntro(2, 0, seenRace: all);
            var ids = new List<string>();
            f.Advance(f.FinalIntroMs);
            while (f.State.Phase != GamePhase.Results)
            {
                if (f.State.Phase == GamePhase.RaceQuestion && (ids.Count == 0 || ids.Last() != f.State.Race.Statement.Id))
                    ids.Add(f.State.Race.Statement.Id);
                f.Advance(50);
            }

            Assert.That(ids.Count, Is.EqualTo(f.Config.Race.MaxQuestions));
            Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Count), "never twice in one race");
        }

        [Test]
        public void UnseenStatementsArePreferredInAGame()
        {
            var all = TestContent.Content().Race.Select(s => s.Id).ToList();
            var seen = all.Take(all.Count - f12()).ToList();
            var (f, _) = ToFinalIntro(2, 0, seenRace: seen);
            f.SkipTo(GamePhase.Results);
            var shown = f.State.SeenRaceIds.Except(seen).ToList();
            Assert.That(shown, Is.EquivalentTo(all.Skip(all.Count - f12())));

            int f12() => TestContent.Config().Race.MaxQuestions;
        }
    }
}
