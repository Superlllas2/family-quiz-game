using System;
using System.Collections.Generic;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The TopicTie spin (D153): a highlight hops between the tied doors, slowing down, and stops on the door the seeded
    /// random generator chose, which then stands out for the rest of the phase. Driven by the fraction of the phase gone
    /// (timers.topicTie), so it ends with the phase and never holds it. Presentation timing only (like TvMotion), not a
    /// game rule. Plain C#, tested without the engine.
    /// </summary>
    public static class TopicTieSpin
    {
        /// <summary>How many times the highlight moves before it rests on the winner.</summary>
        public const int Hops = 9;

        /// <summary>The share of the phase the hops take; the winner stands out for the rest.</summary>
        public const float SpinShare = 0.7f;

        /// <summary>The doors lit from the start (hop 0) to the end (hop <paramref name="hops"/>, the chosen door).</summary>
        public static List<int> Doors(IReadOnlyList<int> tied, int chosen, int hops)
        {
            var doors = new List<int>(hops + 1);
            int n = tied != null ? tied.Count : 0;
            int at = n > 0 ? IndexOf(tied, chosen) : -1;
            for (int k = 0; k <= hops; k++)
            {
                if (at < 0) doors.Add(chosen);
                else doors.Add(tied[Mod(at - (hops - k), n)]);
            }

            return doors;
        }

        /// <summary>The hop at this fraction of the phase (0 to 1): the gaps between hops grow, so the spin slows down.</summary>
        public static int HopAt(float fraction)
        {
            float t = SpinShare > 0f ? fraction / SpinShare : 1f;
            if (t >= 1f) return Hops;
            if (t <= 0f) return 0;
            // Hop k comes at SpinShare * (k / Hops)^2 of the phase.
            return Math.Min(Hops, (int)Math.Floor(Hops * Math.Sqrt(t) + 1e-6));
        }

        /// <summary>The door lit at this fraction of the phase; the chosen door when it is not among the tied ones.</summary>
        public static int DoorAt(IReadOnlyList<int> tied, int chosen, float fraction)
        {
            return Doors(tied, chosen, Hops)[HopAt(fraction)];
        }

        private static int IndexOf(IReadOnlyList<int> list, int value)
        {
            for (int i = 0; i < list.Count; i++) if (list[i] == value) return i;
            return -1;
        }

        private static int Mod(int a, int n)
        {
            return ((a % n) + n) % n;
        }
    }
}
