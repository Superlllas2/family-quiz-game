using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Presentation.Graphics
{
    /// <summary>
    /// The TV's graphics settings (decisions/0021, D122 to D124): what the person at the laptop picks in the Graphics
    /// panel, saved on this laptop in tv_graphics.json. A TV only choice, never a game setting, so it never goes through
    /// Core or the protocol. Plain C# (no engine), so presets, cycling and the file are tested without Unity;
    /// <see cref="GraphicsApplier"/> puts it on screen.
    /// </summary>
    public sealed class GraphicsOptions
    {
        public const string FileName = "tv_graphics.json";

        // ------------------------------------------------------------ the choices of each row, in display order

        public static readonly string[] DisplayModes = { "fullscreen", "borderless", "windowed" };
        public static readonly int[] FrameLimits = { 30, 60, 120, 144, 0 };
        public static readonly string[] Presets = { "low", "medium", "high", "ultra" };
        public static readonly string[] TextureLevels = { "low", "medium", "high" };
        public static readonly string[] ShadowLevels = { "off", "low", "medium", "high" };
        public static readonly string[] AntiAliasingModes = { "off", "fxaa", "smaa", "taa", "msaa" };
        public static readonly int[] RenderScales = { 50, 60, 70, 80, 90, 100 };
        public static readonly int[] AnisoLevels = { 0, 2, 4, 8, 16 };

        /// <summary>A preset that no longer matches the detail settings after a change.</summary>
        public const string Custom = "custom";

        // ------------------------------------------------------------ the values

        public string DisplayMode = "borderless";

        /// <summary>
        /// The display mode and resolution were picked in the panel and saved. Until then the app keeps the window it was
        /// started with (its player settings or command line) and never resizes it on start.
        /// </summary>
        public bool WindowChosen;

        /// <summary>0 keeps the desktop resolution.</summary>
        public int Width;

        public int Height;
        public bool VSync = true;

        /// <summary>Frames per second when VSync is off; 0 is unlimited.</summary>
        public int FrameLimit = 60;

        public string Preset = "high";
        public string Textures = "high";
        public string Shadows = "medium";
        public string AntiAliasing = "msaa";
        public int RenderScale = 100;
        public bool AmbientOcclusion = true;
        public bool Bloom = true;
        public bool DepthOfField = true;
        public int Anisotropic = 8;

        public GraphicsOptions Clone()
        {
            return (GraphicsOptions)MemberwiseClone();
        }

        /// <summary>
        /// Sets every detail of a preset (display mode, resolution, VSync and the frame limit stay). Low suits an old
        /// laptop on a big TV; High is the look the studio was lit for (MSAA ×4 with SMAA, ambient occlusion).
        /// </summary>
        public void ApplyPreset(string preset)
        {
            switch (preset)
            {
                case "low":
                    Set(preset, "medium", "off", "fxaa", 70, false, true, false, 2);
                    break;
                case "medium":
                    Set(preset, "high", "low", "smaa", 80, false, true, true, 4);
                    break;
                case "high":
                    Set(preset, "high", "medium", "msaa", 100, true, true, true, 8);
                    break;
                case "ultra":
                    Set(preset, "high", "high", "msaa", 100, true, true, true, 16);
                    break;
            }
        }

        /// <summary>The preset these details match, or custom.</summary>
        public string MatchingPreset()
        {
            foreach (var preset in Presets)
            {
                var probe = Clone();
                probe.ApplyPreset(preset);
                if (probe.Textures == Textures && probe.Shadows == Shadows && probe.AntiAliasing == AntiAliasing
                    && probe.RenderScale == RenderScale && probe.AmbientOcclusion == AmbientOcclusion && probe.Bloom == Bloom
                    && probe.DepthOfField == DepthOfField && probe.Anisotropic == Anisotropic)
                    return preset;
            }

            return Custom;
        }

        private void Set(string preset, string textures, string shadows, string aa, int scale, bool ao, bool bloom, bool dof, int aniso)
        {
            Preset = preset;
            Textures = textures;
            Shadows = shadows;
            AntiAliasing = aa;
            RenderScale = scale;
            AmbientOcclusion = ao;
            Bloom = bloom;
            DepthOfField = dof;
            Anisotropic = aniso;
        }

        // ------------------------------------------------------------ cycling a row

        /// <summary>The next (step 1) or previous (step -1) choice of a list, wrapping round; the first one for an unknown value.</summary>
        public static T Cycle<T>(IReadOnlyList<T> list, T current, int step)
        {
            int i = IndexOf(list, current);
            if (i < 0) return list[0];
            return list[((i + step) % list.Count + list.Count) % list.Count];
        }

        private static int IndexOf<T>(IReadOnlyList<T> list, T value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], value)) return i;
            }

            return -1;
        }

        // ------------------------------------------------------------ the file

        /// <summary>The options saved on this laptop; a missing or broken file, or an unknown value, reads as the default.</summary>
        public static GraphicsOptions Load(string path)
        {
            var o = new GraphicsOptions();
            try
            {
                if (!File.Exists(path)) return o;
                var j = JObject.Parse(File.ReadAllText(path));
                o.DisplayMode = Pick(DisplayModes, (string)j["displayMode"], o.DisplayMode);
                o.WindowChosen = (bool?)j["windowChosen"] ?? false;
                o.Width = Math.Max(0, (int?)j["width"] ?? 0);
                o.Height = Math.Max(0, (int?)j["height"] ?? 0);
                o.VSync = (bool?)j["vsync"] ?? o.VSync;
                o.FrameLimit = Pick(FrameLimits, (int?)j["frameLimit"] ?? o.FrameLimit, o.FrameLimit);
                o.Textures = Pick(TextureLevels, (string)j["textures"], o.Textures);
                o.Shadows = Pick(ShadowLevels, (string)j["shadows"], o.Shadows);
                o.AntiAliasing = Pick(AntiAliasingModes, (string)j["antiAliasing"], o.AntiAliasing);
                o.RenderScale = Pick(RenderScales, (int?)j["renderScale"] ?? o.RenderScale, o.RenderScale);
                o.AmbientOcclusion = (bool?)j["ambientOcclusion"] ?? o.AmbientOcclusion;
                o.Bloom = (bool?)j["bloom"] ?? o.Bloom;
                o.DepthOfField = (bool?)j["depthOfField"] ?? o.DepthOfField;
                o.Anisotropic = Pick(AnisoLevels, (int?)j["anisotropic"] ?? o.Anisotropic, o.Anisotropic);
                o.Preset = o.MatchingPreset();
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is Newtonsoft.Json.JsonException
                                      || e is FormatException || e is InvalidCastException || e is ArgumentException)
            {
                return new GraphicsOptions();
            }

            return o;
        }

        public void Save(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                var j = new JObject
                {
                    ["windowChosen"] = WindowChosen, ["displayMode"] = DisplayMode, ["width"] = Width, ["height"] = Height, ["vsync"] = VSync,
                    ["frameLimit"] = FrameLimit, ["textures"] = Textures, ["shadows"] = Shadows, ["antiAliasing"] = AntiAliasing,
                    ["renderScale"] = RenderScale, ["ambientOcclusion"] = AmbientOcclusion, ["bloom"] = Bloom,
                    ["depthOfField"] = DepthOfField, ["anisotropic"] = Anisotropic
                };
                File.WriteAllText(path, j.ToString());
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                // The settings still apply this evening; they are only not remembered.
            }
        }

        private static T Pick<T>(IReadOnlyList<T> list, T value, T fallback)
        {
            return IndexOf(list, value) >= 0 ? value : fallback;
        }
    }
}
