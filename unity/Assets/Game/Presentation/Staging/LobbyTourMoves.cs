using System.Collections.Generic;
using UnityEngine;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// The camera moves of the Lobby tour (D160): slow glides around the dim studio, built from the shot markers the
    /// studio builder already places (StudioRig), so the tour follows the set when its layout changes. In tour order,
    /// each move in another part of the studio than the one before it (and the last than the first): the seats, a wide
    /// from the back, the desk, the curtain stage, the wide on the guests, the band. A move whose markers are missing is
    /// left out. Shared by StudioStage and the editor's StudioCapture.
    /// </summary>
    public static class LobbyTourMoves
    {
        /// <param name="rig">The studio's shot markers and the tour's distances.</param>
        /// <param name="band">The band shot's pose (rig.BandShot, or the CAM_BandPreview marker of an older scene); may be null.</param>
        public static List<TourMove> Build(StudioRig rig, Transform band)
        {
            var moves = new List<TourMove>();
            var seats = SeatCentre(rig);

            // The guests' seats, a slow dolly along the sofa.
            if (rig.SeatsShot != null)
                moves.Add(Truck("Seats", rig.SeatsShot, rig.SeatsFieldOfView, seats, rig, 1f, 0f));

            // A wide from the back of the studio, the camera rising slowly towards the establishing shot's start.
            if (rig.EstablishEnd != null && rig.EstablishStart != null)
            {
                var end = Vector3.Lerp(rig.EstablishEnd.position, rig.EstablishStart.position, rig.TourBackRise);
                var endLook = Quaternion.Slerp(rig.EstablishEnd.rotation, rig.EstablishStart.rotation, rig.TourBackRise);
                moves.Add(new TourMove("WideBack", rig.EstablishEnd.position, rig.EstablishEnd.rotation, end, endLook,
                    rig.EstablishFieldOfView, seats));
            }

            // The desk: both hosts' places from behind the host solo cameras, pushing in.
            if (rig.HostSoloKonstantin != null && rig.HostSoloIlja != null && rig.HostKonstantin != null && rig.HostIlja != null)
            {
                var solo = (rig.HostSoloKonstantin.position + rig.HostSoloIlja.position) * 0.5f;
                var forward = Flat(rig.HostSoloKonstantin.forward + rig.HostSoloIlja.forward);
                var hosts = (rig.HostKonstantin.position + rig.HostIlja.position) * 0.5f;
                var subject = new Vector3(hosts.x, solo.y - 0.3f, hosts.z);
                var start = solo - forward * rig.TourDeskPullBackMetres;
                var end = start + forward * rig.TourPushMetres + Right(forward) * (rig.TourTruckMetres * 0.25f);
                moves.Add(new TourMove("Desk", start, Quaternion.LookRotation(subject - start), end, Quaternion.LookRotation(subject - end),
                    rig.TourDeskFieldOfView, subject));
            }

            // The curtain stage, sideways with a little push in.
            if (rig.CurtainShot != null)
            {
                var curtain = rig.CurtainLeft != null && rig.CurtainRight != null
                    ? (rig.CurtainLeft.position + rig.CurtainRight.position) * 0.5f
                    : rig.CurtainShot.position + rig.CurtainShot.forward * 8f;
                moves.Add(Truck("Stage", rig.CurtainShot, rig.CurtainFieldOfView, curtain, rig, 1f, 0.5f));
            }

            // The wide on the guests, the other way along.
            if (rig.WideShot != null)
                moves.Add(Truck("Wide", rig.WideShot, rig.WideFieldOfView, seats, rig, -1f, 0f));

            // The band, pushing in slowly.
            if (band != null)
            {
                var forward = band.forward;
                var start = band.position - Right(Flat(forward)) * (rig.TourTruckMetres * 0.25f);
                var end = band.position + forward * rig.TourPushMetres + Right(Flat(forward)) * (rig.TourTruckMetres * 0.25f);
                var subject = band.position + forward * 7f;
                moves.Add(new TourMove("Band", start, band.rotation, end, band.rotation, rig.TourBandFieldOfView, subject));
            }

            return moves;
        }

        /// <summary>
        /// A dolly across a shot: from one side to the other (direction 1 left to right, -1 right to left), turning a
        /// little towards the middle, with push of TourPushMetres forward over the move.
        /// </summary>
        private static TourMove Truck(string name, Transform anchor, float fov, Vector3 subject, StudioRig rig, float direction, float push)
        {
            var right = anchor.right * (rig.TourTruckMetres * 0.5f * direction);
            var start = anchor.position - right;
            var end = anchor.position + right + anchor.forward * (rig.TourPushMetres * push);
            float pan = rig.TourPanDegrees * 0.5f * direction;
            var startLook = Quaternion.AngleAxis(pan, Vector3.up) * anchor.rotation;
            var endLook = Quaternion.AngleAxis(-pan, Vector3.up) * anchor.rotation;
            return new TourMove(name, start, startLook, end, endLook, fov, subject);
        }

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 1e-6f ? v.normalized : Vector3.forward;
        }

        private static Vector3 Right(Vector3 flatForward)
        {
            return Vector3.Cross(Vector3.up, flatForward).normalized;
        }

        public static Vector3 SeatCentre(StudioRig rig)
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
    }
}
