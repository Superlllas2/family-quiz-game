using System.Collections.Generic;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Where things go on the closest value number line (docs/04_formats.md): a value as a fraction of the range, and
    /// lanes so chips whose guesses are closer than a chip width stack above each other instead of covering each other.
    /// Plain C#, so it is tested without the engine.
    /// </summary>
    public static class NumberLineLayout
    {
        private const double Epsilon = 1e-9;

        /// <summary>The value's place from 0 (min) to 1 (max), clamped; the middle when the range is empty.</summary>
        public static double Fraction(double value, double min, double max)
        {
            if (!(max > min)) return 0.5;
            double f = (value - min) / (max - min);
            if (double.IsNaN(f)) return 0.5;
            return f < 0 ? 0 : f > 1 ? 1 : f;
        }

        /// <summary>
        /// A lane (0 is the lowest) for each fraction: going from left to right, each chip takes the lowest lane whose last
        /// chip is at least minGap to its left. Equal fractions keep their input order.
        /// </summary>
        public static int[] Lanes(IReadOnlyList<double> fractions, double minGap)
        {
            var lanes = new int[fractions.Count];
            var order = new List<int>(fractions.Count);
            for (int i = 0; i < fractions.Count; i++) order.Add(i);
            // List.Sort is not stable: break ties by input index.
            order.Sort((a, b) =>
            {
                int byPlace = fractions[a].CompareTo(fractions[b]);
                return byPlace != 0 ? byPlace : a.CompareTo(b);
            });

            var laneEnds = new List<double>();
            foreach (var i in order)
            {
                double f = fractions[i];
                int lane = -1;
                for (int l = 0; l < laneEnds.Count; l++)
                {
                    if (f - laneEnds[l] >= minGap - Epsilon)
                    {
                        lane = l;
                        break;
                    }
                }

                if (lane < 0)
                {
                    lane = laneEnds.Count;
                    laneEnds.Add(f);
                }
                else
                {
                    laneEnds[lane] = f;
                }

                lanes[i] = lane;
            }

            return lanes;
        }

        /// <summary>How many lanes the result of <see cref="Lanes"/> uses.</summary>
        public static int LaneCount(int[] lanes)
        {
            int count = 0;
            foreach (var lane in lanes)
            {
                if (lane + 1 > count) count = lane + 1;
            }

            return count;
        }
    }
}
