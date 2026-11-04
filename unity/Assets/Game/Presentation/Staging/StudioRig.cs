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
