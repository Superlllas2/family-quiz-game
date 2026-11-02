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
