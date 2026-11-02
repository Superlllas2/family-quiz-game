# 12 Audio direction and sound list

Status: Partial (Nov 2, 2026; `decisions/0005_staging_answers.md`, D57). Game night 1 has no music and no sounds on the TV beyond what a stretch goal adds. The music cues the band plays are listed in `21_staging.md`.

## Music

**Open:** the final music is decided later: composed, commissioned or licensed, its style, reference tracks, and mixing rules (ducking under host voices).

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
| | | | | | | |

## Sound effects

**Open:** the full list with file names and lengths. Known so far from `21_staging.md`: the pop when a guest appears in a seat, the curtain opening. The phone plays no sounds (`06_phone_screens.md`).
