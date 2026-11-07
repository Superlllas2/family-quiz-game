# 02 Show flow

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`; timers marked as starting values and staging linked in `decisions/0004_staging.md`; characters and `startCountdown` from `decisions/0005_staging_answers.md`).

Every phase, in order, with its timer, what the TV shows, what the phones show, and what ends it early. All durations are config keys in `content/config.json`; the numbers below are their defaults. **Every timer in this document is a suggested starting value to tune in playtests, not a fixed rule** (D48): change it in config, never in code, and re-run the timing checks in `19_acceptance.md`. Durations are maximums: a phase ends early the moment its end condition is met.

What the camera, the set, the guests and the band do in each phase is in `21_staging.md`. Staging adds no time in game night 1 (every `staging.extraSeconds` value is 0).

## Phase list (Standard mode)

```
Lobby
StartCountdown              3 s, starts when every connected player is ready; any Not ready stops it
Intro
Round 1  (multiplier 1.0)   TopicVote, then 4 x QuestionCycle, then RoundEnd
Round 2  (multiplier 1.0)
Round 3  (multiplier 1.5)
Round 4  (multiplier 2.0)
FinalIntro
FinalRace                   up to 12 x RaceQuestion
Results
Poll
Lobby                       back to the Lobby: everyone taps Ready again for the next game (no intro)
```

"Back to lobby" in the TV settings panel returns everyone to the Lobby from any phase.

Mini games are not in game night 1. When added later, one goes after rounds 1, 2 and 3 (see `content/show_scripts.json`).

## The phases

| Phase | Suggested max duration (config key) | Ends early when | TV shows | Every phone shows |
| --- | --- | --- | --- | --- |
| Lobby | none | Every connected player tapped Ready and 2 or more are connected (`players.min`) | Room code, QR code, joined players as their characters with a ready mark each, "Tap Ready on your phone"; a host greets each new player (`playerJoined` line) | `join`, then `pickCharacter`, then `lobby` with Ready |
| StartCountdown | 3 s (`lobby.startCountdownSeconds`) | A connected player is not ready (taps Not ready, or a new player joined) or fewer than 2 are connected: back to Lobby | Big 3, 2, 1 and a short host line (`startCountdown`) | `countdown` with Not ready |
| Intro | 30 s (`timers.intro`) | Never early | Title card and one `gameStart` line from each host, one after the other | `watch` (no prediction, D85) |
| TopicVote | 20 s (`timers.topicVote`) | Every connected player voted | Four topic doors two by two (D128) and live vote counts | Four topic buttons. Votes arriving up to `net.inputGraceMs` after the 20 s still count (D84) |
| QuestionShow | 10 s (`timers.questionShow`) | Never early | Question text, read time bar | `watch` (the question is on the TV only, D88) |
| Answer | 20 s multiple choice (D140), 20 s closest value (`timers.answer.multipleChoice`, `timers.answer.closestValue`), times each player's timer multiplier | Every connected player locked in or passed their personal deadline. When all but one have locked in, the remaining player's time is cut to at most 5 s (`timers.lastCall`) | Question, options, countdown, character chips pop as players lock in (not what they chose) | Answer screen for the format (options, slider or True and False only, no question text); active sabotage effect on top if hit |
| Reveal | 6 s (`timers.reveal`) | Never early | Correct answer highlighted, who got it right, points per player | `watch`; the power up pick if the player holds one (Day 4) |
| Scores | 5 s (`timers.scores`) | Never early | Leaderboard rows move to new ranks | The same screen with the same phaseId: the power up pick continues (pick, then target) |
| RoundEnd | 4 s (`timers.roundEnd`) | Never early | Round leaderboard, round winner, host line | `watch` |
| FinalIntro | 8 s (`timers.finalIntro`) | Never early | Race track with head starts by rank, one line of rules | `watch` |
| RaceQuestion | 12 s answer times each player's timer multiplier (`race.answer`) plus 2 s reveal (`race.reveal`) | Answer ends when every connected player answered or passed their personal deadline. No last call in the race | Statement with "True" and "False" side by side on its right end (D128, D129), track, players move on reveal | True and False buttons, then `watch` in the reveal |
| Results | 20 s (`timers.results`) | Never early | The winner (no places 2 and 3), two awards (most correct, best comeback), a funny tag for every other player (D86), a draining bar and "Back to the lobby in N" (D78) | The winner's phone: "You won!" and an interesting fact (D87). Everyone else: `watch` |
| Poll | 90 s (`poll.maxSeconds`) | Every connected phone submitted | "Thanks! Fill in the poll on your phone" and a count of submitted polls | Poll questions, then "Thanks!" while others finish |

After the Poll (with `poll.enabled` = false, after Results) the show returns to the Lobby (D76).

A question cycle is QuestionShow, Answer, Reveal, Scores: about 26 s with a full multiple choice window (31 s for closest value), less when everyone answers early.

**Game length without mini games:** measured from the end of the start countdown to the end of Results. A round takes at most about 3 minutes (vote 20 s, 4 cycles, round end 4 s), so 4 rounds, intro, final race and results come to about 12 to 14 minutes, and at most about 16 minutes with full answer windows (timers set again after the Nov 6, 2026 playtest, D126). Game length is an outcome, not a target (D127). The poll adds up to 90 s. The 15 to 19 minutes in the vision include 3 mini games, which come after game night 1.

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

Every game marks each question and race statement it uses as seen, when it is shown. The list is saved to disk, so it survives the next game, Back to lobby and app restarts. Only "Reset seen questions" in the TV settings panel clears it. The current bank holds exactly 2 Standard games without a repeat (16 medium questions, 24 race statements).

## The final race

1. **Head start:** players are ranked by total score. The leader starts at step 3, last place at step 0, the others spread evenly between and rounded down (config `race.maxHeadStart` = 3, formula in `03_rules.md`). Equal scores get equal steps.
2. **Questions:** true or false statements from `race.json`, unseen first, then seen, picked with the seeded random generator. Each player's window is 12 s (`race.answer`) times their timer multiplier. Correct moves the player 1 step (`race.stepsPerCorrect`). Wrong or no answer: no move.
3. **Finish:** the race ends when a player reaches step 10 (`race.finishSteps`) or after 12 questions (`race.maxQuestions`). Several players may cross on the same question.
4. **Winner:** the furthest player. Ties go to the higher total score before the race, then to the faster total answer time in the race, then to a seeded random pick.
5. **Check:** at the start of the race, log `lastPlaceCanWin` = true when both hold: (the leader's head start minus last place's head start) is less than `race.maxQuestions`, and (`race.finishSteps` minus last place's head start) is at most `race.maxQuestions`. With the defaults (3 − 0 < 12 and 10 − 0 ≤ 12) this is always true; config validation refuses values that make it false for a head start gap of `race.maxHeadStart`.

Ghosts and a chasing threat are left for after game night 1.

## Joining and seats

- Players join only in the Lobby, up to `players.max`. Once a game starts, the roster is fixed until the game ends; a new join gets `refused: gameStarted`.
- On join the TV app gives the player the first free character (lowest id in `content/characters.json`); the `pickCharacter` screen lets them pick another free one. Each character can be taken by one player at a time.
- A player who disconnects keeps their seat for `players.seatHoldSeconds` (300 s) and can always rejoin it with their token. Phases do not wait for disconnected players.
- When the hold runs out, or the player taps Leave on their phone, the seat is freed at once. The player is out for the rest of the game: removed from the leaderboard, ranks and target lists. In the Lobby they disappear from the player list.

## Starting, the next game, back to lobby

There is no host phone: no player has special rights.

- **Ready (D76):** every phone shows Ready in the Lobby; the TV shows who is ready. When every connected player is ready and 2 or more are connected, the 3 s StartCountdown runs on the TV and all phones. Any phone can tap Not ready during it, which stops it; so does a new player joining. Disconnected players never hold the start up.
- **No skipping:** Intro, FinalIntro and Results run on their timers.
- **The next game:** after the poll everyone returns to the Lobby with ready marks cleared and scores reset to 0. Seats, characters, phone languages and timer multipliers are kept, so nobody picks a character again; new players can join and others can leave. Everyone taps Ready again. The next game skips the Intro and keeps the seen questions.
- **New game (D107):** in the pause menu. The running game is abandoned (logged as `gameEnded` with reason `restart`), everyone returns to the Lobby as with Back to lobby, and every connected player with a character is marked ready, so the StartCountdown starts at once when 2 or more are connected. Takes a confirming second click.
- **Back to lobby:** in the TV settings panel. Everyone returns to the Lobby with their seats and characters kept, so new players can join or others can leave. A running game is abandoned (logged as `gameEnded` with reason `backToLobby`).

## TV settings panel

The person at the laptop opens and closes it with the Tab key and uses it with the mouse. It opens at any time and never pauses the game (Settings in the pause menu opens the same panel while the game is paused).

| Setting | Default | Takes effect |
| --- | --- | --- |
| Game language RU or EN (TV text, host text) | Russian | At once |
| Power ups on or off | `powerUps.enabled` | Next game start |
| Each power up on or off (freeze, slime, fog) | `powerUps.types` | Next game start |
| Targeting free or aim up | `powerUps.targeting` | Next game start |
| Reset seen questions | | At once |
| Question feedback on or off (the rating card on the phones, D108) | `feedback.enabled` (off) | At once, from the next Reveal |
| Volume: music and sounds, two sliders from 0 to 100 % (D114, D120; this laptop only) | `music.volume` (35 %), `sfx.volume` (60 %) | At once |
| Back to lobby | | At once |

Phones keep only their own settings: language, more time, leave (`06_phone_screens.md`).

## The question bank test (decisions/0020)

A test tool, not part of the finished game (`banks.chooseOnStart`). When the app starts, the TV asks "Какие вопросы играем?": **Все вопросы** (the full bank) or **Отборные** (the curated test bank, 12 topics). Phones can join meanwhile; Escape keeps the full bank. The pause menu's "Вопросы: …" changes it from the next game. The topic vote offers only the chosen bank's topics.

## Pause menu (decisions/0018)

Escape on the laptop opens the pause menu over the TV: **Continue**, **New game** (hidden in the Lobby), **Settings**. Escape again or Continue closes it; Escape while the settings panel is open from the menu closes the panel and shows the menu again.

- While the menu is open the game is paused: the core clock stands still, so no phase, personal deadline, vote, seat hold or host line moves on. It works in every phase.
- Every phone shows `paused`. On Continue every phone gets its screen back with the same phase id and the time left; the time a phone spent paused does not count in its answer time (D112).
- Inputs already on their way are handled as usual.

The pause menu also has **Графика / Graphics** (decisions/0021): display mode, resolution, VSync, frame limit, quality preset and its details, saved on this laptop.

## Question feedback (decisions/0018)

When question feedback is on, every phone shows a rating card over its screen in the Reveal and Scores of every round question (not the race):

| Item | Question (ru / en) | 1 star | 5 stars |
| --- | --- | --- | --- |
| `fun` | Понравился вопрос? / Did you like the question? | Скучно / Boring | Отлично / Loved it |
| `guess` | Можно было догадаться, не зная? / Could you work it out without knowing? | Никак / No way | Легко / Easily |
| `fact` | Интересный ответ? / Was the answer interesting? | Ну и что / So what | Вау! / Wow! |

- The leftmost star is 1 of 5, the rightmost 5 of 5. Every row is optional; each tap counts at once and can be changed until Scores ends.
- The player can close the card; it comes back for the next question.
- The card never extends a phase (pillar 1): Reveal and Scores run on their timers, and the card goes when Scores ends. The power up pick stays under it.
- The TV shows nothing of the ratings during the game. Every tap is logged (`questionRated`, `15_schemas.md`); `tools/analyze_ratings.mjs` sums the logs.
