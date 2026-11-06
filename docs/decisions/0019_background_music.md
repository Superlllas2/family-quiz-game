# Decision record 0019: background music

Date: Nov 6, 2026. Ilia asked for music in an endless loop, with rights for distribution, jazz, instrumental, several different tunes (about 5) so they do not repeat often, and a music loudness slider in the settings. This brings placeholder music into game night 1, which `19_acceptance.md` listed as out of scope; the rules for placeholder files in `12_audio.md` (D57) apply. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D113 | The music | Five relaxed instrumental jazz tracks by Kevin MacLeod (incompetech.com), CC BY 4.0: Backbay Lounge, Bossa Antigua, Lobby Time, Jazz Brunch, George Street Shuffle (about 22 minutes in all). An endless shuffled loop on the TV: every track plays once before any plays again, never the same track twice in a row, a crossfade of `music.fadeSeconds` (2 s) between tracks. It starts with the app and plays in every phase and through a pause. It never reacts to the game (no ducking, no cues) | "No music" for game night 1 (`19_acceptance.md`, D51) |
| D114 | The volume | A slider in the TV settings panel, "Музыка / Music", 0 to 100 %, at once; 0 mutes. Default `music.volume` (35 %). Remembered on this laptop in `<persistentDataPath>/tv_audio.json`, like the 3D studio toggle: a TV only choice, not a game setting, never logged | |
| D115 | The credit | CC BY needs a credit. The settings panel's last line names the track playing: "Играет: «Lobby Time», Kevin MacLeod (incompetech.com), CC BY 4.0". Every file is in the license log of `12_audio.md` and in `unity/Assets/Audio/Placeholder/LICENSES.md` | |

Files: `unity/Assets/Audio/Placeholder/Resources/Music/music_<title>.mp3` (Git LFS), imported as streamed Vorbis (`Editor/MusicImport.cs`). Code: `Assets/Game/Audio` (`MusicPlayer`, `MusicPlaylist`, `MusicTrack`, `MusicVolumeFile`), started by the composition root.
