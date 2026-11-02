# Decision record 0009: answers during the Day 2 build

Date: Nov 2, 2026. Decided by Ilia, answering the open questions found while building the Day 2 walking skeleton. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D73 | Answer phase end and `net.inputGraceMs`: does the phase end at the last personal deadline, or wait for the grace? | It waits: a player who has not locked in counts as done at their personal deadline plus `net.inputGraceMs`. The Answer phase ends when every connected player has locked in or reached deadline plus grace, so a tap sent at the buzzer still counts. Adds at most 0.5 s to a question someone did not finish | |
| D74 | Last call with only one connected player | Last call needs at least one other connected player who has locked in. A lone connected player keeps their full window | |
| D75 | Day 2 plays 4 multiple choice questions, but the bank has no hard multiple choice | Day 2 is one round of `rounds.questionsPerRound` multiple choice questions drawn from the whole bank, with no Intro, TopicVote or RoundEnd. Slots follow `rounds.difficultyOrder`; a slot whose format is not multiple choice (slot 4, closest value) uses a medium multiple choice question until closest value arrives on Day 3. So: easy, medium, medium, medium | |

## Details added while building (not yet confirmed by Ilia)

| Detail | Where |
| --- | --- |
| Rounding to the nearest `scoring.roundTo` rounds halves away from zero | Core `Rules/Scoring` |
| A timer multiplier changed on the phone applies from the next Answer phase; the personal deadline is fixed when Answer starts | Core |
| Seen questions are kept in memory on Day 2; saving them to disk comes with Day 3 | Core |
| Results awards on Day 2: "Most correct" only; "Best comeback" needs round 2 and the race (Day 3) | Core, Net |
| Day 2 winner (no race yet): the highest score; ties broken by the game's seeded random | Core |
| Lobby and PlayAgain end with `phaseEnded` reason `allDone` when the countdown starts | Core, Telemetry |
| A player whose deadline passed without an answer keeps the `answerChoice` screen (countdown at 0); a late tap gets `closed` ("Time's up") | Core, Net |
| Input refusals: a `choice` without `ms`, a second input after locking in, or the right phaseId in a phase that takes no such input are `invalid`; a wrong phaseId is `stale`, except an input for the Answer phase that just closed, which is `closed` | Core |
| `gameStarted.settings` carries only `gameLang` until the TV settings panel exists (Day 4) | Telemetry |
| An Answer phase with nobody connected is not ended early: it runs to the latest deadline plus grace. Read literally, D73 would end it at once ("every connected player is done" when nobody is connected), so a laptop WiFi drop would skip the question; the core clock is paused while the relay is down, so the question waits instead | Core `ShowDirector` |
| `allDone` only when every connected player locked in; any Answer end caused by a deadline plus grace is `timeout` | Core, Telemetry |
| A player who reaches deadline plus grace without an answer raises `PlayerTimedOut` (new Core event, once per player per question); the other phones' `waiting.pending` lists drop them then | Core, Net |
| Last call is sent even when the cut does not move the deadline (the player already had less than `timers.lastCall` left). It fires after a lock in, disconnect, leave or rejoin, never by time alone | Core |
| `closed` answers an input for the most recently closed Answer phaseId (until the next Answer closes); older phaseIds get `stale` | Core |
| When a game starts, every seated player counts as having picked a character, so after the game nobody is sent back to `pickCharacter` | Core |
| A config, content or protocol helper error message lists every problem, one per line, as `file id: field problem` | Core `ContentLoader` |
| The content loader also checks the bank holds one Day 2 game without a repeat (one question per slot difficulty) | Core `ContentLoader` |
| The `player` message nests the object like `snapshot.player`: `{"v":1,"type":"player","player":{...}}` | Net, phone |
| `result` is sent at Reveal and again at Scores (same phaseId); `endsInMs` counts to the end of the current one | Net |
| `pickCharacter.taken` includes disconnected seats that are still held | Net |
| The phone asks "Leave for real?" with Stay and Leave before sending `leave` (a step beyond `06_phone_screens.md`) | phone |
| The `pickCharacter` grid scrolls inside the screen: 16 cards with personality lines do not fit (Lobby only, never during play) | phone |
| The phone countdown stops at 1 (never shows 0); the answer countdown can show 0 | phone |
| A `settings` message has no ack: one sent while the socket is down is lost; the phone's saved language wins on rejoin | phone |
| The TV PlayAgain view shows every seated player with rank, chip, score and a correct answers column | Presentation |
| The correct tile on the TV turns `#3DF586` with a white rim and grows 1.08 times; wrong tiles fade to 25 % | Presentation |
| TV animation durations (chip pop 0.25 s, leaderboard rows move after 0.5 s over 0.7 s) are constants in `Presentation/TvMotion.cs`, not in `content/config.json`; they never hold a phase. **Open:** move them under `staging.*` in config? | Presentation |
| The Day 1 key `tv.lobby.lastInput` (the lobby test pad) is removed | `content/ui_strings.json` |
