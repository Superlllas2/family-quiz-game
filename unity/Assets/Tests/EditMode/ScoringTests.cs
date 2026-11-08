using System;
using System.Linq;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>docs/03_rules.md "Scoring", multiple choice; docs/19_acceptance.md Day 2 scoring checks.</summary>
    public sealed class ScoringTests
    {
        private static GameConfig Config => TestContent.Content().Config;
        private static ScoringConfig S => Config.Scoring;
        private static long T => LobbyFixture.Ms(Config.Timers.Answer.MultipleChoice);
        private static double RoundOne => Config.Rounds.Multipliers[0];
        /// <summary>Points for a correct answer at the buzzer in round 1: maxPoints x (1 - speedPenalty), rounded to roundTo.</summary>
        private static int AtTheBuzzer => (int)(Math.Round(S.MaxPoints * (1 - S.SpeedPenalty) / S.RoundTo, MidpointRounding.AwayFromZero) * S.RoundTo);

        [Test]
        public void TheConfigHasTheSpecsStartingValues()
        {
            // 03_rules.md: maxPoints 1000 and speedPenalty 0.8 (D144), "so a correct answer at the buzzer earns 200".
            Assert.That(S.MaxPoints, Is.EqualTo(1000));
            Assert.That(S.SpeedPenalty, Is.EqualTo(0.8));
            Assert.That(Math.Round(S.MaxPoints * (1 - S.SpeedPenalty)), Is.EqualTo(200));
        }

        [Test]
        public void ACorrectAnswerAtZeroMsGetsMaxPoints()
        {
            Assert.That(Scoring.MultipleChoice(S, true, 0, T, RoundOne), Is.EqualTo(1000));
        }

        [Test]
        public void ACorrectAnswerAtTheBuzzerKeepsWhatTheSpeedPenaltyLeaves()
        {
            Assert.That(Scoring.MultipleChoice(S, true, T, T, RoundOne), Is.EqualTo(AtTheBuzzer));
        }

        [Test]
        public void AWrongAnswerGetsZero()
        {
            Assert.That(Scoring.MultipleChoice(S, false, 0, T, RoundOne), Is.EqualTo(0));
            Assert.That(Scoring.MultipleChoice(S, false, T, T, Config.Rounds.Multipliers.Max()), Is.EqualTo(0));
        }

        [Test]
        public void PointsAreRoundedToRoundTo()
        {
            for (long ms = 0; ms <= T; ms += 137)
            {
                int points = Scoring.MultipleChoice(S, true, ms, T, RoundOne);
                double raw = S.MaxPoints * (1 - S.SpeedPenalty * ms / (double)T);
                Assert.That(points % S.RoundTo, Is.EqualTo(0), "ms " + ms);
                Assert.That(Math.Abs(points - raw), Is.LessThanOrEqualTo(S.RoundTo / 2.0 + 1e-9), "ms " + ms);
            }
        }

        [Test]
        public void AMidpointRoundsAwayFromZero()
        {
            // A raw score exactly halfway between two multiples of roundTo rounds up (MidpointRounding.AwayFromZero).
            double target = S.MaxPoints - 2 * S.RoundTo - S.RoundTo / 2.0;
            long ms = (long)Math.Round((1 - target / S.MaxPoints) * T / S.SpeedPenalty);
            Assert.That(S.MaxPoints * (1 - S.SpeedPenalty * ms / (double)T), Is.EqualTo(target).Within(1e-9), "test setup");
            Assert.That(Scoring.MultipleChoice(S, true, ms, T, RoundOne), Is.EqualTo(S.MaxPoints - 2 * S.RoundTo));
        }

        [Test]
        public void TheRoundMultiplierAppliesAfterRounding()
        {
            // Round 3 multiplies every point by 1.5 (docs/19_acceptance.md).
            double round3 = Config.Rounds.Multipliers[2];
            Assert.That(round3, Is.EqualTo(1.5));
            Assert.That(Scoring.MultipleChoice(S, true, 0, T, round3), Is.EqualTo(1500));
            Assert.That(Scoring.MultipleChoice(S, true, T, T, round3), Is.EqualTo((int)Math.Round(AtTheBuzzer * round3, MidpointRounding.AwayFromZero)));
            long ms = T / 3;
            int one = Scoring.MultipleChoice(S, true, ms, T, RoundOne);
            Assert.That(Scoring.MultipleChoice(S, true, ms, T, round3), Is.EqualTo((int)Math.Round(one * round3, MidpointRounding.AwayFromZero)));
        }

        [Test]
        public void MsIsClampedToTheWindow()
        {
            Assert.That(Scoring.MultipleChoice(S, true, -5000, T, RoundOne), Is.EqualTo(1000));
            Assert.That(Scoring.MultipleChoice(S, true, T * 10, T, RoundOne), Is.EqualTo(AtTheBuzzer));
            Assert.That(Scoring.ClampMs(-1, T), Is.EqualTo(0));
            Assert.That(Scoring.ClampMs(T + 1, T), Is.EqualTo(T));
            Assert.That(Scoring.ClampMs(T - 1, T), Is.EqualTo(T - 1));
        }

        [Test]
        public void PointsAreNeverNegative()
        {
            var harsh = new ScoringConfig { MaxPoints = S.MaxPoints, SpeedPenalty = 1, RoundTo = S.RoundTo };
            Assert.That(Scoring.MultipleChoice(harsh, true, T, T, RoundOne), Is.EqualTo(0));
        }

        [Test]
        public void ATwoTimesTimerMultiplierDoublesT()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(3);
            double two = f.Config.Players.TimerMultiplierOptions.Max();
            Assert.That(two, Is.EqualTo(2));
            f.Send(new FamilyQuiz.Core.Commands.ChangeSettings("c2", null, two));
            f.StartNextGame();
            f.Advance(f.VoteEndMs);
            f.Advance(f.QuestionShowMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer));
            Assert.That(seats[0].Turn.WindowMs, Is.EqualTo(f.WindowMs));
            Assert.That(seats[1].Turn.WindowMs, Is.EqualTo(2 * f.WindowMs));
            Assert.That(seats[1].Turn.DeadlineMs, Is.EqualTo(f.State.PhaseStartedAtMs + 2 * f.WindowMs));

            // The same tap time scores more with the doubled window: at the base window, 600 instead of 200
            // (speedPenalty 0.8: 1 - 0.8 x 1/2 and 1 - 0.8).
            var a = f.Choice("c1", f.Right, f.WindowMs).OfType<AnswerLocked>().Single();
            var b = f.Choice("c2", f.Right, f.WindowMs).OfType<AnswerLocked>().Single();
            Assert.That(a.Points, Is.EqualTo(200));
            Assert.That(b.Points, Is.EqualTo(600));
        }
    }
}
