using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>Closest value questions (docs/03_rules.md, "Closest value"; docs/04_formats.md).</summary>
    public sealed class ClosestValueTests
    {
        private static readonly ScoringConfig Cfg = TestContent.Config().Scoring;

        [Test]
        public void DistanceScoringFallsToZeroAtTheTolerance()
        {
            // Everest: answer 8849, tolerance 1500.
            Assert.That(Core.Rules.Scoring.ClosestValue(Cfg, 8849, 8849, 1500, 1), Is.EqualTo(1000));
            Assert.That(Core.Rules.Scoring.ClosestValue(Cfg, 8099, 8849, 1500, 1), Is.EqualTo(500));
            Assert.That(Core.Rules.Scoring.ClosestValue(Cfg, 9599, 8849, 1500, 1), Is.EqualTo(500), "above and below the same");
            Assert.That(Core.Rules.Scoring.ClosestValue(Cfg, 7349, 8849, 1500, 1), Is.EqualTo(0));
            Assert.That(Core.Rules.Scoring.ClosestValue(Cfg, 6000, 8849, 1500, 1), Is.EqualTo(0), "never negative");
        }

        [Test]
        public void DistancePointsAreRoundedToTenBeforeTheMultiplier()
        {
            // 1 - 100/1500 = 0.9333 -> 933.3 -> 930, times 1.5 = 1395.
            Assert.That(Core.Rules.Scoring.ClosestValue(Cfg, 8949, 8849, 1500, 1), Is.EqualTo(930));
            Assert.That(Core.Rules.Scoring.ClosestValue(Cfg, 8949, 8849, 1500, 1.5), Is.EqualTo(1395));
            Assert.That(Core.Rules.Scoring.ClosestValue(Cfg, 8949, 8849, 1500, 2), Is.EqualTo(1860));
        }

        [Test]
        public void TheBonusTakesTheMultiplier()
        {
            var cv = TestContent.Config().ClosestValue;
            Assert.That(Core.Rules.Scoring.ClosestBonus(cv, 1), Is.EqualTo(cv.ClosestBonus));
            Assert.That(Core.Rules.Scoring.ClosestBonus(cv, 1.5), Is.EqualTo((int)(cv.ClosestBonus * 1.5)));
            Assert.That(Core.Rules.Scoring.ClosestBonus(cv, 2), Is.EqualTo(cv.ClosestBonus * 2));
        }

        // ---------------------------------------------------------------- in a game

        /// <summary>Three players at the round 1 closest value question (slot 4), its Answer open.</summary>
        private static (LobbyFixture, PlayerState[]) AtClosestValue(int players = 3)
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstVote(players);
            do
            {
                f.SkipTo(GamePhase.Answer);
            } while (!f.State.Question.IsClosestValue);

            Assert.That(f.State.Question.Def.Format, Is.EqualTo(QuestionDef.ClosestValue));
            return (f, seats);
        }

        private static double Answer(LobbyFixture f) => f.State.Question.Def.Answer.Value;
        private static double Tolerance(LobbyFixture f) => f.State.Question.Def.Tolerance.Value;
        private static int Bonus(LobbyFixture f) => Core.Rules.Scoring.ClosestBonus(f.Config.ClosestValue, f.State.Question.Multiplier);

        [Test]
        public void TheClosestValueWindowIsItsOwn()
        {
            var (f, seats) = AtClosestValue();
            Assert.That(seats[0].Turn.WindowMs, Is.EqualTo(f.ClosestWindowMs));
            Assert.That(f.State.PhaseBudgetMs, Is.EqualTo(f.ClosestWindowMs));
            Assert.That(f.State.Question.DisplayOrder, Is.Empty);
            Assert.That(f.State.Question.CorrectDisplayIndex, Is.EqualTo(-1));
        }

        [Test]
        public void TheClosestGuessGetsTheBonusEvenWithZeroDistancePoints()
        {
            var (f, seats) = AtClosestValue();
            double answer = Answer(f);
            double tol = Tolerance(f);
            var range = f.State.Question.Def.Range;
            // Everyone is beyond the tolerance; P2 is the least far off.
            double far = System.Math.Max(range[0], answer - tol * 1.5);
            double farther = System.Math.Max(range[0], answer - tol * 1.8);
            f.Value("c1", farther, 1000);
            f.Value("c2", far, 1000);
            f.Value("c3", range[0], 1000);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal));
            if (far == farther) Assert.Inconclusive("range too narrow for this question");
            Assert.That(seats[1].Turn.Points, Is.EqualTo(Bonus(f)));
            Assert.That(seats[1].Turn.Closest, Is.True);
            Assert.That(seats[1].Turn.Correct, Is.False, "the bonus alone is not a correct answer");
            Assert.That(seats[0].Turn.Points, Is.EqualTo(0));
            Assert.That(seats[2].Turn.Points, Is.EqualTo(0));
        }

        [Test]
        public void TiedClosestGuessesAllGetTheBonus()
        {
            var (f, seats) = AtClosestValue();
            double answer = Answer(f);
            double step = f.State.Question.Def.Step.Value;
            double off = step * 2;
            f.Value("c1", answer - off, 4000);
            f.Value("c2", answer + off, 1000);
            f.Value("c3", answer + off * 3, 1000);
            int distance = Core.Rules.Scoring.ClosestValue(f.Config.Scoring, answer - off, answer, Tolerance(f), f.State.Question.Multiplier);
            Assert.That(seats[0].Turn.Points, Is.EqualTo(distance + Bonus(f)));
            Assert.That(seats[1].Turn.Points, Is.EqualTo(distance + Bonus(f)));
            Assert.That(seats[2].Turn.Closest, Is.False);
            Assert.That(seats[2].Turn.Bonus, Is.EqualTo(0));
            Assert.That(seats.All(p => p.Turn.Correct), Is.True);
        }

        [Test]
        public void NoAnswerScoresNothingAndGetsNoBonus()
        {
            var (f, seats) = AtClosestValue(2);
            f.Value("c1", f.State.Question.Def.Range[1], 500);
            f.SkipTo(GamePhase.Reveal);
            Assert.That(seats[1].Turn.Points, Is.EqualTo(0));
            Assert.That(seats[1].Turn.Closest, Is.False);
            Assert.That(seats[0].Turn.Closest, Is.True, "the only guess is the closest one");
            Assert.That(seats[0].Turn.Points, Is.EqualTo(Bonus(f)));
        }

        [Test]
        public void AnExactGuessScoresMaxPointsPlusBonusAndCountsAsCorrect()
        {
            var (f, seats) = AtClosestValue(2);
            int before = seats[0].CorrectCount;
            var locked = f.Value("c1", Answer(f), 3000).OfType<AnswerLocked>().Single();
            Assert.That(locked.Kind, Is.EqualTo(InputKinds.Value));
            Assert.That(locked.Value, Is.EqualTo(Answer(f)));
            Assert.That(locked.Correct, Is.True);
            Assert.That(locked.Points, Is.EqualTo(f.Config.Scoring.MaxPoints), "the lock knows the distance points, not the bonus");
            f.SkipTo(GamePhase.Reveal);
            var score = seats[0].Turn.Points;
            Assert.That(score, Is.EqualTo(f.Config.Scoring.MaxPoints + Bonus(f)));
            Assert.That(seats[0].CorrectCount, Is.EqualTo(before + 1));
        }

        [Test]
        public void AValueMustBeInsideTheRangeAndAChoiceIsNoValue()
        {
            var (f, _) = AtClosestValue(2);
            var range = f.State.Question.Def.Range;
            Assert.That(LobbyFixture.AckOf(f.Value("c1", range[1] + 1, 100)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(LobbyFixture.AckOf(f.Value("c1", range[0] - 1, 100)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(LobbyFixture.AckOf(f.Value("c1", double.NaN, 100)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(LobbyFixture.AckOf(f.Value("c1", Answer(f), null)).Reason, Is.EqualTo(AckReasons.Invalid), "ms is needed");
            Assert.That(LobbyFixture.AckOf(f.Choice("c1", 0, 100)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(LobbyFixture.AckOf(f.Value("c1", range[0], 100)).Ok, Is.True, "the ends of the range are allowed");
            Assert.That(LobbyFixture.AckOf(f.Value("c1", Answer(f), 100)).Reason, Is.EqualTo(AckReasons.Invalid), "no changing after Lock");
        }

        [Test]
        public void AValueIsRefusedOnAMultipleChoiceQuestion()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            Assert.That(f.State.Question.IsClosestValue, Is.False);
            Assert.That(LobbyFixture.AckOf(f.Value("c1", 5, 100)).Reason, Is.EqualTo(AckReasons.Invalid));
        }

        [Test]
        public void LastCallWorksOnClosestValueToo()
        {
            var (f, seats) = AtClosestValue(2);
            var events = f.Value("c1", Answer(f), 100);
            var call = events.OfType<LastCallStarted>().Single();
            Assert.That(call.PlayerId, Is.EqualTo(seats[1].PlayerId));
            Assert.That(seats[1].Turn.DeadlineMs, Is.EqualTo(f.Clock.NowMs + f.LastCallMs));
        }

        [Test]
        public void TheRevealedEventHasNoCorrectTile()
        {
            var (f, _) = AtClosestValue(2);
            var events = f.SkipTo(GamePhase.Reveal);
            Assert.That(events.OfType<Revealed>().Single().CorrectDisplayIndex, Is.EqualTo(-1));
        }
    }
}
