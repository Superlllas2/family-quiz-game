# Sound effect licenses

Every file under `Resources/` (decisions/0020, D120, D121; the same rows as the license log in `docs/12_audio.md`).

## Sound effects: Kenney.nl, CC0

All from Kenney Vleugels (https://kenney.nl), Creative Commons Zero (public domain, https://creativecommons.org/publicdomain/zero/1.0/): free for personal and commercial use, credit not required. Downloaded Nov 6, 2026.

| File | Cue | Pack | Original file |
| --- | --- | --- | --- |
| `Resources/Sfx/sfx_tick.ogg` | The answer clock's last 5 seconds | Interface Sounds 1.0 | tick_004.ogg |
| `Resources/Sfx/sfx_lock.ogg` | A player locks in an answer | Casino Audio 1.1 | chip-lay-1.ogg |
| `Resources/Sfx/sfx_card.ogg` | A question card appears | Casino Audio 1.1 | card-place-1.ogg |
| `Resources/Sfx/sfx_right.ogg` | Reveal, someone was right | Interface Sounds 1.0 | confirmation_001.ogg |
| `Resources/Sfx/sfx_nobody.ogg` | Reveal, nobody was right | Interface Sounds 1.0 | bong_001.ogg |
| `Resources/Sfx/sfx_scores.ogg` | Scores | Casino Audio 1.1 | chips-stack-3.ogg |
| `Resources/Sfx/sfx_vote.ogg` | A topic vote | Casino Audio 1.1 | chip-lay-2.ogg |
| `Resources/Sfx/sfx_topic.ogg` | The topic is chosen | Casino Audio 1.1 | card-fan-1.ogg |
| `Resources/Sfx/sfx_countdown.ogg` | The start countdown | Casino Audio 1.1 | dice-shake-2.ogg |
| `Resources/Sfx/sfx_start.ogg` | The game starts | Casino Audio 1.1 | dice-throw-1.ogg |
| `Resources/Sfx/sfx_join.ogg` | A player joins | Interface Sounds 1.0 | pluck_002.ogg |
| `Resources/Sfx/sfx_round_end.ogg` | Round end | Music Jingles | Sax jingles/jingles_SAX03.ogg |
| `Resources/Sfx/sfx_winner.ogg` | Results | Music Jingles | Sax jingles/jingles_SAX07.ogg |
| `Resources/Sfx/sfx_race_step.ogg` | A race reveal | Casino Audio 1.1 | card-slide-2.ogg |
| `Resources/Sfx/sfx_freeze.ogg` | Freeze lands | Impact Sounds | impactGlass_light_000.ogg |
| `Resources/Sfx/sfx_slime.ogg` | Slime lands | Impact Sounds | impactSoft_heavy_000.ogg |
| `Resources/Sfx/sfx_fog.ogg` | Fog lands | Digital Audio | lowDown.ogg |
| `Resources/Sfx/sfx_pause.ogg` | Pause menu opens | Interface Sounds 1.0 | toggle_001.ogg |
| `Resources/Sfx/sfx_resume.ogg` | The game goes on | Interface Sounds 1.0 | toggle_002.ogg |

## Host voices: made by the project

`Resources/Voices/voice_<style>_<host>.wav` are synthesized by `tools/audio_host_voices.py` (no recordings, no third party material), owned by the project. Regenerate with `VOICE_SECONDS=8 python tools/audio_host_voices.py unity/Assets/Audio/Sfx/Resources/Voices`.
