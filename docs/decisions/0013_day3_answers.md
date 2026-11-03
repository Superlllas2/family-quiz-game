# Decision record 0013: Ilia's answers to the Day 3 guesses

Date: Nov 3, 2026. Decided by Ilia after reading the Day 3 guesses (`0012_day3_details.md`). The AI building the game never reopens one of these on its own; it asks first. Wording marked "draft" (tag names, facts) is for Ilia to check; the rules are decided.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D79 | Is the final race one phase or two? | Two phases with their own phaseIds: `RaceQuestion` (answers, `race.answer` times the timer multiplier, plus `net.inputGraceMs` as in D73) and `RaceReveal` (`race.reveal`) | |
| D80 | What happens to the `poll` segment before the Poll exists? | Passed over until the Poll is built (Day 4); after Results the show goes straight back to the Lobby | |
| D81 | Race tie break "faster total answer time": what counts for a statement a player did not answer? | Their whole answer window for that statement | |
| D82 | What is "Best comeback"? | The biggest gain in places from the end of round 2 (the shared rank by score then) to the end of the race (the place in the race order). Only a gain above 0 wins it; tied players all get it | |
| D83 | When is a closest value guess correct, and how does it score? | Correct when the guess is within the question's tolerance, the edge included. Points: `scoring.maxPoints` (1000) inside an exact band of `closestValue.exactShare` (0.05) times the tolerance; from there a straight line down to `closestValue.minPoints` (100) at the tolerance edge; 0 past it. Rounded to `scoring.roundTo` before the multiplier. The exact band is a share of the tolerance, not 1 % of the answer, because 1 % of a year (1989) is about 20 years and 1 % of a small answer (5) is below one slider step | 03: `maxPoints × (1 − d / tolerance)`, and "correct when it scores above 0" |
| D84 | Do votes get the network grace? | Yes: a vote is taken until `timers.topicVote` plus `net.inputGraceMs`. Timers on the TV and phones still show 10 s. The vote still ends the moment every connected player voted; when someone does not vote, the vote lasts 0.5 s longer | 0012: "the topic vote has no grace" |
| D85 | The Intro prediction "Who wins tonight?" | Dropped for now: no prediction input, no buttons on the phone, no hint on the TV | 02 and 06: the Intro prediction; PROTOCOL: input kind `prediction`, `getReady.prediction` |
| D86 | Results: who is shown? | One winner; there are no places 2 and 3. The two awards stay (Most correct, Best comeback). Every other player gets one funny tag from what they did in the game (most misses, slowest, and so on; draft names in `ui_strings.json`, `tv.tags.*`). Rules in `03_rules.md`, "Winner and ties" | 02: "Winner, top 3"; PROTOCOL: `results.place`, `correctCount`, `awards` |
| D87 | The winner's reward | An interesting fact on the winner's phone only, from `content/facts.json` (draft facts with sources, for Ilia to check). A fact repeats only once every fact was given; the given ones are kept in the seen file with the questions | |
| D88 | What does a phone show? | As little as possible: the phone is for decisions only. It keeps join, character, Ready, the topic vote, the answer buttons (option texts, slider, True and False), the power up pick, the poll and settings. Everything else is on the TV: the question and the race statement, results, ranks, scores, round ends, the race track, Results. Between decisions the phone shows `watch` ("Смотри на экран / Eyes on the TV"). The top bar keeps the name, the character chip and the gear, not the score. PROTOCOL version 2 | 06: the `getReady`, `roundEnd` and `raceResult` screens, the question on answer screens, the result screen's right or wrong and points, the streak on the phone (03), the pending chips on `waiting`; D78: the "Back to the lobby in N" line on the phone (the TV keeps it); 0012: "Final race next" on the phone after round 4 (approved, then no longer needed) |

## Content for Ilia to check

- `content/facts.json`: 30 draft facts in Russian and English, each with a source.
- `tv.tags.*` in `content/ui_strings.json`: 8 tags from stats and 7 kind ones for players without a stat tag.
- `content/questions.json`: tolerances now mark where a guess stops counting as correct (still 100 points at the edge). Some may want tightening; for example Everest (8849 m) counts anything from 7349 m to 10349 m as correct.
