using FamilyQuiz.Presentation.Staging;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>
    /// The Lobby tour (D160, docs/21_staging.md "LobbyTour"): slow moves around the dim studio, a new one every
    /// staging.lobby.cutSeconds, the fade out and the fade in inside each cut, cycling through the moves without ever
    /// showing the same one twice in a row. Driven by a fake clock in ms.
    /// </summary>
    public sealed class LobbyTourTests
    {
        private const long CutMs = 5000;
        private const long FadeMs = 500;

        private static LobbyTour Tour(int moves = 6) => new LobbyTour(CutMs, FadeMs, moves);

        [Test]
        public void EachCutLastsCutSecondsAndMovesToTheNextMove()
        {
            var tour = Tour();
            Assert.That(tour.At(0).Move, Is.EqualTo(0));
            Assert.That(tour.At(CutMs - 1).Move, Is.EqualTo(0));
            Assert.That(tour.At(CutMs).Move, Is.EqualTo(1));
            Assert.That(tour.At(CutMs).Cut, Is.EqualTo(1));
            Assert.That(tour.At(5 * CutMs + 10).Move, Is.EqualTo(5));
        }

        [Test]
        public void TheMovesCycleAndNeverRepeatTwiceInARow()
        {
            var tour = Tour(6);
            Assert.That(tour.At(6 * CutMs).Move, Is.EqualTo(0), "after the last move the tour starts over");
            int last = -1;
            for (long t = 0; t < 40 * CutMs; t += CutMs)
            {
                int move = tour.At(t).Move;
                Assert.That(move, Is.Not.EqualTo(last));
                last = move;
            }
        }

        [Test]
        public void ProgressRunsFromStartToEndOfEachCut()
        {
            var tour = Tour();
            Assert.That(tour.At(0).Progress, Is.EqualTo(0));
            Assert.That(tour.At(CutMs / 2).Progress, Is.EqualTo(0.5).Within(1e-9));
            Assert.That(tour.At(CutMs + CutMs / 4).Progress, Is.EqualTo(0.25).Within(1e-9));
        }

        [Test]
        public void TheFadesAreInsideTheCut()
        {
            var tour = Tour();
            Assert.That(tour.At(0).Alpha, Is.EqualTo(0), "each cut starts from black");
            Assert.That(tour.At(FadeMs / 2).Alpha, Is.EqualTo(0.5).Within(1e-9));
            Assert.That(tour.At(FadeMs).Alpha, Is.EqualTo(1));
            Assert.That(tour.At(CutMs / 2).Alpha, Is.EqualTo(1));
            Assert.That(tour.At(CutMs - FadeMs).Alpha, Is.EqualTo(1));
            Assert.That(tour.At(CutMs - FadeMs / 2).Alpha, Is.EqualTo(0.5).Within(1e-9));
            Assert.That(tour.At(CutMs - 1).Alpha, Is.LessThan(0.01), "and ends in black");
            Assert.That(tour.At(CutMs).Alpha, Is.EqualTo(0));
            Assert.That(tour.At(CutMs + FadeMs).Alpha, Is.EqualTo(1));
        }

        [Test]
        public void FadesLongerThanHalfACutMeetInTheMiddle()
        {
            var tour = new LobbyTour(1000, 800, 3);
            Assert.That(tour.At(500).Alpha, Is.EqualTo(1));
            Assert.That(tour.At(250).Alpha, Is.EqualTo(0.5).Within(1e-9));
            Assert.That(tour.At(750).Alpha, Is.EqualTo(0.5).Within(1e-9));
        }

        [Test]
        public void NoFadeMeansNoBlack()
        {
            var tour = new LobbyTour(CutMs, 0, 3);
            Assert.That(tour.At(0).Alpha, Is.EqualTo(1));
            Assert.That(tour.At(CutMs - 1).Alpha, Is.EqualTo(1));
        }

        [Test]
        public void ASingleMoveOrNoMovesStillWork()
        {
            Assert.That(new LobbyTour(CutMs, FadeMs, 1).At(3 * CutMs).Move, Is.EqualTo(0));
            Assert.That(new LobbyTour(CutMs, FadeMs, 0).At(3 * CutMs).Move, Is.EqualTo(0));
        }

        [Test]
        public void TimeBeforeTheStartIsTheStart()
        {
            var frame = Tour().At(-300);
            Assert.That(frame.Move, Is.EqualTo(0));
            Assert.That(frame.Progress, Is.EqualTo(0));
            Assert.That(frame.Alpha, Is.EqualTo(0));
        }

        [Test]
        public void ANonPositiveCutHoldsTheFirstMoveInFullLight()
        {
            var frame = new LobbyTour(0, FadeMs, 4).At(12345);
            Assert.That(frame.Move, Is.EqualTo(0));
            Assert.That(frame.Alpha, Is.EqualTo(1));
        }

        [Test]
        public void TheStudioIsDarkStopsDarkerInTheLobbyAndBrightnessAddsOnTop()
        {
            // Light level 0 (Lobby, StartCountdown, the Intro before the lights come up), 1 (the game).
            Assert.That(StudioExposure.PostExposure(1.1, 0, 1, 0), Is.EqualTo(0.1).Within(1e-9));
            Assert.That(StudioExposure.PostExposure(1.1, 1, 1, 0), Is.EqualTo(1.1).Within(1e-9));
            Assert.That(StudioExposure.PostExposure(1.1, 0.5, 1, 0), Is.EqualTo(0.6).Within(1e-9));
            Assert.That(StudioExposure.PostExposure(1.1, 1, 1, 0.75), Is.EqualTo(1.85).Within(1e-9));
            Assert.That(StudioExposure.PostExposure(1.1, 0, 1, -1), Is.EqualTo(-0.9).Within(1e-9));
        }

        [Test]
        public void TheSeatLampsKeepTheirBrightnessInTheDark()
        {
            // The guest and host keys are raised by as many stops as the exposure is lowered.
            Assert.That(StudioExposure.KeyBoost(0, 1), Is.EqualTo(2).Within(1e-9));
            Assert.That(StudioExposure.KeyBoost(1, 1), Is.EqualTo(1).Within(1e-9));
            Assert.That(StudioExposure.KeyBoost(0, 0), Is.EqualTo(1).Within(1e-9));
        }
    }
}
