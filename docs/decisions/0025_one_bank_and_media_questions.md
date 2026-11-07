# Decision record 0025: one question bank, and picture and sound questions

Date: Nov 7, 2026. After the Nov 6 playtest, where the curated bank played better than the full bank (`playtests/2026-11-06/notes.md`, 11 and 12). The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D130 | Question banks | **One bank: the refined one** (`content/questions_refined.json`). The game no longer offers the full bank: no bank choice at start, no "Вопросы" choice in the pause menu. `content/questions.json` and `tools/content/bank/` stay in the repo as the source the refining works from (`docs/question_audit/refining_plan.md`); the game does not load them. Every topic in the refined bank gets at least 20 questions | D116 (two banks), D118 (the start choice) |
| D131 | New topics | Two new topics made of picture and sound questions: **«Угадай мелодию»** (a music clip: which song or artist) and **«Угадай фильм»** (a still or a film's music: which film). At least 20 questions each. More picture or sound question types may come later, for any topic | |
| D132 | How a clip plays | A sound clip plays while the question runs and lasts **at most 20 s**; it stops when the question ends, whichever comes first. **No replay.** A still stays on the TV for the whole question | |
| D133 | Where media comes from | Music previews from the **Deezer API** (free 30 s previews, no key), with the **iTunes Search API** as a fallback. Film stills from **TMDB** (free API key, credit TMDB in the game), and frames that `ffmpeg` grabs from Ilia's own copies where TMDB is thin. The media files are downloaded by a script onto each laptop before the game night, like `tools/fetch_models.mjs`, and are never committed. The TV app plays them from disk, never streaming during play. Private use only: the game is not published or sold with them | |

**Answered by Ilia on Nov 7, 2026:**

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D134 | The phone | The phone shows four text options, as for multiple choice. The picture or the clip is on the TV only | |
| D135 | When a clip starts | When the answer options unlock (Answer starts), not during QuestionShow. Later, once the hosts read questions aloud, the clip starts after the host has read the question | |
| D136 | Music under a clip | The background music gets quieter while a clip plays, and comes back after it. It does not stop. The level is a config value, set in the format spec | |
| D137 | Question kinds in «Угадай фильм» | Three kinds: (1) **a still**: which film is this from; (2) **a still of an actor in a role**: "In which film did she play this role?"; (3) **a sound**: the film's music or **a famous line (quote)**, cut from Ilia's own copies with `ffmpeg`: which film is this from | D133's "music and stills first" |

| D138 | Details | The background music drops to 0.2 of its volume under a clip (a config value). Every clip's loudness is normalised when it is downloaded. During a sound clip the TV shows the question text and a "now playing" card with a moving sound wave | |

| D139 | Media is a field, not a format | A picture or a clip is an optional `media` field on a multiple choice or a closest value question. The media topics mix kinds: picture, sound, and plain questions, including closest value ones such as "In which year did this song come out?". Round slots stay as they are | |
| D140 | Timing | A still appears with the options (Answer), not in QuestionShow. **Every answer window is 20 s**: multiple choice goes from 15 to 20 s, closest value stays 20 s, so a clip is never cut short. New keys `music.duckLevel` (0.2) and the question field `media` (`15_schemas.md`) are approved | D126's 15 s multiple choice window |

**Code to change for D130** (after a test run is possible):
- `BankIds`, `BanksConfig` and the `banks` config keys;
- `GameContent.Banks`, `TvSettings.Bank` and `TvSettingsRules`;
- the curated checks in `ContentLoader` against the full bank;
- `TvBankChooser` and the pause menu;
- the `bank` fields in `gameStarted.settings` and `questionRated` (`15_schemas.md`);
- the tests that cover them.

Until then, `banks.default` is `curated` and `banks.chooseOnStart` is `false`, so the game already behaves this way at start.
