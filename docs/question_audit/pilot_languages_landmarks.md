# Refining pilot: languages and landmarks (Nov 7, 2026)

Step 1 of `refining_plan.md`, for Ilia to read before the other topics are refined. Each topic now has 20 questions in `tools/content/refined/<topic>.json`: 6 easy, 10 medium and 4 closest value, built into `content/questions_refined.json` (160 questions in 14 topics).

| Topic | Same | Rewrite | New | Set aside |
| --- | --- | --- | --- | --- |
| languages | 13 | 5 | 2 | 20 of 50 |
| landmarks | 16 | 4 | 0 | 11 of 50 |

**How the questions were chosen:**
- From the 50 v4 questions of the topic, after the set-aside below, the strongest by the audit's rubric, with Guessability weighted up (rule 3): "why" and "how" questions, and quantities you can estimate rather than years.
- No closest value question asks for a bare year. Two year questions became quantities with an anchor in the question: hangul (1446 became 24 letters) and the Rosetta Stone (1822 became 23 years after 1799).
- Easy questions any age can win (rule 4): robot, karaoke, sandwich, Brazil, Big Ben, Rushmore, Tower Bridge, the Disney castle.
- At most a few of each formula: languages has 6 "what does the word mean / where does it come from" questions out of 20, against about half before.
- Option lengths fixed where the correct option was the longest (Mandarin, Hungarian, tsundoku, London Bridge), and the Rushmore distractors made believable (chiefs, generals, inventors instead of astronauts and English kings).
- Every new or changed fact was checked on the web: Brazil and the 1494 treaty (Wikipedia, Portuguese language), the Bulgarian nod (Wikipedia, Nod (gesture), for the answer; 3seaseurope.com/bulgarians-shake-nod-heads for the head shake meaning yes), the Eiffel Tower's 330 m (Wikipedia), hangul's 24 letters (Wikipedia) and the "one morning" line (Jeong In-ji's 1446 postface, korea.net), the Rosetta Stone dates (Wikipedia), Neuschwanstein's 172 days and unfinished state (Wikipedia), the Burj Khalifa's spire over 240 m (Wikipedia). `same` questions keep v4's checked sources.

## The questions

### languages

| Id | Kind | Question | Answer | Refine | From |
| --- | --- | --- | --- | --- | --- |
| cur_languages_01 | easy | На каком языке впервые появилось слово «робот»? | На чешском | same | languages_002 |
| cur_languages_02 | easy | Что значит японское слово «караоке»? | «Пустой оркестр» | same | languages_009 |
| cur_languages_03 | easy | В честь кого назван сэндвич? | Английского графа | same | languages_011 |
| cur_languages_04 | easy | В каком языке вопрос начинают перевёрнутым знаком «¿»? | В испанском | same | languages_014 |
| cur_languages_05 | easy | Какой язык родной для самого большого числа людей на Земле? | Китайский | rewrite (options) | languages_004 |
| cur_languages_06 | easy | На каком языке говорят в Бразилии? | На португальском | new | |
| cur_languages_07 | medium | Слово «кетчуп» пришло из Азии. Каким соусом он, по основной версии, был изначально? | Рыбным | same | languages_019 |
| cur_languages_08 | medium | Слова «томат», «чили» и «койот» пришли к нам из языка… | Ацтеков | same | languages_020 |
| cur_languages_09 | medium | Что по-японски значит «цундоку»? | Копить книги, не читая | rewrite (options) | languages_026 |
| cur_languages_10 | medium | Какие языки — ближайшие родственники венгерского? | Хантыйский и мансийский | rewrite (options) | languages_027 |
| cur_languages_11 | medium | Сколько тонов в китайском языке путунхуа? | Четыре | same | languages_030 |
| cur_languages_12 | medium | Как выглядит вопросительный знак в греческом языке? | Как наша точка с запятой | same | languages_033 |
| cur_languages_13 | medium | Мы зовём знак @ «собакой». А как его называют итальянцы? | Улитка | same | languages_034 |
| cur_languages_14 | medium | К какому языку жестов ближе всего американский язык жестов? | К французскому | same | languages_035 |
| cur_languages_15 | medium | Чем удивителен язык сильбо с Канарского острова Гомера? | На нём свистят | same | languages_038 |
| cur_languages_16 | medium | В Болгарии вам в ответ кивают головой. Что это, скорее всего, значит? | «Нет» | new | |
| cur_languages_17 | closest | Сколько букв в гавайском алфавите? | 13 | same | languages_039 |
| cur_languages_18 | closest | Сколько живых языков насчитывают в Папуа — Новой Гвинее? | 840 | same | languages_049 |
| cur_languages_19 | closest | Создатели корейской азбуки уверяли, что умный выучит её за одно утро. Сколько в ней основных букв? | 24 | rewrite (year to count) | languages_044 |
| cur_languages_20 | closest | Розеттский камень нашли в 1799 году. Через сколько лет с его помощью прочли египетские иероглифы? | 23 | rewrite (year to span) | languages_047 |

### landmarks

| Id | Kind | Question | Answer | Refine | From |
| --- | --- | --- | --- | --- | --- |
| cur_landmarks_01 | easy | Что на самом деле называется Биг-Беном? | Колокол | same | landmarks_002 |
| cur_landmarks_02 | easy | Чьи лица высечены в скале горы Рашмор? | Четырёх президентов США | rewrite (options) | landmarks_003 |
| cur_landmarks_03 | easy | Чем была Пизанская башня по замыслу строителей? | Колокольней собора | same | landmarks_004 |
| cur_landmarks_04 | easy | Что делает Тауэрский мост в Лондоне, чтобы пропустить высокое судно? | Поднимает половинки пролёта | same | landmarks_009 |
| cur_landmarks_05 | easy | Для чего строили римский Колизей? | Для боёв гладиаторов | same | landmarks_011 |
| cur_landmarks_06 | easy | Какой замок вдохновил замок Спящей красавицы в Диснейленде? | Нойшванштайн | rewrite (fact) | landmarks_014 |
| cur_landmarks_07 | medium | Для чего задумывали шпиль Эмпайр-стейт-билдинг? | Для причаливания дирижаблей | same | landmarks_016 |
| cur_landmarks_08 | medium | Куда переехал старый Лондонский мост, проданный в 1968 году? | В Аризону | rewrite (options) | landmarks_017 |
| cur_landmarks_09 | medium | Что китайские строители добавляли в известковый раствор Великой стены? | Клейкий рис | same | landmarks_018 |
| cur_landmarks_10 | medium | Как Крайслер-билдинг внезапно стал самым высоким зданием мира? | Поднял шпиль, тайно собранный внутри | same | landmarks_023 |
| cur_landmarks_11 | medium | Писатель Мопассан, ненавидевший Эйфелеву башню, часто обедал в её ресторане. Почему? | Только оттуда её не видно | same | landmarks_024 |
| cur_landmarks_12 | medium | Шпиль Нотр-Дама, рухнувший при пожаре 2019 года, был построен в каком веке? | В XIX веке | same | landmarks_026 |
| cur_landmarks_13 | medium | Что идёт по синим трубам на фасаде парижского Центра Помпиду? | Воздух | same | landmarks_031 |
| cur_landmarks_14 | medium | Что появляется на ступенях пирамиды Кукулькана в Чичен-Ице в дни равноденствия? | Тень ползущего змея | same | landmarks_032 |
| cur_landmarks_15 | medium | Куда смотрит большинство статуй моаи на острове Пасхи? | Вглубь острова | same | landmarks_033 |
| cur_landmarks_16 | medium | Что находится на самой верхушке купола римского Пантеона? | Круглая дыра в небо | same | landmarks_035 |
| cur_landmarks_17 | closest | Сколько лет пирамида Хеопса оставалась самым высоким сооружением, построенным людьми? | 3800 | same | landmarks_050 |
| cur_landmarks_18 | closest | Сколько лет прошло от закладки до завершения Кёльнского собора? | 632 | same | landmarks_041 |
| cur_landmarks_19 | closest | Во сколько раз Сиднейский оперный театр обошёлся дороже первой сметы? | 15 | same | landmarks_048 |
| cur_landmarks_20 | closest | Эйфелева башня — 330 метров. Какой высоты небоскрёб Бурдж-Халифа в Дубае? | 828 м | rewrite (anchor, tolerance 80) | landmarks_044 |

**Best five** (audit score where there is one): Maupassant and the tower (86), the Great Pyramid's 3800 years (85), the Notre-Dame spire (84), @ as a snail (82), the Bulgarian nod (new, guessable by logic: why else would we ask?).
**Weakest five** (my judgment, not scored): the Mandarin and Colosseum questions (plain recognition, kept as warm-ups), Papua New Guinea's 840 languages (hard to estimate), the Hungarian relatives (needs a hint most families lack), the Pantheon's hole (a guess for anyone who hasn't been there).

## Set aside (rule 1: no Russia outside the Russian topics)

Nothing was deleted or changed in `tools/content/bank/`. The list lives here for now; see open question 3.

**languages, clearly about the Russian language (11):** 001 (letters in the Russian alphabet), 007 (the «а роза упала» palindrome), 012 («спасибо»), 013 (the Russian for "raining cats and dogs"), 015 (the «съешь же ещё» pangram), 017 (Karamzin and «ё»), 025 (Russian vowel letters), 028 (the old letter «веди»), 037 (the most common Russian letter), 040 (the yat reform), 046 (Dahl's dictionary).

**languages, words borrowed between Russian and another language (9), set aside but see open question 1:** 005 («бутерброд», German), 018 («вокзал», London's Vauxhall, audit 78), 022 ("babushka" in English), 023 («карандаш», Turkic), 024 ("mammoth" from Russian), 029 («не в своей тарелке», French), 031 («магазин», Arabic), 032 («парикмахер», German), 036 («икура» in Japanese).

**landmarks (11):** 008 (Kremlin stars), 013 (St Basil's), 021 (Ostankino), 025 (the Kremlin's Italian architects), 028 (Moscow State University), 029 (the Moskva pool), 037 (the Winter Palace's colour), 038 (Peterhof's fountains), 039 (Kremlin towers), 040 (St Isaac's), 049 (the Tsar Bell). They fit `ru_places` or `ussr` when those topics are refined.

**Not set aside, but not picked:** languages_016 (Saint Cyril's Glagolitic) and languages_050 (Cyril and Methodius) are about Slavic writing made in Moravia and Bulgaria, not about Russia. Judged not Russian, left out only to keep the topic's formulas varied.

## Duplicates found

Within the pilot and against the other 59 topics (the full bank) and the 120 refined questions:

| Pilot question | Repeats | Decision |
| --- | --- | --- |
| languages_003 (tsunami means "harbour wave") | earth_004's fact says the same | Left out of languages; earth keeps it |
| landmarks_033 (moai face inland) | mysteries_008's fact says the same | Landmarks keeps it; mysteries_008 needs another fact when `mysteries` is refined |
| landmarks_041 (Cologne, 632 years, the crane) | medieval_049 (Cologne finished in 1880, the same crane) | Landmarks keeps it; medieval_049 is replaced when `medieval` is refined |
| landmarks_014 (Neuschwanstein opened to tourists weeks after Ludwig's death) | rulers_015's fact | The landmarks fact now says the castle was unfinished instead |
| landmarks_019 (Angkor Wat on the flag) | countries_018 | Left out of landmarks; countries keeps it |
| languages_047 rewrite (the Rosetta Stone) | ancient_001 (which script the stone cracked) | Different questions, same object; ancient_001 should not mention the 23 years when refined |

Mild in-topic overlaps left as they are: the ketchup fact mentions a southern Chinese language (cur_languages_05's answer is Chinese), and the silbo fact mentions Spanish (cur_languages_04's answer). Neither gives away the other question.

## Open questions for Ilia

1. **Borrowed words.** The `languages` scope in `topics.json` says "loanwords between Russian and other languages", and the 9 borrowed-word questions are among the topic's best (vokzal scored 78). Rule 1 sets them aside. Should rule 1 allow a Russian word whose story is foreign (vokzal from Vauxhall, «не в своей тарелке» from French)? If yes, 3 or 4 of them would replace weaker questions here.
2. **Mostly "same" in a strong topic.** Landmarks is 16 same, 4 rewrite: its v4 questions were already good, so refining was mostly choosing. Weaker topics will need far more rewrites and new questions. Is this the level you want?
3. **Where the set-aside list lives.** The plan says a `setAside` field in `tools/content/bank/<topic>.json`. Adding it is safe only if `build_questions.py` keeps those questions in `content/questions.json`; dropping them would renumber every id after them and break `basedOn`. Proposal: one file, `tools/content/set_aside.json` (id and reason), and `build_refined.py` refuses a `basedOn` that is on it. OK?

## Tool changes

- `check_bank.py --refined` and `BankAndLastCallTests.EveryTopicHoldsTwoRounds` accept whole rounds: k times 3 easy, 5 medium, 2 closest value, so a topic of 10 and a topic of 20 both pass.
- `tools/validate_content.mjs` checks `questions.json`, not `questions_refined.json`; the refined bank is checked by `check_bank.py --refined`, `build_refined.py` and the TV's `ContentLoader`.

## Not done yet

- The twelve topics refined first still have 10 questions each; D130 wants 20. That is the plan's step 3, with the other topics.
- Steps 2 and 3 of the plan wait for your reading of this pilot.
