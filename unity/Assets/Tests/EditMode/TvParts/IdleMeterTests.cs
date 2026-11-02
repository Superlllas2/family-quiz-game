using System.Linq;
using FamilyQuiz.Core.State;
using FamilyQuiz.DevTools;
using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>Plain C# parts of the TV presentation and the debug overlay.</summary>
    public sealed class IdleMeterTests
    {
        [Test]
        public void CountsIdleWithTheStateAtTheStartOfEachInterval()
        {
            var m = new IdleMeter();
            m.Start(1000, idle: true, countsForStretch: true);   // QuestionShow
            m.Sample(5000, idle: false, countsForStretch: true); // Answer starts at 5000
            m.Sample(8000, idle: true, countsForStretch: true);  // Reveal at 8000
            Assert.AreEqual(7000, m.GameMs);
            Assert.AreEqual(4000, m.IdleMs);
            Assert.AreEqual(4000, m.LongestStretchMs);
            m.Sample(15000, idle: true, countsForStretch: true); // Reveal + Scores 7 s, then QuestionShow
            m.Sample(19000, idle: false, countsForStretch: true); // 4 s more, then Answer
            Assert.AreEqual(11000, m.LongestStretchMs);
            Assert.AreEqual(0, m.CurrentStretchMs);
            Assert.AreEqual(15000d / 18000d, m.IdleShare, 1e-9);
        }

        [Test]
        public void ResultsBreaksAndIsLeftOutOfTheLongestStretch()
        {
            var m = new IdleMeter();
            m.Start(0, true, true);
            m.Sample(3000, true, false);   // Results starts at 3000
            m.Sample(23000, true, false);  // 20 s of Results
            Assert.AreEqual(3000, m.LongestStretchMs);
            Assert.AreEqual(23000, m.IdleMs);
            m.Stop(23000);
            Assert.IsFalse(m.Running);
            Assert.IsTrue(m.Finished);
        }

        [Test]
        public void StopCountsToTheEndThenFreezes()
        {
            var m = new IdleMeter();
            m.Start(0, false, true);
            m.Sample(1000, true, true);
            m.Stop(4000);
            m.Sample(90000, true, true);
            Assert.AreEqual(4000, m.GameMs);
            Assert.AreEqual(3000, m.IdleMs);
        }

        [Test]
        public void APausedClockAddsNothing()
        {
            var m = new IdleMeter();
            m.Start(500, true, true);
            m.Sample(500, true, true);
            m.Sample(400, true, true); // never backwards
            Assert.AreEqual(0, m.GameMs);
        }

        [Test]
        public void StartClearsThePreviousGame()
        {
            var m = new IdleMeter();
            m.Start(0, true, true);
            m.Sample(5000, true, true);
            m.Stop(5000);
            m.Start(10000, false, true);
            Assert.AreEqual(0, m.GameMs);
            Assert.AreEqual(0, m.LongestStretchMs);
            Assert.IsTrue(m.Running);
        }
    }
}
