# Plan: refine the whole question bank (draft for Ilia, Nov 7, 2026)

**Status:** steps 1 and 2 done, Nov 7: the pilot is `pilot_languages_landmarks.md` here, and Ilia's answers are in "Rules after the pilot" below. Step 3, batch 1 done Nov 7 (`batch_01.md`): the twelve topics already in the game grew to 20, so 14 topics and 280 questions are refined. Every batch question is gated by an independent scorer, not by its editor (see `batch_01.md`, "How it was done").

## Why

On Nov 6 the curated bank (120 questions, 12 topics, `questions_refined.json`) played better than the full bank (3000 questions, `questions.json`). Ilia decided:
- the curated bank is the default (`banks.default` is now `curated`);
- the other topics get the same refining, with no repeated or near-identical questions;
- Russian content stays in the Russian topics only;
- `ru_politics` is dropped;
- Russia-related questions in other topics are set aside, not deleted.

## What "refined" means

The same steps as the 12 curated topics (decision record 0020, D117), plus four new rules.

**The existing steps:**
- Start from the topic's questions in `tools/content/bank/<topic>.json`. Score them with the v4 audit rubric (`README.md` here): Familiarity, Curiosity, Guessability, Distractors, Social, Clarity, Payoff.
- Keep a question as it is (`refine: same`), rewrite it (`rewrite`), or write a new one (`new`), aiming for GOOD (70+).
- Facts are checked against sources. `check_bank.py --refined` and `build_refined.py` build the bank.

**The new rules:**
1. **No Russia in non-Russian topics.** A question whose subject is Russia or the USSR, its people, places, food, films or language goes to the set aside list.
   - This applies everywhere except the six Russian topics: soviet_film, ru_tv, ru_music, ussr, ru_history, ru_places.
   - A human-style review decides each one, not the keyword search. The keyword search found about 220 candidates, with false hits such as Florence Nightingale and the Crimean War.
2. **No repeats across the bank.**
   - A script compares every pair of questions for the same correct answer, the same subject and the same fact. `docs/question_audit/bank_checks.json` already lists likely duplicate facts.
   - A reviewer decides which of each pair stays.
3. **Questions you can reason toward.** The players asked for more questions that let you work your way to the answer and fewer "you know it or you don't". The rubric's Guessability gets more weight when choosing.
4. **Something for every age.** Each topic keeps a few questions younger players can win. The interview said Soviet film questions favour the older generation.

## Rules after the pilot (Ilia, Nov 7)

These replace anything above that disagrees.

1. **Borrowed words may stay.** A Russian word or saying in a non-Russian topic is fine when its story is foreign (вокзал from London's Vauxhall, бутерброд from German) and an English speaker would understand the question. Questions about the Russian language itself (its alphabet, spelling, idioms) are still set aside.
2. **A higher standard from now on.** Every refined question scores at least **70** (GOOD) on the audit rubric, easy questions included: an easy question earns it with a trap or a surprising fact. Scores use the audit's scale; a scorer whose scores run above the audit's on questions both scored subtracts the difference. Being strong in v4 is not enough: a question is kept as it is only if it already reaches 70, and otherwise rewritten or replaced.
   - Put the hook in the question when the surprise is hidden in the fact.
   - No closest value question asks for a bare year; give an anchor or ask for a quantity.
   - The correct option is never the single, much longer option.
   - No one formula fills more than about a quarter of a topic (the word questions of `languages` are the exception).
   - Every new or changed fact is checked on the web, with the source in the question.
3. **The set aside list** is `tools/content/set_aside.json` (id and reason). `build_refined.py` refuses a refined question based on a set aside one. The bank files and `content/questions.json` are not touched, so no id moves.

## Setting questions aside

- A set aside question keeps its place in `tools/content/bank/<topic>.json` and is listed in `tools/content/set_aside.json` with its reason (rule 3 above). The refined bank never uses it.
- So nothing is deleted. Removing the line brings a question back.
- `ru_politics` is set aside as a whole topic: no questions are built from it and it is left out of `topics.json`.
- This only changes the tools and the source files, not the game's schema.

## Size per topic

**At least 20 questions per topic** (Ilia, Nov 7). The curated topics have 10, so they grow too. With 59 topics that is about 1180 questions.

The mix of easy, medium and closest value questions scales from the curated mix of 3, 5 and 2 per 10.

## Steps

1. **Pilot, 2 topics,** for Ilia to read before anything else.
   - `languages`: the most Russia-related questions outside the Russian topics, 19 of 50.
   - `landmarks`: one of the audit's best topics, to show what "refined" means for a strong topic.
   - Output: the refined files, the set aside list with reasons, and the duplicate pairs found.
2. **Rules fixed after the pilot,** written into this file.
3. **The rest in batches of about 10 topics.** Each batch passes:
   - `check_bank.py --refined`;
   - the duplicate scan across everything refined so far;
   - a short report for Ilia: counts, set aside list, the 5 best and 5 weakest questions.
4. **After the last batch,** the refined bank holds all 59 topics. The full bank stays in the repo as a source until Ilia decides otherwise.

## Not in this plan

Picture and sound questions are a new question format, with two new topics: «Угадай мелодию» and «Угадай фильм», at least 20 questions each (Ilia, Nov 7). They need their own spec first.
