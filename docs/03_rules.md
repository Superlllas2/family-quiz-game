# 03 Rules and numbers

Status: Done for game night 1 (Nov 2, 2026).

Every number in the game lives in `content/config.json`. This document explains each rule; the table at the end lists every key with its default. Code never contains these numbers as literals.

## Scoring

### Multiple choice

- Correct: `points = round(maxPoints × (1 − speedPenalty × t / T)) × roundMultiplier`
  - `t` = milliseconds from the moment the options became tappable on that phone to the tap, as measured by the phone
  - `T` = the answer window in milliseconds for that player (after any timer multiplier)
  - `maxPoints` = 1000, `speedPenalty` = 0.5, so a correct answer at the buzzer still earns 500
  - `t` is clamped to the range 0 to T
- Wrong or no answer: 0. Points never go negative.
- Round the result of the formula to the nearest 10 before applying the multiplier, so scores look clean.

### Closest value

- Each question has a `range` (min, max) shown on the slider and a `tolerance`.
- `points = round(maxPoints × max(0, 1 − |guess − answer| / tolerance)) × roundMultiplier`, rounded to the nearest 10 before the multiplier.
- No speed component.
- Closest bonus: the player or players with the smallest distance get +200 (`closestValue.closestBonus`) times the multiplier, even if their distance score is 0.
- No answer: 0 and no bonus.

### Streaks

A streak counts correct multiple choice answers in a row (closest value counts as correct when it scores above 0). The streak is shown on the phone and the TV. It gives no points in game night 1; it exists for host lines and later rules.

## Power ups

Three power ups for game night 1. Each one costs the target a few seconds, never points.

| Power up | Russian name | Effect on the target's phone during their next answer phase |
| --- | --- | --- |
| Freeze | Заморозка | Answer buttons are covered in ice. The player taps the ice 6 times (`powerUps.freeze.taps`) to break it, then can answer. |
| Slime | Слизь | Three slime blobs cover the buttons. The player swipes across each blob to wipe it away. |
| Fog | Туман | The options are hidden behind fog for 3 s (`powerUps.fog.seconds`). Rubbing the screen clears it 2x faster. |

On closest value questions the same effects cover the slider instead of the buttons.

### Earning

- After each correct answer in rounds (not in the final race), the player may draw a power up.
- Draw chance by rank before the question: last place 100%, leader 30% (`powerUps.drawChanceLeader`), linear between. With one player in the game, no draws.
- Which power up: equal chance among those enabled in settings.
- A player holds at most 1 power up (`powerUps.maxHeld`). A new draw while holding one is skipped.
- Draws use the injected random generator, so a saved seed replays them.

### Using

- The pick window is Reveal plus Scores. The phone shows the held power up; the player taps it, then taps a target. Two taps.
- If the player does not pick, they keep it for a later question. Unused power ups are lost at the end of round 4.
- The effect lands on the target's phone at the start of the next Answer phase. The TV shows a small icon next to the target's avatar and the attacker's name ("Dad → Freeze"), and logs a host line trigger.
- **Targeting:** free by default (`powerUps.targeting` = `free`): any other player. With `aimUp`, only players ranked above you; the leader can then not attack (in game night 1 the leader just keeps the power up).
- **Caps:** at most 1 effect per target per question (`powerUps.maxHitsPerQuestion`). A target that is already hit, or immune, shows greyed out. After being hit, a player is immune for 1 question (`powerUps.immunityQuestions`).
- Using a power up on the last question of round 4 does nothing; the pick window is hidden on that question.

### Settings

Power ups on or off as a whole, and each one on or off (Lobby settings on the host phone).

## Timer multiplier

Each player has a timer multiplier (default 1.0, `players.timerMultiplierOptions` = 1.0, 1.5, 2.0) chosen on their phone in settings ("More time"). It multiplies that player's Answer window. The phase on the TV still ends when every player has locked in or the longest personal window runs out. The last call rule (cut to 5 s when all but one have answered) applies to the remaining player's own clock.

## Topic vote

See `02_show_flow.md`. Tie break: the lowest ranked player's choice among the tied doors.

## Final race

See `02_show_flow.md`. Head start = `floor(maxHeadStart × (rankFromBottom / (players − 1)))`, where last place has rankFromBottom 0. Equal scores share the higher head start of their group.

## Winner and ties

- Game winner = final race winner.
- Race ties: higher total score before the race, then faster total answer time in the race, then a seeded random pick.
- Awards on the Results screen: "Most correct" (most correct answers in rounds) and "Best comeback" (biggest rank gain from the end of round 2 to the end of the race). Ties: both shown.

## Config keys

| Key | Default | Meaning |
| --- | --- | --- |
| `timers.intro` | 30 | Seconds |
| `timers.topicVote` | 10 | Seconds |
| `timers.questionShow` | 4 | Seconds before options unlock |
| `timers.answer.multipleChoice` | 15 | Seconds |
| `timers.answer.closestValue` | 20 | Seconds |
| `timers.lastCall` | 5 | Seconds left when all but one answered |
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
| `topics.minUnseenToOffer` | 4 | |
| `scoring.maxPoints` | 1000 | |
| `scoring.speedPenalty` | 0.5 | Share lost at the buzzer |
| `scoring.roundTo` | 10 | |
| `closestValue.closestBonus` | 200 | |
| `powerUps.enabled` | true | |
| `powerUps.types` | freeze, slime, fog | Enabled list |
| `powerUps.targeting` | free | free or aimUp |
| `powerUps.drawChanceLeader` | 0.3 | Last place is always 1.0 |
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
| `race.answer` | 8 | Seconds |
| `race.reveal` | 2 | Seconds |
| `players.min` | 2 | |
| `players.max` | 8 | |
| `players.timerMultiplierOptions` | [1, 1.5, 2] | |
| `players.seatHoldSeconds` | 300 | Seat kept after a disconnect |
| `hosts.minGapSeconds` | 3 | Silence after a line, per host |
| `hosts.bubbleSeconds` | 3.5 | How long a text bubble stays |
| `poll.enabled` | true | |
