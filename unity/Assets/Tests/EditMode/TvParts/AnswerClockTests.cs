using FamilyQuiz.Core.State;
using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>The TV answer countdown in Answer and RaceQuestion, from the personal deadlines.</summary>
    public sealed class AnswerClockTests
    {
        private static PlayerState Player(GameState state, string id, long deadline, bool locked, bool connected)
        {
            var p = new PlayerState(id, id, "en", 0, "t" + id, 1);
            p.Connected = connected;
            p.Turn = new PlayerTurn { DeadlineMs = deadline, Locked = locked };
            state.MutablePlayers.Add(p);
            return p;
        }

        [Test]
        public void WithoutAQuestionTheSpanRunsFromThePhaseStartToTheLatestDeadline()
        {
            // The race: no question in play, personal windows of 8 s and 16 s (timer multiplier 2).
            var s = new GameState { PhaseStartedAtMs = 1000 };
            Player(s, "a", 9000, false, true);
            Player(s, "b", 17000, false, true);
            Assert.AreEqual(16000, AnswerClock.Span(s));
        }

        [Test]
        public void TheOpenDeadlineIgnoresLockedAndDisconnectedPlayers()
        {
            var s = new GameState { PhaseStartedAtMs = 0 };
            Player(s, "a", 8000, false, true);
            Player(s, "b", 16000, true, true);
            Player(s, "c", 20000, false, false);
            Assert.AreEqual(8000, AnswerClock.LatestOpenDeadline(s, 3000));
            Assert.AreEqual(9000, AnswerClock.LatestOpenDeadline(s, 9000));
        }

        [Test]
        public void CountsLockedOfThoseThePhaseWaitsFor()
        {
            var s = new GameState();
            Player(s, "a", 0, true, true);
            Player(s, "b", 0, false, true);
            Player(s, "c", 0, true, false);  // locked in, then left: still counts
            Player(s, "d", 0, false, false); // gone without an answer: not waited for
            AnswerClock.Count(s, out int locked, out int total);
            Assert.AreEqual(2, locked);
            Assert.AreEqual(3, total);
        }
    }
}
