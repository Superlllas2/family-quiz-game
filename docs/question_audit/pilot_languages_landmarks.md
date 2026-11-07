# Refining pilot: languages and landmarks (Nov 7, 2026)

Step 1 of `refining_plan.md`. Each topic has 20 questions in `tools/content/refined/<topic>.json`: 6 easy, 10 medium and 4 closest value, built into `content/questions_refined.json` (160 questions in 14 topics).

**Version 2, after Ilia's answers (Nov 7):** borrowed words are back in `languages`, every question now scores at least 70, and the set aside list is `tools/content/set_aside.json`. The rules this fixed are in `refining_plan.md` ("Rules after the pilot").

| Topic | Same | Rewrite | New | Set aside | Lowest score | Average |
| --- | --- | --- | --- | --- | --- | --- |
| languages | 11 | 5 | 4 | 11 of 50 | 70 | 75.1 |
| landmarks | 11 | 9 | 0 | 11 of 50 | 70 | 76.0 |

## How the questions were chosen

- The strongest of the topic's 50 v4 questions after the set aside, by the audit's rubric with Guessability weighted up (rule 3). Weak ones were rewritten or replaced until every question reached 70.
- **Scores.** "(audit)" is the v4 audit's own score, where the audit sampled that question. The rest are mine on the same rubric, minus 6: on the 10 questions both of us scored, mine ran 6 points above the audit's on average, so the correction keeps both on one scale. One scorer's numbers are rough to a few points.
- **The hook goes into the question** where the surprise was hidden in the fact: who designed the Statue of Liberty's skeleton (Eiffel), what wrecked the Parthenon (gunpowder), why the Burj Dubai was renamed, that moai are ancestors.
- **No closest value question asks for a bare year.** The Rosetta Stone (1822) became "how many years after 1799"; the Burj Khalifa got an anchor (the Eiffel Tower's 330 m).
- **For every age:** robot, karaoke, Brazil, Big Ben, Tower Bridge, the Golden Gate's colour, the Disney castle, Braille.
- **Formulas:** 11 of the 20 `languages` questions ask where a word comes from or what it means. The topic is about words, so this stays high, but the words come from 8 different languages, and the other 9 ask about scripts, gestures, sounds and numbers. Other topics keep any one formula to about a quarter.
- **Option lengths:** no correct option is the single longest by far. Fixed in babushka, tsundoku, London Bridge.
- **Facts checked on the web** for every new or changed fact: Brazil and the 1494 treaty (Wikipedia, Portuguese language); the Bulgarian nod (Wikipedia, Nod (gesture), and 3seaseurope.com for the head shake meaning yes); Braille at 15 and the awl (Wikipedia); the 45-letter word coined in 1935 (Wikipedia); the Rosetta Stone dates (Wikipedia); the Statue of Liberty's 2.4 mm copper and green within twenty years (Wikipedia); Machu Picchu around 1450 and 1911 (Wikipedia); "international orange" and Frémont's 1846 name (Wikipedia, Golden Gate Bridge and Golden Gate); Neuschwanstein's 172 days and unfinished state (Wikipedia); the Burj Khalifa's spire, the Eiffel Tower's 330 m and the renaming after Abu Dhabi's help (Wikipedia); the seven Ahu Akivi moai facing the sea (Wikipedia). `same` questions keep v4's checked sources.

## The questions

### languages

| Id | Kind | Question | Answer | Score | Refine | From |
| --- | --- | --- | --- | --- | --- | --- |
| cur_languages_01 | easy | На каком языке впервые появилось слово «робот»? | На чешском | 70 | same | languages_002 |
| cur_languages_02 | easy | Что значит японское слово «караоке»? | «Пустой оркестр» | 79 | same | languages_009 |
| cur_languages_03 | easy | В честь кого назван сэндвич? | Английского графа | 72 | same | languages_011 |
| cur_languages_04 | easy | Что значит «эсперанто» — имя, которое получил придуманный язык? | «Надеющийся» | 73 | rewrite | languages_042 |
| cur_languages_05 | easy | Из какого языка пришло слово «бутерброд»? | Из немецкого | 74 (audit) | same | languages_005 |
| cur_languages_06 | easy | На каком языке говорят в Бразилии? | На португальском | 75 | new |  |
| cur_languages_07 | medium | Слово «кетчуп» пришло из Азии. Каким соусом он, по основной версии, был изначально? | Рыбным | 75 | same | languages_019 |
| cur_languages_08 | medium | Слова «томат», «чили» и «койот» пришли из Мексики. Из языка какого народа? | Ацтеков | 73 | rewrite | languages_020 |
| cur_languages_09 | medium | Откуда в русском взялось слово «вокзал»? | От лондонского сада развлечений | 78 (audit) | same | languages_018 |
| cur_languages_10 | medium | Из какого языка японцы взяли слово «икура» — красная икра? | Из русского | 80 | same | languages_036 |
| cur_languages_11 | medium | Что ещё значит «babushka» в английском, кроме бабушки? | Головной платок | 76 (audit) | rewrite | languages_022 |
| cur_languages_12 | medium | Как выглядит вопросительный знак в греческом языке? | Как наша точка с запятой | 73 (audit) | same | languages_033 |
| cur_languages_13 | medium | Мы зовём знак @ «собакой». А как его называют итальянцы? | Улитка | 82 (audit) | same | languages_034 |
| cur_languages_14 | medium | Что по-японски значит «цундоку»? | Копить книги, не читая | 76 | rewrite | languages_026 |
| cur_languages_15 | medium | Чем удивителен язык сильбо с Канарского острова Гомера? | На нём свистят | 73 | same | languages_038 |
| cur_languages_16 | medium | В Болгарии вам в ответ кивают головой. Что это, скорее всего, значит? | «Нет» | 79 | new |  |
| cur_languages_17 | closest | Сколько букв в гавайском алфавите? | 13 | 72 (audit) | same | languages_039 |
| cur_languages_18 | closest | Луи Брайль придумал точечную азбуку для слепых, ещё будучи школьником. Сколько ему было лет? | 15 | 76 | new |  |
| cur_languages_19 | closest | Самое длинное слово в словарях английского языка — название болезни лёгких. Сколько в нём букв? | 45 | 74 | new |  |
| cur_languages_20 | closest | Розеттский камень нашли солдаты Наполеона в Египте в 1799 году. Через сколько лет с его помощью прочли иероглифы? | 23 | 72 | rewrite | languages_047 |

### landmarks

| Id | Kind | Question | Answer | Score | Refine | From |
| --- | --- | --- | --- | --- | --- | --- |
| cur_landmarks_01 | easy | Что на самом деле называется Биг-Беном? | Колокол | 75 | same | landmarks_002 |
| cur_landmarks_02 | easy | Статую Свободы США подарила Франция. А кто сконструировал её железный каркас? | Гюстав Эйфель | 79 | rewrite | landmarks_005 |
| cur_landmarks_03 | easy | Этот город инков веками скрывали джунгли, и мир узнал о нём только в 1911 году. Как он называется? | Мачу-Пикчу | 72 | rewrite | landmarks_007 |
| cur_landmarks_04 | easy | Что делает Тауэрский мост в Лондоне, чтобы пропустить высокое судно? | Поднимает половинки пролёта | 75 | same | landmarks_009 |
| cur_landmarks_05 | easy | Какого цвета мост Золотые Ворота в Сан-Франциско? | Оранжевый | 75 | rewrite | landmarks_012 |
| cur_landmarks_06 | easy | Какой замок вдохновил замок Спящей красавицы в Диснейленде? | Нойшванштайн | 72 (audit) | rewrite | landmarks_014 |
| cur_landmarks_07 | medium | Для чего задумывали шпиль Эмпайр-стейт-билдинг? | Для причаливания дирижаблей | 76 | same | landmarks_016 |
| cur_landmarks_08 | medium | Куда переехал старый Лондонский мост, проданный в 1968 году? | В Аризону | 76 | rewrite | landmarks_017 |
| cur_landmarks_09 | medium | Что китайские строители добавляли в известковый раствор Великой стены? | Клейкий рис | 72 | same | landmarks_018 |
| cur_landmarks_10 | medium | Как Крайслер-билдинг внезапно стал самым высоким зданием мира? | Поднял шпиль, тайно собранный внутри | 76 | same | landmarks_023 |
| cur_landmarks_11 | medium | Писатель Мопассан, ненавидевший Эйфелеву башню, часто обедал в её ресторане. Почему? | Только оттуда её не видно | 86 (audit) | same | landmarks_024 |
| cur_landmarks_12 | medium | Шпиль Нотр-Дама, рухнувший при пожаре 2019 года, был построен в каком веке? | В XIX веке | 84 (audit) | same | landmarks_026 |
| cur_landmarks_13 | medium | Небоскрёб в Дубае строили как «Бурдж Дубай», но в день открытия переименовали. В честь кого? | Правителя Абу-Даби | 79 | rewrite | landmarks_006 |
| cur_landmarks_14 | medium | Что появляется на ступенях пирамиды Кукулькана в Чичен-Ице в дни равноденствия? | Тень ползущего змея | 74 | same | landmarks_032 |
| cur_landmarks_15 | medium | Статуи моаи на острове Пасхи изображают предков. Куда смотрит большинство из них? | Вглубь острова | 73 | rewrite | landmarks_033 |
| cur_landmarks_16 | medium | Парфенон простоял почти целым две тысячи лет. Что разрушило его в 1687 году? | Взрыв пороха | 76 | rewrite | landmarks_015 |
| cur_landmarks_17 | closest | Сколько лет пирамида Хеопса оставалась самым высоким сооружением, построенным людьми? | 3800 | 85 (audit) | same | landmarks_050 |
| cur_landmarks_18 | closest | Сколько лет прошло от закладки до завершения Кёльнского собора? | 632 | 70 (audit) | same | landmarks_041 |
| cur_landmarks_19 | closest | Во сколько раз Сиднейский оперный театр обошёлся дороже первой сметы? | 15 | 72 | same | landmarks_048 |
| cur_landmarks_20 | closest | Эйфелева башня — 330 метров. Какой высоты небоскрёб Бурдж-Халифа в Дубае? | 828 | 72 | rewrite | landmarks_044 |

## Set aside

In `tools/content/set_aside.json` with a reason each. Nothing in `tools/content/bank/` or `content/questions.json` changed, so no id moved. `build_refined.py` refuses a refined question based on a set aside one.

- **languages (11), about the Russian language itself:** 001 (letters in the Russian alphabet), 007 (the «а роза упала» palindrome), 012 («спасибо»), 013 (the Russian for "raining cats and dogs"), 015 (the «съешь же ещё» pangram), 017 (Karamzin and «ё»), 025 (Russian vowel letters), 028 (the old letter «веди»), 037 (the most common Russian letter), 040 (the yat reform), 046 (Dahl's dictionary).
- **landmarks (11):** 008 (Kremlin stars), 013 (St Basil's), 021 (Ostankino), 025 (the Kremlin's Italian architects), 028 (Moscow State University), 029 (the Moskva pool), 037 (the Winter Palace's colour), 038 (Peterhof's fountains), 039 (Kremlin towers), 040 (St Isaac's), 049 (the Tsar Bell). They fit `ru_places` or `ussr` when those topics are refined.
- **Borrowed words are not set aside** (Ilia, Nov 7): a Russian word with a foreign story stays when an English speaker understands the question. Four are in the topic now (бутерброд, вокзал, икура, babushka); the other five (карандаш, мамонт, не в своей тарелке, магазин, парикмахер) remain available.

## Duplicates found

Against the other 59 topics (the full bank) and the 120 refined questions:

| Pilot question | Repeats | Decision |
| --- | --- | --- |
| languages_003 (tsunami means "harbour wave") | earth_004's fact | Left out of languages; earth keeps it |
| landmarks_033 (moai face inland) | mysteries_008's fact | Landmarks keeps it; mysteries_008 needs another fact when `mysteries` is refined (not mysteries_041's quarry fact either) |
| landmarks_041 (Cologne, 632 years, the crane) | medieval_049 (the same cathedral and crane) | Landmarks keeps it; medieval_049 is replaced when `medieval` is refined |
| landmarks_014 (Neuschwanstein opened to tourists weeks after Ludwig's death) | rulers_015's fact | The landmarks fact now says the castle was unfinished |
| landmarks_019 (Angkor Wat on the flag) | countries_018 | Left out of landmarks; countries keeps it |
| languages_047 rewrite (the Rosetta Stone) | ancient_001 (which script the stone cracked) | Different questions about one object; ancient_001 must not use the 23 years |
| languages_042 rewrite (Esperanto means "one who hopes") | | Esperanto appears once |

Inside a topic, two buildings appear twice from different sides: the Burj Khalifa (its renaming, its height) and the Eiffel Tower (Maupassant, and as the Burj's yardstick; also the answer to the Statue of Liberty question). None gives another's answer away, though seeing "Эйфелева башня" in one question is a small nudge for the Statue of Liberty one.

## Tool changes

- `check_bank.py --refined` and `BankAndLastCallTests` accept whole rounds: k times 3 easy, 5 medium, 2 closest value, so topics of 10 and 20 both pass. `TheShippedContentIsTheRefinedBankOnly` checks at least 120 questions and 12 topics.
- `tools/content/set_aside.json` and its check in `build_refined.py` (tested both ways).
- `tools/validate_content.mjs` checks `questions.json`, not `questions_refined.json`; the refined bank is checked by `check_bank.py --refined`, `build_refined.py` and the TV's `ContentLoader`.

## Next

Step 3 of the plan: the other topics in batches of about 10, each with the same report. The twelve topics refined first grow from 10 to 20 questions in those batches, under the same rules.
