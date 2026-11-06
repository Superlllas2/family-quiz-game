using System;
using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "sfx": the TV's sound effects and the hosts' gibberish voices (decisions/0020, D120, D121).</summary>
    public sealed class SfxConfig
    {
        /// <summary>The voice styles that have clips (Resources/Voices/voice_&lt;style&gt;_&lt;host&gt;), and none.</summary>
        public static readonly string[] Voices = { "none", "a_animalese", "b_blips", "c_robot", "d_wahwah", "e_bubbles" };

        /// <summary>The settings panel slider's default, 0 to 1, until the laptop remembers its own (tv_audio.json).</summary>
        [JsonProperty("volume", Required = Required.Always)] public double Volume { get; set; }

        /// <summary>The hosts' voice style while a bubble speaks, or none.</summary>
        [JsonProperty("hostVoice", Required = Required.Always)] public string HostVoice { get; set; }

        public static bool IsVoice(string style)
        {
            return Array.IndexOf(Voices, style) >= 0;
        }
    }
}
