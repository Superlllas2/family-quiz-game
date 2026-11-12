using System;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// How bright the studio is on screen (D95, D160, D161). Plain C#. The bake has the studio fully lit; before the
    /// show (light level 0) the post exposure is staging.lobby.darkStops lower while the guest and host keys are raised
    /// as much, so the seats keep their light. The Graphics panel's Brightness adds its stops on top, to the studio
    /// only, never the UI.
    /// </summary>
    public static class StudioExposure
    {
        /// <param name="baseExposure">The film look's own post exposure.</param>
        /// <param name="lightLevel">0 dark (the Lobby) to 1 fully lit (ShotPlan.LightLevel).</param>
        /// <param name="darkStops">staging.lobby.darkStops.</param>
        /// <param name="brightness">The Graphics panel's Brightness, in stops.</param>
        public static double PostExposure(double baseExposure, double lightLevel, double darkStops, double brightness)
        {
            return baseExposure + brightness - darkStops * (1.0 - Clamp01(lightLevel));
        }

        /// <summary>How much the guest and host keys are raised at this light level.</summary>
        public static double KeyBoost(double lightLevel, double darkStops)
        {
            return Math.Pow(2.0, darkStops * (1.0 - Clamp01(lightLevel)));
        }

        private static double Clamp01(double v)
        {
            return v < 0 ? 0 : v > 1 ? 1 : v;
        }
    }
}
