# Decision record 0022: longer question read and a 30 minute game cap

Date: Nov 6, 2026. After the first play on the Mac, Ilia's feedback was that everything is too fast: nobody could read the question before the options unlocked. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D125 | Question read time and game length | `timers.questionShow` goes from 4 s to 8 s (double the read time). The idle limit of D42 is set again to 15 s (QuestionShow 8 + Reveal 3.5 + Scores 3.5), as D56 allows. A Standard game may last up to **30 minutes**, including with every player using the full answer window; the lower bound of 8 minutes in the simulated game check stays | The 10 to 12 minute length of D41 and the 12 minute full window limit in `19_acceptance.md`; the 11 s idle limit of D42 |

With the current timers a game takes about 11 to 13 minutes; the 30 minute cap leaves room to slow other phases after the next playtest without reopening the length.

Changed: `content/config.json`, `01_vision.md`, `02_show_flow.md`, `03_rules.md`, `15_schemas.md`, `19_acceptance.md`, `20_playtest.md`, `21_staging.md`, `Tests/EditMode/SimulatedGameTests.cs`.
