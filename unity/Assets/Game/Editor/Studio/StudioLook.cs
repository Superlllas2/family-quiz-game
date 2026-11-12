using UnityEngine;

namespace FamilyQuiz.EditorTools.Studio
{
    /// <summary>One lighting setup of the studio (0015: three are compared, D99): light colours and strengths and the film look.</summary>
    public sealed class StudioLook
    {
        public string Name;

        // Practical lights: pendants, lamps, sconces, shelf lights (baked).
        public Color Practical;
        public float PracticalScale = 1f;
        // Uplights grazing the back wall and the glow strips.
        public Color Accent;
        public float AccentIntensity = 12f;
        public Color Strip;
        // The evening light through the window and the soft fill from the audience side (baked).
        public Color Window;
        public float WindowIntensity = 2.5f;
        public Color Fill;
        public float FillIntensity = 2.4f;
        // Realtime keys for the guests and hosts, and the stage lamps on the board.
        public Color Key;
        public float GuestKeyIntensity = 45f;
        public float HostKeyIntensity = 26f;
        public Color StageLamp;
        public float StageLampIntensity = 32f;
        // Realtime rim lights from behind the guests and the hosts, to separate them from the back wall.
        public Color Rim = new Color(0.85f, 0.88f, 1f);
        public float RimIntensity = 16f;
        // The flat ambient (environment) light, baked into the lightmaps and the probes: the floor under every shadow.
        public Color Ambient;
        // Soft fill from above (D161): broad, dim, baked area lights under the ceiling over the seats, the desk, the
        // curtain stage and the band, so the shadows read on a TV while the warm practical pools stay the brightest
        // thing in the room. 0 leaves them out. Size in metres, height of the lights above the floor in metres.
        public float CeilingFillIntensity;
        public Vector2 CeilingFillSize = new Vector2(6f, 4f);
        public float CeilingFillHeight = 5.2f;
        // How much the bounced light counts in the bake (LightingSettings.indirectScale): more lifts the shadows evenly
        // in the colours of the room.
        public float IndirectScale = 1.1f;
        public float CityEmission = 1.6f;

        // Film look (URP post processing).
        public float Contrast;
        public float Saturation;
        public float Temperature;
        public float Tint;
        public float BloomIntensity = 0.5f;
        public float BloomThreshold = 1f;
        public float Vignette = 0.28f;
        public float Grain = 0.22f;
        public bool LargeGrain;
        public float ChromaticAberration;
        public Color SplitShadows = Color.grey;
        public Color SplitHighlights = Color.grey;
        public float PostExposure = 1.1f;
        // Haze (exponential fog) and the light shafts seen in it (build_studio.py, beam()), and the lens's smudges.
        public float FogDensity;
        public Color FogColor = Color.black;
        public float BeamIntensity = 0.06f;
        public float LensDirt = 1.2f;
    }
}
