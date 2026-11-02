# 03 Rules and numbers

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`).

Every number in the game lives in `content/config.json`. This document explains each rule; the table at the end lists every key with its default. Code never contains these numbers as literals.

## Ranks

Rank is the position by total score, highest first. Equal scores share the better rank: scores 900, 900 and 400 give ranks 1, 1 and 3. At the start of a game every player has rank 1. "Leader" means rank 1; "last place" means the highest rank number in the game. Whenever a rule needs exactly one player out of several tied players, it picks one with the seeded random generator.

## Scoring

### Multiple choice

- Correct: `points = round(maxPoints × (1 − speedPenalty × t / T)) × roundMultiplier`
  - `t` = milliseconds from the moment the options appeared on that phone to the tap, as measured by the phone. The clock starts when the options appear, sabotage or not. A sabotage therefore costs time, which can cost some speed points (about 100 for 3 s on a 15 s window), but it never takes points away directly.
  - `T` = the player's personal answer window in milliseconds: the format's window times the player's timer multiplier. Last call does not change `T`.
  - `maxPoints` = 1000, `speedPenalty` = 0.5, so a correct answer at the buzzer still earns 500
  - `t` is clamped to the range 0 to T
- Wrong or no answer: 0. Points never go negative.
- Round the result of the formula to the nearest 10 (`scoring.roundTo`) before applying the multiplier, so scores look clean.

### Closest value

- Each question has a `range` (min, max) shown on the slider and a `tolerance`.
- `points = round(maxPoints × max(0, 1 − |guess − answer| / tolerance)) × roundMultiplier`, rounded to the nearest 10 before the multiplier.
- No speed component.
- Closest bonus: the player or players with the smallest distance get +200 (`closestValue.closestBonus`) times the multiplier, even if their distance score is 0.
- No answer: 0 and no bonus.

### Streaks

A streak counts correct answers in a row in rounds (closest value counts as correct when it scores above 0). The streak is shown on the phone and the TV. It gives no points in game night 1; it exists for host lines (`hosts.streakTrigger` = 3) and later rules.

## Power ups

Three power ups for game night 1. Each one costs the target a few seconds. It never takes points away directly; the lost seconds can cost some speed points (see Scoring).

| Power up | Russian name | Effect on the target's phone during their next answer phase |
| --- | --- | --- |
| Freeze | Заморозка | Answer buttons are covered in ice. The player taps the ice 6 times (`powerUps.freeze.taps`) to break it, then can answer. |
| Slime | Слизь | Three slime blobs (`powerUps.slime.blobs`) cover the buttons. The player swipes across each blob to wipe it away. |
| Fog | Туман | The options are hidden behind fog for 3 s (`powerUps.fog.seconds`). Rubbing the screen clears it 2x faster. |

On closest value questions the same effects cover the slider instead of the buttons.

### Earning

- After each correct answer in rounds (not in the final race), the player may draw a power up.
- Draw chance by rank before the question: `chance = drawChanceLeader + (1 − drawChanceLeader) × (rank − 1) / (players − 1)`, using the shared rank. The leader gets 30% (`powerUps.drawChanceLeader`), a player alone in last place 100%. With one player in the game, no draws.
- Which power up: equal chance among those enabled in settings.
- A player holds at most 1 power up (`powerUps.maxHeld`). A new draw while holding one is skipped.
- Draws use the injected random generator, so a saved seed replays them.

### Using

- The pick window is Reveal plus Scores. Both show the same result screen with the same phaseId, so a pick during Scores is valid. The phone shows the held power up; the player taps it, then taps a target. Two taps.
- If the player does not pick, they keep it for a later question. Unused power ups are lost at the end of round 4.
- The effect lands on the target's phone at the start of the next Answer phase. The TV shows a small icon next to the target's avatar and the attacker's name ("Dad → Freeze"), and fires the `powerUpUsed` host line trigger.
- **Targeting:** free by default (`powerUps.targeting` = `free`): any other player. With `aimUp`, only players with a better rank than yours; tied players cannot target each other, and a leader cannot attack (in game night 1 the leader just keeps the power up).
- **Caps:** at most 1 effect per target per question (`powerUps.maxHitsPerQuestion`). A target that is already hit, or immune, shows greyed out. After being hit, a player is immune for 1 question (`powerUps.immunityQuestions`).
- Using a power up on the last question of round 4 does nothing; the pick window is hidden on that question.

### Settings

Power ups on or off as a whole, and each type on or off, in the TV settings panel (`02_show_flow.md`). Defaults come from `powerUps.enabled`, `powerUps.types` and `powerUps.targeting`. Changes apply from the next game start.

## Timer multiplier and personal deadlines

- Each player has a timer multiplier (default 1.0, `players.timerMultiplierOptions` = 1.0, 1.5, 2.0) chosen on their phone in settings ("More time"). It multiplies that player's answer window in round questions and in race questions.
- Each player has a personal deadline: the start of the answer phase plus the window times their multiplier.
- **Last call:** when all but one connected player have locked in, the remaining player's deadline becomes the earlier of their deadline and now plus `timers.lastCall` (5 s). The TV app sends that player a new `screen` message with the same phaseId and the new `endsInMs`. Race questions have no last call.
- An input that arrives after the player's own deadline, plus `net.inputGraceMs` (500 ms) for network delay, is refused as `closed`.
- The phase ends when every connected player has locked in or passed their personal deadline.

## Topic vote

See `02_show_flow.md`. Tie break: among the players who voted for one of the tied doors, the lowest ranked one decides; if several of them share the lowest rank, one of them is picked with the seeded random generator. If nobody voted, the seeded random generator picks one of the four doors.

## Final race

See `02_show_flow.md`. Head start = `floor(maxHeadStart × (rankFromBottom / (players − 1)))`, where `rankFromBottom = players − rank` with the shared rank. Last place alone has rankFromBottom 0, and tied players share the higher head start of their group.

`lastPlaceCanWin` (logged at race start, checked by config validation): (leader's head start − last place's head start) < `race.maxQuestions`, and (`race.finishSteps` − last place's head start) ≤ `race.maxQuestions`.

## Winner and ties

- Game winner = final race winner.
- Race ties: higher total score before the race, then faster total answer time in the race, then a seeded random pick.
- Awards on the Results screen: "Most correct" (most correct answers in rounds) and "Best comeback" (biggest rank gain from the end of round 2 to the end of the race). Ties: both shown.

## Modes

Standard is the only mode for game night 1. Quick and Long in `show_scripts.json` need a multiplier per round in `rounds.multipliers` (and a matching `rounds.count`) before use.

## Config keys

| Key | Default | Meaning |
| --- | --- | --- |
| `configVersion` | 1 | Version of the config format |
| `timers.intro` | 30 | Seconds |
| `timers.topicVote` | 10 | Seconds |
| `timers.questionShow` | 4 | Seconds before options unlock |
| `timers.answer.multipleChoice` | 15 | Seconds, times the player's timer multiplier |
| `timers.answer.closestValue` | 20 | Seconds, times the player's timer multiplier |
| `timers.lastCall` | 5 | Seconds left for the last player when all others answered |
| `timers.reveal` | 3.5 | Seconds |
| `timers.scores` | 3.5 | Seconds |
| `timers.roundEnd` | 4 | Seconds |
| `timers.finalIntro` | 8 | Seconds |
| `timers.results` | 20 | Seconds |
| `rounds.count` | 4 | Rounds in Standard |
| `rounds.questionsPerRound` | 4 | |
| `rounds.multipliers` | [1, 1, 1.5, 2] | Per round |
| `rounds.difficultyOrder` | easy, medium, medium, hard | Per question slot |
| `rounds.formatOrder` | multipleChoice ×3, closestValue | Per question slot |
| `topics.doors` | 4 | Doors per vote, last is the mixed door |
| `topics.minUnseenToOffer` | 4 | Unseen questions a topic needs to be preferred for a door |
| `topics.mixedTopicId` | mixed | Topic id of the mixed door |
| `scoring.maxPoints` | 1000 | |
| `scoring.speedPenalty` | 0.5 | Share lost at the buzzer |
| `scoring.roundTo` | 10 | |
| `scoring.fastAnswerMs` | 2000 | A correct answer faster than this can fire `fastAnswer` |
| `closestValue.closestBonus` | 200 | |
| `powerUps.enabled` | true | Default for the TV settings panel |
| `powerUps.types` | freeze, slime, fog | Enabled list, default for the TV settings panel |
| `powerUps.targeting` | free | free or aimUp, default for the TV settings panel |
| `powerUps.drawChanceLeader` | 0.3 | Last place alone is always 1.0 |
| `powerUps.maxHeld` | 1 | |
| `powerUps.maxHitsPerQuestion` | 1 | |
| `powerUps.immunityQuestions` | 1 | |
| `powerUps.onPhone` | true | Effects play on the phone (D15) |
| `powerUps.freeze.taps` | 6 | |
| `powerUps.slime.blobs` | 3 | |
| `powerUps.fog.seconds` | 3 | |
| `race.maxHeadStart` | 3 | Steps |
| `race.finishSteps` | 10 | |
| `race.maxQuestions` | 12 | |
| `race.stepsPerCorrect` | 1 | |
| `race.answer` | 8 | Seconds, times the player's timer multiplier |
| `race.reveal` | 2 | Seconds |
| `players.min` | 2 | Needed for Start and Play again |
| `players.max` | 8 | |
| `players.timerMultiplierOptions` | [1, 1.5, 2] | |
| `players.seatHoldSeconds` | 300 | Seat kept after a disconnect |
| `lobby.startCountdownSeconds` | 3 | Countdown after Start or Play again |
| `net.inputGraceMs` | 500 | Grace after a personal deadline for network delay |
| `net.tvReconnectSeconds` | 2 | TV app retry interval after losing the relay |
| `hosts.minGapSeconds` | 3 | Silence after a line, per host |
| `hosts.bubbleSeconds` | 3.5 | How long a text bubble stays |
| `hosts.streakTrigger` | 3 | Streak length that fires `streak3` |
| `poll.enabled` | true | |
| `poll.maxSeconds` | 90 | The poll ends after this even if phones have not submitted |
