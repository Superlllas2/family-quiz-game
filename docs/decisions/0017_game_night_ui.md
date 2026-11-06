# Decision record 0017: the game night UI and the question bank v4

Date: Nov 6, 2026. Ilia rejected the first UI mockup ("looks like a simple AI UI", not a game, not a family gathering) and approved the second one ("I like the UI", the board game mockup), asked for it to be built in Unity "so it has no mistakes, doesn't overlap with anything and just works", for the phone page to get the same look, and for a new question bank of 60 topics with 50 questions each. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D100 | The look of the TV and phone UI | A family board game on a walnut table: every piece is cream card stock or printed ink with a thick ink outline and a solid cardboard edge, a little crooked; the question on a card with a topic ribbon, a number coin and a multiplier sticker; chunky A to D tiles; round player tokens; a clock token; a rubber stamp for the right answer; comic speech bubbles from the hosts' cube badges; a cinema ticket for the room code. No glass, no thin lines | The placeholder look of `06_phone_screens.md` and the TV views |
| D101 | Fonts | Unbounded ExtraBold for headings, numbers and tile letters, Nunito (ExtraBold, Black) for text, Caveat Bold for the odd hand written word. OFL Google Fonts with Cyrillic; static TMP assets made by `GameNightFonts`, woff2 subsets on the phone | D66 (Inter as the placeholder) |
| D102 | Answer colours | A `#E4572E` red, B `#3A6EA5` cobalt, C `#F2B33D` mustard, D `#4F9D45` green, the same on the TV and the phones; cream text on A, B and D, ink on C | The placeholder colours of `06_phone_screens.md` |
| D103 | The Answer shot | QuestionShow stays on `Stage` (the card on the board, D92). Answer cuts to `Wide`: the guests stay in view, the question card and the tiles sit under them, under 40 % of the frame. Still no cut during Answer | `Stage` for Answer in `21_staging.md` |
| D104 | The question's fact | Shown: on Reveal the question card flips over to "Why?" and the fact (`fact` in `questions.json`), and the Scores phase keeps it up above the score cards. A closest value Reveal needs the room for its ruler, so its fact shows in Scores only | "Shown or spoken after the reveal" in `15_schemas.md` |

## Details added while applying this (the AI's picks, not yet confirmed by Ilia)

| Detail | Where |
| --- | --- |
| Fixed zones so nothing ever covers anything: the hosts' bubbles own the top left (one row per speaking host), the clock and player tokens or a view's title ribbon own the top right, everything else stays under them (`TvLayout`, checked by `TvLayoutTests`) | Presentation |
| On Reveal each player's pick and points show on the sticker under their token ("A +970", "B Wrong") instead of the mockup's tokens on the tiles, which covered the option text | Presentation |
| Scores is a row of score cards in rank order (token, name, score, points sticker) that glide to their new places, instead of the mockup's winding track: the track showed only letters, and names must be readable from the sofa | Presentation |
| Player tokens carry the first letter of the name, two letters or a number where names share it (`TokenLabels`) | Presentation |
| The race, round end, results and poll keep their content and get the pieces, the title ribbon and the zones | Presentation |

## The question bank v4 (Ilia, Nov 6, 2026)

60 topics with 50 questions each, every one with a fact and a source, written and then checked against the web by one agent per topic. The topics are international ("not only for Americans"), with old and modern versions where that works (films, music, games, books) and Russian topics (Soviet cinema, Russian music, Russian history, the USSR, Russian politics, Across Russia, Soviet and Russian TV). Russian politics and conspiracy theories stay on history and trivia: politics up to about 2000, nothing on current leaders, wars or elections; a conspiracy theory is asked as what it claims, and the fact gives the real answer. Questions are as hard as they need to be so every generation learns something (Ilia: about 23 year olds this weekend, 40 to 50 year olds later). A topic vote still offers three topics drawn at random from the bank plus "A bit of everything". The topics and their scope are in `tools/content/bank/topics.json`; `tools/content/check_bank.py` checks each topic file.
