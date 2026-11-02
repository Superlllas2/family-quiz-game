# Day 3 prompt: the full show (Thursday)

```
Read CLAUDE.md and re-read docs/02_show_flow.md, docs/03_rules.md, docs/04_formats.md, docs/06_phone_screens.md, docs/13_architecture.md, docs/15_schemas.md, docs/18_edge_cases.md and docs/19_acceptance.md (section "Day 3") before starting.

Goal for today: the complete Standard show without power ups: Intro (30 s, no skip), 4 rounds with topic vote and round multipliers, closest value questions, RoundEnd, FinalIntro, the final race with true or false, Results, Play again, and seen questions and race statements remembered across games and app restarts.

Build on a branch called day3-show, tests first:

1. Core: show_scripts.json drives the segments. TopicPicker with every rule in docs/02_show_flow.md (candidates not chosen earlier in this game, prefer at least 4 unseen questions, 3 chosen by seeded random, the fill rule, mixed door always door 4, the tie break by the lowest ranked voter among those who voted for a tied door, seeded random when nobody voted). QuestionPicker with the slot order easy MC, medium MC, medium MC, hard closest value and the fallback order: unseen from the chosen topic, unseen from any topic, seen from the chosen topic, seen from any topic. Closest value scoring with tolerance and the closest bonus including ties. RaceRules: head start formula with shared ranks, true or false statements from race.json (unseen first), the timer multiplier on race answers, stepsPerCorrect, finish or max questions, winner and tie rules, lastPlaceCanWin with both conditions logged at race start. Awards on Results. Seen questions and race statements saved to disk, with a reset.

2. Unit tests for every rule above, and the edge cases marked (test) in docs/18_edge_cases.md that belong to these features.

3. Simulated games (players inside the test code only, the six profiles in docs/13_architecture.md: 2 fast, 2 normal, 2 slow): 100 Standard games. Assert every phase runs in order, game length 8 to 12 minutes from the end of the start countdown to the end of Results (and at most 12 minutes when every simulated player uses the full window), no stretch without input longer than 11 s outside Intro, FinalIntro and Results (The current timers give exactly 11 s (QuestionShow + Reveal + Scores, and Reveal + Scores + RoundEnd), so the simulation must end each phase at its exact budget on the fake clock, never a frame later), lastPlaceCanWin always true, and no repeated question or race statement within 2 games starting from an empty seen list. Print a summary table of phase durations, the idle share (reported only) and the longest idle stretch.

4. Phone: topicVote, answerSlider (slider, big value with unit, plus and minus, Lock), answerTrueFalse, roundEnd, raceResult.

5. TV: topic doors with live votes, the closest value number line reveal with character chips and a flag, the race track with head starts and movement, Results with top 3 and awards. Grey boxes are fine.

Commit after every green test run. When done, list the Day 3 checks and anything you had to guess.
```
