# Plan: 3000 final race statements (D155, Nov 12, 2026)

**Status:** done Nov 12, 2026. `content/race.json` has **3077 statements** (`race_001` to `race_3078`; `race_001`–`race_100` are the D146 ones). Batch 1 was the pilot; Ilia then asked for batches 2 to 6 "straight through, all in one run". The game has 100 statements (`content/race.json`, D146); D155 asks for **3000**, so about 2900 new ones, written and checked in batches through the refining process (`refining_plan.md`), race statements first.

## What a good race statement is

The race is fast: 12 s to answer (`race.answer`), Правда or Ложь or Пас, −1 for a wrong answer (D157). The statement is read on the TV only (D162). So:

1. **One fact, read in 3 seconds.** Russian at most **90 characters** (the schema allows 120; the first 100 average 40). No lists, no "and also".
2. **Clearly true or clearly false.** No hedges a pedant could argue ("very", "most", "usually", "about") unless the fact itself is that hedge. A false statement is false by a wide margin (a wrong number by 2× or more, a swapped country, a myth), never by a technicality.
3. **Worth the risk.** With −1 per wrong answer, the fun is the hesitation: a surprising truth that sounds made up, or a myth everybody "knows". Avoid statements nobody could have an instinct about (a bare date, an obscure name) and statements everybody knows for sure.
4. **You can reason toward it.** The best ones let a player think "a horse can't… wait, why would it?".
5. **About half true.** Each batch file stays between 40 % and 60 % true, so "always say Ложь" never works.
6. **No negation tricks.** No "не" that flips the meaning, no double negatives. A myth is stated as the myth ("Быка злит красный цвет"), not as its denial.
7. **Something for every age.** Each area has statements a 10 year old can win (animals, food, cartoons, the body) next to adult ones.
8. **No Russia or the USSR** (D146, D130's direction): no Russian or Soviet people, places, food, films or language as the subject. Borrowed words with a foreign story are fine (`refining_plan.md`, rule 1).
9. **No repeats.** No fact already in `content/race.json`, in the refined questions (`content/questions_refined.json`, where a statement would give a question's answer away in the same evening) or in another statement. The same subject with a different fact is fine.
10. **A source** for every statement: a URL where the fact can be checked (Wikipedia is fine; the page must actually say it).

## The areas

About 32 areas, about 90 statements each, so no subject dominates an evening. Areas, in batch order:

| Batch | Areas |
| --- | --- |
| 1 (pilot) | animals, the human body, food and cooking, space, the ancient and medieval world, films and cartoons |
| 2 | sea life, insects and small creatures, plants, medicine and health myths, drinks, earth and weather |
| 3 | everyday physics, chemistry and materials, inventions, computers and the internet, modern history, explorers |
| 4 | countries and cities, landmarks and buildings, words and languages, art and museums, music, games and toys |
| 5 | sport, money and brands, records and numbers, household objects, holidays and customs, transport |
| 6 | dinosaurs and prehistory, misconceptions, then top up any area under its share |

## How each batch is made

1. **One writer per area**, in parallel. Each writes its statements into `tools/content/race/<area>.json` (no ids), checks every fact on the web and names the source, and checks every fact against `content/race.json`, the refined questions and the other area files.
2. **`python tools/content/check_race.py`**: the schema, the 90 character limit, sources, the true share per file, near-duplicates across all statements and the refined questions, and Russia keywords (each hit read by a person, not removed by the script).
3. **An independent checker per two areas** who never saw the writers' reasoning reads every statement: is it clearly true or false, is the fact right per its source, would a family enjoy it (1 to 5). Statements marked wrong, ambiguous or 1 to 2 go back to the writer once, then are dropped.
4. **`python tools/content/check_race.py --merge`** gives the new statements ids after the last one (`race_101` on) and appends them to `content/race.json`. An id is never reused or moved, so the seen lists on the laptops stay right.
5. **A short report** per batch here: counts, true share, what was dropped and why, 5 best and 5 weakest.

## Batch 1 report (Nov 12, 2026)

300 new statements, 50 per area, 52 % true (each area 50 to 54 %). The longest Russian line is 76 characters.

| Area | Facts wrong | Doubtful or arguable, fixed | Weak source, fixed | Too easy, replaced | Fun (checker, 1 to 5) |
| --- | --- | --- | --- | --- | --- |
| animals | 0 | 3 (koala, rabbits, a Russian-only pun) | 1 | 3 | 3.7 |
| body | 0 | 3 (sleepwalkers, 36.6, the gut's length) | 0 | 5 | 3.4 |
| food | 0 | 3 (instant noodles, black pasta, ketchup) | 1 | 7 | 3.5 |
| space | 0 | 3 (cars on the Moon, black holes, Ganymede) | 2 | 2 | 3.7 |
| ancient_medieval | 0 | 2 (silk smuggling, salting Carthage) | 1 | 4 | 3.3 |
| films | 0 | 3 (Snow White first, Woody's voice, Shirley Temple) | 0 | 8 | 3.2 |

Fun is the checker's score before the replacements. Writers dropped about 40 candidates on their own because sources disagreed (Nero's music in its first form, the Roman sponge, the Matrix sushi code, the Milky Way collision, spinach iron, Viking compass) or because a round question already used the fact.

**What the checks taught, for batches 2 to 6:**
- The race has no reveal text, so a statement must stand on its own: no fact whose truth depends on knowing an obscure exception (Snow White was "not the first" because of a lost 1917 film).
- The English line must work too: no Russian-only puns (ворон / ворона).
- "Comes from country X" and "the biggest planet" statements were the dullest; at most about 5 origin swaps per area.
- Words like «у всех», «всегда», «любимая» either give the answer away or invite an argument.

## Batches 2 to 6 report (Nov 12, 2026)

**2977 new statements in 73 area files** (`tools/content/race/`), plus the 100 from D146. 54 % true overall; every area file is between 45 % and 59 % true. Russian lines average 52 characters, the longest is 90. 2812 of the sources are Wikipedia pages; the rest are Guinness, Scientific American, NASA, Live Science, Mental Floss and similar.

**How:** each area was split into two halves written in parallel (45 statements each), then a second set of 45 for the pilot areas and nine new areas (books, mythology, TV series, famous people, geography, art, sea life, small creatures, everyday science) to reach 3000. Every file then went to an independent checker who had not written it and who **deleted** anything wrong, contested, arguable, stale or dull, fixed wording and sources, and checked repeats against everything merged so far. Writers had already dropped facts whose sources disagreed or that a round question uses.

**What the checkers removed:** of 3315 written in batches 2 to 6, about 340 (10 %) were deleted. No answer was found wrong. The deletions were facts with a contested source or rival "firsts" (the first TV picture, the first cash machine), wording a pedant could argue (Auckland "was" the capital, the Mongol Empire "twice" Rome), facts that go stale (the most popular baby name, Norway's EV share, Sagrada Família's build time), bare numbers, things everyone knows, cross-file repeats, and a few not right for a family evening (a chastity belt, Nike's slogan from an executed man's last words).

**Rules added during the run** (followed by the later writers and checkers): at most about 4 "comes from country X" statements per file; at most 2 statements on one subject (Pokémon, Everest, Newton, Japan…); nothing that depends on a yearly number; the English line must work on its own (no Russian-only puns). `check_race.py --merge <files>` merges only the named, checked files; its Russia keyword search no longer matches Rossini or Prussia.

| Area file | Kept | Area file | Kept | Area file | Kept |
| --- | --- | --- | --- | --- | --- |
| animals, animals_2 | 50, 40 | food, food_2 | 50, 42 | music_a, music_b | 43, 40 |
| body, body_2 | 50, 36 | drinks_a, drinks_b | 41, 41 | games_a, games_b | 39, 41 |
| space, space_2 | 50, 40 | plants_a, plants_b | 41, 39 | sport_a, sport_b | 43, 38 |
| ancient_medieval, _2 | 50, 38 | earth_a, earth_b | 36, 40 | money_a, money_b | 41, 42 |
| films, films_2 | 50, 37 | physics_a, physics_b | 37, 40 | records_a, records_b | 41, 38 |
| sea_a, sea_b, sea_2 | 43, 42, 42 | chem_a, chem_b | 40, 34 | household_a, household_b | 37, 37 |
| insects_a, insects_b, insects_2 | 45, 39, 43 | inventions_a, inventions_b | 41, 34 | holidays_a, holidays_b | 38, 39 |
| medicine_a, medicine_b | 41, 42 | computers_a, computers_b | 38, 42 | transport_a, transport_b | 42, 41 |
| modern_a, modern_b | 42, 39 | countries_a, countries_b, geo_2 | 40, 34, 40 | dinos_a, dinos_b | 40, 39 |
| explorers_a, explorers_b | 39, 41 | landmarks_a, landmarks_b | 39, 42 | myths_a, myths_b | 40, 42 |
| words_a, words_b | 44, 43 | art_a, art_b, art_2 | 41, 40, 41 | books_a, mythology_a | 41, 37 |
| series_a, people_a | 39, 43 | science_2 | 37 | | |

**Known limits:** the checker's repeat search is word based, so two statements on the same subject with different facts can still meet in one evening (136 such pairs were read and kept on purpose). Statements are drawn at random from the unseen ones, so an evening's race mixes all areas.

## Not in this plan

The 6000 round questions (about 90 new topics with shelf fields, D154, D155) come after the statements, with their own plan.
