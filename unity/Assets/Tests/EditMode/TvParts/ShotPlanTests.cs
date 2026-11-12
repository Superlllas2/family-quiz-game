using FamilyQuiz.Core.State;
using FamilyQuiz.Presentation.Staging;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>
    /// Which camera shot the 3D set shows (docs/21_staging.md, "Shot list per phase" and "Game night 1 version";
    /// decisions 0016): Seats in the dark before the show, the host solo open, the establishing shot with the lights
    /// coming up, then the phase shots. A listener only: shots follow phases and never hold one.
    /// </summary>
    public sealed class ShotPlanTests
    {
        private const long EstablishMs = 6000;
        private const long LightsUpMs = 1500;
        private const long BubbleMs = 3500;

        private static ShotPlan Plan() => new ShotPlan(EstablishMs, LightsUpMs);

        private static ShotPlan In(GamePhase phase, long atMs)
        {
            var plan = Plan();
            plan.OnPhase(phase, atMs);
            return plan;
        }

        [TestCase(GamePhase.Lobby, Shot.LobbyTour)] // D160: the camera tours the dim studio
        [TestCase(GamePhase.StartCountdown, Shot.Seats)]
        [TestCase(GamePhase.TopicVote, Shot.Curtain)]
        [TestCase(GamePhase.TopicTie, Shot.Curtain)] // the TopicVote shot
        [TestCase(GamePhase.Attacks, Shot.Wide)] // the Scores shot
        [TestCase(GamePhase.QuestionShow, Shot.Stage)]
        [TestCase(GamePhase.Answer, Shot.Wide)] // on the guests, the question card and tiles under them (0017)
        [TestCase(GamePhase.Reveal, Shot.Wide)]
        [TestCase(GamePhase.Scores, Shot.Wide)]
        [TestCase(GamePhase.RoundEnd, Shot.Wide)]
        [TestCase(GamePhase.FinalIntro, Shot.Wide)]
        [TestCase(GamePhase.RaceQuestion, Shot.Wide)]
        [TestCase(GamePhase.RaceReveal, Shot.Wide)]
        [TestCase(GamePhase.Results, Shot.Wide)]
        [TestCase(GamePhase.Poll, Shot.Wide)]
        public void EachPhaseHasItsShot(GamePhase phase, Shot shot)
        {
            Assert.That(In(phase, 1000).Current(1500).Shot, Is.EqualTo(shot));
        }

        [Test]
        public void AJoiningPlayerNeverCutsTheLobbyTour()
        {
            // D160: the tour never cuts to a player who joins; a host greeting them does not cut either.
            var plan = In(GamePhase.Lobby, 0);
            plan.OnHostLine("konstantin", 4000);
            Assert.That(plan.Current(100).Shot, Is.EqualTo(Shot.LobbyTour));
        }

        [Test]
        public void TheStudioIsDarkAroundTheSeatsBeforeTheShow()
        {
            Assert.That(In(GamePhase.Lobby, 0).LightLevel(500), Is.EqualTo(0));
            Assert.That(In(GamePhase.StartCountdown, 0).LightLevel(500), Is.EqualTo(0));
        }

        [Test]
        public void TheIntroOpensOnEachHostAloneThenEstablishesTheStudio()
        {
            var plan = In(GamePhase.Intro, 10000);
            plan.OnHostLine("konstantin", 10000 + BubbleMs);
            Assert.That(plan.Current(10100), Is.EqualTo(ShotChoice.Solo("konstantin")));
            Assert.That(plan.LightLevel(10100), Is.EqualTo(0), "the studio stays dark while the hosts open the show");

            // The second gameStart line starts when the first bubble ends (docs/15_schemas.md, "Intro exception").
            plan.OnHostLine("ilja", 10000 + 2 * BubbleMs);
            Assert.That(plan.Current(10000 + BubbleMs + 50), Is.EqualTo(ShotChoice.Solo("ilja")));

            long establishAt = 10000 + 2 * BubbleMs + ShotPlan.HostLineSlackMs;
            Assert.That(plan.Current(establishAt).Shot, Is.EqualTo(Shot.Establish));
            Assert.That(plan.LightLevel(establishAt), Is.EqualTo(0));
            Assert.That(plan.LightLevel(establishAt + LightsUpMs / 2), Is.EqualTo(0.5).Within(1e-9));
            Assert.That(plan.LightLevel(establishAt + LightsUpMs), Is.EqualTo(1));
            Assert.That(plan.Current(establishAt + EstablishMs - 1).Shot, Is.EqualTo(Shot.Establish));
            Assert.That(plan.Current(establishAt + EstablishMs).Shot, Is.EqualTo(Shot.Wide));
        }

        [Test]
        public void TheSecondLineMayStartAFrameLateWithoutACutToTheStudio()
        {
            var plan = In(GamePhase.Intro, 0);
            plan.OnHostLine("konstantin", BubbleMs);
            // A frame just after the first bubble ended, before the dialogue started the second line.
            Assert.That(plan.Current(BubbleMs + 20), Is.EqualTo(ShotChoice.Solo("konstantin")));
            plan.OnHostLine("ilja", BubbleMs + 30 + BubbleMs);
            Assert.That(plan.Current(BubbleMs + 40), Is.EqualTo(ShotChoice.Solo("ilja")));
        }

        [Test]
        public void AnIntroWithoutHostLinesEstablishesAtOnce()
        {
            var plan = In(GamePhase.Intro, 0);
            Assert.That(plan.Current(0).Shot, Is.EqualTo(Shot.Establish));
            Assert.That(plan.Current(EstablishMs).Shot, Is.EqualTo(Shot.Wide));
            Assert.That(plan.LightLevel(EstablishMs), Is.EqualTo(1));
        }

        [Test]
        public void ALineAfterTheStudioWasEstablishedDoesNotCutBack()
        {
            var plan = In(GamePhase.Intro, 0);
            Assert.That(plan.Current(100).Shot, Is.EqualTo(Shot.Establish));
            plan.OnHostLine("konstantin", 200 + BubbleMs);
            Assert.That(plan.Current(300).Shot, Is.EqualTo(Shot.Establish));
        }

        [Test]
        public void TheFinalIntroCutsToTheHostWhileTheLineShows()
        {
            var plan = In(GamePhase.FinalIntro, 0);
            plan.OnHostLine("ilja", BubbleMs);
            Assert.That(plan.Current(100), Is.EqualTo(ShotChoice.Solo("ilja")));
            Assert.That(plan.Current(BubbleMs + ShotPlan.HostLineSlackMs).Shot, Is.EqualTo(Shot.Wide));
            Assert.That(plan.LightLevel(100), Is.EqualTo(1));
        }

        [TestCase(GamePhase.Lobby)]
        [TestCase(GamePhase.StartCountdown)]
        [TestCase(GamePhase.TopicVote)]
        [TestCase(GamePhase.TopicTie)]
        [TestCase(GamePhase.QuestionShow)]
        [TestCase(GamePhase.Answer)]
        [TestCase(GamePhase.Reveal)]
        [TestCase(GamePhase.Scores)]
        [TestCase(GamePhase.Attacks)]
        [TestCase(GamePhase.RoundEnd)]
        [TestCase(GamePhase.Results)]
        public void NoHostSoloWherePlayersNeedTheSet(GamePhase phase)
        {
            var plan = In(phase, 0);
            var before = plan.Current(10).Shot;
            plan.OnHostLine("konstantin", BubbleMs);
            Assert.That(plan.Current(100).Shot, Is.EqualTo(before));
        }

        [Test]
        public void APhaseChangeCutsARunningHostSolo()
        {
            var plan = In(GamePhase.FinalIntro, 0);
            plan.OnHostLine("konstantin", 8000);
            plan.OnPhase(GamePhase.RaceQuestion, 2000);
            Assert.That(plan.Current(2100).Shot, Is.EqualTo(Shot.Wide));
        }

        [Test]
        public void TheIntroEndingEarlyCutsTheEstablishingShot()
        {
            var plan = In(GamePhase.Intro, 0);
            Assert.That(plan.Current(100).Shot, Is.EqualTo(Shot.Establish));
            plan.OnPhase(GamePhase.TopicVote, 1000);
            Assert.That(plan.Current(1100).Shot, Is.EqualTo(Shot.Curtain));
            Assert.That(plan.LightLevel(1100), Is.EqualTo(1), "a cut ends the lights coming up at once");
        }

        [Test]
        public void BackInTheLobbyTheStudioGoesDarkAgain()
        {
            var plan = In(GamePhase.Poll, 0);
            plan.OnPhase(GamePhase.Lobby, 1000);
            Assert.That(plan.Current(1100).Shot, Is.EqualTo(Shot.LobbyTour));
            Assert.That(plan.LightLevel(1100), Is.EqualTo(0));
        }

        [Test]
        public void ASecondIntroOpensWithTheHostsAgain()
        {
            var plan = In(GamePhase.Intro, 0);
            Assert.That(plan.Current(100).Shot, Is.EqualTo(Shot.Establish));
            plan.OnPhase(GamePhase.Lobby, 1000);
            plan.OnPhase(GamePhase.Intro, 2000);
            plan.OnHostLine("ilja", 2000 + BubbleMs);
            Assert.That(plan.Current(2100), Is.EqualTo(ShotChoice.Solo("ilja")));
        }
    }
}
