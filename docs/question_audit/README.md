# Question bank audit (v4, Nov 6, 2026)

An editorial audit of `content/questions.json` (3000 questions, 60 topics) for entertainment value, using the seven-criterion rubric of the Head Question Editor brief. `questions.json` was not changed. These are editorial heuristics, not a measure of fun; game night polls (pillar 5) are the real test.

## Files

| File | What it holds |
| --- | --- |
| `README.md` | Method, the bank-wide summary, best and worst questions, recommendations |
| `sample_verdicts.md` | Every sampled question: original text and options, seven scores, weighted score, classification, flags, verdict, action, rewrite with its score |
| `sample_audit.json` | The same as data, with the original IDs, for a later session that applies rewrites |
| `bank_checks.json` | Script checks over all 3000 questions: likely duplicate facts (the list mixes true duplicates with same-formula pairs; the true ones are in finding 5), telegraphed correct options, NOT-wording (has false positives, for example languages_029, olympics_020, books_old_007), long texts, question formula counts |

## Method

- **Scope (Ilia's choice): a calibrated sample.** 10 questions per topic (3 easy, 5 medium, 2 hard closest value, matching the 15/23/12 mix), 600 in all, drawn with a fixed seed. Per-question verdicts exist only for these 600. Bank-wide percentages below are estimates from the sample; the structural checks in `bank_checks.json` cover all 3000.
- **Scoring.** The lead editor scored a pilot and fixed seven anchor questions (92, 77, 65, 63, 56, 50, 41) that every scorer had to match. Four scoring agents took 15 topics each and gave raw 0 to 5 scores only; a script computed the weighted scores, classes and overrides. The four parts averaged 67.7, 68.8, 70.6 and 69.1, so no scorer drifted far from the others, though each part had different topics, so this is no proof of agreement. A script checked all 171 rewrites: none changes the correct answer (50 MC rewrites only shorten its wording), each MC rewrite has exactly one correct option, and every Russian text is 100 characters or less.
- **Closest value questions** have no wrong options, so "Distractor Quality" (10 %) was scored as **range and number quality** for them (Ilia's choice): can everyday estimation get close, and do the range and tolerance reward a good guess?
- **Audience.** Familiarity was scored for a Russian-speaking family who also know international pop culture (decision record 0017). Clarity and length were judged on the Russian text, because the game plays in Russian. The Russian topics are deliberate and were not flagged as cultural bias.
- **Weights:** Familiarity 15, Curiosity 20, Guessability 20, Distractors 10, Social 15, Clarity 10, Payoff 10. 85+ EXCELLENT, 70 to 84 GOOD, 55 to 69 NEEDS IMPROVEMENT, under 55 REJECT. Overrides (ambiguous or wrong cannot pass; no payoff and no guessability cannot be EXCELLENT; pure obscure memorisation cannot pass) were applied by script. None changed a class.
- **Facts** were not re-verified wholesale: the bank was already checked against sources by a second agent per topic. Scorers flagged only genuine doubts, and the lead editor checked those on the web.

## Summary (600 sampled questions)

| Measure | Value |
| --- | --- |
| Questions evaluated | 600 of 3000 (20 %) |
| Average score | 69.0 |
| Median score | 69 |
| EXCELLENT | 13 (2.2 %) |
| GOOD | 283 (47.2 %) |
| NEEDS IMPROVEMENT | 275 (45.8 %) |
| REJECT | 29 (4.8 %) |
| Action KEEP | 390 (65 %), of which 95 score under 70: 90 of them easy warm-up recognition questions kept on purpose |
| Action IMPROVE | 144 (24 %), each with a rewrite in Russian and English |
| Action REPLACE | 64 (10.7 %) |
| Action FACT-CHECK | 2, plus one REPLACE with a factual doubt |
| Rewrites written | 171 (also some cheap fixes on KEEPs); average 65.3 before, 71.1 after; 87 of 148 failing questions reach GOOD when rewritten |

Extrapolated to the bank: roughly 320 questions to replace and 700 worth rewriting. These are estimates with a margin of a few points.

**Average per criterion (0 to 5):** Familiarity 4.05, Curiosity 2.92, Guessability 3.12, Distractors/range 3.42, Social 2.99, Clarity 4.72, Payoff 3.75.

The bank is clear, familiar and well rewarded, thanks to the "Why?" facts. It is weakest on **surprise** and **social reaction**: half the questions ask something players either know or don't, and the fun is in the fact instead of the question.

**By difficulty**

| Difficulty | Average | Share 70+ | KEEP / IMPROVE / REPLACE |
| --- | --- | --- | --- |
| Easy (MC) | 67.7 | 37.8 % | 158 / 17 / 5 |
| Medium (MC) | 71.1 | 60.3 % | 186 / 81 / 33 |
| Hard (closest value) | 65.9 | 39.2 % | 46 / 46 / 26 (+2 FACT-CHECK) |

Easy questions score low because the rubric rewards surprise (easy Curiosity averages 1.8). That is a property of the rubric, not of the questions: a game needs warm-up recognition questions, and the scorers kept them. The real weak spot is the hard tier.

**By topic** (average of 10). Best: kitchen 76.2, animals 75.7, brands 75.6, landmarks 75.2, people 73.0, sea 72.6, languages 72.6. Weakest: series 63.7, board 64.4, records 64.7, holidays 65.3, medieval 65.4, ru_music 65.6, ru_tv 65.6, mysteries 65.8, mythology 66.1, ru_history 66.2. With 10 questions per topic these rankings are rough.

## The 10 strongest questions

| Score | ID | Question | Answer | Why it works |
| --- | --- | --- | --- | --- |
| 92 | brands_033 | What did the man who designed the Pringles can want done with his ashes? | Buried in a Pringles can | Absurd but true. The funniest option is right, and his children buying the can on the way to the funeral is a perfect kicker |
| 92 | kitchen_028 (anchor, scored by the lead editor) | Where was Hawaiian pizza with pineapple invented? | Canada | Everyone knows the pizza, "Hawaii" is a perfect trap, and the reveal starts an argument |
| 88 | animals_035 | Why does a sloth climb down from its tree about once a week? | To go to the toilet | Funny, surprising, and guessable with a grin |
| 87 | classical_017 | Chopin died in Paris. Where is his heart kept? | In a Warsaw church | Gruesome and romantic, with a jar of cognac smuggled by his sister to retell |
| 87 | ancient_019 | What did the Romans collect urine from public toilets for? | Washing clothes | A groan and a laugh, and "money does not smell" is a line people repeat |
| 87 | space_038 | How did Gagarin himself get back down to Earth? | On his own parachute | A hidden secret about a national hero; every option is plausible |
| 86 | landmarks_024 | Maupassant hated the Eiffel Tower yet often ate in its restaurant. Why? | The one spot he couldn't see it | A "why" question that wit can solve; the room laughs at the reveal |
| 86 | brands_010 | The Michelin stars for restaurants come from a company that makes… | Tyres | A famous award from an unexpected maker, with a clever business reason |
| 86 | science_045 | How many kilograms does all the air in a 4 × 5 m room with a 2.5 m ceiling weigh? | 60 | Everyday, estimable, and shocking (as heavy as an adult): the ideal closest value question |
| 85 | landmarks_050 | For how many years was the Great Pyramid the tallest structure built by people? | About 3800 | Known subject, staggering scale |

They share a pattern: a **familiar thing, a "why" or "how" with a twist, and an answer you can reason toward but that still surprises**. Close behind: nineties_019 (the NSA banned Furbies), drinks_019 (the USSR paid for Pepsi with vodka) and people_031 (Hawking's party for time travellers).

## The 10 weakest questions

| Score | ID | Question | Why it fails | Action |
| --- | --- | --- | --- | --- |
| 40 | rulers_050 | Merneptah inherited Ramesses II's throne. What number son was he? (13) | Unknown pharaoh, a number nobody can estimate; the unit is also empty | REPLACE |
| 41 | conspiracy_042 | How many years does Heribert Illig say were invented? (297) | Unknown theorist, a number nobody can estimate; it also repeats conspiracy_020 | REPLACE |
| 43 | drinks_039 | In what year did a Mongol khan send Tsar Michael tea? (1638) | An exact year of a diplomatic gift | REPLACE |
| 44 | fashion_043 | In what year did Louis Vuitton open his business in Paris? (1854) | A brand's founding year: an exam fact | REPLACE |
| 45 | countries_049 | In what year did Nigeria move its capital to Abuja? (1991) | An arbitrary year in a country few can place | REPLACE |
| 49 | series_023 | Who played the young Elizabeth II in The Crown? (Claire Foy) | Bare actor-name recall, no reasoning path | REPLACE |
| 50 | mysteries_045 | How many pages does the Voynich manuscript have? (240) | A half-known object and an arbitrary count | REPLACE |
| 50 | holidays_047 | In which year was Gävle's straw Christmas goat first built? (1966) | The funny part (it gets burned almost every year) is hidden in the fact | IMPROVE |
| 50 | olympics_040 | Paris 2024 was the Games of which Olympiad? (33) | Countable, but nobody cares | REPLACE |
| 51 | ru_tv_040 | In which year did What? Where? When? first air? (1975) | A founding year nobody can estimate or care about | REPLACE |

Nine of the ten are **closest value questions that ask for a bare year (5) or count (4)**. That is the bank's clearest weakness (below).

## Most common problems (sample)

| Flag | Count | Typical case |
| --- | --- | --- |
| OBVIOUS_ANSWER | 61 | The correct option is longer, more specific or worded differently; one distractor is a joke |
| SCHOOL_EXAM | 59 | Founding years, chemical elements, "which Olympiad" |
| NO_DEDUCTION | 41 | Bare names and numbers with nothing to reason from |
| BORING_REVEAL | 32 | The fact doesn't pay off the answer |
| OBSCURE_TRIVIA | 20 | Merneptah, Illig, Voynich page count |
| REPETITIVE | 14 | See duplicates below |
| TOO_WORDY | 10 | Long set-ups, mostly history and inventions |
| FACTUAL_RISK | 3 | See fact checks |
| AMBIGUOUS | 1 | science_037 (magnetic vs geographic pole wording) |
| CULTURAL_BIAS | 0 | |

## Bank-wide findings (all 3000, by script)

1. **Correct-answer position: no effect in play.** 1684 of the 2280 multiple choice questions store the correct option first, but `OptionShuffle` reorders the options every time a question is shown. Only someone hand-editing the file sees it.
2. **Telegraphed options.** In 773 of the 2280 MC questions (34 %, against 25 % by chance) the correct option is the single longest one in Russian. In 131 it is more than 1.5 times longer than every distractor, and in 41 much shorter. The IDs are in `bank_checks.json` (`telegraphedLongest`). This is the cheapest fix in the bank: shorten the correct option or lengthen the distractors.
3. **Closest value questions that ask for a year.** 150 of the 720 hard questions ask for a year. In the sample they average **57.4**, against 68.4 for other closest value questions, and 12 of 27 were marked REPLACE. A year can't be estimated by everyday sense unless the question gives an anchor ("…before or after the first moon landing?"). The good closest value questions ask for quantities people can picture: weights, distances, durations, counts.
4. **Question formulas.** "How many / how much" opens 375 questions (12.5 %), superlatives (first, biggest, most…) appear in 442, "which country" in 104, the name-or-word-origin formula ("what does X mean", "named after") in 100, "how old was X when he died" in 37. The etymology questions mostly play well, but 100 is a lot for one formula. A sample evening shouldn't hear "where does the name … come from" three times.
5. **Duplicate facts across topics.** The v4 pass replaced 57 repeats, but these are still in the bank (same fact or answer):
   - brands_050 = nineties_042 (the same Moscow McDonald's question word for word)
   - films_new_024 = cartoons_016 (Shrek, first animated feature Oscar)
   - history20_041 = conspiracy_046 (kg of Moon rock)
   - ru_history_032 = medicine_032 (Catherine the Great's smallpox inoculation)
   - sea_029 = ru_places_020 (Murmansk ice-free port)
   - holidays_006 = records_031 (Gloucestershire cheese rolling)
   - medieval_006 = people_015 (Marco Polo at Kublai Khan's court)
   - landmarks_019 = countries_018 (Angkor Wat on Cambodia's flag)
   - Close cousins: ussr_044 / world_047 (share of the Earth's land), ussr_050 / ru_places_003 (time zones), ussr_047 / olympics_047 (1980 medals), gadgets_047 / noughties_039 (SMS characters), crime_014 / crime_050 (D. B. Cooper)
   - Found by the scorers: medieval_023 / rulers_021 (Mansa Musa's gold), rulers_015 / landmarks_014 (Ludwig's castle), conspiracy_042 / conspiracy_020 (phantom time), three Mariana Trench questions in sea
6. **Spoilers inside a topic.** games_retro_041's fact names level 256, which answers games_retro_040. A round draws several questions from one topic, so a fact can spoil a later question the same evening. Worth a script that checks each fact against the other answers in its topic.
7. **Difficulty mix** is 15/23/12 per topic by design and was not judged. A few medium questions play like easy ones (mythology_034, nineties_024, ru_politics_022).
8. **Order of questions.** The order is chosen at play time, so there is no stored sequence whose thematic links could be judged.

## Fact checks

| ID | Doubt | Result |
| --- | --- | --- |
| films_old_044 | The fact says Julie Andrews beat Taylor's 65-costume record with 125 changes in *Star!* | Taylor's 65 costumes and the Guinness record are well sourced. The Julie Andrews record appears only on fan blogs, and other sources name Madonna in *Evita*. **Cut that sentence from the fact**; the answer 65 stands |
| crime_049 | "Expected about £1 million in Spanish pesetas" | Matches the cited Wikipedia article, but other accounts say about £3 million in cash. Safer wording: "they expected cash, not gold". Marked REPLACE anyway |
| kitchen_041 | De Gaulle's cheese count (246, 258 or 265 by source) | Not web-checked: the tolerance of 60 covers all three, so play is unaffected |

Also: rulers_050 has an empty `unit`.

## Recommendations

**Before game night 1 (Nov 7 to 8), cheap and safe:**
1. Remove one question of each true duplicate pair in finding 5, starting with the word-for-word brands_050 / nineties_042.
2. Cut the unverified Julie Andrews sentence from films_old_044's fact.
3. If the bank can be edited in time, drop the 64 REPLACE questions, if `tools/content/check_bank.py` allows fewer than 50 per topic or a different 15/23/12 split. A topic still keeps about 45 questions, enough for an evening.

**After game night 1:**
4. **Fix the hard tier first.** Rewrite or replace the 150 year questions. Ask for a quantity people can picture, or give an anchor in the question. The rewrites in `sample_verdicts.md` show the move: the hook from the "Why?" fact goes into the question.
5. **Move the hook into the question.** All four scorers found the same pattern independently: the surprising part is often in the fact while the question asks for a bare name or date (Gävle's goat, Ramesses outliving twelve sons, the Brink's-Mat robbers who expected cash). Asking about the surprise turns a recall question into a "wait, seriously?" question.
6. **Balance option lengths** in the 131 flagged questions (one script-guided pass).
7. **Vary the formulas.** Cap the "how many", superlative and name-origin formulas per topic, and add more "why" and "how" questions. "Why" questions dominate the top 10, yet only 114 of the 3000 (4 %) start with "why".
8. **Russian topics** (ru_history, ru_politics, ru_tv, ru_music, ru_places, medieval) score lowest. That isn't cultural bias: they lean on years and names. Story questions such as space_038 (Gagarin's parachute) and drinks_019 (Pepsi for vodka) show these subjects can be the best in the bank.
9. **Keep the easy warm-ups,** but give each a better "Why?" fact where it is thin (heroes_008, Batman's butler).
10. **Check every rewrite before it goes in.** The rewrites keep each question's fact and answer, but they were written by a model and are not source-checked. Run them through the same second-agent check the bank had, then through `tools/content/check_bank.py`.
11. **Audit the rest** (2400 questions) with the same brief and anchors only if the next game night poll suggests question quality, not pace or hosts, is what holds the fun back. The sample already shows where the problems are.
