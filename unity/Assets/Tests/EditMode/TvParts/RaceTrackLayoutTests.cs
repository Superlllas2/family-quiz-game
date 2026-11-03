using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>The race track: one cell per step from 0 to race.finishSteps, the finish line before the last cell.</summary>
    public sealed class RaceTrackLayoutTests
    {
        [Test]
        public void HasOneCellPerStepIncludingTheFinish()
        {
            Assert.AreEqual(11, RaceTrackLayout.Cells(10));
            Assert.AreEqual(1f / 11f, RaceTrackLayout.CellWidth(10), 1e-6f);
        }

        [Test]
        public void PutsAStepInTheMiddleOfItsCell()
        {
            Assert.AreEqual(0.5f / 11f, RaceTrackLayout.CellCenter(0, 10), 1e-6f);
            Assert.AreEqual(3.5f / 11f, RaceTrackLayout.CellCenter(3, 10), 1e-6f);
            Assert.AreEqual(10.5f / 11f, RaceTrackLayout.CellCenter(10, 10), 1e-6f);
        }

        [Test]
        public void ClampsStepsOutsideTheTrack()
        {
            Assert.AreEqual(RaceTrackLayout.CellCenter(0, 10), RaceTrackLayout.CellCenter(-2, 10), 1e-6f);
            Assert.AreEqual(RaceTrackLayout.CellCenter(10, 10), RaceTrackLayout.CellCenter(14, 10), 1e-6f);
        }

        [Test]
        public void TheFinishLineIsTheLeftEdgeOfTheFinishCell()
        {
            Assert.AreEqual(10f / 11f, RaceTrackLayout.FinishLine(10), 1e-6f);
            Assert.Less(RaceTrackLayout.CellCenter(9, 10), RaceTrackLayout.FinishLine(10));
            Assert.Greater(RaceTrackLayout.CellCenter(10, 10), RaceTrackLayout.FinishLine(10));
        }
    }
}
