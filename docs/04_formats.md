# 04 Question format catalog

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`). File fields are defined in `15_schemas.md`.

Game night 1 has three formats: multiple choice and closest value in rounds, true or false in the final race. Clue by clue is a stretch goal and is specified here so it can be added without new decisions.

## Multiple choice (`multipleChoice`)

| | |
| --- | --- |
| Used in | Round slots 1 to 3 |
| TV | During Answer: the question, very large, and no answer tiles (D150): the option texts are on the phones only (D141). Countdown, players' tokens. At Reveal the four options appear 2x2, A to D in the phone's colours, with the correct one marked (D142) |
| Phone | During QuestionShow: `watch`; the phone never shows the question (D162). During Answer: four big buttons in a 2x2 grid, same order, letter and color as the TV, option text on each button |
| Timer | 20 s (D140) times the player's timer multiplier, ends early when all locked in. Last call is off (`timers.lastCallEnabled`, D119) |
| Input | One tap locks the answer. No changing after the tap |
| Scoring | Speed curve, 500 to 1,000 points times the multiplier (`03_rules.md`) |
| Reveal | The option texts appear on the tiles; the correct tile grows with a cream halo and the stamp, wrong tiles fade; each player sits on the tile they chose with token, name and points (D142); each phone shows its own result |
| Options order | Shuffled per question with the seeded random generator; the same order on TV and all phones |

## Closest value in a range (`closestValue`)

| | |
| --- | --- |
| Used in | Round slot 4 (the hard question) |
| TV | Question, the range written as "from 1800 to 2000", the unit, countdown, character chips |
| Phone | A horizontal slider across the full width from `range.min` to `range.max`, a big number above it showing the current value with its unit, plus and minus buttons for fine steps of `step`, and a Lock button. The slider starts in the middle |
| Timer | 20 s times the player's timer multiplier, ends early when all locked in; last call cuts the last player to 5 s |
| Input | Move the slider, then tap Lock. No changing after Lock. If time runs out without Lock, no answer |
| Scoring | By distance: from 1000 at the exact band down to 0 at 3× the `tolerance` (D156), plus +200 for the closest (`03_rules.md`) |
| Reveal | The TV shows a number line with every player's guess as their character chip and the true answer as a flag; the closest chip gets a crown |

## True or false (`trueFalse`)

| | |
| --- | --- |
| Used in | The final race |
| TV | The statement, the race track with all players |
| Phone | Two huge buttons side by side: "Правда / True" on the left (green) and "Ложь / False" on the right (red), the same order and colours as the pair on the TV's statement card (D128, D129); a wide «Пас» button under them (D157) |
| Timer | 12 s times the player's timer multiplier, ends early when all answered. No last call |
| Scoring | Correct moves 1 step forward, wrong 1 step back (never below 0), Пас or no answer stays (D157). No points |
| Reveal | 2 s: the right answer is shown and players who were right move forward one step |

## Picture and sound questions (a `media` field)

**Status: Done, Nov 7, 2026.** Decisions D130 to D140 (`decisions/0025_one_bank_and_media_questions.md`). File fields are in `15_schemas.md`.

A picture or a sound clip is **not a format of its own**: it is an optional `media` field on a multiple choice or a closest value question (D139). The topics «Угадай мелодию» and «Угадай фильм» mix kinds freely, and so can any topic later. For example:
- multiple choice with a still: "Which film is this from?";
- multiple choice with a clip: "Which song is this?";
- closest value with a clip: "In which year did this song come out?";
- a plain question with no media: "How many Oscars did Titanic win?"

The round's slots are unchanged: slots 1 to 3 multiple choice, slot 4 closest value.

| | |
| --- | --- |
| Kinds | **Picture** (`image`): a film still, or an actor in a role ("In which film did she play this role?"). **Sound** (`audio`): a music clip (song, artist, film theme) or a film line (quote) |
| TV in QuestionShow | The question text on the card, as for any question. No picture and no sound yet, so nobody guesses before they can answer (D140) |
| TV in Answer | **Picture:** the still, large, on the card in place of the question text, with the question as a short line under it. It stays up until the end of Reveal. **Sound:** the question text, plus a "now playing" card with a moving sound wave while the clip plays (D138). The wave stops when the clip ends. The options, the ruler and the countdown are as for the question's format |
| Phone | As for the question's format: four text options 2×2, or the slider (D134). No picture and no sound on the phone |
| Clip | Starts when Answer starts and the options unlock (D135). Later, once the hosts read questions aloud, it starts after the host has read the question. It plays for at most 20 s (`media.length`) and stops when Answer ends, whichever comes first (D132). **No replay** |
| Music | The background music drops to `music.duckLevel` (0.2 of its volume, D136, D138) while a clip plays. It fades back over `music.fadeSeconds` after the clip, and never stops |
| Loudness | Every clip is normalised when it is downloaded, so no clip is much louder or quieter than another (D138) |
| Timer | Every answer window is 20 s (D140): `timers.answer.multipleChoice` and `timers.answer.closestValue` are both 20. A clip is never cut short by the answer window |
| Input, scoring, reveal | As for the question's format. The reveal keeps the still up |
| Files | The media is never in git. A fetch script downloads it onto the laptop before the game night into `<persistentDataPath>/media/` (D133): music previews from Deezer (iTunes as a fallback), stills from TMDB, lines and frames from Ilia's own copies with `ffmpeg`. The TV app plays from disk only. **A question whose file is missing is never picked**, and the content check reports it |

## Clue by clue (`clueByClue`), stretch goal

| | |
| --- | --- |
| Used in | Not in game night 1 unless finished by Friday. When enabled, it replaces round slot 3 in rounds 2 and 4 |
| TV | Three clues appear one by one, every 4 s, under a heading "Who or what is it?" |
| Phone | Four option buttons, tappable from the first clue |
| Timer | 12 s (3 clues × 4 s) |
| Scoring | Correct after clue 1: 1000, after clue 2: 700, after clue 3: 400, times the multiplier. Wrong: 0 |
