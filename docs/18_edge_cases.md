# 18 Edge cases

Status: Done for game night 1 (Nov 2, 2026). Each line says what the game does. Bot simulations or unit tests cover the ones marked (test).

| Case | What happens |
| --- | --- |
| Only 2 players | Everything works. Power ups target the other player. Final race head start: leader 3, other 0 (test) |
| 8 players | Ninth join gets `refused: roomFull` (test) |
| 1 player left after others leave mid game | The game continues; Start needs 2 players, but a running game never stops because of player count |
| A name joins during a game | `refused: gameStarted`; they can join at the next Lobby |
| Same phone joins twice (two tabs) | The second `join` without token is a new player; with the same token, the newer socket wins and the older gets closed (test) |
| Duplicate names | `refused: nameTaken` (case insensitive) |
| Name with emoji, Cyrillic or more than 16 characters | Emoji and Cyrillic allowed; trimmed; over 16 characters refused with `badName`; empty refused |
| Phone locks, reloads or loses WiFi | Seat held 300 s; phases stop waiting for that player; `rejoin` restores the current screen with the remaining time; an answer already locked stays locked (test) |
| Host phone leaves | Next player in join order becomes host phone; TV shows a note; if the old host rejoins, they are a normal player |
| Nobody answers a question | Phase ends at timeout; everyone gets 0; trigger `nobodyCorrect` |
| Everyone answers wrong | Same as above |
| Everyone answers right | Trigger `everyoneCorrect` |
| Tie in topic vote | Lowest ranked player's choice among the tied doors, else seeded random (test) |
| Tie in score at the race start | Same head start for the tied players (test) |
| Tie at the end of the race | Higher total before the race, then faster race time, then seeded random (test) |
| Power up target disconnected | Target shows as unavailable; if they disconnect after being targeted, the effect is lost silently |
| Power up target already hit or immune | Greyed out; an input for them gets `ack` reason `immune` and the power up stays held (test) |
| Late input arrives after the phase ended | Refused with `closed`; phone shows "Время вышло / Time's up" |
| Phone clock reports impossible `ms` (negative, or over the window) | Clamped to 0 to window (test) |
| Topic bank runs out of unseen questions | Use seen questions from the same topic, then from any topic; log a warning (test) |
| Every topic has fewer than 4 unseen questions | Offer the 3 topics with the most unseen questions plus the mixed door |
| The laptop loses internet | TV shows "Нет соединения с сервером / Lost connection to the server" and pauses the core clock; reconnects every 2 s with the same room code and secret; phones reconnect by themselves |
| The relay is down at start | Lobby shows the error and a Retry button; no room code shown |
| Config or content file is broken | The app shows the validation errors at startup and does not start a game |
| App closed by accident mid game | Not recovered in game night 1; seen questions are kept on disk |
