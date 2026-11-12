# 02 Show flow

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`; timers marked as starting values and staging linked in `decisions/0004_staging.md`; characters and `startCountdown` from `decisions/0005_staging_answers.md`).

Every phase, in order, with its timer, what the TV shows, what the phones show, and what ends it early. All durations are config keys in `content/config.json`; the numbers below are their defaults. **Every timer in this document is a suggested starting value to tune in playtests, not a fixed rule** (D48): change it in config, never in code, and re-run the timing checks in `19_acceptance.md`. Durations are maximums: a phase ends early the moment its end condition is met.

What the camera, the set, the guests and the band do in each phase is in `21_staging.md`. Staging adds no time in game night 1 (every `staging.extraSeconds` value is 0).

## Phase list (Standard mode)

```
Lobby
StartCountdown              3 s, starts when every connected player is ready; any Not ready stops it
Intro
Round 1                     TopicVote (TopicTie on a tie), then 4 x QuestionCycle, then RoundEnd
Round 2
Round 3
Round 4                     no round multipliers (D151): every question counts the same
FinalIntro
FinalRace                   RaceQuestion after RaceQuestion until someone reaches the finish (D157)
Results
Poll
Lobby                       back to the Lobby: everyone taps Ready again for the next game (no intro)
```

"Back to lobby" in the TV settings panel returns everyone to the Lobby from any phase.

Mini games are not in game night 1. When added later, one goes after rounds 1, 2 and 3 (see `content/show_scripts.json`).

## The phases

| Phase | Suggested max duration (config key) | Ends early when | TV shows | Every phone shows |
| --- | --- | --- | --- | --- |
| Lobby | none | Every connected player tapped Ready and 2 or more are connected (`players.min`) | The camera tours the dim studio in 5 s cuts (D160); fixed on screen: room code, QR code, joined players with a ready mark each, "Tap Ready on your phone"; a host greets each new player (`playerJoined` line) | `join`, then `pickCharacter`, then `lobby` with Ready |
| StartCountdown | 3 s (`lobby.startCountdownSeconds`) | A connected player is not ready (taps Not ready, or a new player joined) or fewer than 2 are connected: back to Lobby | Big 3, 2, 1 and a short host line (`startCountdown`) | `countdown` with Not ready |
| Intro | 30 s (`timers.intro`) | Never early | Title card and one `gameStart` line from each host, one after the other | `watch` (no prediction, D85) |
| TopicVote | 20 s (`timers.topicVote`) | Every connected player voted | Four topic doors two by two (D128) and live vote counts | Four topic buttons. Votes arriving up to `net.inputGraceMs` after the 20 s still count (D84) |
| TopicTie | 2.5 s (`timers.topicTie`), only when the vote tied or nobody voted (D153) | Never early | A spin between the tied doors (all four when nobody voted) that stops on the door the seeded random picked | `watch` |
| QuestionShow | 12 s (`timers.questionShow`) | Never early | Question text, read time bar | `watch` (the question is on the TV only, D88) |
| Answer | 20 s multiple choice (D140), 20 s closest value (`timers.answer.multipleChoice`, `timers.answer.closestValue`), times each player's timer multiplier | Every connected player locked in or passed their personal deadline. When all but one have locked in, the remaining player's time is cut to at most 5 s (`timers.lastCall`) | The question, very large, with no answer tiles (D150); countdown; character chips pop as players lock in (not what they chose); each sabotaged player's character wears its effects (D149) | Answer screen for the format (options or slider, no question text, D162); sabotage effects on top if hit, one after another, melting at the cap (D148) |
| Reveal | 9 s (`timers.reveal`) | Never early | Correct answer highlighted, who got it right, points per player | `watch`; the power up pick if the player holds one (Day 4) |
| Scores | 7 s (`timers.scores`) | Never early | Score cards in sofa order, each under its character, a crown on the leader's (D159); an arrow from attacker to target for every power up used (D149) | The same screen with the same phaseId: the power up pick continues (pick, then target) |
| Attacks | 4 s (`timers.attacks`), only when a power up was used in this pick window (D149) | Never early | Every attack of this question at once: arrows from attackers to targets, the effect icons on the targets | The same screen and phaseId as Scores: the pick still counts |
| RoundEnd | 6 s (`timers.roundEnd`) | Never early | Round leaderboard, round winner, host line | `watch` |
| FinalIntro | 12 s (`timers.finalIntro`) | Never early | Race track with head starts by rank, the rules: right one step forward, wrong one step back, Пас stays (D157) | `watch` |
| RaceQuestion | 12 s answer times each player's timer multiplier (`race.answer`) plus 4 s reveal (`race.reveal`) | Answer ends when every connected player answered, passed (Пас) or reached their personal deadline. No last call in the race | Statement with "True" and "False" side by side on its right end (D128, D129), track; on reveal players move forward or back | True and False side by side and Пас under them (D157), then `watch` in the reveal |
| Results | 20 s (`timers.results`) | Never early | The winner (no places 2 and 3), two awards (most correct, best comeback), a funny tag for every other player (D86), a draining bar and "Back to the lobby in N" (D78) | The winner's phone: "You won!" and an interesting fact (D87). Everyone else: `watch` |
| Poll | 90 s (`poll.maxSeconds`) | Every connected phone submitted | "Thanks! Fill in the poll on your phone" and a count of submitted polls | Poll questions, then "Thanks!" while others finish |

After the Poll (with `poll.enabled` = false, after Results) the show returns to the Lobby (D76).

A question cycle is QuestionShow, Answer, Reveal, Scores (and Attacks after an attack): at most 48 s with a full answer window and an attack, less when everyone answers early.

**Game length without mini games:** measured from the end of the start countdown to the end of Results. A round takes at most about 3.5 minutes (vote 20 s, 4 cycles, round end 6 s); with the longer race (D157) a game comes to about 15 to 18 minutes (timers set again after the Nov 8, 2026 playtest, D152). Game length is an outcome, not a target (D127). The poll adds up to 90 s. The 15 to 19 minutes in the vision include 3 mini games, which come after game night 1.

## Ranks

Rank is the position by total score, highest first. Equal scores share the better rank: scores 900, 900 and 400 give ranks 1, 1 and 3. At the start of a game every player has rank 1. Whenever a rule needs exactly one player out of several tied players, it picks one with the seeded random generator.

## Rules inside a round

1. **TopicVote:** four doors.
   - Door 4 is always the mixed door (`topics.mixedTopicId`, "Обо всём / A bit of everything", questions from any topic).
   - Doors 1 to 3 are the three shelves, always in this order (D154): door 1 `screen`, door 2 `knowledge`, door 3 `life` (the `shelf` of each topic in `topics.json`).
   - For each shelf the candidates are its topics with at least 4 unseen questions (`topics.minUnseenToOffer`; any format or difficulty) that pass every rule:
     1. not offered earlier in this game;
     2. its `category` not chosen earlier in this game;
     3. at most one `russian` topic among doors 1 to 3;
     4. doors 1 to 3 not all `young` and not all `old`.
   - Among the candidates, the topic offered longest ago comes first (a topic never offered counts as longest ago); equal ones in seeded random order. The offer history is saved in the seen file across games and evenings.
   - When a shelf has no candidate, the rules are dropped one at a time from the last (4, then 3, then 2, then 1) until it has one; then the 4 unseen questions rule; a shelf with no topic at all takes one from another shelf.
   - Majority wins. A tie, or nobody voted: the seeded random generator picks among the tied doors (all four when nobody voted), and TopicTie shows the spin (D153).
2. **Questions:** 4 per round. The slots are, in order: easy multiple choice, medium multiple choice, medium multiple choice, hard closest value (`rounds.difficultyOrder`, `rounds.formatOrder`). For each slot, take a question of the slot's format and difficulty from the first of these groups that has one:
   1. unseen, from the chosen topic (any topic for the mixed door)
   2. unseen, from any topic
   3. seen, from the chosen topic
   4. seen, from any topic

   Within that group pick with the seeded random generator. A question is never used twice in the same game. Log a warning whenever group 1 had nothing.
3. **Multiplier:** none for now (D151): `rounds.multipliers` is 1 for every round.

## Seen questions

Every game marks each question and race statement it uses as seen, when it is shown, and each topic offered on a door. The lists are saved to disk in the order they were seen, so they survive the next game, Back to lobby and app restarts. Only "Reset seen questions" in the TV settings panel clears them. When the unseen ones run out, the **least recently seen** comes back first (D155); seeing an item again moves it to the end of its list.

## The final race

1. **Head start:** players are ranked by total score. The leader starts at step 4 (`race.maxHeadStart`), last place at step 0, the others spread evenly between and rounded down (formula in `03_rules.md`). Equal scores get equal steps.
2. **Questions:** true or false statements from `race.json`: unseen first (seeded random), then the least recently seen (D155). Each player's window is 12 s (`race.answer`) times their timer multiplier. Correct moves the player 1 step forward (`race.stepsPerCorrect`); wrong moves them 1 step back (`race.stepsPerWrong`), never below 0; Пас or no answer: no move (D157).
3. **Finish:** the race ends when a player reaches step 15 (`race.finishSteps`). There is no statement limit. Several players may cross on the same question. Safety stop: when no connected player answered or passed for 3 statements in a row (`race.idleStopStatements`), the race ends with the order as it stands.
4. **Winner:** the furthest player. Ties go to the faster total answer time in the race, then to the higher total score before the race, then to a seeded random pick (D158).
5. **Check:** `lastPlaceCanWin` is logged at the start of the race; with no statement limit it is always true.

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

## The question bank (D130)

The game plays one bank: the refined questions in `content/questions_refined.json`. There is no bank choice at start and none in the pause menu. The topic vote offers only the topics that have questions in it, and the mixed door. `content/questions.json`, the full bank, stays in the repo as the source the refining works from (`question_audit/refining_plan.md`); the game does not load it. A question whose full bank question (`basedOn`) a family saw before D130 counts as seen. (The bank test of decisions/0020, D116 to D118, offered both banks; D130 replaced it.)

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
