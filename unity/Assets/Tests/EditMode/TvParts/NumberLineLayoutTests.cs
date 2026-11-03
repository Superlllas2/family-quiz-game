using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>The closest value number line: values to places, and lanes for chips that would cover each other.</summary>
    public sealed class NumberLineLayoutTests
    {
        [Test]
        public void MapsTheRangeToZeroToOne()
        {
            Assert.AreEqual(0.0, NumberLineLayout.Fraction(1800, 1800, 2000), 1e-9);
            Assert.AreEqual(1.0, NumberLineLayout.Fraction(2000, 1800, 2000), 1e-9);
            Assert.AreEqual(0.25, NumberLineLayout.Fraction(1850, 1800, 2000), 1e-9);
            Assert.AreEqual(0.5, NumberLineLayout.Fraction(-5, -10, 0), 1e-9);
        }

        [Test]
        public void ClampsValuesOutsideTheRangeAndCentersAnEmptyRange()
        {
            Assert.AreEqual(0.0, NumberLineLayout.Fraction(1700, 1800, 2000), 1e-9);
            Assert.AreEqual(1.0, NumberLineLayout.Fraction(2100, 1800, 2000), 1e-9);
            Assert.AreEqual(0.5, NumberLineLayout.Fraction(5, 5, 5), 1e-9);
            Assert.AreEqual(0.5, NumberLineLayout.Fraction(5, 10, 0), 1e-9);
            Assert.AreEqual(0.5, NumberLineLayout.Fraction(double.NaN, 0, 10), 1e-9);
        }

        [Test]
        public void ChipsFarApartShareTheLowestLane()
        {
            var lanes = NumberLineLayout.Lanes(new[] { 0.9, 0.1, 0.5 }, 0.2);
            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, lanes);
            Assert.AreEqual(1, NumberLineLayout.LaneCount(lanes));
        }

        [Test]
        public void CloseChipsStackAndALaneIsReusedOnceThereIsRoom()
        {
            // Sorted: 0.10 (a), 0.15 (b), 0.20 (c), 0.40 (d). a lane 0, b too close to a: lane 1, c too close to a
            // and b: lane 2, d far enough from a: lane 0.
            var lanes = NumberLineLayout.Lanes(new[] { 0.10, 0.15, 0.20, 0.40 }, 0.2);
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 0 }, lanes);
            Assert.AreEqual(3, NumberLineLayout.LaneCount(lanes));
        }

        [Test]
        public void EqualGuessesStackInInputOrder()
        {
            var lanes = NumberLineLayout.Lanes(new[] { 0.5, 0.5, 0.5 }, 0.1);
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, lanes);
        }

        [Test]
        public void AGapOfExactlyAChipWidthIsEnough()
        {
            var lanes = NumberLineLayout.Lanes(new[] { 0.3, 0.1 }, 0.2);
            CollectionAssert.AreEqual(new[] { 0, 0 }, lanes);
        }

        [Test]
        public void NoChipsNoLanes()
        {
            var lanes = NumberLineLayout.Lanes(new double[0], 0.2);
            Assert.AreEqual(0, lanes.Length);
            Assert.AreEqual(0, NumberLineLayout.LaneCount(lanes));
        }
    }
}
