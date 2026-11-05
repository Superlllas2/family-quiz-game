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
        public float WideFieldOfView = 35f;
        public float SeatsFieldOfView = 34f;
        public float EstablishFieldOfView = 42f;
        public float HostSoloFieldOfView = 17f;
        public float CurtainFieldOfView = 38f;
        public float StageFieldOfView = 16f;

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
        /// <summary>How far the studio is darkened at light level 0 (Seats), in exposure stops.</summary>
        public float DarkStops = 2.2f;
    }
}
