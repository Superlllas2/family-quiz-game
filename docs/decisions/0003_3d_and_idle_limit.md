# Decision record 0003: 3D look and the idle limit

Date: Nov 2, 2026. Decided by Ilia. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D42 | Idle limit in simulated games | No stretch without input may last longer than 11 s outside Intro, FinalIntro and Results. The timers stay as they are | The 10 s limit in D35; resolves the open issue in 0002 |
| D43 | Look of the game | Stylized real time 3D in Unity, not photoreal. The hosts become 3D characters. The TV shows the 3D studio with the question UI over it. The phone stays a flat 2D controller | "Not a 3D game" in 01 and D10 (2D portraits). D2's "3D hosts possible later" now applies |

**Amended Nov 2, 2026 by D56 (`0005_staging_answers.md`):** the 11 s limit of D42 is a suggested starting value like every timer, set again whenever timers or staging extras change after a playtest.

## Details added while applying these (not yet confirmed by Ilia)

| Detail | Where |
| --- | --- |
| Game night 1 scope does not change: grey box TV views, no art. They render as UGUI over the camera of the Universal 3D project. The 3D studio and characters come after game night 1 | 01, 13 |
| The art direction is chosen from six 3D options (published as a separate page) and then written up as spec document 10. Superseded by D44 and D45 in `0004_staging.md` | 00 |
| The 11 s limit equals the current timers exactly, so simulated games must end each phase at its exact budget on the fake clock | 19, prompts |
