# Question audit: per-question verdicts (600-question sample)

Generated from `sample_audit.json`. Scores: F Familiarity, C Curiosity, G Guessability, D Distractors (closest value: range and number quality), E Social, A Clarity, P Payoff, each 0 to 5. See `README.md` for the method and the summary.

## films_old

### films_old_001 (multipleChoice, easy): 81 GOOD, KEEP

- RU: Какого цвета волшебные туфельки Дороти в фильме «Волшебник страны Оз» (1939)?
- EN: What colour are Dorothy's magic slippers in the film The Wizard of Oz (1939)?
- Options: **Рубиново-красные | Ruby red** / Серебряные | Silver / Золотые | Gold / Изумрудные | Emerald green
- Scores: F 5, C 4, G 3, D 3, E 4, A 5, P 5
- Verdict: Iconic film plus a great reveal (silver in the book, red to show off Technicolor), and 'Silver' is a lovely trap; only flaw is the hyphenated 'Рубиново-красные', the one long, specific option. Cheap fix: plain 'Красные'.
- Rewrite (85 EXCELLENT): RU: Какого цвета волшебные туфельки Дороти в фильме «Волшебник страны Оз» (1939)? / EN: What colour are Dorothy's magic slippers in The Wizard of Oz (1939)?
  - Options: **Красные | Red** / Серебряные | Silver / Золотые | Gold / Изумрудные | Emerald green
  - Why better: Single colour words remove the length tell, so the book's 'silver' trap actually bites.

### films_old_002 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: В каком фильме звучит знаменитое «I'll be back» («Я вернусь»)?
- EN: Which film gave us the famous line "I'll be back"?
- Options: **«Терминатор» | The Terminator** / «Рэмбо» | Rambo / «Хищник» | Predator / «Крепкий орешек» | Die Hard
- Scores: F 5, C 1, G 3, D 4, E 3, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Fine easy recognition warm-up; KEEP as such. The behind-the-scenes fact (Arnold struggled to say "I'll" and wanted "I will be back") is stronger than the question and could become its own question.

### films_old_012 (multipleChoice, easy): 63 NEEDS IMPROVEMENT, KEEP

- RU: Что киллер Леон из фильма Люка Бессона (1994) повсюду носит с собой?
- EN: What does the hitman Léon in Luc Besson's 1994 film carry everywhere?
- Options: **Комнатное растение | A houseplant** / Кошку | A cat / Аквариум с рыбкой | A goldfish bowl / Канарейку | A canary
- Scores: F 4, C 2, G 2, D 4, E 3, A 5, P 4
- Verdict: Satisfying recognition for anyone who saw Léon, with a touching 'roots' fact; those who did not see it can only guess. KEEP as an easy recognition question.

### films_old_032 (multipleChoice, medium): 74 GOOD, KEEP

- RU: Почему фильм «В джазе только девушки» (1959) сняли чёрно-белым?
- EN: Why was Some Like It Hot (1959) shot in black and white?
- Options: **В цвете грим мужчин смотрелся жутко | Colour made the men's make-up ghastly** / Так потребовала Мэрилин Монро | Marilyn Monroe demanded it / Цветная плёнка была запрещена | Colour film was banned / Не хватило денег | There was no money
- Scores: F 4, C 4, G 3, D 3, E 4, A 4, P 4
- Verdict: Fun 'why' question: men in drag make-up looking ghastly in colour is a funny image, and the Monroe distractor is a clever half-truth (her contract demanded colour). 'Colour film was banned' is a throwaway.

### films_old_027 (multipleChoice, medium): 56 NEEDS IMPROVEMENT, REPLACE

- RU: На чём Одри Хепбёрн и Грегори Пек катаются по Риму в «Римских каникулах» (1953)?
- EN: What do Audrey Hepburn and Gregory Peck ride around Rome in Roman Holiday (1953)?
- Options: **На мотороллере «Веспа» | A Vespa scooter** / На велосипеде | A bicycle / В конной коляске | A horse carriage / На кабриолете | A convertible
- Scores: F 4, C 1, G 3, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER, BORING_REVEAL
- Verdict: 'Rome + 1953 + romance' points straight at the Vespa even for those who never saw the film, and the fact (Hepburn's first Oscar) is a standard bio line unrelated to the question.

### films_old_017 (multipleChoice, medium): 74 GOOD, KEEP

- RU: В «Новых временах» (1936) зрители впервые услышали голос Чаплина. Что он спел?
- EN: Modern Times (1936) was the first time audiences heard Chaplin's voice. What did he sing?
- Options: **Песенку на выдуманной тарабарщине | A song in made-up gibberish** / Ковбойскую балладу | A cowboy ballad / Английскую колыбельную | An English lullaby / Итальянскую арию | An Italian aria
- Scores: F 4, C 4, G 3, D 3, E 4, A 4, P 4
- Verdict: Lovely payoff: the first time Chaplin speaks, he sings nonsense. The correct option is the longest and the obviously 'Chaplin-ish' one, but the reveal still delights.

### films_old_018 (multipleChoice, medium): 84 GOOD, KEEP

- RU: Какой фразы на самом деле НЕТ в фильме «Касабланка» (1942)?
- EN: Which famous line is NOT actually said in Casablanca (1942)?
- Options: **«Сыграй это ещё раз, Сэм» | "Play it again, Sam"** / «За тебя, малыш» | "Here's looking at you, kid" / «Задержите обычных подозреваемых» | "Round up the usual suspects" / «У нас всегда будет Париж» | "We'll always have Paris"
- Scores: F 4, C 5, G 3, D 5, E 4, A 4, P 5
- Verdict: Classic misquote question: every option sounds like Casablanca, the famous one is the fake, and the Woody Allen origin is a great 'tell someone' fact. The NOT wording is justified here.

### films_old_034 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Что постановил суд насчёт «Носферату» (1922) — пиратской экранизации «Дракулы»?
- EN: What did a court order about Nosferatu (1922), an unlicensed Dracula adaptation?
- Options: **Уничтожить все копии | Destroy every copy** / Переименовать вампира | Rename the vampire / Переснять финал | Reshoot the ending / Показывать только ночью | Show it only at night
- Scores: F 3, C 4, G 3, D 3, E 3, A 4, P 4
- Verdict: Strong story (a court ordered a film destroyed, it survived anyway), but the question does not explain why a court was involved and 'show it only at night' is a throwaway.
- Rewrite (72 GOOD): RU: «Носферату» (1922) сняли по «Дракуле» без разрешения. Что за это решил суд? / EN: Nosferatu (1922) adapted Dracula without permission. What did the court order?
  - Options: **Уничтожить все копии | Destroy every copy** / Переименовать вампира | Rename the vampire / Отдать вдове Стокера сборы | Give Stoker's widow the takings / Вырезать все сцены с вампиром | Cut every vampire scene
  - Why better: The setup tells players it was a pirate film, and a credible money option replaces the joke one, so the drastic real verdict lands as a surprise.

### films_old_044 (closestValue, hard): 67 NEEDS IMPROVEMENT, FACT-CHECK

- RU: Сколько костюмов сшили для Элизабет Тейлор в фильме «Клеопатра» (1963)?
- EN: How many costumes were made for Elizabeth Taylor in Cleopatra (1963)?
- Answer: **65** costumes, range 0 to 300, tolerance 20
- Scores: F 4, C 3, G 3, D 3, E 3, A 5, P 3. Flags: FACTUAL_RISK
- Verdict: Decent estimation (dozens of outfits for a queen) with a fair range; the record-breaking angle in the fact is the payoff, so it must be right.
- Fact check: The fact says Julie Andrews beat Taylor's 65-costume Guinness record five years later with 125 changes in Star! (1968). Many sources instead credit Madonna in Evita (1996, 85 changes) as the record breaker; verify which claim the source supports before keeping the 'five years later' line. The answer 65 itself looks fine.
- Lead check (web): Taylor's 65 costumes and the Guinness record are well sourced; the Julie Andrews "125 changes in Star!" record is found only on fan blogs, and other sources name Madonna in Evita as the record breaker. Cut that sentence from the fact.

### films_old_045 (closestValue, hard): 83 GOOD, KEEP

- RU: «Челюсти» планировали снять за 55 съёмочных дней. Сколько дней ушло на самом деле?
- EN: Jaws was scheduled for 55 days of shooting. How many did it actually take?
- Answer: **159** days, range 55 to 500, tolerance 30
- Scores: F 5, C 4, G 3, D 4, E 4, A 5, P 5
- Verdict: Great closest value: the 55-day anchor invites reasoning about overruns, the near-triple answer surprises, and Spielberg fleeing the crew is a perfect reveal.

## films_new

### films_new_006 (multipleChoice, easy): 60 NEEDS IMPROVEMENT, KEEP

- RU: В какой телеигре выигрывает юноша из Мумбаи в «Миллионере из трущоб» (2008)?
- EN: Which TV quiz does the young man from Mumbai win in Slumdog Millionaire (2008)?
- Options: **«Кто хочет стать миллионером?» | Who Wants to Be a Millionaire?** / «Слабое звено» | The Weakest Link / «Своя игра» | Jeopardy! / «Колесо фортуны» | Wheel of Fortune
- Scores: F 4, C 1, G 4, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: The film title contains "Millionaire", so the answer is given away; KEEP only as a free easy point. The more surprising fact (the novel was written by an Indian diplomat) is buried in the reveal and could carry its own question.

### films_new_003 (multipleChoice, easy): 74 GOOD, KEEP

- RU: В 2017 году на «Оскаре» по ошибке объявили не тот лучший фильм. Какой назвали?
- EN: At the 2017 Oscars the wrong Best Picture was read out. Which film was announced?
- Options: **Ла-Ла Ленд | La La Land** / Прибытие | Arrival / Манчестер у моря | Manchester by the Sea / Скрытые фигуры | Hidden Figures
- Scores: F 4, C 3, G 3, D 4, E 4, A 5, P 4
- Verdict: A famous TV blunder that people half-remember, credible Best Picture rivals, and the envelope mix-up explanation is a satisfying reveal.

### films_new_013 (multipleChoice, easy): 68 NEEDS IMPROVEMENT, KEEP

- RU: Кто спел заглавную песню к фильму «007: Координаты „Скайфолл“» (2012)?
- EN: Who sang the title song of the Bond film Skyfall (2012)?
- Options: **Адель | Adele** / Сэм Смит | Sam Smith / Билли Айлиш | Billie Eilish / Шакира | Shakira
- Scores: F 5, C 1, G 4, D 4, E 3, A 5, P 3
- Verdict: Good easy recognition with smart distractors (Sam Smith and Billie Eilish really sang Bond themes), but little surprise. KEEP as a deliberate easy question.

### films_new_036 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Чем уникален «Русский ковчег» Сокурова, снятый в Эрмитаже в 2001 году?
- EN: What is unique about Sokurov's Russian Ark, shot in the Hermitage in 2001?
- Options: **Весь фильм — один кадр | The film is one unbroken shot** / Он немой | It is silent / Он снят при свечах | It was lit only by candles / Он снят задом наперёд | It was filmed backwards
- Scores: F 3, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: Impressive Russian film feat with plausible gimmick distractors, and the 'three failed tries, fourth worked' detail is a strong payoff.

### films_new_035 (multipleChoice, medium): 64 NEEDS IMPROVEMENT, REPLACE

- RU: Песня «Naatu Naatu» в 2023 году получила «Оскар». Из фильма какой страны она?
- EN: The song Naatu Naatu won an Oscar in 2023. Which country's film is it from?
- Options: **Индия | India** / Южная Корея | South Korea / Турция | Turkey / Бразилия | Brazil
- Scores: F 3, C 2, G 4, D 3, E 3, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: The song title sounds Indian and the viral RRR dance makes India the reflex answer; the reveal adds only a 'first Asian song' record.

### films_new_033 (multipleChoice, medium): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: Кто первой из женщин получил «Оскар» за режиссуру?
- EN: Who was the first woman to win the Oscar for Best Director?
- Options: **Кэтрин Бигелоу | Kathryn Bigelow** / София Коппола | Sofia Coppola / Грета Гервиг | Greta Gerwig / Джейн Кэмпион | Jane Campion
- Scores: F 3, C 3, G 2, D 4, E 3, A 5, P 4. Flags: NO_DEDUCTION
- Verdict: Name recall with little reasoning, but the fact (she beat her ex-husband Cameron and Avatar) is a great hook that should be in the question.
- Rewrite (71 GOOD): RU: Первая женщина с «Оскаром» за режиссуру обошла в тот год бывшего мужа — Кэмерона. Кто она? / EN: The first woman to win Best Director beat her ex-husband, James Cameron, that year. Who is she?
  - Options: **Кэтрин Бигелоу | Kathryn Bigelow** / София Коппола | Sofia Coppola / Грета Гервиг | Greta Gerwig / Джейн Кэмпион | Jane Campion
  - Why better: The ex-husband rivalry gives the table a story to react to and a clue (who was married to Cameron?) to reason from.

### films_new_027 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Жёлтый комбинезон Невесты в «Убить Билла» — дань уважения какой звезде?
- EN: The Bride's yellow jumpsuit in Kill Bill is a tribute to which star?
- Options: **Брюс Ли | Bruce Lee** / Джеки Чан | Jackie Chan / Чак Норрис | Chuck Norris / Джет Ли | Jet Li
- Scores: F 4, C 2, G 4, D 4, E 3, A 5, P 4
- Verdict: Satisfying recognition: the yellow jumpsuit is iconic, all four options are martial arts stars, and the unfinished Game of Death adds a poignant note.

### films_new_019 (multipleChoice, medium): 76 GOOD, KEEP

- RU: Что вместо пальцев у людей в одной из вселенных фильма «Всё везде и сразу»?
- EN: In one universe of Everything Everywhere All at Once, what do people have instead of fingers?
- Options: **Сосиски | Hot dogs** / Морковки | Carrots / Карандаши | Pencils / Ложки | Spoons
- Scores: F 3, C 5, G 2, D 4, E 5, A 5, P 3
- Verdict: Absurd and very funny for anyone who saw it, and the options make everyone laugh; the fact is a generic Oscar tally, and older players may not know the film.

### films_new_040 (closestValue, hard): 78 GOOD, KEEP

- RU: На съёмках «Аватар: Путь воды» Кейт Уинслет задержала дыхание под водой. На сколько секунд?
- EN: Filming Avatar: The Way of Water, Kate Winslet held her breath underwater. For how many seconds?
- Answer: **434** seconds, range 30 to 1200, tolerance 90
- Scores: F 4, C 5, G 3, D 3, E 4, A 4, P 4
- Verdict: Jaw-dropping number (over 7 minutes) that players can compare to their own breath-holding; the tolerance is fair, though seconds make the slider awkward.

### films_new_039 (closestValue, hard): 78 GOOD, KEEP

- RU: Сколько всего «Оскаров» получила трилогия «Властелин колец»?
- EN: How many Oscars did the Lord of the Rings trilogy win in total?
- Answer: **17** Oscars, range 0 to 60, tolerance 7
- Scores: F 5, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Well-known trilogy, reasonable estimation path (many know Return of the King swept), fair range and tolerance; solid hard question.

## soviet_film

### soviet_film_005 (multipleChoice, easy): 59 NEEDS IMPROVEMENT, KEEP

- RU: Кто НЕ входит в знаменитую тройку из комедий Гайдая?
- EN: Who is NOT part of the famous trio from Gaidai's comedies?
- Options: **Хитрец | Sly** / Трус | Coward / Балбес | Dunce / Бывалый | Experienced
- Scores: F 5, C 0, G 5, D 2, E 2, A 4, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Every Russian player knows Coward, Dunce and Experienced, so "Sly" is a free point; KEEP only as an easy warm-up. The Nikulin-was-a-circus-clown fact would make a better question of its own.

### soviet_film_003 (multipleChoice, easy): 66 NEEDS IMPROVEMENT, KEEP

- RU: Куда Женя Лукашин с друзьями ходит каждый год 31 декабря?
- EN: Where do Zhenya Lukashin and his friends go every year on December 31?
- Options: **В баню | To the bathhouse** / На каток | To the ice rink / В ресторан | To a restaurant / На рыбалку | Fishing
- Scores: F 5, C 1, G 4, D 3, E 3, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: The title 'С лёгким паром' gives it away and the fact only restates that; KEEP as a deliberate easy warm-up everyone will get.

### soviet_film_015 (multipleChoice, easy): 62 NEEDS IMPROVEMENT, KEEP

- RU: Как подчинённые за глаза зовут Калугину в «Служебном романе»?
- EN: What do the staff call their boss Kalugina behind her back in Office Romance?
- Options: **Мымра | The Frump** / Снежная королева | The Snow Queen / Железная леди | The Iron Lady / Кобра | The Cobra
- Scores: F 5, C 1, G 3, D 4, E 3, A 5, P 2. Flags: BORING_REVEAL
- Verdict: Warm recognition ('Мымра' gets a laugh), but the fact about the play is dry. KEEP as an easy recognition question.

### soviet_film_025 (multipleChoice, medium): 84 GOOD, KEEP

- RU: Что Гайдай поставил в финал «Бриллиантовой руки», но по требованию начальства вырезал?
- EN: What did Gaidai put at the end of The Diamond Arm, only to be told to cut it?
- Options: **Ядерный взрыв | A nuclear explosion** / Танец живота | A belly dance / Погоню на танке | A tank chase / Высадку на Луну | A Moon landing
- Scores: F 5, C 5, G 2, D 4, E 5, A 4, P 5
- Verdict: A beloved comedy with a genuinely shocking cut ending, absurd-but-plausible distractors, and Gaidai's deadpan excuse as a perfect reveal.

### soviet_film_017 (multipleChoice, medium): 53 REJECT, REPLACE

- RU: Какую награду получил первый фильм Тарковского «Иваново детство»?
- EN: Which prize did Tarkovsky's first feature, Ivan's Childhood, win?
- Options: **«Золотого льва» Венеции | Venice's Golden Lion** / «Золотую пальмовую ветвь» | The Palme d'Or / «Оскар» | An Oscar / «Золотого медведя» Берлина | Berlin's Golden Bear
- Scores: F 3, C 2, G 2, D 4, E 2, A 5, P 2. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: A festival-prize lookup with nothing to reason from, and the fact (he got the project after another director failed) is mild.

### soviet_film_020 (multipleChoice, medium): 75 GOOD, KEEP

- RU: «Москва слезам не верит»: Гоша, он же Гога, он же…?
- EN: Moscow Does Not Believe in Tears: Gosha, also known as Goga, also known as…?
- Options: **Жора | Zhora** / Гена | Gena / Гриша | Grisha / Вова | Vova
- Scores: F 5, C 2, G 4, D 4, E 4, A 5, P 3
- Verdict: Everyone will say the quote along; non-fans can deduce that all forms come from Georgy. A warm, quotable recognition question.

### soviet_film_034 (multipleChoice, medium): 62 NEEDS IMPROVEMENT, REPLACE

- RU: Какой российский фильм получил «Оскар» в 1995 году?
- EN: Which Russian film won an Oscar in 1995?
- Options: **«Утомлённые солнцем» | Burnt by the Sun** / «Брат» | Brother / «Особенности национальной охоты» | Peculiarities of the National Hunt / «Вор» | The Thief
- Scores: F 3, C 2, G 3, D 4, E 3, A 5, P 3. Flags: NO_DEDUCTION
- Verdict: A medium-slot Oscar lookup: recognition for the 40-50 group, a guess for the 23-year-olds, and no same-answer reframing lifts it. The Nadya-plays-his-daughter fact is nice and could seed a different question.

### soviet_film_029 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой кремль сыграл царский Кремль в «Иван Васильевич меняет профессию»?
- EN: Which kremlin stood in for the tsar's Kremlin in Ivan Vasilievich Changes Profession?
- Options: **Ростовский | Rostov Veliky's** / Московский | Moscow's / Казанский | Kazan's / Тульский | Tula's
- Scores: F 4, C 3, G 3, D 3, E 3, A 5, P 3
- Verdict: Fun filming-location fact, but 'Moscow' is a near-throwaway and Tula is random; the reveal is modest.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Какой кремль «сыграл» Московский Кремль XVI века в «Иван Васильевич меняет профессию»? / EN: Which kremlin played the 16th century Moscow Kremlin in Ivan Vasilievich Changes Profession?
  - Options: **Ростовский | Rostov Veliky's** / Новгородский | Novgorod's / Казанский | Kazan's / Суздальский | Suzdal's
  - Why better: Four old-Russian-city kremlins are all credible film locations, so players argue about which one looked most like Moscow.

### soviet_film_046 (closestValue, hard): 73 GOOD, KEEP

- RU: Сколько минут длится «Пёс Барбос и необычный кросс», где впервые появились Трус, Балбес и Бывалый?
- EN: How many minutes long is Dog Barbos and the Unusual Cross, the trio's first film?
- Answer: **10** min, range 0 to 120, tolerance 7
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 3
- Verdict: Surprising that the trio started in a ten-minute short; easy to estimate from 'short vs feature' and the tolerance rewards it.

### soviet_film_045 (closestValue, hard): 83 GOOD, KEEP

- RU: Сколько лет Людмиле Прокофьевне Калугиной, грозной начальнице из «Служебного романа»?
- EN: How old is Lyudmila Kalugina, the dreaded boss in Office Romance?
- Answer: **36** years, range 18 to 90, tolerance 7
- Scores: F 5, C 4, G 4, D 4, E 4, A 5, P 3
- Verdict: Everyone pictures 'our old woman' as much older, so 36 gets a real reaction and invites age-guessing banter; the fact only restates the answer.

## series

### series_010 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: На какой машине ездит мистер Бин?
- EN: What car does Mr. Bean drive?
- Options: **«Мини» | A Mini** / «Фольксваген Жук» | A Volkswagen Beetle / «Фиат 500» | A Fiat 500 / Газонокосилка | A lawnmower
- Scores: F 5, C 1, G 3, D 3, E 3, A 5, P 4
- Verdict: Easy recognition with a funny reveal (padlock, armchair on the roof); the lawnmower option is a joke but adds a laugh. KEEP as a deliberate easy question.

### series_012 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: Что кричит Шелдон из «Теории большого взрыва», когда кого-то разыграл?
- EN: What does Sheldon from The Big Bang Theory shout after pranking someone?
- Options: **«Базинга!» | Bazinga!** / «Д'оу!» | D'oh! / «Легендарно!» | Legendary! / «Ябба-дабба-ду!» | Yabba dabba doo!
- Scores: F 4, C 1, G 3, D 4, E 4, A 5, P 3
- Verdict: Quick recognition that people enjoy shouting; all distractors are real catchphrases. KEEP as an easy recognition question.

### series_011 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: Какой первый покемон у Эша в мультсериале «Покемон»?
- EN: What is Ash's first Pokémon in the Pokémon series?
- Options: **Пикачу | Pikachu** / Бульбазавр | Bulbasaur / Чармандер | Charmander / Сквиртл | Squirtle
- Scores: F 5, C 1, G 3, D 4, E 3, A 5, P 3
- Verdict: Near-universal recognition; the starter Pokémon are sly distractors for those who know the games, and 'Ash overslept' is a small payoff. KEEP as an easy question.

### series_032 (multipleChoice, medium): 58 NEEDS IMPROVEMENT, IMPROVE

- RU: Что обожает есть бог смерти Рюк в аниме «Тетрадь смерти»?
- EN: What does the death god Ryuk love to eat in Death Note?
- Options: **Яблоки | Apples** / Суши | Sushi / Шоколад | Chocolate / Тетради | Notebooks
- Scores: F 3, C 2, G 2, D 3, E 3, A 5, P 4. Flags: NO_DEDUCTION
- Verdict: Anime fans get it instantly, others have nothing to reason from, and 'Notebooks' is a throwaway; the withdrawal-handstands fact is genuinely funny.
- Rewrite (64 NEEDS IMPROVEMENT): RU: Что обожает есть бог смерти Рюк в аниме «Тетрадь смерти»? / EN: What does the death god Ryuk love to eat in Death Note?
  - Options: **Яблоки | Apples** / Конфеты | Sweets / Человеческие души | Human souls / Сырое мясо | Raw meat
  - Why better: 'Human souls' and 'raw meat' are what a death god 'should' eat, so non-fans can reason and get caught, and the mundane apple becomes the surprise.

### series_035 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: Что такое «лаф-бокс», без которого с 1950-х не обходились ситкомы?
- EN: What was the "laff box" that sitcoms relied on from the 1950s?
- Options: **Машина записанного смеха | A machine of recorded laughter** / Первый пульт | The first remote control / Ящик для писем зрителей | A box for viewers' letters / Будка суфлёра | A prompter's booth
- Scores: F 3, C 3, G 4, D 2, E 3, A 4, P 4. Flags: OBVIOUS_ANSWER
- Verdict: "Laff" gives away "laughter" and the distractors are unrelated; the padlocked, secret machine is the real story and should be the hook.
- Rewrite (70 GOOD): RU: Инженер Чарли Дуглас держал своё изобретение для ситкомов под замком. Что это было? / EN: Engineer Charley Douglass kept his sitcom invention padlocked. What was it?
  - Options: **Машина записанного смеха | A machine of recorded laughter** / Первый телесуфлёр | The first teleprompter / Пульт табло «Аплодисменты» | The APPLAUSE sign switch / Машина искусственного снега | A fake snow machine
  - Why better: Dropping the word "laff" removes the give-away, and the padlock mystery makes players weigh four believable studio gadgets before the organ-like machine is revealed.

### series_022 (multipleChoice, medium): 56 NEEDS IMPROVEMENT, REPLACE

- RU: Кем работает Декстер Морган в полиции Майами?
- EN: What is Dexter Morgan's job with the Miami police?
- Options: **Экспертом по следам крови | Blood spatter analyst** / Патологоанатомом | Pathologist / Детективом по убийствам | Homicide detective / Полицейским фотографом | Police photographer
- Scores: F 4, C 1, G 3, D 4, E 2, A 5, P 2. Flags: BORING_REVEAL
- Verdict: The answer is the show's premise and the fact just restates it; fans get a free point, others guess.

### series_034 (multipleChoice, medium): 79 GOOD, KEEP

- RU: Какая кинозвезда сыграла в «Друзьях» бывшего одноклассника, ненавидевшего Рэйчел?
- EN: Which film star played Rachel's old classmate who hated her, in Friends?
- Options: **Брэд Питт | Brad Pitt** / Том Круз | Tom Cruise / Джонни Депп | Johnny Depp / Леонардо Ди Каприо | Leonardo DiCaprio
- Scores: F 5, C 3, G 4, D 4, E 4, A 4, P 4
- Verdict: Star cameo in a beloved sitcom, reasoning path via his marriage to Aniston, and an Emmy nod as a bonus; good fun.

### series_023 (multipleChoice, medium): 49 REJECT, REPLACE

- RU: Кто играл молодую Елизавету II в первых двух сезонах «Короны»?
- EN: Who played the young Elizabeth II in the first two seasons of The Crown?
- Options: **Клэр Фой | Claire Foy** / Оливия Колман | Olivia Colman / Хелен Миррен | Helen Mirren / Эмма Томпсон | Emma Thompson
- Scores: F 3, C 1, G 2, D 4, E 2, A 5, P 2. Flags: NO_DEDUCTION, BORING_REVEAL
- Verdict: Actress-name recall with no reasoning path, and the reveal (they recast as the Queen aged) is common knowledge to viewers.

### series_046 (closestValue, hard): 77 GOOD, KEEP

- RU: Сколько лет Питер Фальк играл лейтенанта Коломбо?
- EN: For how many years did Peter Falk play Lieutenant Columbo?
- Answer: **35** years, range 5 to 100, tolerance 8
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 5
- Verdict: Surprisingly long run that people can estimate from Falk's ageing face, and the Bing-Crosby-chose-golf fact is a great payoff.

### series_043 (closestValue, hard): 60 NEEDS IMPROVEMENT, IMPROVE

- RU: Раз в сколько минут герои «Остаться в живых» должны были вводить код в компьютер бункера?
- EN: In Lost, how often, in minutes, did the code have to be typed into the hatch computer?
- Answer: **108** minutes, range 0 to 360, tolerance 15
- Scores: F 3, C 3, G 2, D 3, E 3, A 4, P 4. Flags: NO_DEDUCTION
- Verdict: Fans know 108, everyone else picks a random number; giving the famous numbers in the question turns it into a fun mental-maths puzzle that the reveal confirms.
- Rewrite (77 GOOD): RU: Герои «Остаться в живых» вводили в компьютер 4, 8, 15, 16, 23, 42. Раз в сколько минут? / EN: In Lost the castaways typed 4, 8, 15, 16, 23, 42 into a computer. How often, in minutes?
  - Answer 108, range 0 to 360, tolerance 15
  - Why better: Sharp players can spot that the numbers add up to the answer, so it rewards reasoning instead of fandom and the reveal gets an 'of course!'.

## ru_tv

### ru_tv_005 (multipleChoice, easy): 75 GOOD, KEEP

- RU: Что показывали по телевизору утром 19 августа 1991 года, в первый день путча?
- EN: What did Soviet TV show on the morning of 19 August 1991, the first day of the coup?
- Options: **Балет «Лебединое озеро» | The ballet Swan Lake** / «Ну, погоди!» | Nu, Pogodi! / Хоккейный матч | An ice hockey match / «Иронию судьбы» | The Irony of Fate
- Scores: F 5, C 2, G 4, D 3, E 4, A 5, P 4
- Verdict: Iconic Soviet TV moment that both generations know as a meme; the 'Balet' prefix makes the correct option the longest, cheap to fix.
- Rewrite (77 GOOD): RU: Что показывали по телевизору утром 19 августа 1991 года, в первый день путча? / EN: What did Soviet TV show on the morning of 19 August 1991, the first day of the coup?
  - Options: **«Лебединое озеро» | Swan Lake** / «Ну, погоди!» | Nu, Pogodi! / Хоккейный матч | An ice hockey match / «Иронию судьбы» | The Irony of Fate
  - Why better: Dropping 'Балет' evens the option lengths so the answer is recalled, not spotted.

### ru_tv_011 (multipleChoice, easy): 66 NEEDS IMPROVEMENT, KEEP

- RU: Как на самом деле звали Штирлица из «Семнадцати мгновений весны»?
- EN: What was Stierlitz's real name in Seventeen Moments of Spring?
- Options: **Максим Исаев | Maxim Isaev** / Глеб Жеглов | Gleb Zheglov / Павел Корчагин | Pavel Korchagin / Иван Бровкин | Ivan Brovkin
- Scores: F 5, C 1, G 3, D 4, E 3, A 5, P 4
- Verdict: Warm recognition; the other options are famous Soviet screen heroes, which is a fun touch, and the 'empty streets' fact lands. KEEP as an easy question.

### ru_tv_002 (multipleChoice, easy): 59 NEEDS IMPROVEMENT, KEEP

- RU: Что в «Что? Где? Когда?» решает, какой вопрос будет сыгран следующим?
- EN: In What? Where? When?, what decides which question is played next?
- Options: **Волчок со стрелкой | A spinning top with an arrow** / Бросок кубика | A roll of the dice / Лотерейный барабан | A lottery drum / Выбор капитана | The team captain's choice
- Scores: F 5, C 1, G 3, D 4, E 2, A 5, P 2. Flags: BORING_REVEAL
- Verdict: Everyone knows the волчок, so it is a free point, and the fact only restates the rule. KEEP only as a deliberate easy warm-up.

### ru_tv_026 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: Чей голос годами звучал за кадром «Что? Где? Когда?», пока имя ведущего держали в секрете?
- EN: Whose voice spoke off screen in What? Where? When? for years while the host's name was kept secret?
- Options: **Владимира Ворошилова | Vladimir Voroshilov** / Игоря Кириллова | Igor Kirillov / Леонида Якубовича | Leonid Yakubovich / Владислава Листьева | Vladislav Listyev
- Scores: F 4, C 3, G 3, D 3, E 3, A 3, P 4. Flags: TOO_WORDY
- Verdict: A nice 'secret host' story for the older players, but the question is long, and Yakubovich is too obviously from another show.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Имя ведущего «Что? Где? Когда?» годами не писали в титрах. Как его звали? / EN: For years the host of What? Where? When? was left out of the credits. Who was he?
  - Options: **Владимир Ворошилов | Vladimir Voroshilov** / Игорь Кириллов | Igor Kirillov / Александр Масляков | Alexander Maslyakov / Владислав Листьев | Vladislav Listyev
  - Why better: Shorter, leads with the mystery, and four genuine Soviet TV hosts make it a real recall test.

### ru_tv_037 (multipleChoice, medium): 52 REJECT, REPLACE

- RU: Кто вёл музыкальную передачу «Утренняя почта»?
- EN: Who hosted the music show Morning Mail?
- Options: **Юрий Николаев | Yuri Nikolaev** / Игорь Кириллов | Igor Kirillov / Николай Дроздов | Nikolai Drozdov / Леонид Якубович | Leonid Yakubovich
- Scores: F 4, C 1, G 2, D 4, E 2, A 5, P 2. Flags: NO_DEDUCTION, BORING_REVEAL
- Verdict: Host-name recall that the 23-year-olds cannot reason about, with a flat fact (he hosted it from 1974).

### ru_tv_027 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Кто вёл самые первые выпуски «Поля чудес» до Якубовича?
- EN: Who hosted the very first episodes of Field of Miracles, before Yakubovich?
- Options: **Владислав Листьев | Vladislav Listyev** / Александр Масляков | Alexander Maslyakov / Валдис Пельш | Valdis Pelsh / Владимир Ворошилов | Vladimir Voroshilov
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 3
- Verdict: Most people assume Yakubovich hosted from day one, so Listyev is a real surprise, and all options are credible TV hosts.

### ru_tv_034 (multipleChoice, medium): 84 GOOD, KEEP

- RU: Где в 1986 году прозвучала фраза «В СССР секса нет»?
- EN: Where was the phrase "There is no sex in the USSR" heard in 1986?
- Options: **На телемосте Ленинград — Бостон | A Leningrad to Boston TV link-up** / В игре КВН | In a KVN game / В программе «Время» | On the Vremya news / В «Кабачке „13 стульев“» | In Kabachok 13 Chairs
- Scores: F 5, C 3, G 4, D 3, E 5, A 5, P 5
- Verdict: Legendary phrase, and the reveal (the question was about ads, 'we have love' was drowned in laughter) gets a big reaction; the telemost option is a little longer than the rest.

### ru_tv_016 (multipleChoice, medium): 76 GOOD, KEEP

- RU: На что намекало название КВН, когда игра только появилась?
- EN: What did the name KVN hint at when the show first appeared?
- Options: **На телевизор «КВН-49» | The KVN-49 TV set** / На клуб в МГУ | A Moscow University club / На фамилии трёх студентов | Three students' surnames / На журнал «Крокодил» | Krokodil magazine
- Scores: F 4, C 4, G 2, D 4, E 4, A 5, P 5
- Verdict: Surprising double meaning, and 'Купил, включил, не работает' is a great line to repeat at the table.

### ru_tv_040 (closestValue, hard): 51 REJECT, REPLACE

- RU: В каком году вышел первый выпуск «Что? Где? Когда?»?
- EN: In which year did the first episode of What? Where? When? air?
- Answer: **1975** year, range 1960 to 2020, tolerance 8
- Scores: F 4, C 1, G 3, D 3, E 1, A 5, P 2. Flags: SCHOOL_EXAM
- Verdict: A founding-year question nobody cares about; the six-points rule fact is dry. The script also flags similarity to ru_tv_025 (not in this batch).

### ru_tv_039 (closestValue, hard): 55 NEEDS IMPROVEMENT, REPLACE

- RU: В каком году впервые вышел в эфир КВН?
- EN: In which year was KVN first broadcast?
- Answer: **1961** year, range 1945 to 2005, tolerance 8
- Scores: F 4, C 2, G 3, D 3, E 1, A 5, P 2. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: Another founding year: estimable but joyless, and the fact (live, 90 minutes) adds little.

## cartoons

### cartoons_008 (multipleChoice, easy): 66 NEEDS IMPROVEMENT, KEEP

- RU: За чем вечно гоняется белка Скрат в «Ледниковом периоде»?
- EN: What is Scrat the squirrel forever chasing in Ice Age?
- Options: За шишкой | A pine cone / **За жёлудем | An acorn** / За бананом | A banana / За куском сыра | A piece of cheese
- Scores: F 5, C 1, G 3, D 4, E 3, A 5, P 4
- Verdict: Easy recognition with a good pine-cone trap and a fun 'real Scrat' fossil reveal. KEEP as a deliberate easy question.

### cartoons_005 (multipleChoice, easy): 75 GOOD, KEEP

- RU: Кем работал крокодил Гена?
- EN: What was Gena the Crocodile's job?
- Options: **Крокодилом в зоопарке | Crocodile at the zoo** / Почтальоном | Postman / Сторожем на стройке | Building site watchman / Водителем трамвая | Tram driver
- Scores: F 5, C 3, G 3, D 3, E 4, A 5, P 4
- Verdict: The famous joke answer (a crocodile working as a crocodile) gets a smile, though the option echoes the question's word; Livanov-as-Gena is a good reveal.

### cartoons_006 (multipleChoice, easy): 57 NEEDS IMPROVEMENT, KEEP

- RU: Где живёт Шрек в начале первого фильма?
- EN: Where does Shrek live at the start of the first film?
- Options: В пещере | In a cave / **На болоте | In a swamp** / В башне | In a tower / Под мостом | Under a bridge
- Scores: F 5, C 0, G 3, D 3, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Pure recognition everyone gets; KEEP as an easy warm-up. The Yiddish 'Shrek = fear' fact is strong enough to deserve its own question.

### cartoons_023 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: В честь кого назвали Молнию Маккуина из «Тачек»?
- EN: Who was Lightning McQueen from Cars named after?
- Options: Актёра Стива Маккуина | Actor Steve McQueen / **Аниматора Pixar Гленна Маккуина | Pixar animator Glenn McQueen** / Режиссёра Стива Маккуина | Director Steve McQueen / Королевы Англии | The Queen of England
- Scores: F 4, C 4, G 2, D 3, E 3, A 4, P 3
- Verdict: Great trap (everyone assumes actor Steve McQueen), but 'The Queen of England' is a throwaway and two Steve McQueens confuse more than they tempt.
- Rewrite (67 NEEDS IMPROVEMENT): RU: В честь кого назвали Молнию Маккуина из «Тачек»? / EN: Who was Lightning McQueen from Cars named after?
  - Options: **Аниматора Pixar Гленна Маккуина | Pixar animator Glenn McQueen** / Актёра Стива Маккуина | Actor Steve McQueen / Модельера Александра Маккуина | Designer Alexander McQueen / Группы Queen | The band Queen
  - Why better: Every option is a real famous (Mc)Queen, so the actor trap stays strong, the duplicate Steve McQueen is gone and nothing is a pure throwaway.

### cartoons_027 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Кто первым озвучивал Микки Мауса?
- EN: Who was the first voice of Mickey Mouse?
- Options: **Сам Уолт Дисней | Walt Disney himself** / Чарли Чаплин | Charlie Chaplin / Мел Бланк | Mel Blanc / Фрэнк Синатра | Frank Sinatra
- Scores: F 5, C 3, G 3, D 3, E 3, A 5, P 4
- Verdict: Nice surprise that Disney himself voiced Mickey, and 'Mortimer' is a fun reveal; 'Сам' in the correct option is a slight tell.

### cartoons_031 (multipleChoice, medium): 56 NEEDS IMPROVEMENT, REPLACE

- RU: Кто озвучил Шрека в оригинальной версии?
- EN: Who voiced Shrek in the original version?
- Options: **Майк Майерс | Mike Myers** / Эдди Мёрфи | Eddie Murphy / Джим Керри | Jim Carrey / Робин Уильямс | Robin Williams
- Scores: F 4, C 1, G 2, D 4, E 2, A 5, P 4. Flags: NO_DEDUCTION
- Verdict: Russian players know the dubbed Shrek, so the original voice is name recall; the Chris Farley story is good but Farley is unknown here.

### cartoons_030 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Какой советский мультфильм, по словам Миядзаки, удержал его в профессии аниматора?
- EN: Which Soviet cartoon, Miyazaki says, kept him working in animation?
- Options: **«Снежная королева» | The Snow Queen** / «Ну, погоди!» | Nu, Pogodi! / «Чебурашка» | Cheburashka / «Винни-Пух» | Winnie the Pooh
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: Proud-of-our-cartoons surprise with four credible Soviet classics (Cheburashka is a real Japan favourite), and the near-quitting story pays off.

### cartoons_019 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Кто озвучил Винни-Пуха в советском мультфильме?
- EN: Who voiced Winnie the Pooh in the Soviet cartoon?
- Options: Анатолий Папанов | Anatoly Papanov / **Евгений Леонов | Yevgeny Leonov** / Юрий Никулин | Yuri Nikulin / Андрей Миронов | Andrei Mironov
- Scores: F 5, C 2, G 3, D 4, E 3, A 5, P 4
- Verdict: The voice is instantly recognisable once you think of it, all distractors are great comic actors, and the sped-up-tape fact surprises.

### cartoons_040 (closestValue, hard): 57 NEEDS IMPROVEMENT, REPLACE

- RU: В каком году вышел «Пароходик Вилли» — официальный дебют Микки Мауса?
- EN: In which year did Steamboat Willie, Mickey Mouse's official debut, come out?
- Answer: **1928** year, range 1900 to 2000, tolerance 10
- Scores: F 4, C 1, G 4, D 3, E 1, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: A release year that is estimable but nobody cares about; the 'third Mickey film' fact is mild.

### cartoons_043 (closestValue, hard): 81 GOOD, KEEP

- RU: Сколько щенков родилось у Понго и Пэдди в «101 далматинце» (1961)?
- EN: How many puppies were born to Pongo and Perdita in 101 Dalmatians (1961)?
- Answer: **15** puppies, range 5 to 60, tolerance 7
- Scores: F 5, C 4, G 3, D 4, E 4, A 5, P 4
- Verdict: Everyone assumes 101 were born, the real 15 surprises, and the reveal explains where the rest came from; fair range and tolerance.

## heroes

### heroes_011 (multipleChoice, easy): 70 GOOD, KEEP

- RU: Кто в оригинале озвучивает Грута, который говорит только «Я есть Грут»?
- EN: Who voices Groot, whose only line is "I am Groot"?
- Options: Дуэйн Джонсон | Dwayne Johnson / **Вин Дизель | Vin Diesel** / Джейсон Стэйтем | Jason Statham / Арнольд Шварценеггер | Arnold Schwarzenegger
- Scores: F 5, C 2, G 3, D 4, E 3, A 5, P 4
- Verdict: All four are action stars with deep voices, so it is a fair recognition guess, and Diesel voicing Groot in foreign dubs is a fun extra.

### heroes_009 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: Какой страной правит Чёрная Пантера?
- EN: Which country does Black Panther rule?
- Options: Латверия | Latveria / Соковия | Sokovia / **Ваканда | Wakanda** / Дженовия | Genovia
- Scores: F 5, C 1, G 3, D 4, E 3, A 5, P 3
- Verdict: Easy recognition with witty fictional-country distractors (Genovia from The Princess Diaries); the reveal is a solid but familiar 'first Black superhero' line. KEEP as an easy question.

### heroes_008 (multipleChoice, easy): 56 NEEDS IMPROVEMENT, KEEP

- RU: Как зовут дворецкого Бэтмена?
- EN: What is the name of Batman's butler?
- Options: Джарвис | Jarvis / Дживс | Jeeves / **Альфред | Alfred** / Бэрримор | Barrymore
- Scores: F 5, C 1, G 2, D 4, E 3, A 5, P 1. Flags: BORING_REVEAL
- Verdict: Fine warm-up recognition, but zero surprise and the moustache fact is no payoff. KEEP as a deliberate easy recognition question.

### heroes_019 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Кто, кроме Тора, поднимает молот Мьёльнир в «Мстителях: Финал»?
- EN: In Avengers: Endgame, who besides Thor lifts the hammer Mjolnir?
- Options: Железный человек | Iron Man / **Капитан Америка | Captain America** / Халк | The Hulk / Чёрная вдова | Black Widow
- Scores: F 4, C 2, G 3, D 4, E 4, A 5, P 4
- Verdict: A crowd-pleasing movie moment fans cheered in cinemas, credible options, and the Age of Ultron foreshadowing is a satisfying reveal.

### heroes_035 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Чем зарабатывает на жизнь Обеликс?
- EN: How does Obelix earn his living?
- Options: **Развозит менгиры | Delivering menhirs** / Куёт мечи | Forging swords / Торгует рыбой | Selling fish / Поёт песни | Singing as a bard
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Familiar from the Depardieu films, the menhir answer is satisfying, and 'Obelix from obelisk' is a nice click.

### heroes_032 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Откуда взялся псевдоним создателя Тинтина Эрже (Hergé)?
- EN: Where does the pen name of Tintin's creator, Hergé, come from?
- Options: **Его инициалы наоборот | His initials reversed** / Кличка его собаки | His dog's name / Название деревни | A Belgian village / Так он чихал | The sound of his sneeze
- Scores: F 2, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: Clever answer and a funny sneeze distractor, but Tintin is half-known in Russia; putting his real name in the question turns it into a satisfying deduction.
- Rewrite (79 GOOD): RU: Бельгиец Жорж Реми, автор комиксов о Тинтине, подписывался «Эрже». Откуда этот псевдоним? / EN: Belgian Georges Remi, creator of Tintin, signed himself Hergé. Where does the name come from?
  - Options: **Его инициалы наоборот | His initials reversed** / Кличка его собаки | His dog's name / Название деревни | A Belgian village / Так он чихал | The sound of his sneeze
  - Why better: With 'Georges Remi' on screen, players can sound out R.G. themselves, so it rewards thinking and gets an aha even from people who never read Tintin.

### heroes_016 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Какого цвета был Халк в самом первом комиксе 1962 года?
- EN: What colour was the Hulk in his very first comic in 1962?
- Options: **Серого | Grey** / Красного | Red / Синего | Blue / В горошек | Polka dot
- Scores: F 5, C 4, G 2, D 3, E 4, A 5, P 4
- Verdict: Everyone knows green Hulk, so grey is a real surprise and the printing-problem reason is a good story; 'Polka dot' is a joke option but earns a laugh.

### heroes_018 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Где впервые появилась Харли Квинн, подружка Джокера?
- EN: Where did Harley Quinn, the Joker's sidekick, first appear?
- Options: **В мультсериале | In a cartoon series** / В комиксе 1940-х | In a 1940s comic / В фильме 1989 года | In the 1989 film / В видеоигре | In a video game
- Scores: F 4, C 4, G 2, D 4, E 3, A 5, P 4
- Verdict: Surprising origin (a cartoon side character who became a film star), credible format distractors, good reveal.

### heroes_045 (closestValue, hard): 73 GOOD, KEEP

- RU: Сколько лет было Стэну Ли, когда вышла «Фантастическая четвёрка» (1961), начало эпохи Marvel?
- EN: How old was Stan Lee when the Fantastic Four, which began the Marvel age, came out in 1961?
- Answer: **38** years old, range 10 to 90, tolerance 7
- Scores: F 4, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: People can reason from his cameo-era old age, the number is mildly surprising, and the 'nothing to lose' wife advice is a lovely reveal; the tolerance is generous.

### heroes_039 (closestValue, hard): 57 NEEDS IMPROVEMENT, REPLACE

- RU: В каком году вышел первый комикс о Супермене, с которого начался «золотой век» комиксов?
- EN: In what year did Superman's first comic come out, starting the golden age of comics?
- Answer: **1938** year, range 1900 to 2010, tolerance 8
- Scores: F 4, C 1, G 4, D 3, E 1, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: Estimable but dull publication year; the teenagers-searching-five-years fact is better than the question it rides on.

## games_retro

### games_retro_011 (multipleChoice, easy): 70 GOOD, KEEP

- RU: Кто такой Донки Конг?
- EN: What is Donkey Kong?
- Options: **Горилла | A gorilla** / Осёл | A donkey / Медведь | A bear / Дракон | A dragon
- Scores: F 5, C 2, G 3, D 4, E 3, A 5, P 4
- Verdict: The name invites the 'donkey' trap, and the 'not a typo for Monkey Kong' fact is a nice myth-buster.

### games_retro_012 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: Кто смеётся над тобой в Duck Hunt, если промахнулся?
- EN: Who laughs at you in Duck Hunt when you miss?
- Options: **Собака | The dog** / Утка | A duck / Охотник | A hunter / Заяц | A hare
- Scores: F 4, C 1, G 3, D 3, E 4, A 5, P 4
- Verdict: Shared Dendy-era grudge against the laughing dog gets a groan, and the shootable-dog arcade fact is fun. KEEP as an easy nostalgia question.

### games_retro_009 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Какой реальный город изображает GTA: Vice City (2002)?
- EN: Which real city is GTA: Vice City (2002) based on?
- Options: **Майами | Miami** / Лос-Анджелес | Los Angeles / Нью-Йорк | New York / Лас-Вегас | Las Vegas
- Scores: F 4, C 1, G 4, D 4, E 3, A 5, P 3
- Verdict: 'Vice' plus Miami Vice makes it easy; KEEP as an easy recognition question with a decent 80s-radio reveal.

### games_retro_035 (multipleChoice, medium): 81 GOOD, KEEP

- RU: Игру какой компании скопировала советская «Электроника» «Ну, погоди!»?
- EN: The Soviet Elektronika handheld "Nu, Pogodi!" copied a game by which company?
- Options: **Нинтендо | Nintendo** / Касио | Casio / Атари | Atari / Сега | Sega
- Scores: F 5, C 4, G 3, D 3, E 4, A 5, P 5
- Verdict: Everyone had the Elektronika, few know it was a Nintendo copy, and 'the original wolf wore a hat too' is a perfect reveal. Nintendo is a little longer, but Atari/Sega are fair traps.

### games_retro_017 (multipleChoice, medium): 54 REJECT, REPLACE

- RU: Что давал в Doom код IDDQD?
- EN: What did typing IDDQD do in Doom?
- Options: **Бессмертие | Invincibility** / Всё оружие | All the weapons / Переход на любой уровень | A jump to any level / Хождение сквозь стены | Walking through walls
- Scores: F 3, C 1, G 2, D 4, E 3, A 5, P 3. Flags: NO_DEDUCTION
- Verdict: A nostalgia flash for Doom players and a coin toss for everyone else; the 'Degreelessness' fact is mild.

### games_retro_024 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Как световой пистолет в Duck Hunt понимал, что ты попал?
- EN: How did the light gun in Duck Hunt know you had hit?
- Options: **Экран мигал белым квадратом | The screen flashed a white box** / Невидимый луч бил в датчик | An invisible beam hit a sensor / Ловил ультразвук от телевизора | It picked up ultrasound from the TV / Подсказывала собака | The dog told it
- Scores: F 4, C 4, G 3, D 4, E 4, A 4, P 5
- Verdict: A childhood mystery solved: credible tech distractors and a reveal that explains why it fails on modern TVs.

### games_retro_036 (multipleChoice, medium): 59 NEEDS IMPROVEMENT, REPLACE

- RU: Чем сначала должна была стать Diablo (1996)?
- EN: What was Diablo (1996) first planned to be?
- Options: **Пошаговой игрой | A turn-based game** / Дополнением к Warcraft | A Warcraft add-on / Космическим шутером | A space shooter / Настольной игрой | A board game
- Scores: F 3, C 3, G 2, D 4, E 2, A 5, P 3. Flags: OBSCURE_TRIVIA
- Verdict: Niche development history only PC gamers care about; the 'working in hours' fact does not land for a family.

### games_retro_034 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: В какой стране StarCraft стал почти национальным спортом?
- EN: In which country did StarCraft become almost a national sport?
- Options: **В Южной Корее | South Korea** / В Японии | Japan / В Швеции | Sweden / В Бразилии | Brazil
- Scores: F 3, C 2, G 4, D 4, E 3, A 5, P 4
- Verdict: Korea's esports fame makes it guessable, but the surprising part (half of all copies sold there) is hidden in the fact; move it into the question.
- Rewrite (74 GOOD): RU: К 2001 году почти половину всех проданных в мире копий StarCraft купили в одной стране. В какой? / EN: By 2001 almost half of all StarCraft copies sold worldwide were bought in one country. Which?
  - Options: **В Южной Корее | South Korea** / В Японии | Japan / В Швеции | Sweden / В Бразилии | Brazil
  - Why better: The 'half the world's copies' hook makes even non-gamers go 'wait, really?' and invites reasoning about esports nations.

### games_retro_041 (closestValue, hard): 72 GOOD, KEEP

- RU: Сколько маленьких точек нужно съесть Пакмену, чтобы пройти лабиринт?
- EN: How many small dots does Pac-Man have to eat to clear the maze?
- Answer: **240** dots, range 20 to 800, tolerance 30
- Scores: F 5, C 3, G 3, D 3, E 3, A 5, P 4. Flags: REPETITIVE
- Verdict: Everyone has played Pac-Man and can estimate dots in a maze, so it is a fair hard question. Its fact mentions level 256, which spoils games_retro_040 if both are played in one evening.

### games_retro_040 (closestValue, hard): 78 GOOD, KEEP

- RU: На каком уровне Pac-Man ломается: половина экрана превращается в кашу и пройти дальше нельзя?
- EN: On which level does Pac-Man break, with half the screen turning to garbage so no one can go on?
- Answer: **256** level, range 1 to 1000, tolerance 30
- Scores: F 4, C 4, G 3, D 4, E 4, A 4, P 5. Flags: REPETITIVE
- Verdict: A broken level in Pac-Man is a fun surprise, techies can reason to 256, and the one-byte overflow is a satisfying explanation; keep apart from games_retro_041, whose fact names level 256.

## games_new

### games_new_009 (multipleChoice, easy): 62 NEEDS IMPROVEMENT, KEEP

- RU: Как в Among Us предатель тайно перемещается по кораблю?
- EN: How does the impostor secretly move around the ship in Among Us?
- Options: **Через вентиляцию | Through the vents** / Через телепорт | By teleporter / На лифте | By lift / В шкафу на колёсах | Inside a rolling wardrobe
- Scores: F 4, C 1, G 3, D 3, E 4, A 5, P 3
- Verdict: Quick 'sus' recognition that gets a reaction from players, with a 'wardrobe on wheels' joke option. KEEP as an easy question.

### games_new_013 (multipleChoice, easy): 60 NEEDS IMPROVEMENT, KEEP

- RU: Как называется игровая валюта Roblox?
- EN: What is Roblox's in-game currency called?
- Options: **Робаксы | Robux** / Вибаксы | V-Bucks / Симолеоны | Simoleons / Рубины | Rubies
- Scores: F 4, C 0, G 5, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Roblox → Robux gives the answer away; 'older than its players' is a nice fact. KEEP only as a free easy point.

### games_new_002 (multipleChoice, easy): 70 GOOD, KEEP

- RU: Игры «Ведьмак» сделаны по книгам писателя из какой страны?
- EN: The Witcher games are based on books by a writer from which country?
- Options: **Польша | Poland** / Чехия | Czech Republic / Финляндия | Finland / Венгрия | Hungary
- Scores: F 5, C 1, G 4, D 4, E 3, A 5, P 4
- Verdict: Many know Sapkowski is Polish, the neighbours are fair distractors, and the flat-fee regret is a good reveal.

### games_new_038 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: Чем знаменита игра Undertale? Её можно пройти…
- EN: What is Undertale famous for? You can finish it...
- Options: **никого не убив | without killing anyone** / не сходя с места | without moving / ни разу не прыгнув | without ever jumping / с закрытыми глазами | with your eyes shut
- Scores: F 2, C 4, G 3, D 3, E 3, A 5, P 3
- Verdict: Charming idea, but the game is unknown to the older players and 'with your eyes shut' is a throwaway; tighter distractors would make non-players reason.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Чем знаменита игра Undertale? Её можно пройти… / EN: What is Undertale famous for? You can finish it...
  - Options: **никого не убив | without killing anyone** / ни разу не погибнув | without ever dying / не подняв ни одного предмета | without picking anything up / не сказав ни слова | without saying a word
  - Why better: All four sound like real 'challenge run' claims, so non-gamers have to think and the pacifist answer feels like a discovery.

### games_new_032 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Какая страна в 2014 году воссоздала себя в Minecraft в натуральную величину?
- EN: Which country recreated itself at full scale in Minecraft in 2014?
- Options: **Дания | Denmark** / Исландия | Iceland / Япония | Japan / Монако | Monaco
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: Fun 'a whole country in Minecraft' surprise, reasoning via a small flat country, and the geography-lesson purpose is a nice reveal.

### games_new_033 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, KEEP

- RU: Как зовут главного героя игры The Legend of Zelda: Breath of the Wild?
- EN: What is the hero of The Legend of Zelda: Breath of the Wild called?
- Options: **Линк | Link** / Зельда | Zelda / Ганон | Ganon / Марио | Mario
- Scores: F 4, C 2, G 4, D 4, E 3, A 5, P 3
- Verdict: Recognition question that works as a trap for the "Zelda is the hero" myth; KEEP. The script flags similarity to games_retro_007 (not in this batch), so check the two do not both test Link vs Zelda.

### games_new_026 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Стриминговый сервис Twitch вырос из сайта, где один человек…
- EN: The streaming service Twitch grew out of a site where one man...
- Options: **транслировал всю свою жизнь | streamed his whole life** / продавал старые игры | sold old games / учил играть в шахматы | taught chess / показывал своих кошек | showed off his cats
- Scores: F 4, C 5, G 2, D 4, E 4, A 5, P 4
- Verdict: Genuine 'wait, seriously?': Twitch began with a man streaming his whole life; distractors are plausible small-site ideas.

### games_new_020 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько планет можно посетить в No Man's Sky?
- EN: How many planets can you visit in No Man's Sky?
- Options: Около 18 тысяч | About 18 thousand / Около 18 миллионов | About 18 million / Около 18 миллиардов | About 18 billion / **Около 18 квинтиллионов | About 18 quintillion**
- Scores: F 3, C 4, G 3, D 3, E 3, A 4, P 4
- Verdict: Amazing scale, but four '18 ×' options make 'pick the biggest' a pure pattern guess; an 'infinitely many' trap would make it bite.
- Rewrite (70 GOOD): RU: Сколько планет можно посетить в No Man's Sky? / EN: How many planets can you visit in No Man's Sky?
  - Options: **Около 18 квинтиллионов | About 18 quintillion** / Бесконечно много | Infinitely many / Около 18 миллионов | About 18 million / Около 255 | About 255
  - Why better: Breaking the '18 ×' pattern removes the pick-the-biggest tell, and 'infinitely many' is a tempting trap the 2-to-the-64th reveal explains.

### games_new_045 (closestValue, hard): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: ВВС США в 2010 году собрали суперкомпьютер из PlayStation 3. Сколько приставок в нём было?
- EN: In 2010 the US Air Force built a supercomputer out of PlayStation 3s. How many consoles?
- Answer: **1760** consoles, range 0 to 10000, tolerance 400
- Scores: F 4, C 4, G 2, D 3, E 3, A 4, P 3
- Verdict: Fun premise (a military supercomputer made of PlayStations), but there is little to reason from and the 0-10000 range is very wide.
- Rewrite (69 NEEDS IMPROVEMENT): RU: ВВС США собрали суперкомпьютер из PlayStation 3. Сколько приставок в нём было? / EN: The US Air Force built a supercomputer out of PlayStation 3s. How many consoles did it use?
  - Answer 1760, range 0 to 5000, tolerance 400
  - Why better: A tighter range makes the slider less arbitrary, so 'a couple of thousand' instinct guesses land in the bonus zone.

### games_new_044 (closestValue, hard): 60 NEEDS IMPROVEMENT, REPLACE

- RU: Сколько всего приставок Nintendo Wii продали за всё время?
- EN: How many Nintendo Wii consoles were sold in total?
- Answer: **102** million, range 0 to 400, tolerance 15
- Scores: F 4, C 2, G 3, D 3, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: A sales figure with a modest reveal; nothing to argue about at the table.

## board

### board_006 (multipleChoice, easy): 76 GOOD, KEEP

- RU: Кто победит в крестиках-ноликах, если оба игрока не ошибаются?
- EN: Who wins at tic-tac-toe if both players play perfectly?
- Options: **Никто: будет ничья | Nobody: it's a draw** / Тот, кто ходит первым | The first player / Тот, кто ходит вторым | The second player / Тот, кто ставит нолики | Whoever plays noughts
- Scores: F 5, C 2, G 5, D 3, E 3, A 5, P 4
- Verdict: Everyone has played it and can reason to the draw; the 1952 computer fact is a nice surprise. 'Whoever plays noughts' overlaps with 'the second player'.

### board_008 (multipleChoice, easy): 56 NEEDS IMPROVEMENT, REPLACE

- RU: Какие фигуры двигаются во время рокировки?
- EN: Which pieces move when you castle?
- Options: **Король и ладья | King and rook** / Король и ферзь | King and queen / Ладья и конь | Rook and knight / Ферзь и слон | Queen and bishop
- Scores: F 4, C 1, G 3, D 4, E 2, A 5, P 2. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: A chess-rules check with a fact that just restates the rule; no surprise for players, no fun for non-players.

### board_004 (multipleChoice, easy): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: Камни какого цвета делают первый ход в игре го?
- EN: In the game of Go, which colour moves first?
- Options: **Чёрные | Black** / Белые | White / Красные | Red / Синие | Blue
- Scores: F 3, C 3, G 3, D 3, E 2, A 5, P 3
- Verdict: Really a 50/50 between black and white (red and blue are filler); the hook is that it is the opposite of chess, which the question should say.
- Rewrite (67 NEEDS IMPROVEMENT): RU: В шахматах первыми ходят белые. А камни какого цвета ходят первыми в го? / EN: In chess White moves first. Which colour moves first in Go?
  - Options: **Чёрные | Black** / Белые | White / Красные | Red / Синие | Blue
  - Why better: The chess contrast sets up a 'surely the same?' trap, so the reversal and the komi reveal feel like a twist.

### board_037 (multipleChoice, medium): 59 NEEDS IMPROVEMENT, REPLACE

- RU: Что можно сделать в шахматах, если 50 ходов подряд никто не брал фигур и не ходил пешкой?
- EN: In chess, what may happen after 50 moves with no capture and no pawn move?
- Options: **Потребовать ничью | A player can claim a draw** / Выиграть, если фигур больше | Whoever has more pieces wins / Снять с доски ферзей | Both queens are removed / Сыграть блиц | A blitz game decides it
- Scores: F 3, C 3, G 3, D 3, E 2, A 4, P 3. Flags: SCHOOL_EXAM
- Verdict: A tournament-rules question only chess players care about; 'claim a draw' is guessable but nobody reacts.

### board_020 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, KEEP

- RU: Что значит слово «дженга» на языке суахили?
- EN: What does the Swahili word behind Jenga mean?
- Options: **Строить | To build** / Падать | To fall / Башня | Tower / Дерево | Wood
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 3
- Verdict: Familiar game, a surprising Swahili origin, and 'tower'/'to fall' are tempting traps; borderline GOOD, no cheap fix needed.

### board_019 (multipleChoice, medium): 77 GOOD, KEEP

- RU: Как изобретатель «Скрэббла» решил, сколько фишек с каждой буквой положить в набор?
- EN: How did Scrabble's inventor decide how many tiles each letter gets?
- Options: **Подсчитал буквы в газетах | He counted letters in newspapers** / Сделал всех букв поровну | He gave all letters equal numbers / Тянул жребий | He drew lots / Спросил жену | He asked his wife
- Scores: F 4, C 4, G 4, D 4, E 3, A 4, P 4
- Verdict: Clever method players can reason to, plausible alternatives, and the reveal explains why rare letters score more.

### board_016 (multipleChoice, medium): 77 GOOD, KEEP

- RU: В средневековых шахматах эта фигура ходила лишь на одну клетку по диагонали. Какая?
- EN: In medieval chess, which piece could move only one square diagonally?
- Options: **Ферзь | Queen** / Ладья | Rook / Конь | Knight / Слон | Bishop
- Scores: F 4, C 5, G 3, D 4, E 3, A 5, P 3
- Verdict: The strongest piece was once the weakest: a real 'wait, seriously?'. The fact mostly restates the question, though the Spain date adds a little.

### board_024 (multipleChoice, medium): 52 REJECT, REPLACE

- RU: На доске какого размера играют в международные шашки?
- EN: What size of board is international draughts played on?
- Options: **10×10 | 10×10** / 8×8 | 8×8 / 12×12 | 12×12 / 9×9 | 9×9
- Scores: F 3, C 2, G 3, D 3, E 1, A 5, P 2. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: Board dimensions are an exam fact with a flat reveal.

### board_043 (closestValue, hard): 51 REJECT, REPLACE

- RU: Сколько костей в классическом китайском наборе для маджонга?
- EN: How many tiles are in a classic Chinese mahjong set?
- Answer: **144** tiles, range 20 to 500, tolerance 16
- Scores: F 3, C 2, G 2, D 3, E 2, A 5, P 2. Flags: NO_DEDUCTION, BORING_REVEAL
- Verdict: Few have handled a mahjong set, so the tile count is hard to estimate, and the 1920s craze fact is unrelated to the number.

### board_047 (closestValue, hard): 66 NEEDS IMPROVEMENT, REPLACE

- RU: Сколько всего фишек в англоязычном наборе «Скрэббл»?
- EN: How many tiles are in an English-language Scrabble set?
- Answer: **100** tiles, range 20 to 300, tolerance 14
- Scores: F 4, C 2, G 4, D 4, E 2, A 5, P 3
- Verdict: Estimable and fair, but a round 100 tiles is no surprise and the reveal is a points table; it also sits next to the stronger board_019 Scrabble question.

## music_old

### music_old_012 (multipleChoice, easy): 68 NEEDS IMPROVEMENT, KEEP

- RU: Какая группа записала Smells Like Teen Spirit в 1991 году?
- EN: Which band recorded Smells Like Teen Spirit in 1991?
- Options: **Нирвана | Nirvana** / Пёрл Джем | Pearl Jam / Металлика | Metallica / Ганз-н-Роузес | Guns N' Roses
- Scores: F 5, C 1, G 3, D 4, E 3, A 5, P 5
- Verdict: Easy recognition with a brilliant reveal (Teen Spirit was a deodorant). KEEP as an easy question; the fact carries it.

### music_old_014 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Кто из битлов написал песню Imagine?
- EN: Which Beatle wrote the song Imagine?
- Options: **Джон Леннон | John Lennon** / Пол Маккартни | Paul McCartney / Джордж Харрисон | George Harrison / Ринго Старр | Ringo Starr
- Scores: F 5, C 1, G 4, D 4, E 2, A 5, P 3
- Verdict: Well-known answer (Lennon's signature song); the Yoko co-credit is a mild twist. KEEP as an easy recognition question.

### music_old_009 (multipleChoice, easy): 63 NEEDS IMPROVEMENT, KEEP

- RU: Какой жанр прославил фильм «Лихорадка субботнего вечера» с песнями Bee Gees?
- EN: Which genre did Saturday Night Fever, with its Bee Gees songs, make huge?
- Options: **Диско | Disco** / Панк | Punk / Гранж | Grunge / Регги | Reggae
- Scores: F 5, C 0, G 5, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Bee Gees + 'Fever' = disco for anyone; KEEP only as a free easy point. The best-seller-until-Thriller fact is decent.

### music_old_023 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Откуда взялось название Led Zeppelin?
- EN: Where did the name Led Zeppelin come from?
- Options: **Шутка: «рухнут, как свинцовый шар» | A joke: like a lead balloon** / Так звали деда гитариста | The guitarist's grandfather's name / Название паба, где они играли | A pub they played in / Марка их первого фургона | Their first van's make
- Scores: F 4, C 4, G 2, D 3, E 4, A 3, P 4
- Verdict: Great origin story, but the correct option is the long one that literally says 'joke'; balanced short options and a translated band name let players reason it out. The script's 'similar to' brands/rulers ids are other name-origin questions, not the same fact.
- Rewrite (76 GOOD): RU: Led Zeppelin — это «свинцовый цеппелин». Откуда взялось такое название? / EN: Led Zeppelin means a lead zeppelin. Where did the name come from?
  - Options: **Из шутки, что они провалятся | A joke that they would flop** / Из названия паба | From a pub's name / Из фамилии менеджера | From their manager's surname / Из марки их фургона | From their van's make
  - Why better: Equal-length options remove the tell, and 'a lead airship' invites players to work out the 'it will sink' joke themselves.

### music_old_036 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Как впервые вышла в свет I Will Survive Глории Гейнор?
- EN: How was Gloria Gaynor's I Will Survive first released?
- Options: **На обороте другого её сингла | As the B-side of one of her singles** / Как реклама духов | As a perfume advert / В фильме про диско | In a disco film / На «Евровидении» | At Eurovision
- Scores: F 4, C 4, G 2, D 3, E 3, A 4, P 4
- Verdict: Lovely underdog story (DJs flipped the record), but the correct option is far longer than the rest.
- Rewrite (73 GOOD): RU: Как впервые вышла песня I Will Survive Глории Гейнор? / EN: How was Gloria Gaynor's I Will Survive first released?
  - Options: **На стороне Б сингла | As a B-side** / В рекламе духов | In a perfume advert / В фильме о диско | In a disco film / На «Евровидении» | At Eurovision
  - Why better: Balanced lengths mean the answer has to be reasoned (an anthem hidden on a B-side), which makes the reveal more satisfying.

### music_old_018 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Чей клип первым показал канал MTV в 1981 году?
- EN: Whose music video was the first ever shown on MTV in 1981?
- Options: **Группы Баглз | The Buggles** / Майкла Джексона | Michael Jackson / Группы Дайр Стрейтс | Dire Straits / Группы Квин | Queen
- Scores: F 4, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Michael Jackson is a strong trap, and once players remember 'Video Killed the Radio Star' the irony clicks; good reveal.

### music_old_024 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Почему глаза Дэвида Боуи казались разного цвета?
- EN: Why did David Bowie's eyes look like two different colours?
- Options: **Удар друга в школьной драке | A friend's punch in a school fight** / Цветная контактная линза | A coloured contact lens / Ожог от сценической пиротехники | A burn from stage pyrotechnics / Глазные капли | Eye drops he used
- Scores: F 4, C 5, G 2, D 4, E 4, A 4, P 5
- Verdict: Surprising cause and a great twist (the friend who punched him designed his covers); credible distractors.

### music_old_037 (multipleChoice, medium): 79 GOOD, KEEP

- RU: О каком реальном событии песня Deep Purple Smoke on the Water?
- EN: Which real event is Deep Purple's Smoke on the Water about?
- Options: **Пожар в казино на концерте | A casino fire during a concert** / Пожар на нефтяном танкере | A burning oil tanker / Костры на фестивале «Вудсток» | Bonfires at Woodstock / Извержение вулкана | A volcanic eruption
- Scores: F 5, C 4, G 3, D 4, E 4, A 4, P 4
- Verdict: Everyone knows the riff, few know the story; the Montreux casino fire and Frank Zappa make a vivid reveal.

### music_old_044 (closestValue, hard): 76 GOOD, KEEP

- RU: Сколько лет было Стиви Уандеру, когда его песня впервые стала №1 в США?
- EN: How old was Stevie Wonder when he first had a US No. 1 hit?
- Answer: **13** years, range 5 to 40, tolerance 7
- Scores: F 5, C 4, G 3, D 3, E 3, A 5, P 4
- Verdict: Surprisingly young; 'Little Stevie' gives a reasoning path. A tolerance of 7 on a 5-40 range is generous (6 to 20 all score), so consider 4.

### music_old_042 (closestValue, hard): 63 NEEDS IMPROVEMENT, REPLACE

- RU: В каком году вышел альбом Майкла Джексона Thriller?
- EN: In what year was Michael Jackson's album Thriller released?
- Answer: **1982** year, range 1965 to 2025, tolerance 7
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: Estimable release year with no surprise; the 'seven top-ten singles' fact is fine but does not rescue a date question.

## music_new

### music_new_003 (multipleChoice, easy): 63 NEEDS IMPROVEMENT, KEEP

- RU: Какой французский дуэт прославился тем, что выступал в шлемах роботов?
- EN: Which French duo became famous for performing in robot helmets?
- Options: **Дафт Панк | Daft Punk** / Джастис | Justice / Эйр | Air / Моджо | Modjo
- Scores: F 4, C 1, G 3, D 4, E 3, A 5, P 4
- Verdict: Easy recognition with real French duos as distractors, and the name-from-a-bad-review fact is a nice payoff. KEEP as an easy question.

### music_new_007 (multipleChoice, easy): 73 GOOD, KEEP

- RU: Хит BTS «Dynamite» (2020) — первая песня группы, целиком спетая на каком языке?
- EN: BTS's hit 'Dynamite' (2020) was the band's first song sung entirely in which language?
- Options: **На английском | English** / На японском | Japanese / На испанском | Spanish / На китайском | Chinese
- Scores: F 4, C 3, G 4, D 4, E 3, A 5, P 3
- Verdict: Players can reason that a global hit needed English, and Japanese is a credible trap; solid easy question.

### music_new_004 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: Какая группа выиграла «Евровидение-2006» в гриме и костюмах монстров?
- EN: Which band won Eurovision 2006 dressed and made up as monsters?
- Options: **Лорди | Lordi** / Монескин | Måneskin / Рамштайн | Rammstein / Слипнот | Slipknot
- Scores: F 4, C 1, G 3, D 3, E 4, A 5, P 4
- Verdict: Memorable Eurovision moment; Rammstein and Slipknot never entered, so only Måneskin really competes, but the 45-year Finnish drought is a good reveal. KEEP as an easy question.

### music_new_019 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Какое словечко для одержимого фаната пошло от песни Эминема 2000 года?
- EN: Which word for an obsessive fan comes from a 2000 Eminem song?
- Options: **Стэн | Stan** / Хейтер | Hater / Группи | Groupie / Фрик | Freak
- Scores: F 4, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Players who know the song deduce it, the others enjoy learning a slang word came from a track; credible slang distractors.

### music_new_018 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Какая песня первой из рэп-композиций получила «Оскар»?
- EN: Which was the first rap song to win an Oscar?
- Options: **Lose Yourself — Эминем | Lose Yourself, Eminem** / In da Club — 50 Cent | In da Club, 50 Cent / Empire State of Mind — Джей-Зи | Empire State of Mind, Jay-Z / Gold Digger — Канье Уэст | Gold Digger, Kanye West
- Scores: F 4, C 2, G 4, D 4, E 3, A 4, P 5
- Verdict: 8 Mile makes it guessable, and 'he slept through his own Oscar' is a great reveal.

### music_new_033 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Какую премию в 2018 году первым из рэперов получил Кендрик Ламар?
- EN: Which prize did Kendrick Lamar win in 2018, the first rapper ever to do so?
- Options: **Пулитцеровскую | The Pulitzer Prize** / Нобелевскую | The Nobel Prize / Букеровскую | The Booker Prize / Притцкеровскую | The Pritzker Prize
- Scores: F 3, C 4, G 3, D 4, E 3, A 5, P 3
- Verdict: A rapper winning the Pulitzer is surprising, and the Nobel (Bob Dylan) is a clever trap; Kendrick is less known to the older players.

### music_new_020 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Где впервые прозвучала песня Фаррелла Уильямса «Happy»?
- EN: Where was Pharrell Williams's song 'Happy' first heard?
- Options: **В мультфильме «Гадкий я 2» | In the film Despicable Me 2** / В рекламе Coca-Cola | In a Coca-Cola advert / На открытии Олимпиады в Сочи | At the Sochi Olympics opening / В сериале «Теория большого взрыва» | In The Big Bang Theory
- Scores: F 5, C 3, G 3, D 4, E 4, A 4, P 4
- Verdict: Many forget it began in a cartoon, credible launch-venue distractors, and the 24-hour video is a fun extra.

### music_new_027 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: Кем прославился рэпер Дрейк, прежде чем начал читать рэп?
- EN: What was the rapper Drake famous as before his music career?
- Options: **Актёр молодёжного сериала | A teen TV series actor** / Хоккеист | An ice hockey player / Ведущий кулинарного шоу | A cookery show host / Профессиональный танцор | A professional dancer
- Scores: F 3, C 4, G 2, D 4, E 3, A 5, P 3
- Verdict: Surprising past for a rap star, but with nothing to reason from; mentioning Canada adds a hockey trap and a hint.
- Rewrite (70 GOOD): RU: Рэпер Дрейк начинал не с музыки. Кем он сначала прославился у себя в Канаде? / EN: The rapper Drake did not start in music. What first made him famous in Canada?
  - Options: **Актёр молодёжного сериала | A teen TV series actor** / Хоккеист | An ice hockey player / Ведущий кулинарного шоу | A cookery show host / Профессиональный танцор | A professional dancer
  - Why better: 'Canada' makes hockey a tempting trap, so the table debates before the Degrassi reveal.

### music_new_050 (closestValue, hard): 78 GOOD, KEEP

- RU: Сколько концертов в лондонской арене O2 собирался дать Майкл Джексон в 2009 году?
- EN: How many shows at London's O2 arena did Michael Jackson plan to play in 2009?
- Answer: **50** shows, range 0 to 200, tolerance 15
- Scores: F 5, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: Famous, poignant event; 50 shows surprises people who remember 'a comeback tour', and the 10→50 fact makes a satisfying reveal.

### music_new_043 (closestValue, hard): 54 REJECT, REPLACE

- RU: Сколько баллов набрал Александр Рыбак, победив на «Евровидении-2009»?
- EN: How many points did Alexander Rybak score when he won Eurovision 2009?
- Answer: **387** points, range 0 to 1000, tolerance 60
- Scores: F 4, C 2, G 2, D 2, E 2, A 5, P 3. Flags: NO_DEDUCTION
- Verdict: Rybak is liked, but nobody has a sense of Eurovision point scales, so 387 cannot be estimated; the 'record' reveal is mild.

## ru_music

### ru_music_004 (multipleChoice, easy): 75 GOOD, KEEP

- RU: Какой российский дуэт в 2003 году первым из наших артистов возглавил британский хит-парад?
- EN: Which Russian duo became the first Russian act to top the UK singles chart, in 2003?
- Options: «Блестящие» | Blestyashchie / «Серебро» | Serebro / «ВИА Гра» | VIA Gra / **«Тату» | t.A.T.u.**
- Scores: F 5, C 2, G 4, D 4, E 4, A 5, P 3
- Verdict: Proud national moment both generations remember, with four credible 2000s girl groups as options.

### ru_music_001 (multipleChoice, easy): 66 NEEDS IMPROVEMENT, KEEP

- RU: Кто написал музыку к «Миллиону алых роз»?
- EN: Who composed the music for "A Million Scarlet Roses"?
- Options: **Раймонд Паулс | Raimonds Pauls** / Давид Тухманов | David Tukhmanov / Александр Зацепин | Alexander Zatsepin / Игорь Николаев | Igor Nikolaev
- Scores: F 5, C 1, G 3, D 4, E 3, A 5, P 4
- Verdict: Easy recognition for most, with strong composer distractors, and the original Latvian lullaby story is a lovely reveal. KEEP as an easy question.

### ru_music_006 (multipleChoice, easy): 67 NEEDS IMPROVEMENT, KEEP

- RU: Из какого фильма песня «Прекрасное далёко»?
- EN: Which film is the song "Beautiful Faraway" from?
- Options: «Мэри Поппинс, до свидания» | Mary Poppins, Goodbye / **«Гостья из будущего» | Guest from the Future** / «Приключения Электроника» | Adventures of Electronic / «Чародеи» | The Magicians
- Scores: F 5, C 1, G 3, D 4, E 4, A 5, P 3
- Verdict: A sing-along recognition question; Adventures of Electronic is a clever trap (same authors). KEEP as an easy question.

### ru_music_019 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: Мелодию «Города золотого» выдали за старинную итальянскую. Кто её, как считается, сочинил?
- EN: The tune of "The Golden City" was passed off as old Italian music. Who is believed to have written it?
- Options: Микаэл Таривердиев | Mikael Tariverdiev / Андрей Петров | Andrei Petrov / **Владимир Вавилов | Vladimir Vavilov** / Борис Гребенщиков | Boris Grebenshchikov
- Scores: F 3, C 5, G 1, D 3, E 3, A 4, P 5. Flags: NO_DEDUCTION
- Verdict: A great hoax story, but the answer is a name nobody knows; adding the fake 'Francesco da Milano' as a trap makes the hoax part of the game.
- Rewrite (72 GOOD): RU: Мелодию «Города золотого» выдали за старинную итальянскую. Кто, как считается, её автор? / EN: The tune of The Golden City was passed off as old Italian music. Who is believed to have written it?
  - Options: **Владимир Вавилов | Vladimir Vavilov** / Франческо да Милано | Francesco da Milano / Борис Гребенщиков | Boris Grebenshchikov / Микаэл Таривердиев | Mikael Tariverdiev
  - Why better: The fake Italian name the record used becomes a trap, so players live through the hoax and the reveal explains how they were fooled.

### ru_music_029 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: Как поначалу называлась песня «Подмосковные вечера»?
- EN: What was the song "Moscow Nights" first called?
- Options: **«Ленинградские вечера» | Leningrad Evenings** / «Московские ночи» | Moscow Nights / «Волжские вечера» | Volga Evenings / «Летние вечера» | Summer Evenings
- Scores: F 4, C 4, G 2, D 2, E 3, A 4, P 4
- Verdict: Good story, but one distractor is the song's own title (absurd as an answer to 'first called') and the 'X Evenings' pattern makes it a pure elimination guess.
- Rewrite (75 GOOD): RU: Песня «Подмосковные вечера» сначала была о другом городе. О каком? / EN: The song Moscow Nights was first about a different city. Which one?
  - Options: **О Ленинграде | Leningrad** / О Киеве | Kyiv / О Сталинграде | Stalingrad / Об Одессе | Odesa
  - Why better: Four big Soviet cities are equally singable, so it is a real guess, and the reveal about the sports film and the jazz hit still lands.

### ru_music_038 (multipleChoice, medium): 70 GOOD, KEEP

- RU: На какой гитаре в основном играл Высоцкий?
- EN: What kind of guitar did Vysotsky mostly play?
- Options: На электрогитаре | Electric guitar / **На семиструнной | Seven-string** / На шестиструнной | Six-string / На двенадцатиструнной | Twelve-string
- Scores: F 5, C 3, G 3, D 3, E 3, A 5, P 3
- Verdict: Many assume a normal six-string, so the 'Russian' seven-string surprises; the electric-guitar option is filler.

### ru_music_035 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, REPLACE

- RU: Какой ВИА пел «Вологду» и «Косил Ясь конюшину»?
- EN: Which Soviet band sang "Vologda" and "Yas Was Mowing Clover"?
- Options: «Самоцветы» | Samotsvety / «Ариэль» | Ariel / **«Песняры» | Pesniary** / «Сябры» | Syabry
- Scores: F 4, C 1, G 3, D 4, E 3, A 5, P 3. Flags: NO_DEDUCTION
- Verdict: A medium-slot band-name recall: easy for the older players, opaque for the younger ones, with a modest US-tour reveal; Syabry is a clever trap but cannot rescue it.

### ru_music_017 (multipleChoice, medium): 74 GOOD, KEEP

- RU: «Арлекино», прославившая Пугачёву, — переделка песни из какой страны?
- EN: "Arlekino", the song that made Pugacheva famous, was a cover of a song from which country?
- Options: **Болгария | Bulgaria** / Италия | Italy / Франция | France / Польша | Poland
- Scores: F 5, C 4, G 2, D 4, E 3, A 5, P 4
- Verdict: Most would guess Italy (Harlequin, commedia dell'arte), so Bulgaria surprises and the Golden Orpheus story completes it.

### ru_music_041 (closestValue, hard): 57 NEEDS IMPROVEMENT, REPLACE

- RU: В каком году Андрей Макаревич со школьными друзьями основал «Машину времени»?
- EN: In what year did Andrei Makarevich and school friends found Mashina Vremeni?
- Answer: **1969** year, range 1955 to 2010, tolerance 7
- Scores: F 4, C 2, G 3, D 3, E 1, A 5, P 3. Flags: SCHOOL_EXAM, REPETITIVE
- Verdict: A band founding year: estimable for older players, joyless for all; it uses the same formula as ru_music_048 in the same topic.

### ru_music_048 (closestValue, hard): 55 NEEDS IMPROVEMENT, REPLACE

- RU: В каком году Борис Гребенщиков основал «Аквариум»?
- EN: In what year did Boris Grebenshchikov found Aquarium?
- Answer: **1972** year, range 1955 to 2020, tolerance 7
- Scores: F 4, C 2, G 3, D 3, E 1, A 5, P 2. Flags: SCHOOL_EXAM, REPETITIVE
- Verdict: Same 'founding year of a rock band' formula as ru_music_041, with a flatter reveal.

## classical

### classical_010 (multipleChoice, easy): 63 NEEDS IMPROVEMENT, KEEP

- RU: Кого называли «королём вальса»?
- EN: Who was known as the Waltz King?
- Options: Шопена | Chopin / Брамса | Brahms / **Иоганна Штрауса-сына | Johann Strauss II** / Моцарта | Mozart
- Scores: F 5, C 1, G 4, D 2, E 2, A 5, P 4
- Verdict: Easy recognition, but the correct option is the only full name with 'сына', a strong tell; the 'Richard Strauss is no relation' fact is good. KEEP with balanced options.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Кого называли «королём вальса»? / EN: Who was known as the Waltz King?
  - Options: **Иоганна Штрауса-сына | Johann Strauss II** / Фридерика Шопена | Frédéric Chopin / Иоганнеса Брамса | Johannes Brahms / Вольфганга Моцарта | Wolfgang Mozart
  - Why better: Full names for every composer remove the length tell, so players recall the answer instead of spotting it.

### classical_013 (multipleChoice, easy): 61 NEEDS IMPROVEMENT, KEEP

- RU: С кем сражается Щелкунчик в балете Чайковского?
- EN: Whom does the Nutcracker fight in Tchaikovsky's ballet?
- Options: Со Снежной королевой | The Snow Queen / С Кощеем | Koschei / С драконом | A dragon / **С Мышиным королём | The Mouse King**
- Scores: F 5, C 1, G 3, D 3, E 2, A 5, P 4
- Verdict: Easy recognition from the New Year ballet; the secret-celesta fact is a good payoff. KEEP as an easy question.

### classical_014 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Каким инструментом Бах прославился как исполнитель?
- EN: Which instrument made Bach famous as a performer?
- Options: Трубой | The trumpet / Флейтой | The flute / **Органом | The organ** / Арфой | The harp
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 4
- Verdict: Most know Bach's organ (Toccata), so it is easy; the 450 km walk is a memorable reveal. KEEP as an easy question.

### classical_025 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, REPLACE

- RU: Что вдохновило Мусоргского на «Картинки с выставки»?
- EN: What inspired Mussorgsky's Pictures at an Exhibition?
- Options: Коллекция Эрмитажа | The Hermitage collection / Парижский салон | A Paris salon / Картины Айвазовского | Aivazovsky's seascapes / **Выставка умершего друга | A late friend's art show**
- Scores: F 3, C 3, G 3, D 3, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: Feels like a music-school test; the 'late friend' option stands out as the only personal one, and Hartmann/Ravel is a dry reveal.

### classical_022 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Бетховен посвятил Третью симфонию, а потом в гневе снял посвящение. Кому?
- EN: Beethoven dedicated his Third Symphony to someone, then angrily withdrew it. Who?
- Options: Австрийскому императору | The Austrian emperor / Гёте | Goethe / **Наполеону | Napoleon** / Моцарту | Mozart
- Scores: F 4, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: 'In anger' plus 1804 lets players reason to Napoleon, and tearing the title page is a vivid reveal; Mozart is a weak option.

### classical_021 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Какое сочинение 15 минут повторяет одни и те же две мелодии, только всё громче?
- EN: Which piece repeats the same two tunes for about 15 minutes, only getting louder?
- Options: «Лунная соната» | The Moonlight Sonata / «Аве Мария» Шуберта | Schubert's Ave Maria / «Весна» Вивальди | Vivaldi's Spring / **«Болеро» Равеля | Ravel's Boléro**
- Scores: F 4, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Players can hum it to confirm, distractors are all famous pieces, and Ravel calling it 'orchestral tissue without music' is a fun reveal.

### classical_024 (multipleChoice, medium): 53 REJECT, REPLACE

- RU: Кто из этих композиторов НЕ входил в «Могучую кучку»?
- EN: Which of these composers was NOT in the Mighty Handful?
- Options: **Чайковский | Tchaikovsky** / Мусоргский | Mussorgsky / Бородин | Borodin / Римский-Корсаков | Rimsky-Korsakov
- Scores: F 4, C 1, G 4, D 3, E 1, A 4, P 2. Flags: SCHOOL_EXAM
- Verdict: A music-history exam question with NOT wording; the Borodin-was-a-chemist fact deserves its own question instead.

### classical_017 (multipleChoice, medium): 87 EXCELLENT, KEEP

- RU: Шопен умер в Париже. Где хранится его сердце?
- EN: Chopin died in Paris. Where is his heart kept?
- Options: В Лувре | In the Louvre / В Вене | In Vienna / В его могиле в Париже | In his Paris grave / **В костёле в Варшаве | In a Warsaw church**
- Scores: F 4, C 5, G 3, D 4, E 5, A 5, P 5
- Verdict: Gruesome-romantic surprise, all options plausible, and the jar of cognac smuggled by his sister makes everyone retell it.

### classical_045 (closestValue, hard): 58 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько раз подряд принято играть пьесу Эрика Сати «Раздражения» (Vexations)?
- EN: How many times in a row is Erik Satie's piece Vexations traditionally played?
- Answer: **840** times, range 0 to 3000, tolerance 200
- Scores: F 1, C 5, G 1, D 2, E 3, A 4, P 5. Flags: OBSCURE_TRIVIA, NO_DEDUCTION
- Verdict: Wonderful absurd fact, but the piece is unknown and the number cannot be estimated; giving the 18-hour marathon in the question gives players something to reason from.
- Rewrite (72 GOOD): RU: Пьесу Сати «Раздражения» в 1963 году пианисты играли по очереди 18 часов. Сколько раз подряд? / EN: In 1963 a relay of pianists spent 18 hours playing Satie's Vexations. How many times in a row?
  - Answer 840, range 0 to 3000, tolerance 200
  - Why better: The 18-hour marathon is both the hook and a clue (a short piece, over and over), so guesses become estimates and the number lands.

### classical_046 (closestValue, hard): 79 GOOD, KEEP

- RU: Сколько секунд длится «тихая» пьеса Джона Кейджа, где музыкант ничего не играет?
- EN: How many seconds long is John Cage's silent piece, where the performer plays nothing?
- Answer: **273** seconds, range 0 to 1000, tolerance 60
- Scores: F 3, C 4, G 4, D 4, E 4, A 5, P 4
- Verdict: Funny premise (a piece of silence); those who know '4′33″' can compute it, others estimate, and the reveal lands.

## art

### art_006 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Что «растекается» на картине Сальвадора Дали «Постоянство памяти»?
- EN: What is melting in Salvador Dali's The Persistence of Memory?
- Options: **Часы | Watches** / Свечи | Candles / Мороженое | Ice cream / Зеркала | Mirrors
- Scores: F 5, C 1, G 4, D 4, E 3, A 5, P 5
- Verdict: Famous image, easy answer, but the melting camembert reveal makes it worth asking.

### art_012 (multipleChoice, easy): 55 NEEDS IMPROVEMENT, KEEP

- RU: Чего не хватает у статуи Венеры Милосской?
- EN: What is the Venus de Milo missing?
- Options: **Рук | Her arms** / Головы | Her head / Ног | Her legs / Крыльев | Her wings
- Scores: F 5, C 0, G 3, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Everyone knows she is armless; 'head' and 'wings' are fair nods to Nike of Samothrace. KEEP only as a deliberate easy warm-up.

### art_008 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Что делают люди на картине Репина «Бурлаки на Волге»?
- EN: What are the men doing in Repin's Barge Haulers on the Volga?
- Options: **Тянут баржу | Pulling a barge** / Ловят рыбу | Fishing / Строят мост | Building a bridge / Косят траву | Mowing grass
- Scores: F 5, C 0, G 5, D 3, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: The painting's title ('бурлаки') answers the question; the defrocked-priest fact is good. KEEP only as a free easy point.

### art_033 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Как Леонардо да Винчи писал в своих тетрадях?
- EN: How did Leonardo da Vinci write in his notebooks?
- Options: **Зеркально, справа налево | In mirror script, right to left** / Цифровым шифром | In a number code / Только по-латыни | Only in Latin / Только рисунками | Only in pictures
- Scores: F 5, C 3, G 3, D 4, E 3, A 5, P 3
- Verdict: Many half-remember mirror writing, the code and Latin options are credible, and left-handedness explains it.

### art_036 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Что написано под трубкой на картине Рене Магритта?
- EN: What is written under the pipe in Rene Magritte's painting?
- Options: **«Это не трубка» | This is not a pipe** / «Это трубка» | This is a pipe / «Это не картина» | This is not a painting / «Трубка мира» | Peace pipe
- Scores: F 4, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: A famous paradox that players can reason to via surrealism, and Magritte's 'try stuffing it with tobacco' is a witty reveal.

### art_019 (multipleChoice, medium): 57 NEEDS IMPROVEMENT, REPLACE

- RU: Кто написал «Девочку с персиками»?
- EN: Who painted Girl with Peaches?
- Options: **Валентин Серов | Valentin Serov** / Илья Репин | Ilya Repin / Карл Брюллов | Karl Bryullov / Василий Суриков | Vasily Surikov
- Scores: F 5, C 1, G 2, D 4, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: A textbook painter-name match; those who know, know, others guess, and the age facts are mild.

### art_023 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Почему картину Рембрандта называют «Ночной дозор»?
- EN: Why is Rembrandt's painting called The Night Watch?
- Options: **Её затемнил лак, а сцена дневная | Varnish darkened a daytime scene** / Рембрандт писал её ночами | Rembrandt painted it at night / Её прятали от воров ночью | It was hidden at night from thieves / Так звали отряд стражи | That was the guard unit's name
- Scores: F 4, C 5, G 3, D 3, E 4, A 4, P 4
- Verdict: Real 'wait, seriously?': the Night Watch is a daytime scene; the correct option is the longest and gives the twist away, cheap to fix.
- Rewrite (82 GOOD): RU: Почему картину Рембрандта называют «Ночной дозор»? / EN: Why is Rembrandt's painting called The Night Watch?
  - Options: **Её затемнил лак | Varnish darkened it** / Её писали ночами | It was painted at night / Её прятали ночью | It was hidden at night / Так звался отряд | The unit had that name
  - Why better: Equal-length options hide the twist until the reveal says the scene is actually in daylight.

### art_017 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Из чего Микеланджело высек своего «Давида»?
- EN: What did Michelangelo carve his David from?
- Options: **Из брошенной глыбы мрамора | An abandoned block of marble** / Из гранитной скалы | A granite cliff / Из ствола дуба | An oak trunk / Из десятка мраморных частей | A dozen marble pieces
- Scores: F 4, C 4, G 2, D 3, E 3, A 5, P 4
- Verdict: Lovely 'reject block becomes masterpiece' story, but 'abandoned' is the only option with a twist, while the others are just other materials.
- Rewrite (71 GOOD): RU: Из какого мрамора Микеланджело высек своего «Давида»? / EN: What marble did Michelangelo carve his David from?
  - Options: **Из глыбы, испорченной другими | A block others had spoiled** / Из колонны античного храма | A column from an ancient temple / Из глыбы, подаренной папой | A block the Pope gave him / Из обломков старой статуи | Pieces of an old statue
  - Why better: Every option now tells a backstory, so the 'spoiled reject' is one credible story among four and the 25-years-in-the-yard reveal surprises.

### art_039 (closestValue, hard): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько картин с кувшинками написал Клод Моне в своём саду в Живерни?
- EN: How many water lily paintings did Claude Monet make in his garden at Giverny?
- Answer: **250** paintings, range 0 to 1000, tolerance 100
- Scores: F 4, C 3, G 3, D 3, E 2, A 5, P 3
- Verdict: Famous series, but there is little to anchor a guess; moving the '30 years' from the fact into the question gives an estimation path.
- Rewrite (70 GOOD): RU: Моне больше 30 лет писал пруд в своём саду в Живерни. Сколько картин с кувшинками вышло? / EN: Monet painted his pond at Giverny for over 30 years. How many water lily paintings came of it?
  - Answer 250, range 0 to 1000, tolerance 100
  - Why better: '30 years' lets players reason 'a few a year' and land near 250, turning a blind number into an estimate.

### art_048 (closestValue, hard): 77 GOOD, KEEP

- RU: Сколько картин написал Иван Айвазовский за свою жизнь?
- EN: How many paintings did Ivan Aivazovsky make in his lifetime?
- Answer: **6000** paintings, range 0 to 20000, tolerance 2000
- Scores: F 5, C 5, G 2, D 3, E 4, A 5, P 3
- Verdict: A staggering number everyone underestimates, and the generous tolerance rewards bold guesses; the fact is a mild coda.

## landmarks

### landmarks_008 (multipleChoice, easy): 80 GOOD, KEEP

- RU: Из чего сделаны красные грани звёзд на башнях Московского Кремля?
- EN: What are the red faces of the Moscow Kremlin's tower stars made of?
- Options: **Из рубинового стекла | Ruby-red glass** / Из крашеной меди | Painted copper / Из красного гранита | Red granite / Из пластика | Plastic
- Scores: F 5, C 3, G 4, D 3, E 5, A 3, P 5
- Verdict: Everyone knows the Kremlin stars, and glass vs copper vs granite gives a fair guess; 'Plastic' is a throwaway and the eagle fact is only a mild payoff.

### landmarks_014 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Какой замок вдохновил замок Спящей красавицы в Диснейленде?
- EN: Which castle inspired Sleeping Beauty Castle at Disneyland?
- Options: **Нойшванштайн | Neuschwanstein** / Виндзорский замок | Windsor Castle / Эдинбургский замок | Edinburgh Castle / Замок Бран | Bran Castle
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 4
- Verdict: Satisfying recognition: everyone knows the Disney logo and photos of Neuschwanstein, Bran (Dracula) is a good trap, and 172 days in his own castle is a nice payoff. Note rulers_015 reuses the 'opened to tourists weeks after his death' fact.

### landmarks_003 (multipleChoice, easy): 69 NEEDS IMPROVEMENT, KEEP

- RU: Чьи лица высечены в скале горы Рашмор?
- EN: Whose faces are carved into Mount Rushmore?
- Options: **Четырёх президентов США | Four US presidents** / Четырёх индейских вождей | Four Native American chiefs / Четырёх первых астронавтов | Four early astronauts / Четырёх английских королей | Four English kings
- Scores: F 5, C 2, G 5, D 1, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Easy recognition question; astronauts and English kings are absurd for a US mountain, so it is a free point, but the 'meant to be carved to the waist' fact pays off. Fine as a deliberate warm-up; swapping the absurd options for US generals and inventors would add a moment of thought.

### landmarks_024 (multipleChoice, medium): 86 EXCELLENT, KEEP

- RU: Писатель Мопассан, ненавидевший Эйфелеву башню, часто обедал в её ресторане. Почему?
- EN: Maupassant hated the Eiffel Tower yet often ate in its restaurant. Why?
- Options: **Только оттуда её не видно | It was the one spot he couldn't see it** / Эйфель кормил его бесплатно | Eiffel fed him for free / Там работал его брат-повар | His brother was the chef / Это был самый дешёвый обед | It was the cheapest lunch in Paris
- Scores: F 5, C 4, G 4, D 4, E 5, A 4, P 4
- Verdict: A great 'why' question: the punchline (the only spot he could not see it) is guessable by wit, every distractor is a credible joke, and the room laughs at the reveal.

### landmarks_022 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Из какой страны был архитектор Сиднейского оперного театра?
- EN: Where was the architect of the Sydney Opera House from?
- Options: **Из Дании | Denmark** / Из Австралии | Australia / Из Бразилии | Brazil / Из Японии | Japan
- Scores: F 4, C 4, G 2, D 4, E 2, A 5, P 4
- Verdict: The Australia trap works and Denmark surprises, but the question itself is a dry nationality lookup; the better story (Utzon never saw it finished) is hidden in the fact.
- Rewrite (71 GOOD): RU: Архитектор Сиднейской оперы так и не увидел её готовой. Откуда он был родом? / EN: The Sydney Opera House's architect never saw it finished. Where was he from?
  - Options: **Из Дании | Denmark** / Из Австралии | Australia / Из Бразилии | Brazil / Из Японии | Japan
  - Why better: The hook puts the drama up front, so the answer lands as part of a story rather than a passport check.

### landmarks_026 (multipleChoice, medium): 84 GOOD, KEEP

- RU: Шпиль Нотр-Дама, рухнувший при пожаре 2019 года, был построен в каком веке?
- EN: The Notre-Dame spire that fell in the 2019 fire dated from which century?
- Options: **В XIX веке | The 19th** / В XII веке | The 12th / В XIII веке | The 13th / В XVI веке | The 16th
- Scores: F 4, C 5, G 3, D 4, E 4, A 5, P 5
- Verdict: Most assume the spire was medieval, so '19th century' is a real 'wait, seriously?', and the architect putting his own face on an apostle is a great payoff.

### landmarks_029 (multipleChoice, medium): 77 GOOD, KEEP

- RU: Что построили в котловане недостроенного Дворца Советов в Москве?
- EN: What was built in the foundation pit of Moscow's never-finished Palace of Soviets?
- Options: **Бассейн «Москва» | The Moskva swimming pool** / Гостиницу «Россия» | Hotel Rossiya / Парк Зарядье | Zaryadye Park / Стадион «Лужники» | Luzhniki Stadium
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 5
- Verdict: The Palace of Soviets pit becoming a pool is a story the older players will tell around the table, and the cathedral coming full circle is a strong payoff; Rossiya and Zaryadye are credible traps.

### landmarks_027 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: Что Наполеон увёз в Париж с Бранденбургских ворот в 1806 году?
- EN: What did Napoleon take from Berlin's Brandenburg Gate to Paris in 1806?
- Options: **Колесницу с богиней на крыше | The chariot statue on top** / Сами створки ворот | The gates themselves / Часы с фасада | The clock on the front / Колокола | The bells
- Scores: F 3, C 3, G 3, D 2, E 2, A 5, P 4
- Verdict: The stolen quadriga is a decent fact, but the correct option is the only long descriptive one (telegraphed) and 'Bells' is weak filler.
- Rewrite (65 NEEDS IMPROVEMENT): RU: Какой трофей Наполеон увёз в Париж с Бранденбургских ворот в 1806 году? / EN: What trophy did Napoleon take to Paris from Berlin's Brandenburg Gate in 1806?
  - Options: **Колесницу с богиней | The goddess's chariot** / Бронзовых львов | The bronze lions / Створки ворот | The gate doors / Часы с фасада | The front clock
  - Why better: Balanced, equally concrete options stop the answer from being spotted by its length.

### landmarks_041 (closestValue, hard): 70 GOOD, KEEP

- RU: Сколько лет прошло от закладки до завершения Кёльнского собора?
- EN: How many years passed from start to finish of Cologne Cathedral?
- Answer: **632** years, range 0 to 1000, tolerance 80
- Scores: F 3, C 4, G 3, D 3, E 3, A 5, P 4
- Verdict: Everyone knows cathedrals took ages, so there is a reasoning path, and 632 years plus a crane left up for 400 years is a fun reveal; the range and tolerance are fair.

### landmarks_050 (closestValue, hard): 85 EXCELLENT, KEEP

- RU: Сколько лет пирамида Хеопса оставалась самым высоким сооружением, построенным людьми?
- EN: For how many years was the Great Pyramid the tallest structure built by people?
- Answer: **3800** years, range 0 to 6000, tolerance 700
- Scores: F 5, C 5, G 3, D 3, E 4, A 5, P 5
- Verdict: A brilliant scale question: everyone knows the pyramid, the answer (about 3,800 years) is genuinely staggering, and the fallen English spire is a perfect kicker.

## money

### money_013 (multipleChoice, easy): 76 GOOD, KEEP

- RU: Луковицу какого цветка в Голландии 1630-х продавали по цене дома?
- EN: In the 1630s Dutch buyers paid the price of a house for a bulb of which flower?
- Options: **Тюльпана | Tulip** / Розы | Rose / Лилии | Lily / Орхидеи | Orchid
- Scores: F 5, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: Tulip mania is fairly well known so it is close to recognition, but the 'price of a house' framing is fun and the bulbs traded while still in the ground make a good payoff.

### money_006 (multipleChoice, easy): 74 GOOD, KEEP

- RU: Английское salary (зарплата) происходит от латинского слова, означающего…
- EN: The word salary comes from the Latin word for…
- Options: **соль | salt** / серебро | silver / хлеб | bread / щит | shield
- Scores: F 5, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: A classic word-origin question with a satisfying 'salt' reveal and credible distractors; the soldier/solidus fact doubles the payoff.

### money_011 (multipleChoice, easy): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: В честь чего названа нью-йоркская Уолл-стрит?
- EN: What is New York's Wall Street named after?
- Options: **Настоящей стены поселенцев | A real settlers' wall** / Банкира по фамилии Уолл | A banker named Wall / Стены первой биржи | The wall of the first exchange / Стены злых кредиторов | A wall of angry creditors
- Scores: F 4, C 3, G 4, D 2, E 3, A 4, P 4
- Verdict: Good literal-name reveal, but 'real settlers' wall' gives itself away with the word 'real' and 'a wall of angry creditors' is a throwaway.
- Rewrite (75 GOOD): RU: В честь чего названа нью-йоркская Уолл-стрит? / EN: What is New York's Wall Street named after?
  - Options: **Городской стены | A town wall** / Банкира Уолла | A banker named Wall / Стены биржи | The stock exchange wall / Капитана Уолла | A captain named Wall
  - Why better: Short parallel options with two tempting 'someone named Wall' traps make the literal answer a real surprise.

### money_026 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: В какой стране работает старейший из ныне действующих банков, основанный в 1472 году?
- EN: The oldest bank still operating was founded in 1472. In which country?
- Options: **Италия | Italy** / Англия | England / Швейцария | Switzerland / Нидерланды | The Netherlands
- Scores: F 3, C 3, G 3, D 4, E 2, A 5, P 4
- Verdict: Italy is guessable via Renaissance banking, but a bare country answer is flat; the pawnshop-for-the-poor origin is the better hook and is buried in the fact.
- Rewrite (72 GOOD): RU: Старейший действующий банк мира (1472) начинался как ломбард для бедных. В какой стране? / EN: The oldest bank still operating (1472) began as a pawnshop for the poor. In which country?
  - Options: **Италия | Italy** / Англия | England / Швейцария | Switzerland / Нидерланды | The Netherlands
  - Why better: The surprising pawnshop origin moves into the question, so players react before the country is even revealed.

### money_036 (multipleChoice, medium): 84 GOOD, KEEP

- RU: Что изображено на мостах с банкнот евро?
- EN: What do the bridges on euro banknotes show?
- Options: **Выдуманные мосты | Imaginary bridges** / Мосты столиц еврозоны | Bridges of eurozone capitals / Мосты Брюсселя | Bridges in Brussels / Мосты стран-основательниц | Bridges of the founding nations
- Scores: F 4, C 5, G 3, D 4, E 4, A 5, P 5
- Verdict: Most players have held a euro note and never noticed the bridges are fictional; the Dutch town building real copies is a lovely, tellable payoff.

### money_019 (multipleChoice, medium): 54 REJECT, IMPROVE

- RU: Из чего были сделаны первые в мире монеты, отчеканенные в Лидии в VII веке до н. э.?
- EN: What were the world's first coins, struck in Lydia in the 7th century BC, made of?
- Options: **Сплава золота и серебра | A gold and silver alloy** / Бронзы | Bronze / Железа | Iron / Чистой меди | Pure copper
- Scores: F 2, C 3, G 3, D 2, E 2, A 3, P 4. Flags: OBSCURE_TRIVIA
- Verdict: Lydia means little to most players, the long date-laden question reads like a textbook, and the correct option is the only two-part answer (telegraphed). Croesus is the hook worth using.
- Rewrite (63 NEEDS IMPROVEMENT): RU: Из чего чеканили первые монеты в мире — в Лидии, на родине богача Крёза? / EN: What were the world's first coins made of, in Lydia, the land of rich King Croesus?
  - Options: **Золото с серебром | Gold mixed with silver** / Медь с оловом | Copper and tin / Серебро со свинцом | Silver and lead / Железо с никелем | Iron and nickel
  - Why better: 'Rich as Croesus' gives players a foothold, and four parallel two-metal alloys remove the length tell.

### money_021 (multipleChoice, medium): 76 GOOD, KEEP

- RU: Копейка получила название по тому, что было на ней изображено. Что это?
- EN: The kopeck is named after what was pictured on it. What?
- Options: **Всадник с копьём | A rider with a spear** / Двуглавый орёл | A double headed eagle / Царская корона | A royal crown / Пучок колосьев | A sheaf of wheat
- Scores: F 5, C 2, G 5, D 3, E 3, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Kopeck/kopyo makes the answer almost self-evident, so there is little surprise, but Russian players enjoy confirming it and the Elena Glinskaya detail adds a bit. Acceptable as an easy recognition question.

### money_020 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Какой была самая крупная купюра Зимбабве, выпущенная в 2009 году?
- EN: What was Zimbabwe's biggest banknote, issued in 2009?
- Options: **100 триллионов долларов | 100 trillion dollars** / 1 миллион долларов | 1 million dollars / 1 миллиард долларов | 1 billion dollars / 50 миллиардов долларов | 50 billion dollars
- Scores: F 4, C 4, G 3, D 4, E 4, A 5, P 4
- Verdict: The Zimbabwe trillion note is a known meme yet still makes people gasp at the exact size, and 10^27 original dollars is a great payoff.

### money_039 (closestValue, hard): 75 GOOD, KEEP

- RU: Сколько пенни было в британском фунте до 1971 года?
- EN: How many pennies were there in a British pound before 1971?
- Answer: **240** pennies, range 100 to 700, tolerance 35
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: 240 is surprising, and players who half-remember shillings can reason toward it; the 20 x 12 explanation is satisfying.

### money_050 (closestValue, hard): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько раз долларовую купюру можно согнуть туда-обратно, прежде чем она порвётся?
- EN: How many times can a dollar bill be folded back and forth before it tears?
- Answer: **4000** times, range 0 to 20000, tolerance 700
- Scores: F 4, C 4, G 2, D 3, E 4, A 4, P 3
- Verdict: A fun 'who would test that?' number, but there is nothing to estimate from, so it is near pure guessing; the cloth-not-paper fact is the clue that should be in the question.
- Rewrite (74 GOOD): RU: Долларовая купюра гнётся почти как ткань. Сколько сгибов туда-обратно она выдержит? / EN: A dollar bill bends almost like cloth. How many back-and-forth folds can it take?
  - Answer 4000, range 0 to 10000, tolerance 1000
  - Why better: The cloth clue (from the fact) gives players a reason to guess high, turning a blind number into an estimation argument.

## brands

### brands_010 (multipleChoice, easy): 86 EXCELLENT, KEEP

- RU: «Мишленовские звёзды» ресторанам раздаёт компания, которая делает…
- EN: The Michelin stars for restaurants come from a company that makes…
- Options: Вино | Wine / Газеты | Newspapers / Кухонные ножи | Kitchen knives / **Шины | Tyres**
- Scores: F 5, C 4, G 3, D 4, E 5, A 5, P 5
- Verdict: A top-tier reveal: the tyre company behind the most prestigious restaurant award, with a clever business reason in the fact. Some already know it, but it still starts conversations.

### brands_013 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Из какой страны компания Nokia?
- EN: Which country is Nokia from?
- Options: Норвегия | Norway / Япония | Japan / **Финляндия | Finland** / Швеция | Sweden
- Scores: F 5, C 1, G 4, D 4, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Nokia = Finland is widely known, so this is a free warm-up; the sables etymology is a mild bonus. Fine as a deliberate easy recognition question.

### brands_005 (multipleChoice, easy): 74 GOOD, KEEP

- RU: В честь кого назван Nike?
- EN: Who is Nike named after?
- Options: Олимпийской чемпионки | An Olympic champion / **Греческой богини победы | The Greek goddess of victory** / Жены основателя | The founder's wife / Римской богини охоты | The Roman goddess of hunting
- Scores: F 5, C 2, G 4, D 4, E 3, A 5, P 4
- Verdict: Familiar recognition with a good trap (the Roman goddess of hunting), and the 'Dimension Six' near-miss name is a fun payoff. The 'named after' formula recurs in the bank but the fact does not repeat.

### brands_027 (multipleChoice, medium): 81 GOOD, KEEP

- RU: Как изначально расшифровывалось название LG?
- EN: What did LG originally stand for?
- Options: **Лаки Голдстар | Lucky Goldstar** / Лайфс Гуд | Life's Good / Лайт энд Голд | Light & Gold / Ли Груп | Lee Group
- Scores: F 5, C 4, G 3, D 4, E 4, A 5, P 4
- Verdict: Everyone knows 'Life's Good', so the trap is perfect and 'Lucky Goldstar' (face cream plus radios) is a real surprise.

### brands_016 (multipleChoice, medium): 81 GOOD, KEEP

- RU: С чего начиналась Nokia в 1865 году?
- EN: What did Nokia start out making in 1865?
- Options: **Бумажная масса | Wood pulp for paper** / Лыжи | Skis / Телеграфные аппараты | Telegraph machines / Рыболовные сети | Fishing nets
- Scores: F 5, C 5, G 2, D 4, E 4, A 5, P 4
- Verdict: Nokia making paper pulp (and later rubber boots) is a classic 'wait, seriously?' with solid distractors. Note brands_013 is also about Nokia; consider not drawing both in one evening.

### brands_033 (multipleChoice, medium): 92 EXCELLENT, KEEP

- RU: Что создатель тубы Pringles завещал сделать со своим прахом?
- EN: What did the man who designed the Pringles can want done with his ashes?
- Options: Отправить в космос | Sent into space / Развеять над картофельным полем | Scattered over a potato field / **Похоронить в банке Pringles | Buried in a Pringles can** / Замуровать в стену завода | Sealed in the factory wall
- Scores: F 5, C 5, G 4, D 3, E 5, A 5, P 5
- Verdict: Absurd-but-true: the correct answer is the funniest option, so groups laugh as they pick it, and the children buying the can on the way to the funeral is a perfect kicker. The option repeats 'Pringles' from the question, a mild clue.

### brands_037 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Из чего сложено название IKEA?
- EN: What is the name IKEA made of?
- Options: **Имя основателя, ферма и деревня | Founder, his farm and village** / Имена четырёх детей основателя | Founder's four children / «Красивый дом» по-шведски | "Beautiful home" in Swedish / Аббревиатура мебельной фабрики | A furniture factory acronym
- Scores: F 5, C 3, G 3, D 3, E 3, A 4, P 4
- Verdict: IKEA is universally familiar and the acronym reveal is satisfying, though the correct option is the most specific one; the dyslexia/names-not-numbers fact lifts the payoff.

### brands_018 (multipleChoice, medium): 77 GOOD, KEEP

- RU: Чем торговала Samsung, когда открылась в 1938 году?
- EN: What did Samsung trade in when it opened in 1938?
- Options: Швейными машинками | Sewing machines / **Сушёной рыбой и лапшой | Dried fish and noodles** / Радиоприёмниками | Radios / Велосипедами | Bicycles
- Scores: F 5, C 5, G 2, D 2, E 4, A 5, P 4
- Verdict: A great surprise (Samsung began with dried fish), but the correct answer is the only two-item option, which telegraphs it as in the carrot anchor.
- Rewrite (81 GOOD): RU: Чем торговала Samsung, когда открылась в 1938 году? / EN: What did Samsung trade in when it opened in 1938?
  - Options: **Сушёной рыбой | Dried fish** / Швейными машинками | Sewing machines / Радиоприёмниками | Radios / Велосипедами | Bicycles
  - Why better: Single-item options remove the length tell so the fish answer becomes a real surprise.

### brands_045 (closestValue, hard): 59 NEEDS IMPROVEMENT, IMPROVE

- RU: С какого года строит храмы японская Kongō Gumi — её называют старейшей компанией мира?
- EN: Since what year has Japan's Kongō Gumi, often called the oldest company, built temples?
- Answer: **578** year, range 0 to 2000, tolerance 100
- Scores: F 2, C 4, G 2, D 3, E 3, A 4, P 3. Flags: NO_DEDUCTION
- Verdict: Kongō Gumi is unknown to most players and a founding year is hard to estimate; the 1,400-year lifespan is the real wow but only emerges if players also know it closed in 2006.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Японская фирма Kongō Gumi оставалась самостоятельной до 2006 года. В каком году её основали? / EN: Japan's temple builder Kongō Gumi stayed independent until 2006. In what year was it founded?
  - Answer 578, range 0 to 2000, tolerance 150
  - Why better: Anchoring the 2006 end date makes the founding year land as a jaw-dropping 1,400-year lifespan, and the wider tolerance rewards a bold early guess.

### brands_042 (closestValue, hard): 71 GOOD, KEEP

- RU: В каком году в Киото основали компанию Nintendo?
- EN: In what year was Nintendo founded in Kyoto?
- Answer: **1889** year, range 1600 to 2000, tolerance 20
- Scores: F 4, C 4, G 3, D 3, E 3, A 5, P 3
- Verdict: Most gamers are surprised Nintendo predates cars and films, and the range invites reasoning; the payoff would be stronger if the fact mentioned it began by making hanafuda playing cards.

## ancient

### ancient_003 (multipleChoice, easy): 67 NEEDS IMPROVEMENT, KEEP

- RU: Что должна была охранять Терракотовая армия в Китае?
- EN: What was China's Terracotta Army made to guard?
- Options: Великую стену | The Great Wall / Императорскую казну | The imperial treasury / **Гробницу первого императора | The first emperor's tomb** / Ворота столицы | The capital's gates
- Scores: F 5, C 2, G 4, D 3, E 2, A 5, P 3
- Verdict: Easy recognition question with a guessable answer; the 'every soldier has its own face' fact is a decent payoff. Fine as a warm-up.

### ancient_004 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Какое число римляне не могли записать своими цифрами?
- EN: Which number could the Romans not write with their numerals?
- Options: Тысячу | A thousand / **Ноль | Zero** / Единицу | One / Пятёрку | Five
- Scores: F 5, C 3, G 4, D 2, E 2, A 4, P 2
- Verdict: Most adults know the Romans had no zero, so it is an easy recognition point; 'One' is an absurd option. Acceptable as a warm-up, with a cheap distractor fix.
- Rewrite (70 GOOD): RU: Какое число римляне не могли записать своими цифрами? / EN: Which number could the Romans not write with their numerals?
  - Options: **Ноль | Zero** / Тысячу | A thousand / Четыре | Four / Девятьсот | Nine hundred
  - Why better: 'A thousand' (is it M?) and 'Four' (IIII or IV?) make players hesitate and argue instead of eliminating instantly.

### ancient_014 (multipleChoice, easy): 69 NEEDS IMPROVEMENT, KEEP

- RU: Тело какого животного у Большого сфинкса в Гизе?
- EN: The Great Sphinx of Giza has the body of which animal?
- Options: **Льва | A lion** / Быка | A bull / Шакала | A jackal / Сокола | A falcon
- Scores: F 5, C 1, G 5, D 3, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Almost everyone knows the Sphinx is a lion, so the question itself is free; the Napoleon nose myth-buster is the real payoff. Keep only as an easy warm-up.

### ancient_032 (multipleChoice, medium): 76 GOOD, KEEP

- RU: Что такое термополий в Помпеях?
- EN: What was a thermopolium in Pompeii?
- Options: Лавка лекаря | A doctor's shop / **Уличная закусочная | A street food counter** / Общественная баня | A public bath / Школа гладиаторов | A gladiator school
- Scores: F 3, C 4, G 4, D 4, E 3, A 5, P 4
- Verdict: 'Thermo' plus Pompeii gives a reasoning path, the distractors are credible, and Roman fast food for kitchenless flats is a fun modern parallel.

### ancient_025 (multipleChoice, medium): 76 GOOD, KEEP

- RU: Что должен был проверить Архимед, когда крикнул «Эврика!»?
- EN: What was Archimedes trying to test when he shouted "Eureka!"?
- Options: **Из чистого ли золота корона | If a crown was pure gold** / Круглая ли Земля | If the Earth was round / Глубину моря | How deep the sea was / Прочность моста | How strong a bridge was
- Scores: F 5, C 3, G 4, D 2, E 3, A 5, P 5
- Verdict: Everyone knows 'Eureka!' and the bath, few know why; the naked run home is a great payoff. The correct option is much longer than the others (telegraphed).
- Rewrite (80 GOOD): RU: Что должен был проверить Архимед, когда крикнул «Эврика!»? / EN: What was Archimedes trying to test when he shouted "Eureka!"?
  - Options: **Из золота ли корона | If a crown was pure gold** / Кругла ли Земля | If the Earth was round / Глубоко ли море | How deep the sea was / Прочен ли мост | If a bridge was strong
  - Why better: Parallel 'is it X' options of equal length remove the length tell.

### ancient_034 (multipleChoice, medium): 84 GOOD, KEEP

- RU: Что сделал Ксеркс, когда буря разрушила его мост через Геллеспонт?
- EN: What did Xerxes do when a storm wrecked his bridge over the Hellespont?
- Options: Принёс в жертву коня | Sacrificed his horse / Отменил поход | Called off the war / Переименовал пролив | Renamed the strait / **Велел высечь море плетьми | Had the sea whipped**
- Scores: F 3, C 5, G 3, D 4, E 5, A 5, P 5
- Verdict: Whipping the sea is absurd and true, the other options are sensible-sounding, and 300 lashes plus shackles is a laugh-out-loud payoff.

### ancient_027 (multipleChoice, medium): 59 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой древний народ обозначал ноль значком в виде раковины?
- EN: Which ancient people wrote zero with a sign shaped like a shell?
- Options: Викинги | The Vikings / Египтяне | The Egyptians / **Майя | The Maya** / Римляне | The Romans
- Scores: F 3, C 3, G 3, D 2, E 2, A 5, P 3
- Verdict: The shell-shaped zero is a nice detail, but the distractors are weak: the Vikings are absurd and the Romans had no zero at all (ancient_004 even teaches that), so Maya wins by elimination.
- Rewrite (63 NEEDS IMPROVEMENT): RU: Какой древний народ обозначал ноль значком в виде раковины? / EN: Which ancient people wrote zero with a sign shaped like a shell?
  - Options: **Майя | The Maya** / Кельты | The Celts / Этруски | The Etruscans / Финикийцы | The Phoenicians
  - Why better: Four credible ancient peoples (the seafaring Phoenicians are a tempting 'shell' trap) replace two give-away eliminations.

### ancient_019 (multipleChoice, medium): 87 EXCELLENT, KEEP

- RU: Для чего римляне собирали мочу из общественных уборных?
- EN: What did the Romans collect urine from public toilets for?
- Options: Для строительного раствора | Building mortar / Для краски стен | Painting walls / **Для стирки одежды | Washing clothes** / Для заправки ламп | Fuelling lamps
- Scores: F 4, C 5, G 3, D 4, E 5, A 5, P 5
- Verdict: Gross, surprising and educational: urine for laundry gets a groan and a laugh, and 'money does not smell' is a payoff people will repeat.

### ancient_048 (closestValue, hard): 57 NEEDS IMPROVEMENT, IMPROVE

- RU: До скольких лет дожил первый римский император Август?
- EN: How old was Augustus, the first Roman emperor, when he died?
- Answer: **75** years old, range 20 to 100, tolerance 10
- Scores: F 3, C 2, G 3, D 3, E 2, A 5, P 3. Flags: BORING_REVEAL
- Verdict: An emperor's age at death is an exam-style number with a dull reveal; the hook (August named after him while alive) sits in the fact.
- Rewrite (66 NEEDS IMPROVEMENT): RU: Месяц август назван в честь императора Августа ещё при его жизни. До скольких лет он дожил? / EN: August was named after Emperor Augustus in his lifetime. How old was he when he died?
  - Answer 75, range 20 to 100, tolerance 8
  - Why better: The month hook makes the subject instantly familiar and hints that he lived long enough to see it.

### ancient_046 (closestValue, hard): 60 NEEDS IMPROVEMENT, REPLACE

- RU: Сколько тысяч человек, по оценкам историков, жило в Риме в эпоху расцвета?
- EN: How many thousand people lived in Rome at its peak, by historians' estimates?
- Answer: **1000** thousand people, range 50 to 3000, tolerance 350
- Scores: F 4, C 3, G 3, D 3, E 2, A 3, P 3. Flags: NO_DEDUCTION
- Verdict: Rome reaching a million is a decent fact, but 'thousand people' as the unit is awkward and there is nothing in the question or fact to estimate from. Better rebuilt as a multiple-choice question ('which was likely the first city to reach a million people?').

## mythology

### mythology_005 (multipleChoice, easy): 73 GOOD, KEEP

- RU: Чем Один заплатил за глоток из источника мудрости?
- EN: What did Odin pay for a drink from the well of wisdom?
- Options: **Своим глазом | One of his eyes** / Своей рукой | One of his hands / Своим конём | His horse / Своим голосом | His voice
- Scores: F 4, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: Odin's missing eye is familiar from Marvel and mythology, the distractors are plausible sacrifices, and the Tyr hand twist is a good payoff.

### mythology_010 (multipleChoice, easy): 62 NEEDS IMPROVEMENT, KEEP

- RU: Кицунэ из японских легенд — это хитрый волшебный...
- EN: In Japanese legend, a kitsune is a cunning magical...
- Options: **Лис | Fox** / Кот | Cat / Журавль | Crane / Карп | Carp
- Scores: F 4, C 2, G 3, D 3, E 2, A 5, P 4
- Verdict: Kitsune = fox is easy for the anime-literate younger players and guessable for the rest; the nine tails fact is a nice bonus. Fine as an easy recognition question.

### mythology_011 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: В кого влюбился Нарцисс?
- EN: Who did Narcissus fall in love with?
- Options: **В своё отражение | His own reflection** / В нимфу Эхо | The nymph Echo / В богиню Геру | The goddess Hera / В мраморную статую | A marble statue
- Scores: F 5, C 1, G 4, D 4, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: The word 'narcissism' gives it away, though Echo is a good trap; acceptable as a deliberate easy warm-up.

### mythology_035 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Кем по ремеслу были лепреконы в ирландском фольклоре?
- EN: In Irish folklore, what was the leprechaun's trade?
- Options: **Сапожниками | Shoemaker** / Кузнецами | Blacksmith / Пекарями | Baker / Портными | Tailor
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 3
- Verdict: Leprechauns are familiar and 'shoemaker' surprises; the buckled-shoe image gives a faint reasoning path. Solid.

### mythology_029 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Как японские боги выманили богиню солнца Аматэрасу из пещеры?
- EN: How did the Japanese gods lure the sun goddess Amaterasu out of her cave?
- Options: **Устроили шумное веселье | They threw a noisy party** / Пообещали ей рис | They promised her rice / Устроили землетрясение | They caused an earthquake / Притворились спящими | They pretended to sleep
- Scores: F 2, C 4, G 3, D 4, E 4, A 4, P 5
- Verdict: Amaterasu is half-familiar, but 'the gods threw a party' is a delightful answer and the mirror trick is a great payoff.

### mythology_030 (multipleChoice, medium): 81 GOOD, KEEP

- RU: Как, по японским поверьям, одолеть водяного каппу?
- EN: In Japanese folklore, how can you beat a kappa, the river imp?
- Options: **Низко ему поклониться | Bow deeply to it** / Посмотреть ему в глаза | Stare it in the eye / Спеть ему песню | Sing it a song / Бросить в него рыбу | Throw a fish at it
- Scores: F 2, C 5, G 3, D 4, E 5, A 5, P 5
- Verdict: Beating a monster by bowing to it is a wonderful twist, the distractors are credible folklore moves, and the spilled-water explanation is a perfect payoff.

### mythology_034 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, KEEP

- RU: Каким мифическим существам обязаны своим названием слова «музей» и «музыка»?
- EN: Which mythical beings gave us the words 'museum' and 'music'?
- Options: **Музам | The Muses** / Сиренам | The Sirens / Нимфам | The nymphs / Харитам | The Graces
- Scores: F 4, C 2, G 5, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: The words 'museum' and 'music' both contain 'mus', so the answer is self-evident; it works only as an easy warm-up (consider moving it to easy).

### mythology_027 (multipleChoice, medium): 63 NEEDS IMPROVEMENT, IMPROVE

- RU: Велеса летописи называют «скотьим богом». Чем ещё он ведал?
- EN: The chronicles call Veles the 'god of cattle'. What else did he rule?
- Options: **Богатством | Wealth** / Войной | War / Солнцем | The sun / Огнём | Fire
- Scores: F 3, C 3, G 4, D 3, E 2, A 4, P 3
- Verdict: The 'cattle god' hint makes wealth guessable, which is nice, but the distractors (war, sun, fire) are generic and the reveal is mild.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Летописи называют Велеса «скотьим богом». Чему ещё он покровительствовал? / EN: The chronicles call Veles the 'god of cattle'. What else did he rule over?
  - Options: **Богатству | Wealth** / Грозе | Thunder / Солнцу | The sun / Войне | War
  - Why better: Thunder (Perun's domain) is a tempting Slavic trap, so players who know a little mythology must think rather than eliminate.

### mythology_041 (closestValue, hard): 52 REJECT, REPLACE

- RU: Сколько примерно двустиший-шлок в индийском эпосе «Махабхарата»?
- EN: About how many couplets, or shlokas, are in the Indian epic, the Mahabharata?
- Answer: **100000** couplets, range 0 to 400000, tolerance 35000
- Scores: F 2, C 4, G 1, D 2, E 2, A 4, P 4. Flags: NO_DEDUCTION, OBSCURE_TRIVIA
- Verdict: Nobody can estimate couplets in the Mahabharata and the number means nothing on its own; the real wow (about ten times the Iliad and Odyssey combined) needs a different question with a different answer.

### mythology_040 (closestValue, hard): 52 REJECT, REPLACE

- RU: В каком году Джордж Смит объявил, что нашёл на табличке о Гильгамеше рассказ о потопе?
- EN: In which year did George Smith announce he had found a flood story on a Gilgamesh tablet?
- Answer: **1872** year, range 1800 to 2025, tolerance 25
- Scores: F 1, C 3, G 2, D 2, E 3, A 3, P 5. Flags: OBSCURE_TRIVIA, NO_DEDUCTION
- Verdict: George Smith is unknown and the year is arbitrary; the fun part is that he started undressing in excitement, which deserves its own multiple-choice question rather than a date.

## medieval

### medieval_011 (multipleChoice, easy): 58 NEEDS IMPROVEMENT, IMPROVE

- RU: Кто был главным противником Ричарда Львиное Сердце в Третьем крестовом походе?
- EN: Who was Richard the Lionheart's great rival in the Third Crusade?
- Options: **Саладин | Saladin** / Сулейман Великолепный | Suleiman the Magnificent / Харун ар-Рашид | Harun al-Rashid / Синдбад-мореход | Sinbad the Sailor
- Scores: F 4, C 2, G 3, D 2, E 2, A 5, P 3
- Verdict: Saladin is a fair recognition answer, but Sinbad and Harun al-Rashid are throwaways and the correct option is much shorter than the rest; the gift-swapping rivalry is the better hook.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Ричард Львиное Сердце в Крестовом походе получал подарки от главного врага. От кого? / EN: On crusade, Richard the Lionheart received gifts from his great enemy. Who was it?
  - Options: **Саладин | Saladin** / Тамерлан | Tamerlane / Сулейман | Suleiman / Баязид | Bayezid
  - Why better: The 'gifts from the enemy' hook adds surprise, and four similar Muslim rulers' names remove the Sinbad giveaway.

### medieval_009 (multipleChoice, easy): 76 GOOD, KEEP

- RU: Кто добрался до Северной Америки примерно за 500 лет до Колумба?
- EN: Who reached North America about 500 years before Columbus?
- Options: **Викинги | The Vikings** / Монголы | The Mongols / Венецианцы | The Venetians / Португальцы | The Portuguese
- Scores: F 5, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: A familiar, guessable question with a tree-ring payoff that feels like detective work; fine.

### medieval_008 (multipleChoice, easy): 56 NEEDS IMPROVEMENT, IMPROVE

- RU: Кто около 1450 года начал печатать книги наборным шрифтом в Европе?
- EN: Who began printing books with movable type in Europe around 1450?
- Options: **Иоганн Гутенберг | Johannes Gutenberg** / Леонардо да Винчи | Leonardo da Vinci / Галилео Галилей | Galileo Galilei / Христофор Колумб | Christopher Columbus
- Scores: F 5, C 1, G 3, D 2, E 1, A 5, P 4. Flags: SCHOOL_EXAM, OBVIOUS_ANSWER
- Verdict: Gutenberg is a school-exam answer next to Columbus and Galileo; the interesting part (China and Korea were centuries earlier) is hidden in the fact.
- Rewrite (71 GOOD): RU: Китай печатал наборным шрифтом с 1040 года. Кто начал так печатать в Европе около 1450-го? / EN: China printed with movable type from 1040. Who started doing it in Europe around 1450?
  - Options: **Иоганн Гутенберг | Johannes Gutenberg** / Иван Фёдоров | Ivan Fyodorov / Альбрехт Дюрер | Albrecht Dürer / Леонардо да Винчи | Leonardo da Vinci
  - Why better: The China hook adds a 'Europe was late' surprise, and Fyodorov (a real printer) plus Dürer (an engraver) are credible traps.

### medieval_024 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: Где стояла столица ацтеков Теночтитлан?
- EN: Where did the Aztec capital Tenochtitlan stand?
- Options: **На острове посреди озера | On an island in a lake** / На вершине вулкана | On top of a volcano / В пещерах под землёй | In caves underground / На берегу океана | On the ocean shore
- Scores: F 3, C 3, G 3, D 3, E 3, A 5, P 4
- Verdict: The island city is a good fact, but 'in caves underground' is filler and the question reads like a geography quiz; the Venice comparison is a better way in.
- Rewrite (74 GOOD): RU: Испанцы сравнивали столицу ацтеков Теночтитлан с Венецией. Почему? / EN: The Spanish compared the Aztec capital Tenochtitlan to Venice. Why?
  - Options: **Стояла на острове в озере | It stood on an island in a lake** / Славилась стеклом и масками | It was famous for glass and masks / Была построена на морских сваях | It was built on piles in the sea / Её заложили итальянцы | Italians had founded it
  - Why better: Players reason from what they know about Venice, and the drained-lake payoff about Mexico City lands harder.

### medieval_018 (multipleChoice, medium): 76 GOOD, KEEP

- RU: К какому веку относятся первые описания чумного доктора в маске с клювом?
- EN: From which century do the first descriptions of the beaked plague doctor mask come?
- Options: **К XVII веку | The 17th century** / К XIV веку | The 14th century / К XII веку | The 12th century / К XIX веку | The 19th century
- Scores: F 4, C 5, G 2, D 4, E 4, A 4, P 4
- Verdict: Busts the Black Death image everyone has: the beaked mask is 300 years later, a real 'wait, seriously?'.

### medieval_032 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: Монголы построили по всей империи сеть станций «ям». Для чего она была?
- EN: The Mongols built a network of yam stations across their empire. What was it for?
- Options: **Для почты и гонцов | Mail and messengers** / Для хранения зерна | Storing grain / Для торговли шёлком | Trading silk / Для слежки за звёздами | Watching the stars
- Scores: F 3, C 3, G 4, D 3, E 2, A 4, P 4
- Verdict: The yamshchik link is a great hook for Russian players but it is only in the fact, and 'watching the stars' is a throwaway.
- Rewrite (73 GOOD): RU: Монгольские «ямы» подарили русскому языку слово «ямщик». Чем они были? / EN: The Mongol yam gave Russian the word yamshchik, a coachman. What were the yams?
  - Options: **Почтовыми станциями | Relay post stations** / Зерновыми складами | Grain stores / Таможенными постами | Customs posts / Тюремными ямами | Prison pits
  - Why better: The familiar Russian word gives a reasoning path and an 'aha', and 'prison pits' is a fun literal-minded trap.

### medieval_031 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Какой из этих городов монголы так и НЕ взяли?
- EN: Which of these cities did the Mongols NEVER take?
- Options: **Вену | Vienna** / Киев | Kyiv / Багдад | Baghdad / Пекин | Beijing
- Scores: F 4, C 3, G 4, D 4, E 3, A 4, P 3
- Verdict: Good reasoning question for Russians who know Kyiv fell in 1240; the NOT wording is clear. The turn-back story is a modest payoff.

### medieval_023 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, REPLACE

- RU: Чьё паломничество в Мекку в 1324 году, говорят, обрушило цену золота в Каире?
- EN: Whose pilgrimage to Mecca in 1324 is said to have crashed the price of gold in Cairo?
- Options: **Мансы Мусы, правителя Мали | Mansa Musa of Mali** / Персидского шаха | A Persian shah / Китайского императора | A Chinese emperor / Короля Англии | The King of England
- Scores: F 2, C 5, G 2, D 2, E 4, A 4, P 4. Flags: REPETITIVE
- Verdict: Same 1324 Cairo gold story as rulers_021 (and per the script money_027), and here the correct option is the only named specific ruler among vague titles; keep rulers_021 instead.

### medieval_043 (closestValue, hard): 59 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько миллионов квадратных километров занимала Монгольская империя на пике?
- EN: How many million square kilometres did the Mongol Empire cover at its peak?
- Answer: **23.5** million km², range 0 to 70, tolerance 5
- Scores: F 3, C 3, G 3, D 3, E 2, A 4, P 3. Flags: NO_DEDUCTION
- Verdict: A pure area estimate with no anchor; players have no feel for millions of square kilometres unless given a comparison.
- Rewrite (66 NEEDS IMPROVEMENT): RU: Россия занимает 17 млн км². Сколько миллионов км² занимала Монгольская империя на пике? / EN: Russia covers 17 million km². How many million km² did the Mongol Empire cover at its peak?
  - Answer 23.5, range 0 to 60, tolerance 4
  - Why better: Russia's size (about 17 million km², school knowledge) is a number every player in this family knows, so the guess becomes a real 'bigger or smaller than Russia?' debate.

### medieval_048 (closestValue, hard): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой длины гобелен из Байё, вышитая история завоевания Англии в 1066 году?
- EN: How long is the Bayeux Tapestry, the stitched story of the conquest of England in 1066?
- Answer: **68** m, range 0 to 200, tolerance 15
- Scores: F 2, C 4, G 2, D 3, E 3, A 4, P 4
- Verdict: Nice 'it is not even a tapestry' payoff, but the length is a blind guess for most and the question reads dry.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Гобелен из Байё — вышитый «комикс» о завоевании Англии. Какой он длины в метрах? / EN: The Bayeux Tapestry is an embroidered comic strip of the conquest of England. How long is it, in metres?
  - Answer 68, range 0 to 200, tolerance 15
  - Why better: Calling it a comic strip makes players picture a long scroll of panels, a better basis for a guess.

## history20

### history20_002 (multipleChoice, easy): 63 NEEDS IMPROVEMENT, KEEP

- RU: В каком году затонул «Титаник»?
- EN: In which year did the Titanic sink?
- Options: **1912 | 1912** / 1905 | 1905 / 1921 | 1921 / 1931 | 1931
- Scores: F 5, C 1, G 3, D 3, E 2, A 5, P 5. Flags: OBVIOUS_ANSWER
- Verdict: Everyone knows 1912, so the question is free, but the 'Futility' novel predicting the sinking is a superb payoff. Keep as a deliberate easy recognition question.

### history20_012 (multipleChoice, easy): 58 NEEDS IMPROVEMENT, KEEP

- RU: В каком году люди впервые прошлись по Луне?
- EN: In which year did people first walk on the Moon?
- Options: **1969 | 1969** / 1961 | 1961 / 1975 | 1975 / 1957 | 1957
- Scores: F 5, C 1, G 3, D 3, E 1, A 5, P 4. Flags: OBVIOUS_ANSWER, SCHOOL_EXAM
- Verdict: 1969 is common knowledge, so it is an exam warm-up; the 4 KB computer fact gives some payoff. Keep only as a deliberate easy recognition question.

### history20_007 (multipleChoice, easy): 81 GOOD, KEEP

- RU: Огромную надпись HOLLYWOOD на холмах Лос-Анджелеса поставили в 1923 году. Что она рекламировала?
- EN: The giant HOLLYWOOD sign went up in the Los Angeles hills in 1923. What was it advertising?
- Options: **Новый жилой посёлок | A new housing estate** / Киностудию | A film studio / Радиостанцию | A radio station / Парк развлечений | An amusement park
- Scores: F 5, C 4, G 3, D 4, E 4, A 4, P 5
- Verdict: Everyone knows the sign, almost nobody knows it was a real-estate ad, and HOLLYWOODLAND is a great tellable reveal; the film-studio trap is perfect.

### history20_025 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Кто первым в истории получил две Нобелевские премии?
- EN: Who was the first person ever to win two Nobel Prizes?
- Options: **Мария Склодовская-Кюри | Marie Curie** / Альберт Эйнштейн | Albert Einstein / Нильс Бор | Niels Bohr / Иван Павлов | Ivan Pavlov
- Scores: F 4, C 2, G 4, D 4, E 2, A 5, P 5
- Verdict: Curie is a known answer for many, but Einstein and Bohr are credible traps and the radioactive notebooks are a memorable payoff.

### history20_020 (multipleChoice, medium): 74 GOOD, KEEP

- RU: В честь кого назван плюшевый мишка Тедди?
- EN: Who is the teddy bear named after?
- Options: **Президента США Теодора Рузвельта | US President Theodore Roosevelt** / Медведя из Лондонского зоопарка | A bear at London Zoo / Мастерицы Маргарете Штайфф | Toymaker Margarete Steiff / Русского циркового медведя | A Russian circus bear
- Scores: F 5, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Familiar subject with a good Steiff trap, and the 'refused to shoot a tied bear' story is satisfying.

### history20_017 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: Каким был первый радиосигнал через Атлантику, который, по словам Маркони, он принял в 1901 году?
- EN: What was the first radio signal across the Atlantic that Marconi said he received, in 1901?
- Options: **Буква S азбуки Морзе | The Morse letter S** / Сигнал SOS | An SOS signal / Слово «Привет» | The word "Hello" / Его фамилия | His own surname
- Scores: F 3, C 3, G 3, D 4, E 2, A 3, P 4. Flags: TOO_WORDY
- Verdict: The SOS trap is good, but the question is long and clunky to read, and the kite-held aerial (the fun bit) is hidden in the fact.
- Rewrite (70 GOOD): RU: Маркони поднял антенну на воздушном змее. Какой сигнал он принял через Атлантику в 1901-м? / EN: Marconi flew his aerial on a kite. What signal did he say he got across the Atlantic in 1901?
  - Options: **Букву S азбуки Морзе | The Morse letter S** / Сигнал SOS | An SOS signal / Слово «Привет» | The word 'Hello' / Свою фамилию | His own surname
  - Why better: Shorter and opens with a vivid image, so the 'just three clicks' reveal feels charming rather than technical.

### history20_023 (multipleChoice, medium): 55 NEEDS IMPROVEMENT, REPLACE

- RU: Какой канал открылся в 1914 году?
- EN: Which canal opened in 1914?
- Options: **Панамский | The Panama Canal** / Суэцкий | The Suez Canal / Кильский | The Kiel Canal / Коринфский | The Corinth Canal
- Scores: F 4, C 1, G 3, D 4, E 1, A 5, P 3. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: Matching a canal to its opening year is a pure school exam question, and the 'halved the voyage' fact is predictable.

### history20_027 (multipleChoice, medium): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: В какой стране в 1989 году прошла «бархатная революция»?
- EN: Which country had the Velvet Revolution in 1989?
- Options: **В Чехословакии | Czechoslovakia** / В Польше | Poland / В Венгрии | Hungary / В Румынии | Romania
- Scores: F 4, C 2, G 3, D 4, E 2, A 5, P 4
- Verdict: A known name-the-country question; the much better story (a jailed playwright became president within months) is buried in the fact.
- Rewrite (75 GOOD): RU: Весной 1989-го драматург сидел в тюрьме, а к концу года стал президентом. Какой страны? / EN: In spring 1989 a playwright was in prison; by the end of the year he was president. Of which country?
  - Options: **Чехословакии | Czechoslovakia** / Польши | Poland / Венгрии | Hungary / Румынии | Romania
  - Why better: The prison-to-presidency twist creates surprise and invites reasoning about which 1989 revolution was led by artists.

### history20_042 (closestValue, hard): 74 GOOD, KEEP

- RU: Сколько спасательных шлюпок было на «Титанике»?
- EN: How many lifeboats did the Titanic carry?
- Answer: **20** lifeboats, range 0 to 100, tolerance 7
- Scores: F 5, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Everyone knows there were too few lifeboats, so players reason toward a small number; 'more than the law required' is a good twist.

### history20_049 (closestValue, hard): 75 GOOD, KEEP

- RU: Какой была средняя продолжительность жизни в мире в 1900 году?
- EN: What was the average life expectancy in the world in 1900?
- Answer: **32** years, range 0 to 100, tolerance 8
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: Most will guess far higher, the number is estimable, and the child-mortality explanation turns the surprise into insight.

## ussr

### ussr_015 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Сколько этажей чаще всего было в хрущёвке?
- EN: How many storeys did a typical Khrushchyovka block have?
- Options: **5 | 5** / 9 | 9 / 3 | 3 / 12 | 12
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 4
- Verdict: Instant recognition for this family, and the 'no lift above five floors' reason turns a known fact into an 'aha'.

### ussr_002 (multipleChoice, easy): 62 NEEDS IMPROVEMENT, KEEP

- RU: Сколько соток обычно давали семье под садовый участок?
- EN: How many "sotkas" (100 m² each) was a typical Soviet garden plot?
- Options: **6 | 6** / 2 | 2 / 25 | 25 / 50 | 50
- Scores: F 5, C 1, G 4, D 2, E 3, A 4, P 3. Flags: OBVIOUS_ANSWER
- Verdict: 'Six sotkas' is a household phrase, so it is a nostalgic free point; 25 and 50 are implausible. Keep as a deliberate easy recognition question.

### ussr_001 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Чей портрет был на октябрятской звёздочке?
- EN: Whose portrait was on the Little Octobrist star badge?
- Options: **Маленького Ленина | Lenin as a child** / Юрия Гагарина | Yuri Gagarin / Павлика Морозова | Pavlik Morozov / Мальчика-пионера | A Young Pioneer boy
- Scores: F 4, C 2, G 3, D 4, E 3, A 5, P 3
- Verdict: A warm nostalgia question for the 40-50 group (less so for 23-year-olds); the distractors are credible Soviet icons and the five-pointed 'stars' detail is a small payoff. Fine as easy recognition.

### ussr_033 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Сколько пассажирских авиакомпаний было в СССР?
- EN: How many passenger airlines did the USSR have?
- Options: **Одна | One** / Три | Three / 15, по одной на республику | 15, one per republic / Около пятидесяти | About fifty
- Scores: F 4, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: 'Just one airline' surprises the younger players and the crop-spraying, air-ambulance Aeroflot is a good payoff; 'about fifty' is weak.

### ussr_036 (multipleChoice, medium): 77 GOOD, KEEP

- RU: Как попрощался олимпийский Мишка на закрытии Игр-80?
- EN: How did Misha the bear say goodbye at the 1980 Olympics closing ceremony?
- Options: **Улетел на воздушных шарах | Flew off on balloons** / Уплыл по Москве-реке | Sailed off down the Moskva / Уехал на тройке | Rode off on a troika / Ушёл спать в берлогу | Went off to hibernate
- Scores: F 5, C 3, G 3, D 4, E 4, A 5, P 4
- Verdict: An iconic, emotional memory for the older players, and the crying-Misha card mosaic is a lovely payoff; the distractors are playful but credible.

### ussr_023 (multipleChoice, medium): 79 GOOD, KEEP

- RU: Сколько весила советская монета в 5 копеек образца 1961 года?
- EN: How much did a Soviet 5-kopeck coin of the 1961 type weigh?
- Options: **5 граммов | 5 grams** / 7 граммов | 7 grams / 3 грамма | 3 grams / 10 граммов | 10 grams
- Scores: F 4, C 4, G 4, D 4, E 3, A 5, P 4
- Verdict: A coin weighing its own value is a neat 'wait, really?', the matching number gives a smart reasoning path, and the 'count change on scales' payoff is great.

### ussr_026 (multipleChoice, medium): 80 GOOD, KEEP

- RU: Для кого изначально создавали «Докторскую» колбасу в 1936 году?
- EN: Who was Doktorskaya sausage originally created for in 1936?
- Options: **Для больных и ослабленных | The sick and frail** / Для врачей на дежурствах | Doctors on night shifts / Для полярников | Polar explorers / Для детей в яслях | Children in nurseries
- Scores: F 5, C 3, G 4, D 4, E 3, A 5, P 5
- Verdict: Everyone knows Doktorskaya, 'for doctors on night shift' is a perfect trap, and the 'tsarist despotism' quote is a laugh.

### ussr_021 (multipleChoice, medium): 54 REJECT, REPLACE

- RU: Как называлась главная государственная фирма грамзаписи СССР?
- EN: What was the Soviet Union's main state record label called?
- Options: **«Мелодия» | Melodiya** / «Гармония» | Garmoniya / «Пластинка» | Plastinka / «Аккорд» | Akkord
- Scores: F 4, C 1, G 3, D 3, E 2, A 5, P 2. Flags: OBVIOUS_ANSWER, BORING_REVEAL
- Verdict: Older players know Melodiya instantly, younger ones guess from the name; the 200 million records fact is a flat payoff with no surprise.

### ussr_042 (closestValue, hard): 57 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько километров проехал по Луне «Луноход-1»?
- EN: How many kilometres did Lunokhod 1 drive on the Moon?
- Answer: **10.5** km, range 0 to 100, tolerance 4
- Scores: F 3, C 3, G 2, D 3, E 2, A 5, P 3. Flags: NO_DEDUCTION
- Verdict: Lunokhod is familiar, but players have nothing to estimate the distance from; the 11-month run is the clue that should be in the question.
- Rewrite (65 NEEDS IMPROVEMENT): RU: «Луноход-1» колесил по Луне почти 11 месяцев. Сколько километров он проехал? / EN: Lunokhod 1 roamed the Moon for almost 11 months. How many kilometres did it cover?
  - Answer 10.5, range 0 to 60, tolerance 4
  - Why better: Knowing it drove for 11 months makes the tiny distance a real surprise and gives a basis for reasoning about speed.

### ussr_045 (closestValue, hard): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году в СССР перешли на пятидневку с двумя выходными?
- EN: In what year did the USSR switch to a five-day week with two days off?
- Answer: **1967** year, range 1922 to 1991, tolerance 8
- Scores: F 4, C 3, G 3, D 3, E 3, A 5, P 2
- Verdict: A year to recall, with a dull fact; framing it around working Saturdays makes it personal for the family's elders.
- Rewrite (70 GOOD): RU: До какого года суббота в СССР была обычным рабочим днём? / EN: Until what year was Saturday an ordinary working day in the USSR?
  - Answer 1967, range 1922 to 1991, tolerance 6
  - Why better: 'Working Saturdays' is something parents and grandparents remember, so the year sparks stories instead of a date quiz.

## nineties

### nineties_002 (multipleChoice, easy): 66 NEEDS IMPROVEMENT, KEEP

- RU: Кто спел «My Heart Will Go On» — главную песню «Титаника»?
- EN: Who sang My Heart Will Go On, the big song from Titanic?
- Options: **Селин Дион | Céline Dion** / Уитни Хьюстон | Whitney Houston / Мэрайя Кэри | Mariah Carey / Тони Брэкстон | Toni Braxton
- Scores: F 5, C 1, G 3, D 4, E 3, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Céline Dion is common knowledge, but the Horner-sneaking-the-demo payoff is good; keep as a deliberate easy recognition question.

### nineties_013 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: Где в 1991 году заработал первый в мире сайт?
- EN: Where did the world's first website go live in 1991?
- Options: **В ЦЕРН под Женевой | At CERN near Geneva** / В Массачусетском технологическом | At MIT / В Microsoft | At Microsoft / В NASA | At NASA
- Scores: F 4, C 3, G 3, D 4, E 2, A 4, P 3
- Verdict: CERN is a fair surprise vs MIT, but the MIT option is cut off in Russian ('Массачусетском технологическом' with no noun) and Microsoft is not a credible web birthplace.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Где в 1991 году заработал первый в мире сайт? / EN: Where did the world's first website go live in 1991?
  - Options: **В ЦЕРН под Женевой | At CERN near Geneva** / В MIT | At MIT / В NASA | At NASA / В Кремниевой долине | In Silicon Valley
  - Why better: Fixes the broken MIT option and replaces Microsoft with Silicon Valley, the instinctive wrong guess.

### nineties_004 (multipleChoice, easy): 67 NEEDS IMPROVEMENT, KEEP

- RU: Какой чемпион мира по шахматам в 1997 году проиграл матч компьютеру IBM Deep Blue?
- EN: Which world chess champion lost a match to IBM's Deep Blue in 1997?
- Options: **Гарри Каспаров | Garry Kasparov** / Анатолий Карпов | Anatoly Karpov / Владимир Крамник | Vladimir Kramnik / Вишванатан Ананд | Viswanathan Anand
- Scores: F 5, C 1, G 4, D 4, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Kasparov is a gimme for a Russian family, but the 'won the year before, IBM refused a rematch' payoff is good; keep as easy recognition.

### nineties_027 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Чья песня звучала в рекламе запуска Windows 95?
- EN: Whose song launched Windows 95 in the adverts?
- Options: **«Роллинг Стоунз» | The Rolling Stones** / «Куин» | Queen / U2 | U2 / «Битлз» | The Beatles
- Scores: F 4, C 3, G 2, D 4, E 3, A 5, P 4
- Verdict: Start Me Up for the Start button is a great pun, but players do not know which song was used, so it is a band guess; give them the song title and it becomes recognition plus an 'aha'.
- Rewrite (72 GOOD): RU: Рекламу Windows 95 и кнопки «Пуск» озвучила песня «Start Me Up». Чья она? / EN: Windows 95 and its Start button were launched to the song Start Me Up. Whose song is it?
  - Options: **«Роллинг Стоунз» | The Rolling Stones** / «Куин» | Queen / U2 | U2 / «Битлз» | The Beatles
  - Why better: Players can now hum and reason their way to the band, and the Start/Start pun lands in the question itself.

### nineties_034 (multipleChoice, medium): 71 GOOD, KEEP

- RU: В какой стране в 1999 году появилось первое реалити-шоу «Большой брат»?
- EN: In which country did the first Big Brother reality show air, in 1999?
- Options: **В Нидерландах | The Netherlands** / В Великобритании | The UK / В США | The USA / В Швеции | Sweden
- Scores: F 4, C 4, G 2, D 5, E 3, A 5, P 3
- Verdict: Most will say UK or USA, so the Netherlands is a fair surprise with excellent distractors; the Orwell fact is well known and a weak payoff.

### nineties_019 (multipleChoice, medium): 85 EXCELLENT, KEEP

- RU: Какая организация в 1999 году запретила приносить на работу игрушку Furby?
- EN: Which organisation banned Furby toys from its premises in 1999?
- Options: **Агентство нацбезопасности США | The US National Security Agency** / НАСА | NASA / Ватикан | The Vatican / Британский музей | The British Museum
- Scores: F 4, C 5, G 3, D 3, E 5, A 5, P 5
- Verdict: The NSA banning a Furby is hilarious and the payoff (it could not record anything) is perfect; only the correct option's length gives it away.
- Rewrite (87 EXCELLENT): RU: Какая организация в 1999 году запретила приносить на работу игрушку Furby? / EN: Which organisation banned Furby toys from its premises in 1999?
  - Options: **АНБ США | The NSA** / Ватикан | The Vatican / НАСА | NASA / Банк Англии | The Bank of England
  - Why better: Equal-length options keep the absurd answer from being spotted by its size.

### nineties_024 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, KEEP

- RU: Какой порошковый напиток рекламировали словами «Просто добавь воды!»?
- EN: Which powdered drink was sold with the slogan 'Just add water!'?
- Options: **«Инвайт» | Invite** / «Юпи» | Yupi / «Зуко» | Zuko / «Танг» | Tang
- Scores: F 5, C 2, G 2, D 5, E 4, A 5, P 3
- Verdict: Pure nostalgia recognition: anyone who grew up in the 90s shouts 'Invite!', and Yupi and Zuko are perfect traps; younger players may struggle. The Africa-suitcase fact is a nice payoff. Keep only if re-tagged as easy recognition.

### nineties_035 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Что называли «ножками Буша»?
- EN: What were 'Bush legs' in Russia?
- Options: **Американские куриные окорочка | American frozen chicken legs** / Американские ковбойские сапоги | American cowboy boots / Танец из американского клипа | A dance from a US music video / Узкие американские джинсы | Skinny American jeans
- Scores: F 5, C 2, G 4, D 3, E 5, A 5, P 4
- Verdict: Every Russian adult knows the phrase and it gets a laugh, though it is mostly recognition; the 1990 trade-deal fact explains the name nicely.

### nineties_042 (closestValue, hard): 79 GOOD, KEEP

- RU: Сколько посетителей обслужил первый московский «Макдоналдс» в день открытия, 31 января 1990-го?
- EN: How many customers did Moscow's first McDonald's serve on its opening day, 31 January 1990?
- Answer: **30000** people, range 0 to 100000, tolerance 9000
- Scores: F 5, C 4, G 3, D 4, E 4, A 4, P 4
- Verdict: The legendary queue is familiar and the number is estimable from it; a great reveal. The script links it to brands_050: if that question asks the same opening-day figure, keep only one.

### nineties_049 (closestValue, hard): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой юбилей Москва пышно отмечала в 1997 году?
- EN: Which anniversary did Moscow celebrate in grand style in 1997?
- Answer: **850** years, range 100 to 1500, tolerance 30
- Scores: F 5, C 2, G 4, D 3, E 2, A 5, P 3
- Verdict: Older players remember the 850th celebrations and can count from 1147, so it is close to recognition, and a tolerance of 30 makes it easy for a hard question; the 'Come to me, brother' line is the best part and sits only in the fact.
- Rewrite (72 GOOD): RU: «Приди ко мне, брате, в Москов» — первое упоминание Москвы. Какой юбилей она отмечала в 1997-м? / EN: 'Come to me, brother, in Moscow' is the city's first mention. Which anniversary did Moscow mark in 1997?
  - Answer 850, range 100 to 1500, tolerance 10
  - Why better: The chronicle quote turns a date sum into a story, and the tighter tolerance means remembering 1147 actually pays.

## ru_history

### ru_history_005 (multipleChoice, easy): 67 NEEDS IMPROVEMENT, KEEP

- RU: Как называлось войско Ивана Грозного, возившее у седла собачью голову и метлу?
- EN: What was Ivan the Terrible's guard called, riders with a dog's head and a broom at the saddle?
- Options: Стрельцы | Streltsy / Гвардейцы | Guardsmen / **Опричники | Oprichniki** / Мушкетёры | Musketeers
- Scores: F 5, C 2, G 4, D 3, E 2, A 4, P 4
- Verdict: Oprichniki is a school-level answer and the musketeers are absurd, but the 'gnaw out and sweep away treason' payoff is vivid. Keep as easy recognition.

### ru_history_001 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Что Пётр I обложил особым налогом, чтобы подданные выглядели по-европейски?
- EN: What did Peter the Great tax to make Russians look more European?
- Options: Лапти | Bast shoes / Самовары | Samovars / **Бороды | Beards** / Балалайки | Balalaikas
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 4
- Verdict: The beard tax is well known but fun, the balalaika and samovar options are playful, and the copper beard token is a good payoff.

### ru_history_007 (multipleChoice, easy): 60 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой царь отменил крепостное право в России?
- EN: Which tsar abolished serfdom in Russia?
- Options: Николай I | Nicholas I / Александр I | Alexander I / **Александр II | Alexander II** / Екатерина II | Catherine II
- Scores: F 5, C 1, G 3, D 4, E 1, A 5, P 4. Flags: SCHOOL_EXAM
- Verdict: A school-test question; the genuinely surprising angle (Russia freed serfs before the US freed slaves) is buried in the fact.
- Rewrite (71 GOOD): RU: Какой царь отменил крепостное право за два года до освобождения рабов в США? / EN: Which tsar abolished serfdom two years before Lincoln freed the slaves in the US?
  - Options: **Александр II | Alexander II** / Николай I | Nicholas I / Александр I | Alexander I / Екатерина II | Catherine II
  - Why better: The US comparison adds a 'wait, earlier than America?' moment to a school fact.

### ru_history_019 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Откуда пошла история о «потёмкинских деревнях» — декорациях для Екатерины II?
- EN: Where did the story of 'Potemkin villages', fake sets for Catherine the Great, come from?
- Options: Из мемуаров самой Екатерины | Catherine's own memoirs / Из донесения самого Потёмкина | Potemkin's own report / **Из очерков саксонского дипломата | A Saxon diplomat's writings** / Из комедии Гоголя | A comedy by Gogol
- Scores: F 4, C 4, G 2, D 3, E 3, A 4, P 4
- Verdict: A good myth-buster, but 'a comedy by Gogol' is a throwaway and the options are long and uneven.
- Rewrite (71 GOOD): RU: Кто пустил байку о «потёмкинских деревнях» для Екатерины II? / EN: Who spread the tale of 'Potemkin villages' built to fool Catherine the Great?
  - Options: **Саксонский дипломат | A Saxon diplomat** / Сама Екатерина | Catherine herself / Сам Потёмкин | Potemkin himself / Английская газета | A British newspaper
  - Why better: Shorter, balanced options; the reveal that a foreigner who was not even there invented it lands cleanly.

### ru_history_025 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: Как в 1480 году закончилось ордынское иго?
- EN: How did the Horde's rule over Russia end in 1480?
- Options: **Войска постояли на реке и разошлись | The armies faced off and left** / Куликовской битвой | With the Battle of Kulikovo / Взятием Казани | With the capture of Kazan / Мирным договором с Ордой | A peace treaty with the Horde
- Scores: F 5, C 3, G 3, D 3, E 2, A 4, P 3
- Verdict: Kulikovo is a great trap, but the correct option is a descriptive sentence among named events, which telegraphs it, and the grammar does not match the question.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Как в 1480 году закончилось ордынское иго? / EN: How did the Horde's rule over Russia end in 1480?
  - Options: **Стоянием на реке | A stand-off at a river** / Куликовской битвой | The Battle of Kulikovo / Взятием Казани | The capture of Kazan / Мирным договором | A peace treaty
  - Why better: Parallel noun-phrase options remove the tell, so players must actually remember the Ugra.

### ru_history_017 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: За кого выдавал себя Емельян Пугачёв?
- EN: Who did Yemelyan Pugachev claim to be?
- Options: За Ивана VI | Ivan VI / **За Петра III | Peter III** / За Павла I | Paul I / За царевича Дмитрия | Tsarevich Dmitry
- Scores: F 5, C 2, G 3, D 4, E 2, A 5, P 4
- Verdict: Peter III is a school answer from 'The Captain's Daughter'; the fun fact (about forty pretenders used his name) is hidden.
- Rewrite (74 GOOD): RU: Пугачёв — лишь один из сорока самозванцев под этим именем. За кого он себя выдавал? / EN: Pugachev was just one of about forty pretenders using this name. Who did he claim to be?
  - Options: **За Петра III | Peter III** / За Ивана VI | Ivan VI / За Павла I | Paul I / За царевича Дмитрия | Tsarevich Dmitry
  - Why better: 'Forty pretenders' is a surprising hook and makes Tsarevich Dmitry (another favourite mask) an even better trap.

### ru_history_033 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Почему Москву называют «белокаменной»?
- EN: Why is Moscow called 'white-stone Moscow'?
- Options: Дома белили известью к праздникам | Houses were whitewashed for feasts / Город стоит на меловых горах | The city stands on chalk hills / Снег лежит там полгода | Snow lies there half the year / **Кремль когда-то был из белого камня | The Kremlin was once white stone**
- Scores: F 5, C 3, G 3, D 3, E 2, A 4, P 4
- Verdict: Good plausible distractors, but the correct option is the longest and most specific, and the question gives little to argue about.
- Rewrite (71 GOOD): RU: Откуда у Москвы прозвище «белокаменная»? / EN: Why is Moscow nicknamed 'white-stone'?
  - Options: **От белого каменного Кремля | Its white stone Kremlin** / От побелённых домов | Its whitewashed houses / От меловых холмов | Its chalk hills / От долгих снежных зим | Its long snowy winters
  - Why better: Balanced options mean players must weigh four sensible theories, and 'the red Kremlin was once white' lands as a surprise.

### ru_history_038 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Под каким именем Пётр I ездил по Европе с Великим посольством?
- EN: Under what name did Peter the Great travel Europe with his Grand Embassy?
- Options: Граф Северный | Count of the North / **Пётр Михайлов | Pyotr Mikhailov** / Иван Иванов | Ivan Ivanov / Князь Московский | Prince of Moscow
- Scores: F 4, C 3, G 2, D 4, E 3, A 5, P 4
- Verdict: 'Count of the North' (Paul I's alias) is a smart trap, but 'Ivan Ivanov' is a joke option; the shipyard carpenter fact is a good payoff.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Под каким именем Пётр I ездил по Европе с Великим посольством? / EN: Under what name did Peter the Great travel Europe with his Grand Embassy?
  - Options: **Пётр Михайлов | Pyotr Mikhailov** / Пётр Алексеев | Pyotr Alekseyev / Граф Северный | Count of the North / Князь Московский | Prince of Moscow
  - Why better: 'Pyotr Alekseyev' (his real patronymic) is a far more tempting trap than 'Ivan Ivanov'.

### ru_history_043 (closestValue, hard): 72 GOOD, KEEP

- RU: Сколько дней Наполеон пробыл в занятой Москве в 1812 году?
- EN: How many days did Napoleon stay in occupied Moscow in 1812?
- Answer: **36** days, range 0 to 150, tolerance 8
- Scores: F 5, C 3, G 3, D 4, E 3, A 5, P 3
- Verdict: Everyone knows Napoleon took Moscow and left; the number is estimable and the range fair, with a decent 'waited for a peace offer' payoff.

### ru_history_049 (closestValue, hard): 58 NEEDS IMPROVEMENT, REPLACE

- RU: Сколько лет, по традиционному счёту, длилось ордынское иго на Руси?
- EN: By the traditional count, how many years did the Horde's yoke over Rus last?
- Answer: **240** years, range 0 to 600, tolerance 35
- Scores: F 5, C 1, G 4, D 3, E 1, A 5, P 2. Flags: SCHOOL_EXAM
- Verdict: Subtracting 1240 from 1480 is a school arithmetic exercise with a bare-number reveal, and the Ugra is already covered by ru_history_025.

## ru_politics

### ru_politics_005 (multipleChoice, easy): 62 NEEDS IMPROVEMENT, KEEP

- RU: Кто возглавил первое советское правительство — Совнарком — в 1917 году?
- EN: Who headed the first Soviet government, the Council of People's Commissars, in 1917?
- Options: **Владимир Ленин | Vladimir Lenin** / Лев Троцкий | Leon Trotsky / Иосиф Сталин | Joseph Stalin / Александр Керенский | Alexander Kerensky
- Scores: F 5, C 1, G 4, D 3, E 1, A 5, P 4. Flags: SCHOOL_EXAM, OBVIOUS_ANSWER
- Verdict: Lenin is a free answer; the 'minister sounded too bourgeois' fact is a nice payoff. Keep only as a deliberate easy recognition question.

### ru_politics_007 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Каким соглашением в декабре 1991 года было объявлено, что СССР прекращает существование?
- EN: Which December 1991 agreement declared that the USSR had ceased to exist?
- Options: **Беловежским | The Belovezha Accords** / Ялтинским | The Yalta Agreement / Хельсинкским актом | The Helsinki Accords / Брестским миром | The Treaty of Brest-Litovsk
- Scores: F 5, C 1, G 4, D 4, E 2, A 5, P 3
- Verdict: Belovezha is well known to this family; credible distractors and a hunting-lodge payoff. Keep as easy recognition.

### ru_politics_001 (multipleChoice, easy): 61 NEEDS IMPROVEMENT, KEEP

- RU: Кто стал первым президентом России?
- EN: Who was the first President of Russia?
- Options: **Борис Ельцин | Boris Yeltsin** / Михаил Горбачёв | Mikhail Gorbachev / Александр Руцкой | Alexander Rutskoy / Руслан Хасбулатов | Ruslan Khasbulatov
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 2. Flags: OBVIOUS_ANSWER
- Verdict: Yeltsin is a gimme (Gorbachev is a decent trap) and the 57% fact is dry; keep only as a deliberate easy warm-up.

### ru_politics_022 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, KEEP

- RU: Какое из этих слов НЕ было лозунгом Горбачёва?
- EN: Which of these was NOT one of Gorbachev's slogans?
- Options: **Оттепель | Thaw** / Перестройка | Perestroika / Гласность | Glasnost / Ускорение | Acceleration
- Scores: F 5, C 2, G 4, D 4, E 2, A 4, P 3
- Verdict: A satisfying recognition question: Thaw vs Perestroika slogans is fair reasoning and the Ehrenburg origin is a small payoff. Keep only if re-tagged as easy recognition.

### ru_politics_027 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Чья настоящая фамилия была Скрябин?
- EN: Whose real surname was Skryabin?
- Options: **Вячеслава Молотова | Vyacheslav Molotov** / Сергея Кирова | Sergei Kirov / Льва Троцкого | Leon Trotsky / Лазаря Кагановича | Lazar Kaganovich
- Scores: F 4, C 4, G 2, D 4, E 3, A 5, P 3
- Verdict: Molotov born Skryabin is surprising, but players have little to reason from; the composer link is the hook to use.
- Rewrite (72 GOOD): RU: Кто из советских вождей родился Скрябиным, как знаменитый композитор? / EN: Which Soviet leader was born Skryabin, like the famous composer?
  - Options: **Вячеслав Молотов | Vyacheslav Molotov** / Сергей Киров | Sergei Kirov / Лев Троцкий | Leon Trotsky / Лазарь Каганович | Lazar Kaganovich
  - Why better: The composer link makes the name familiar and turns a surname lookup into a funny contrast.

### ru_politics_023 (multipleChoice, medium): 60 NEEDS IMPROVEMENT, REPLACE

- RU: Сколько конституций было у Советского Союза?
- EN: How many constitutions did the Soviet Union have?
- Options: **Три | Three** / Одна | One / Две | Two / Пять | Five
- Scores: F 4, C 2, G 3, D 3, E 2, A 5, P 3. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: Counting constitutions is a dry exam question with a list-of-years payoff, and the RSFSR 1918 nuance invites 'it depends' arguments.

### ru_politics_033 (multipleChoice, medium): 73 GOOD, KEEP

- RU: За что в 1964 году судили поэта Иосифа Бродского?
- EN: What was the poet Joseph Brodsky convicted of in 1964?
- Options: **За тунеядство | Social parasitism** / За спекуляцию | Black market trading / За хулиганство | Hooliganism / За шпионаж | Spying
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 5
- Verdict: Familiar to the family, 'social parasitism' surprises the young, and Brodsky's 'from God' reply is a powerful payoff.

### ru_politics_036 (multipleChoice, medium): 79 GOOD, KEEP

- RU: По легенде, восставшие в 1825 году солдаты кричали «Константин и Конституция!». Кем они считали Конституцию?
- EN: Legend says the rebel soldiers of 1825 cried 'Constantine and Constitution!'. Who did they think Constitution was?
- Options: **Женой Константина | Constantine's wife** / Его лошадью | His horse / Его генералом | His general / Его дочерью | His daughter
- Scores: F 4, C 4, G 4, D 3, E 5, A 3, P 4. Flags: TOO_WORDY
- Verdict: The 'Constitution was Constantine's wife' legend is hilarious and guessable, but the question is over 100 characters.
- Rewrite (81 GOOD): RU: В 1825 году солдаты кричали «Константин и Конституция!». Кем, по легенде, они считали Конституцию? / EN: In 1825 soldiers shouted 'Constantine and Constitution!'. Who, legend says, did they think Constitution was?
  - Options: **Женой Константина | Constantine's wife** / Его лошадью | His horse / Его генералом | His general / Его дочерью | His daughter
  - Why better: Same joke, shorter setup, so it reads in time.

### ru_politics_041 (closestValue, hard): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году столицу перенесли из Петрограда в Москву?
- EN: In which year was the capital moved from Petrograd to Moscow?
- Answer: **1918** year, range 1850 to 1960, tolerance 8
- Scores: F 5, C 2, G 4, D 3, E 2, A 5, P 3
- Verdict: 1918 is close to common knowledge and the tolerance of 8 makes it easy; the 'moved almost in secret, fearing the Germans' story is the better hook.
- Rewrite (74 GOOD): RU: Боясь немцев, правительство тайком переехало из Петрограда в Москву. В каком году? / EN: Fearing the Germans, the government quietly moved from Petrograd to Moscow. In which year?
  - Answer 1918, range 1850 to 1960, tolerance 4
  - Why better: The secret flight is a better story, and the tighter tolerance makes knowing the year actually count.

### ru_politics_049 (closestValue, hard): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько миллионов членов было в КПСС в конце 1980-х?
- EN: How many million members did the Soviet Communist Party have in the late 1980s?
- Answer: **19** million, range 1 to 60, tolerance 7
- Scores: F 4, C 3, G 3, D 3, E 2, A 5, P 3
- Verdict: An estimable number for those who lived it, but the framing is dry and the tolerance of 7 on a 1-60 range is generous.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Без партбилета карьера не складывалась. Сколько миллионов членов было в КПСС в конце 1980-х? / EN: Without a party card careers stalled. How many million members did the Soviet Communist Party have in the late 1980s?
  - Answer 19, range 0 to 60, tolerance 4
  - Why better: The 'party card' hook prompts memories and arguments, and a tighter tolerance rewards the ones who remember.

## rulers

### rulers_015 (multipleChoice, easy): 61 NEEDS IMPROVEMENT, REPLACE

- RU: На что баварский король Людвиг II потратил своё состояние?
- EN: What did King Ludwig II of Bavaria spend his fortune on?
- Options: На военный флот | A navy / На зоопарк | A zoo / На железные дороги | Railways / **На сказочные замки | Fairy tale castles**
- Scores: F 3, C 2, G 4, D 3, E 2, A 5, P 3. Flags: REPETITIVE, OBVIOUS_ANSWER
- Verdict: Repeats landmarks_014 (Neuschwanstein, opened to tourists weeks after his death) almost word for word, and 'fairy tale castles' is obvious to anyone who just saw that question.

### rulers_001 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: Как прозвали французского короля Людовика XIV?
- EN: What was Louis XIV of France nicknamed?
- Options: **Король-солнце | The Sun King** / Король-орёл | The Eagle King / Король-луна | The Moon King / Король-лев | The Lion King
- Scores: F 5, C 1, G 3, D 3, E 3, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: The Sun King is common knowledge (the Lion King option gets a smile); the ballet origin is a good payoff. Keep as easy recognition.

### rulers_014 (multipleChoice, easy): 72 GOOD, KEEP

- RU: В память о ком император Шах-Джахан построил Тадж-Махал?
- EN: In memory of whom did the emperor Shah Jahan build the Taj Mahal?
- Options: О матери | His mother / Об отце | His father / **О жене | His wife** / О любимом слоне | His favourite elephant
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 4
- Verdict: A known story with a fun elephant option, and the 14th child plus 22 years of building is a strong payoff.

### rulers_021 (multipleChoice, medium): 80 GOOD, KEEP

- RU: Что случилось, когда правитель Мали Манса Муса проехал через Каир?
- EN: What happened when Mansa Musa, ruler of Mali, passed through Cairo?
- Options: **Золото в Каире подешевело | Gold lost value in Cairo** / В городе кончилась вода | The city ran out of water / Город сдался без боя | The city surrendered / Его не пустили в город | He was refused entry
- Scores: F 2, C 5, G 4, D 4, E 4, A 5, P 4. Flags: REPETITIVE
- Verdict: Gold getting cheaper because one man spent too much is a wonderful 'wait, seriously?', and it is guessable from Mansa Musa's wealth. It repeats medieval_023 (and money_027): keep this version, drop the other.

### rulers_029 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: Каким ремеслом увлекался Людовик XVI?
- EN: Which craft was Louis XVI's hobby?
- Options: Пчеловодством | Beekeeping / Гончарным делом | Pottery / Выпечкой хлеба | Baking bread / **Слесарным делом | Making locks**
- Scores: F 3, C 4, G 2, D 4, E 3, A 5, P 3
- Verdict: A locksmith king is a fun irony, but the options are mixed crafts and the payoff ('he loved to make locks') just restates the answer.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Что Людовик XVI любил мастерить в своей мастерской в Версале? / EN: What did Louis XVI love to make in his own workshop at Versailles?
  - Options: **Дверные замки | Door locks** / Скрипки | Violins / Парики | Wigs / Музыкальные шкатулки | Music boxes
  - Why better: Concrete objects make the picture of a king at a workbench vivid, so the locks answer gets a bigger smile.

### rulers_017 (multipleChoice, medium): 74 GOOD, KEEP

- RU: Что означает прозвище римского императора Калигулы?
- EN: What does the Roman emperor Caligula's nickname mean?
- Options: «Волчонок» | Little wolf / «Орлёнок» | Little eagle / **«Сапожок» | Little boot** / «Львёнок» | Little lion
- Scores: F 4, C 4, G 2, D 4, E 4, A 5, P 4
- Verdict: 'Little boot' for a tyrant is a delightful contrast, the animal-cub distractors are credible, and the toddler-in-uniform payoff is charming.

### rulers_035 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Какое насекомое Наполеон выбрал символом своей империи?
- EN: Which insect did Napoleon choose as a symbol of his empire?
- Options: Муравья | The ant / Бабочку | The butterfly / **Пчелу | The bee** / Стрекозу | The dragonfly
- Scores: F 4, C 4, G 2, D 4, E 3, A 5, P 4
- Verdict: Bees are a surprising choice for Napoleon, the other insects are credible, and the Childeric tomb link is a satisfying explanation.

### rulers_031 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Как провёл последние годы Шах-Джахан, построивший Тадж-Махал?
- EN: How did Shah Jahan, who built the Taj Mahal, spend his last years?
- Options: В путешествиях по Европе | Travelling in Europe / Монахом в горах | As a monk in the mountains / В изгнании в Персии | In exile in Persia / **Узником собственного сына | As his own son's prisoner**
- Scores: F 3, C 4, G 3, D 3, E 4, A 5, P 5
- Verdict: A tragic twist (the Taj Mahal builder imprisoned by his own son, gazing at it) gets a real reaction; the correct option is slightly the longest.

### rulers_041 (closestValue, hard): 73 GOOD, KEEP

- RU: Сколько внуков было у королевы Виктории?
- EN: How many grandchildren did Queen Victoria have?
- Answer: **42** grandchildren, range 10 to 120, tolerance 10
- Scores: F 4, C 4, G 3, D 3, E 3, A 5, P 4
- Verdict: Players know she had many children and can reason up to about 40, and 'the grandmother of Europe' payoff is great.

### rulers_050 (closestValue, hard): 40 REJECT, REPLACE

- RU: Каким по счёту сыном Рамсеса II был фараон Мернептах, унаследовавший его трон?
- EN: Merneptah inherited Ramesses II's throne. What number son was he?
- Answer: **13** , range 1 to 60, tolerance 7
- Scores: F 1, C 3, G 1, D 2, E 1, A 4, P 3. Flags: OBSCURE_TRIVIA, NO_DEDUCTION
- Verdict: Merneptah is unknown and his birth order cannot be estimated; the fun part (Ramesses outlived twelve sons) needs another framing. The unit field is also empty (' | ').

## conspiracy

### conspiracy_012 (multipleChoice, easy): 78 GOOD, KEEP

- RU: «Эффект Манделы» назван так, потому что многие «помнили», будто Нельсон Мандела…
- EN: The 'Mandela effect' got its name because many people 'remembered' that Nelson Mandela…
- Options: был американским пастором | was an American pastor / жил в изгнании в Париже | lived in exile in Paris / никогда не сидел в тюрьме | was never in prison / **умер в тюрьме в 1980-х | died in prison in the 1980s**
- Scores: F 4, C 4, G 4, D 3, E 4, A 4, P 4
- Verdict: The Mandela effect is well known and the 'died in prison' false memory is a satisfying reveal, with a cheeky 'never in prison' option.

### conspiracy_003 (multipleChoice, easy): 79 GOOD, KEEP

- RU: Для чего на самом деле в 1955 году построили «Зону 51»?
- EN: What was Area 51 actually built for in 1955?
- Options: Прятать летающую тарелку | To hide a flying saucer / Готовить астронавтов | To train astronauts / **Испытывать самолёт-шпион U-2 | To test the U-2 spy plane** / Укрыть президента от войны | To shelter the president
- Scores: F 5, C 3, G 3, D 4, E 4, A 5, P 5
- Verdict: The flying-saucer option tempts, the U-2 is a good reveal, and 'pilots reported its glinting wings as UFOs' is a perfect payoff.

### conspiracy_001 (multipleChoice, easy): 74 GOOD, KEEP

- RU: Какого режиссёра конспирологи обвиняют в том, что он снял высадку на Луну в павильоне?
- EN: Which film director do Moon hoax believers say staged the landing in a studio?
- Options: **Стэнли Кубрик | Stanley Kubrick** / Альфред Хичкок | Alfred Hitchcock / Стивен Спилберг | Steven Spielberg / Джордж Лукас | George Lucas
- Scores: F 4, C 2, G 4, D 4, E 4, A 5, P 4
- Verdict: Kubrick is the famous answer and the other directors are credible; the laser mirrors on the Moon fact is a strong rebuttal payoff.

### conspiracy_023 (multipleChoice, medium): 55 NEEDS IMPROVEMENT, REPLACE

- RU: Как критики прозвали пулю, которая, по официальной версии, ранила и Кеннеди, и губернатора Коннэлли?
- EN: What did critics nickname the bullet that, officially, hit both Kennedy and Governor Connally?
- Options: Серебряная пуля | The silver bullet / Золотая пуля | The golden bullet / **Волшебная пуля | The magic bullet** / Ледяная пуля | The ice bullet
- Scores: F 3, C 2, G 3, D 3, E 2, A 4, P 3. Flags: BORING_REVEAL
- Verdict: 'Magic bullet' is a name lookup with no surprise, the other metals are filler, and the ballistics explanation is technical.

### conspiracy_018 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Какой знаменитый учёный был уверен, что пьесы Шекспира написал граф Оксфорд?
- EN: Which famous scientist was convinced the Earl of Oxford wrote Shakespeare's plays?
- Options: Альберт Эйнштейн | Albert Einstein / **Зигмунд Фрейд | Sigmund Freud** / Чарлз Дарвин | Charles Darwin / Иван Павлов | Ivan Pavlov
- Scores: F 4, C 5, G 2, D 4, E 4, A 4, P 5
- Verdict: Freud as an Oxfordian is a genuine surprise, and the author literally named Looney is a perfect payoff (it lands best in English; the Russian fact explains it).

### conspiracy_020 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: Чьё существование отрицает «гипотеза фантомного времени» Хериберта Илига?
- EN: Whose existence does Heribert Illig's 'phantom time hypothesis' deny?
- Options: Юлия Цезаря | Julius Caesar / Чингисхана | Genghis Khan / Жанны д'Арк | Joan of Arc / **Карла Великого | Charlemagne**
- Scores: F 2, C 4, G 2, D 4, E 3, A 3, P 4. Flags: OBSCURE_TRIVIA
- Verdict: The theory is fun, but naming Heribert Illig hides it behind an unknown name, and Julius Caesar is not even medieval.
- Rewrite (77 GOOD): RU: По одной теории, 300 лет Средневековья выдуманы, а с ними и знаменитый правитель. Кто? / EN: One theory says 300 years of the Middle Ages were invented, along with a famous ruler. Who?
  - Options: **Карл Великий | Charlemagne** / Ричард Львиное Сердце | Richard the Lionheart / Чингисхан | Genghis Khan / Ярослав Мудрый | Yaroslav the Wise
  - Why better: Leading with the outrageous claim instead of an unknown theorist makes it a 'wait, seriously?', and four medieval rulers make players reason about dates.

### conspiracy_031 (multipleChoice, medium): 57 NEEDS IMPROVEMENT, IMPROVE

- RU: Через какие пояса, по словам конспирологов, экипажи «Аполлонов» не пролетели бы живыми из-за радиации?
- EN: Which belts, hoaxers say, would have killed Apollo crews with radiation?
- Options: Пояс Койпера | The Kuiper belt / Пояс астероидов | The asteroid belt / **Пояса Ван Аллена | The Van Allen belts** / Пояс Ориона | Orion's Belt
- Scores: F 3, C 2, G 3, D 4, E 2, A 3, P 4. Flags: TOO_WORDY
- Verdict: The Kuiper and asteroid belts are credible traps and Orion's Belt is a joke, but the question is over 100 characters and the answer is a technical name.
- Rewrite (59 NEEDS IMPROVEMENT): RU: Какие пояса, по словам конспирологов, «Аполлоны» не пролетели бы живыми из-за радиации? / EN: Which belts, say hoaxers, would have killed the Apollo crews with radiation?
  - Options: **Пояса Ван Аллена | The Van Allen belts** / Пояс Койпера | The Kuiper belt / Пояс астероидов | The asteroid belt / Пояс Ориона | Orion's Belt
  - Why better: Shorter and readable in time; the content is unchanged.

### conspiracy_030 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Физик Дэвид Граймс подсчитал, через сколько раскрылась бы подделка высадки на Луну. Его ответ?
- EN: Physicist David Grimes worked out how soon a faked Moon landing would leak. His answer?
- Options: Лет через 40 | In about 40 years / **Меньше чем через 4 года | In under 4 years** / Лет через 100 | In about 100 years / Никогда | Never
- Scores: F 3, C 4, G 3, D 3, E 4, A 4, P 4
- Verdict: A fun scientific twist on the moon hoax with an interesting payoff; 'under 4 years' is more specific than the round distractors.
- Rewrite (73 GOOD): RU: Физик Дэвид Граймс подсчитал, через сколько раскрылась бы подделка высадки на Луну. Его ответ? / EN: Physicist David Grimes worked out how soon a faked Moon landing would leak. His answer?
  - Options: **Примерно через 4 года | In about 4 years** / Примерно через 40 лет | In about 40 years / Примерно через 400 лет | In about 400 years / Не раскрылась бы никогда | It would never leak
  - Why better: Parallel 'about N' options remove the specificity tell.

### conspiracy_044 (closestValue, hard): 82 GOOD, KEEP

- RU: Плоскоземельцы купили лазерный гироскоп за 20 тысяч долларов. Сколько градусов в час он показал?
- EN: Flat-earthers bought a $20,000 laser gyroscope. What rotation, in degrees per hour, did it detect?
- Answer: **15** °/h, range 0 to 90, tolerance 7
- Scores: F 3, C 4, G 4, D 4, E 5, A 4, P 5
- Verdict: Flat-earthers' own gyroscope proving the Earth spins gets a big laugh, and 360/24 gives a satisfying reasoning path.

### conspiracy_042 (closestValue, hard): 41 REJECT, REPLACE

- RU: Сколько лет истории, по Хериберту Илигу, придумали и вписали в Средневековье?
- EN: How many years of history does Heribert Illig say were invented and slipped into the Middle Ages?
- Answer: **297** years, range 0 to 1000, tolerance 50
- Scores: F 1, C 3, G 1, D 2, E 2, A 3, P 3. Flags: OBSCURE_TRIVIA, NO_DEDUCTION, REPETITIVE
- Verdict: Same as the calibration anchor: an unknown theorist and a number nobody can estimate; it also repeats the phantom-time theory of conspiracy_020.

## mysteries

### mysteries_003 (multipleChoice, easy): 60 NEEDS IMPROVEMENT, IMPROVE

- RU: Куда направлена главная ось Стоунхенджа?
- EN: What is the main axis of Stonehenge lined up with?
- Options: **На восход в день летнего солнцестояния | Sunrise on the summer solstice** / На Полярную звезду | The North Star / На восход полной Луны в марте | The full Moon rising in March / На вершину Бен-Невиса | The peak of Ben Nevis
- Scores: F 4, C 2, G 4, D 2, E 2, A 4, P 3
- Verdict: Solstice is a known answer, and the correct option is by far the longest and most specific; the 'midwinter sunset may matter more' twist is the better story.
- Rewrite (71 GOOD): RU: Куда смотрит главная ось Стоунхенджа? / EN: What is the main axis of Stonehenge lined up with?
  - Options: **На летний рассвет | Midsummer sunrise** / На зимний рассвет | Midwinter sunrise / На Полярную звезду | The North Star / На восход Луны | Moonrise
  - Why better: Short balanced options with a summer-vs-winter trap make players think, and the reveal's sunset twist fits the trap.

### mysteries_012 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Чью могилу так и не нашли, хотя её ищут даже со спутников?
- EN: Whose grave has never been found, even with satellite searches?
- Options: **Чингисхана | Genghis Khan's** / Тутанхамона | Tutankhamun's / Наполеона | Napoleon's / Петра Первого | Peter the Great's
- Scores: F 5, C 3, G 3, D 3, E 3, A 5, P 4
- Verdict: A familiar mystery with a vivid payoff (horses trampling the grave), though Napoleon's and Peter's graves are well known, which makes it an easy elimination.

### mysteries_013 (multipleChoice, easy): 78 GOOD, KEEP

- RU: Где в 2012 году нашли останки английского короля Ричарда III?
- EN: Where were the remains of England's King Richard III found in 2012?
- Options: **Под парковкой | Under a car park** / В Темзе | In the River Thames / Под футбольным полем | Under a football pitch / В подвале Тауэра | In the Tower's cellar
- Scores: F 3, C 5, G 2, D 4, E 5, A 5, P 4
- Verdict: 'Under a car park' is a brilliant, absurd and true answer that gets a laugh; strong.

### mysteries_034 (multipleChoice, medium): 57 NEEDS IMPROVEMENT, REPLACE

- RU: Какое первое слово прочли в 2023 году в сгоревшем свитке из Геркуланума?
- EN: What was the first word read in 2023 from a charred scroll from Herculaneum?
- Options: **«Пурпур» | Purple** / «Любовь» | Love / «Цезарь» | Caesar / «Хлеб» | Bread
- Scores: F 2, C 4, G 1, D 3, E 3, A 4, P 4. Flags: NO_DEDUCTION
- Verdict: Reading a charred scroll with AI is a great story, but the first word is random, so the question is a coin toss; rebuild it around how the word was read.

### mysteries_027 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Как власти Страсбурга сначала «лечили» танцевальную чуму 1518 года?
- EN: How did the authorities of Strasbourg first try to cure the dancing plague of 1518?
- Options: **Наняли музыкантов для танцоров | Hired musicians to keep them dancing** / Штрафовали танцующих | Fined the dancers / Обливали людей холодной водой | Doused the dancers in cold water / Закрыли городские ворота | Locked the city gates
- Scores: F 3, C 5, G 2, D 4, E 5, A 4, P 5
- Verdict: Hiring musicians to cure a dancing plague is absurd and true; the reveal gets a laugh and the 'built a stage' detail is a great payoff.

### mysteries_038 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: Какое реальное бедствие часто называют прообразом гибели Атлантиды?
- EN: Which real disaster is often named as the inspiration for the sinking of Atlantis?
- Options: **Извержение вулкана на Санторини | The eruption on Santorini** / Гибель Помпей | The destruction of Pompeii / Извержение Кракатау | The eruption of Krakatoa / Лиссабонское землетрясение | The Lisbon earthquake
- Scores: F 4, C 3, G 3, D 4, E 2, A 5, P 3
- Verdict: A fair 'real Atlantis' question, but Pompeii and Lisbon are easy to rule out and the reveal is mild.
- Rewrite (76 GOOD): RU: Какое реальное бедствие часто называют прообразом гибели Атлантиды? / EN: Which real disaster is often named as the inspiration for the sinking of Atlantis?
  - Options: **Взрыв вулкана Санторини | The Santorini eruption** / Извержение Везувия | The eruption of Vesuvius / Извержение Этны | The eruption of Etna / Извержение Кракатау | The eruption of Krakatoa
  - Why better: Four volcanoes make players reason (Plato wrote before Vesuvius and Krakatoa erupted), a real deduction path.

### mysteries_035 (multipleChoice, medium): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: Что на самом деле издало загадочный подводный звук «Блуп», записанный в 1997 году?
- EN: What was really behind the mysterious underwater sound called the Bloop, recorded in 1997?
- Options: **Раскалывающиеся айсберги | Icebergs cracking** / Гигантский кальмар | A giant squid / Секретная подлодка | A secret submarine / Пробуждение Ктулху | Cthulhu waking up
- Scores: F 2, C 4, G 2, D 3, E 4, A 4, P 4
- Verdict: The Bloop has internet fame and the Cthulhu option is a fun laugh, but most players have never heard of it and the question is a bit long.
- Rewrite (66 NEEDS IMPROVEMENT): RU: В 1997 году датчики в океане записали загадочный звук «Блуп». Что его издало? / EN: In 1997 ocean sensors recorded a mysterious sound, the Bloop. What made it?
  - Options: **Ломающиеся айсберги | Icebergs breaking up** / Гигантский кальмар | A giant squid / Секретная подлодка | A secret submarine / Пробуждение Ктулху | Cthulhu waking up
  - Why better: Simpler phrasing and parallel options; the monster-vs-ice contrast stays.

### mysteries_018 (multipleChoice, medium): 76 GOOD, KEEP

- RU: Что говорят о Бермудском треугольнике страховщики «Ллойдс» и береговая охрана США?
- EN: What do Lloyd's insurers and the US Coast Guard say about the Bermuda Triangle?
- Options: **Он не опаснее других морей | It is no worse than other seas** / Страховка там вдвое дороже | Insurance there costs double / Там отказывают компасы | Compasses fail there / Суда обязаны его обходить | Ships must sail around it
- Scores: F 5, C 4, G 3, D 4, E 3, A 5, P 3
- Verdict: Everyone knows the Bermuda Triangle legend, and 'insurers say it is no worse than anywhere' is a satisfying myth-buster.

### mysteries_045 (closestValue, hard): 50 REJECT, REPLACE

- RU: Сколько страниц в рукописи Войнича?
- EN: How many pages does the Voynich manuscript have?
- Answer: **240** pages, range 0 to 1000, tolerance 70
- Scores: F 2, C 2, G 2, D 3, E 2, A 5, P 3. Flags: NO_DEDUCTION, BORING_REVEAL
- Verdict: The Voynich manuscript is half-known at best and its page count is an arbitrary number with no estimation path or payoff.

### mysteries_044 (closestValue, hard): 57 NEEDS IMPROVEMENT, REPLACE

- RU: Сколько бронзовых шестерёнок сохранилось в Антикитерском механизме — древнегреческом «компьютере»?
- EN: How many bronze gears survive in the Antikythera mechanism, the ancient Greek computer?
- Answer: **30** gears, range 0 to 200, tolerance 10
- Scores: F 3, C 3, G 2, D 3, E 2, A 4, P 4. Flags: NO_DEDUCTION
- Verdict: The Antikythera mechanism is fascinating, but the count of surviving gears is an arbitrary number; the 'computer of antiquity' angle deserves another question.

## crime

### crime_008 (multipleChoice, easy): 66 NEEDS IMPROVEMENT, KEEP

- RU: За что в итоге посадили гангстера Аль Капоне?
- EN: What was gangster Al Capone finally jailed for?
- Options: За подделку денег | Counterfeiting / За кражу картины | Stealing a painting / **За неуплату налогов | Tax evasion** / За ограбление банка | Bank robbery
- Scores: F 5, C 2, G 3, D 3, E 3, A 5, P 3
- Verdict: The tax-evasion answer is widely known but still satisfying; keep as easy recognition.

### crime_003 (multipleChoice, easy): 74 GOOD, KEEP

- RU: Откуда родом Эркюль Пуаро?
- EN: Where does Hercule Poirot come from?
- Options: Из Франции | France / Из Швейцарии | Switzerland / Из Нидерландов | The Netherlands / **Из Бельгии | Belgium**
- Scores: F 5, C 2, G 3, D 5, E 3, A 5, P 5
- Verdict: Belgium vs France is the perfect trap (Poirot's annoyance at being called French), and the NYT front-page obituary is a superb payoff.

### crime_010 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Сколько сравнительно честных способов отъёма денег знал Остап Бендер?
- EN: How many relatively honest ways of taking people's money did Ostap Bender know?
- Options: **Четыреста | Four hundred** / Сорок | Forty / Сто | One hundred / Тысячу | A thousand
- Scores: F 5, C 1, G 2, D 4, E 4, A 5, P 4
- Verdict: A beloved quote for this family that people will shout out; the drawn-lots fact is a good payoff. Keep as recognition.

### crime_037 (multipleChoice, medium): 56 NEEDS IMPROVEMENT, IMPROVE

- RU: Пилтдаунский человек, «недостающее звено» из Англии (1912), оказался подделкой. Из чего его собрали?
- EN: Piltdown Man, a 1912 'missing link' found in England, was a fake. What was it made of?
- Options: Череп гориллы и кости овцы | Gorilla skull, sheep bones / **Череп человека и челюсть орангутана | Human skull, orangutan jaw** / Кости медведя | Bear bones / Гипс и дерево | Plaster and wood
- Scores: F 2, C 3, G 3, D 3, E 2, A 4, P 3
- Verdict: Human plus ape is the natural guess for a 'missing link', so the answer is not surprising, and 'bear bones' is vague filler.
- Rewrite (60 NEEDS IMPROVEMENT): RU: Пилтдаунский человек (1912) оказался подделкой. Из чего его собрали? / EN: Piltdown Man (1912) turned out to be a fake. What was it made from?
  - Options: **Человек и орангутан | A human and an orangutan** / Горилла и овца | A gorilla and a sheep / Неандерталец и медведь | A Neanderthal and a bear / Гипс и дерево | Plaster and wood
  - Why better: Shorter question and parallel 'X and Y' options make each choice a credible forgery recipe.

### crime_026 (multipleChoice, medium): 79 GOOD, KEEP

- RU: Какой знаменитой фразы НЕТ ни в одном рассказе Конан Дойла о Холмсе?
- EN: Which famous line never appears in Conan Doyle's Holmes stories?
- Options: **«Элементарно, Ватсон!» | 'Elementary, my dear Watson'** / «Игра началась!» | 'The game is afoot' / «Вы были в Афганистане» | 'You have been in Afghanistan' / «Отбросьте всё невозможное…» | 'Eliminate the impossible...'
- Scores: F 5, C 4, G 3, D 4, E 4, A 4, P 4
- Verdict: Most players 'know' the line, especially from the Soviet Holmes films, so learning Doyle never wrote it is a real surprise; the real quotes are clever distractors.

### crime_033 (multipleChoice, medium): 76 GOOD, KEEP

- RU: Зачем голландец Хан ван Мегерен в 1945 году писал «Вермеера» на глазах у свидетелей?
- EN: Why did Dutchman Han van Meegeren paint a 'Vermeer' in front of witnesses in 1945?
- Options: Сдать экзамен в академию | To pass an art academy exam / **Доказать, что он фальсификатор | To prove he was a forger** / Для съёмок фильма | For a film shoot / По заказу королевы | On the Queen's orders
- Scores: F 2, C 5, G 3, D 4, E 4, A 4, P 5
- Verdict: A forger proving his guilt to escape a treason charge is a wonderful twist with a strong payoff.

### crime_031 (multipleChoice, medium): 59 NEEDS IMPROVEMENT, IMPROVE

- RU: Откуда бежал Анри Шарьер по прозвищу «Мотылёк» (Papillon)?
- EN: Where did Henri Charrière, 'Papillon', escape from?
- Options: С каторги в Австралии | A penal colony in Australia / С каторги на Сахалине | A penal colony on Sakhalin / Из тюрьмы на Корсике | A prison on Corsica / **С каторги во Французской Гвиане | A penal colony in French Guiana**
- Scores: F 3, C 2, G 3, D 4, E 2, A 4, P 4
- Verdict: Papillon is familiar to the older group, but the correct option is the longest and the 'French' name hints at French Guiana; Sakhalin is a token Russian option.
- Rewrite (63 NEEDS IMPROVEMENT): RU: Откуда бежал Анри Шарьер по прозвищу «Мотылёк» (Papillon)? / EN: Where did Henri Charrière, 'Papillon', escape from?
  - Options: **Из Французской Гвианы | French Guiana** / С Новой Каледонии | New Caledonia / С Сахалина | Sakhalin / С Корсики | Corsica
  - Why better: New Caledonia, another French penal colony, is a much stronger trap, and shorter options remove the length tell.

### crime_024 (multipleChoice, medium): 71 GOOD, KEEP

- RU: На чём якобы зарабатывал Чарльз Понци в 1920 году?
- EN: What did Charles Ponzi claim his 1920 profits came from?
- Options: На луковицах тюльпанов | Tulip bulbs / На акциях железных дорог | Railway shares / **На почтовых купонах | Postal reply coupons** / На золотых приисках | Gold mines
- Scores: F 4, C 4, G 2, D 4, E 3, A 5, P 4
- Verdict: Postal coupons are a surprising engine for the original Ponzi scheme, and the tulip option is a nice wink at money_013.

### crime_044 (closestValue, hard): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: На сколько лет осудили Берни Мейдоффа в 2009 году за крупнейшую известную финансовую пирамиду?
- EN: How many years in prison did Bernie Madoff get in 2009 for the largest known Ponzi scheme?
- Answer: **150** years, range 0 to 1000, tolerance 40
- Scores: F 3, C 4, G 2, D 3, E 4, A 4, P 3
- Verdict: 150 years is absurd and fun, but the range of 0-1000 gives little to reason from.
- Rewrite (71 GOOD): RU: Мейдофф построил крупнейшую финансовую пирамиду. Сколько лет тюрьмы дал ему суд в 2009-м? / EN: Madoff ran the biggest known Ponzi scheme. How many years in prison did the court give him in 2009?
  - Answer 150, range 0 to 500, tolerance 30
  - Why better: A tighter range and the 'biggest ever' hook push players toward absurdly long sentences and a satisfying reveal.

### crime_049 (closestValue, hard): 52 REJECT, REPLACE

- RU: Сколько золотых слитков унесли грабители со склада Brink's-Mat у Хитроу в 1983 году?
- EN: How many gold bars did robbers take from the Brink's-Mat warehouse at Heathrow in 1983?
- Answer: **6800** bars, range 0 to 20000, tolerance 2000
- Scores: F 1, C 4, G 1, D 2, E 3, A 4, P 4. Flags: NO_DEDUCTION, OBSCURE_TRIVIA, FACTUAL_RISK
- Verdict: The number of bars is unguessable without knowing bar weight; the fun part (they expected cash and found three tonnes of gold) needs a different question.
- Fact check: Not certain of the detail that the robbers expected 'about £1 million in Spanish pesetas'; my recollection is that they expected around £3 million in cash. Check before reusing the fact.
- Lead check (web): Wikipedia (the cited source) says they expected about £1 million in pesetas; other accounts say about £3 million in cash. Safer fact wording: "they expected cash, not gold".

## misconceptions

### misconceptions_004 (multipleChoice, easy): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Какую часть мозга человек на самом деле использует?
- EN: How much of the brain does a person really use?
- Options: 10 процентов | 10 percent / Около трети | About a third / Ровно половину | Exactly half / **Практически весь | Practically all of it**
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 2. Flags: OBVIOUS_ANSWER
- Verdict: The 10% myth is the whole point, but "на самом деле" tells everyone it is a myth, so "practically all" is a gimme; "exactly half" is a throwaway.
- Rewrite (77 GOOD): RU: Какую часть своего мозга использует человек? / EN: How much of their brain does a person use?
  - Options: Около 10% | About 10% / Около трети | About a third / Около половины | About half / **Почти весь | Nearly all of it**
  - Why better: Without "really" the famous 10% myth becomes a genuine trap instead of a signpost to the answer.

### misconceptions_008 (multipleChoice, easy): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Какого цвета кровь в венах человека?
- EN: What colour is the blood in human veins?
- Options: Синяя | Blue / **Тёмно-красная | Dark red** / Голубая | Light blue / Зеленоватая | Greenish
- Scores: F 5, C 2, G 4, D 2, E 3, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Well-known myth bust; "Синяя" and "Голубая" are the same answer twice and the only red option stands alone.
- Rewrite (72 GOOD): RU: Какого цвета кровь в венах человека? / EN: What colour is the blood in human veins?
  - Options: **Тёмно-красная | Dark red** / Тёмно-синяя | Dark blue / Фиолетовая | Purple / Зеленоватая | Greenish
  - Why better: Removes the duplicate blue pair and gives the trap the same "dark" wording, so the answer no longer stands alone.

### misconceptions_013 (multipleChoice, easy): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Как дикобраз защищается иглами?
- EN: How does a porcupine use its quills?
- Options: Стреляет ими во врага | It shoots them at enemies / **Они отрываются и застревают | They come loose and stick in** / Впрыскивает ими яд | It injects venom with them / Роет ими нору | It digs its burrow with them
- Scores: F 4, C 3, G 3, D 4, E 3, A 4, P 3
- Verdict: Good myth trap ("it shoots them"), but the correct option switches subject to "Они", which grammatically singles it out.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Как дикобраз защищается иглами? / EN: How does a porcupine use its quills?
  - Options: Стреляет ими во врага | It shoots them at enemies / **Оставляет их в обидчике | It leaves them stuck in the attacker** / Впрыскивает ими яд | It injects venom with them / Роет ими нору | It digs its burrow with them
  - Why better: All four options now share the same verb-first form, so only knowledge or the myth decides.

### misconceptions_024 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Почему старинные оконные стёкла часто толще внизу?
- EN: Why is old window glass often thicker at the bottom?
- Options: Стекло веками стекало вниз | The glass flowed down over centuries / **Их делали неровными | Panes were made uneven** / Низ укрепляли от ветра | The bottom was thickened for wind / Нарос иней и не сошёл | Frost built up and stayed
- Scores: F 3, C 4, G 3, D 4, E 3, A 4, P 4
- Verdict: Strong myth bust: "glass flows" is a believable trap and the plain truth is a nice anticlimax.

### misconceptions_019 (multipleChoice, medium): 83 GOOD, KEEP

- RU: Что показали слепые опыты про сахар и поведение детей?
- EN: What did blind trials find about sugar and children's behaviour?
- Options: Сахар делает их гиперактивными | Sugar makes them hyperactive / Так действует только шоколад | Only chocolate does that / Сахар их успокаивает | Sugar calms them down / **Разницы с пустышкой нет | No difference from a placebo**
- Scores: F 5, C 4, G 4, D 3, E 4, A 4, P 5
- Verdict: Everyone believes the sugar rush; the placebo result and the mothers who imagined wilder sons make a great reveal. "Only chocolate" is a little weak.

### misconceptions_035 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Чем в основном питаются ежи?
- EN: What do hedgehogs mostly eat?
- Options: Яблоками и грушами | Apples and pears / Грибами | Mushrooms / **Насекомыми, червями, слизнями | Insects, worms and slugs** / Лесными ягодами | Forest berries
- Scores: F 5, C 3, G 4, D 2, E 3, A 5, P 3
- Verdict: Cartoon hedgehogs with apples make a perfect trap for a Russian family, but the correct option is the only three-item list and much longer.
- Rewrite (76 GOOD): RU: Чем в основном питаются ежи? / EN: What do hedgehogs mostly eat?
  - Options: Яблоками и грушами | Apples and pears / Грибами и ягодами | Mushrooms and berries / **Жуками и червями | Beetles and worms** / Орехами и семенами | Nuts and seeds
  - Why better: Every option is now a two-item pair of similar length, so the apples stay a pure trap.

### misconceptions_036 (multipleChoice, medium): 79 GOOD, KEEP

- RU: Правда ли, что Санта-Клауса одела в красное «Кока-Кола»?
- EN: Did Coca-Cola dress Santa Claus in red?
- Options: Да, в рекламе 1931 года | Yes, in its 1931 ads / **Нет, красный Санта был раньше | No, red Santas came earlier** / Да, в телерекламе 1950-х | Yes, in 1950s TV ads / Нет, до 1990-х он был зелёным | No, he was green until the 1990s
- Scores: F 5, C 4, G 3, D 4, E 4, A 4, P 4
- Verdict: A famous "fact" that turns out false; the 1931 ad option is a strong trap and the reveal sparks discussion.

### misconceptions_020 (multipleChoice, medium): 78 GOOD, KEEP

- RU: За что Эйнштейн получил Нобелевскую премию?
- EN: What did Einstein win his Nobel Prize for?
- Options: За теорию относительности | The theory of relativity / **За объяснение фотоэффекта | Explaining the photoelectric effect** / За формулу E = mc² | The formula E = mc² / За атомную бомбу | The atomic bomb
- Scores: F 5, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: Relativity and E=mc² are perfect traps and the "too controversial" reason is a good payoff; the "similar to people_029" flag is only the Nobel formula, not a repeated fact.

### misconceptions_040 (closestValue, hard): 52 REJECT, IMPROVE

- RU: Цезарь появился на свет вовсе не кесаревым сечением. Сколько лет после его рождения прожила его мать?
- EN: Caesar was not born by C-section. How many years did his mother live after his birth?
- Answer: **46** years, range 0 to 150, tolerance 8
- Scores: F 4, C 3, G 2, D 2, E 2, A 2, P 3. Flags: TOO_WORDY, NO_DEDUCTION
- Verdict: Nice myth (Caesar not born by C-section), but the text is over 100 characters with a NOT framing and 46 years cannot be estimated on a 0-150 slider.
- Rewrite (61 NEEDS IMPROVEMENT): RU: Цезарь вряд ли родился кесаревым: его мать выжила. Сколько лет она прожила после родов? / EN: Caesar was hardly born by C-section: his mother survived. How many years did she live after his birth?
  - Answer 46, range 0 to 80, tolerance 8
  - Why better: Positive framing, shorter text and a tighter range let players reason from "she lived to see him famous".

### misconceptions_044 (closestValue, hard): 58 NEEDS IMPROVEMENT, IMPROVE

- RU: Алмазы — не сжатый уголь. На какой глубине в км образовалось большинство из них?
- EN: Diamonds are not squeezed coal. At what depth in km did most of them form?
- Answer: **200** km, range 0 to 1000, tolerance 70
- Scores: F 4, C 3, G 2, D 3, E 2, A 3, P 4. Flags: NO_DEDUCTION
- Verdict: The coal myth and "older than land plants" are good, but a depth in km from 0 to 1000 has no everyday anchor to estimate from.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Алмазы — не сжатый уголь. Марианская впадина — 11 км. На какой глубине в км рождаются алмазы? / EN: Diamonds are not squeezed coal. The Mariana Trench is 11 km deep. At what depth in km do most diamonds form?
  - Answer 200, range 0 to 500, tolerance 60
  - Why better: A familiar depth gives players a yardstick, so the surprise is "far deeper than anything I know".

## body

### body_007 (multipleChoice, easy): 66 NEEDS IMPROVEMENT, KEEP

- RU: Что мы на самом деле задеваем, когда ударяемся локтем и «бьёт током»?
- EN: What are you really hitting when you bang your elbow and feel an electric jolt?
- Options: **Нерв | A nerve** / Кость | A bone / Сухожилие | A tendon / Вену | A vein
- Scores: F 5, C 2, G 4, D 3, E 3, A 4, P 2
- Verdict: Deliberate easy recognition: most people sense it is a nerve; the fact adds little. Fine as a warm-up.

### body_001 (multipleChoice, easy): 61 NEEDS IMPROVEMENT, KEEP

- RU: Сколько зубов у взрослого человека, если выросли все до одного?
- EN: How many teeth does an adult have once every one has grown in?
- Options: 20 | 20 / 28 | 28 / **32 | 32** / 36 | 36
- Scores: F 5, C 1, G 3, D 4, E 2, A 5, P 3. Flags: SCHOOL_EXAM, OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition question; 32 is widely known, so no surprise. Acceptable warm-up only.

### body_013 (multipleChoice, easy): 63 NEEDS IMPROVEMENT, KEEP

- RU: Сколько костей в скелете взрослого человека?
- EN: How many bones are in an adult human skeleton?
- Options: 106 | 106 / **206 | 206** / 306 | 306 / 406 | 406
- Scores: F 5, C 2, G 3, D 2, E 2, A 5, P 4. Flags: SCHOOL_EXAM
- Verdict: Deliberate easy recognition: 206 is school knowledge and the x06 pattern helps; the hands-and-feet fact is a decent payoff.

### body_024 (multipleChoice, medium): 74 GOOD, KEEP

- RU: Одинаковые ли отпечатки пальцев у однояйцевых близнецов?
- EN: Do identical twins have the same fingerprints?
- Options: Да, точь-в-точь | Yes, exactly the same / **Нет, разные | No, they differ** / Совпадают только большие пальцы | Only the thumbs match / Да, но только до 10 лет | Yes, but only until age 10
- Scores: F 5, C 3, G 4, D 3, E 3, A 5, P 3
- Verdict: Relatable question with a believable trap ("identical twins, identical prints"); solid if not stunning.

### body_035 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Какое лёгкое у человека больше?
- EN: Which of your lungs is bigger?
- Options: **Правое | The right one** / Левое | The left one / Они одинаковые | They are the same / Зависит от того, правша ли ты | Depends if you are right handed
- Scores: F 5, C 3, G 4, D 3, E 3, A 4, P 3
- Verdict: Answerable by reasoning about where the heart sits; mild surprise, clean options.

### body_021 (multipleChoice, medium): 84 GOOD, KEEP

- RU: В какое время суток человек выше всего ростом?
- EN: At what time of day are you tallest?
- Options: **Утром | In the morning** / Днём | At midday / Вечером | In the evening / Рост весь день одинаковый | The same all day
- Scores: F 5, C 4, G 4, D 3, E 5, A 4, P 4
- Verdict: Relatable and surprising; players will want to measure themselves tonight.

### body_017 (multipleChoice, medium): 62 NEEDS IMPROVEMENT, REPLACE

- RU: Где находится самая маленькая кость человека?
- EN: Where is the smallest bone in the human body?
- Options: **В ухе | In the ear** / В мизинце ноги | In the little toe / В носу | In the nose / В пальце руки | In a finger
- Scores: F 4, C 2, G 3, D 4, E 2, A 5, P 3. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: Textbook fact most adults remember from school (the stirrup in the ear); little surprise and the reveal is just a size.

### body_028 (multipleChoice, medium): 82 GOOD, KEEP

- RU: Какой длины ДНК из одной клетки человека, если её размотать?
- EN: How long is the DNA from one human cell when unwound?
- Options: 2 миллиметра | 2 millimetres / 2 сантиметра | 2 centimetres / **2 метра | 2 metres** / 2 километра | 2 kilometres
- Scores: F 4, C 5, G 3, D 4, E 4, A 5, P 4
- Verdict: Two metres of DNA per cell is a real "wait, seriously?", and the order-of-magnitude options make a good guess.

### body_050 (closestValue, hard): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько у человека генов, в которых записаны белки?
- EN: How many protein-coding genes does a human have?
- Answer: **20000** genes, range 0 to 200000, tolerance 7000
- Scores: F 3, C 4, G 2, D 3, E 3, A 4, P 5. Flags: NO_DEDUCTION
- Verdict: The worm comparison is a great payoff, but a gene count from 0 to 200,000 gives nothing to reason from.
- Rewrite (70 GOOD): RU: Учёные ждали у человека 50–140 тысяч генов белков. Сколько оказалось на самом деле? / EN: Scientists expected humans to have 50,000 to 140,000 protein-coding genes. How many are there really?
  - Answer 20000, range 0 to 150000, tolerance 7000
  - Why better: The expectation gives an anchor and sets up the "far fewer than we thought" surprise.

### body_042 (closestValue, hard): 76 GOOD, KEEP

- RU: Сколько раз за день, пока не спит, моргает человек?
- EN: How many times does a person blink in a waking day?
- Answer: **15000** times, range 1000 to 80000, tolerance 8000
- Scores: F 5, C 3, G 4, D 4, E 3, A 5, P 3
- Verdict: Estimable from blinks per minute, fair range, and the screen tip is useful.

## medicine

### medicine_015 (multipleChoice, easy): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Красно-белый столб у парикмахерской напоминает о прежнем втором ремесле цирюльников. Каком?
- EN: A barber's red and white pole recalls the barbers' old second trade. Which?
- Options: **Кровопускание | Bloodletting** / Торговля мясом | Selling meat / Шитьё флагов | Sewing flags / Покраска домов | Painting houses
- Scores: F 3, C 4, G 3, D 3, E 3, A 4, P 4
- Verdict: Bloodletting is a fun surprise, but distractors like "sewing flags" are random rather than tempting.
- Rewrite (72 GOOD): RU: Красно-белый столб у парикмахерской напоминает о втором ремесле цирюльников. Каком? / EN: A barber's red and white pole recalls the barbers' old second trade. Which?
  - Options: **Кровопускание | Bloodletting** / Торговля мясом | Selling meat / Окраска тканей | Dyeing cloth / Шитьё флагов | Sewing flags
  - Why better: A plausible red-and-white trade (dyeing cloth) replaces house painting, and the question is a little shorter.

### medicine_011 (multipleChoice, easy): 71 GOOD, KEEP

- RU: Что клали в клюв маски чумного доктора?
- EN: What was put inside the beak of a plague doctor's mask?
- Options: **Пахучие травы | Fragrant herbs** / Лёд | Ice / Песок | Sand / Порох | Gunpowder
- Scores: F 4, C 3, G 4, D 2, E 3, A 5, P 4
- Verdict: Iconic costume, guessable from "bad air", and the 17th-century twist is a good payoff; the correct option is longer than the one-word distractors.
- Rewrite (75 GOOD): RU: Что клали в клюв маски чумного доктора? / EN: What was put inside the beak of a plague doctor's mask?
  - Options: **Душистые травы | Fragrant herbs** / Толчёный лёд | Crushed ice / Мокрый песок | Wet sand / Тлеющие угли | Smouldering coals
  - Why better: Two-word options of equal weight remove the length tell.

### medicine_012 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Нехватка какого витамина вызывает цингу, бич моряков?
- EN: Lack of which vitamin causes scurvy, the curse of sailors?
- Options: **Витамин C | Vitamin C** / Витамин D | Vitamin D / Витамин A | Vitamin A / Витамин B12 | Vitamin B12
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 4. Flags: SCHOOL_EXAM, OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition question; the guinea pig fact rescues the reveal a little.

### medicine_018 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Что Игнац Земмельвейс в 1847 году потребовал от врачей роддома?
- EN: What did Ignaz Semmelweis demand of maternity doctors in 1847?
- Options: **Мыть руки хлоркой | Wash hands in chlorine** / Носить маски | Wear masks / Брить бороды | Shave their beards / Не пить кофе | Stop drinking coffee
- Scores: F 3, C 3, G 4, D 3, E 3, A 4, P 4
- Verdict: Answerable by common sense and the mocked-hero story is a good payoff, but the obscure name leads and "no coffee" is a throwaway.
- Rewrite (75 GOOD): RU: Что венский врач Земмельвейс в 1847 году потребовал от акушеров? / EN: What did Viennese doctor Semmelweis demand of obstetricians in 1847?
  - Options: **Мыть руки хлоркой | Wash hands in chlorine** / Носить маски | Wear masks / Брить бороды | Shave their beards / Кипятить халаты | Boil their gowns
  - Why better: A plausible hygiene distractor replaces the joke option, so players weigh real alternatives.

### medicine_022 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Откуда взялось «спир» в названии «аспирин»?
- EN: Where does the spir in Aspirin come from?
- Options: **От растения таволги | From the meadowsweet plant** / От слова «спирт» | From the word spirit / От слова «спираль» | From the word spiral / От фамилии химика | From a chemist's surname
- Scores: F 4, C 4, G 3, D 4, E 3, A 4, P 4
- Verdict: Nice wordplay with the "спирт" trap; the meadowsweet answer is obscure but the plant option invites a guess.

### medicine_030 (multipleChoice, medium): 62 NEEDS IMPROVEMENT, IMPROVE

- RU: Кому принадлежит патент на вакцину от полиомиелита, спросили Джонаса Солка. Он ответил: «Разве можно запатентовать…»
- EN: Asked who owned the patent on his polio vaccine, Jonas Salk replied: Could you patent…
- Options: **…солнце? | …the sun?** / …воздух? | …the air? / …море? | …the sea? / …любовь? | …love?
- Scores: F 3, C 3, G 3, D 4, E 3, A 2, P 4. Flags: TOO_WORDY
- Verdict: Famous quote with elegant options, but the question is over 100 characters and hard to read in time.
- Rewrite (66 NEEDS IMPROVEMENT): RU: Джонас Солк не стал патентовать вакцину от полиомиелита: «Разве можно запатентовать…» / EN: Jonas Salk refused to patent his polio vaccine: "Could you patent…"
  - Options: **…солнце? | …the sun?** / …воздух? | …the air? / …море? | …the sea? / …любовь? | …love?
  - Why better: Shorter and clearer, and the refusal framing makes the punchline easier to feel.

### medicine_023 (multipleChoice, medium): 84 GOOD, KEEP

- RU: Ботулизм, а с ним и ботокс, назван по латинскому слову, означающему…
- EN: Botulism, and so Botox, is named after the Latin word for…
- Options: **Колбаса | Sausage** / Сыр | Cheese / Яд | Poison / Морщина | Wrinkle
- Scores: F 5, C 5, G 2, D 4, E 5, A 4, P 5
- Verdict: Botox named after sausage is a pure "wait, seriously?"; the wrinkle and poison traps are well chosen.

### medicine_026 (multipleChoice, medium): 79 GOOD, KEEP

- RU: Зачем хирург Холстед в 1889 году заказал первые резиновые перчатки?
- EN: Why did surgeon William Halsted order the first rubber surgical gloves in 1889?
- Options: **Берёг руки любимой медсестры | To spare his favourite nurse's hands** / Чтобы руки не мёрзли | To keep his hands warm / Скрывал обручальное кольцо | To hide his wedding ring / Так велел закон | The law required it
- Scores: F 4, C 5, G 2, D 3, E 5, A 4, P 5
- Verdict: A love story behind surgical gloves: surprising, funny and very retellable.

### medicine_039 (closestValue, hard): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году ВОЗ объявила, что оспа побеждена во всём мире?
- EN: In what year did the WHO declare smallpox wiped out worldwide?
- Answer: **1980** year, range 1900 to 2020, tolerance 15
- Scores: F 4, C 2, G 4, D 3, E 2, A 5, P 4. Flags: SCHOOL_EXAM
- Verdict: A bare date with a generous tolerance; the strong part (the only disease ever eradicated) is hidden in the fact.
- Rewrite (71 GOOD): RU: Оспа — единственная болезнь человека, побеждённая полностью. В каком году ВОЗ это объявила? / EN: Smallpox is the only human disease ever wiped out. In what year did the WHO declare it?
  - Answer 1980, range 1900 to 2020, tolerance 10
  - Why better: Leading with "the only disease ever beaten" gives the date a reason to matter and a slightly tighter tolerance.

### medicine_047 (closestValue, hard): 54 REJECT, IMPROVE

- RU: В каком году в Бостоне впервые публично провели операцию под эфирным наркозом?
- EN: In what year was an operation first done under ether in public, in Boston?
- Answer: **1846** year, range 1800 to 2000, tolerance 20
- Scores: F 2, C 2, G 3, D 3, E 2, A 4, P 4. Flags: SCHOOL_EXAM
- Verdict: A historical date with an obscure Boston setting; the "no humbug" quote is a good payoff but the question is an exam item.
- Rewrite (66 NEEDS IMPROVEMENT): RU: В каком году хирурги впервые публично усыпили пациента эфиром? / EN: In what year did surgeons first put a patient under with ether in public?
  - Answer 1846, range 1700 to 2000, tolerance 20
  - Why better: Drops the obscure place and widens the range so the "surgery without anaesthesia until when?" intuition does the work.

## space

### space_007 (multipleChoice, easy): 74 GOOD, KEEP

- RU: Что на самом деле такое «падающая звезда»?
- EN: What is a 'shooting star' really?
- Options: Звезда, сорвавшаяся с неба | A star falling from the sky / **Пылинка, сгорающая в воздухе | Dust burning up in the air** / Пролетающий спутник | A passing satellite / Далёкая молния | Distant lightning
- Scores: F 5, C 3, G 4, D 3, E 3, A 5, P 3
- Verdict: Familiar sight with a surprising scale (a grain of sand); solid.

### space_005 (multipleChoice, easy): 60 NEEDS IMPROVEMENT, KEEP

- RU: Что сказал Юрий Гагарин в момент старта?
- EN: What did Yuri Gagarin say at lift-off?
- Options: **«Поехали!» | Poyekhali! (Let's go!)** / «Ключ на старт!» | Key to start! / «Земля, прощай!» | Farewell, Earth! / «Я — Чайка!» | This is Seagull!
- Scores: F 5, C 1, G 2, D 4, E 3, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition for a Russian family; the call sign fact is a nice bonus.

### space_009 (multipleChoice, easy): 55 NEEDS IMPROVEMENT, IMPROVE

- RU: Кто стал первой женщиной в космосе?
- EN: Who was the first woman in space?
- Options: **Валентина Терешкова | Valentina Tereshkova** / Светлана Савицкая | Svetlana Savitskaya / Салли Райд | Sally Ride / Мария Кюри | Marie Curie
- Scores: F 5, C 1, G 2, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER, SCHOOL_EXAM
- Verdict: Every Russian knows Tereshkova, and Marie Curie is a throwaway; the best part ("only woman ever to fly solo") is buried in the fact.
- Rewrite (72 GOOD): RU: Кто до сих пор единственная женщина, слетавшая в космос в одиночку? / EN: Who is still the only woman ever to fly into space alone?
  - Options: **Валентина Терешкова | Valentina Tereshkova** / Светлана Савицкая | Svetlana Savitskaya / Салли Райд | Sally Ride / Елена Кондакова | Yelena Kondakova
  - Why better: Same answer, but the "still the only one" angle surprises and all four women are real cosmonauts or astronauts.

### space_026 (multipleChoice, medium): 73 GOOD, KEEP

- RU: На какой планете сутки длиннее года?
- EN: On which planet is a day longer than a year?
- Options: **Венера | Venus** / Юпитер | Jupiter / Марс | Mars / Нептун | Neptune
- Scores: F 4, C 4, G 3, D 3, E 3, A 5, P 4
- Verdict: A day longer than a year is a genuine paradox; Venus is reachable by reasoning.

### space_022 (multipleChoice, medium): 73 GOOD, KEEP

- RU: На какой планете самый высокий вулкан Солнечной системы?
- EN: Which planet has the tallest volcano in the Solar System?
- Options: **Марс | Mars** / Венера | Venus / Земля | Earth / Юпитер | Jupiter
- Scores: F 4, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: Recognisable (Olympus Mons) with a good height comparison; solid if not surprising.

### space_021 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Какого цвета закат на Марсе?
- EN: What colour is a sunset on Mars?
- Options: **Голубой | Blue** / Ярко-красный | Bright red / Зелёный | Green / Фиолетовый | Purple
- Scores: F 4, C 5, G 2, D 4, E 4, A 5, P 4
- Verdict: Blue sunsets on the red planet are a great reversal of expectation.

### space_029 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Что такое Большое красное пятно на Юпитере?
- EN: What is the Great Red Spot on Jupiter?
- Options: **Гигантский ураган | A giant storm** / Гигантский вулкан | A giant volcano / След от удара астероида | An asteroid impact scar / Море раскалённой лавы | A sea of molten lava
- Scores: F 4, C 2, G 4, D 3, E 3, A 5, P 4
- Verdict: Widely known as a storm; the shrinking mystery is the interesting part but it only appears after the reveal.
- Rewrite (71 GOOD): RU: Большое красное пятно на Юпитере за век сжалось втрое. Что это такое? / EN: Jupiter's Great Red Spot has shrunk to about a third in a century. What is it?
  - Options: **Гигантский ураган | A giant storm** / Гигантский вулкан | A giant volcano / След от удара астероида | An asteroid impact scar / Море раскалённой лавы | A sea of molten lava
  - Why better: The shrinking hook adds intrigue up front and makes the volcano and lava options feel more tempting.

### space_038 (multipleChoice, medium): 87 EXCELLENT, KEEP

- RU: Как сам Гагарин вернулся на Землю?
- EN: How did Gagarin himself get back down to Earth?
- Options: **Сам, на своём парашюте | On his own parachute** / В капсуле, в море | In the capsule, into the sea / В капсуле, на сушу | In the capsule, on land / Сел на полосу, как самолёт | On a runway, like a plane
- Scores: F 5, C 5, G 3, D 4, E 4, A 5, P 5
- Verdict: A hidden Soviet secret about a national hero: surprising, plausible options, great for this family.

### space_043 (closestValue, hard): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько земных дней длится год на Марсе?
- EN: How many Earth days does a year on Mars last?
- Answer: **687** days, range 200 to 1500, tolerance 100
- Scores: F 4, C 2, G 4, D 4, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: Estimable but feels like an astronomy lesson; the fact just restates the reasoning.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Сколько земных дней пришлось бы ждать следующего дня рождения на Марсе? / EN: How many Earth days would you wait for your next birthday on Mars?
  - Answer 687, range 200 to 1500, tolerance 100
  - Why better: Same number, but a birthday framing makes it personal and funnier to argue about.

### space_044 (closestValue, hard): 78 GOOD, KEEP

- RU: Сколько планет размером с Землю встало бы в ряд между Землёй и Луной?
- EN: How many Earths would fit in a row between the Earth and the Moon?
- Answer: **30** , range 0 to 200, tolerance 8
- Scores: F 4, C 4, G 3, D 3, E 4, A 5, P 5
- Verdict: Hard to imagine but estimable, and the "all planets fit" fact is a wow.

## science

### science_005 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Астронавт на Луне уронил молоток и перо одновременно. Что упало первым?
- EN: An astronaut on the Moon dropped a hammer and a feather at once. Which landed first?
- Options: **Упали одновременно | They landed together** / Молоток | The hammer / Перо | The feather / Перо улетело вверх | The feather floated up
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 4
- Verdict: Many know the vacuum answer, but the real Apollo 15 demo is a nice payoff; "feather floated up" is a throwaway.

### science_004 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Как микроволновка греет еду?
- EN: How does a microwave oven heat food?
- Options: **Раскачивает молекулы воды | It makes water molecules jiggle** / Скрытой спиралью | With a hidden heating coil / Сначала греет тарелку | It heats the plate first / Ультрафиолетовой лампой | With an ultraviolet lamp
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 4
- Verdict: Familiar appliance; answer known to many, but the melted candy bar story is a good reveal.

### science_014 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Зачем на мостах оставляют зазоры с металлическими «гребёнками»?
- EN: Why do bridges have gaps with metal 'combs' in the road?
- Options: **Мост удлиняется от жары | The bridge grows in the heat** / Чтобы стекала вода | To let rain drain away / Чтобы будить водителей | To keep drivers awake / Так дешевле строить | It is cheaper to build
- Scores: F 4, C 2, G 4, D 3, E 3, A 4, P 3
- Verdict: Deliberate easy recognition question with a fun distractor ("to keep drivers awake"); reasoning gets you there.

### science_031 (multipleChoice, medium): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: Где звук распространяется быстрее всего?
- EN: Where does sound travel fastest?
- Options: **В стали | In steel** / В воде | In water / В воздухе | In air / В вакууме | In a vacuum
- Scores: F 4, C 2, G 4, D 2, E 2, A 5, P 4. Flags: SCHOOL_EXAM
- Verdict: School physics; "in a vacuum" is a throwaway, though the train-in-the-rails fact is good; the rewrite is only a partial fix, the ceiling is the fact itself.
- Rewrite (66 NEEDS IMPROVEMENT): RU: Где звук распространяется быстрее всего? / EN: Where does sound travel fastest?
  - Options: **В стали | In steel** / В воде | In water / Во льду | In ice / В воздухе | In air
  - Why better: Ice replaces the vacuum, so there are three plausible solids and liquids to weigh.

### science_038 (multipleChoice, medium): 62 NEEDS IMPROVEMENT, IMPROVE

- RU: При какой температуре вода самая плотная?
- EN: At what temperature is water at its densest?
- Options: **+4 °C | +4 °C** / 0 °C | 0 °C / −4 °C | −4 °C / +100 °C | +100 °C
- Scores: F 4, C 2, G 3, D 3, E 2, A 5, P 4. Flags: SCHOOL_EXAM
- Verdict: Reads like a physics exam item; the frozen-lake consequence is the interesting part.
- Rewrite (71 GOOD): RU: Какая температура зимой у дна глубокого озера, если сверху лёд? / EN: In winter, with ice on top, what temperature is the bottom of a deep lake?
  - Options: **Около +4 °C | About +4 °C** / Около 0 °C | About 0 °C / Около −4 °C | About −4 °C / Около +10 °C | About +10 °C
  - Why better: Same answer through a real-world puzzle that players can reason about instead of recalling a textbook number.

### science_033 (multipleChoice, medium): 51 REJECT, REPLACE

- RU: Какой металл окрашивает салют в зелёный цвет?
- EN: Which metal makes fireworks glow green?
- Options: **Барий | Barium** / Натрий | Sodium / Стронций | Strontium / Кальций | Calcium
- Scores: F 3, C 2, G 1, D 4, E 2, A 5, P 3. Flags: SCHOOL_EXAM, NO_DEDUCTION
- Verdict: Pure chemistry memorisation with no reasoning path; the colour list in the fact is mildly interesting at best.

### science_037 (multipleChoice, medium): 59 NEEDS IMPROVEMENT, IMPROVE

- RU: По законам магнетизма магнитный полюс Земли в Арктике — это…
- EN: In terms of magnet physics, Earth's magnetic pole in the Arctic is…
- Options: **Южный полюс | A south pole** / Северный полюс | A north pole / Вообще не полюс | Not a pole at all / То северный, то южный каждый день | North one day, south the next
- Scores: F 3, C 4, G 3, D 2, E 2, A 2, P 4. Flags: AMBIGUOUS
- Verdict: A good brain teaser, but the wording is technical, everyday language calls it the North Magnetic Pole, and the last option is a throwaway.
- Rewrite (70 GOOD): RU: Северный конец стрелки компаса тянется в Арктику. Какой там полюс по законам магнетизма? / EN: A compass needle's north end points to the Arctic. What pole is there in magnet terms?
  - Options: **Южный | A south pole** / Северный | A north pole / Никакой | No pole at all / Оба сразу | Both at once
  - Why better: The compass clue gives players the reasoning path (opposites attract) and removes the naming ambiguity.

### science_027 (multipleChoice, medium): 79 GOOD, KEEP

- RU: Какой буквы нет ни в одном символе таблицы Менделеева?
- EN: Which letter appears in no symbol of the periodic table?
- Options: **Буква J | The letter J** / Буква X | The letter X / Буква Z | The letter Z / Буква W | The letter W
- Scores: F 4, C 4, G 4, D 4, E 3, A 5, P 4
- Verdict: A neat puzzle players can work out by recalling symbols, and the "Q is missing too" fact adds a twist.

### science_044 (closestValue, hard): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: До какой температуры молния раскаляет воздух?
- EN: How hot does lightning make the air?
- Answer: **30000** °C, range 0 to 100000, tolerance 10000
- Scores: F 4, C 4, G 2, D 3, E 3, A 5, P 4. Flags: NO_DEDUCTION
- Verdict: Surprising number, but a 0 to 100,000 slider gives no anchor to estimate from.
- Rewrite (71 GOOD): RU: Поверхность Солнца — около 5500 °C. До скольких градусов молния раскаляет воздух? / EN: The Sun's surface is about 5,500 °C. How hot does lightning make the air?
  - Answer 30000, range 0 to 100000, tolerance 8000
  - Why better: A familiar reference temperature lets players reason and makes "hotter than the Sun" land as a shock.

### science_045 (closestValue, hard): 86 EXCELLENT, KEEP

- RU: Сколько килограммов весит весь воздух в комнате 4 × 5 м с потолком 2,5 м?
- EN: How many kilograms does all the air in a 4 × 5 m room with a 2.5 m ceiling weigh?
- Answer: **60** kg, range 0 to 300, tolerance 15
- Scores: F 4, C 5, G 4, D 4, E 4, A 4, P 5
- Verdict: An everyday room, a calculable answer and a shock (as heavy as an adult); excellent estimation question.

## inventions

### inventions_011 (multipleChoice, easy): 62 NEEDS IMPROVEMENT, IMPROVE

- RU: Кем работал Ласло Биро, изобретатель шариковой ручки?
- EN: What was László Bíró, inventor of the ballpoint pen, by trade?
- Options: **Журналистом | A journalist** / Лётчиком | A pilot / Зубным врачом | A dentist / Поваром | A chef
- Scores: F 3, C 3, G 2, D 3, E 3, A 5, P 4. Flags: NO_DEDUCTION
- Verdict: Good link between job and invention, but nothing in the question lets players reason toward journalism.
- Rewrite (66 NEEDS IMPROVEMENT): RU: Идею шариковой ручки Ласло Биро подсказала его работа. Кем он работал? / EN: László Bíró's job gave him the idea for the ballpoint pen. What was he?
  - Options: **Журналистом | A journalist** / Лётчиком | A pilot / Зубным врачом | A dentist / Поваром | A chef
  - Why better: Hinting that his job inspired the pen gives players something to reason with, and the newspaper ink fact then clicks.

### inventions_010 (multipleChoice, easy): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Кем был Сэмюэл Морзе, прежде чем придумал свою азбуку?
- EN: What was Samuel Morse before he devised his code?
- Options: **Художником-портретистом | A portrait painter** / Капитаном корабля | A ship's captain / Почтальоном | A postman / Телеграфистом | A telegraph operator
- Scores: F 4, C 4, G 2, D 3, E 3, A 5, P 4
- Verdict: A painter inventing a code is a nice surprise, but "telegraph operator" is anachronistic before the telegraph existed and repeats the question's idea.
- Rewrite (71 GOOD): RU: Кем был Сэмюэл Морзе, прежде чем придумал свою азбуку? / EN: What was Samuel Morse before he devised his code?
  - Options: **Художником-портретистом | A portrait painter** / Капитаном корабля | A ship's captain / Почтальоном | A postman / Часовщиком | A watchmaker
  - Why better: A plausible technical trade replaces the self-referential distractor.

### inventions_007 (multipleChoice, easy): 70 GOOD, KEEP

- RU: Что показал Элиша Отис в 1854 году, велев перерубить трос?
- EN: What did Elisha Otis show off in 1854 by having the rope cut?
- Options: **Безопасный лифт | A safe lift** / Подвесной мост | A suspension bridge / Канатную дорогу | A cable car / Парашют | A parachute
- Scores: F 3, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: The rope-cutting stunt is vivid and leads to the answer by reasoning; the skyscraper link is a good payoff.

### inventions_035 (multipleChoice, medium): 79 GOOD, KEEP

- RU: Первый моторный пылесос 1901 года был так велик, что...
- EN: The first powered vacuum cleaner of 1901 was so big that...
- Options: **его везла лошадь, шланг шёл в окно | a horse pulled it, hoses went in windows** / его ставили на крышу | it was set up on the roof / им чистили только корабли | it only cleaned ships / его тянул паровоз | a steam engine towed it
- Scores: F 4, C 5, G 3, D 2, E 5, A 4, P 4
- Verdict: A horse-drawn vacuum cleaner is funny and surprising, but the correct option is twice as long as the rest.
- Rewrite (83 GOOD): RU: Первый моторный пылесос 1901 года был так велик, что... / EN: The first powered vacuum cleaner of 1901 was so big that...
  - Options: **его возила лошадь | a horse pulled it** / его ставили на крышу | it sat on the roof / им чистили лишь корабли | it only cleaned ships / его тянул паровоз | a steam engine towed it
  - Why better: Equal-length endings remove the tell while the horse stays the funniest answer.

### inventions_031 (multipleChoice, medium): 58 NEEDS IMPROVEMENT, IMPROVE

- RU: Маргарет Найт отсудила патент у мужчины, укравшего её идею. Что делала её машина?
- EN: Margaret Knight won her patent back from a man who copied her. What did her machine make?
- Options: **Бумажные пакеты | Paper bags** / Спички | Matches / Пуговицы | Buttons / Конверты | Envelopes
- Scores: F 3, C 3, G 2, D 3, E 3, A 3, P 4. Flags: TOO_WORDY
- Verdict: The sexism story is a good payoff, but the setup is wordy and "paper bags" is the longest option; the rewrite is only a partial fix, the ceiling is the obscure inventor.
- Rewrite (62 NEEDS IMPROVEMENT): RU: Изобретательница в 1871-м отсудила патент у мужчины. Что делала её машина? / EN: In 1871 a woman inventor won her patent back from a man. What did her machine make?
  - Options: **Бумажные пакеты | Paper bags** / Спичечные коробки | Matchboxes / Жестяные банки | Tin cans / Почтовые конверты | Envelopes
  - Why better: Shorter question with four balanced everyday products.

### inventions_028 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Кто в 1596 году описал в книге смывной унитаз собственной конструкции?
- EN: Who described his own flushing toilet in a book in 1596?
- Options: **Крестник Елизаветы I | Queen Elizabeth I's godson** / Томас Крэппер | Thomas Crapper / Исаак Ньютон | Isaac Newton / Бенджамин Франклин | Benjamin Franklin
- Scores: F 3, C 5, G 2, D 2, E 4, A 4, P 5
- Verdict: The Thomas Crapper trap and the "Ajax" pun are great, but the correct option is the only description among three names.
- Rewrite (75 GOOD): RU: Кто в 1596 году описал в книге смывной унитаз собственной конструкции? / EN: Who described his own flushing toilet in a book in 1596?
  - Options: **Крестник Елизаветы I | Elizabeth I's godson** / Сантехник Томас Крэппер | Plumber Thomas Crapper / Придворный шут королевы | The queen's court jester / Лекарь королевы | The queen's physician
  - Why better: All options are now descriptions of people, so the format no longer singles out the answer.

### inventions_030 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Тефлон открыли случайно. Что химик пытался получить?
- EN: Teflon was found by accident. What was the chemist trying to make?
- Options: **Хладагент для холодильников | A refrigerant for fridges** / Ракетное топливо | Rocket fuel / Жевательную резинку | Chewing gum / Краску для машин | Car paint
- Scores: F 5, C 3, G 2, D 4, E 3, A 5, P 4
- Verdict: Teflon is in every kitchen and the fridge-gas accident is a satisfying reveal.

### inventions_036 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Суперклей открыли в 1942 году, когда искали материал для...
- EN: Super glue was found in 1942 by a team trying to make...
- Options: **прозрачных прицелов | clear plastic gun sights** / зубных пломб | dental fillings / обойного клея | wallpaper paste / обувных подошв | shoe soles
- Scores: F 4, C 4, G 2, D 4, E 3, A 5, P 4
- Verdict: Super glue from a gun sight project is surprising and the dental filling trap is believable.

### inventions_048 (closestValue, hard): 74 GOOD, KEEP

- RU: Сколько бритвенных станков продала компания Gillette в 1903 году, первом году продаж?
- EN: How many razors did Gillette sell in 1903, its first year on sale?
- Answer: **51** razors, range 0 to 2000, tolerance 30
- Scores: F 4, C 5, G 2, D 3, E 4, A 4, P 4
- Verdict: Only 51 razors in year one is a real surprise; players will guess thousands.

### inventions_047 (closestValue, hard): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Уолтер Хант придумал английскую булавку, чтобы отдать долг в 15 долларов. За сколько он продал патент?
- EN: Walter Hunt invented the safety pin to repay a $15 debt. How much did he sell the patent for?
- Answer: **400** dollars, range 0 to 5000, tolerance 150
- Scores: F 3, C 4, G 3, D 3, E 3, A 3, P 5. Flags: TOO_WORDY
- Verdict: Great story ($15 debt, $400 sale), but the question is over 100 characters.
- Rewrite (72 GOOD): RU: Английскую булавку придумали, чтобы отдать долг в $15. За сколько продали патент? / EN: The safety pin was invented to pay off a $15 debt. How much did its patent sell for?
  - Answer 400, range 0 to 5000, tolerance 150
  - Why better: Same hook in under 85 characters, readable in the time limit.

## gadgets

### gadgets_008 (multipleChoice, easy): 70 GOOD, KEEP

- RU: Откуда взялось слово «пиксель»?
- EN: Where does the word "pixel" come from?
- Options: Название первой видеокарты | The first graphics card's name / **Сокращение от «элемент картинки» | Short for "picture element"** / Латинское слово «точка» | The Latin for "dot" / Фамилия изобретателя | The inventor's surname
- Scores: F 4, C 3, G 4, D 3, E 2, A 5, P 4
- Verdict: Guessable from "pix" + "el", and the 176 by 176 baby photo is a nice reveal; the similar-to flags are only the word-origin formula.

### gadgets_012 (multipleChoice, easy): 63 NEEDS IMPROVEMENT, KEEP

- RU: Из чего в основном делают компьютерные микросхемы?
- EN: What are computer chips mostly made of?
- Options: Из пластика | Plastic / Из меди | Copper / Из золота | Gold / **Из кремния | Silicon**
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition question; Silicon Valley gives it away.

### gadgets_006 (multipleChoice, easy): 77 GOOD, KEEP

- RU: Зачем на клавишах F и J (в русской раскладке А и О) есть маленькие выступы?
- EN: Why do the F and J keys have small bumps on them?
- Options: **Чтобы находить их не глядя | To find them without looking** / Это буквы шрифта Брайля | They are Braille letters / Это следы от литья пластика | They are moulding marks / Чтобы клавиши не залипали | To stop the keys sticking
- Scores: F 5, C 3, G 4, D 4, E 4, A 4, P 3
- Verdict: Everyone has touched those bumps; relatable and quick.

### gadgets_024 (multipleChoice, medium): 81 GOOD, KEEP

- RU: Зачем Microsoft добавила в Windows пасьянс «Косынка»?
- EN: Why did Microsoft put Solitaire into Windows?
- Options: Это была шутка программиста | As a programmer's prank / **Чтобы учить работать мышью | To teach using a mouse** / По просьбе казино | At a casino's request / Чтобы проверять мониторы | To test monitors
- Scores: F 5, C 4, G 3, D 4, E 4, A 5, P 4
- Verdict: Solitaire as a mouse tutorial is a great "oh, that makes sense" moment for every generation.

### gadgets_017 (multipleChoice, medium): 76 GOOD, KEEP

- RU: Откуда взялось название Wi-Fi?
- EN: Where did the name Wi-Fi come from?
- Options: **Рекламщики обыграли слово Hi-Fi | An ad agency's play on Hi-Fi** / Название городка в Уэльсе | A town in Wales / Инициалы изобретателей | The inventors' initials / Сокращение от Wide Field | Short for "Wide Field"
- Scores: F 5, C 4, G 3, D 3, E 3, A 5, P 4
- Verdict: Most assume Wi-Fi stands for something; the marketing pun is a fun reveal. The similar-to flags are the naming formula only.

### gadgets_037 (multipleChoice, medium): 76 GOOD, KEEP

- RU: В честь чего назван язык программирования Python?
- EN: What is the Python programming language named after?
- Options: **Комик-группы «Монти Пайтон» | The Monty Python comedy group** / Питомца автора | Its author's pet / Змеи питона | The python snake / Греческого чудовища Пифона | The Greek monster Python
- Scores: F 4, C 4, G 3, D 4, E 4, A 5, P 3
- Verdict: The snake trap is perfect and Monty Python is a fun reveal.

### gadgets_025 (multipleChoice, medium): 76 GOOD, KEEP

- RU: Для чего в 1994 году придумали QR-код?
- EN: What was the QR code invented for in 1994?
- Options: Для меню в ресторанах | Restaurant menus / **Учёт деталей машин на заводе | Tracking car parts in factories** / Для билетов в метро | Metro tickets / Для защиты банкнот | Protecting banknotes
- Scores: F 5, C 4, G 3, D 3, E 3, A 5, P 4
- Verdict: QR codes for car parts is a good surprise; the correct option lacks the "Для" the others share.
- Rewrite (78 GOOD): RU: Для чего в 1994 году придумали QR-код? / EN: What was the QR code invented for in 1994?
  - Options: Для меню в ресторанах | Restaurant menus / **Для учёта автодеталей | Tracking car parts** / Для билетов в метро | Metro tickets / Для защиты банкнот | Protecting banknotes
  - Why better: Matching "Для..." wording removes the grammar tell.

### gadgets_026 (multipleChoice, medium): 58 NEEDS IMPROVEMENT, IMPROVE

- RU: Какое животное стало талисманом Linux?
- EN: Which animal is the mascot of Linux?
- Options: Сова | An owl / Лиса | A fox / Слон | An elephant / **Пингвин | A penguin**
- Scores: F 3, C 2, G 2, D 3, E 3, A 5, P 4. Flags: NO_DEDUCTION
- Verdict: Known to tech fans, a random pick for others; the penguin bite story is the fun part and is hidden in the fact.
- Rewrite (68 NEEDS IMPROVEMENT): RU: Создатель Linux уверял, что его укусил этот зверь. Кто стал талисманом системы? / EN: Linux's creator claimed this animal once bit him. Which became the system's mascot?
  - Options: Сова | An owl / Лиса | A fox / Енот | A raccoon / **Пингвин | A penguin**
  - Why better: The bite hook makes the question funny even for players who have never seen Tux.

### gadgets_040 (closestValue, hard): 78 GOOD, KEEP

- RU: Сколько граммов весил телефон, с которого Мартин Купер сделал первый звонок с мобильного?
- EN: How many grams did the phone Martin Cooper used for the first mobile call weigh?
- Answer: **1100** g, range 0 to 5000, tolerance 400
- Scores: F 4, C 3, G 4, D 4, E 4, A 5, P 4
- Verdict: The "brick phone" image makes this estimable and the 10-hour charge is a fun payoff.

### gadgets_045 (closestValue, hard): 63 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году IBM представила первый жёсткий диск?
- EN: In which year did IBM unveil the first hard disk drive?
- Answer: **1956** year, range 1920 to 2020, tolerance 10
- Scores: F 3, C 3, G 3, D 3, E 2, A 5, P 4. Flags: SCHOOL_EXAM
- Verdict: A bare year; the striking part (a tonne for 3.75 MB) only appears after the reveal.
- Rewrite (72 GOOD): RU: Первый жёсткий диск IBM весил около тонны и хранил 3,75 МБ. В каком году он вышел? / EN: IBM's first hard drive weighed about a tonne and held 3.75 MB. In what year did it come out?
  - Answer 1956, range 1920 to 2020, tolerance 8
  - Why better: Leading with the absurd size gives players era clues and a laugh before they guess.

## memes

### memes_011 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Кто в меме 2019 года будит хозяйку словами «Наташ, вставай, мы там всё уронили»?
- EN: Who wakes their owner in the 2019 meme 'Natasha, wake up, we dropped everything'?
- Options: **Коты | Cats** / Дети | Kids / Собаки | Dogs / Соседи | Neighbours
- Scores: F 5, C 1, G 3, D 3, E 4, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition for the young players; instantly fun, zero surprise.

### memes_012 (multipleChoice, easy): 76 GOOD, KEEP

- RU: Откуда взялась фотография «Неверного парня», который оглядывается на другую девушку?
- EN: Where did the 'Distracted boyfriend' photo, a man turning to look at another woman, come from?
- Options: **Это стоковое фото | A stock photo** / Кадр из фильма | A film still / Снимок папарацци | A paparazzi shot / Рекламный плакат | An advertising poster
- Scores: F 4, C 4, G 3, D 4, E 4, A 4, P 4
- Verdict: Everyone knows the meme; that it is a staged stock photo is a satisfying reveal.

### memes_001 (multipleChoice, easy): 79 GOOD, KEEP

- RU: В 2015 году словом года по версии Оксфордского словаря стало эмодзи. Какое?
- EN: In 2015 Oxford Dictionaries named an emoji its Word of the Year. Which one?
- Options: **Лицо со слезами радости | Face with tears of joy** / Красное сердечко | Red heart / Большой палец вверх | Thumbs up / Подмигивающий смайлик | Winking face
- Scores: F 5, C 3, G 4, D 4, E 4, A 5, P 3
- Verdict: Guessable and fun to argue about which emoji wins.

### memes_028 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Ухмыляющееся лицо интернет-тролля, Trollface, нарисовали в 2008 году. В какой программе?
- EN: Trollface, the grinning face of the internet troll, was drawn in 2008. In which program?
- Options: Фотошоп | Photoshop / Эксель | Excel / **Пэйнт | Paint** / ПауэрПойнт | PowerPoint
- Scores: F 4, C 3, G 4, D 3, E 4, A 5, P 5
- Verdict: The crude look points to Paint, and earning $100,000 from it is a great payoff.

### memes_016 (multipleChoice, medium): 74 GOOD, KEEP

- RU: Что значит «вики» в слове «Википедия»?
- EN: What does 'wiki' in Wikipedia mean?
- Options: **«Быстро» по-гавайски | 'Quick' in Hawaiian** / Это сокращение What I Know Is | Short for 'What I Know Is' / «Знание» на суахили | 'Knowledge' in Swahili / «Свободный» по-фински | 'Free' in Finnish
- Scores: F 5, C 4, G 2, D 4, E 3, A 5, P 4
- Verdict: The "What I Know Is" backronym is a perfect trap and the airport shuttle is a lovely reveal.

### memes_023 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Почему жаргон Рунета прозвали «олбанским языком»?
- EN: Why was Russian internet slang dubbed the 'Albanian language'?
- Options: **Шутка над американцем в ЖЖ | A joke on an American blogger** / Стиль албанского хакера | An Albanian hacker's style / Опечатка в словаре | A dictionary typo / Сленг СМС из 90-х | 1990s texting slang
- Scores: F 4, C 3, G 3, D 3, E 4, A 4, P 4
- Verdict: A Runet classic for the older players; the LiveJournal story is fun.

### memes_037 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Из какой страны носильщики гроба из мема Coffin Dance (2020)?
- EN: Which country are the dancing pallbearers of the Coffin Dance meme (2020) from?
- Options: **Гана | Ghana** / Ямайка | Jamaica / Бразилия | Brazil / Куба | Cuba
- Scores: F 5, C 2, G 4, D 3, E 4, A 5, P 3
- Verdict: Africa is the only plausible pick for many, but the meme is beloved and the joyful-funeral fact lands.

### memes_020 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Кто на самом деле снят в вирусном ролике «Драматичный бурундук» (2007)?
- EN: Which animal really stars in the viral 'Dramatic Chipmunk' clip (2007)?
- Options: **Луговая собачка | A prairie dog** / Бурундук | A chipmunk / Хомяк | A hamster / Белка | A squirrel
- Scores: F 3, C 4, G 3, D 3, E 4, A 5, P 3
- Verdict: The chipmunk option repeats the question, but the "it was never a chipmunk" twist is fun.

### memes_043 (closestValue, hard): 60 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году на YouTube появился Nyan Cat — пиксельный кот с радугой?
- EN: In which year did Nyan Cat, the pixel cat with a rainbow trail, appear on YouTube?
- Answer: **2011** year, range 1950 to 2026, tolerance 7
- Scores: F 4, C 1, G 4, D 2, E 2, A 5, P 4. Flags: SCHOOL_EXAM
- Verdict: A meme year on a 1950-2026 slider with tolerance 7 is trivial; the Pop-Tart and NFT facts are the real payoff.
- Rewrite (67 NEEDS IMPROVEMENT): RU: В каком году на YouTube появился Nyan Cat — пиксельный кот с радугой? / EN: In which year did Nyan Cat, the pixel cat with a rainbow trail, appear on YouTube?
  - Answer 2011, range 2000 to 2020, tolerance 2
  - Why better: A realistic range and tight tolerance turn a free point into a real nostalgia test.

### memes_050 (closestValue, hard): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: В 2006 году последние 1000 пикселей «Сайта на миллион долларов» продали на eBay. За сколько?
- EN: In 2006 the last 1,000 pixels of the Million Dollar Homepage were auctioned on eBay. For how much?
- Answer: **38100** dollars, range 0 to 200000, tolerance 10000
- Scores: F 2, C 4, G 3, D 3, E 3, A 4, P 4. Flags: OBSCURE_TRIVIA
- Verdict: Surprising number, but the Million Dollar Homepage is unknown to most and the $1-per-pixel setup is hidden in the fact.
- Rewrite (68 NEEDS IMPROVEMENT): RU: Студент продавал пиксели своего сайта по $1. За сколько на eBay ушли последние 1000? / EN: A student sold his site's pixels at $1 each. What did the last 1,000 fetch on eBay?
  - Answer 38100, range 0 to 100000, tolerance 8000
  - Why better: The $1 baseline gives players an anchor and makes the 38x jump the punchline.

## noughties

### noughties_001 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Какая английская фраза спрятана в названии ICQ?
- EN: Which English phrase is hidden in the name ICQ?
- Options: **I seek you — «ищу тебя» | I seek you** / Instant chat queue — «очередь чата» | Instant chat queue / Ice cube — «кубик льда» | Ice cube / I keep quiet — «я молчу» | I keep quiet
- Scores: F 4, C 3, G 4, D 3, E 4, A 4, P 3
- Verdict: Nostalgic "Аська" for the older players and the phrase is guessable by sounding it out.

### noughties_007 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: Какой самолёт совершил последний коммерческий рейс в 2003 году?
- EN: Which airliner made its last commercial flight in 2003?
- Options: **«Конкорд» | Concorde** / Ту-144 | Tu-144 / «Боинг-747» | Boeing 747 / Ил-86 | Il-86
- Scores: F 4, C 2, G 4, D 3, E 2, A 5, P 3
- Verdict: Deliberate easy recognition question; the Tu-144 trap works for this family.

### noughties_011 (multipleChoice, easy): 73 GOOD, KEEP

- RU: Как называлась мужская сумочка с ремешком на запястье, хит конца 90-х и нулевых?
- EN: What was the small men's bag with a wrist strap, a must in Russia around 2000, called?
- Options: **Борсетка | Borsetka** / Авоська | Avoska / Ридикюль | Ridikyul / Кисет | Kiset
- Scores: F 5, C 2, G 3, D 4, E 4, A 5, P 4
- Verdict: Instant nostalgia for the 40-50 players and the Italian "lady's handbag" twist is funny.

### noughties_019 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Какой хит коверкает припев «Asereje» группы Las Ketchup (2002)?
- EN: Which hit does the chorus of Las Ketchup's Asereje (2002) garble?
- Options: **Рэп «Rapper's Delight» | Rapper's Delight** / Танец «Macarena» | Macarena / Песню «Y.M.C.A.» | Y.M.C.A. / Песню «La Bamba» | La Bamba
- Scores: F 4, C 5, G 2, D 3, E 4, A 5, P 5
- Verdict: Nobody suspects Asereje is garbled Rapper's Delight, and the "Tomato" father is a great extra.

### noughties_016 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Чего НЕ умел первый iPhone (2007)?
- EN: What could the first iPhone (2007) NOT do?
- Options: **Снимать видео | Record video** / Выходить в интернет | Browse the web / Фотографировать | Take photos / Играть музыку | Play music
- Scores: F 5, C 4, G 2, D 4, E 4, A 4, P 4
- Verdict: Surprising limitation of a famous device; the NOT wording is the only weakness.
- Rewrite (77 GOOD): RU: Без чего вышел первый iPhone в 2007 году? / EN: What was the first iPhone (2007) missing?
  - Options: **Без видеосъёмки | Video recording** / Без интернета | Web browsing / Без камеры | A camera / Без музыки | Music playback
  - Why better: Positive phrasing reads faster than a NOT question with the same options.

### noughties_037 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Чем полифонический рингтон отличался от монофонического?
- EN: What made a polyphonic ringtone different from a monophonic one?
- Options: **Звучало несколько нот сразу | Several notes at once** / Он был громче | It was louder / Играла настоящая запись | It played a real recording / Мигала подсветка | The screen flashed
- Scores: F 4, C 2, G 4, D 3, E 3, A 5, P 3
- Verdict: Nostalgic, but "poly" gives the answer away for anyone who thinks about the word.
- Rewrite (74 GOOD): RU: В 2003-м все хвастались полифонией в телефоне. Что она означала? / EN: In 2003 everyone bragged about polyphonic ringtones. What did that mean?
  - Options: **Звучало несколько нот сразу | Several notes at once** / Он был громче | It was louder / Играла настоящая запись | It played a real recording / Мигала подсветка | The screen flashed
  - Why better: The bragging framing brings back the memory and makes it a shared laugh rather than a definition.

### noughties_033 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Где жили участники первого российского реалити-шоу «За стеклом» (2001)?
- EN: Where did the cast of Russia's first reality show, Behind the Glass (2001), live?
- Options: **В гостинице «Россия» | In the Rossiya hotel** / В Останкинской башне | In the Ostankino tower / В ГУМе | In the GUM store / На теплоходе | On a river boat
- Scores: F 3, C 3, G 2, D 4, E 4, A 5, P 4
- Verdict: Memorable for the older players, but the fun detail (paying to peek) is hidden in the fact.
- Rewrite (70 GOOD): RU: За 20 рублей прохожие 5 минут глядели на участников шоу «За стеклом». Где те жили? / EN: For 20 roubles passers-by could watch the Behind the Glass cast for 5 minutes. Where did they live?
  - Options: **В гостинице «Россия» | In the Rossiya hotel** / В Останкинской башне | In the Ostankino tower / В ГУМе | In the GUM store / На теплоходе | On a river boat
  - Why better: Leading with the paid peeking makes the setting vivid and gives the guess context.

### noughties_035 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Чем удивил «Ночной дозор» (2004) зрителей в зарубежном прокате?
- EN: What surprised foreign audiences of the Russian film Night Watch (2004)?
- Options: **Анимированными субтитрами | Animated subtitles** / Дубляжом голливудских звёзд | Hollywood stars dubbing it / Чёрно-белой версией | A black and white cut / Двумя разными концовками | Two different endings
- Scores: F 3, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: "Foreign audience" points to subtitles or dubbing, so it is reasonable, and the dissolving red letters are a nice detail.

### noughties_044 (closestValue, hard): 75 GOOD, KEEP

- RU: Сколько точек в ширину был экран Nokia 3310?
- EN: How many pixels wide was the Nokia 3310 screen?
- Answer: **84** pixels, range 0 to 500, tolerance 20
- Scores: F 5, C 3, G 3, D 4, E 4, A 5, P 3
- Verdict: Everyone remembers the chunky Snake screen; good nostalgia estimation.

### noughties_046 (closestValue, hard): 78 GOOD, KEEP

- RU: Windows XP вышла в 2001 году. Сколько лет Microsoft её поддерживала?
- EN: Windows XP came out in 2001. For how many years did Microsoft support it?
- Answer: **12.5** years, range 0 to 50, tolerance 3.5
- Scores: F 5, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Estimable from memory and the "still on 30% of PCs" fact is fun.

## animals

### animals_014 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Как медоносная пчела рассказывает другим, где нашла цветы?
- EN: How does a honeybee tell the others where it found flowers?
- Options: **Танцует | It dances** / Рисует карту на сотах | It draws a map on the comb / Приносит лепесток | It brings back a petal / Меняет цвет брюшка | It changes colour
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 4
- Verdict: Many know the waggle dance; the angle-to-the-sun detail is a satisfying payoff.

### animals_001 (multipleChoice, easy): 81 GOOD, KEEP

- RU: Какой формы какашки у вомбата?
- EN: What shape is wombat poop?
- Options: Звёздочки | Stars / **Кубики | Cubes** / Спиральки | Spirals / Сердечки | Hearts
- Scores: F 4, C 5, G 2, D 4, E 5, A 5, P 4
- Verdict: Cube poop is a classic crowd-pleaser; all options are equally silly so nothing is telegraphed.

### animals_008 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Кто самый крупный грызун на Земле?
- EN: What is the largest rodent on Earth?
- Options: **Капибара | The capybara** / Бобр | The beaver / Дикобраз | The porcupine / Сурок | The marmot
- Scores: F 5, C 2, G 4, D 4, E 3, A 5, P 3
- Verdict: Capybara is a known meme animal now; recognition with a decent fact.

### animals_023 (multipleChoice, medium): 84 GOOD, KEEP

- RU: Какой вкус кошки не чувствуют вовсе?
- EN: Which taste can cats not detect at all?
- Options: **Сладкий | Sweet** / Мясной (умами) | Savoury (umami) / Горький | Bitter / Кислый | Sour
- Scores: F 5, C 4, G 3, D 4, E 5, A 5, P 4
- Verdict: Every cat owner will argue; the broken sweet gene is a great payoff.

### animals_038 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой из этих зверьков — родственник слона?
- EN: Which of these small animals is a relative of the elephant?
- Options: **Даман | The hyrax** / Сурок | The marmot / Кролик | The rabbit / Морская свинка | The guinea pig
- Scores: F 2, C 5, G 2, D 2, E 3, A 4, P 5. Flags: NO_DEDUCTION
- Verdict: Great reveal, but "даман" is the only unfamiliar name, so players pick it by elimination rather than intuition.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Какой из этих зверьков — родственник слона? / EN: Which of these small animals is a relative of the elephant?
  - Options: **Даман | The hyrax** / Тушканчик | The jerboa / Шиншилла | The chinchilla / Сурикат | The meerkat
  - Why better: All four are small exotic animals, so the odd name no longer gives the answer away.

### animals_036 (multipleChoice, medium): 77 GOOD, KEEP

- RU: Как защищается жук-бомбардир?
- EN: How does a bombardier beetle defend itself?
- Options: **Брызжет горячей едкой струёй | It sprays a hot caustic jet** / Кусает ядом | A venomous bite / Притворяется веточкой | It pretends to be a twig / Ярко светится | It glows brightly
- Scores: F 3, C 4, G 4, D 3, E 4, A 5, P 4
- Verdict: The name "bombardier" invites a reasoning guess, and the 100 °C pop is a fun reveal.

### animals_035 (multipleChoice, medium): 88 EXCELLENT, KEEP

- RU: Зачем ленивец примерно раз в неделю слезает с дерева?
- EN: Why does a sloth climb down from its tree about once a week?
- Options: **Сходить в туалет | To go to the toilet** / Попить воды | To drink water / Погреться на солнце | To warm up in the sun / Почистить когти | To clean its claws
- Scores: F 5, C 5, G 3, D 4, E 5, A 5, P 4
- Verdict: A sloth risking its life to poop is funny and surprising; strong group reaction.

### animals_030 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой зверёк ест быстрее всех: находит и глотает добычу за четверть секунды?
- EN: Which mammal eats fastest, finding and swallowing prey in a quarter of a second?
- Options: **Звездонос | The star-nosed mole** / Муравьед | The anteater / Землеройка | The shrew / Белка | The squirrel
- Scores: F 2, C 4, G 2, D 3, E 3, A 4, P 4. Flags: OBSCURE_TRIVIA
- Verdict: The star-nosed mole is unknown to most and its odd name is the only exotic option, so it is picked by elimination; the rewrite is only a partial fix, the ceiling is the obscure animal.
- Rewrite (63 NEEDS IMPROVEMENT): RU: Какой зверёк ест быстрее всех: находит и глотает добычу за четверть секунды? / EN: Which mammal eats fastest, finding and swallowing prey in a quarter of a second?
  - Options: **Звездонос | The star-nosed mole** / Землеройка | The shrew / Муравьед | The anteater / Утконос | The platypus
  - Why better: The platypus adds a second odd name, so the answer is no longer the only exotic option.

### animals_044 (closestValue, hard): 78 GOOD, KEEP

- RU: Сколько граммов весит страусиное яйцо?
- EN: How much does an ostrich egg weigh, in grams?
- Answer: **1400** g, range 0 to 5000, tolerance 400
- Scores: F 5, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Estimable from chicken eggs and the "small for the bird" fact is a nice twist.

### animals_049 (closestValue, hard): 79 GOOD, KEEP

- RU: Сколько минут ленивец может не дышать, ныряя под воду?
- EN: For how many minutes can a sloth hold its breath underwater?
- Answer: **40** minutes, range 0 to 150, tolerance 12
- Scores: F 5, C 5, G 2, D 3, E 4, A 5, P 4
- Verdict: A sloth holding its breath for 40 minutes is a genuine shock.

## sea

### sea_001 (multipleChoice, easy): 74 GOOD, KEEP

- RU: Сколько сердец у осьминога?
- EN: How many hearts does an octopus have?
- Options: Одно | One / Два | Two / **Три | Three** / Восемь, по одному на щупальце | Eight, one per arm
- Scores: F 5, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Well-known fact by now, but the "one per arm" trap and blue blood keep it fun.

### sea_005 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Что в первую очередь вызывает приливы и отливы?
- EN: What mainly causes the tides?
- Options: **Притяжение Луны | The Moon's gravity** / Ветер | The wind / Нагрев воды Солнцем | The Sun warming the water / Реки, впадающие в море | Rivers flowing into the sea
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 4. Flags: SCHOOL_EXAM, OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition question; the two-bulges fact is a modest payoff.

### sea_004 (multipleChoice, easy): 63 NEEDS IMPROVEMENT, REPLACE

- RU: Где находится самое глубокое место Мирового океана?
- EN: Where is the deepest point of the world's oceans?
- Options: **В Марианской впадине | The Mariana Trench** / В Бермудском треугольнике | The Bermuda Triangle / В Пуэрто-Риканском жёлобе | The Puerto Rico Trench / Под Северным полюсом | Under the North Pole
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 3. Flags: SCHOOL_EXAM, OBVIOUS_ANSWER, REPETITIVE
- Verdict: Pure school recognition, and the sea topic already has sea_031 and sea_041 on the Mariana Trench; three in one topic is too many and this is the weakest of them.

### sea_031 (multipleChoice, medium): 79 GOOD, KEEP

- RU: Кто в 2012 году в одиночку опустился на дно Марианской впадины?
- EN: Who went down alone to the bottom of the Mariana Trench in 2012?
- Options: Жак-Ив Кусто | Jacques Cousteau / **Джеймс Кэмерон | James Cameron** / Ричард Брэнсон | Richard Branson / Илон Маск | Elon Musk
- Scores: F 5, C 4, G 3, D 3, E 4, A 5, P 4
- Verdict: James Cameron diving alone is a fun surprise and the Musk distractor gets a laugh.

### sea_028 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, REPLACE

- RU: Кто из них ближайший родственник морской звезды?
- EN: Which of these is the starfish's closest relative?
- Options: **Морской ёж | The sea urchin** / Медуза | The jellyfish / Краб | The crab / Осьминог | The octopus
- Scores: F 4, C 3, G 3, D 4, E 2, A 5, P 3
- Verdict: Classification with a modest reveal; the jellyfish trap is fine but the payoff is thin compared with the rest of the sea topic.

### sea_024 (multipleChoice, medium): 78 GOOD, KEEP

- RU: У кого самые большие глаза среди ныне живущих животных?
- EN: Which living animal has the biggest eyes?
- Options: У синего кита | The blue whale / **У глубоководного кальмара | A deep sea squid** / У рыбы-меч | The swordfish / У кашалота | The sperm whale
- Scores: F 4, C 4, G 3, D 3, E 4, A 5, P 5
- Verdict: Dinner-plate eyes for spotting sperm whales is a great payoff; the correct option is the longest.
- Rewrite (80 GOOD): RU: У кого самые большие глаза среди ныне живущих животных? / EN: Which living animal has the biggest eyes?
  - Options: У синего кита | The blue whale / **У кальмара | A squid** / У рыбы-меч | The swordfish / У кашалота | The sperm whale
  - Why better: Short, equal options keep the whales as the trap without a length tell.

### sea_035 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Что происходит с глазами камбалы, пока она растёт?
- EN: What happens to a flounder's eyes as it grows up?
- Options: **Один переползает на другую сторону | One moves to the other side** / Один глаз отмирает | One eye dies off / Вырастает третий глаз | It grows a third eye / Они срастаются в один | They merge into one
- Scores: F 4, C 5, G 3, D 3, E 4, A 4, P 4
- Verdict: A migrating eye is a real "wait, seriously?", but the correct option is much longer than the rest.
- Rewrite (80 GOOD): RU: Что происходит с глазами камбалы, пока она растёт? / EN: What happens to a flounder's eyes as it grows up?
  - Options: **Один переползает на другой бок | One moves to the other side** / Один зарастает кожей | One grows over with skin / Между ними растёт третий | A third grows between them / Они срастаются в один | They merge into one
  - Why better: Balanced lengths remove the tell.

### sea_019 (multipleChoice, medium): 82 GOOD, KEEP

- RU: Кого учёные называют «бессмертным»?
- EN: Which animal do scientists call immortal?
- Options: **Медузу турритопсис | The Turritopsis jellyfish** / Морскую черепаху | The sea turtle / Осьминога | The octopus / Рыбу-клоуна | The clownfish
- Scores: F 4, C 5, G 3, D 3, E 4, A 5, P 5
- Verdict: An immortal jellyfish is a wow fact; the Latin name hints a little but the reveal pays off.

### sea_042 (closestValue, hard): 72 GOOD, KEEP

- RU: На какой глубине лежат обломки «Титаника»?
- EN: How deep does the wreck of the Titanic lie?
- Answer: **3800** m, range 0 to 10000, tolerance 700
- Scores: F 5, C 3, G 3, D 4, E 3, A 5, P 3
- Verdict: Everyone knows Titanic; estimable depth with a fair range.

### sea_041 (closestValue, hard): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Какова глубина Марианской впадины в самой глубокой точке?
- EN: How deep is the Mariana Trench at its deepest point?
- Answer: **10900** m, range 2000 to 14000, tolerance 800
- Scores: F 5, C 2, G 4, D 3, E 2, A 5, P 4. Flags: REPETITIVE
- Verdict: Many know "about 11 km", the range is loose, and it repeats sea_004 (also the Mariana Trench) in the same topic; the Everest image is the payoff.
- Rewrite (71 GOOD): RU: Какова глубина Марианской впадины в самой глубокой точке, в метрах? / EN: How deep is the Mariana Trench at its deepest point, in metres?
  - Answer 10900, range 5000 to 15000, tolerance 600
  - Why better: A tighter range and tolerance make the guess a real estimate; with sea_004 dropped the trench appears less often.

## dinosaurs

### dinosaurs_015 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Кем на самом деле был вымерший тасманийский тигр?
- EN: What was the extinct Tasmanian tiger really?
- Options: **Сумчатым хищником | A marsupial predator** / Дикой кошкой | A wild cat / Полосатой гиеной | A striped hyena / Одичавшей собакой | A feral dog
- Scores: F 3, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: Most think it was a dog or cat; "marsupial" is a good reveal.

### dinosaurs_013 (multipleChoice, easy): 67 NEEDS IMPROVEMENT, KEEP

- RU: Что такое янтарь?
- EN: What is amber?
- Options: **Окаменевшая смола деревьев | Fossilised tree resin** / Застывшая лава | Cooled lava / Окаменевший мёд | Fossilised honey / Кристаллы морской соли | Sea salt crystals
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 5. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition question; the feathered dinosaur tail in amber is an excellent payoff.

### dinosaurs_010 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Где чаще всего находят мамонтов с сохранившейся шерстью и мясом?
- EN: Where are mammoths with their fur and flesh still intact usually found?
- Options: **В Сибири | In Siberia** / В Сахаре | In the Sahara / В джунглях Амазонки | In the Amazon jungle / В Австралии | In Australia
- Scores: F 5, C 1, G 5, D 1, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition for a Russian family; Sahara and Amazon are throwaways.
- Rewrite (71 GOOD): RU: Где чаще всего находят мамонтов с сохранившейся шерстью и мясом? / EN: Where are mammoths with their fur and flesh still intact usually found?
  - Options: **В Сибири | In Siberia** / В Гренландии | In Greenland / В Альпах | In the Alps / В Скандинавии | In Scandinavia
  - Why better: Cold places as distractors make players think instead of discarding three deserts and jungles.

### dinosaurs_024 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: Какое вымирание было самым массовым в истории Земли?
- EN: Which was the biggest mass extinction in Earth's history?
- Options: **Пермское, 252 млн лет назад | The Permian, 252 million years ago** / Гибель динозавров | The end of the dinosaurs / Конец ледникового периода | The end of the Ice Age / Триасовое, 201 млн лет назад | The Triassic, 201 million years ago
- Scores: F 3, C 4, G 2, D 3, E 2, A 4, P 4
- Verdict: The dinosaur trap is good and the Siberian volcano link is great for this family, but the dated options read like a geology exam.
- Rewrite (68 NEEDS IMPROVEMENT): RU: Какое вымирание погубило больше всего видов за всю историю Земли? / EN: Which extinction killed the most species in Earth's history?
  - Options: **Пермское | The Permian** / Гибель динозавров | The dinosaurs' end / Конец ледникового периода | The end of the Ice Age / Триасовое | The Triassic
  - Why better: Without dates the options read evenly and the dinosaur trap pulls harder.

### dinosaurs_031 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Что такое копролит?
- EN: What is a coprolite?
- Options: **Окаменевший помёт | Fossilised dung** / Яйцо динозавра | A dinosaur egg / Камень из желудка ящера | A stomach stone / Вид янтаря | A kind of amber
- Scores: F 3, C 3, G 3, D 4, E 4, A 5, P 4
- Verdict: Poop is always a crowd-pleaser and the copro- root gives a reasoning path; bone shards in T. rex dung are a good payoff.

### dinosaurs_021 (multipleChoice, medium): 62 NEEDS IMPROVEMENT, IMPROVE

- RU: Кем саблезубый тигр смилодон приходился настоящему тигру?
- EN: How was the sabre toothed tiger Smilodon related to the real tiger?
- Options: **Дальней роднёй | A distant cousin** / Прямым предком | Its direct ancestor / Тем же видом | The same species / Родным братом льва | The lion's brother
- Scores: F 4, C 3, G 3, D 2, E 2, A 5, P 3
- Verdict: "The same species" and "the lion's brother" are weak, and the reveal is a dry taxonomy point.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Саблезубый «тигр» смилодон — кем он приходился настоящему тигру? / EN: The sabre-toothed "tiger" Smilodon: what was it to a real tiger?
  - Options: **Дальней роднёй | A distant cousin** / Прямым предком | Its direct ancestor / Тем же видом | The same species / Предком льва | The lion's ancestor
  - Why better: Quoting "tiger" hints the name misleads, inviting a guess, and the lion option is a cleaner trap.

### dinosaurs_033 (multipleChoice, medium): 62 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой хищный динозавр был длиннее тираннозавра и ловил рыбу?
- EN: Which meat eating dinosaur was longer than T. rex and caught fish?
- Options: **Спинозавр | Spinosaurus** / Аллозавр | Allosaurus / Велоцираптор | Velociraptor / Компсогнат | Compsognathus
- Scores: F 3, C 3, G 2, D 4, E 3, A 5, P 3
- Verdict: Jurassic Park fans may know it, but the small dinosaurs are easy to discard and the fact repeats the question.
- Rewrite (70 GOOD): RU: Какой хищный динозавр с парусом на спине был длиннее тираннозавра и ловил рыбу? / EN: Which meat-eating dinosaur with a sail on its back was longer than T. rex and caught fish?
  - Options: **Спинозавр | Spinosaurus** / Аллозавр | Allosaurus / Велоцираптор | Velociraptor / Компсогнат | Compsognathus
  - Why better: The sail clue lets anyone who saw Jurassic Park III recognise it.

### dinosaurs_018 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Кто ближайшие живые родственники птицы додо?
- EN: Who are the dodo's closest living relatives?
- Options: **Голуби | Pigeons** / Страусы | Ostriches / Индюки | Turkeys / Пингвины | Penguins
- Scores: F 4, C 5, G 2, D 4, E 4, A 5, P 4
- Verdict: A dodo being a giant pigeon surprises everyone.

### dinosaurs_049 (closestValue, hard): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: Каким был размах крыльев кецалькоатля, одного из крупнейших летающих ящеров?
- EN: What was the wingspan of Quetzalcoatlus, one of the biggest flying reptiles?
- Answer: **10.5** m, range 0 to 40, tolerance 3.5
- Scores: F 2, C 4, G 2, D 3, E 3, A 4, P 4. Flags: OBSCURE_TRIVIA
- Verdict: The species name is unknown to most; the giraffe and small plane images are the payoff.
- Rewrite (73 GOOD): RU: Каким был размах крыльев самых больших летающих ящеров, в метрах? / EN: What was the wingspan of the biggest flying reptiles, in metres?
  - Answer 10.5, range 0 to 30, tolerance 3
  - Why better: Dropping the unknown name and tightening the range turns it into an estimate anyone can try.

### dinosaurs_048 (closestValue, hard): 77 GOOD, KEEP

- RU: Какой длины самый большой найденный зуб тираннозавра вместе с корнем?
- EN: How long is the biggest T. rex tooth ever found, root included?
- Answer: **30** cm, range 0 to 150, tolerance 8
- Scores: F 5, C 4, G 3, D 3, E 4, A 5, P 3
- Verdict: Easy to picture and estimate; the banana comparison helps.

## earth

### earth_014 (multipleChoice, easy): 58 NEEDS IMPROVEMENT, KEEP

- RU: Как называют расплавленную породу, пока она ещё под землёй?
- EN: What is molten rock called while it is still underground?
- Options: **Магма | Magma** / Лава | Lava / Пемза | Pumice / Базальт | Basalt
- Scores: F 5, C 1, G 3, D 4, E 1, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: Deliberate easy recognition question; the lava trap works but there is no surprise.

### earth_001 (multipleChoice, easy): 68 NEEDS IMPROVEMENT, KEEP

- RU: Почему молнию мы видим раньше, чем слышим гром?
- EN: Why do we see lightning before we hear the thunder?
- Options: **Свет быстрее звука | Light is faster than sound** / Гром рождается позже молнии | Thunder is made later / Гром стесняется | Thunder is shy / Глаза быстрее ушей | Eyes react faster than ears
- Scores: F 5, C 1, G 5, D 1, E 3, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition question with a joke option; the count-to-three tip is useful.

### earth_011 (multipleChoice, easy): 71 GOOD, KEEP

- RU: Какой камень может плавать в воде?
- EN: Which rock can float on water?
- Options: **Пемза | Pumice** / Гранит | Granite / Мрамор | Marble / Базальт | Basalt
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Many have seen pumice in a bathroom; drifting pumice rafts are a nice payoff.

### earth_032 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Где произошло самое сильное землетрясение за всю историю измерений?
- EN: Where was the strongest earthquake ever recorded?
- Options: **Чили, 1960 | Chile, 1960** / Япония, 2011 | Japan, 2011 / Аляска, 1964 | Alaska, 1964 / Индонезия, 2004 | Indonesia, 2004
- Scores: F 4, C 4, G 2, D 5, E 3, A 5, P 3
- Verdict: Japan 2011 and Indonesia 2004 are strong traps from memory; Chile is a good surprise.

### earth_034 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Чем ураган отличается от тайфуна?
- EN: What is the difference between a hurricane and a typhoon?
- Options: **Только местом, где он бушует | Only where it happens** / Силой ветра | Wind strength / Временем года | Time of year / Направлением вращения | Direction of spin
- Scores: F 5, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: A word difference many never questioned; tidy reveal with the cyclone extra.

### earth_037 (multipleChoice, medium): 77 GOOD, KEEP

- RU: Какую форму на самом деле имеет крупная падающая капля дождя?
- EN: What shape is a large falling raindrop really?
- Options: **Булочки, сплюснутой снизу | A bun, flat underneath** / Слезы | A teardrop / Длинной иглы | A long needle / Кубика | A little cube
- Scores: F 5, C 5, G 2, D 3, E 4, A 4, P 4
- Verdict: The teardrop myth is busted by a bun; great, but the correct option is longest.
- Rewrite (81 GOOD): RU: Какую форму на самом деле имеет крупная падающая капля дождя? / EN: What shape is a large falling raindrop really?
  - Options: **Булочки | A bun** / Слезинки | A teardrop / Иголки | A needle / Пирамидки | A pyramid
  - Why better: One-word options remove the length tell.

### earth_038 (multipleChoice, medium): 77 GOOD, KEEP

- RU: В 2010 году в США упала градина-рекордсменка. Какого она была размера?
- EN: A record-breaking hailstone fell in the USA in 2010. How big was it?
- Options: С вишню | Like a cherry / С грецкий орех | Like a walnut / С теннисный мяч | Like a tennis ball / **С волейбольный мяч | Like a volleyball**
- Scores: F 5, C 4, G 3, D 2, E 4, A 5, P 4
- Verdict: Hail the size of a volleyball is vivid, though "record" telegraphs the biggest option.

### earth_022 (multipleChoice, medium): 52 REJECT, REPLACE

- RU: Что измеряет шкала Бофорта?
- EN: What does the Beaufort scale measure?
- Options: **Силу ветра | Wind strength** / Облачность | Cloud cover / Количество осадков | Amount of rain / Твёрдость минералов | Hardness of minerals
- Scores: F 3, C 2, G 2, D 4, E 1, A 5, P 3. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: A definition question with an unknown name; nobody reacts to "wind strength".

### earth_039 (closestValue, hard): 76 GOOD, KEEP

- RU: Сколько градусов мороза зафиксировали на станции «Восток» в Антарктиде в 1983 году?
- EN: How many degrees below zero (°C) were recorded at Vostok Station in Antarctica in 1983?
- Answer: **89** °C below zero, range 0 to 150, tolerance 10
- Scores: F 5, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: Russians know "minus 89" and the dry ice comparison is vivid.

### earth_044 (closestValue, hard): 55 NEEDS IMPROVEMENT, REPLACE

- RU: Какой температуры свежая базальтовая лава, как на Гавайях?
- EN: How hot is fresh basalt lava, like that of Hawaii?
- Answer: **1150** °C, range 0 to 3000, tolerance 250
- Scores: F 3, C 2, G 3, D 3, E 2, A 5, P 2. Flags: BORING_REVEAL
- Verdict: A bare temperature with a fact about lava names that has nothing to do with the answer.

## plants

### plants_015 (multipleChoice, easy): 71 GOOD, KEEP

- RU: Какое хищное растение можно найти на болотах России?
- EN: Which carnivorous plant grows wild in Russian bogs?
- Options: **Росянка | The sundew** / Венерина мухоловка | The Venus flytrap / Непентес | The tropical pitcher plant / Кувшинка | The water lily
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Good for a Russian family; the famous Venus flytrap is a fair trap.

### plants_002 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: Зачем венерина мухоловка ловит насекомых?
- EN: Why does a Venus flytrap catch insects?
- Options: **Почва, где она растёт, бедна азотом | Its soil is poor in nitrogen** / У неё нет листьев для фотосинтеза | It has no leaves to photosynthesise / Так она защищает свои цветы | To protect its flowers / Из насекомых она берёт воду | To get water from them
- Scores: F 4, C 3, G 3, D 4, E 2, A 4, P 3
- Verdict: Reasonable deduction, but the options are long and it feels like a biology lesson; the rewrite is only a partial fix.
- Rewrite (66 NEEDS IMPROVEMENT): RU: Зачем венерина мухоловка ловит насекомых? / EN: Why does a Venus flytrap catch insects?
  - Options: **Почва бедна азотом | Its soil lacks nitrogen** / У неё нет листьев | It has no leaves / Так она охраняет цветы | To guard its flowers / Ей нужна их вода | It needs their water
  - Why better: Shorter options read in time and keep the "it still makes its own sugar" reveal.

### plants_006 (multipleChoice, easy): 76 GOOD, KEEP

- RU: Где созревает арахис?
- EN: Where do peanuts ripen?
- Options: **Под землёй | Underground** / На кустах | On bushes / На деревьях | On trees / Под водой | Under water
- Scores: F 5, C 3, G 4, D 3, E 3, A 5, P 4
- Verdict: Many have never pictured peanuts growing; the burying flower is a nice payoff.

### plants_023 (multipleChoice, medium): 81 GOOD, KEEP

- RU: Куда смотрят взрослые цветущие подсолнухи?
- EN: Which way do mature sunflowers in bloom face?
- Options: **На восток, весь день | East, all day** / Вслед за солнцем весь день | They follow the sun all day / На запад, весь день | West, all day / Каждый куда придётся | Each one any way at all
- Scores: F 5, C 5, G 2, D 4, E 4, A 4, P 5
- Verdict: Everyone "knows" sunflowers follow the sun; the east-facing truth is a great reversal.

### plants_026 (multipleChoice, medium): 77 GOOD, KEEP

- RU: Какого цвета была морковь, когда её только начали выращивать?
- EN: What colour were the first cultivated carrots?
- Options: **Фиолетовой или жёлтой | Purple or yellow** / Оранжевой | Orange / Зелёной | Green / Голубой | Blue
- Scores: F 5, C 4, G 3, D 2, E 4, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Great fact, but the correct option is the only two-part answer (telegraphed) and "Blue" is a throwaway.
- Rewrite (81 GOOD): RU: Какого цвета была морковь, когда её только начали выращивать? / EN: What colour were the first cultivated carrots?
  - Options: **Фиолетовой | Purple** / Оранжевой | Orange / Белой | White / Красной | Red
  - Why better: Single colours remove the two-part tell and every option sounds plausible.

### plants_030 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько листьев за всю жизнь вырастает у вельвичии из пустыни Намиб?
- EN: How many leaves does Welwitschia of the Namib desert grow in its whole life?
- Options: **Два | Two** / Ни одного | None / Около тысячи | About a thousand / Новые каждую весну | New ones every spring
- Scores: F 2, C 5, G 2, D 3, E 3, A 5, P 5
- Verdict: Two leaves for a thousand years is a wow, but the plant name is unknown and "Два" stands out as the only small number.
- Rewrite (72 GOOD): RU: Сколько листьев за всю жизнь вырастает у вельвичии, растения пустыни, живущего тысячу лет? / EN: How many leaves does Welwitschia, a desert plant that lives 1,000 years, grow in its whole life?
  - Options: **Два | Two** / Ни одного | None / Около сотни | About a hundred / Несколько тысяч | Several thousand
  - Why better: The thousand-year lifespan sharpens the paradox and the number options are evenly spread.

### plants_028 (multipleChoice, medium): 80 GOOD, KEEP

- RU: Кто ближайший родственник помидора из этого списка?
- EN: Which of these is the tomato's closest relative?
- Options: **Картофель | Potato** / Огурец | Cucumber / Арбуз | Watermelon / Морковь | Carrot
- Scores: F 5, C 4, G 3, D 4, E 3, A 5, P 5
- Verdict: Tomato and potato as siblings surprises, and the graft fact is fun; the similar-to flag (sea_028) is only the "closest relative" formula.

### plants_033 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Чем тупайи полезны хищному непентесу Лоу с Борнео?
- EN: How do tree shrews help the carnivorous pitcher plant Nepenthes lowii of Borneo?
- Options: **Оставляют в кувшине помёт | They leave droppings in it** / Опыляют его цветки | They pollinate its flowers / Разносят его семена | They spread its seeds / Загоняют в него насекомых | They chase insects into it
- Scores: F 1, C 5, G 2, D 4, E 4, A 3, P 5. Flags: OBSCURE_TRIVIA, TOO_WORDY
- Verdict: A plant using a shrew as a toilet is hilarious, but two obscure names make the question hard to read.
- Rewrite (82 GOOD): RU: Зверёк тупайя лижет сладкое с крышки хищного растения-кувшина. Чем он ему полезен? / EN: A tree shrew licks sweet stuff off a carnivorous pitcher plant's lid. How does it help the plant?
  - Options: **Оставляет в нём помёт | It leaves droppings in it** / Опыляет его цветки | It pollinates its flowers / Разносит его семена | It spreads its seeds / Загоняет в него мух | It herds flies into it
  - Why better: A vivid scene replaces the Latin name, so the toilet punchline lands for everyone.

### plants_047 (closestValue, hard): 56 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году в Голландии рухнули цены в разгар «тюльпаномании»?
- EN: In what year did the Dutch 'tulip mania' collapse?
- Answer: **1637** year, range 1500 to 1950, tolerance 25
- Scores: F 2, C 2, G 3, D 3, E 2, A 5, P 4. Flags: SCHOOL_EXAM
- Verdict: Tulip mania is a great story, but the question is a bare year.
- Rewrite (66 NEEDS IMPROVEMENT): RU: В каком году в Голландии лопнул тюльпанный пузырь, один из первых в истории? / EN: In what year did the Dutch tulip bubble, one of history's first, burst?
  - Answer 1637, range 1500 to 1950, tolerance 25
  - Why better: Calling it a bubble connects it to modern crashes and gives players an era to reason from.

### plants_045 (closestValue, hard): 60 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько лет назад индейцы Мексики одомашнили кукурузу?
- EN: About how many years ago was maize domesticated in Mexico?
- Answer: **9000** years, range 0 to 30000, tolerance 2000
- Scores: F 4, C 3, G 2, D 3, E 2, A 4, P 4. Flags: NO_DEDUCTION
- Verdict: Thousands of years is hard to place on a 0-30,000 slider with no reference point.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Пирамидам Египта около 4500 лет. Сколько лет назад в Мексике одомашнили кукурузу? / EN: The pyramids of Egypt are about 4,500 years old. How many years ago was maize domesticated in Mexico?
  - Answer 9000, range 0 to 30000, tolerance 2000
  - Why better: A familiar ancient landmark gives players a yardstick.

## world

### world_009 (multipleChoice, easy): 62 NEEDS IMPROVEMENT, KEEP

- RU: Ниагарский водопад стоит на границе США и какой страны?
- EN: Niagara Falls sit on the border of the USA and which country?
- Options: **Канады | Canada** / Мексики | Mexico / Кубы | Cuba / Гренландии | Greenland
- Scores: F 5, C 1, G 5, D 1, E 1, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition question; Cuba and Greenland are throwaways, but the Horseshoe fact is a nice payoff.

### world_011 (multipleChoice, easy): 69 NEEDS IMPROVEMENT, KEEP

- RU: С какой страной Аляска соседствует через Берингов пролив?
- EN: Across the Bering Strait, which country is Alaska's neighbour?
- Options: **С Россией | Russia** / С Японией | Japan / С Канадой | Canada / С Китаем | China
- Scores: F 5, C 1, G 5, D 2, E 2, A 5, P 5. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition for a Russian family; the islands a day apart are an excellent payoff.

### world_008 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Крошечное Монако с трёх сторон окружено какой страной?
- EN: Tiny Monaco is surrounded on three sides by which country?
- Options: **Францией | France** / Италией | Italy / Испанией | Spain / Швейцарией | Switzerland
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition question; the Sokolniki comparison is a fun reveal.

### world_027 (multipleChoice, medium): 82 GOOD, KEEP

- RU: Кто порой оказывается ближе всех к точке Немо — самому удалённому от суши месту океана?
- EN: Who are sometimes the closest people to Point Nemo, the ocean spot farthest from land?
- Options: **Космонавты на МКС | Astronauts on the ISS** / Смотрители маяка | Lighthouse keepers / Полярники Антарктиды | Antarctic scientists / Жители острова Пасхи | Easter Islanders
- Scores: F 4, C 5, G 3, D 4, E 4, A 5, P 4
- Verdict: The nearest humans are in space: a perfect "wait, seriously?".

### world_021 (multipleChoice, medium): 70 GOOD, KEEP

- RU: В городке Барле граница проходит прямо через дома и кафе. Каких двух стран?
- EN: In the town of Baarle the border runs through houses and cafés. Between which two countries?
- Options: **Бельгии и Нидерландов | Belgium and the Netherlands** / Франции и Испании | France and Spain / Германии и Австрии | Germany and Austria / Швеции и Норвегии | Sweden and Norway
- Scores: F 3, C 4, G 2, D 4, E 3, A 5, P 5
- Verdict: A door split by a border is delightful; the guess is mostly luck.

### world_032 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Какая страна — самая большая в мире, где нет ни одной постоянной реки?
- EN: Which is the largest country in the world with no permanent river?
- Options: **Саудовская Аравия | Saudi Arabia** / Египет | Egypt / Монголия | Mongolia / Австралия | Australia
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 3
- Verdict: Egypt and Australia are good traps; the wadis fact is solid.

### world_037 (multipleChoice, medium): 77 GOOD, KEEP

- RU: В какой стране есть настоящий город Батман?
- EN: Which country has a real city called Batman?
- Options: **В Турции | Turkey** / В Бразилии | Brazil / В Индии | India / В Норвегии | Norway
- Scores: F 4, C 4, G 2, D 4, E 5, A 5, P 4
- Verdict: A real city called Batman gets a laugh and the river explanation lands.

### world_036 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Где на самом деле южная оконечность Африки?
- EN: Where is the real southern tip of Africa?
- Options: **Мыс Игольный | Cape Agulhas** / Мыс Доброй Надежды | Cape of Good Hope / Мыс Горн | Cape Horn / Мыс Гвардафуй | Cape Guardafui
- Scores: F 3, C 4, G 3, D 3, E 3, A 5, P 3
- Verdict: Good of Hope trap, but Cape Horn is in South America and is easy to discard.
- Rewrite (70 GOOD): RU: Где на самом деле южная оконечность Африки? / EN: Where is the real southern tip of Africa?
  - Options: **Мыс Игольный | Cape Agulhas** / Мыс Доброй Надежды | Cape of Good Hope / Мыс Альмади | Cape Almadi / Мыс Гвардафуй | Cape Guardafui
  - Why better: All four capes are now African, so geography knowledge rather than elimination decides.

### world_044 (closestValue, hard): 71 GOOD, KEEP

- RU: Какова ширина Гибралтарского пролива между Европой и Африкой в самом узком месте, в километрах?
- EN: How wide is the Strait of Gibraltar between Europe and Africa at its narrowest, in km?
- Answer: **14** km, range 0 to 100, tolerance 8
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Estimable and the "see Morocco from Spain" image is nice.

### world_047 (closestValue, hard): 80 GOOD, KEEP

- RU: Какую долю всей суши Земли занимает Россия, в процентах?
- EN: What share of all the Earth's land does Russia cover, in percent?
- Answer: **11** percent, range 0 to 60, tolerance 7
- Scores: F 5, C 4, G 3, D 4, E 3, A 5, P 5. Flags: REPETITIVE
- Verdict: Estimable via "one sixth of the land" and the Pluto comparison is superb, but it repeats ussr_044 (USSR share of land); do not play both in one evening.

## countries

### countries_002 (multipleChoice, easy): 76 GOOD, KEEP

- RU: Какой город — столица Австралии?
- EN: Which city is the capital of Australia?
- Options: Сидней | Sydney / Мельбурн | Melbourne / **Канберра | Canberra** / Перт | Perth
- Scores: F 5, C 3, G 3, D 5, E 3, A 5, P 4
- Verdict: Classic Sydney/Melbourne trap that still catches people, and the fact explains why the capital is a compromise city. Many adults already know Canberra, so the surprise is modest.

### countries_010 (multipleChoice, easy): 72 GOOD, KEEP

- RU: В какой стране овец больше, чем людей?
- EN: In which country do sheep outnumber people?
- Options: **Новая Зеландия | New Zealand** / Нидерланды | The Netherlands / Япония | Japan / Канада | Canada
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 4
- Verdict: Easy and guessable (New Zealand is the famous sheep country); Japan and the Netherlands are weak decoys. The 'twenty sheep per person' fact gives a good little payoff.

### countries_013 (multipleChoice, easy): 62 NEEDS IMPROVEMENT, IMPROVE

- RU: Что означает название «Австралия»?
- EN: What does the name 'Australia' mean?
- Options: **Южная земля | Southern land** / Великий остров | Great island / Красная земля | Red land / Страна кенгуру | Kangaroo land
- Scores: F 4, C 2, G 4, D 2, E 2, A 5, P 3
- Verdict: The Latin 'australis' makes it guessable, but 'Kangaroo land' is a throwaway and the Austria fact has no decoy to connect to. Making 'Eastern land' (Austria's real meaning) a distractor turns the fact into a payoff.
- Rewrite (71 GOOD): RU: Что означает название «Австралия»? / EN: What does the name 'Australia' mean?
  - Options: **Южная земля | Southern land** / Восточная земля | Eastern land / Красная земля | Red land / Новая земля | New land
  - Why better: Four parallel 'X land' options remove the throwaway, and the 'Eastern land' decoy is exactly Austria's meaning, so the fact lands as a twist.

### countries_017 (multipleChoice, medium): 70 GOOD, KEEP

- RU: На флаге какой страны изображён дракон?
- EN: Which country has a dragon on its flag?
- Options: **Бутан | Bhutan** / Китай | China / Вьетнам | Vietnam / Монголия | Mongolia
- Scores: F 3, C 3, G 3, D 5, E 3, A 5, P 4
- Verdict: China is a strong trap (dragons are Chinese, but its flag has none) and all distractors are credible Asian countries. The thunder-dragon fact gives a nice image.

### countries_016 (multipleChoice, medium): 71 GOOD, KEEP

- RU: У какой страны единственный в мире непрямоугольный флаг?
- EN: Which country has the world's only non-rectangular national flag?
- Options: **Непал | Nepal** / Бутан | Bhutan / Шри-Ланка | Sri Lanka / Монголия | Mongolia
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Nepal's two-pennant flag is visually memorable, so players feel they know it; the constitution-as-geometry-manual fact is a good tell-someone payoff.

### countries_018 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, REPLACE

- RU: На флаге какой страны изображён храм?
- EN: Which country's flag shows a temple?
- Options: **Камбоджа | Cambodia** / Индия | India / Греция | Greece / Египет | Egypt
- Scores: F 3, C 2, G 4, D 3, E 2, A 5, P 3. Flags: REPETITIVE
- Verdict: Third 'which country's flag shows X' question in this topic (with dragon and Nepal). Anyone who links Angkor Wat to Cambodia gets it, and Greece/Egypt are eliminated by knowing their flags; the fact restates the answer.

### countries_027 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Название какой страны придумали как сокращение из названий её областей?
- EN: Which country's name was invented as an acronym of its regions?
- Options: **Пакистан | Pakistan** / Бангладеш | Bangladesh / Афганистан | Afghanistan / Шри-Ланка | Sri Lanka
- Scores: F 4, C 4, G 3, D 4, E 3, A 4, P 4
- Verdict: Nice surprise that a country name was invented by a student as an acronym; Afghanistan and Bangladesh are credible 'stan/desh' decoys. Fact lists the regions, which is the payoff.

### countries_032 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Символа какой части Великобритании нет на её флаге?
- EN: Which nation of the UK has no symbol on the Union Jack?
- Options: **Уэльса | Wales** / Шотландии | Scotland / Англии | England / Северной Ирландии | Northern Ireland
- Scores: F 4, C 4, G 3, D 5, E 4, A 4, P 4
- Verdict: Everyone can picture the Union Jack and the Welsh dragon is famous, so the 'Wales is missing' reveal starts a discussion. All four nations are credible options.

### countries_046 (closestValue, hard): 59 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году у Канады появился флаг с кленовым листом?
- EN: In what year did Canada get its maple leaf flag?
- Answer: **1965** year, range 1800 to 2020, tolerance 20
- Scores: F 3, C 3, G 3, D 3, E 2, A 5, P 2. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: The surprise (the maple leaf is only from 1965) is hidden behind a bare date question, and the fact barely adds anything. Framing it around how long the Union Jack stayed on the flag makes the late date the point.
- Rewrite (66 NEEDS IMPROVEMENT): RU: До какого года на флаге Канады в углу был британский «Юнион Джек»? / EN: Until what year did Canada's flag carry the British Union Jack in the corner?
  - Answer 1965, range 1850 to 2020, tolerance 15
  - Why better: Asking how long Canada kept the British flag invites a 'surely long ago' guess, so the 1965 answer surprises instead of just being a date.

### countries_049 (closestValue, hard): 45 REJECT, REPLACE

- RU: В каком году Нигерия перенесла столицу из Лагоса в Абуджу?
- EN: In what year did Nigeria move its capital from Lagos to Abuja?
- Answer: **1991** year, range 1900 to 2020, tolerance 15
- Scores: F 2, C 2, G 2, D 2, E 1, A 5, P 3. Flags: OBSCURE_TRIVIA, SCHOOL_EXAM, NO_DEDUCTION
- Verdict: An arbitrary relocation year for a capital most of the family cannot place; there is nothing to reason from and nobody will care about the number. The neutral-ground fact is mildly interesting but not enough.

## ru_places

### ru_places_010 (multipleChoice, easy): 61 NEEDS IMPROVEMENT, KEEP

- RU: На какой реке стоит Новосибирск?
- EN: Which river is Novosibirsk on?
- Options: **Обь | The Ob** / Енисей | The Yenisei / Иртыш | The Irtysh / Лена | The Lena
- Scores: F 5, C 1, G 3, D 4, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: Deliberate easy recognition question for a Russian family; zero surprise, but the bridge-builders' camp origin is a decent small payoff. Fine as a warm-up.

### ru_places_007 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: В какое море впадает Волга?
- EN: Into which sea does the Volga flow?
- Options: В Чёрное | The Black Sea / В Азовское | The Sea of Azov / **В Каспийское | The Caspian Sea** / В Балтийское | The Baltic Sea
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 4. Flags: SCHOOL_EXAM, OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition (school geography); the fact that the Caspian is a lake, so Europe's longest river never reaches the ocean, rescues it. One of four river questions in this topic.

### ru_places_008 (multipleChoice, easy): 67 NEEDS IMPROVEMENT, KEEP

- RU: Как назывался Калининград до 1946 года?
- EN: What was Kaliningrad called before 1946?
- Options: Данциг | Danzig / **Кёнигсберг | Königsberg** / Мемель | Memel / Тильзит | Tilsit
- Scores: F 5, C 2, G 3, D 5, E 2, A 5, P 3
- Verdict: Deliberate easy recognition: most know Königsberg, but the decoys are all real Prussian cities, which makes it feel fair. Kant fact is familiar rather than surprising.

### ru_places_024 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Какой город России считается самым древним — ему больше 2000 лет?
- EN: Which Russian city is considered the oldest, at over 2,000 years old?
- Options: Великий Новгород | Veliky Novgorod / Казань | Kazan / **Дербент | Derbent** / Старая Ладога | Staraya Ladoga
- Scores: F 3, C 4, G 3, D 4, E 3, A 5, P 3
- Verdict: Good trap: most will pick Novgorod or Staraya Ladoga, and Derbent being over 2,000 years old surprises. The 'closed gates' fact is a solid payoff.

### ru_places_021 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Какая река — единственная, что вытекает из Байкала?
- EN: Which river is the only one that flows out of Lake Baikal?
- Options: Селенга | The Selenga / Лена | The Lena / Енисей | The Yenisei / **Ангара | The Angara**
- Scores: F 4, C 2, G 3, D 4, E 3, A 5, P 4. Flags: SCHOOL_EXAM, REPETITIVE
- Verdict: Known school fact (Angara is the only outflow) and one of four river questions; the Yenisei decoy is clever. The legend in the fact is the best part, so lead with it.
- Rewrite (74 GOOD): RU: По легенде, у Байкала была дочь-река, сбежавшая к Енисею. Как её звали? / EN: Legend says Baikal had a river daughter who ran off to the Yenisei. What was her name?
  - Options: **Ангара | Angara** / Селенга | Selenga / Лена | Lena / Бирюса | Biryusa
  - Why better: The runaway-daughter legend turns a geography fact into a story, and players can still reason that the river leaving Baikal flows to the Yenisei.

### ru_places_031 (multipleChoice, medium): 60 NEEDS IMPROVEMENT, REPLACE

- RU: Какая река России самая полноводная?
- EN: Which river in Russia carries the most water?
- Options: Волга | The Volga / Обь | The Ob / Лена | The Lena / **Енисей | The Yenisei**
- Scores: F 4, C 3, G 2, D 4, E 2, A 5, P 2. Flags: REPETITIVE, BORING_REVEAL, SCHOOL_EXAM
- Verdict: Fourth river question in the topic; the Volga trap is decent but the reveal is a dry geography lecture about Siberian plains. Nothing to tell anyone afterwards.

### ru_places_033 (multipleChoice, medium): 75 GOOD, KEEP

- RU: В честь кого назван город Тольятти?
- EN: Who is the city of Tolyatti named after?
- Options: **Итальянского коммуниста | An Italian communist** / Инженера ВАЗа | A VAZ car engineer / Волжского купца | A Volga merchant / Героя войны 1812 года | A hero of the 1812 war
- Scores: F 4, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: The city name sounds Italian, which gives a real reasoning path, and 'a VAZ engineer' is a great trap given the Fiat link. Togliatti the communist is a satisfying reveal.

### ru_places_032 (multipleChoice, medium): 64 NEEDS IMPROVEMENT, REPLACE

- RU: Какой крупный город целиком стоит на вечной мерзлоте?
- EN: Which large city stands entirely on permafrost?
- Options: **Якутск | Yakutsk** / Иркутск | Irkutsk / Хабаровск | Khabarovsk / Новосибирск | Novosibirsk
- Scores: F 4, C 2, G 4, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Solved by picking the coldest, most northern city in the list, so there is no real choice, and the stilts fact is fairly well known. Northern decoys such as Norilsk would also sit on permafrost and make it ambiguous, so it cannot be fixed with the same fact.

### ru_places_045 (closestValue, hard): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько глав (куполов) у Преображенской церкви в Кижах?
- EN: How many domes does the Church of the Transfiguration at Kizhi have?
- Answer: **22** , range 1 to 70, tolerance 7
- Scores: F 3, C 3, G 3, D 3, E 2, A 5, P 3
- Verdict: People have seen Kizhi on postcards, so estimating domes from memory is possible, but the framing is dry. Inviting players to picture the postcard and tightening the range makes it a visual-memory game.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Вспомните открытку с Кижами: сколько куполов у Преображенской церкви? / EN: Picture the Kizhi postcard: how many domes does the Church of the Transfiguration have?
  - Answer 22, range 1 to 50, tolerance 5
  - Why better: Invites everyone to picture a familiar image and count, which turns a memorised number into a visual estimate.

### ru_places_039 (closestValue, hard): 74 GOOD, KEEP

- RU: Какова наибольшая глубина Байкала?
- EN: What is the greatest depth of Lake Baikal?
- Answer: **1642** m, range 200 to 2600, tolerance 150
- Scores: F 5, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Everyone knows Baikal is the deepest lake, so the guess starts from real knowledge; the fair range and tolerance reward it. Three stacked Ostankino towers is a vivid payoff.

## languages

### languages_006 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Какой из этих языков пишут справа налево?
- EN: Which of these languages is written right to left?
- Options: **Арабский | Arabic** / Хинди | Hindi / Грузинский | Georgian / Греческий | Greek
- Scores: F 5, C 1, G 4, D 3, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition: almost everyone knows Arabic goes right to left. The numbers-run-left-to-right fact is a genuinely nice twist.

### languages_005 (multipleChoice, easy): 74 GOOD, KEEP

- RU: Из какого языка пришло слово «бутерброд»?
- EN: Which language gave Russian the word 'buterbrod' (open sandwich)?
- Options: **Из немецкого | German** / Из французского | French / Из английского | English / Из итальянского | Italian
- Scores: F 5, C 2, G 4, D 4, E 3, A 5, P 4
- Verdict: Guessable from 'Butter' + 'Brot', and the 'buterbrod with butter says butter twice' line gets a laugh at the table.

### languages_007 (multipleChoice, easy): 77 GOOD, KEEP

- RU: Закончите знаменитый палиндром: «А роза упала на лапу…»
- EN: Finish the famous Russian palindrome: 'A roza upala na lapu…' (a rose fell on the paw of…)
- Options: **Азора | Azor** / Барбоса | Barbos / Шарика | Sharik / Тузика | Tuzik
- Scores: F 5, C 2, G 4, D 4, E 4, A 5, P 4
- Verdict: Everyone knows the phrase, and those who don't can solve it by reversing the letters, which is a fun group moment. The Malvina and Fet facts are a pleasant payoff.

### languages_018 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Откуда в русском взялось слово «вокзал»?
- EN: Where does the Russian word 'vokzal' (railway station) come from?
- Options: **От лондонского сада развлечений | A London pleasure garden** / От немецкого «зал ожидания» | German for 'waiting hall' / От фамилии инженера | An engineer's surname / От французского «путешествие» | French for 'journey'
- Scores: F 4, C 5, G 2, D 4, E 4, A 4, P 5
- Verdict: Real 'wait, seriously?' etymology: a Russian station named after a London pleasure garden. Decoys are believable; the Pavlovsk story makes a great reveal.

### languages_033 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Как выглядит вопросительный знак в греческом языке?
- EN: What does a question mark look like in Greek?
- Options: **Как наша точка с запятой | Like our semicolon** / Как перевёрнутый «?» | Like an upside down '?' / Как двоеточие | Like a colon / Как звёздочка | Like an asterisk
- Scores: F 3, C 5, G 2, D 4, E 4, A 4, P 4
- Verdict: Surprising and easy to picture; the Spanish upside-down '?' is a smart decoy. The raised-dot semicolon fact doubles the oddity.

### languages_029 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Откуда взялось выражение «не в своей тарелке»?
- EN: Where does the Russian idiom 'not in one's own plate' (feeling out of sorts) come from?
- Options: **Неточный перевод с французского | A mistranslation from French** / Боярский застольный обычай | A boyars' table custom / Морской жаргон | Sailors' slang / Поговорка поваров | A cooks' saying
- Scores: F 4, C 4, G 3, D 4, E 3, A 4, P 4
- Verdict: Everyone uses the idiom and nobody knows it is a mistranslation; 'assiette' meaning both plate and state of mind is a satisfying reveal. The NOT script flag is a false positive (the 'не' is part of the idiom).

### languages_022 (multipleChoice, medium): 70 GOOD, KEEP

- RU: Что ещё значит «babushka» в английском, кроме бабушки?
- EN: Besides 'grandmother', what does 'babushka' mean in English?
- Options: **Платок, завязанный под подбородком | A headscarf tied under the chin** / Кресло-качалка | A rocking chair / Вязаный свитер | A knitted sweater / Пуховая подушка | A down pillow
- Scores: F 4, C 4, G 3, D 2, E 4, A 4, P 3
- Verdict: Fun angle on how English borrowed a Russian word, but the correct option is far longer and more detailed than the rest. A short 'headscarf' option fixes the telegraph cheaply.
- Rewrite (76 GOOD): RU: Что ещё значит «babushka» в английском, кроме бабушки? / EN: Besides 'grandmother', what does 'babushka' mean in English?
  - Options: **Головной платок | A headscarf** / Кресло-качалка | A rocking chair / Вязаный свитер | A knitted sweater / Тёплые тапки | Warm slippers
  - Why better: All options are now short granny-style objects of equal length, so nothing gives the answer away.

### languages_034 (multipleChoice, medium): 82 GOOD, KEEP

- RU: Мы зовём знак @ «собакой». А как его называют итальянцы?
- EN: Russians call the @ sign 'the little dog'. What do Italians call it?
- Options: **Улитка | Snail** / Обезьяний хвост | Monkey's tail / Котёнок | Kitten / Хобот слона | Elephant's trunk
- Scores: F 4, C 4, G 3, D 5, E 4, A 5, P 5
- Verdict: Everyone knows the Russian 'sobaka', and the decoys are other countries' real names for @, which the fact then reveals. The snail shape gives an intuition path.

### languages_039 (closestValue, hard): 72 GOOD, KEEP

- RU: Сколько букв в гавайском алфавите?
- EN: How many letters are in the Hawaiian alphabet?
- Answer: **13** letters, range 0 to 60, tolerance 7
- Scores: F 3, C 4, G 3, D 4, E 3, A 5, P 4
- Verdict: Players can reason from 'aloha' and 'Honolulu' that the alphabet is tiny; 13 is a surprising number, and the fair tolerance rewards that logic.

### languages_040 (closestValue, hard): 62 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году реформа орфографии убрала из алфавита букву «ять»?
- EN: In what year did a spelling reform drop the old letter yat from Russian?
- Answer: **1918** year, range 1700 to 2000, tolerance 25
- Scores: F 4, C 2, G 4, D 2, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: Anyone who links the reform to the revolution gets it, and the ±25 tolerance makes it nearly free. Tightening the tolerance and naming all three letters makes it a real estimate.
- Rewrite (66 NEEDS IMPROVEMENT): RU: В каком году из русского алфавита исчезли «ять», «і» и «фита»? / EN: In what year did yat, i and fita disappear from the Russian alphabet?
  - Answer 1918, range 1700 to 2000, tolerance 5
  - Why better: A tight tolerance rewards players who place the reform right after the revolution instead of giving points for any 20th-century guess.

## holidays

### holidays_009 (multipleChoice, easy): 71 GOOD, KEEP

- RU: Какое главное угощение Масленицы?
- EN: What is the signature food of Maslenitsa?
- Options: **Блины | Pancakes (blini)** / Пельмени | Dumplings (pelmeni) / Кулич | Easter cake (kulich) / Окрошка | Cold soup (okroshka)
- Scores: F 5, C 1, G 5, D 3, E 2, A 5, P 5. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition: everyone answers blini. The myth-busting fact (the 'pancake as sun' idea was invented by 19th-century scholars) is a real payoff and will start an argument.

### holidays_006 (multipleChoice, easy): 76 GOOD, KEEP

- RU: За чем бегут вниз по крутому холму на ежегодном празднике в английском Глостершире?
- EN: What do people chase down a steep hill at a yearly festival in Gloucestershire, England?
- Options: **За кругом сыра | A wheel of cheese** / За поросёнком | A piglet / За бочкой | A barrel / За тыквой | A pumpkin
- Scores: F 3, C 4, G 3, D 4, E 5, A 4, P 4
- Verdict: Many have seen the viral cheese-rolling videos, the answer is funny, and the 'cheese is uncatchable' fact lands. Slightly long question but still readable.

### holidays_005 (multipleChoice, easy): 66 NEEDS IMPROVEMENT, KEEP

- RU: На каком индийском празднике все обсыпают друг друга цветным порошком?
- EN: At which Indian festival does everyone throw coloured powder at each other?
- Options: **Холи | Holi** / Дивали | Diwali / Навруз | Nowruz / Ханука | Hanukkah
- Scores: F 5, C 1, G 4, D 4, E 3, A 5, P 2. Flags: OBVIOUS_ANSWER, BORING_REVEAL
- Verdict: Deliberate easy recognition: Holi's colour powder is widely known. Decoys are real festivals, but the fact (welcomes spring, powder from flowers) is flat.

### holidays_025 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Какая страна каждый год дарит Лондону ёлку для Трафальгарской площади?
- EN: Which country gives London a Christmas tree for Trafalgar Square every year?
- Options: **Норвегия | Norway** / Канада | Canada / Финляндия | Finland / Германия | Germany
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Plausible decoys (Canada, Finland, Germany) and a warm thank-you story from WWII as payoff. Solid medium question.

### holidays_036 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: Из чего вырезают фигуры на конкурсе 23 декабря в мексиканском городе Оахака?
- EN: What are figures carved from at the 23 December contest in Oaxaca, Mexico?
- Options: **Из огромных редисок | Giant radishes** / Изо льда | Ice / Из тыкв | Pumpkins / Из сыра | Cheese
- Scores: F 2, C 5, G 1, D 2, E 5, A 4, P 4
- Verdict: Wonderful 'wait, seriously?' answer, but the correct option is twice as long and the only oddly specific one, and 'ice' or 'cheese' make little sense in Mexico. Balanced Mexican-plausible options keep the surprise without the telegraph.
- Rewrite (73 GOOD): RU: Из чего вырезают фигуры на празднике 23 декабря в мексиканском городе Оахака? / EN: What are figures carved from at the 23 December festival in Oaxaca, Mexico?
  - Options: **Из редиски | Radishes** / Из сахара | Sugar / Из кукурузы | Corn / Из тыквы | Pumpkins
  - Why better: Short, equal options that all sound Mexican make players genuinely weigh sugar skulls and corn, so the radish reveal is a real surprise.

### holidays_029 (multipleChoice, medium): 60 NEEDS IMPROVEMENT, IMPROVE

- RU: Кто приносит подарки итальянским детям в ночь на 6 января?
- EN: Who brings Italian children presents on the night before 6 January?
- Options: **Добрая ведьма Бефана | Befana, a kind witch** / Дед Мороз | Ded Moroz / Пасхальный кролик | The Easter Bunny / Святой Валентин | Saint Valentine
- Scores: F 3, C 3, G 3, D 1, E 3, A 4, P 4
- Verdict: Befana is a fun figure, but the Easter Bunny and Saint Valentine in January are throwaways and 'kind witch' description singles out the answer. Real Christmas gift-bringers as decoys make it a genuine choice.
- Rewrite (73 GOOD): RU: Кто приносит подарки итальянским детям в ночь на 6 января? / EN: Who brings Italian children presents on the night before 6 January?
  - Options: **Ведьма Бефана | Befana the witch** / Три волхва | The Three Kings / Святой Николай | Saint Nicholas / Баббо Натале | Babbo Natale
  - Why better: Every option is a real holiday gift-bringer (the Three Kings even come on 6 January in Spain), so the broomstick witch is a surprise rather than the only sensible answer.

### holidays_028 (multipleChoice, medium): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: В Альпах вместе со святым Николаем ходит рогатый демон и пугает непослушных детей. Кто это?
- EN: In the Alps, a horned demon follows St Nicholas to scare naughty children. Who is he?
- Options: **Крампус | Krampus** / Гринч | The Grinch / Румпельштильцхен | Rumpelstiltskin / Кощей | Koschei
- Scores: F 4, C 2, G 3, D 3, E 4, A 4, P 3
- Verdict: Krampus is known from films, and the long description plus non-Alpine decoys (Grinch, Koschei) make it easy elimination. A shorter question and a credible bogeyman decoy tighten it.
- Rewrite (66 NEEDS IMPROVEMENT): RU: Кто ходит по Альпам вместе со святым Николаем и пугает непослушных детей? / EN: Who walks the Alps with St Nicholas, scaring naughty children?
  - Options: **Крампус | Krampus** / Бабай | The Babay / Гринч | The Grinch / Кощей | Koschei
  - Why better: Shorter and readable at a glance, and the Babay decoy is a real child-scaring figure for Russian players.

### holidays_031 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Кого, по легенде, отпугивают петардами и красным цветом на китайский Новый год?
- EN: In legend, who are firecrackers and red decorations at Chinese New Year meant to scare off?
- Options: **Чудовище Нянь | The beast Nian** / Голодных мышей | Hungry mice / Лунного зайца | The Moon rabbit / Горного медведя | A mountain bear
- Scores: F 2, C 4, G 3, D 3, E 3, A 4, P 5
- Verdict: Great pun payoff (Nian also means 'year'), but 'beast' is the only monster among mice, a rabbit and a bear, so the answer is telegraphed. Make every option a mythical creature.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Кого, по легенде, отпугивают петардами и красным цветом на китайский Новый год? / EN: In legend, who are firecrackers and red decorations at Chinese New Year meant to scare off?
  - Options: **Чудовище Нянь | The beast Nian** / Лису-оборотня | A fox spirit / Морского дракона | A sea dragon / Тигра-людоеда | A man-eating tiger
  - Why better: All four are scary creatures, so players must reason instead of picking the only monster, and the 'Nian = year' pun still lands.

### holidays_047 (closestValue, hard): 50 REJECT, IMPROVE

- RU: В каком году в шведском городе Евле впервые поставили гигантского соломенного козла?
- EN: In which year did the Swedish town of Gävle first build its giant straw Christmas goat?
- Answer: **1966** year, range 1850 to 2020, tolerance 12
- Scores: F 1, C 3, G 1, D 2, E 3, A 4, P 5. Flags: OBSCURE_TRIVIA, NO_DEDUCTION
- Verdict: The burning-goat story is hilarious, but the question hides it behind a founding year nobody can estimate. Putting the arson tradition into the question gives context and makes the date a curiosity.
- Rewrite (59 NEEDS IMPROVEMENT): RU: В шведском Евле на Рождество ставят огромного соломенного козла, а его часто сжигают. С какого года? / EN: Every Christmas the Swedish town of Gävle builds a giant straw goat, and people keep burning it. Since what year?
  - Answer 1966, range 1900 to 2020, tolerance 12
  - Why better: The arson tradition up front gets laughs before the answer, and the fact (burnt on its very first night) becomes the punchline.

### holidays_041 (closestValue, hard): 63 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько рождественских парней — троллей-проказников — навещают исландских детей перед Рождеством?
- EN: How many Yule Lads, prankster trolls, visit Icelandic children before Christmas?
- Answer: **13** , range 0 to 60, tolerance 7
- Scores: F 1, C 4, G 2, D 3, E 4, A 4, P 5. Flags: OBSCURE_TRIVIA
- Verdict: Delightful folklore with a funny fact, but the number is pure guesswork as asked. Mentioning that they come one per night gives a reasoning path (count the nights before Christmas).
- Rewrite (72 GOOD): RU: Перед Рождеством к исландским детям приходят тролли-проказники, по одному за ночь. Сколько их? / EN: Before Christmas, prankster trolls visit Icelandic children, one per night. How many are there?
  - Answer 13, range 0 to 60, tolerance 5
  - Why better: 'One per night' lets players reason from the run-up to Christmas, and the spoon-licker and door-slammer fact remains the payoff.

## kitchen

### kitchen_013 (multipleChoice, easy): 70 GOOD, KEEP

- RU: В честь кого назван бефстроганов?
- EN: Who is beef Stroganoff named after?
- Options: Глагола «строгать» | The verb strogat, to whittle / Деревни Строгино | The village of Strogino / **Графов Строгановых | The Counts Stroganov** / Повара Жана Строгана | Chef Jean Strogan
- Scores: F 5, C 2, G 4, D 3, E 3, A 5, P 3
- Verdict: Familiar dish; the joke decoys (the verb strogat, Strogino, chef Jean Strogan) make the table smile, though most will guess the counts. Fact is fine.

### kitchen_002 (multipleChoice, easy): 78 GOOD, KEEP

- RU: Какую еду нашли в гробницах фараонов — и она до сих пор съедобна?
- EN: Which food found in pharaohs' tombs is still edible today?
- Options: Хлеб | Bread / Сыр | Cheese / Финики | Dates / **Мёд | Honey**
- Scores: F 5, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Well-known 'eternal honey' fact, but satisfying to get right, with credible food decoys and a clear science payoff.

### kitchen_006 (multipleChoice, easy): 72 GOOD, KEEP

- RU: Откуда у гамбургера такое название?
- EN: Where does the hamburger get its name?
- Options: От фамилии изобретателя | Its inventor's surname / От первой закусочной «Бургер» | A diner called Burger's / **От города Гамбурга | The city of Hamburg** / От ветчины, по-английски ham | Ham
- Scores: F 5, C 2, G 4, D 4, E 3, A 5, P 3
- Verdict: Familiar, with a fun 'ham' trap that the fact explicitly debunks. Most will reason their way to Hamburg.

### kitchen_037 (multipleChoice, medium): 79 GOOD, KEEP

- RU: Почему в пасту, ставшую потом «Нутеллой», положили так много фундука?
- EN: Why did the spread that became Nutella get so much hazelnut?
- Options: **После войны не хватало какао | Cocoa was scarce after the war** / Так требовал закон Италии | Italian law required it / Фундук продлевал срок годности | Hazelnut kept it fresh longer / Ради рекламного слогана | For an advertising slogan
- Scores: F 5, C 4, G 3, D 4, E 4, A 4, P 4
- Verdict: Everyone loves Nutella, and the post-war cocoa shortage is a surprising and satisfying reason. The 'advertising slogan' decoy is weaker but acceptable.

### kitchen_034 (multipleChoice, medium): 83 GOOD, KEEP

- RU: Мороженое пломбир названо в честь…
- EN: Plombir ice cream is named after…
- Options: Свинцовой пломбы | A lead seal / Кондитера Пломбье | A confectioner called Plombier / **Французского городка | A French spa town** / Зубной пломбы | A dental filling
- Scores: F 5, C 5, G 2, D 5, E 4, A 5, P 4
- Verdict: Excellent: everyone eats plombir and the dental filling (plomba) trap is irresistible; a French spa town is a real surprise.

### kitchen_024 (multipleChoice, medium): 81 GOOD, KEEP

- RU: Слово «пельмени» пришло из коми и удмуртского языков. Что оно значит?
- EN: The word pelmeni comes from the Komi and Udmurt languages. What does it mean?
- Options: Медвежья лапа | Bear's paw / **Хлебное ухо | Bread ear** / Снежный ком | Snowball / Мясной мешочек | Meat pouch
- Scores: F 5, C 4, G 3, D 4, E 4, A 5, P 4
- Verdict: Familiar food, surprising origin, and once you hear 'bread ear' you look at a pelmen and agree. Decoys are credible.

### kitchen_028 (multipleChoice, medium): 92 EXCELLENT, KEEP

- RU: Где придумали «гавайскую» пиццу с ананасами?
- EN: Where was Hawaiian pizza with pineapple invented?
- Options: **В Канаде | Canada** / На Гавайях | Hawaii / В Италии | Italy / В Австралии | Australia
- Scores: F 5, C 5, G 3, D 5, E 5, A 5, P 5
- Verdict: Calibration anchor: everyone knows the pizza, the Hawaii trap is perfect, and the Canada reveal starts arguments.

### kitchen_026 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Кто открыл первый ресторан «Макдоналдс»?
- EN: Who opened the first McDonald's restaurant?
- Options: Уолт Дисней | Walt Disney / Рэй Крок | Ray Kroc / Клоун Рональд Макдональд | Ronald McDonald the clown / **Братья Макдональд | The McDonald brothers**
- Scores: F 5, C 2, G 4, D 2, E 3, A 5, P 3
- Verdict: Ray Kroc is a great trap (The Founder film), but Walt Disney and Ronald the clown are throwaways. Swapping them for another fast-food founder makes it a real two-way think.
- Rewrite (70 GOOD): RU: Кто открыл первый ресторан «Макдоналдс»? / EN: Who opened the first McDonald's restaurant?
  - Options: **Братья Макдональд | The McDonald brothers** / Рэй Крок | Ray Kroc / Полковник Сандерс | Colonel Sanders / Уолт Дисней | Walt Disney
  - Why better: Colonel Sanders replaces the clown, so two credible fast-food founders compete with the brothers and the Kroc trap still works.

### kitchen_041 (closestValue, hard): 70 GOOD, FACT-CHECK

- RU: «Как управлять страной, где столько сортов сыра?» — жаловался де Голль. Сколько сортов он назвал?
- EN: 'How can you govern a country with so many kinds of cheese?' de Gaulle asked. How many did he name?
- Answer: **246** kinds, range 0 to 1000, tolerance 60
- Scores: F 4, C 3, G 3, D 3, E 4, A 5, P 3. Flags: FACTUAL_RISK
- Verdict: Famous quote and a fun estimate, but the exact number in de Gaulle's line is reported differently across sources. The wide tolerance (±60) already absorbs the variants, so it plays fine once the figure is confirmed.
- Fact check: The cheese count in de Gaulle's quote circulates as 246, 258 and 265 depending on the source; confirm which figure the cited source supports (tolerance 60 covers all three).
- Not web-checked: 246, 258 and 265 all circulate; the tolerance of 60 covers all three, so play is unaffected.

### kitchen_042 (closestValue, hard): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: В 2018 году учёные описали древнейший известный хлеб — обугленные крошки из Иордании. Сколько ему лет?
- EN: In 2018 scientists reported the oldest known bread, charred crumbs from Jordan. How old is it?
- Answer: **14400** years, range 0 to 50000, tolerance 5000
- Scores: F 4, C 4, G 3, D 3, E 3, A 3, P 4. Flags: TOO_WORDY
- Verdict: Great fact (bread older than farming), but the Russian question is over 100 characters and mentions a 2018 report nobody needs. A shorter question keeps the estimate and the payoff.
- Rewrite (73 GOOD): RU: Древнейшему найденному хлебу — обугленным крошкам из Иордании — сколько лет? / EN: The oldest bread ever found, charred crumbs from Jordan: how old is it?
  - Answer 14400, range 0 to 50000, tolerance 4000
  - Why better: Shorter and easier to read on the TV, with the same surprising 'older than farming' reveal.

## drinks

### drinks_008 (multipleChoice, easy): 59 NEEDS IMPROVEMENT, KEEP

- RU: Из чего делают традиционный квас?
- EN: What is traditional kvass made from?
- Options: Из риса | Rice / Из молока | Milk / **Из ржаного хлеба | Rye bread** / Из картофеля | Potatoes
- Scores: F 5, C 1, G 4, D 2, E 2, A 5, P 2. Flags: OBVIOUS_ANSWER, BORING_REVEAL
- Verdict: Deliberate easy recognition for Russians; rice, milk and potatoes are throwaways and the okroshka fact is common knowledge. Acceptable only as a warm-up.

### drinks_005 (multipleChoice, easy): 61 NEEDS IMPROVEMENT, KEEP

- RU: Чем ароматизирован чай «Эрл Грей»?
- EN: What gives Earl Grey tea its flavour?
- Options: Жасмином | Jasmine / Мятой | Mint / Корицей | Cinnamon / **Бергамотом | Bergamot**
- Scores: F 5, C 2, G 2, D 4, E 2, A 5, P 3
- Verdict: Deliberate easy recognition: most tea drinkers know bergamot. The fact that bergamot is a sour citrus (many think it is a herb) is a small payoff.

### drinks_003 (multipleChoice, easy): 79 GOOD, KEEP

- RU: Что означает итальянское слово «латте»?
- EN: What does the Italian word 'latte' mean?
- Options: Утро | Morning / **Молоко | Milk** / Пена | Foam / Чашка | Cup
- Scores: F 5, C 3, G 4, D 3, E 4, A 5, P 4
- Verdict: Easy to guess, but the payoff (order a 'latte' in Italy and get a glass of milk) is a story people repeat.

### drinks_021 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком кофе больше кофеина?
- EN: Which coffee has more caffeine?
- Options: В арабике | Arabica / Поровну | They are equal / Зависит только от обжарки | Only the roast decides / **В робусте | Robusta**
- Scores: F 4, C 3, G 3, D 4, E 2, A 5, P 3
- Verdict: A real kitchen debate (many believe arabica is 'stronger'), but the dry framing does not invite the argument. Framing it as a coffee dispute and using the popular dark-roast myth as a decoy makes it social.
- Rewrite (72 GOOD): RU: Кофейный спор: в каких зёрнах больше кофеина? / EN: Coffee argument: which beans have more caffeine?
  - Options: **В робусте | Robusta** / В арабике | Arabica / Поровну, разница во вкусе | Equal, only the taste differs / Больше в тёмной обжарке | Dark roast has more
  - Why better: Calling it an argument invites the table to take sides, and the dark-roast myth is a believable trap.

### drinks_020 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Откуда взялось слово «мокко»?
- EN: Where does the word 'mocha' come from?
- Options: От реки в Бразилии | A river in Brazil / От турецкого слова «пена» | The Turkish word for foam / **От порта в Йемене | A port in Yemen** / От итальянского шоколатье | An Italian chocolatier
- Scores: F 4, C 4, G 2, D 4, E 3, A 5, P 3
- Verdict: Pleasant etymology, but the 'Brazil river' and 'Italian chocolatier' decoys are easy to dismiss. An Ethiopian decoy (the home of coffee) makes the Yemeni port a real surprise.
- Rewrite (75 GOOD): RU: Откуда взялось слово «мокко»? / EN: Where does the word 'mocha' come from?
  - Options: **От порта в Йемене | A port in Yemen** / От города в Эфиопии | A town in Ethiopia / От реки в Бразилии | A river in Brazil / От турецкого слова «пена» | The Turkish word for foam
  - Why better: Ethiopia, the birthplace of coffee, is a strong trap next to Yemen, so coffee-lovers must actually think.

### drinks_022 (multipleChoice, medium): 74 GOOD, KEEP

- RU: Ради чего придумали растворимый кофе «Нескафе»?
- EN: What problem was Nescafé invented to solve?
- Options: **Пристроить излишки бразильского кофе | Brazil's unsold coffee surplus** / Накормить лётчиков в полёте | Feeding pilots in flight / Кофе для альпинистов | Coffee for climbers / Нехватку чайников в Швейцарии | A kettle shortage in Switzerland
- Scores: F 5, C 4, G 3, D 3, E 3, A 4, P 4
- Verdict: Familiar brand with a surprising economic origin; the kettle-shortage decoy is a nice joke though the correct option is the longest.

### drinks_032 (multipleChoice, medium): 80 GOOD, KEEP

- RU: Монах Дом Периньон, «отец шампанского», на самом деле пытался…
- EN: Dom Pérignon, the 'father of champagne', actually tried to…
- Options: Делать вино из яблок | Make wine from apples / Подслащивать вино мёдом | Sweeten wine with honey / **Избавиться от пузырьков | Get rid of the bubbles** / Сделать вино игристее | Make wine more sparkling
- Scores: F 4, C 5, G 3, D 3, E 4, A 5, P 4
- Verdict: Great reversal (the 'father of champagne' fought bubbles), though 'actually' hints that the answer is the opposite of expected. The 'tasting the stars' quote being an ad is a bonus payoff.

### drinks_019 (multipleChoice, medium): 85 EXCELLENT, KEEP

- RU: Рубль нельзя было вывезти. Чем СССР в 1970-х платил за «Пепси»?
- EN: The rouble was useless abroad. How did the USSR pay for Pepsi in the 1970s?
- Options: Лесом | With timber / **Водкой | With vodka** / Золотом | With gold / Икрой | With caviar
- Scores: F 5, C 4, G 4, D 4, E 4, A 5, P 4
- Verdict: Strong: everyone knows Pepsi and Stolichnaya, all decoys are credible Soviet exports, and the barter story is a perfect Soviet-nostalgia reveal.

### drinks_039 (closestValue, hard): 43 REJECT, REPLACE

- RU: В каком году монгольский хан прислал царю Михаилу Фёдоровичу в подарок четыре пуда чая?
- EN: In what year did a Mongol khan send Tsar Michael four poods (about 65 kg) of tea?
- Answer: **1638** year, range 1500 to 2020, tolerance 40
- Scores: F 2, C 2, G 2, D 2, E 1, A 4, P 3. Flags: OBSCURE_TRIVIA, SCHOOL_EXAM, NO_DEDUCTION
- Verdict: An exact year of a diplomatic gift that only history buffs can place; the reign of Michael Fyodorovich is the only anchor and few remember it. The 'boyars found it odd' fact is not enough.

### drinks_041 (closestValue, hard): 63 NEEDS IMPROVEMENT, IMPROVE

- RU: По мексиканскому списку цен 1545 года, сколько какао-бобов стоила хорошая индейка?
- EN: In a Mexican price list from 1545, how many cocoa beans did a good turkey hen cost?
- Answer: **100** beans, range 0 to 1000, tolerance 40
- Scores: F 2, C 4, G 2, D 3, E 3, A 4, P 5
- Verdict: Lovely fact (cocoa beans as money), but there is no anchor to estimate a turkey's price. Giving the rabbit's price in the question turns it into a fun ratio guess.
- Rewrite (77 GOOD): RU: В Мексике XVI века какао-бобы были деньгами: маленький кролик стоил 30 бобов. А хорошая индейка? / EN: In 16th-century Mexico cocoa beans were money: a small rabbit cost 30 beans. And a good turkey?
  - Answer 100, range 0 to 500, tolerance 30
  - Why better: The rabbit price gives an everyday anchor (a turkey is worth a few rabbits), so players estimate instead of guessing blindly.

## fashion

### fashion_014 (multipleChoice, easy): 78 GOOD, KEEP

- RU: Почему в Америке кроссовки называют sneakers?
- EN: Why are sports shoes called sneakers in America?
- Options: **В них можно тихо подкрасться | You can sneak around in them** / Их шили из змеиной кожи | They were made of snakeskin / По фамилии изобретателя | After the inventor's surname / В честь батончика Snickers | After the Snickers bar
- Scores: F 5, C 3, G 5, D 3, E 3, A 5, P 3
- Verdict: Guessable from the English verb 'sneak', which rewards intuition; the Snickers decoy gets a smile. A pleasant easy question.

### fashion_004 (multipleChoice, easy): 69 NEEDS IMPROVEMENT, KEEP

- RU: Шпилька по-английски — stiletto. Что значит это слово по-итальянски?
- EN: A spike heel is called a stiletto. What does the Italian word mean?
- Options: **Кинжал | A dagger** / Иголка | A needle / Цапля | A heron / Карандаш | A pencil
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 3
- Verdict: Deliberate easy question: Russian 'стилет' already means a dagger, so many will get it by sound. The steel-rod fact is a modest payoff.

### fashion_013 (multipleChoice, easy): 68 NEEDS IMPROVEMENT, KEEP

- RU: Чем в XVIII веке пудрили парики?
- EN: What was 18th century wig powder made of?
- Options: **Крахмалом | Starch** / Сахарной пудрой | Icing sugar / Золой | Ash / Солью | Salt
- Scores: F 3, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Easy warm-up with a fun icing-sugar decoy; the 1795 powder tax that killed the fashion is the real payoff. Acceptable as an easy question.

### fashion_028 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Почему французский король Людовик XIII в 1624 году начал носить парик?
- EN: Why did King Louis XIII of France start wearing a wig in 1624?
- Options: **Он рано облысел | He was going bald** / Он был в трауре | He was in mourning / Чтобы казаться выше | To look taller / Чтобы греться на охоте | To keep warm when hunting
- Scores: F 3, C 3, G 4, D 4, E 4, A 5, P 3
- Verdict: Most will guess baldness and feel clever; the decoys (mourning, height, warmth) are believable for a king. The idea that a whole fashion began with a bald king is fun.

### fashion_038 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Откуда англичане привезли слово и моду на пижамы?
- EN: Where did the British pick up the word and fashion for pyjamas?
- Options: **Из Индии | India** / Из Японии | Japan / Из Китая | China / Из Испании | Spain
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Familiar garment, credible Asian decoys, and the 'leg garment' etymology is a satisfying reveal.

### fashion_026 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Чья свадьба в 1840 году сделала модным белое свадебное платье?
- EN: Whose 1840 wedding made the white wedding dress fashionable?
- Options: **Королевы Виктории | Queen Victoria** / Марии-Антуанетты | Marie Antoinette / Императрицы Жозефины | Empress Josephine / Екатерины II | Catherine the Great
- Scores: F 4, C 3, G 4, D 2, E 3, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Good subject, but the year 1840 eliminates Marie Antoinette, Josephine and Catherine, who were all dead by then. Dropping the year makes the decoys live.
- Rewrite (71 GOOD): RU: Чья свадьба сделала модным белое свадебное платье? / EN: Whose wedding made the white wedding dress fashionable?
  - Options: **Королевы Виктории | Queen Victoria** / Марии-Антуанетты | Marie Antoinette / Императрицы Жозефины | Empress Josephine / Екатерины II | Catherine the Great
  - Why better: Without the date, all four royal brides are credible, so players have to reason rather than check death dates.

### fashion_022 (multipleChoice, medium): 57 NEEDS IMPROVEMENT, REPLACE

- RU: Какую причёску по закону носили мужчины в Китае при династии Цин?
- EN: What hairstyle did Chinese men have to wear by law under the Qing dynasty?
- Options: **Бритый лоб и длинная коса | Shaved forehead, long braid** / Пучок на макушке | A topknot / Голову наголо | A fully shaved head / Длинные распущенные волосы | Long loose hair
- Scores: F 3, C 2, G 3, D 3, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: Recognisable from films, but the correct option is the only detailed two-part answer and the reveal reads like a history lesson. Little surprise or group reaction.

### fashion_016 (multipleChoice, medium): 79 GOOD, KEEP

- RU: От названия какого народа произошло французское слово cravate — галстук?
- EN: The French word cravate, a necktie, comes from the name of which people?
- Options: **Хорваты | Croats** / Чехи | Czechs / Казаки | Cossacks / Кельты | Celts
- Scores: F 4, C 4, G 4, D 4, E 3, A 5, P 4
- Verdict: Cravate sounds like 'Croat', so intuition is rewarded, and the mercenaries story is a nice payoff.

### fashion_043 (closestValue, hard): 44 REJECT, REPLACE

- RU: В каком году Луи Виттон открыл в Париже своё дело?
- EN: In what year did Louis Vuitton open his business in Paris?
- Answer: **1854** year, range 1780 to 2020, tolerance 15
- Scores: F 3, C 1, G 2, D 2, E 1, A 5, P 3. Flags: SCHOOL_EXAM, NO_DEDUCTION
- Verdict: A brand's founding year is an exam fact nobody can estimate within ±15; the flat-lid trunk fact is mildly nice but cannot carry a bare date.

### fashion_048 (closestValue, hard): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько лет кожаному башмаку, найденному в 2008 году в армянской пещере Арени-1?
- EN: How old is the leather shoe found in 2008 in Armenia's Areni-1 cave?
- Answer: **5500** years, range 500 to 20000, tolerance 1500
- Scores: F 2, C 4, G 3, D 3, E 3, A 4, P 4. Flags: OBSCURE_TRIVIA
- Verdict: The oldest leather shoe (size 37, stuffed with grass) is a fun fact, but the cave name makes the question feel specialist. Asking about the world's oldest shoe directly keeps the estimate and drops the jargon.
- Rewrite (70 GOOD): RU: Древнейшую кожаную туфлю в мире нашли в пещере в Армении. Сколько ей лет? / EN: The world's oldest leather shoe was found in a cave in Armenia. How old is it?
  - Answer 5500, range 500 to 20000, tolerance 1500
  - Why better: 'World's oldest shoe' is instantly relatable, so players reason about prehistory instead of stumbling over a cave name.

## transport

### transport_008 (multipleChoice, easy): 69 NEEDS IMPROVEMENT, KEEP

- RU: Что такое катамаран?
- EN: What is a catamaran?
- Options: **Судно с двумя корпусами | A boat with two hulls** / Судно с двумя мачтами | A boat with two masts / Маленькая подводная лодка | A small submarine / Плот из бочек | A raft made of barrels
- Scores: F 5, C 2, G 4, D 3, E 2, A 5, P 4
- Verdict: Deliberate easy recognition; 'barrel raft' is weak. The Tamil 'tied logs' etymology is a nice surprise payoff.

### transport_002 (multipleChoice, easy): 79 GOOD, KEEP

- RU: Какого цвета «чёрный ящик» самолёта?
- EN: What colour is an aircraft's 'black box'?
- Options: Чёрного | Black / **Ярко-оранжевого | Bright orange** / Белого | White / Серебристого | Silver
- Scores: F 5, C 4, G 3, D 4, E 4, A 5, P 3
- Verdict: The 'black' option is a perfect intended trap and people love being told the black box is orange. Fact explains why.

### transport_013 (multipleChoice, easy): 66 NEEDS IMPROVEMENT, KEEP

- RU: Какую советскую машину в народе прозвали «копейкой»?
- EN: Which Soviet car was nicknamed 'Kopeyka', the kopeck?
- Options: **ВАЗ-2101 «Жигули» | VAZ-2101 Zhiguli** / «Москвич-412» | Moskvich-412 / ГАЗ-21 «Волга» | GAZ-21 Volga / ЗАЗ-965 «Запорожец» | ZAZ-965 Zaporozhets
- Scores: F 5, C 1, G 3, D 5, E 3, A 5, P 3
- Verdict: Deliberate easy recognition with strong nostalgia for the older generation; all decoys are real Soviet cars. The Fiat 124 fact is fairly well known.

### transport_019 (multipleChoice, medium): 63 NEEDS IMPROVEMENT, IMPROVE

- RU: Где стоял двигатель у «горбатого» «Запорожца» ЗАЗ-965?
- EN: Where was the engine of the 'humpbacked' Zaporozhets ZAZ-965?
- Options: **Сзади | At the back** / Спереди | At the front / Под сиденьями | Under the seats / Двигателя не было | It had no engine
- Scores: F 4, C 2, G 4, D 1, E 3, A 5, P 3
- Verdict: Nostalgic for the older players, but 'it had no engine' and 'under the seats' are throwaways, leaving front vs back. Credible engine positions make it a real question.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Где стоял двигатель у «горбатого» «Запорожца» ЗАЗ-965? / EN: Where was the engine of the 'humpbacked' Zaporozhets ZAZ-965?
  - Options: **Сзади | At the back** / Спереди | At the front / Под полом салона | Under the cabin floor / Посередине, за сиденьями | In the middle, behind the seats
  - Why better: All four positions exist on real cars, so younger players cannot eliminate jokes and the older ones get to show off.

### transport_032 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой конструктор построил в России самолёт «Илья Муромец», а в США — первые серийные вертолёты?
- EN: Who designed Russia's Ilya Muromets plane and later America's first mass-produced helicopters?
- Options: **Игорь Сикорский | Igor Sikorsky** / Андрей Туполев | Andrei Tupolev / Михаил Миль | Mikhail Mil / Николай Камов | Nikolai Kamov
- Scores: F 3, C 3, G 4, D 4, E 2, A 4, P 4
- Verdict: Mil and Kamov are clever helicopter traps and the US link points to the emigrant Sikorsky; the plane-with-a-toilet fact is fun. The question is long; trimming it helps readability.
- Rewrite (72 GOOD): RU: Кто построил «Илью Муромца», а потом в США — первые серийные вертолёты? / EN: Who built the Ilya Muromets plane and later America's first mass-produced helicopters?
  - Options: **Игорь Сикорский | Igor Sikorsky** / Андрей Туполев | Andrei Tupolev / Михаил Миль | Mikhail Mil / Николай Камов | Nikolai Kamov
  - Why better: Shorter and readable in a few seconds, so more players engage with the 'emigrant designer' reasoning path.

### transport_029 (multipleChoice, medium): 74 GOOD, KEEP

- RU: Что по британскому закону 1865 года должно было двигаться перед паровым автомобилем?
- EN: Under an 1865 British law, what had to go in front of a steam road vehicle?
- Options: **Человек с красным флагом | A man with a red flag** / Всадник с горном | A rider with a horn / Собака с колокольчиком | A dog with a bell / Полицейский с барабаном | A policeman with a drum
- Scores: F 2, C 5, G 3, D 4, E 4, A 4, P 4
- Verdict: Absurd-sounding but true; all decoys are equally silly, so it plays as a fun guessing game, and the 4 mph limit is a great payoff.

### transport_037 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Какая компания придумала трёхточечный ремень безопасности и отдала патент всем бесплатно?
- EN: Which carmaker invented the three-point seat belt and let everyone use the patent free?
- Options: **«Вольво» | Volvo** / «Мерседес-Бенц» | Mercedes-Benz / «Форд» | Ford / «Тойота» | Toyota
- Scores: F 5, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Volvo's safety reputation gives a reasoning path, and giving the patent away for free is a satisfying story.

### transport_036 (multipleChoice, medium): 81 GOOD, KEEP

- RU: Первый в мире светофор поставили в Лондоне в 1868 году. Чем он светил ночью?
- EN: The first traffic light went up in London in 1868. What lit it at night?
- Options: **Газовыми фонарями | Gas lamps** / Электрическими лампочками | Electric bulbs / Свечами | Candles / Светящейся краской | Glow-in-the-dark paint
- Scores: F 3, C 4, G 4, D 4, E 4, A 5, P 5
- Verdict: Good reasoning path (1868 means no electric bulbs yet) and the exploding traffic light is a memorable payoff.

### transport_048 (closestValue, hard): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько колёс было у шасси самого тяжёлого самолёта в истории, Ан-225 «Мрия»?
- EN: How many wheels did the An-225 Mriya, the heaviest aircraft ever built, have?
- Answer: **32** wheels, range 4 to 200, tolerance 10
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 3
- Verdict: Mriya is well known to this family, but the fact only restates why so many wheels. Putting the 640 tonnes in the question gives an estimation path.
- Rewrite (71 GOOD): RU: Ан-225 «Мрия» весил на взлёте до 640 тонн. Сколько колёс держало его на земле? / EN: The An-225 Mriya weighed up to 640 tonnes at take-off. How many wheels held it up?
  - Answer 32, range 4 to 200, tolerance 8
  - Why better: The weight lets players estimate tonnes per wheel instead of guessing a bare count.

### transport_041 (closestValue, hard): 70 GOOD, KEEP

- RU: Сколько тонн груза может поднять Ми-26, самый большой серийный вертолёт в мире?
- EN: How many tonnes can the Mi-26, the largest helicopter ever mass-produced, carry?
- Answer: **20** tonnes, range 0 to 100, tolerance 7
- Scores: F 3, C 3, G 3, D 4, E 3, A 5, P 5
- Verdict: A fair estimate with an everyday sense of scale, and the payoff (it airlifted American Chinooks) is a great story.

## people

### people_014 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Какое животное носит имя путешественника Пржевальского?
- EN: Which animal is named after the explorer Przhevalsky?
- Options: **Лошадь | A horse** / Медведь | A bear / Тигр | A tiger / Волк | A wolf
- Scores: F 5, C 2, G 3, D 3, E 2, A 5, P 4
- Verdict: Deliberate easy recognition for Russian players; the extinct-in-the-wild-and-returned story is a good payoff.

### people_005 (multipleChoice, easy): 61 NEEDS IMPROVEMENT, KEEP

- RU: Как прозвали медсестру Флоренс Найтингейл?
- EN: What was nurse Florence Nightingale's nickname?
- Options: Белая королева | The White Queen / **Леди с лампой | The Lady with the Lamp** / Снежная королева | The Snow Queen / Орлеанская дева | The Maid of Orleans
- Scores: F 3, C 2, G 4, D 3, E 2, A 5, P 3
- Verdict: Deliberate easy recognition; the other nicknames belong to queens or Joan of Arc, so it is easy to eliminate. The night-rounds fact is gentle.

### people_004 (multipleChoice, easy): 76 GOOD, KEEP

- RU: Кто стал первыми пассажирами воздушного шара братьев Монгольфье в 1783 году?
- EN: Who were the first passengers of the Montgolfier brothers' balloon in 1783?
- Options: Король и королева | The king and queen / Сами братья | The brothers themselves / **Баран, утка и петух | A sheep, a duck and a rooster** / Собака и кошка | A dog and a cat
- Scores: F 3, C 5, G 2, D 3, E 5, A 5, P 4
- Verdict: Delightful answer that gets a laugh; the correct option is the only three-part one, but the reveal is worth it.

### people_018 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Кого Никола Тесла под старость любил больше всего на свете?
- EN: Whom did Nikola Tesla love most in his old age?
- Options: **Белую голубку | A white pigeon** / Кошку | His cat / Попугая | A parrot / Собаку | His dog
- Scores: F 4, C 5, G 1, D 2, E 5, A 4, P 5
- Verdict: Very surprising and funny, but 'white pigeon' is far longer than the one-word decoys. A one-word option fixes the telegraph cheaply.
- Rewrite (79 GOOD): RU: Кого Никола Тесла под старость любил больше всего на свете? / EN: Whom did Nikola Tesla love most in his old age?
  - Options: **Голубку | A pigeon** / Кошку | His cat / Попугая | A parrot / Собаку | His dog
  - Why better: Four single-word pets of equal length, so the pigeon is a pure surprise.

### people_037 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Как Софья Ковалевская смогла уехать учиться за границу?
- EN: How did Sofia Kovalevskaya manage to go abroad to study?
- Options: Её отправила императрица | The Empress sent her / **Вступила в фиктивный брак | A sham marriage** / Переоделась юношей | Disguised as a young man / Сбежала на корабле | Ran away on a ship
- Scores: F 3, C 4, G 3, D 4, E 4, A 5, P 4
- Verdict: Disguising as a young man is a great trap and the sham marriage is a surprising, story-like reveal.

### people_022 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Где погиб капитан Джеймс Кук?
- EN: Where did Captain James Cook die?
- Options: **На Гавайях | Hawaii** / В Новой Зеландии | New Zealand / В Австралии | Australia / На Таити | Tahiti
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 3
- Verdict: Cook is famous, but the question misses the hook every Russian knows: Vysotsky's song about natives eating Cook. Quoting it makes the table laugh before they answer.
- Rewrite (76 GOOD): RU: «Почему аборигены съели Кука?» — пел Высоцкий. А где капитан погиб на самом деле? / EN: 'Why did the natives eat Cook?' sang Vysotsky. Where did the captain really die?
  - Options: **На Гавайях | Hawaii** / В Новой Зеландии | New Zealand / В Австралии | Australia / На Таити | Tahiti
  - Why better: The Vysotsky line gives an instant laugh and a shared cultural hook for both generations.

### people_023 (multipleChoice, medium): 77 GOOD, KEEP

- RU: Какую должность предложили Эйнштейну в 1952 году (он отказался)?
- EN: What post was Einstein offered in 1952, and turned down?
- Options: Глава NASA | Head of NASA / Мэр Принстона | Mayor of Princeton / Канцлер Германии | Chancellor of Germany / **Президент Израиля | President of Israel**
- Scores: F 4, C 4, G 4, D 3, E 3, A 5, P 4
- Verdict: Surprising and reasonable via Einstein being Jewish, but 'Head of NASA' is an anachronism (NASA dates from 1958), which sharp players notice. Swap it for a credible post.
- Rewrite (79 GOOD): RU: Какую должность предложили Эйнштейну в 1952 году (он отказался)? / EN: What post was Einstein offered in 1952, and turned down?
  - Options: **Президент Израиля | President of Israel** / Мэр Принстона | Mayor of Princeton / Канцлер Германии | Chancellor of Germany / Ректор Гарварда | President of Harvard
  - Why better: Removes the NASA anachronism so no decoy can be ruled out by dates.

### people_031 (multipleChoice, medium): 85 EXCELLENT, KEEP

- RU: Почему на вечеринку Стивена Хокинга в 2009 году никто не пришёл?
- EN: Why did nobody come to Stephen Hawking's party in 2009?
- Options: Она была на Южном полюсе | It was at the South Pole / Приглашения были в формулах | The invitations were equations / Он забыл назвать дату | He forgot to give a date / **О ней объявили уже после | He announced it only afterwards**
- Scores: F 4, C 5, G 3, D 4, E 5, A 4, P 5
- Verdict: Hawking's time-traveller party is a perfect 'wait, seriously?' with a logical path for those who think about time travel; big group reaction.

### people_049 (closestValue, hard): 81 GOOD, KEEP

- RU: До какого возраста дожила француженка Жанна Кальман, видевшая в детстве Ван Гога?
- EN: To what age did Jeanne Calment, who met Van Gogh as a girl, live?
- Answer: **122** years, range 80 to 200, tolerance 8
- Scores: F 3, C 4, G 4, D 4, E 4, A 5, P 5
- Verdict: 'Oldest person ever' gives a good anchor near 120, the tolerance is fair, and Calment calling Van Gogh ugly and disagreeable is a great payoff.

### people_041 (closestValue, hard): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько дней плот «Кон-Тики» Тура Хейердала плыл через Тихий океан?
- EN: How many days did Thor Heyerdahl's raft Kon-Tiki take to cross the Pacific?
- Answer: **101** days, range 5 to 365, tolerance 20
- Scores: F 3, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Kon-Tiki is familiar and the range is fair, but there is little to reason from. Putting the 7,000 km distance in the question gives a speed-based estimate.
- Rewrite (70 GOOD): RU: Плот «Кон-Тики» прошёл почти 7000 км от Перу до Полинезии. Сколько дней длилось плавание? / EN: The Kon-Tiki raft drifted almost 7,000 km from Peru to Polynesia. How many days did it take?
  - Answer 101, range 5 to 365, tolerance 20
  - Why better: With the distance given, players can estimate a raft's daily drift, so the guess rewards reasoning.

## football

### football_008 (multipleChoice, easy): 77 GOOD, KEEP

- RU: С каким счётом Германия разгромила Бразилию в полуфинале ЧМ-2014?
- EN: By what score did Germany crush Brazil in the 2014 World Cup semi-final?
- Options: 3:0 | 3–0 / **7:1 | 7–1** / 4:0 | 4–0 / 5:2 | 5–2
- Scores: F 5, C 2, G 4, D 4, E 4, A 5, P 4
- Verdict: Iconic score that even non-fans remember; the '5-0 after 29 minutes, at home' fact makes it sting again.

### football_004 (multipleChoice, easy): 75 GOOD, KEEP

- RU: Что сделал Зинедин Зидан в финале чемпионата мира 2006 года?
- EN: What did Zinedine Zidane do in the 2006 World Cup final?
- Options: Отказался выходить на поле | Refused to take the field / **Ударил соперника головой | Headbutted an opponent** / Забил гол пяткой | Scored with his heel / Встал в ворота | Went in goal
- Scores: F 5, C 2, G 4, D 3, E 4, A 5, P 4
- Verdict: Famous moment that everyone recognises; the fact that he was still named player of the tournament is a nice twist.

### football_005 (multipleChoice, easy): 84 GOOD, KEEP

- RU: Кто в 2010 году угадал исходы всех матчей Германии на чемпионате мира?
- EN: Who correctly predicted every German game at the 2010 World Cup?
- Options: **Осьминог | An octopus** / Попугай | A parrot / Черепаха | A turtle / Козёл | A goat
- Scores: F 5, C 3, G 4, D 4, E 5, A 5, P 4
- Verdict: Paul the Octopus is a beloved story; the animal decoys keep it playful and the 8-out-of-8 record is a fun payoff.

### football_032 (multipleChoice, medium): 76 GOOD, KEEP

- RU: Что было до 1970 года, если игрок получал травму на чемпионате мира?
- EN: Before 1970, what happened when a player got injured at a World Cup?
- Options: Соперник тоже убирал игрока | The opponents took a player off too / **Команда доигрывала в меньшинстве | His team played on a man down** / Его менял запасной | A substitute came on / Матч переигрывали | The match was replayed
- Scores: F 3, C 4, G 4, D 4, E 3, A 4, P 5
- Verdict: Surprising rule from the past with credible decoys, and the first-ever substitute being Soviet is a great payoff for this family.

### football_038 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Чей портрет украсил официальный плакат чемпионата мира 2018 года?
- EN: Whose portrait was on the official poster of the 2018 World Cup?
- Options: Пеле | Pelé / Диего Марадоны | Diego Maradona / Франца Беккенбауэра | Franz Beckenbauer / **Льва Яшина | Lev Yashin**
- Scores: F 4, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Russia hosting points to a Russian legend, so it is guessable, and the 'Black Spider' nickname is a good payoff.

### football_023 (multipleChoice, medium): 65 NEEDS IMPROVEMENT, IMPROVE

- RU: Кто в 1890 году предложил ввести в футбол пенальти?
- EN: Who proposed the penalty kick in 1890?
- Options: Король | A king / Журналист | A journalist / **Вратарь | A goalkeeper** / Судья | A referee
- Scores: F 2, C 4, G 3, D 2, E 3, A 5, P 4
- Verdict: The irony (a goalkeeper invented the penalty) is great, but 'a king' is a throwaway and the options mix roles. Asking for the player's position makes every option credible and keeps the irony.
- Rewrite (77 GOOD): RU: Пенальти в 1890 году предложил ввести футболист. На какой позиции он играл? / EN: In 1890 a footballer proposed the penalty kick. What position did he play?
  - Options: **Вратарь | Goalkeeper** / Нападающий | Striker / Защитник | Defender / Полузащитник | Midfielder
  - Why better: Four real positions make it a fair guess, and the reveal that a keeper invented his own nightmare gets a laugh.

### football_027 (multipleChoice, medium): 62 NEEDS IMPROVEMENT, REPLACE

- RU: Какой вратарь забил за карьеру больше ста голов?
- EN: Which goalkeeper scored more than a hundred career goals?
- Options: Хосе Луис Чилаверт | José Luis Chilavert / Джанлуиджи Буффон | Gianluigi Buffon / **Рожерио Сени | Rogério Ceni** / Рене Игита | René Higuita
- Scores: F 2, C 4, G 2, D 5, E 2, A 5, P 3. Flags: OBSCURE_TRIVIA, NO_DEDUCTION
- Verdict: The surprise (a goalkeeper with 100+ goals) is buried in a name most of the family has never heard; Chilavert and Higuita are clever traps but only for fans. The number would make a better new question, which changes the answer.

### football_029 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Под руководством какого тренера сборная России дошла до полуфинала Евро-2008?
- EN: Which coach led Russia to the semi-finals of Euro 2008?
- Options: **Гус Хиддинк | Guus Hiddink** / Дик Адвокат | Dick Advocaat / Фабио Капелло | Fabio Capello / Олег Романцев | Oleg Romantsev
- Scores: F 4, C 2, G 3, D 5, E 3, A 5, P 4
- Verdict: Older fans remember Euro 2008; the irony that Hiddink knocked out his own country is the hook. Putting the irony in the question makes Advocaat, also Dutch, a real trap.
- Rewrite (70 GOOD): RU: На Евро-2008 Россия выбила Нидерланды, а тренировал её голландец. Кто? / EN: At Euro 2008 Russia knocked out the Netherlands, with a Dutch coach. Who?
  - Options: **Гус Хиддинк | Guus Hiddink** / Дик Адвокат | Dick Advocaat / Фабио Капелло | Fabio Capello / Олег Романцев | Oleg Romantsev
  - Why better: The irony is up front, and Advocaat, also a Dutch coach of Russia, stays a genuine trap for the fans.

### football_042 (closestValue, hard): 53 REJECT, IMPROVE

- RU: Сколько голов забил француз Жюст Фонтен на одном только чемпионате мира 1958 года?
- EN: How many goals did France's Just Fontaine score at the 1958 World Cup alone?
- Answer: **13** goals, range 0 to 50, tolerance 7
- Scores: F 1, C 3, G 2, D 3, E 2, A 5, P 4. Flags: OBSCURE_TRIVIA
- Verdict: Just Fontaine is unknown to most of the family, so the number is a blind guess. Framing it as the single-tournament record lets players anchor on modern top scorers (5 to 8 goals).
- Rewrite (72 GOOD): RU: Рекорд 1958 года: сколько голов забил француз Жюст Фонтен за один чемпионат мира? / EN: A 1958 record: how many goals did France's Just Fontaine score at a single World Cup?
  - Answer 13, range 0 to 40, tolerance 4
  - Why better: Calling it a record invites comparison with recent Golden Boot winners, giving a real estimation path.

### football_045 (closestValue, hard): 84 GOOD, KEEP

- RU: Сколько голов Австралия забила Американскому Самоа в 2001 году?
- EN: How many goals did Australia score against American Samoa in 2001?
- Answer: **31** goals, range 0 to 100, tolerance 10
- Scores: F 3, C 5, G 3, D 4, E 5, A 5, P 5
- Verdict: Absurd scoreline that gets a 'no way' from everyone, the range is fair, and the passport-trouble teenagers fact is a perfect payoff.

## olympics

### olympics_014 (multipleChoice, easy): 69 NEEDS IMPROVEMENT, KEEP

- RU: В каком году на самом деле прошли Олимпийские игры «Токио-2020»?
- EN: In which year were the Tokyo 2020 Olympics actually held?
- Options: 2019 | 2019 / 2020 | 2020 / **2021 | 2021** / 2022 | 2022
- Scores: F 5, C 1, G 5, D 3, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition (everyone lived through 2021); the 'first Games ever postponed' fact is a good payoff.

### olympics_001 (multipleChoice, easy): 71 GOOD, KEEP

- RU: Кто был талисманом Олимпиады-80 в Москве?
- EN: Who was the mascot of the 1980 Moscow Olympics?
- Options: **Медвежонок | A bear cub** / Волк | A wolf / Заяц | A hare / Чебурашка | Cheburashka
- Scores: F 5, C 1, G 4, D 3, E 4, A 5, P 4
- Verdict: Easy for the older generation, with strong nostalgia, and the Misha-flying-away-on-balloons memory makes people sing. Cheburashka decoy is fun.

### olympics_008 (multipleChoice, easy): 63 NEEDS IMPROVEMENT, KEEP

- RU: Какую страну представлял Усэйн Болт?
- EN: Which country did Usain Bolt run for?
- Options: **Ямайку | Jamaica** / США | USA / Кению | Kenya / Великобританию | Great Britain
- Scores: F 5, C 1, G 4, D 4, E 2, A 5, P 2. Flags: OBVIOUS_ANSWER, BORING_REVEAL
- Verdict: Deliberate easy recognition question; credible decoys but zero surprise, and the three-Games fact is common knowledge.

### olympics_032 (multipleChoice, medium): 74 GOOD, KEEP

- RU: Где соревновались пловцы на первой современной Олимпиаде в 1896 году?
- EN: Where did the swimmers race at the first modern Olympics in 1896?
- Options: **В море | In the sea** / В бассейне | In a pool / В реке | In a river / В озере | In a lake
- Scores: F 3, C 4, G 3, D 4, E 3, A 5, P 5
- Verdict: Surprising, with credible decoys, and the winner saying his will to live beat his will to win is a great payoff.

### olympics_020 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Женщинам в Олимпии выступать запрещали. Как же спартанская царевна Киниска стала чемпионкой?
- EN: Women could not compete at ancient Olympia. So how did the Spartan princess Cynisca win there?
- Options: **Её кони выиграли гонку колесниц | Her horses won the chariot race** / Бежала, переодевшись мужчиной | She ran disguised as a man / Победила в конкурсе поэтов | She won a poetry contest / Её приняли за мальчика | She was mistaken for a boy
- Scores: F 2, C 4, G 3, D 4, E 4, A 4, P 5
- Verdict: Clever loophole story; disguise decoys are believable, and the owner-not-driver rule is a satisfying 'aha'. The NOT script flag is a false positive (it is set-up, not a trick).

### olympics_019 (multipleChoice, medium): 66 NEEDS IMPROVEMENT, IMPROVE

- RU: Какое слово добавили к девизу «Быстрее, выше, сильнее» в 2021 году?
- EN: Which word was added to the motto 'Faster, Higher, Stronger' in 2021?
- Options: **Вместе | Together** / Честнее | Fairer / Дальше | Further / Ярче | Brighter
- Scores: F 4, C 2, G 4, D 4, E 2, A 5, P 3
- Verdict: Guessable after the pandemic Games, but it feels like a news quiz; 'Together' also breaks the comparative pattern of the decoys. Making every decoy a comparative and explaining it is a fourth word sharpens it a little.
- Rewrite (67 NEEDS IMPROVEMENT): RU: В 2021 году к девизу «Быстрее, выше, сильнее» добавили четвёртое слово. Какое? / EN: In 2021 a fourth word was added to 'Faster, Higher, Stronger'. Which one?
  - Options: **Вместе | Together** / Честнее | Fairer / Дальше | Further / Смелее | Braver
  - Why better: The 'fourth word' framing invites players to complete the pattern, which makes the non-comparative 'Together' a small surprise.

### olympics_018 (multipleChoice, medium): 62 NEEDS IMPROVEMENT, REPLACE

- RU: Какой город первым принял летние Олимпийские игры в третий раз?
- EN: Which city was the first to host the Summer Olympics three times?
- Options: **Лондон | London** / Париж | Paris / Токио | Tokyo / Лос-Анджелес | Los Angeles
- Scores: F 4, C 2, G 3, D 4, E 2, A 5, P 3. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: A host-city list fact with no story; the reveal (Paris caught up in 2024) adds little. Feels like an exam question.

### olympics_017 (multipleChoice, medium): 81 GOOD, KEEP

- RU: Почему марафон — это ровно 42 км 195 м?
- EN: Why is a marathon exactly 42.195 km?
- Options: Столько пробежал древний гонец | An ancient messenger ran that far / **Из-за королевской семьи в 1908 году | The royal family wanted it in 1908** / Это ровно 100 000 шагов | It is exactly 100,000 steps / Так решили по жребию | It was drawn by lot
- Scores: F 5, C 4, G 3, D 4, E 4, A 5, P 4
- Verdict: Everyone knows the odd marathon distance, and 'the royal family wanted it' is a surprising, slightly absurd answer with a vivid payoff.

### olympics_040 (closestValue, hard): 50 REJECT, REPLACE

- RU: Игры в Париже-2024 официально названы Играми какой по счёту Олимпиады?
- EN: Paris 2024 was officially the Games of which Olympiad? Give the number.
- Answer: **33** , range 1 to 100, tolerance 7
- Scores: F 3, C 1, G 4, D 3, E 1, A 4, P 2. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: Calibration anchor: countable from 1896, but nobody cares about the number; it feels like an exam.

### olympics_041 (closestValue, hard): 63 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько спортсменов выступило на первой современной Олимпиаде в 1896 году?
- EN: How many athletes competed at the first modern Olympics in 1896?
- Answer: **241** athletes, range 10 to 2000, tolerance 80
- Scores: F 3, C 3, G 3, D 4, E 2, A 5, P 3
- Verdict: Fair range, but the contrast that makes the number interesting is hidden in the fact. Putting the 10,000 of Paris 2024 into the question gives an anchor and makes the small answer a surprise.
- Rewrite (73 GOOD): RU: В Париже-2024 выступили больше 10 000 спортсменов. А сколько — на первых Играх в 1896-м? / EN: Paris 2024 had over 10,000 athletes. How many competed at the first Games in 1896?
  - Answer 241, range 10 to 2000, tolerance 80
  - Why better: The modern figure gives a starting point, and the tiny 1896 number lands as a contrast.

## records

### records_010 (multipleChoice, easy): 64 NEEDS IMPROVEMENT, KEEP

- RU: Сумо — национальный спорт какой страны?
- EN: Sumo is the national sport of which country?
- Options: Китая | China / Кореи | Korea / **Японии | Japan** / Монголии | Mongolia
- Scores: F 5, C 1, G 5, D 3, E 1, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition; the Mongolia decoy is credible for fans, and the salt ritual is a mild payoff. Warm-up only.

### records_006 (multipleChoice, easy): 55 NEEDS IMPROVEMENT, REPLACE

- RU: Сколько длится раунд в профессиональном боксе у мужчин?
- EN: How long is a round in men's professional boxing?
- Options: 1 минуту | 1 minute / 2 минуты | 2 minutes / **3 минуты | 3 minutes** / 5 минут | 5 minutes
- Scores: F 4, C 1, G 3, D 4, E 1, A 5, P 3. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: Sports rule trivia with no surprise or group reaction; the Queensberry rules fact is dry.

### records_008 (multipleChoice, easy): 53 REJECT, REPLACE

- RU: Как в теннисе называют подачу, до которой соперник даже не дотронулся?
- EN: In tennis, what do you call a serve the opponent cannot even touch?
- Options: **Эйс | An ace** / Смэш | A smash / Лет | A let / Свеча | A lob
- Scores: F 4, C 1, G 3, D 4, E 1, A 5, P 2. Flags: SCHOOL_EXAM, BORING_REVEAL
- Verdict: Tennis jargon definition with no surprise; the first-serve fact is technical and will not get a reaction.

### records_026 (multipleChoice, medium): 75 GOOD, KEEP

- RU: Какая команда выступает в Формуле-1 во всех сезонах с первого чемпионата 1950 года?
- EN: Which team has raced in every Formula One season since the first championship in 1950?
- Options: **Феррари | Ferrari** / Макларен | McLaren / Уильямс | Williams / Мерседес | Mercedes
- Scores: F 4, C 3, G 4, D 4, E 3, A 5, P 4
- Verdict: Ferrari is the natural guess, and the twist that it skipped the very first race over start money is a good payoff.

### records_037 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Из чего делают воланы для бадминтона на серьёзных турнирах?
- EN: What are shuttlecocks made of at serious badminton tournaments?
- Options: **Из птичьих перьев | Bird feathers** / Из пластика | Plastic / Из бумаги | Paper / Из шёлка | Silk
- Scores: F 4, C 3, G 3, D 2, E 3, A 5, P 5
- Verdict: Great fact (16 feathers from the same wing), but the correct option is the longest and most specific. Short parallel options fix it.
- Rewrite (73 GOOD): RU: Из чего делают воланы для бадминтона на серьёзных турнирах? / EN: What are shuttlecocks made of at serious badminton tournaments?
  - Options: **Из перьев | Feathers** / Из пластика | Plastic / Из бумаги | Paper / Из шёлка | Silk
  - Why better: Equal-length options remove the telegraph, so the feathers answer and the same-wing fact both surprise.

### records_035 (multipleChoice, medium): 73 GOOD, KEEP

- RU: Из какого спорта пришло выражение «хет-трик»?
- EN: Which sport did the term "hat trick" come from?
- Options: **Из крикета | Cricket** / Из хоккея | Hockey / Из бокса | Boxing / Из скачек | Horse racing
- Scores: F 4, C 4, G 2, D 4, E 3, A 5, P 5
- Verdict: Everyone uses 'hat trick' and few know it is from cricket; the fans buying him a hat is a delightful payoff.

### records_021 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком городе прошёл «матч века» Фишер — Спасский в 1972 году?
- EN: Where was the 1972 "Match of the Century" between Fischer and Spassky played?
- Options: **В Рейкьявике | Reykjavik** / В Москве | Moscow / В Нью-Йорке | New York / В Женеве | Geneva
- Scores: F 3, C 3, G 3, D 3, E 2, A 5, P 3
- Verdict: Legendary match for older players, but New York and Moscow are eliminated by Cold War logic. Neutral-city decoys alongside Moscow make it a real choice.
- Rewrite (68 NEEDS IMPROVEMENT): RU: В каком городе прошёл «матч века» Фишер — Спасский в 1972 году? / EN: Where was the 1972 'Match of the Century' between Fischer and Spassky played?
  - Options: **В Рейкьявике | Reykjavik** / В Москве | Moscow / В Хельсинки | Helsinki / В Вене | Vienna
  - Why better: Helsinki and Vienna are credible neutral venues, so players cannot just eliminate the superpowers' capitals.

### records_038 (multipleChoice, medium): 62 NEEDS IMPROVEMENT, REPLACE

- RU: Что такое «Золотой шлем», который в 1988 году собрала Штеффи Граф?
- EN: What is the "Golden Slam" that Steffi Graf completed in 1988?
- Options: **Все 4 «Шлема» и олимпийское золото | All 4 majors plus Olympic gold** / Сто побед подряд | A hundred wins in a row / Шлем из золота за 10 титулов | A gold helmet for 10 titles / Титулы в одиночке и паре | Singles and doubles titles
- Scores: F 2, C 3, G 4, D 3, E 2, A 4, P 4. Flags: SCHOOL_EXAM
- Verdict: A tennis-jargon definition that is deducible from the word 'Golden' and the joke decoy; niche for this family even though the wheelchair-tennis follow-up is nice.

### records_040 (closestValue, hard): 81 GOOD, KEEP

- RU: Какую скорость в км/ч развил Усэйн Болт на пике рекордной стометровки 2009 года?
- EN: What top speed in km/h did Usain Bolt reach in his record 100 m in 2009?
- Answer: **45** km/h, range 10 to 120, tolerance 7
- Scores: F 5, C 3, G 4, D 4, E 4, A 5, P 4
- Verdict: Excellent estimation question: from 100 m in 9.58 s players can reason to around 40 km/h, and comparing him to city speed limits is a great payoff.

### records_041 (closestValue, hard): 54 REJECT, REPLACE

- RU: В каком году впервые сыграли Уимблдонский турнир?
- EN: In what year was the Wimbledon tournament first played?
- Answer: **1877** year, range 1700 to 2000, tolerance 15
- Scores: F 3, C 2, G 3, D 3, E 1, A 5, P 3. Flags: SCHOOL_EXAM, NO_DEDUCTION
- Verdict: Bare founding date; the croquet-club fact is the only interesting part and it does not need this question.

## books_old

### books_old_006 (multipleChoice, easy): 67 NEEDS IMPROVEMENT, KEEP

- RU: На какой улице Лондона жил Шерлок Холмс?
- EN: On which London street did Sherlock Holmes live?
- Options: **Бейкер-стрит | Baker Street** / Оксфорд-стрит | Oxford Street / Даунинг-стрит | Downing Street / Флит-стрит | Fleet Street
- Scores: F 5, C 1, G 4, D 4, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition; the payoff that number 221 did not exist when Conan Doyle wrote is a genuinely nice twist.

### books_old_004 (multipleChoice, easy): 67 NEEDS IMPROVEMENT, KEEP

- RU: Кто написал «Трёх мушкетёров»?
- EN: Who wrote The Three Musketeers?
- Options: **Александр Дюма | Alexandre Dumas** / Виктор Гюго | Victor Hugo / Жюль Верн | Jules Verne / Оноре де Бальзак | Honoré de Balzac
- Scores: F 5, C 1, G 4, D 4, E 2, A 5, P 4. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition with good French-author decoys; the uncredited co-writer Maquet is a payoff people repeat.

### books_old_007 (multipleChoice, easy): 56 NEEDS IMPROVEMENT, KEEP

- RU: Из какой пьесы Шекспира слова «Быть или не быть»?
- EN: Which Shakespeare play has "To be or not to be"?
- Options: **«Гамлет» | Hamlet** / «Макбет» | Macbeth / «Король Лир» | King Lear / «Отелло» | Othello
- Scores: F 5, C 0, G 4, D 4, E 1, A 5, P 2. Flags: OBVIOUS_ANSWER, BORING_REVEAL
- Verdict: Deliberate easy warm-up: everyone knows it, and 'longest play' is a weak payoff. The NOT script flag is a false positive (it is the quote). Keep only as a first-round confidence builder.

### books_old_020 (multipleChoice, medium): 63 NEEDS IMPROVEMENT, IMPROVE

- RU: Какой роман начинается словами «Все счастливые семьи похожи друг на друга…»?
- EN: Which novel opens "All happy families are alike..."?
- Options: **«Анна Каренина» | Anna Karenina** / «Война и мир» | War and Peace / «Отцы и дети» | Fathers and Sons / «Воскресение» | Resurrection
- Scores: F 5, C 1, G 3, D 4, E 2, A 5, P 4
- Verdict: Pure recognition of a famous opening line, too easy for a medium slot; the Turgenev decoy is out of place. Making every option a Tolstoy novel makes players actually recall which book it opens, and the Levin-from-Lev fact stays the payoff.
- Rewrite (65 NEEDS IMPROVEMENT): RU: Какой роман Толстого начинается словами «Все счастливые семьи похожи друг на друга…»? / EN: Which Tolstoy novel opens with 'All happy families are alike...'?
  - Options: **«Анна Каренина» | Anna Karenina** / «Война и мир» | War and Peace / «Воскресение» | Resurrection / «Крейцерова соната» | The Kreutzer Sonata
  - Why better: Four Tolstoy titles, including the marriage drama The Kreutzer Sonata, make it a real recall test instead of an instant pick.

### books_old_030 (multipleChoice, medium): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько кругов ада у Данте в «Божественной комедии»?
- EN: How many circles of Hell are there in Dante's Divine Comedy?
- Options: **Девять | Nine** / Семь | Seven / Двенадцать | Twelve / Три | Three
- Scores: F 4, C 2, G 3, D 4, E 2, A 5, P 4
- Verdict: Plain recognition for a medium slot; 'seven' (the deadly sins) is a good trap but the question does not use it. Setting up the seven sins in the question turns it into a misdirection, and the 'Divine was added later' fact stays.
- Rewrite (71 GOOD): RU: Смертных грехов семь. А сколько кругов ада у Данте в «Божественной комедии»? / EN: There are seven deadly sins. How many circles of Hell are there in Dante's Divine Comedy?
  - Options: **Девять | Nine** / Семь | Seven / Двенадцать | Twelve / Три | Three
  - Why better: The seven-sins setup tempts players toward 'seven', so those who really remember Dante win and the rest laugh at falling for it.

### books_old_036 (multipleChoice, medium): 67 NEEDS IMPROVEMENT, IMPROVE

- RU: Кем был прадед Пушкина Абрам Ганнибал?
- EN: Who was Pushkin's great-grandfather Abram Gannibal?
- Options: **Африканец, крестник Петра I | An African, Peter I's godson** / Шотландский генерал | A Scottish general / Турецкий паша | A Turkish pasha / Венецианский купец | A Venetian merchant
- Scores: F 5, C 2, G 4, D 3, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: Familiar from Russian school, but the correct option is the only two-part one and gives itself away. One-word nationalities fix the telegraph.
- Rewrite (69 NEEDS IMPROVEMENT): RU: Кем был прадед Пушкина Абрам Ганнибал? / EN: Who was Pushkin's great-grandfather Abram Gannibal?
  - Options: **Африканцем | An African** / Шотландцем | A Scot / Турком | A Turk / Венецианцем | A Venetian
  - Why better: Equal one-word options mean the answer comes from knowledge, not from the extra 'Peter I's godson' detail.

### books_old_033 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Кто в «Мастере и Маргарите» говорит «Рукописи не горят»?
- EN: Who says "Manuscripts don't burn" in The Master and Margarita?
- Options: **Воланд | Woland** / Мастер | The Master / Маргарита | Margarita / Коровьев | Koroviev
- Scores: F 5, C 2, G 3, D 5, E 3, A 5, P 4
- Verdict: Famous quote with the perfect trap (the Master burns his manuscript), and Bulgakov burning his own draft is a fitting payoff.

### books_old_032 (multipleChoice, medium): 71 GOOD, KEEP

- RU: Каким звуком заканчивается «Вишнёвый сад» Чехова?
- EN: What sound ends Chekhov's The Cherry Orchard?
- Options: **Стуком топора | An axe chopping** / Выстрелом | A gunshot / Колоколом | A church bell / Свистом поезда | A train whistle
- Scores: F 4, C 2, G 4, D 4, E 3, A 5, P 4
- Verdict: Logical path (the orchard is cut down) rewards reasoning, and Chekhov being horrified that it was staged as a tragedy sparks discussion.

### books_old_041 (closestValue, hard): 74 GOOD, KEEP

- RU: Сколько лет было Льву Толстому, когда он ушёл из дома и умер в пути?
- EN: How old was Leo Tolstoy when he left home and died on the road?
- Answer: **82** years old, range 30 to 110, tolerance 7
- Scores: F 5, C 2, G 4, D 4, E 3, A 5, P 4
- Verdict: Everyone knows Tolstoy lived long, so the estimate is fair, and the station now being called 'Lev Tolstoy' is a nice payoff.

### books_old_045 (closestValue, hard): 64 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году «Мастер и Маргарита» впервые вышла в печати?
- EN: In which year did The Master and Margarita first appear in print?
- Answer: **1966** year, range 1900 to 2000, tolerance 10
- Scores: F 4, C 3, G 3, D 3, E 2, A 5, P 3. Flags: SCHOOL_EXAM
- Verdict: The interesting part (published 26 years after Bulgakov's death) is hidden in the fact. Giving the death year makes the long delay the thing players estimate.
- Rewrite (71 GOOD): RU: Булгаков умер в 1940-м, так и не увидев «Мастера и Маргариту» в печати. Когда роман вышел? / EN: Bulgakov died in 1940 without seeing The Master and Margarita in print. When did it come out?
  - Answer 1966, range 1900 to 2000, tolerance 8
  - Why better: The death year gives an anchor and turns the question into 'how long did censorship hold it back', which is the real story.

## books_new

### books_new_005 (multipleChoice, easy): 70 GOOD, KEEP

- RU: Чей девиз «Зима близко» в «Песни льда и пламени»?
- EN: In A Song of Ice and Fire, whose words are "Winter is coming"?
- Options: **Старков | House Stark** / Ланнистеров | House Lannister / Таргариенов | House Targaryen / Баратеонов | House Baratheon
- Scores: F 5, C 1, G 4, D 4, E 3, A 5, P 4
- Verdict: Deliberate easy recognition for fans of the show; the payoff that 'A Lannister always pays his debts' is not their official motto is a good twist.

### books_new_012 (multipleChoice, easy): 58 NEEDS IMPROVEMENT, IMPROVE

- RU: Каким проклятием Белая Колдунья сковала Нарнию?
- EN: What curse did the White Witch put on Narnia?
- Options: Вечная ночь | Endless night / Нескончаемый дождь | Endless rain / Засуха без конца | Endless drought / **Вечная зима без Рождества | Always winter, never Christmas**
- Scores: F 4, C 2, G 3, D 1, E 2, A 5, P 4
- Verdict: The correct option is the longest and the only one echoing a 'White' witch's winter, so it is given away. Parallel 'X without Y' options hide it.
- Rewrite (67 NEEDS IMPROVEMENT): RU: Каким проклятием Белая Колдунья сковала Нарнию? / EN: What curse did the White Witch put on Narnia?
  - Options: **Зима без Рождества | Winter without Christmas** / Лето без дождя | Summer without rain / Ночь без рассвета | Night without dawn / Сон без пробуждения | Sleep without waking
  - Why better: Four equal 'X without Y' curses remove the length telegraph, so fans must actually remember the book.

### books_new_002 (multipleChoice, easy): 59 NEEDS IMPROVEMENT, KEEP

- RU: Где во «Властелине колец» можно уничтожить Кольцо Всевластья?
- EN: Where in The Lord of the Rings can the One Ring be destroyed?
- Options: В Ривенделле | In Rivendell / **В жерле Роковой горы | In the fires of Mount Doom** / В Минас-Тирите | In Minas Tirith / В Шире | In the Shire
- Scores: F 5, C 0, G 4, D 3, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER, BORING_REVEAL
- Verdict: Deliberate easy recognition; the correct option is the only one mentioning fire and the fact retells the plot. A shorter correct option removes the telegraph.
- Rewrite (61 NEEDS IMPROVEMENT): RU: Где во «Властелине колец» можно уничтожить Кольцо Всевластья? / EN: Where in The Lord of the Rings can the One Ring be destroyed?
  - Options: **В Роковой горе | In Mount Doom** / В Ривенделле | In Rivendell / В Минас-Тирите | In Minas Tirith / В Шире | In the Shire
  - Why better: Equal-length place names so the answer is recalled rather than read off the option's wording.

### books_new_029 (multipleChoice, medium): 61 NEEDS IMPROVEMENT, IMPROVE

- RU: О каком мире роман Виктора Пелевина «Generation „П“»?
- EN: Which world is Victor Pelevin's novel Generation P about?
- Options: **О рекламе | Advertising** / О шахтёрах | Coal mining / О космонавтике | Space flight / О футболе | Football
- Scores: F 3, C 2, G 3, D 3, E 2, A 5, P 5
- Verdict: Known to the older players; the Vavilen name fact is the gem but the question does not set it up. Naming the hero in the question makes the fact explain a name players just heard.
- Rewrite (70 GOOD): RU: Кем работает Вавилен Татарский, герой «Generation „П“» Пелевина? / EN: What does Vavilen Tatarsky, hero of Pelevin's Generation P, do for a living?
  - Options: **Пишет рекламу | Writes adverts** / Водит такси | Drives a taxi / Снимает кино | Makes films / Играет в группе | Plays in a band
  - Why better: The odd name in the question makes players curious, so the Aksyonov + Lenin explanation lands as a punchline.

### books_new_023 (multipleChoice, medium): 74 GOOD, KEEP

- RU: Как звали Северуса Снейпа в первом русском переводе «Гарри Поттера» от «Росмэна»?
- EN: What was Snape called in Rosman's first Russian translation of Harry Potter?
- Options: Змеев | Zmeev / Снайперов | Snaiperov / **Снегг | Snegg** / Северов | Severov
- Scores: F 4, C 3, G 3, D 4, E 4, A 5, P 4
- Verdict: The young players grew up with 'Snegg', the decoys are funny plausible translations, and Zloteus Zley is a great payoff.

### books_new_018 (multipleChoice, medium): 79 GOOD, KEEP

- RU: В честь кого Ян Флеминг назвал Джеймса Бонда?
- EN: Who was James Bond named after?
- Options: Шпиона времён войны | A wartime spy / **Орнитолога | A bird expert** / Дяди Флеминга | Fleming's uncle / Футболиста | A footballer
- Scores: F 5, C 5, G 1, D 4, E 4, A 5, P 5
- Verdict: Pure surprise: 007 named after a bird expert because Fleming wanted the dullest name possible. Hard to deduce, but everyone wants to retell it.

### books_new_032 (multipleChoice, medium): 72 GOOD, KEEP

- RU: Где Роулинг впервые пришла в голову идея «Гарри Поттера»?
- EN: Where did J. K. Rowling first get the idea for Harry Potter?
- Options: В кафе в Эдинбурге | In an Edinburgh café / Во сне | In a dream / На экскурсии по замку | On a castle tour / **В опаздывающем поезде | On a delayed train**
- Scores: F 5, C 3, G 2, D 5, E 3, A 5, P 4
- Verdict: The Edinburgh café myth is a perfect trap, and the four-hour delayed train with no pen is a relatable payoff.

### books_new_037 (multipleChoice, medium): 71 GOOD, KEEP

- RU: В честь чего назван роман Харуки Мураками «Норвежский лес»?
- EN: What is Haruki Murakami's novel Norwegian Wood named after?
- Options: **Песни «Битлз» | A Beatles song** / Леса в Норвегии | A forest in Norway / Мебельной фабрики | A furniture factory / Картины Мунка | A painting by Munch
- Scores: F 4, C 3, G 3, D 4, E 3, A 5, P 4
- Verdict: Many know the Beatles song and can connect it; the baseball-game epiphany is a fun story even if it is about Murakami rather than the title.

### books_new_044 (closestValue, hard): 58 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году происходит действие романа Умберто Эко «Имя розы»?
- EN: In which year is Umberto Eco's The Name of the Rose set?
- Answer: **1327** year, range 1100 to 2000, tolerance 40
- Scores: F 2, C 2, G 3, D 3, E 2, A 5, P 5. Flags: NO_DEDUCTION
- Verdict: 'Middle Ages' gives an anchor, but the year itself is dull; the 'felt like poisoning a monk' line is the real gem. Framing the novel as a monastery murder mystery gives context and a tighter tolerance makes the estimate count.
- Rewrite (64 NEEDS IMPROVEMENT): RU: «Имя розы» Эко — детектив в средневековом монастыре. В каком году идёт действие? / EN: Eco's The Name of the Rose is a murder mystery in a medieval abbey. In what year is it set?
  - Answer 1327, range 1000 to 2000, tolerance 40
  - Why better: The murder-mystery framing gives context for everyone who has not read it, and the 'poison a monk' quote stays as the payoff.

### books_new_050 (closestValue, hard): 63 NEEDS IMPROVEMENT, IMPROVE

- RU: Сколько примерно книг написал или составил Айзек Азимов?
- EN: About how many books did Isaac Asimov write or edit?
- Answer: **500** books, range 0 to 2000, tolerance 100
- Scores: F 2, C 4, G 2, D 3, E 3, A 5, P 4
- Verdict: Calibration anchor: a surprising number, but the name is only half familiar to young players and there is little to reason from. Naming his best-known book helps familiarity and 'incredibly prolific' gives a scale hint.
- Rewrite (70 GOOD): RU: Фантаст Айзек Азимов («Я, робот») был невероятно плодовит. Сколько книг он написал или составил? / EN: Sci-fi writer Isaac Asimov (I, Robot) was incredibly prolific. About how many books did he write or edit?
  - Answer 500, range 0 to 2000, tolerance 100
  - Why better: 'I, Robot' makes the name click for younger players, and 'incredibly prolific' pushes guesses into the hundreds.

## fairytales

### fairytales_005 (multipleChoice, easy): 65 NEEDS IMPROVEMENT, KEEP

- RU: Как зовут мальчика, лучшего друга Винни-Пуха?
- EN: What is the name of Winnie the Pooh's best friend, the boy?
- Options: **Кристофер Робин | Christopher Robin** / Питер Пэн | Peter Pan / Том Сойер | Tom Sawyer / Оливер Твист | Oliver Twist
- Scores: F 5, C 1, G 4, D 4, E 2, A 5, P 3. Flags: OBVIOUS_ANSWER
- Verdict: Deliberate easy recognition; the real son and teddy bear fact is fairly well known but pleasant.

### fairytales_009 (multipleChoice, easy): 70 GOOD, KEEP

- RU: На чём Емеля приехал к царю «по щучьему веленью»?
- EN: What did Emelya ride to the tsar's palace, by the pike's command?
- Options: **На печи | On the stove** / На ковре-самолёте | On a flying carpet / На медведе | On a bear / На сером волке | On a grey wolf
- Scores: F 5, C 1, G 4, D 4, E 3, A 5, P 4
- Verdict: Easy recognition with fun fairy-tale decoys; the dark barrel-in-the-sea ending from Afanasyev surprises people who only know the cartoon.

### fairytales_014 (multipleChoice, easy): 74 GOOD, KEEP

- RU: Из-за чего Иван-царевич потерял Царевну-лягушку?
- EN: Why did Ivan Tsarevich lose the Frog Princess?
- Options: **Сжёг её лягушачью кожу | He burned her frog skin** / Выдал её тайну отцу | He told his father her secret / Не поцеловал её | He never kissed her / Посмеялся над ней | He laughed at her
- Scores: F 5, C 2, G 4, D 4, E 3, A 5, P 4
- Verdict: Familiar tale with a good Western trap (the kiss), and 'had he waited a little longer' is a satisfying payoff.

### fairytales_035 (multipleChoice, medium): 78 GOOD, KEEP

- RU: Добрались ли бременские музыканты до Бремена в сказке братьев Гримм?
- EN: In the Grimms' tale, did the Town Musicians of Bremen ever reach Bremen?
- Options: **Нет, остались в доме разбойников | No, they stayed in the robbers' house** / Да, и стали городским оркестром | Yes, and became the town band / Да, но их прогнали | Yes, but they were chased out / Нет, вернулись к хозяевам | No, they went back to their owners
- Scores: F 4, C 5, G 3, D 3, E 4, A 4, P 4
- Verdict: Genuine surprise: most assume they reach Bremen (or know only the Soviet cartoon), and the city loving them anyway is a sweet payoff.

### fairytales_018 (multipleChoice, medium): 69 NEEDS IMPROVEMENT, IMPROVE

- RU: Как зовут итальянского «брата» Буратино?
- EN: Who is Buratino's Italian original?
- Options: Чиполлино | Cipollino / **Пиноккио | Pinocchio** / Джеппетто | Geppetto / Карабас | Karabas
- Scores: F 5, C 1, G 5, D 3, E 2, A 5, P 4
- Verdict: Too easy as asked: Geppetto is Pinocchio's father and Karabas is Buratino's villain, so only Cipollino competes. All-Italian '-ino' style names make players think, and the burattino = puppet fact pays off.
- Rewrite (72 GOOD): RU: Как зовут итальянского «брата» Буратино? / EN: Who is Buratino's Italian original?
  - Options: **Пиноккио | Pinocchio** / Чиполлино | Cipollino / Арлекино | Harlequin / Пульчинелла | Pulcinella
  - Why better: Four Italian puppet-or-tale names mean players must actually recall the original rather than eliminate side characters.

### fairytales_024 (multipleChoice, medium): 83 GOOD, KEEP

- RU: Чем Коллоди закончил «Пиноккио», когда печатал его в журнале в 1881 году?
- EN: How did Collodi first end Pinocchio in the 1881 magazine serial?
- Options: **Его повесили на дубе | He was hanged from an oak** / Он стал мальчиком | He became a real boy / Он сгорел в очаге | He burned in the fireplace / Его проглотила акула | A shark swallowed him
- Scores: F 4, C 5, G 2, D 4, E 5, A 5, P 5
- Verdict: Shocking reveal (the author hanged Pinocchio) that gets a strong reaction, plus readers rescuing him is a great story.

### fairytales_029 (multipleChoice, medium): 68 NEEDS IMPROVEMENT, IMPROVE

- RU: У какого писателя Пушкин позаимствовал сюжет «Сказки о золотом петушке»?
- EN: Whose story did Pushkin borrow for The Tale of the Golden Cockerel?
- Options: **Вашингтона Ирвинга | Washington Irving** / Братьев Гримм | The Brothers Grimm / Шарля Перро | Charles Perrault / Ганса Христиана Андерсена | Hans Christian Andersen
- Scores: F 3, C 5, G 1, D 4, E 3, A 5, P 4
- Verdict: Very surprising source, but Andersen is an anachronism for 1834 and Irving is pure memorisation. A Walter Scott decoy (Pushkin's favourite) makes the British/European options more tempting.
- Rewrite (72 GOOD): RU: У какого писателя Пушкин позаимствовал сюжет «Сказки о золотом петушке»? / EN: Whose story did Pushkin borrow for The Tale of the Golden Cockerel?
  - Options: **Вашингтона Ирвинга | Washington Irving** / Вальтера Скотта | Walter Scott / Братьев Гримм | The Brothers Grimm / Шарля Перро | Charles Perrault
  - Why better: Walter Scott, whom Pushkin admired, is a believable trap, so the American Irving lands as a bigger surprise.

### fairytales_028 (multipleChoice, medium): 77 GOOD, KEEP

- RU: Английские шляпники XIX века и правда часто сходили с ума. Что их травило?
- EN: Hatters in Carroll's England really did go mad. What poisoned them?
- Options: **Ртуть | Mercury** / Угольный дым | Coal smoke / Крахмальная пыль | Starch dust / Нюхательный табак | Snuff
- Scores: F 4, C 4, G 4, D 3, E 3, A 5, P 4
- Verdict: Satisfying 'mad as a hatter' explanation with a reasoning path (toxic chemicals), but 'Mercury' is much shorter than the decoys. 'Mercury fumes' balances the lengths.
- Rewrite (79 GOOD): RU: Английские шляпники XIX века и правда часто сходили с ума. Что их травило? / EN: Hatters in Carroll's England really did go mad. What poisoned them?
  - Options: **Пары ртути | Mercury fumes** / Угольный дым | Coal smoke / Крахмальная пыль | Starch dust / Нюхательный табак | Snuff
  - Why better: Equal-length two-word options so the short answer does not stand out.

### fairytales_049 (closestValue, hard): 77 GOOD, KEEP

- RU: Сколько тюфяков положили поверх горошины в «Принцессе на горошине»?
- EN: In The Princess and the Pea, how many mattresses were piled on the pea?
- Answer: **20** mattresses, range 0 to 100, tolerance 10
- Scores: F 5, C 3, G 3, D 4, E 4, A 5, P 4
- Verdict: Everyone knows the tale but not the number, so the guesses vary widely and spark laughter; the pea in the museum 'if no one stole it' is a charming payoff.

### fairytales_042 (closestValue, hard): 62 NEEDS IMPROVEMENT, IMPROVE

- RU: В каком году вышла первая книга Алана Милна о Винни-Пухе?
- EN: In which year did A. A. Milne's first Winnie-the-Pooh book come out?
- Answer: **1926** year, range 1880 to 2020, tolerance 12
- Scores: F 4, C 2, G 3, D 3, E 2, A 5, P 4. Flags: SCHOOL_EXAM
- Verdict: Bare publication date; the toys-in-New-York fact is the real payoff. Linking the question to the real teddy bear gives a little warmth and context.
- Rewrite (65 NEEDS IMPROVEMENT): RU: Плюшевый мишка Кристофера Робина стал Винни-Пухом. В каком году вышла первая книга о нём? / EN: Christopher Robin's teddy bear became Winnie-the-Pooh. In what year did the first book come out?
  - Answer 1926, range 1880 to 2020, tolerance 12
  - Why better: The real-teddy framing makes the date feel connected to a story, setting up the real-toys-in-New-York fact.
