# Plan: refine the whole question bank (draft for Ilia, Nov 7, 2026)

**Status:** the pilot (step 1) is built, Nov 7: `pilot_languages_landmarks.md` here, with three open questions for Ilia. Steps 2 and 3 wait for his reading.

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

## Setting questions aside

- A set aside question keeps its place in `tools/content/bank/<topic>.json` with `"setAside": "<reason>"`. The build scripts leave it out of the game's files.
- So nothing is deleted. Removing the field brings a question back.
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
