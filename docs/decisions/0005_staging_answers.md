# Decision record 0005: answers on the staging concept

Date: Nov 2, 2026. Decided by Ilia, answering the open questions of `0004_staging.md`. The AI building the game never reopens one of these on its own; it asks first.

## Confirmed

These details from `0004_staging.md` are approved: the seat layout and fill order, the curtain closing off camera at the start of Reveal, `playerJoined` greetings dropped by the normal gap rule, and the `stagingExtraMs` field on `phaseStarted`. The push of commit 21a291b is approved after the fact. The guest variants detail (outfit color and accessory from the avatar) is replaced by D55.

## Decisions

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D52 | Where the hosts are | Both hosts sit at the desk. A sidekick spot by the band is listed as an idea for later in 21 | The "both at the desk" guess in 0004 |
| D53 | Which stinger plays on Reveal | Among the players who answered: everyone right plays an everyone right stinger; nobody right (or nobody answered) plays a nobody right stinger; otherwise right when at least half were right, wrong when fewer were | |
| D54 | Final race staging | Decided after game night 1. Until then a flat track overlay over the `Wide` shot | |
| D55 | Guests | A cast of distinct characters to pick from, like the contestants in Knowledge is Power, not one body with variants. A roster of 10 in `content/characters.json` (id, name, personality line, look, signature color, idle pose, happy reaction, sad reaction, card hold). Phone screen `pickAvatar` becomes `pickCharacter` (character cards with name and personality line, taken ones greyed). PROTOCOL renames avatar to character (`character` message, `characterId`, ack reason `characterTaken`). Game night 1: a capsule in the signature color with the name above it on the TV, a colored card on the phone | The 12 avatar presets of D33 and `content/avatars.json` (deleted); the guest variants detail of 0004 |
| D56 | Staging extras and the idle limit | After the first playtest `staging.extraSeconds` values may go above 0. The 11 s idle limit is a suggested starting value like every timer | Amends D42 (limit fixed at 11 s) and the "needs a decision first" part of D48 |
| D57 | Music | Free placeholder loops for the prototype (sources and licenses in `12_audio.md`). The final music is decided later | |
| D58 | Host line at Start | New trigger `startCountdown`: a short host line when the start countdown begins, 2 lines per host. `gameStart` stays at the start of the Intro | |
| D59 | Reference images | `content/ideation/` is ignored by git. Real show stills never go into the repo. Only original generated images go into `docs/moodboard/`, added by Ilia | |
| D60 | Who the characters are | Personas, not people: no real names, no nationality, no age. Unserious and not realistic, like the contestants in Knowledge is Power (but original). As different from one another as possible. Each persona drives its animations and emotions | The first draft roster (10 named family members) |
| D61 | More characters | The roster grows to 16. Added: a gentleman in a suit, an introvert, an emo, a family friendly bombshell, and two more of Claude's choosing (an alien and a mime) | The roster of 10 in D55 |

## Details added while applying these (not yet confirmed by Ilia)

| Detail | Where |
| --- | --- |
| The roster itself (personas, looks, colors, animations) is a draft to approve. The ghost changed from shy to "tries to be scary" so it does not overlap the introvert | `content/characters.json`, 21 |
| Two more signature animations per character, entrance and victory, so the persona shows when they join and when they win (six in all) | `content/characters.json`, 15, 21, validator |
| D45's matte, textured materials apply to the cast; its believable proportions now apply to the hosts only, since the cast may be cartoonish. Superseded by D62 in `0006_art_direction.md` | 21 |
| Character ids are integers 0 to 15, like the avatar ids were; the protocol version stays 1 (nothing is built yet) | PROTOCOL, 15 |
| Name tag on the TV: the character's name, with the player's name under it in smaller text | 21 |
| On join the TV gives the first free character (lowest id), as it did with avatars | 02, PROTOCOL |
| In flat UI (TV lock in row, vote chips, phone top bar and waiting list) a player is shown as a character chip: a rounded card in the signature color with the player's name in `#14161C`. The validator checks that each signature color has a contrast of at least 4.5 with `#14161C` | 06, 15, validator |
| Characters hold their card in their own way only for the raise and hold; lowering the card and the shrug for no answer are shared animations | 21 |
| `startCountdown` fires after Start and after Play again; a host who just spoke it is still inside the gap when the Intro starts, so the other host says the first `gameStart` line | 15 |
| Race reveals in the `Wide` shot play each guest's happy or sad reaction | 21 |
| The idle limit stays a number in the acceptance check, not a config key, until Ilia decides otherwise | 19, 21 open questions |
