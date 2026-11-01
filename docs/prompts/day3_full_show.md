# Day 3 prompt: the full show (Thursday)

```
Read CLAUDE.md and re-read docs/02_show_flow.md, docs/03_rules.md, docs/04_formats.md, docs/06_phone_screens.md, docs/15_schemas.md, docs/18_edge_cases.md and docs/19_acceptance.md (section "Day 3") before starting.

Goal for today: the complete Standard show without power ups: Intro, 4 rounds with topic vote and round multipliers, closest value questions, RoundEnd, FinalIntro, the final race with true or false, Results, Play again, and seen questions remembered across games and app restarts.

Build on a branch called day3-show, tests first:

1. Core: show_scripts.json drives the segments. TopicPicker and QuestionPicker with every rule in docs/02_show_flow.md (doors, mixed door, min unseen, tie break by lowest ranked voter, slot order easy MC, medium MC, medium MC, hard closest value, fallbacks). Closest value scoring with tolerance and the closest bonus including ties. RaceRules: head start formula, true or false questions from race.json, stepsPerCorrect, finish or max questions, winner and tie rules, lastPlaceCanWin logged at race start. Awards on Results. Seen questions saved to disk.

2. Unit tests for every rule above, and the edge cases marked (test) in docs/18_edge_cases.md that belong to these features.

3. Bot simulation: 100 Standard games with 6 bots of mixed speed and accuracy. Assert every phase runs in order, game length 8 to 12 minutes of game clock, idle share under 20% for normal speed bots, lastPlaceCanWin always true, and no repeated question within 3 games while unseen ones remain. Print a summary table of phase durations.

4. Phone: topicVote, answerSlider (slider, big value with unit, plus and minus, Lock), answerTrueFalse, roundEnd, raceResult.

5. TV: topic doors with live votes, the closest value number line reveal with avatars and a flag, the race track with head starts and movement, Results with top 3 and awards. Grey boxes are fine.

Commit after every green test run. When done, list the Day 3 checks and anything you had to guess.
```
