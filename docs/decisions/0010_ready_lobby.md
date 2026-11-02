# Decision record 0010: everyone taps Ready; after a game, back to the Lobby

Date: Nov 2, 2026. Decided by Ilia after the first Day 2 game on a real phone. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D76 | How a game starts, and what follows Results | Every phone shows Ready in the Lobby, and the TV shows a ready mark on each player. The 3 s countdown starts by itself the moment every connected player is ready and `players.min` (2) or more are connected; disconnected players never hold it up. Ready is a toggle: Not ready, in the Lobby or during the countdown, clears the mark, and during the countdown stops it. A new player joining during the countdown also stops it (they are not ready yet), as does falling below `players.min` connected players. After Results (after the Poll once it exists) everyone returns to the Lobby with ready marks cleared and scores, streaks and correct counts reset to 0; seats, characters, languages and timer multipliers stay, so nobody picks a character again. There is no PlayAgain phase and no Play again button. No fallback when one player never taps Ready: the family asks them, or they tap Leave | D23: "Every phone shows Start in the Lobby (2 or more players) and Play again after the poll", "any phone can Cancel", the `lobby` actions `start`, `cancelStart`, `playAgain`; 0002's PlayAgain phase and `pollDone.canPlayAgain` |

## What changes in `PROTOCOL.md`

- `lobby` actions are `ready` and `notReady` (both `ok` when nothing changes); `start`, `cancelStart` and `playAgain` are gone.
- `lobby` screen data: `players[].ready`, `ready` (this player) and `enoughPlayers` replace `canStart`.
- `pollDone` has no `canPlayAgain`.

## Carried over from Play again

The next game skips the Intro and keeps the seen questions, as Play again did (`02_show_flow.md`).

## Counted the same way everywhere

"Enough players" counts connected players, for starting the countdown and for stopping it.
