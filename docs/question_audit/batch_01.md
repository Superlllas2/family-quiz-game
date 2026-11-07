# Refining batch 1: the twelve topics already in the game, 10 to 20 questions (Nov 7, 2026)

Step 3 of `refining_plan.md`, batch 1. The twelve topics refined first (decision record 0020), which played on Nov 6, now hold 20 questions each: 6 easy, 10 medium, 4 closest value. With the pilot's two topics, `content/questions_refined.json` has **280 questions in 14 topics**. The rules are the plan's "Rules after the pilot".

## How it was done

1. **One editor per topic** (12 in parallel) worked from the topic's 50 v4 questions, the audit, and its current 10. They scored the current 10 too and improved or replaced any under 70. Every new or changed fact was checked on the web, and every subject was checked against all 3000 v4 questions, the other refined questions and the race statements. Each editor's report (scores, set aside, duplicates, sources) is kept with the session's working files.
2. **Independent scoring.** Each editor calibrated against the audit after already seeing the audit's numbers, so its own scores could not be the gate. Three readers who never saw the editors' scores scored all 240 questions blind. Each first scored its topics' 40 audited questions and corrected for its difference from the audit (+3.8, +3.3 and +0.3 points). They found **28 questions under 70 and 22 doubtful facts**.
3. **Second pass.** The editors improved or replaced the 28 and fixed or dropped the doubtful facts. The readers then scored all 51 new or changed questions. That found 3 more under 70, replaced at the end, and 8 more fact wordings, softened or cut.
4. **Script checks** on the result: `check_bank.py --refined`, `build_refined.py` (ids, set aside), a scan for repeats across the bank and the race, option-length tells, in-topic spoilers, bare-year closest value questions, and Russia keywords outside the Russian topics.

## Result

Scores are the independent readers' (the gate), not the editors'. All 240 are 70 or more; the average is 77.5.

| Topic | Same | Rewrite | New | Old 10 kept as is | Old 10 changed | Old 10 retired | Lowest | Average |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| kitchen | 6 | 9 | 5 | 5 | 1 | 4 | 70 | 79.5 |
| animals | 8 | 8 | 4 | 10 | 0 | 0 | 73 | 81.0 |
| brands | 9 | 9 | 2 | 9 | 0 | 1 | 72 | 82.0 |
| films_new | 6 | 9 | 5 | 4 | 1 | 5 | 70 | 77.7 |
| series | 3 | 10 | 7 | 6 | 2 | 2 | 70 | 76.7 |
| soviet_film | 4 | 10 | 6 | 7 | 1 | 2 | 70 | 75.3 |
| memes | 4 | 10 | 6 | 5 | 1 | 4 | 70 | 75.3 |
| games_new | 3 | 12 | 5 | 5 | 1 | 4 | 70 | 74.8 |
| crime | 2 | 14 | 4 | 6 | 1 | 3 | 70 | 76.4 |
| body | 7 | 9 | 4 | 8 | 1 | 1 | 70 | 76.7 |
| space | 8 | 5 | 7 | 5 | 0 | 5 | 70 | 76.4 |
| misconceptions | 7 | 10 | 3 | 4 | 3 | 3 | 72 | 77.7 |

**Of the 120 questions that played on Nov 6:** 74 stay exactly as they were, 12 changed with the same question and answer (a sharper option, a clue, a corrected or softer fact) and keep their ids, and 34 are retired. A retired id never comes back (`tools/content/refined_last_ids.json`), so the seen lists on the laptops and the Nov 6 ratings still point at the right questions.

**Why the 34 went:** the "no Russia" rule (Olivier salad, pelmeni, Russian Ark, the Black Cat gang, Tereshkova, Gagarin's parachute, the Orlan suit); a race statement saying the same thing (honey, Venus's day, Olympus Mons, the 10% brain); or a score under 70 on the new standard (mostly recognition questions with no trap, and a few closest value questions nobody can estimate).

**Facts corrected in questions that played on Nov 6:** the hedgehog fact said Pliny wrote about apples (it was grapes); the Kill Bill fact repeated a claim about footprints that was never confirmed; the hot dog record now names the Coney Island contest (a separate 2024 event had 83); the Caesar salad's leftovers story is now told as a legend.

## Set aside

31 more v4 questions are in `tools/content/set_aside.json`, now 53 in all:
- kitchen: borscht, Olivier, kholodets, Stroganoff, pelmeni, plombir, Molokhovets, Provansal.
- brands: the Moscow McDonald's.
- films_new: Russian Ark, Hardcore Henry.
- series: «Богатые тоже плачут», HBO's Chernobyl.
- memes: «Наташ, вставай», олбанский, Trololo, .ru.
- games_new: Metro 2033.
- crime: Ostap Bender, Sonka the Golden Hand.
- space: 10 questions on the Soviet space programme, which belongs to `ussr`.
- misconceptions: Alaska and the Lyube song.

## Repeats found (the other topic must not reuse these when it is refined)

| Kept here | The other question | Note |
| --- | --- | --- |
| cur_crime_05 (van Meegeren) | art_025 | Same fact and answer |
| crime: the Mona Lisa theft | art | Art must not ask about the 1911 theft |
| memes: Baby Shark | music_new_036's fact | Drop that sentence from music_new |
| films_new: the Burj Khalifa climb is left out | cur_landmarks_20 | |
| soviet_film: Brylska's singing voice left out | ru_music_030 | ru_music keeps it |
| space: weightlessness, spine | cur_body_06 | Space's "growing taller" question was replaced so an evening doesn't explain it twice |
| Left out because another topic has it | earth (tsunami), plants (saffron, chilli, vanilla, the Last of Us fungus), drinks (Red Bull), fashion (Puma), nineties (Google, the Pokémon seizures), history20 and inventions (the barcode), games_retro (Link), cartoons (Shrek), books_new, misconceptions_042 (the owl) | |

The race statements in `content/race.json` repeated 15 v4 questions in these topics, mostly in misconceptions, animals and space; none of those were used.

## For Ilia

1. **Plombir** (kitchen_034, audit 83): its story is French (a spa town), but the question only works in Russian, so the editor set it aside. Bring it back under the borrowed-word rule?
2. **Превед and Ждун** (memes_017, memes_022): Russian memes with foreign roots. They pass the borrowed-word test in one reading; the editor left them out to be safe.
3. **The kitchen scope line** in `tools/content/bank/topics.json` still says "Russian and Soviet food too", which rule 1 now overrides. OK to remove that clause?
4. **HBO's Chernobyl** (series_021) is set aside as a Soviet subject, although the show is British-American. Agree?
5. **Gagarin's parachute** (space_038, one of the audit's ten best) and the other Soviet space questions now wait for `ussr`.

## Notes

- The `kids` field (false for crime, conspiracy, some drinks) is not carried into the refined bank, but the game does not read it, so nothing changes in play.
- Soviet film: several new questions can be won by reasoning or from quotes still used as memes, for younger players (rule 4).
- Two closest value questions changed format (Twitch Plays Pokémon became a number; the pilot's Esperanto became multiple choice). Allowed: the fact is the same.
