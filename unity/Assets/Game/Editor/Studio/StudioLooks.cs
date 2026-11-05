using UnityEngine;

namespace FamilyQuiz.EditorTools.Studio
{
    /// <summary>The three lighting setups of 0015, all original designs (D49): warm late night, retro 70s, modern night.</summary>
    public static class StudioLooks
    {
        public const string WarmLateNight = "warm";
        public const string Retro70s = "retro";
        public const string ModernNight = "modern";

        public static readonly string[] All = { WarmLateNight, Retro70s, ModernNight };

        public static StudioLook Get(string name)
        {
            switch (name)
            {
                case Retro70s:
                    return new StudioLook
                    {
                        Name = Retro70s,
                        Practical = new Color(1f, 0.55f, 0.24f), PracticalScale = 1.1f,
                        Accent = new Color(1f, 0.36f, 0.12f), AccentIntensity = 15f, Strip = new Color(1f, 0.42f, 0.14f),
                        Window = new Color(0.55f, 0.42f, 0.8f), WindowIntensity = 2f,
                        Fill = new Color(1f, 0.78f, 0.55f), FillIntensity = 2.6f,
                        Key = new Color(1f, 0.82f, 0.6f), StageLamp = new Color(1f, 0.8f, 0.55f),
                        Ambient = new Color(0.035f, 0.025f, 0.02f), CityEmission = 1.3f,
                        Contrast = -6f, Saturation = -8f, Temperature = 16f, Tint = 4f,
                        BloomIntensity = 0.75f, BloomThreshold = 0.95f, Vignette = 0.36f, Grain = 0.45f, LargeGrain = true,
                        ChromaticAberration = 0.12f,
                        SplitShadows = new Color(0.36f, 0.45f, 0.5f), SplitHighlights = new Color(0.62f, 0.5f, 0.38f)
                    };
                case ModernNight:
                    return new StudioLook
                    {
                        Name = ModernNight,
                        Practical = new Color(1f, 0.74f, 0.48f), PracticalScale = 0.9f,
                        Accent = new Color(0.25f, 0.5f, 1f), AccentIntensity = 14f, Strip = new Color(0.4f, 0.7f, 1f),
                        Window = new Color(0.35f, 0.5f, 1f), WindowIntensity = 3.2f,
                        Fill = new Color(0.85f, 0.9f, 1f), FillIntensity = 2.2f,
                        Key = new Color(0.95f, 0.97f, 1f), GuestKeyIntensity = 48f, StageLamp = new Color(0.95f, 0.95f, 1f),
                        Ambient = new Color(0.02f, 0.025f, 0.04f), CityEmission = 2.0f,
                        Contrast = 14f, Saturation = 4f, Temperature = -6f,
                        BloomIntensity = 0.8f, BloomThreshold = 0.9f, Vignette = 0.22f, Grain = 0.12f
                    };
                default:
                    return new StudioLook
                    {
                        // Practicals about 2800 K; the walls' uplights and strips a softer amber, so the wood reads
                        // brown, not red; a neutral fill and key keep faces readable; the window and the shadows a
                        // restrained night blue, for contrast with the warm room (cozy pass, Nov 5, 2026).
                        Name = WarmLateNight,
                        Practical = new Color(1f, 0.69f, 0.42f),
                        Accent = new Color(1f, 0.72f, 0.48f), AccentIntensity = 7f, Strip = new Color(1f, 0.68f, 0.42f),
                        Window = new Color(0.45f, 0.58f, 1f), WindowIntensity = 3.2f,
                        Fill = new Color(0.96f, 0.93f, 0.9f), FillIntensity = 3f,
                        Key = new Color(1f, 0.93f, 0.84f), StageLamp = new Color(1f, 0.9f, 0.76f), StageLampIntensity = 32f,
                        Rim = new Color(0.78f, 0.86f, 1f),
                        Ambient = new Color(0.05f, 0.055f, 0.07f), CityEmission = 1f,
                        Contrast = 5f, Saturation = -6f, Temperature = 1f
                    };
            }
        }
    }
}
