# Decision record 0004: staging the TV as a late night show

Date: Nov 2, 2026. Decided by Ilia. The AI building the game never reopens one of these on its own; it asks first. The full concept is in `21_staging.md`. Amended by `0005_staging_answers.md`: D52 confirms both hosts at the desk, D55 replaces the guest variants detail with a character roster, D56 lets staging extras go above 0 after the first playtest. Amended by `0006_art_direction.md`: D62 makes everything realistic except the guests, who are cartoonish and may be mixed media, replacing "characters stylized with believable proportions" in D45. This record was asked for as `0003_staging.md`; number 0003 was already taken by `0003_3d_and_idle_limit.md`.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D44 | What the TV shows | A 3D late night talk show set: a guest area with a sofa and armchairs, the host desk, a curtain stage with stage lamps, and a small live band (drums, bass, keys, maybe guitar). The question appears full screen on the stage; the leaderboard is a flat overlay | The "3D studio" of D43 is now this set |
| D45 | Look | Not plastic, not glossy, not toy like. Matte, slightly textured materials (wood, fabric, worn floor), warm practical lights, soft shadows, a cinematic TV broadcast feel. Characters stylized with believable proportions and fabrics | The six 3D directions page and its Vinyl Toy Show recommendation (never logged as a decision) |
| D46 | Players on the TV | Every player is a guest character in a seat. In the Lobby a guest appears in a free seat with a pop sound and a name tag. On Reveal every guest raises a card with their answer (A to D, or their number); right answers react happily, wrong ones slump | |
| D47 | Music | The live band on the set plays the music cues: lobby loop, suspense loop with a rising last 5 s, right stinger, wrong stinger, round end, final race, winner | |
| D48 | Staging durations | Every staging duration, and every timer in 02 and 03, is a suggested starting value to tune in playtests, not a fixed rule. Staging may lengthen a phase, only through config keys; the event log records how long each phase really took | D42 still holds: the staging extras are 0 for game night 1, and raising one above 0 needs a decision first |
| D49 | Set design | Original. Inspired by Russian late night TV, never copying a real show's set, logo, name or signature set pieces | |
| D50 | Lobby greeting | New host line trigger `playerJoined`: a host greets each new player in the Lobby | |
| D51 | Game night 1 | Only a grey box version (capsules in chairs, a box stage, a desk, two camera shots, colored answer cards, no music), as a stretch goal after every Day 4 check passes | |

## Details added while applying these (not yet confirmed by Ilia)

| Detail | Where |
| --- | --- |
| Shot names and the shot per phase (`Establish`, `Wide`, `Desk`, `Curtain`, `Stage`, `Band`), hard cuts only, no cut during Answer, host lines never force a cut | 21 |
| Staging can lengthen only QuestionShow, Reveal, Scores and RoundEnd (`staging.extraSeconds.*`); the core adds the extra to the budget, Presentation never waits for an animation | 21, 03, config |
| `phaseStarted` gains `stagingExtraMs`; its `budgetMs` includes the extra | 15 |
| Seats: a four seat sofa and two armchairs on each side; first free seat from the middle of the sofa outward; players keep their seat through Play again and Back to lobby | 21 |
| Guest variants: outfit color from the avatar color, one accessory per avatar shape. Replaced by D55 (character roster) | 21 |
| Both hosts sit at the desk; the first `gameStart` line of the Intro is the opening line (the Intro rule of D31 is unchanged) | 21 |
| The suspense riser follows the latest personal deadline of the players still answering, and starts at once on last call | 21 |
| Stinger rule: right when at least half of those who answered were right | 21 |
| The curtain closes off camera at the start of Reveal, so it opens again for every question at no time cost | 21 |
| `playerJoined` fires only for a new player, not for a rejoin or for players kept by Play again or Back to lobby | 15 |
| The grey box uses two plain cameras, no Cinemachine | 21, 13 |
| In the spec kit, Staging takes number 21 and the Decisions log moves to 22 | 00 |
