# Decision record 0014: details chosen while building Day 4 (not yet confirmed by Ilia)

Date: Nov 3, 2026. Built on branch `day4-powerups`. None of these is a decision by Ilia yet: each is the reading the AI chose where the spec left room. Confirm, change or turn each into a D number. The AI never reopens a confirmed one on its own. Nothing in `PROTOCOL.md` or the config key names changed.

## Power ups (Core)

| Detail | Where |
| --- | --- |
| A power up drawn at a Reveal can be used in the same pick window (Reveal and Scores of that question) | Core `ShowDirector` |
| "Players" in the draw formula is every seated player (connected or not); the rank is the shared rank before the question (`PlayerTurn.RankBefore`) | Core `PowerUpDealer` |
| The chance is rolled only when it is above 0 and a type is on; the type is a second roll only on a hit (a seed replays both) | Core `PowerUpDealer` |
| `powerUps.maxHeld` must be 1 (config validation): the `player` object carries one `heldPowerUp` | Core `ConfigLoader` |
| No draw on the last question of the last round (nothing could use it); no draws in the race | Core |
| Unused power ups are cleared when the last round's RoundEnd starts | Core |
| "One hit per target per question" counts power ups waiting for the next question; a second pick on that target gets `immune` (the protocol has no other reason for it) | Core `Targeting` |
| Immunity: hit on question q (the effect landed there), immune for questions q+1 to q+`immunityQuestions`; question numbers run through the whole game (round 2 question 1 is question 5) | Core `Targeting` |
| A disconnected player can be targeted (the seat is held); the effect lands even if they are away and counts for immunity | Core |
| aimUp: targets are only players with a strictly better shared rank; they are not listed at all otherwise (free lists every other player). With no listed target the phone gets `watch`, so the aimUp leader keeps the power up without a screen | Core, Net |
| Ack reasons: `stale` for an old phaseId, `closed` for the pick window that just ended, `invalid` outside an open window, for self, an unknown or not allowed target or a bad type, `noPowerUp` when not holding that type, `immune` for hit or immune targets | Core |
| `effectCleared` is taken only in Answer with that Answer's phaseId, once, for the effect that landed; it never locks an answer | Core |
| Power ups off, or every type off, means no draws and no pick window. `gameStarted.settings.powerUps` lists the types the game draws from (empty when off) | Core, Telemetry |
| There is no log line for an effect landing (no `powerUpApplied` in 15_schemas); `powerUpUsed` and `effectCleared` cover it | Telemetry |

## The poll

| Detail | Where |
| --- | --- |
| The game ends with Results (`gameEnded` and the game length are measured there); the Poll follows as its own phase `Poll`, outside game time and outside the idle numbers | Core, Telemetry |
| Scores and ready marks are reset when the Poll ends (on the way to the Lobby), not when Results ends | Core |
| Nobody connected: the poll runs to `poll.maxSeconds` | Core |
| Answers are cleaned against `poll.json`: text trimmed and cut to 200 characters, empty text left out, a choice must be a whole number in range, unknown ids and wrong types left out; the log keeps `poll.json` order | Core `PollAnswers` |
| A second poll from the same player is `invalid`; a poll outside the Poll phase is `closed`; `answers` that is not an object is `invalid` | Core |
| `poll` and `pollDone` screens: `pollDone` has `data: {}`; text questions carry no `options` field | Net |
| poll.json question texts at most 120 characters, options at most 40 (no limit in 15_schemas; chosen like questions and options) | Core `ContentLimits` |

## Back to lobby and the TV settings

| Detail | Where |
| --- | --- |
| Back to lobby in the Lobby only logs `settingsChanged backToLobby`; in the countdown it stops it (no `gameEnded`, no game ran); in the Poll it ends the poll (the game had already ended) | Core |
| Back to lobby clears ready marks, scores and power ups, keeps seats, characters, languages and timer multipliers; the next game skips the Intro | Core |
| `settingsChanged` values: `gameLang` "ru"/"en", `powerUps` "true"/"false", `powerUpTypes` the switched-on list joined with commas ("freeze,fog"), `targeting` "free"/"aimUp", `backToLobby` "true" | Core, Telemetry |

## Host line triggers (Core) and selection (Dialogue)

| Detail | Where |
| --- | --- |
| Core raises every trigger as `HostLineRequested` (with the player picked by the seeded random when several fit); Dialogue picks the line | Core, Dialogue |
| `lastPlaceCorrect`: "last place" is the highest shared rank before the question, and only when it is above 1 (while everyone shares rank 1, at the start, nobody is in last place) | Core `RevealTriggers` |
| `fastAnswer`: the fastest correct `ms` must be held by one player alone; a tie fires nothing | Core |
| `everyoneCorrect` looks at connected players; `nobodyCorrect` at everyone with a turn | Core |
| `streak3` fires when the streak equals `hosts.streakTrigger` (not again at 4) | Core |
| `lastCall` is not a question cycle trigger: it plays by the gap rule and does not use up the cycle's one line | Dialogue |
| Silence and the gap are measured from the end of a host's bubble: a host may speak `hosts.minGapSeconds` after their bubble ended | Dialogue |
| A cycle line dropped by the gap leaves the cycle open, so a later `powerUpUsed` of that question may still play | Dialogue |
| The Intro's two `gameStart` lines ignore the gap (they never overlap a host's own bubble) | Dialogue |
| The host bags are saved in `<persistentDataPath>/host_bags.json`; "Reset seen questions" does not reset them | Dialogue, Bootstrap |
