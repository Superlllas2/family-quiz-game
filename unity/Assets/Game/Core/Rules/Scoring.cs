using System;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>Points per answer (docs/03_rules.md, "Scoring"). Pure functions of the config and the answer.</summary>
    public static class Scoring
    {
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
            double raw = scoring.MaxPoints * (1.0 - scoring.SpeedPenalty * t / windowMs);
            double rounded = RoundTo(raw, scoring.RoundTo);
            double points = Math.Round(rounded * multiplier, MidpointRounding.AwayFromZero);
            return points <= 0 ? 0 : (int)points;
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
