# 02 Show flow

Status: Done for game night 1 (Nov 2, 2026).

Every phase, in order, with its timer, what the TV shows, what the phones show, and what ends it early. All durations are config keys in `content/config.json` (section `timers`); the numbers below are their defaults. Durations are maximums: a phase ends early the moment its end condition is met.

## Phase list (Standard mode)

```
Lobby
Intro
Round 1  (multiplier 1.0)   TopicVote, then 4 x QuestionCycle, then RoundEnd
Round 2  (multiplier 1.0)
Round 3  (multiplier 1.5)
Round 4  (multiplier 2.0)
FinalIntro
FinalRace                   up to 12 x RaceQuestion
Results
Poll
  "Play again" goes back to Round 1 (no intro); "New players" goes back to Lobby
```

Mini games are not in game night 1. When added later, one goes after rounds 1, 2 and 3 (see `content/show_scripts.json`).

## The phases

| Phase | Max duration (config key) | Ends early when | TV shows | Every phone shows |
| --- | --- | --- | --- | --- |
| Lobby | none | Host phone taps Start (needs 2 or more players) | Room code, QR code, joined players with avatars, "Waiting for players" | Join form, then Lobby screen (host phone also sees Start, game language, settings) |
| Intro | 30 s (`intro`) | Host phone taps Skip | Title card and one host text exchange | "Get ready" plus prediction: "Who wins tonight?" (tap a player) |
| TopicVote | 10 s (`topicVote`) | Every connected player voted | Four topic doors and live vote counts | Four topic buttons |
| QuestionShow | 4 s (`questionShow`) | Never early | Question text, read time bar | Question text, "Get ready" |
| Answer | 15 s multiple choice, 20 s closest value (`answer.multipleChoice`, `answer.closestValue`) | Every connected player locked in. When all but one have locked in, the remaining time is cut to at most 5 s (`lastCall`) | Question, options, countdown, avatars pop as players lock in (not what they chose) | Answer screen for the format; active sabotage effect on top if hit |
| Reveal | 3.5 s (`reveal`) | Never early | Correct answer highlighted, who got it right, points per player | "Correct +850" or "Wrong" plus streak and rank. Power up pick opens here if the player holds one |
| Scores | 3.5 s (`scores`) | Never early | Leaderboard rows move to new ranks | Power up pick continues (pick, then target) |
| RoundEnd | 4 s (`roundEnd`) | Never early | Round leaderboard, round winner, host line | "Round 2 coming", own rank |
| FinalIntro | 8 s (`finalIntro`) | Host phone taps Skip | Race track with head starts by rank, one line of rules | "Final race: answer true or false, fast" |
| RaceQuestion | 8 s answer (`race.answer`) plus 2 s reveal (`race.reveal`) | Answer ends when every connected player answered | Statement, track, players move on reveal | True and False buttons |
| Results | 20 s (`results`) | Host phone taps Continue | Winner, top 3, two awards (most correct, best comeback) | Own placement and stats |
| Poll | none | Every phone submitted, or host phone taps Finish | "Thanks! Fill in the poll on your phone" and a count of submitted polls | Poll questions, then "Play again?" waiting screen |

A question cycle is QuestionShow, Answer, Reveal, Scores: about 26 s with a full answer window, less when everyone answers early.

**Game length without mini games:** a round takes at most about 2 minutes (vote 10 s, 4 cycles, round end 4 s), so 4 rounds, intro, final race and results come to about 10 to 11 minutes at most. The 15 to 19 minutes in the vision include 3 mini games, which come after game night 1.

## Rules inside a round

1. **TopicVote:** four doors. Door 4 is always "Обо всём / A bit of everything" (questions from any topic). A topic is only offered if it has at least 4 unseen questions. A topic chosen earlier in this game is never offered again in the same game. Majority wins; on a tie, the choice of the lowest ranked player who voted for one of the tied doors wins; if they did not vote, a seeded random pick among the tied doors.
2. **Questions:** 4 per round, from the chosen topic, unseen first. Order: easy multiple choice, medium multiple choice, medium multiple choice, hard closest value. If the topic has no unseen question of the needed kind, take a seen one from the same topic, then from any topic.
3. **Multiplier:** the round multiplier applies to every point earned in that round.

## The final race

1. **Head start:** players are ranked by total score. The leader starts at step 3, last place at step 0, the others spread evenly between and rounded down (config `race.maxHeadStart` = 3). Equal scores get equal steps.
2. **Questions:** true or false statements, 8 s each. Correct moves the player 1 step (`race.stepsPerCorrect`). Wrong or no answer: no move.
3. **Finish:** the race ends when a player reaches step 10 (`race.finishSteps`) or after 12 questions (`race.maxQuestions`). Several players may cross on the same question.
4. **Winner:** the furthest player. Ties go to the higher total score before the race, then to the faster total answer time in the race.
5. **Check:** at the start of the race, log `lastPlaceCanWin` = true when last place can still reach the finish before the leader, assuming last place answers every question right and the leader misses at least one: (finishSteps minus last place's head start) is at most maxQuestions. With the defaults (10 steps, 12 questions, head start gap 3) this is always true; config validation refuses values that make it false.

Ghosts and a chasing threat are left for after game night 1.

## Host phone

The first player who joined is the host phone. If they leave, the next player in join order becomes host phone, and the TV shows a small note. Only the host phone can: start the game, skip the intro or final intro, continue from Results, finish the poll, choose Play again or New players, and change game settings in the Lobby.

## Play again

"Play again" keeps all players, their seats and their phone languages, resets scores, skips Lobby and Intro, and remembers which questions were seen this evening (saved to disk, so a restart of the app keeps them too).
