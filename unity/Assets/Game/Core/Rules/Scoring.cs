using System;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>Points per answer (docs/03_rules.md, "Scoring"). Pure functions of the config and the answer.</summary>
    public static class Scoring
    {
        private const int FloatDecimals = 6;

        /// <summary>
        /// Multiple choice: points = roundTo(maxPoints x (1 - speedPenalty x t / T)) x multiplier, with t clamped to 0..T,
        /// rounded to the nearest scoring.roundTo before the multiplier (midpoints away from zero). Wrong or no answer: 0.
        /// Never negative.
        /// </summary>
        /// <param name="scoring">config.json "scoring".</param>
        /// <param name="correct">True when the locked option is the right one.</param>
        /// <param name="ms">t: the phone's ms from the options appearing to the tap.</param>
        /// <param name="windowMs">T: the player's personal window in ms (last call never changes it).</param>
        /// <param name="multiplier">The round multiplier (rounds.multipliers).</param>
        public static int MultipleChoice(ScoringConfig scoring, bool correct, long ms, long windowMs, double multiplier)
        {
            if (!correct || windowMs <= 0) return 0;
            long t = ClampMs(ms, windowMs);
            // Rounded to 6 decimals first so a value that is a midpoint on paper (875) is not 874.9999999 in floating point.
            double raw = Math.Round(scoring.MaxPoints * (1.0 - scoring.SpeedPenalty * t / windowMs), FloatDecimals);
            double rounded = RoundTo(raw, scoring.RoundTo);
            double points = Math.Round(rounded * multiplier, MidpointRounding.AwayFromZero);
            return points <= 0 ? 0 : (int)points;
        }

        /// <summary>
        /// Closest value distance points (docs/03_rules.md, D83, D156). With d = |guess - answer|, the exact band
        /// e = exactShare x tolerance and the zero point z = zeroAtTolerances x tolerance: d up to e scores maxPoints; from
        /// there the points fall in a straight line to 0 at z; d at z or beyond scores 0. Rounded to the nearest
        /// scoring.roundTo before the multiplier. No speed component. The closest bonus is apart (<see cref="ClosestBonus"/>).
        /// A guess can score beyond the tolerance without being correct (<see cref="ClosestValueCorrect"/>).
        /// </summary>
        public static int ClosestValue(ScoringConfig scoring, ClosestValueConfig closest, double guess, double answer,
            double tolerance, double multiplier)
        {
            if (tolerance <= 0) return 0;
            double d = Distance(guess, answer);
            double exact = Math.Round(closest.ExactShare * tolerance, FloatDecimals);
            double zero = Math.Round(closest.ZeroAtTolerances * tolerance, FloatDecimals);
            double raw;
            if (d <= exact) raw = scoring.MaxPoints;
            else if (d >= zero || zero <= exact) raw = 0;
            else raw = Math.Round(scoring.MaxPoints * (zero - d) / (zero - exact), FloatDecimals);

            double rounded = RoundTo(raw, scoring.RoundTo);
            double points = Math.Round(rounded * multiplier, MidpointRounding.AwayFromZero);
            return points <= 0 ? 0 : (int)points;
        }

        /// <summary>A closest value guess is correct when it lies within the tolerance of the answer, edge included (D83).</summary>
        public static bool ClosestValueCorrect(double guess, double answer, double tolerance)
        {
            return tolerance > 0 && Distance(guess, answer) <= Math.Round(tolerance, FloatDecimals);
        }

        /// <summary>|guess - answer|, rounded to 6 decimals so slider steps like 0.1 compare cleanly.</summary>
        public static double Distance(double guess, double answer)
        {
            return Math.Round(Math.Abs(guess - answer), FloatDecimals);
        }

        /// <summary>The closest bonus (closestValue.closestBonus) times the round multiplier, rounded to a whole point.</summary>
        public static int ClosestBonus(ClosestValueConfig config, double multiplier)
        {
            return (int)Math.Round(config.ClosestBonus * multiplier, MidpointRounding.AwayFromZero);
        }

        /// <summary>The phone's ms clamped to 0..windowMs (PROTOCOL.md section 6).</summary>
        public static long ClampMs(long ms, long windowMs)
        {
            if (ms < 0) return 0;
            return ms > windowMs ? windowMs : ms;
        }

        /// <summary>Rounds to the nearest multiple of step, midpoints away from zero.</summary>
        public static double RoundTo(double value, int step)
        {
            if (step <= 0) return Math.Round(value, MidpointRounding.AwayFromZero);
            return Math.Round(value / step, MidpointRounding.AwayFromZero) * step;
        }
    }
}
