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
                        Name = WarmLateNight,
                        Practical = new Color(1f, 0.66f, 0.38f),
                        Accent = new Color(1f, 0.55f, 0.25f), Strip = new Color(1f, 0.56f, 0.26f),
                        Window = new Color(0.42f, 0.52f, 0.9f),
                        Fill = new Color(1f, 0.86f, 0.7f),
                        Key = new Color(1f, 0.9f, 0.76f), StageLamp = new Color(1f, 0.88f, 0.7f),
                        Ambient = new Color(0.03f, 0.025f, 0.025f),
                        Contrast = 8f, Saturation = 2f, Temperature = 8f
                    };
            }
        }
    }
}
