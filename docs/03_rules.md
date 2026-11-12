# 03 Rules and numbers

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`; starting values and staging keys from `decisions/0004_staging.md`).

Every number in the game lives in `content/config.json`. This document explains each rule; the table at the end lists every key with its starting value. Code never contains these numbers as literals.

**Every timer and every staging duration is a suggested starting value to tune in playtests, not a fixed rule** (D48). That covers `timers.*`, `race.answer`, `race.reveal`, `lobby.startCountdownSeconds`, `poll.maxSeconds`, `hosts.minGapSeconds`, `hosts.bubbleSeconds` and every `staging.*` key. Tune them in config only; tests read them from config, and the timing checks in `19_acceptance.md` must still pass after a change; the 21 s idle limit there is a starting value too and is set again with them (D56).

## Ranks

Rank is the position by total score, highest first. Equal scores share the better rank: scores 900, 900 and 400 give ranks 1, 1 and 3. At the start of a game every player has rank 1. "Leader" means rank 1; "last place" means the highest rank number in the game. Whenever a rule needs exactly one player out of several tied players, it picks one with the seeded random generator.

## Scoring

### Multiple choice

- Correct: `points = round(maxPoints × (1 − speedPenalty × t / T)) × roundMultiplier`
  - `t` = milliseconds from the moment the options appeared on that phone to the tap, as measured by the phone. The tap counts when the finger lifts inside the button (D143), so that is when `t` is taken. The clock starts when the options appear, sabotage or not. A sabotage therefore costs time, which can cost some speed points (about 120 for 3 s on a 20 s window with `speedPenalty` 0.8), but it never takes points away directly.
  - `T` = the player's personal answer window in milliseconds: the format's window times the player's timer multiplier. Last call does not change `T`.
  - `maxPoints` = 1000, `speedPenalty` = 0.8 (D144), so a correct answer at the buzzer earns 200
  - `t` is clamped to the range 0 to T
- Wrong or no answer: 0. Points never go negative.
- Round the result of the formula to the nearest 10 (`scoring.roundTo`) before applying the multiplier, so scores look clean.

### Closest value

- Each question has a `range` (min, max) shown on the slider and a `tolerance`.
- Distance `d = |guess − answer|`, exact band `e = exactShare × tolerance` (`closestValue.exactShare` = 0.05) (D83):
  - `d ≤ e`: `maxPoints` (1000).
  - With the zero point `z = zeroAtTolerances × tolerance` (`closestValue.zeroAtTolerances` = 3, D156):
  - `e < d < z`: `maxPoints × (z − d) / (z − e)`, so the points fall in a straight line from 1000 at the exact band to 0 at 3× the tolerance.
  - `d ≥ z`: 0.
  - Rounded to the nearest 10 before the multiplier, then times the round multiplier (1 for now, D151). Example (Everest, 8849 m, tolerance 1500 m, z 4500 m): 8849 ± 75 m scores 1000, 1500 m off 680, 3000 m off 340, 4500 m off 0.
- A guess is correct when `d ≤ tolerance` (the edge included): it counts for streaks and "Most correct".
- No speed component.
- A guess beyond the tolerance can score points (up to 3× the tolerance) without being correct.
- Closest bonus: the player or players with the smallest distance get +200 (`closestValue.closestBonus`) times the multiplier, even if their distance score is 0.
- No answer: 0 and no bonus.

### Streaks

A streak counts correct answers in a row in rounds (closest value counts as correct when the guess is within the tolerance, D83). The streak is shown on the TV only (D88). It gives no points in game night 1; it exists for host lines (`hosts.streakTrigger` = 3) and later rules.

## Power ups

Three power ups for game night 1. Each one costs the target a few seconds. It never takes points away directly; the lost seconds can cost some speed points (see Scoring).

| Power up | Russian name | Effect on the target's phone during their next answer phase |
| --- | --- | --- |
| Freeze | Заморозка | Answer buttons are covered in ice. The player taps the ice 15 times (`powerUps.freeze.taps`, D148) to break it, then can answer. |
| Slime | Слизь | Six slime blobs (`powerUps.slime.blobs`, D148) cover the buttons. The player swipes across each blob to wipe it away. |
| Fog | Туман | The options are hidden behind fog for 6 s (`powerUps.fog.seconds`, D145). Rubbing the screen clears it 2x faster. |

On closest value questions the same effects cover the slider instead of the buttons.

### Earning

- After each correct answer in rounds (not in the final race), the player may draw a power up.
- Draw chance by rank before the question: `chance = drawChanceLeader + (1 − drawChanceLeader) × (rank − 1) / (players − 1)`, using the shared rank. The leader gets 30% (`powerUps.drawChanceLeader`), a player alone in last place 100%. With one player in the game, no draws.
- Which power up: equal chance among those enabled in settings.
- A player holds at most 1 power up (`powerUps.maxHeld`). A new draw while holding one is skipped.
- Draws use the injected random generator, so a saved seed replays them.

### Using

- The pick window is Reveal plus Scores, plus Attacks when that phase runs (D149). All show the same result screen with the same phaseId, so a pick during Scores or Attacks is valid. The phone shows the held power up; the player taps it, then taps a target. Two taps.
- If the player does not pick, they keep it for a later question. Unused power ups are lost at the end of round 4.
- The effect lands on the target's phone at the start of the next Answer phase. Several effects on one phone play one after another, in the order they were used; the phone clears them in that order (D145). The TV shows one tag per hit with attacker, power up and target ("Аня → Слизь → Соня", D145) and an arrow from the attacker's character to the target's (D149), and fires the `powerUpUsed` host line trigger. After Scores, when any power up was used in this pick window, the Attacks phase (`timers.attacks`) shows them all at once.
- **Sabotage cap (D148):** a player loses at most `powerUps.maxSabotageSeconds` (12 s) to sabotage per question, counted from the moment the options appear on their phone. The TV sends the cap with each effect (`params.capMs`, the time left of the cap); at the cap every effect still on the phone melts away. Effects that melted are not reported as cleared.
- When a player draws a power up at Reveal, the TV says why on a tag in the power up's colour, as long as they hold it: "Аня получает: Слизь" and "За верный ответ", plus "Кто позади лидера, получает чаще" for a player who was not the leader (D145).
- **Targeting:** free by default (`powerUps.targeting` = `free`): any other player. With `aimUp`, only players with a better rank than yours; tied players cannot target each other, and a leader cannot attack (in game night 1 the leader just keeps the power up).
- **Caps:** at most 3 effects per target per question (`powerUps.maxHitsPerQuestion`, D145), so players can gang up on the leader. A target that already has `maxHitsPerQuestion` hits waiting, or is immune, shows greyed out. No immunity after a hit (`powerUps.immunityQuestions` 0, D148); a value above 0 makes a player immune for that many questions after being hit.
- Using a power up on the last question of round 4 does nothing; the pick window is hidden on that question.

### Settings

Power ups on or off as a whole, and each type on or off, in the TV settings panel (`02_show_flow.md`). Defaults come from `powerUps.enabled`, `powerUps.types` and `powerUps.targeting`. Changes apply from the next game start.

## Timer multiplier and personal deadlines

- Each player has a timer multiplier (default 1.0, `players.timerMultiplierOptions` = 1.0, 1.5, 2.0) chosen on their phone in settings ("More time"). It multiplies that player's answer window in round questions and in race questions.
- Each player has a personal deadline: the start of the answer phase plus the window times their multiplier.
- **Last call (off since decisions/0020, D119: `timers.lastCallEnabled` false; another player's answer never shortens anyone's time):** when all but one connected player have locked in, the remaining player's deadline becomes the earlier of their deadline and now plus `timers.lastCall` (5 s). The TV app sends that player a new `screen` message with the same phaseId and the new `endsInMs`. Race questions have no last call.
- An input that arrives after the player's own deadline, plus `net.inputGraceMs` (500 ms) for network delay, is refused as `closed`.
- The phase ends when every connected player has locked in or passed their personal deadline.

## Topic vote

See `02_show_flow.md`. Tie break (D153): the seeded random generator picks among the tied doors; if nobody voted, among all four. TopicTie (`timers.topicTie`) shows the spin.

## Final race

See `02_show_flow.md`. Head start = `floor(maxHeadStart × (rankFromBottom / (players − 1)))`, where `rankFromBottom = players − rank` with the shared rank. Last place alone has rankFromBottom 0, and tied players share the higher head start of their group.

Moves (D157): right `+race.stepsPerCorrect`, wrong `−race.stepsPerWrong` (never below 0), Пас or no answer 0. No statement limit; the safety stop after `race.idleStopStatements` statements in a row with no answer and no pass from any connected player ends the race as it stands. `lastPlaceCanWin` is logged at race start; with no statement limit it is always true.

## Winner and ties

- Game winner = final race winner.
- Race ties (D158): faster total answer time in the race, then higher total score before the race, then a seeded random pick.
- Race ties, total answer time: each statement counts the answer's or the pass's `ms`, or the player's whole window when they did neither (D81).
- Results show one winner; there are no places 2 and 3 (D86). The winner's phone gets an interesting fact from `content/facts.json`, one not given before (kept in the seen file; once all were given, any) (D87).
- Awards on the Results screen: "Most correct" (most correct answers in rounds) and "Best comeback" (biggest rank gain from the end of round 2, by shared rank, to the end of the race, by the race order; only a gain above 0 wins it) (D82). Ties: all shown.
- Funny tags (D86): every player except the winner gets one tag, never the same tag twice in a game. A stat tag goes only to the player who really is the most of all players (the winner included); ties among untagged players are broken by the seeded random. The stat tags are tried in a seeded order: most wrong answers, slowest average answer (share of the window), most wrong answers faster than `scoring.fastAnswerMs`, the furthest closest value guess outside the tolerance, most last calls, most unanswered questions, most topic votes for a losing door, fewest race steps. Players left without one get a kind tag (dark horse, life of the party and others). Texts: `tv.tags.*` in `ui_strings.json`.

## Modes

Standard is the only mode for game night 1. Quick and Long in `show_scripts.json` need a multiplier per round in `rounds.multipliers` (and a matching `rounds.count`) before use. Every multiplier is 1 for now (D151).

## Config keys

Starting values for playtests. Config validation also checks that each staging animation fits inside its phase (the phase timer plus its staging extra): the curtain and the question fade in within QuestionShow, cards up plus reactions within Reveal, the leaderboard overlay within Scores, the suspense riser within the shortest answer window, the establishing shot within the Intro, and the winner walk within Results.

| Key | Starting value | Meaning |
| --- | --- | --- |
| `configVersion` | 1 | Version of the config format |
| `timers.intro` | 30 | Seconds |
| `timers.topicVote` | 20 | Seconds |
| `timers.questionShow` | 12 | Seconds before options unlock |
| `timers.answer.multipleChoice` | 20 | Seconds, times the player's timer multiplier |
| `timers.answer.closestValue` | 20 | Seconds, times the player's timer multiplier |
| `timers.lastCall` | 5 | Seconds left for the last player when all others answered |
| `timers.reveal` | 9 | Seconds |
| `timers.scores` | 7 | Seconds |
| `timers.attacks` | 4 | Seconds of the Attacks phase, only after a power up was used (D149) |
| `timers.topicTie` | 2.5 | Seconds of the TopicTie spin, only on a tie (D153) |
| `timers.roundEnd` | 6 | Seconds |
| `timers.finalIntro` | 12 | Seconds |
| `timers.results` | 20 | Seconds |
| `rounds.count` | 4 | Rounds in Standard |
| `rounds.questionsPerRound` | 4 | |
| `rounds.multipliers` | [1, 1, 1, 1] | Per round; 1 for now (D151) |
| `rounds.difficultyOrder` | easy, medium, medium, hard | Per question slot |
| `rounds.formatOrder` | multipleChoice ×3, closestValue | Per question slot |
| `topics.doors` | 4 | Doors per vote, last is the mixed door |
| `topics.minUnseenToOffer` | 4 | Unseen questions a topic needs to be preferred for a door |
| `topics.mixedTopicId` | mixed | Topic id of the mixed door |
| `scoring.maxPoints` | 1000 | |
| `scoring.speedPenalty` | 0.8 | Share lost at the buzzer |
| `scoring.roundTo` | 10 | |
| `scoring.fastAnswerMs` | 2000 | A correct answer faster than this can fire `fastAnswer` |
| `closestValue.closestBonus` | 200 | |
| `closestValue.zeroAtTolerances` | 3 | Distance points reach 0 at this many tolerances (D156) |
| `closestValue.exactShare` | 0.05 | The exact band as a share of the tolerance: this close or closer scores `scoring.maxPoints` (D83) |
| `powerUps.enabled` | true | Default for the TV settings panel |
| `powerUps.types` | freeze, slime, fog | Enabled list, default for the TV settings panel |
| `powerUps.targeting` | free | free or aimUp, default for the TV settings panel |
| `powerUps.drawChanceLeader` | 0.3 | Last place alone is always 1.0 |
| `powerUps.maxHeld` | 1 | |
| `powerUps.maxHitsPerQuestion` | 3 | |
| `powerUps.immunityQuestions` | 0 | Questions of immunity after a hit (D148) |
| `powerUps.maxSabotageSeconds` | 12 | Most seconds of sabotage per player per question (D148) |
| `powerUps.onPhone` | true | Effects play on the phone (D15) |
| `powerUps.freeze.taps` | 15 | |
| `powerUps.slime.blobs` | 6 | |
| `powerUps.fog.seconds` | 6 | |
| `race.maxHeadStart` | 4 | Steps |
| `race.finishSteps` | 15 | |
| `race.stepsPerCorrect` | 1 | |
| `race.stepsPerWrong` | 1 | Steps back for a wrong answer, never below 0 (D157) |
| `race.idleStopStatements` | 3 | Statements in a row with no answer or pass from anyone that end the race (D157) |
| `race.answer` | 12 | Seconds, times the player's timer multiplier |
| `race.reveal` | 4 | Seconds |
| `players.min` | 2 | Needed for the start countdown (every connected player Ready, D76) |
| `players.max` | 8 | |
| `players.timerMultiplierOptions` | [1, 1.5, 2] | |
| `players.seatHoldSeconds` | 300 | Seat kept after a disconnect |
| `lobby.startCountdownSeconds` | 3 | Countdown once every connected player is Ready (D76) |
| `net.inputGraceMs` | 500 | Grace after a personal deadline for network delay |
| `net.tvReconnectSeconds` | 2 | TV app retry interval after losing the relay |
| `hosts.minGapSeconds` | 3 | Silence after a line, per host |
| `hosts.bubbleSeconds` | 3.5 | How long a text bubble stays |
| `hosts.streakTrigger` | 3 | Streak length that fires `streak3` |
| `poll.enabled` | true | |
| `poll.maxSeconds` | 90 | The poll ends after this even if phones have not submitted |
| `staging.extraSeconds.questionShow` | 0 | Seconds staging adds to QuestionShow (`21_staging.md`). 0 for game night 1; may go above 0 after the first playtest, with the idle limit set again (D56) |
| `staging.extraSeconds.reveal` | 0 | The same, for Reveal |
| `staging.extraSeconds.scores` | 0 | The same, for Scores |
| `staging.extraSeconds.roundEnd` | 0 | The same, for RoundEnd |
| `staging.lobby.cutSeconds` | 5 | Length of each camera cut in the Lobby tour, fades included (D160) |
| `staging.lobby.fadeSeconds` | 0.5 | Fade out at the end and fade in at the start of each Lobby cut (D160) |
| `staging.lobby.darkStops` | 1 | How much darker the studio is in the Lobby, in exposure stops (D160) |
| `staging.lobby.seatPopSeconds` | 0.5 | The pop when a character leaves its seat on a swap, and when a grey box capsule appears |
| `staging.intro.establishSeconds` | 6 | Establishing shot at the start of the Intro |
| `staging.intro.lightsUpSeconds` | 1.5 | The studio lights coming up at the start of the establishing shot (D94); at most `establishSeconds` |
| `staging.questionShow.curtainOpenSeconds` | 1.2 | The stage curtain opens |
| `staging.questionShow.questionInSeconds` | 0.4 | The question panel fades in |
| `staging.answer.suspenseRiseSeconds` | 5 | The suspense riser starts this long before the latest deadline |
| `staging.reveal.cardsUpSeconds` | 0.5 | Guests raise their answer cards |
| `staging.reveal.reactionSeconds` | 1.5 | Happy or slumped reactions after the cards are up |
| `staging.scores.overlayInSeconds` | 0.4 | The leaderboard overlay slides in |
| `staging.results.winnerWalkSeconds` | 3 | The winner walks to the center of the floor |
