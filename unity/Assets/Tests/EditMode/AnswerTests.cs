using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// The Answer phase: personal deadlines, the early end (D73), last call (D74), ms clamping, stale and closed inputs
    /// (docs/03_rules.md "Timer multiplier and personal deadlines", PROTOCOL.md section 6, docs/18_edge_cases.md).
    /// </summary>
    public sealed class AnswerTests
    {
        private static List<string> Names(IEnumerable<IGameEvent> events)
        {
            return events.Select(LobbyFixture.Describe).ToList();
        }

        [Test]
        public void EveryPlayerGetsAPersonalWindowDeadlineAndRank()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            long start = f.State.PhaseStartedAtMs;
            foreach (var p in seats)
            {
                Assert.That(p.Turn.WindowMs, Is.EqualTo(f.WindowMs));
                Assert.That(p.Turn.DeadlineMs, Is.EqualTo(start + f.WindowMs));
                Assert.That(p.Turn.RankBefore, Is.EqualTo(1));
                Assert.That(p.Turn.Locked, Is.False);
            }

            Assert.That(f.State.Question.AnswerPhaseId, Is.EqualTo(f.State.PhaseId));
            Assert.That(f.State.Question.BaseWindowMs, Is.EqualTo(f.WindowMs));
            Assert.That(f.State.PhaseEndsAtMs, Is.EqualTo(start + f.WindowMs + f.GraceMs));
            Assert.That(f.Game.EndsInMs(seats[0]), Is.EqualTo(f.WindowMs));
            Assert.That(f.Game.AnyoneCanAnswer, Is.True);
        }

        [Test]
        public void TheAnswerEndsOnTheSameCallAsTheLastLockIn()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(3);
            long answerId = f.State.PhaseId;
            f.Advance(1200);
            f.Choice("c1", f.Right, 1200);
            f.Choice("c2", f.Wrong, 1200);
            var events = f.Choice("c3", f.Right, 1200);

            Assert.That(Names(events), Is.EqualTo(new[]
            {
                "InputAcked(True)", "AnswerLocked", "PhaseEnded(Answer, allDone)", "PhaseStarted(Reveal)", "Revealed",
                "ScoreChanged", "ScoreChanged", "PlayerUpdated", "PlayerUpdated", "PlayerUpdated"
            }));
            var ended = events.OfType<PhaseEnded>().Single();
            Assert.That(ended.PhaseId, Is.EqualTo(answerId));
            Assert.That(ended.DurationMs, Is.EqualTo(1200));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal));
            Assert.That(f.State.PhaseStartedAtMs, Is.EqualTo(f.Clock.NowMs));
        }

        [Test]
        public void RevealScoresEveryTurnAndUpdatesStreakAndCorrectCount()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Wrong, 0);
            var events = f.Choice("c3", f.Right, f.WindowMs);

            Assert.That(seats[0].Score, Is.EqualTo(1000));
            Assert.That(seats[1].Score, Is.EqualTo(0));
            Assert.That(seats[2].Score, Is.EqualTo(500));
            Assert.That(seats.Select(p => p.Streak), Is.EqualTo(new[] { 1, 0, 1 }));
            Assert.That(seats.Select(p => p.CorrectCount), Is.EqualTo(new[] { 1, 0, 1 }));
            Assert.That(seats.Select(p => p.Turn.Correct), Is.EqualTo(new[] { true, false, true }));
            Assert.That(seats.Select(p => p.Turn.Points), Is.EqualTo(new[] { 1000, 0, 500 }));
            Assert.That(seats.All(p => p.Turn.Revealed), Is.True);

            var changes = events.OfType<ScoreChanged>().ToList();
            Assert.That(changes.Select(c => c.PlayerId), Is.EqualTo(new[] { seats[0].PlayerId, seats[2].PlayerId }));
            Assert.That(changes[0].Rank, Is.EqualTo(1));
            Assert.That(changes[1].Rank, Is.EqualTo(2));
            var revealed = events.OfType<Revealed>().Single();
            Assert.That(revealed.CorrectDisplayIndex, Is.EqualTo(f.State.Question.CorrectDisplayIndex));
            Assert.That(revealed.QuestionId, Is.EqualTo(f.State.Question.Def.Id));
        }

        [Test]
        public void TheAnswerPointsMatchTheReveal()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            var a = f.Choice("c1", f.Right, 3410).OfType<AnswerLocked>().Single();
            var b = f.Choice("c2", f.Right, 7777).OfType<AnswerLocked>().Single();
            Assert.That(a.Correct, Is.True);
            Assert.That(seats[0].Score, Is.EqualTo(a.Points));
            Assert.That(seats[1].Score, Is.EqualTo(b.Points));
        }

        [Test]
        public void NobodyAnsweringEndsAtTheLatestDeadlinePlusGraceWithTimeout()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            long start = f.State.PhaseStartedAtMs;
            var events = f.RunUntilPhase(GamePhase.Reveal, stepMs: 7);
            var ended = events.OfType<PhaseEnded>().Single(e => e.Phase == GamePhase.Answer);
            Assert.That(ended.Reason, Is.EqualTo(PhaseEndReasons.Timeout));
            Assert.That(ended.DurationMs, Is.EqualTo(f.WindowMs + f.GraceMs));
            Assert.That(f.State.PhaseStartedAtMs, Is.EqualTo(start + f.WindowMs + f.GraceMs), "Reveal starts at the scheduled end");
            Assert.That(seats.All(p => p.Score == 0 && p.Streak == 0), Is.True);
        }

        [Test]
        public void APlayerWhoReachesDeadlinePlusGraceTimesOutOnceBeforeTheEnd()
        {
            var (f, seats) = SlowSecondPlayer();
            long answerId = f.State.PhaseId;

            Assert.That(f.Advance(f.WindowMs + f.GraceMs - 1).OfType<PlayerTimedOut>(), Is.Empty);
            var events = f.Advance(1);
            var timedOut = events.OfType<PlayerTimedOut>().Single();
            Assert.That(timedOut.PlayerId, Is.EqualTo(seats[0].PlayerId));
            Assert.That(timedOut.PhaseId, Is.EqualTo(answerId));
            Assert.That(seats[0].Turn.TimedOut, Is.True);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer));
            Assert.That(f.Advance(10).OfType<PlayerTimedOut>(), Is.Empty, "once per player");
            Assert.That(seats[1].Turn.TimedOut, Is.False);
        }

        [Test]
        public void ACommandAlsoRaisesATimeoutThatIsDueWithoutATick()
        {
            var (f, seats) = SlowSecondPlayer();
            f.Clock.Advance(f.WindowMs + f.GraceMs); // no Tick: the next command finds c1 done
            var events = f.Choice("c2", f.Right, 100);
            Assert.That(Names(events).Take(3), Is.EqualTo(new[] { "PlayerTimedOut", "InputAcked(True)", "AnswerLocked" }));
            Assert.That(events.OfType<PlayerTimedOut>().Single().PlayerId, Is.EqualTo(seats[0].PlayerId));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal));
        }

        [Test]
        public void ADisconnectedPlayerTimesOutToo()
        {
            var (f, seats) = SlowSecondPlayer();
            f.Send(new ConnectionClosed("c1"));
            var events = f.Advance(f.WindowMs + f.GraceMs);
            Assert.That(events.OfType<PlayerTimedOut>().Single().PlayerId, Is.EqualTo(seats[0].PlayerId));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer), "the connected c2 still has time");
        }

        [Test]
        public void ATimerMultiplierChangedDuringAnswerAppliesFromTheNextAnswer()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            double most = f.Config.Players.TimerMultiplierOptions.Max();
            f.Send(new ChangeSettings("c2", null, most));
            Assert.That(seats[1].Turn.WindowMs, Is.EqualTo(f.WindowMs));
            Assert.That(seats[1].Turn.DeadlineMs, Is.EqualTo(f.State.PhaseStartedAtMs + f.WindowMs));
            f.RunUntilPhase(GamePhase.QuestionShow);
            f.RunUntilPhase(GamePhase.Answer);
            Assert.That(seats[1].Turn.WindowMs, Is.EqualTo(LobbyFixture.Ms(f.Config.Timers.Answer.MultipleChoice * most)));
        }

        [Test]
        public void TimeoutsComeBeforeTheAnswerEndInTheSameBatch()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            var events = f.Advance(f.WindowMs + f.GraceMs + 5000);
            var names = Names(events);
            Assert.That(names.Take(3), Is.EqualTo(new[] { "PlayerTimedOut", "PlayerTimedOut", "PhaseEnded(Answer, timeout)" }));
            Assert.That(events.OfType<PlayerTimedOut>().Select(e => e.PlayerId),
                Is.EqualTo(new[] { seats[0].PlayerId, seats[1].PlayerId }));
        }

        [Test]
        public void ALockedPlayerNeverTimesOut()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            f.Choice("c1", f.Right, 100);
            var events = f.RunUntilPhase(GamePhase.Reveal);
            Assert.That(events.OfType<PlayerTimedOut>().Select(e => e.PlayerId), Is.EqualTo(new[] { seats[1].PlayerId }));
        }

        [Test]
        public void APlayerPastTheDeadlineIsNotDoneUntilTheGraceIsOver()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            f.Choice("c1", f.Right, 100);
            // Last call cut c2 to now + lastCall; wait until just after that deadline, still inside the grace.
            long deadline = seats[1].Turn.DeadlineMs;
            f.Advance(deadline - f.Clock.NowMs + 1);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer));
            Assert.That(f.Game.EndsInMs(seats[1]), Is.EqualTo(0));
            var ack = LobbyFixture.AckOf(f.Choice("c2", f.Right, 99999));
            Assert.That(ack.Ok, Is.True, "an input before deadline + grace is accepted");
            Assert.That(seats[1].Turn.Ms, Is.EqualTo(seats[1].Turn.WindowMs), "ms clamped to the window");
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal));
        }

        /// <summary>Two players, c2 on the largest timer multiplier, so the Answer stays open after c1's deadline.</summary>
        private static (LobbyFixture, PlayerState[]) SlowSecondPlayer()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.Send(new ChangeSettings("c2", null, f.Config.Players.TimerMultiplierOptions.Max()));
            f.Lobby("c1", LobbyActions.Start);
            f.Advance(f.CountdownMs);
            f.Advance(f.QuestionShowMs);
            Assert.That(seats[1].Turn.WindowMs, Is.GreaterThan(seats[0].Turn.WindowMs));
            return (f, seats);
        }

        [Test]
        public void AnInputOneMsBeforeDeadlinePlusGraceIsAccepted()
        {
            var (f, seats) = SlowSecondPlayer();
            f.Advance(seats[0].Turn.DeadlineMs + f.GraceMs - 1 - f.Clock.NowMs);
            Assert.That(LobbyFixture.AckOf(f.Choice("c1", f.Right, 1)).Ok, Is.True);
        }

        [Test]
        public void AnInputExactlyAtDeadlinePlusGraceIsClosed()
        {
            var (f, seats) = SlowSecondPlayer();
            long answerId = f.State.PhaseId;
            f.Advance(seats[0].Turn.DeadlineMs + f.GraceMs - f.Clock.NowMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer), "c2 still has time");
            var events = f.Choice("c1", f.Right, 1);
            Assert.That(LobbyFixture.AckOf(events).Reason, Is.EqualTo(AckReasons.Closed));
            var refused = events.OfType<InputRefused>().Single();
            Assert.That(refused.Reason, Is.EqualTo(AckReasons.Closed));
            Assert.That(refused.PhaseId, Is.EqualTo(answerId));
            Assert.That(seats[0].Turn.Locked, Is.False);
            Assert.That(f.Game.AnyoneCanAnswer, Is.True, "c2 can still answer");
        }

        [Test]
        public void PersonalDeadlinesDifferAndThePhaseWaitsForTheLatest()
        {
            var (f, seats) = SlowSecondPlayer();
            long start = f.State.PhaseStartedAtMs;
            Assert.That(f.State.PhaseEndsAtMs, Is.EqualTo(seats[1].Turn.DeadlineMs + f.GraceMs));
            var events = f.RunUntilPhase(GamePhase.Reveal, stepMs: 9);
            Assert.That(events.OfType<PlayerTimedOut>().Select(e => e.PlayerId), Is.EqualTo(new[] { seats[0].PlayerId, seats[1].PlayerId }));
            Assert.That(f.State.PhaseStartedAtMs, Is.EqualTo(start + seats[1].Turn.WindowMs + f.GraceMs));
        }

        [Test]
        public void AnInputForTheClosedAnswerIsClosedAndForAnOlderPhaseIsStale()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            long answerId = f.State.PhaseId;
            long showId = answerId - 1;
            f.RunUntilPhase(GamePhase.Reveal);

            var late = f.Send(new SubmitAnswer("c1", "late", answerId, InputKinds.Choice, 0, 100));
            Assert.That(LobbyFixture.AckOf(late).Reason, Is.EqualTo(AckReasons.Closed));
            var old = f.Send(new SubmitAnswer("c1", "old", showId, InputKinds.Choice, 0, 100));
            Assert.That(LobbyFixture.AckOf(old).Reason, Is.EqualTo(AckReasons.Stale));
            Assert.That(old.OfType<InputRefused>().Single().Reason, Is.EqualTo(AckReasons.Stale));
            var future = f.Send(new SubmitAnswer("c1", "future", f.State.PhaseId + 5, InputKinds.Choice, 0, 100));
            Assert.That(LobbyFixture.AckOf(future).Reason, Is.EqualTo(AckReasons.Stale));

            f.RunUntilPhase(GamePhase.Scores);
            Assert.That(LobbyFixture.AckOf(f.Send(new SubmitAnswer("c1", "late2", answerId, InputKinds.Choice, 0, 100))).Reason,
                Is.EqualTo(AckReasons.Closed), "still closed during Scores");
            // The last closed Answer stays "closed" until the next Answer closes; then it is only stale.
            f.RunUntilPhase(GamePhase.Answer);
            Assert.That(LobbyFixture.AckOf(f.Send(new SubmitAnswer("c1", "late3", answerId, InputKinds.Choice, 0, 100))).Reason,
                Is.EqualTo(AckReasons.Closed));
            f.RunUntilPhase(GamePhase.Reveal);
            Assert.That(LobbyFixture.AckOf(f.Send(new SubmitAnswer("c1", "late4", answerId, InputKinds.Choice, 0, 100))).Reason,
                Is.EqualTo(AckReasons.Stale));
        }

        [TestCase(-500, 0)]
        [TestCase(-1, 0)]
        public void NegativeMsIsClampedToZero(long sent, long expected)
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            var locked = f.Choice("c1", f.Right, sent).OfType<AnswerLocked>().Single();
            Assert.That(locked.Ms, Is.EqualTo(expected));
            Assert.That(seats[0].Turn.RawMs, Is.EqualTo(sent));
            Assert.That(locked.Points, Is.EqualTo(1000));
        }

        [Test]
        public void MsOverTheWindowIsClampedToTheWindow()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            var locked = f.Choice("c1", f.Right, f.WindowMs * 3).OfType<AnswerLocked>().Single();
            Assert.That(locked.Ms, Is.EqualTo(f.WindowMs));
            Assert.That(seats[0].Turn.RawMs, Is.EqualTo(f.WindowMs * 3));
            Assert.That(locked.Points, Is.EqualTo(500));
        }

        // ---------------------------------------------------------------- last call (D74)

        [Test]
        public void LastCallCutsTheOneRemainingPlayerWithTheSamePhaseId()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            long answerId = f.State.PhaseId;
            f.Advance(1000);
            Assert.That(f.Choice("c1", f.Right, 1000).OfType<LastCallStarted>(), Is.Empty, "two still answering");
            var events = f.Choice("c2", f.Right, 1000);

            Assert.That(Names(events), Is.EqualTo(new[] { "InputAcked(True)", "AnswerLocked", "LastCallStarted" }));
            var call = events.OfType<LastCallStarted>().Single();
            Assert.That(call.PlayerId, Is.EqualTo(seats[2].PlayerId));
            Assert.That(call.PhaseId, Is.EqualTo(answerId));
            Assert.That(f.State.PhaseId, Is.EqualTo(answerId));
            Assert.That(call.DeadlineMs, Is.EqualTo(f.Clock.NowMs + f.LastCallMs));
            Assert.That(seats[2].Turn.DeadlineMs, Is.EqualTo(call.DeadlineMs));
            Assert.That(seats[2].Turn.WindowMs, Is.EqualTo(f.WindowMs), "T unchanged");
            Assert.That(seats[2].Turn.LastCall, Is.True);
            Assert.That(f.State.PhaseEndsAtMs, Is.EqualTo(call.DeadlineMs + f.GraceMs));
            Assert.That(f.Game.EndsInMs(seats[2]), Is.EqualTo(f.LastCallMs));

            // Unanswered, the phase ends at the cut deadline plus grace.
            var rest = f.RunUntilPhase(GamePhase.Reveal, stepMs: 3);
            var ended = rest.OfType<PhaseEnded>().Single();
            Assert.That(ended.Reason, Is.EqualTo(PhaseEndReasons.Timeout));
            Assert.That(ended.DurationMs, Is.EqualTo(1000 + f.LastCallMs + f.GraceMs));
        }

        [Test]
        public void LastCallNeverExtendsADeadline()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            long deadline = seats[1].Turn.DeadlineMs;
            f.Advance(f.WindowMs - f.LastCallMs / 2);
            var call = f.Choice("c1", f.Right, 1).OfType<LastCallStarted>().Single();
            Assert.That(call.DeadlineMs, Is.EqualTo(deadline));
            Assert.That(seats[1].Turn.DeadlineMs, Is.EqualTo(deadline));
        }

        [Test]
        public void ALoneConnectedPlayerKeepsTheFullWindow()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            long deadline = seats[1].Turn.DeadlineMs;
            Assert.That(f.Send(new ConnectionClosed("c1")).OfType<LastCallStarted>(), Is.Empty);
            f.Advance(f.WindowMs - 1);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer));
            Assert.That(seats[1].Turn.LastCall, Is.False);
            Assert.That(seats[1].Turn.DeadlineMs, Is.EqualTo(deadline));
        }

        [Test]
        public void NoLastCallWhenTheOnlyLockedPlayerIsDisconnected()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 100);
            f.Send(new ConnectionClosed("c1"));
            f.Send(new ConnectionClosed("c2"));
            // c3 is the only connected player and nobody connected has locked in: full window.
            Assert.That(seats[2].Turn.LastCall, Is.False);
            Assert.That(seats[2].Turn.DeadlineMs, Is.EqualTo(f.State.PhaseStartedAtMs + f.WindowMs));
        }

        [Test]
        public void NoLastCallForAPlayerAlreadyPastTheirDeadline()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Send(new ChangeSettings("c1", null, 2));
            f.Send(new ChangeSettings("c2", null, 2));
            f.RunUntilPhase(GamePhase.Answer);
            // c3 has the base window, c1 and c2 double. c3 passes the deadline (inside grace) while two are open.
            f.Advance(f.WindowMs + 1);
            f.Choice("c1", f.Right, 1);
            var events = f.Choice("c2", f.Right, 1);
            Assert.That(events.OfType<LastCallStarted>(), Is.Empty);
            Assert.That(seats[2].Turn.LastCall, Is.False);
        }

        [Test]
        public void LastCallHappensOncePerPlayerPerQuestion()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 1);
            f.Choice("c2", f.Right, 1);
            Assert.That(seats[2].Turn.LastCall, Is.True);
            // c2 disconnects and comes back: c3 is again the only one open, but the cut is not repeated.
            f.Send(new ConnectionClosed("c2"));
            var events = f.Send(new PlayerRejoin("c2b", seats[1].Token));
            Assert.That(events.OfType<LastCallStarted>(), Is.Empty);
        }

        [Test]
        public void ADisconnectCanTriggerLastCall()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 1);
            var events = f.Send(new ConnectionClosed("c2"));
            var call = events.OfType<LastCallStarted>().Single();
            Assert.That(call.PlayerId, Is.EqualTo(seats[2].PlayerId));
        }

        [Test]
        public void ARejoinCanTriggerLastCall()
        {
            var h = new LobbyFixture();
            var t = h.ToFirstAnswer(3);
            h.Choice("c1", h.Right, 1);
            h.Send(new ConnectionClosed("c1"));
            h.Send(new ConnectionClosed("c2"));
            Assert.That(t[2].Turn.LastCall, Is.False, "no connected locked player");
            var events = h.Send(new PlayerRejoin("c1b", t[0].Token));
            Assert.That(events.OfType<LastCallStarted>().Single().PlayerId, Is.EqualTo(t[2].PlayerId));
        }

        // ---------------------------------------------------------------- disconnects and leaving mid Answer

        [Test]
        public void ADisconnectOfTheLastOpenPlayerEndsTheAnswerAtOnce()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 1);
            f.Choice("c2", f.Right, 1);
            var events = f.Send(new ConnectionClosed("c3"));
            Assert.That(Names(events).Take(3), Is.EqualTo(new[] { "PlayerConnectionChanged", "PhaseEnded(Answer, allDone)", "PhaseStarted(Reveal)" }));
        }

        [Test]
        public void LeaveMidAnswerFreesTheSeatAndEndsTheAnswer()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 1);
            f.Choice("c2", f.Right, 1);
            var events = f.Send(new PlayerLeave("c3", "bye"));
            Assert.That(Names(events).Take(5), Is.EqualTo(new[]
            {
                "InputAcked(True)", "PlayerLeft", "RosterChanged", "PhaseEnded(Answer, allDone)", "PhaseStarted(Reveal)"
            }));
            Assert.That(f.State.FindById(seats[2].PlayerId), Is.Null);
            Assert.That(f.State.Players.Count, Is.EqualTo(2));
        }

        [Test]
        public void LeaveMidAnswerCanTriggerLastCall()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 1);
            var events = f.Send(new PlayerLeave("c2", "bye"));
            Assert.That(Names(events), Is.EqualTo(new[] { "InputAcked(True)", "PlayerLeft", "RosterChanged", "LastCallStarted" }));
            Assert.That(events.OfType<LastCallStarted>().Single().PlayerId, Is.EqualTo(seats[2].PlayerId));
        }

        [Test]
        public void AnAnswerWithNobodyConnectedEndsAtOnce()
        {
            // D73 read literally: every connected player is done when nobody is connected.
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            f.Send(new ConnectionClosed("c1"));
            var events = f.Send(new ConnectionClosed("c2"));
            Assert.That(events.OfType<PhaseEnded>().Single().Reason, Is.EqualTo(PhaseEndReasons.AllDone));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal));
        }

        [Test]
        public void ADisconnectedPlayerIsNotWaitedFor()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Send(new ConnectionClosed("c3"));
            f.Choice("c1", f.Right, 1);
            var events = f.Choice("c2", f.Right, 1);
            Assert.That(events.OfType<PhaseEnded>().Single().Reason, Is.EqualTo(PhaseEndReasons.AllDone));
            Assert.That(seats[2].Turn.Points, Is.EqualTo(0));
            Assert.That(seats[2].Turn.Revealed, Is.True);
        }

        [Test]
        public void ARejoinedPlayerKeepsTheirLockedAnswer()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", 2, 1000);
            f.Send(new ConnectionClosed("c1"));
            f.Send(new PlayerRejoin("c1b", seats[0].Token));
            Assert.That(seats[0].Turn.Choice, Is.EqualTo(2));
            Assert.That(LobbyFixture.AckOf(f.Choice("c1b", 1, 1000)).Reason, Is.EqualTo(AckReasons.Invalid));
        }
    }
}
