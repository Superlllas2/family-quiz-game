# Decision record 0012: details chosen while building Day 3 (not yet confirmed by Ilia)

Date: Nov 3, 2026. Built on branch `day3-show`. None of these is a decision by Ilia yet: each is the reading the AI chose where the spec left room. Confirm, change or turn each into a D number. The AI never reopens a confirmed one on its own.

## Show flow and phases

| Detail | Where |
| --- | --- |
| The race is two phases with their own phaseIds: `RaceQuestion` (the answer part, 8 s times the multiplier, plus `net.inputGraceMs` as in D73) and `RaceReveal` (2 s). `phaseStarted.phase` logs them by these names | Core `GamePhase`, Telemetry |
| The `poll` segment of `show_scripts.json` is passed over until the Poll is built (Day 4); `miniGame` too. After Results the show goes straight back to the Lobby | Core `ShowDirector` |
| "The next game skips the Intro" is read as: every game after the first of the session (`GameNumber > 1`) drops the `intro` segment | Core |
| The race state (head starts, `raceStarted` log line with `lastPlaceCanWin`) is created when FinalIntro starts, because FinalIntro already shows the track with head starts | Core |
| The topic vote has no grace: a vote must arrive before the 10 s end; a vote for the vote that just closed gets `closed`, like a late answer. The vote ends early only when every connected player voted (nobody connected: it runs to its timer) | Core |
| One vote per player, no changing it; a vote of a player who left the game no longer counts | Core |
| The tie break looks at the shared ranks at the moment the vote ends | Core `TopicPicker` |
| A topic chosen in an earlier round includes the mixed topic; the mixed door stays door 4 regardless | Core `TopicPicker` |
| For the mixed door, question group 1 and group 2 are the same (unseen from any topic), and group 3 is "seen from any topic". A warning (log `error`, where `content`) is written whenever group 1 was empty | Core `QuestionPicker`, Telemetry |
| The Intro prediction ("Who wins tonight?") is accepted once per player (input kind `prediction` with `playerId`, any seated player including yourself), stored and logged; it has no effect on the game yet. It counts as an input for the idle share | Core, Net |

## Scoring

| Detail | Where |
| --- | --- |
| Closest value "correct" (streak, Most correct, `result.correct`) means the distance score is above 0; the closest bonus alone does not make a guess correct | Core |
| The closest bonus is `closestBonus × multiplier`, rounded to a whole point (300 in round 3, 400 in round 4); it is not rounded to `scoring.roundTo` | Core `Scoring` |
| Closest value input: `value` must lie inside the slider range (ends included) and `ms` is required (as for `choice`); a value off the step grid is accepted as sent | Core |
| `AnswerLocked.points` (and the log `input.points`) for closest value is the distance score only; the bonus is only known at Reveal and shows in `scoreChanged` | Core, Telemetry |

## Final race and Results

| Detail | Where |
| --- | --- |
| A player alone in the game (only one seated) starts the race on `race.maxHeadStart` (leader rule) | Core `RaceRules.HeadStart` |
| A step never goes past `race.finishSteps` | Core `RaceRules.Move` |
| "Faster total answer time in the race" sums each statement's clamped `ms`, and the player's whole window for a statement they did not answer | Core |
| Results order (`results.place`, the top 3): the race order with its tie rules (furthest, then higher score before the race, then faster race time, then seeded random), so every place is distinct. Without a race in the script, by score with seeded ties | Core |
| Best comeback: rank at the end of round 2 is the shared rank by score when round 2's RoundEnd starts; rank at the end of the race is the place in the race order. Only a gain above 0 wins it; ties all win | Core `Awards` |
| The round winner on RoundEnd (TV) is the player or players with the most points in that round | Presentation |

## Seen lists

| Detail | Where |
| --- | --- |
| Saved to `<persistentDataPath>/seen.json` as `{"questions": [...], "race": [...]}` after every question and statement shown (a crash mid game keeps them). A broken file is reported and treated as empty | Telemetry `SeenFile`, Bootstrap |
| "Reset seen questions" clears both questions and race statements, logged as `settingsChanged` source `tv`, setting `resetSeen`. A Core command exists (`ResetSeen`); the button comes with the TV settings panel on Day 4 | Core |

## Event log additions

| Detail | Where |
| --- | --- |
| `input` lines for `vote` carry `door` and `topicId`; for `prediction` they carry `predicted`; `value` and `trueFalse` inputs carry `value` or `answer` | Telemetry |
| `phaseStarted` carries `round` in TopicVote and RoundEnd, and the statement id as `questionId` in RaceQuestion and RaceReveal | Telemetry |

## Phone screen data (needs a yes before PROTOCOL.md is touched)

| Detail | Where |
| --- | --- |
| `roundEnd.nextRound` is `null` after the last round (the final race comes next); the phone then shows "Final race next". PROTOCOL.md does not say what `nextRound` is after round 4 | Net, phone |
| `raceResult.finish` is the finish step (`race.finishSteps`), so the phone can draw the small track | Net |
| `waiting.locked` is the door title after a vote, "1889 m" (value and unit) after a closest value Lock, "True"/"False" after a race answer | Net |
| Closest value numbers are written with no thousands separator and at most 2 decimals | Core `NumberText` |
