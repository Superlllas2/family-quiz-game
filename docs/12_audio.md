# 12 Audio direction and sound list

Status: Partial (Nov 2, 2026; `decisions/0005_staging_answers.md`, D57; background music from `decisions/0019_background_music.md`, Nov 6, 2026). Game night 1 has placeholder background music (below) and no sounds on the TV beyond what a stretch goal adds. The music cues the band plays are listed in `21_staging.md`.

## Music

**Open:** the final music is decided later: composed, commissioned or licensed, its style, reference tracks, and mixing rules (ducking under host voices).

**Background music (D113 to D115):** five instrumental jazz tracks in an endless shuffled loop with a 2 s crossfade, from app start, in every phase; never the same track twice in a row. A volume slider in the TV settings panel (default `music.volume`, remembered on the laptop); the panel's last line credits the track playing.

**For the prototype:** free placeholder loops and stingers, one per cue in `21_staging.md`, so the staging can be judged with sound. They are replaced before anything is sold or shown publicly.

### Where placeholder loops come from

| Source | License | Attribution | Good for |
| --- | --- | --- | --- |
| Kenney audio packs (kenney.nl), for example "Music Jingles" and "Interface Sounds" | CC0 | Not required | Stingers, pops, short jingles |
| Incompetech, Kevin MacLeod (incompetech.com) | CC BY 4.0 | Required: title, "Kevin MacLeod (incompetech.com)", license name and link | Jazz and funk loops for the lobby, suspense beds |
| Pixabay Music (pixabay.com/music) | Pixabay Content License | Not required | Lobby and suspense loops, risers |
| Freesound (freesound.org) | Set per sound: CC0, CC BY 4.0 or CC BY-NC 4.0 | Required for CC BY | Stingers, drum fills, the curtain and pop sounds |
| OpenGameArt (opengameart.org) | Set per file: CC0, CC BY, CC BY-SA, GPL and others | Depends on the file | Loops and jingles |
| Free Music Archive (freemusicarchive.org) | Set per track, mostly Creative Commons | Depends on the track | Longer loops |

### Rules for placeholder files

- Use only files under CC0, CC BY or the Pixabay Content License. Never NC (non commercial), ND (no derivatives), SA (share alike) or GPL, so nothing has to be removed in a hurry if the game is sold.
- Check the license on the file's own page when downloading; on Freesound, OpenGameArt and Free Music Archive it differs per file.
- Never use music recorded from a real TV show, and never rip audio from video sites.
- Record every file in the license log below in the same commit that adds it.
- Placeholder files go into `unity/Assets/Audio/Placeholder/`, named after their cue (for example `lobby_loop.ogg`, `stinger_right.ogg`).

### License log

| File | Cue | Source URL | Author | License | Attribution text | Added |
| --- | --- | --- | --- | --- | --- | --- |
| `Resources/Music/music_backbay_lounge.mp3` | Background loop (D113) | https://incompetech.com/music/royalty-free/mp3-royaltyfree/Backbay%20Lounge.mp3 | Kevin MacLeod | CC BY 4.0 | "Backbay Lounge" Kevin MacLeod (incompetech.com), Licensed under Creative Commons: By Attribution 4.0, https://creativecommons.org/licenses/by/4.0/ | Nov 6, 2026 |
| `Resources/Music/music_bossa_antigua.mp3` | Background loop (D113) | https://incompetech.com/music/royalty-free/mp3-royaltyfree/Bossa%20Antigua.mp3 | Kevin MacLeod | CC BY 4.0 | "Bossa Antigua" Kevin MacLeod (incompetech.com), Licensed under Creative Commons: By Attribution 4.0, https://creativecommons.org/licenses/by/4.0/ | Nov 6, 2026 |
| `Resources/Music/music_lobby_time.mp3` | Background loop (D113) | https://incompetech.com/music/royalty-free/mp3-royaltyfree/Lobby%20Time.mp3 | Kevin MacLeod | CC BY 4.0 | "Lobby Time" Kevin MacLeod (incompetech.com), Licensed under Creative Commons: By Attribution 4.0, https://creativecommons.org/licenses/by/4.0/ | Nov 6, 2026 |
| `Resources/Music/music_jazz_brunch.mp3` | Background loop (D113) | https://incompetech.com/music/royalty-free/mp3-royaltyfree/Jazz%20Brunch.mp3 | Kevin MacLeod | CC BY 4.0 | "Jazz Brunch" Kevin MacLeod (incompetech.com), Licensed under Creative Commons: By Attribution 4.0, https://creativecommons.org/licenses/by/4.0/ | Nov 6, 2026 |
| `Resources/Music/music_george_street_shuffle.mp3` | Background loop (D113) | https://incompetech.com/music/royalty-free/mp3-royaltyfree/George%20Street%20Shuffle.mp3 | Kevin MacLeod | CC BY 4.0 | "George Street Shuffle" Kevin MacLeod (incompetech.com), Licensed under Creative Commons: By Attribution 4.0, https://creativecommons.org/licenses/by/4.0/ | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_tick.ogg` | Answer clock, last 5 s (D120) | https://kenney.nl (Interface Sounds, tick_004.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_lock.ogg` | Lock in (D120) | https://kenney.nl (Casino Audio, chip-lay-1.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_card.ogg` | Question appears (D120) | https://kenney.nl (Casino Audio, card-place-1.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_right.ogg` | Reveal, someone right (D120) | https://kenney.nl (Interface Sounds, confirmation_001.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_nobody.ogg` | Reveal, nobody right (D120) | https://kenney.nl (Interface Sounds, bong_001.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_scores.ogg` | Scores (D120) | https://kenney.nl (Casino Audio, chips-stack-3.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_vote.ogg` | Topic vote (D120) | https://kenney.nl (Casino Audio, chip-lay-2.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_topic.ogg` | Topic chosen (D120) | https://kenney.nl (Casino Audio, card-fan-1.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_countdown.ogg` | Start countdown (D120) | https://kenney.nl (Casino Audio, dice-shake-2.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_start.ogg` | Game start (D120) | https://kenney.nl (Casino Audio, dice-throw-1.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_join.ogg` | Player joins (D120) | https://kenney.nl (Interface Sounds, pluck_002.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_round_end.ogg` | Round end (D120) | https://kenney.nl (Music Jingles, jingles_SAX03.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_winner.ogg` | Results (D120) | https://kenney.nl (Music Jingles, jingles_SAX07.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_race_step.ogg` | Race reveal (D120) | https://kenney.nl (Casino Audio, card-slide-2.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_freeze.ogg` | Freeze lands (D120) | https://kenney.nl (Impact Sounds, impactGlass_light_000.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_slime.ogg` | Slime lands (D120) | https://kenney.nl (Impact Sounds, impactSoft_heavy_000.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_fog.ogg` | Fog lands (D120) | https://kenney.nl (Digital Audio, lowDown.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_pause.ogg` | Pause (D120) | https://kenney.nl (Interface Sounds, toggle_001.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Sfx/sfx_resume.ogg` | Resume (D120) | https://kenney.nl (Interface Sounds, toggle_002.ogg) | Kenney Vleugels | CC0 | Not required | Nov 6, 2026 |
| `Sfx/Resources/Voices/voice_*.wav` | Host voices (D121) | Generated by `tools/audio_host_voices.py` | The project | Own work | None | Nov 6, 2026 |

## Sound effects

**Game night 1 (decisions/0020, D120, D121):** the TV plays the effects in the license log above and, once Ilia picks a style, a gibberish host voice; the volume slider "Звуки" sits beside the music slider. **Open:** the full list with file names and lengths for the finished game. Known so far from `21_staging.md`: the pop when a guest appears in a seat, the curtain opening. The phone plays no sounds (`06_phone_screens.md`).
