# 02 Show flow

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`; timers marked as starting values and staging linked in `decisions/0004_staging.md`; characters and `startCountdown` from `decisions/0005_staging_answers.md`).

Every phase, in order, with its timer, what the TV shows, what the phones show, and what ends it early. All durations are config keys in `content/config.json`; the numbers below are their defaults. **Every timer in this document is a suggested starting value to tune in playtests, not a fixed rule** (D48): change it in config, never in code, and re-run the timing checks in `19_acceptance.md`. Durations are maximums: a phase ends early the moment its end condition is met.

What the camera, the set, the guests and the band do in each phase is in `21_staging.md`. Staging adds no time in game night 1 (every `staging.extraSeconds` value is 0).

## Phase list (Standard mode)

```
Lobby
StartCountdown              3 s, any phone can cancel
Intro
Round 1  (multiplier 1.0)   TopicVote, then 4 x QuestionCycle, then RoundEnd
Round 2  (multiplier 1.0)
Round 3  (multiplier 1.5)
Round 4  (multiplier 2.0)
FinalIntro
FinalRace                   up to 12 x RaceQuestion
Results
Poll
PlayAgain                   any phone taps Play again: StartCountdown, then Round 1 (no intro)
```

"Back to lobby" in the TV settings panel returns everyone to the Lobby from any phase.

Mini games are not in game night 1. When added later, one goes after rounds 1, 2 and 3 (see `content/show_scripts.json`).

## The phases

| Phase | Suggested max duration (config key) | Ends early when | TV shows | Every phone shows |
| --- | --- | --- | --- | --- |
| Lobby | none | Any phone taps Start (enabled at 2 or more players, `players.min`) | Room code, QR code, joined players as their characters, "Waiting for players"; a host greets each new player (`playerJoined` line) | `join`, then `pickCharacter`, then `lobby` with Start |
| StartCountdown | 3 s (`lobby.startCountdownSeconds`) | Any phone taps Cancel, or fewer than 2 players remain: back to Lobby (or to PlayAgain) | Big 3, 2, 1 and a short host line (`startCountdown`) | `countdown` with Cancel |
| Intro | 30 s (`timers.intro`) | Never early | Title card and one `gameStart` line from each host, one after the other | "Get ready" plus prediction: "Who wins tonight?" (tap a player) |
| TopicVote | 10 s (`timers.topicVote`) | Every connected player voted | Four topic doors and live vote counts | Four topic buttons |
| QuestionShow | 4 s (`timers.questionShow`) | Never early | Question text, read time bar | Question text, "Get ready" |
| Answer | 15 s multiple choice, 20 s closest value (`timers.answer.multipleChoice`, `timers.answer.closestValue`), times each player's timer multiplier | Every connected player locked in or passed their personal deadline. When all but one have locked in, the remaining player's time is cut to at most 5 s (`timers.lastCall`) | Question, options, countdown, character chips pop as players lock in (not what they chose) | Answer screen for the format; active sabotage effect on top if hit |
| Reveal | 3.5 s (`timers.reveal`) | Never early | Correct answer highlighted, who got it right, points per player | "Correct +850" or "Wrong" plus streak and rank. Power up pick opens here if the player holds one |
| Scores | 3.5 s (`timers.scores`) | Never early | Leaderboard rows move to new ranks | The same result screen with the same phaseId: power up pick continues (pick, then target) |
| RoundEnd | 4 s (`timers.roundEnd`) | Never early | Round leaderboard, round winner, host line | "Round 2 coming", own rank |
| FinalIntro | 8 s (`timers.finalIntro`) | Never early | Race track with head starts by rank, one line of rules | "Final race: answer true or false, fast" |
| RaceQuestion | 8 s answer times each player's timer multiplier (`race.answer`) plus 2 s reveal (`race.reveal`) | Answer ends when every connected player answered or passed their personal deadline. No last call in the race | Statement, track, players move on reveal | True and False buttons |
| Results | 20 s (`timers.results`) | Never early | Winner, top 3, two awards (most correct, best comeback) | Own placement and stats |
| Poll | 90 s (`poll.maxSeconds`) | Every connected phone submitted | "Thanks! Fill in the poll on your phone" and a count of submitted polls | Poll questions, then "Thanks!" while others finish |
| PlayAgain | none | Any phone taps Play again (needs 2 or more players): StartCountdown, then Round 1 | Final standings and "Play again? Tap on your phone" | "Thanks!" with Play again |

With `poll.enabled` = false, Results goes straight to PlayAgain.

A question cycle is QuestionShow, Answer, Reveal, Scores: about 26 s with a full multiple choice window (31 s for closest value), less when everyone answers early.

**Game length without mini games:** measured from the end of the start countdown to the end of Results. A round takes at most about 2 minutes (vote 10 s, 4 cycles, round end 4 s), so 4 rounds, intro, final race and results come to about 9 to 11 minutes, and at most about 11.5 minutes with full answer windows. The poll adds up to 90 s. The 15 to 19 minutes in the vision include 3 mini games, which come after game night 1.

## Ranks

Rank is the position by total score, highest first. Equal scores share the better rank: scores 900, 900 and 400 give ranks 1, 1 and 3. At the start of a game every player has rank 1. Whenever a rule needs exactly one player out of several tied players, it picks one with the seeded random generator.

## Rules inside a round

1. **TopicVote:** four doors.
   - Door 4 is always the mixed door (`topics.mixedTopicId`, "Обо всём / A bit of everything", questions from any topic).
   - Doors 1 to 3: the candidates are the topics not chosen earlier in this game. Among the candidates with at least 4 unseen questions (`topics.minUnseenToOffer`; any 4, of any format or difficulty), choose 3 with the seeded random generator.
   - If fewer than 3 candidates qualify, fill the free doors with the other candidates that have the most unseen questions, then with topics chosen earlier in this game, most unseen first. Equal counts are ordered with the seeded random generator.
   - Majority wins. Tie break: among the players who voted for one of the tied doors, the lowest ranked one decides; if several of them share the lowest rank, one of them is picked with the seeded random generator. If nobody voted, the seeded random generator picks one of the four doors.
2. **Questions:** 4 per round. The slots are, in order: easy multiple choice, medium multiple choice, medium multiple choice, hard closest value (`rounds.difficultyOrder`, `rounds.formatOrder`). For each slot, take a question of the slot's format and difficulty from the first of these groups that has one:
   1. unseen, from the chosen topic (any topic for the mixed door)
   2. unseen, from any topic
   3. seen, from the chosen topic
   4. seen, from any topic

   Within that group pick with the seeded random generator. A question is never used twice in the same game. Log a warning whenever group 1 had nothing.
3. **Multiplier:** the round multiplier applies to every point earned in that round.

## Seen questions

Every game marks each question and race statement it uses as seen, when it is shown. The list is saved to disk, so it survives Play again, Back to lobby and app restarts. Only "Reset seen questions" in the TV settings panel clears it. The current bank holds exactly 2 Standard games without a repeat (16 medium questions, 24 race statements).

## The final race

1. **Head start:** players are ranked by total score. The leader starts at step 3, last place at step 0, the others spread evenly between and rounded down (config `race.maxHeadStart` = 3, formula in `03_rules.md`). Equal scores get equal steps.
2. **Questions:** true or false statements from `race.json`, unseen first, then seen, picked with the seeded random generator. Each player's window is 8 s (`race.answer`) times their timer multiplier. Correct moves the player 1 step (`race.stepsPerCorrect`). Wrong or no answer: no move.
3. **Finish:** the race ends when a player reaches step 10 (`race.finishSteps`) or after 12 questions (`race.maxQuestions`). Several players may cross on the same question.
4. **Winner:** the furthest player. Ties go to the higher total score before the race, then to the faster total answer time in the race, then to a seeded random pick.
5. **Check:** at the start of the race, log `lastPlaceCanWin` = true when both hold: (the leader's head start minus last place's head start) is less than `race.maxQuestions`, and (`race.finishSteps` minus last place's head start) is at most `race.maxQuestions`. With the defaults (3 − 0 < 12 and 10 − 0 ≤ 12) this is always true; config validation refuses values that make it false for a head start gap of `race.maxHeadStart`.

Ghosts and a chasing threat are left for after game night 1.

## Joining and seats

- Players join only in the Lobby, up to `players.max`. Once a game starts, the roster is fixed until the game ends; a new join gets `refused: gameStarted`.
- On join the TV app gives the player the first free character (lowest id in `content/characters.json`); the `pickCharacter` screen lets them pick another free one. Each character can be taken by one player at a time.
- A player who disconnects keeps their seat for `players.seatHoldSeconds` (300 s) and can always rejoin it with their token. Phases do not wait for disconnected players.
- When the hold runs out, or the player taps Leave on their phone, the seat is freed at once. The player is out for the rest of the game: removed from the leaderboard, ranks and target lists. In the Lobby they disappear from the player list.

## Starting, play again, back to lobby

There is no host phone: no player has special rights.

- **Start:** every phone shows Start in the Lobby, enabled at 2 or more players. Anyone can press it. Start runs the 3 s StartCountdown on the TV and all phones; any phone can tap Cancel during it.
- **No skipping:** Intro, FinalIntro and Results run on their timers.
- **Play again:** after the poll every phone shows Play again. Anyone can press it; it runs the same countdown with Cancel. It keeps the same players, seats, characters, phone languages and timer multipliers, resets scores, skips Lobby and Intro, and keeps the seen questions.
- **Back to lobby:** in the TV settings panel. Everyone returns to the Lobby with their seats and characters kept, so new players can join or others can leave. A running game is abandoned (logged as `gameEnded` with reason `backToLobby`).

## TV settings panel

The person at the laptop opens and closes it with the Tab key and uses it with the mouse. It opens at any time and never pauses the game.

| Setting | Default | Takes effect |
| --- | --- | --- |
| Game language RU or EN (TV text, host text) | Russian | At once |
| Power ups on or off | `powerUps.enabled` | Next game start |
| Each power up on or off (freeze, slime, fog) | `powerUps.types` | Next game start |
| Targeting free or aim up | `powerUps.targeting` | Next game start |
| Reset seen questions | | At once |
| Back to lobby | | At once |

Phones keep only their own settings: language, more time, leave (`06_phone_screens.md`).
