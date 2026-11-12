# Plan: 3000 final race statements (D155, Nov 12, 2026)

**Status:** batch 1 (the pilot, 6 areas × 50) done Nov 12: `race.json` has 400 statements (`race_101` to `race_400`). Batches 2 to 6 wait for Ilia's read of the pilot. The game has 100 statements (`content/race.json`, D146); D155 asks for **3000**, so about 2900 new ones, written and checked in batches through the refining process (`refining_plan.md`), race statements first.

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

## Not in this plan

The 6000 round questions (about 90 new topics with shelf fields, D154, D155) come after the statements, with their own plan.
