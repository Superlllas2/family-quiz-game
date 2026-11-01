# 04 Question format catalog

Status: Done for game night 1 (Nov 2, 2026). File fields are defined in `15_schemas.md`.

Game night 1 has three formats: multiple choice and closest value in rounds, true or false in the final race. Clue by clue is a stretch goal and is specified here so it can be added without new decisions.

## Multiple choice (`multipleChoice`)

| | |
| --- | --- |
| Used in | Round slots 1 to 3 |
| TV | Question at the top (max 2 lines), four option tiles in a 2x2 grid labelled A, B, C, D, countdown bar, avatars along the bottom |
| Phone | During QuestionShow: question text only. During Answer: four big buttons in a 2x2 grid, same order, letter and color as the TV, option text on each button |
| Timer | 15 s, ends early when all locked in |
| Input | One tap locks the answer. No changing after the tap |
| Scoring | Speed curve, 500 to 1,000 points times the multiplier (`03_rules.md`) |
| Reveal | Correct tile turns green and grows, wrong tiles fade; each phone shows its own result |
| Options order | Shuffled per question with the seeded random generator; the same order on TV and all phones |

## Closest value in a range (`closestValue`)

| | |
| --- | --- |
| Used in | Round slot 4 (the hard question) |
| TV | Question, the range written as "from 1800 to 2000", the unit, countdown, avatars |
| Phone | A horizontal slider across the full width from `range.min` to `range.max`, a big number above it showing the current value with its unit, plus and minus buttons for fine steps of `step`, and a Lock button. The slider starts in the middle |
| Timer | 20 s, ends early when all locked in |
| Input | Move the slider, then tap Lock. No changing after Lock. If time runs out without Lock, no answer |
| Scoring | By distance with `tolerance`, plus +200 for the closest (`03_rules.md`) |
| Reveal | The TV shows a number line with every player's guess as their avatar and the true answer as a flag; the closest avatar gets a crown |

## True or false (`trueFalse`)

| | |
| --- | --- |
| Used in | The final race |
| TV | The statement, the race track with all players |
| Phone | Two huge buttons: "Правда / True" (top half, green) and "Ложь / False" (bottom half, red) |
| Timer | 8 s, ends early when all answered |
| Scoring | Correct moves 1 step. No points |
| Reveal | 2 s: the right answer is shown and players who were right move forward one step |

## Clue by clue (`clueByClue`), stretch goal

| | |
| --- | --- |
| Used in | Not in game night 1 unless finished by Friday. When enabled, it replaces round slot 3 in rounds 2 and 4 |
| TV | Three clues appear one by one, every 4 s, under a heading "Who or what is it?" |
| Phone | Four option buttons, tappable from the first clue |
| Timer | 12 s (3 clues × 4 s) |
| Scoring | Correct after clue 1: 1000, after clue 2: 700, after clue 3: 400, times the multiplier. Wrong: 0 |
