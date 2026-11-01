# Day 2 prompt: walking skeleton (Wednesday)

```
Read CLAUDE.md, docs/02_show_flow.md, docs/03_rules.md, docs/04_formats.md, docs/06_phone_screens.md, docs/15_schemas.md, docs/PROTOCOL.md, docs/18_edge_cases.md and docs/19_acceptance.md (section "Day 2") fully before starting. Yesterday's relay and join work is on main.

Goal for today: the thinnest real game that runs end to end. Lobby, then 4 multiple choice questions, scores, results, Play again. Real questions from content/, bots to fill seats, and the event log.

Build on a branch called day2-skeleton, tests first:

1. Core: IClock and IRandom (injected), GameState, PlayerState, the ShowDirector running phases Lobby, QuestionShow, Answer, Reveal, Scores, Results on the clock with every duration from content/config.json, ending Answer early when every connected player locked in and applying the last call rule. Scoring for multiple choice exactly as docs/03_rules.md. Content loading and validation for config.json and questions.json (a broken file must produce a clear error). Option order shuffled with IRandom.

2. Unit tests: scoring (0 ms gives 1000, full window gives 500, wrong gives 0, rounding to 10, multiplier), early phase end, last call, clamping of ms, stale and closed inputs.

3. Bot simulation in Tests/EditMode: 3 bots plus the game loop on a fake clock; asserts a question ends within 200 ms of the last answer.

4. Net: ScreenBuilder turning core events into the screen messages for getReady, answerChoice, waiting, result and results, in each player's language. Host phone actions start and continue.

5. Phone: screens join, pickAvatar, lobby (Start for the host phone), getReady, answerChoice, waiting, result, results, settings (language, more time), reconnecting, error. Replace yesterday's test screen.

6. TV Presentation: plain grey box views for each phase: question and options in a 2x2 grid with letters and placeholder colors from docs/06_phone_screens.md, countdown bar, avatars that pop on lock in, reveal, leaderboard. Text big enough to read from a sofa.

7. DevTools: bots that join as fake players inside the TV app (no phone needed) with a key to add or remove one; F1 debug overlay with phase, budget, time left and the live idle share.

8. Telemetry: the JSONL writer from docs/15_schemas.md.

Commit after every green test run. When done, tell me how to run a game with bots and my phone, and list the Day 2 checks for me to try.
```
