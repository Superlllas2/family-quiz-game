using System.Collections.Generic;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The arc of an attack arrow (D149): a quadratic curve from the attacker to the target whose control point sits
    /// over the middle, out on the bulge side. Plain math in canvas units, tested without the engine. Sizes of a TV
    /// drawing, not game numbers (like TvMotion).
    /// </summary>
    public static class ArrowPath
    {
        /// <summary>The bulge of the shortest arrow on lane 0, in canvas units.</summary>
        public const float BaseBulge = 70f;

        /// <summary>How much the bulge grows per unit of distance between the two players.</summary>
        public const float BulgePerUnit = 0.22f;

        /// <summary>How much further each lane arcs.</summary>
        public const float LaneBulge = 55f;

        /// <summary>The space between arrow ends that share one piece.</summary>
        public const float EndGap = 30f;

        /// <summary>How far end number <paramref name="end"/> of <paramref name="ends"/> sits from the piece's anchor, centred.</summary>
        public static float Spread(int end, int ends)
        {
            return ends <= 1 ? 0f : (end - (ends - 1) / 2f) * EndGap;
        }

        public static float Bulge(float distance, int lane)
        {
            return BaseBulge + BulgePerUnit * Mathf.Max(0f, distance) + LaneBulge * Mathf.Max(0, lane);
        }

        /// <summary>segments + 1 points from <paramref name="from"/> to <paramref name="to"/>, arcing toward <paramref name="side"/>.</summary>
        public static List<Vector2> Points(Vector2 from, Vector2 to, Vector2 side, float bulge, int segments)
        {
            int n = Mathf.Max(1, segments);
            var control = (from + to) * 0.5f + side.normalized * bulge;
            var points = new List<Vector2>(n + 1);
            for (int i = 0; i <= n; i++)
            {
                float t = (float)i / n;
                float u = 1f - t;
                points.Add(i == 0 ? from : i == n ? to : u * u * from + 2f * u * t * control + t * t * to);
            }

            return points;
        }
    }
}
