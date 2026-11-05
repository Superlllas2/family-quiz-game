using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// The lens of each shot (docs/21_staging.md, "Look": shallow depth of field, a broadcast feel): a physical bokeh
    /// focused on what the shot is about, the guests, a host or the curtain, so the city and the far walls go soft
    /// like a real camera's. The Stage shot stays sharp all over: the question is read off the board (D92). Shared by
    /// StudioStage at runtime and the editor's StudioCapture, so the captures show what the TV shows.
    /// </summary>
    public static class StudioLens
    {
        /// <summary>Sets dof for the shot seen from camera; host is the solo host ("ilja" or the other one).</summary>
        public static void Apply(DepthOfField dof, StudioRig rig, Shot shot, Transform camera, string host)
        {
            if (dof == null) return;
            Vector2 lens;
            Vector3 subject;
            switch (shot)
            {
                case Shot.Wide:
                    lens = rig.WideLens;
                    subject = SeatCentre(rig);
                    break;
                case Shot.Seats:
                    lens = rig.SeatsLens;
                    subject = SeatCentre(rig);
                    break;
                case Shot.Establish:
                    lens = rig.EstablishLens;
                    subject = SeatCentre(rig);
                    break;
                case Shot.HostSolo:
                    lens = rig.HostSoloLens;
                    var who = host == "ilja" ? rig.HostIlja : rig.HostKonstantin;
                    subject = who != null ? who.position + Vector3.up * 0.5f : camera.position + camera.forward * 3f;
                    break;
                case Shot.Curtain:
                    lens = rig.CurtainLens;
                    subject = rig.CurtainLeft != null && rig.CurtainRight != null
                        ? (rig.CurtainLeft.position + rig.CurtainRight.position) * 0.5f
                        : camera.position + camera.forward * 8f;
                    break;
                default:
                    dof.mode.Override(DepthOfFieldMode.Off);
                    return;
            }

            // Focus is the subject's depth along the lens axis, not its straight line distance.
            float distance = Mathf.Max(0.5f, Vector3.Dot(subject - camera.position, camera.forward));
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(distance);
            dof.focalLength.Override(lens.x);
            dof.aperture.Override(lens.y);
            dof.bladeCount.Override(7);
            dof.bladeCurvature.Override(0.85f);
            dof.bladeRotation.Override(18f);
        }

        private static Vector3 SeatCentre(StudioRig rig)
        {
            var sum = Vector3.zero;
            int n = 0;
            foreach (var seat in rig.Seats)
            {
                if (seat == null) continue;
                sum += seat.position;
                n++;
            }

            return n > 0 ? sum / n + Vector3.up * 0.4f : Vector3.zero;
        }

        /// <summary>
        /// A slow, small wander of the camera, like an operator holding a shot on a pedestal: never still, never shaky.
        /// seed keeps each shot's wander its own.
        /// </summary>
        public static void Drift(StudioRig rig, ref Vector3 position, ref Quaternion rotation, float time, float seed)
        {
            float t = time * rig.DriftSpeed;
            float Wave(float offset) => (Mathf.PerlinNoise(t + offset, seed) - 0.5f) * 2f;
            var right = rotation * Vector3.right;
            var up = rotation * Vector3.up;
            position += right * (Wave(0f) * rig.DriftMetres) + up * (Wave(17.3f) * rig.DriftMetres * 0.6f);
            rotation = Quaternion.AngleAxis(Wave(41.7f) * rig.DriftDegrees, up) * Quaternion.AngleAxis(Wave(73.1f) * rig.DriftDegrees * 0.7f, right) * rotation;
        }
    }
}
