using System.Collections.Generic;

namespace FamilyQuiz.Audio
{
    /// <summary>
    /// One background music track (decisions/0019, D113): its file under Resources/Music and the credit its license
    /// asks for. Every file is in the license log of docs/12_audio.md and in Assets/Audio/Placeholder/LICENSES.md.
    /// </summary>
    public sealed class MusicTrack
    {
        public MusicTrack(string resource, string title, string author, string license)
        {
            Resource = resource;
            Title = title;
            Author = author;
            License = license;
        }

        /// <summary>The clip's name under a Resources/Music folder, without extension.</summary>
        public string Resource { get; }

        public string Title { get; }
        public string Author { get; }
        public string License { get; }

        private const string MacLeod = "Kevin MacLeod (incompetech.com)";
        private const string CcBy4 = "CC BY 4.0";

        /// <summary>The background playlist: five relaxed instrumental jazz tracks, about 22 minutes before a repeat.</summary>
        public static readonly IReadOnlyList<MusicTrack> All = new[]
        {
            new MusicTrack("music_backbay_lounge", "Backbay Lounge", MacLeod, CcBy4),
            new MusicTrack("music_bossa_antigua", "Bossa Antigua", MacLeod, CcBy4),
            new MusicTrack("music_lobby_time", "Lobby Time", MacLeod, CcBy4),
            new MusicTrack("music_jazz_brunch", "Jazz Brunch", MacLeod, CcBy4),
            new MusicTrack("music_george_street_shuffle", "George Street Shuffle", MacLeod, CcBy4)
        };
    }
}
