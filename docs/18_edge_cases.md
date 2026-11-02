# 18 Edge cases

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`). Each line says what the game does. Unit tests or simulated games cover the ones marked (test).

| Case | What happens |
| --- | --- |
| Only 2 players | Everything works. Power ups target the other player. Final race head start: leader 3, other 0; with equal scores both 3 (test) |
| 8 players | Ninth join gets `refused: roomFull` (test) |
| 1 player left after others leave mid game | The game continues to the end; Start and Play again need 2 players |
| A new name joins during a game | `refused: gameStarted`; they can join at the next Lobby (after Back to lobby on the TV) (test) |
| Same phone opens the page in two tabs | Both tabs find the same token and rejoin the same seat; the newer socket wins and the TV app closes the older one with close code 4002, which shows "Game open in another tab" and does not reconnect (test). Testing alone uses `&tab=<n>` so tabs do not share a token |
| Duplicate names | `refused: nameTaken` (case insensitive) |
| Name with emoji, symbols, or more than 16 characters | Only Latin and Cyrillic letters, digits and spaces are allowed; the name is trimmed; anything else, an empty name or more than 16 characters is refused with `badName` (test) |
| Two players pick the same avatar at once | The first wins; the second gets `ack` reason `avatarTaken` and a refreshed `pickAvatar` screen (test) |
| Phone locks, reloads or loses WiFi | Seat held 300 s; phases stop waiting for that player; `rejoin` restores the current screen with the remaining time; an answer already locked stays locked (test) |
| Player taps Leave | Seat freed at once; the player is out for the rest of the game; in the Lobby they disappear from the list (test) |
| Seat hold runs out | Same as Leave |
| Start tapped on two phones at once | One countdown runs; the later tap gets `ack` with `ok: false`, reason `invalid` |
| A player leaves during the start countdown and fewer than 2 remain | The countdown is cancelled; back to the Lobby (or PlayAgain) |
| Nobody taps Play again | The TV waits in PlayAgain; the TV settings panel offers Back to lobby |
| A phone never submits the poll | The poll ends after 90 s (`poll.maxSeconds`) |
| Nobody answers a question | Phase ends at the last personal deadline; everyone gets 0; trigger `nobodyCorrect` |
| Everyone answers wrong | Same as above |
| Everyone answers right | Trigger `everyoneCorrect` |
| Tie in topic vote | Among the players who voted for one of the tied doors, the lowest ranked one decides; if several of them share the lowest rank, one of them is picked with the seeded random generator. If nobody voted, the seeded random generator picks one of the four doors (test) |
| Tie in score | Shared rank (scores 900, 900, 400 give ranks 1, 1, 3); when one player must be chosen among tied players, seeded random (test) |
| Tie in score at the race start | Same head start for the tied players, the higher one of their group (test) |
| Tie at the end of the race | Higher total before the race, then faster race time, then seeded random (test) |
| Power up target disconnected | Target shows as unavailable; if they disconnect after being targeted, the effect is lost silently |
| Power up target already hit or immune | Greyed out; an input for them gets `ack` reason `immune` and the power up stays held (test) |
| Late input after the player's personal deadline | Refused with `closed` once `net.inputGraceMs` has passed; phone shows "Время вышло / Time's up" (test) |
| Phone clock reports impossible `ms` (negative, or over the window) | Clamped to 0 to the player's personal window (test) |
| Topic or bank runs out of unseen questions | Per slot: unseen from the chosen topic, then unseen from any topic, then seen from the chosen topic, then seen from any topic; log a warning (test) |
| Fewer than 3 unused topics have 4 unseen questions | Fill the doors with the other unused topics that have the most unseen questions, then with topics chosen earlier in this game; the mixed door is always door 4 (test) |
| Race statements run out of unseen ones | Seen statements, picked with the seeded random generator (test) |
| The laptop loses internet | TV shows "Нет соединения с сервером / Lost connection to the server" and pauses the core clock; reconnects every `net.tvReconnectSeconds` (2 s) with the same room code and secret. The relay sends phones `tvAway` (they show reconnecting) and `tvBack` on return (they rejoin). Phone messages sent meanwhile are dropped |
| The relay is down at start | Lobby shows the error and a Retry button; no room code shown |
| A phone sends more than 20 messages per second | The relay drops the extra messages; the socket stays open (test) |
| Config or content file is broken | The app shows the validation errors at startup and does not start a game |
| App closed by accident mid game | Not recovered in game night 1; seen questions are kept on disk |
