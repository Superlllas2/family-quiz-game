using UnityEngine;
using UnityEngine.Rendering;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// The references the 3D set's runtime code needs, filled in by the editor's studio builder when it builds
    /// Assets/Scenes/Studio.unity (docs/21_staging.md, "Game night 1 version"). Holds no logic.
    /// </summary>
    public sealed class StudioRig : MonoBehaviour
    {
        [Header("Camera and shots")]
        public Camera Camera;
        public Transform SeatsShot;
        public Transform EstablishStart;
        public Transform EstablishEnd;
        public Transform HostSoloKonstantin;
        public Transform HostSoloIlja;
        public Transform CurtainShot;
        public Transform StageShot;
        public Transform StagePushIn;
        public Transform WideShot;
        /// <summary>The band seen from the floor (CAM_BandPreview); one of the Lobby tour's moves. Null in a scene built before D160.</summary>
        public Transform BandShot;
        public float WideFieldOfView = 35f;
        public float SeatsFieldOfView = 34f;
        public float EstablishFieldOfView = 42f;
        public float HostSoloFieldOfView = 17f;
        public float CurtainFieldOfView = 38f;
        public float StageFieldOfView = 16f;

        [Header("Lobby tour (D160, LobbyTourMoves): slow moves around the dim studio")]
        // How far each sideways move travels from start to end, in metres (a dolly across the shot).
        public float TourTruckMetres = 1.2f;
        // How far each push in travels, in metres.
        public float TourPushMetres = 0.9f;
        // How far the camera turns over a move, in degrees, so the set slides past with some parallax.
        public float TourPanDegrees = 4f;
        // The desk move starts this far behind the host solo cameras, so both hosts and the desk are in frame.
        public float TourDeskPullBackMetres = 2.2f;
        // How far the wide from the back rises towards the establishing shot's start (0 none, 1 all the way).
        public float TourBackRise = 0.35f;
        public float TourDeskFieldOfView = 30f;
        public float TourBandFieldOfView = 36f;
        public Vector2 TourLens = new Vector2(50f, 2.8f);

        [Header("Lens (StudioLens): a bokeh focused on each shot's subject")]
        // Focal lengths in mm and f-stops, a little longer and wider open than the field of view alone gives, for the
        // shallow focus of a large sensor camera.
        public Vector2 WideLens = new Vector2(85f, 1.8f);
        public Vector2 SeatsLens = new Vector2(85f, 1.8f);
        public Vector2 EstablishLens = new Vector2(50f, 4f);
        public Vector2 HostSoloLens = new Vector2(135f, 2.8f);
        public Vector2 CurtainLens = new Vector2(70f, 2.8f);
        /// <summary>The operator's slow wander (StudioLens.Drift), in metres, degrees and noise cycles a second.</summary>
        public float DriftMetres = 0.014f;
        public float DriftDegrees = 0.14f;
        public float DriftSpeed = 0.12f;

        [Header("Set pieces")]
        public Transform CurtainLeft;
        public Transform CurtainRight;
        public Transform[] Seats = new Transform[8];
        public Transform HostKonstantin;
        public Transform HostIlja;

        [Header("Lights and look")]
        public Volume Volume;
        public Light GuestKey;
        public Light HostKey;
        public Light[] StageLamps;
        // How far the studio is darkened at light level 0 is staging.lobby.darkStops in content/config.json (D160).
    }
}
