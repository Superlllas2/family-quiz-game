# Decision record 0016: the Lobby shot, the question board and the set pieces

Date: Nov 4, 2026. Decided by Ilia after a review of the reference pictures. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D91 | What the Lobby shows | Only the seats: a new `Seats` shot, with the rest of the studio dark. Players see the whole set for the first time when the show starts, in the Intro's `Establish` shot, as the studio lights come up | `Wide` for Lobby and StartCountdown in `21_staging.md` |
| D92 | Where the question appears | The curtain opens on a plain flat board lit by the stage lamps. The question is the flat overlay placed inside the board, never text drawn on the board in 3D | "The question panel appears full screen on the stage" in `21_staging.md` |
| D93 | Signature pieces from the reference footage | Removed and replaced by original pieces: no bulb arch around the curtain, no logo on the floor, no recognizable Moscow skyline, no stacked block desk, no teal tufted sofa. The table is in `21_staging.md` | Adds detail to D49 |
| D94 | New config key | `staging.intro.lightsUpSeconds`, how long the studio lights take to come up in `Establish`. Ilia left the value to the AI: 1.5 s, a **guess** reviewed after game night 1 like every staging duration | |
| D95 | How the lights come up | The bake has the studio fully lit. `Seats` darkens it with post processing (exposure and a strong vignette) while the seat lamps stay on; `Establish` lifts that over `staging.intro.lightsUpSeconds` | |
| D96 | Shots for game night 1 | `Seats`, `Establish`, `HostSolo`, `Stage` and `Wide`. No `Desk` or `Band` shot yet (cube hosts, no band members); `Wide` plays where the shot list names them, and `HostSolo` replaces `Desk` in the Intro | 0015's silence on shots |
| D97 | Host solo | New shot `HostSolo`: one host alone, close, for rule explanations and jokes. Ilia asked for it and left the details to the AI; they are in `21_staging.md` | |
| D98 | Guest props | Capsules with answer cards first; the 16 signature props follow once everything else works | |
| D99 | The look | Compare the three lighting setups of 0015 in Unity. Ilia left the pick to the AI; the screenshots of all three go to Ilia with the pick, and Ilia can overrule it | |

## Details added while applying this (confirmed by Ilia on Nov 4, 2026, unless marked)

| Detail | Where |
| --- | --- |
| StartCountdown stays in `Seats` with the studio dark, so the lights come up on "go" at the start of the Intro | 21 |
| The question overlay sits inside the board with a margin, so the slow push in during Answer only makes the board grow around the text; the text never crosses the board's edge | 21 |
| Layout seen from the audience, left to right: guest area in front of the windows, desk, curtain stage, band riser (from `Concept of wide angle.png`, a map of the studio, not a camera view). **Amended Nov 4, 2026 by Ilia:** not in one line but a bay, like the reference pictures: guests and desk along the window wall, a 45 degree turn to the curtain wall, another to the band, so the studio is compact and cosy | 21 |
| The set fabric is muted dark green or brown so the 16 signature colors stand out | 21 |
| Lighting stays as written in 0015: Blender only models, the lighting is done and baked in Unity (Ilia left this to the AI) | 0015 |
| `HostSolo` plays only in the Intro and FinalIntro, never where players need the set to read the game (not yet confirmed) | 21 |
| The Intro opens on the hosts: `HostSolo` for each `gameStart` line in the dark studio, then `Establish` with the lights coming up, then `Wide`. Both lines start in the first 7 s (D58 keeps `gameStart` at the Intro start), so the establishing shot cannot come first without hiding the host solo | 21 |
| `Curtain` stays for TopicVote, so game night 1 has six shots (AI's pick; Ilia put this question off until later, Nov 4, 2026) | 21 |
| The look picked under D99: warm late night (Ilia saw the screenshots of all three and kept it) | 21 |
| Stand ins until the props come (D98): capsules with eyes; the hosts are cubes with eyes, Konstantin dark red, Ilja navy (fine for now, revisited with the real hosts) | 21 |
| The board is a dark matte felt, so the light question overlay reads on it; it is 16:9 so the `Stage` shot fills the screen with it | 21 |
| The 3D set adds no time: every staging extra stays 0 and the core is untouched apart from reading the new config key; a full scripted game passed with it on | 21 |
| Photo scanned CC0 textures from Poly Haven for the set's wood, fabric, leather, velvet and carpet, downloaded by `tools/fetch_textures.mjs` into `Assets/Game/Art/Textures/Scanned` with a license log there; about 58 MB in the repo with the masks and grey copies made from them, no large file storage (authorized by Ilia, Nov 5, 2026) | 21 |
| The on/off switch of 0015 is a toggle in the TV settings panel: on shows the 3D set behind the overlay, off shows the current flat views (confirmed by Ilia) | 21, Presentation |
| **Amended Nov 5, 2026 by Ilia:** the curtain is a deep, muted teal velvet instead of deep green (still no bulb arch, which is what D93 removes) | 21 |
