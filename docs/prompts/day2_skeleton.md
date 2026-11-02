# Day 2 prompt: walking skeleton (Wednesday)

```
Read CLAUDE.md, docs/02_show_flow.md, docs/03_rules.md, docs/04_formats.md, docs/06_phone_screens.md, docs/15_schemas.md, docs/PROTOCOL.md, docs/18_edge_cases.md, docs/decisions/0002_spec_review.md and docs/19_acceptance.md (section "Day 2") fully before starting. Yesterday's relay and join work is on main.

Goal for today: the thinnest real game that runs end to end. Lobby with Start from any phone and the start countdown, then 4 multiple choice questions, scores, results, Play again. Real questions from content/ and the event log. There are no bots in the game: extra players are browser tabs on the laptop (&tab=<n>).

Build on a branch called day2-skeleton, tests first:

1. Core: IClock and IRandom (injected), GameState, PlayerState, shared ranks (1, 1, 3), the ShowDirector running phases Lobby, StartCountdown, QuestionShow, Answer, Reveal, Scores, Results and PlayAgain on the clock with every duration from content/config.json. Start, cancelStart and playAgain from the `lobby` message (any player, 2 or more players). Personal deadlines per player (window times timer multiplier), ending Answer early when every connected player locked in or passed their deadline, and the last call rule (new screen, same phaseId). Inputs after a player's own deadline plus net.inputGraceMs are refused as closed. Scoring for multiple choice exactly as docs/03_rules.md. Leave frees the seat at once. Content loading and validation with Newtonsoft for config.json, questions.json and avatars.json (a broken file must produce a clear error). Option order shuffled with IRandom.

2. Editor script: copy content/ into Assets/StreamingAssets/content/ before Play mode and before every build (docs/13_architecture.md).

3. Unit tests: scoring (0 ms gives 1000, full window gives 500, wrong gives 0, rounding to 10, multiplier, a 2x timer multiplier doubling T), early phase end, personal deadlines, last call, clamping of ms, stale and closed inputs, the start countdown and Cancel, name rules, avatarTaken, leave.

4. Simulated game in Tests/EditMode: 3 simulated players (inside the test code only) plus the game loop on a fake clock; asserts a question ends within 200 ms of the last answer.

5. Net: ScreenBuilder turning core events into the screen messages for lobby, pickAvatar, countdown, getReady, answerChoice, waiting, result, results and pollDone (with canPlayAgain; the poll itself comes on Day 4, so for now PlayAgain follows Results), in each player's language.

6. Phone: screens join, pickAvatar (12 presets from content/avatars.json drawn as colored shapes with the first letter), lobby (Start on every phone), countdown (Cancel), getReady, answerChoice, waiting, result, results, pollDone (Play again), settings (language, more time, leave), reconnecting, error. Replace yesterday's test screen.

7. TV Presentation (UGUI with TextMeshPro): plain grey box views for each phase: question and options in a 2x2 grid with letters and placeholder colors from docs/06_phone_screens.md, countdown bar, avatars (colored shape with first letter) that pop on lock in, reveal, leaderboard, the start countdown. Text big enough to read from a sofa.

8. DevTools: F1 debug overlay with phase, budget, time left and the live idle share.

9. Telemetry: the JSONL writer from docs/15_schemas.md.

Commit after every green test run. When done, tell me how to run a game with my phone and extra browser tabs, and list the Day 2 checks for me to try.
```
