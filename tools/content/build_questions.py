# Generates content/questions.json, race.json and topics.json: the question bank written to surprise, to be worked out
# rather than remembered and to start arguments on the sofa (most multiple choice questions have one joke option), every
# question with a one line why fact and a source. Run from the repo root:  python tools/content/build_questions.py .
# It checks the length limits of 15_schemas.md, closest value ranges and that every topic can fill a round; then run
# node tools/validate_content.mjs. The data below is grouped by where it was first written (v2), then remapped onto
# the topic bank (v3); ids are given in order per topic.
import json, sys

W = "https://en.wikipedia.org/wiki/"
MC, CV = {}, {}

MC["geography"] = [
 ("geo_001", "easy", "Какая страна на карте похожа на сапог?", "Which country looks like a boot on the map?",
  [("Италия", "Italy"), ("Чили", "Chile"), ("Норвегия", "Norway"), ("Япония", "Japan")], 0,
  "Носок «сапога» почти пинает Сицилию, как мяч.", "The boot's toe looks like it is kicking Sicily like a ball.", W + "Italy"),
 ("geo_002", "easy", "Какой город стоит сразу в двух частях света?", "Which city lies on two continents at once?",
  [("Каир", "Cairo"), ("Стамбул", "Istanbul"), ("Афины", "Athens"), ("Москва", "Moscow")], 1,
  "Пролив Босфор делит Стамбул на европейскую и азиатскую части.", "The Bosphorus splits Istanbul into a European and an Asian half.", W + "Istanbul"),
 ("geo_003", "easy", "Какая страна целиком помещается внутри одного города, Рима?", "Which country fits entirely inside one city, Rome?",
  [("Монако", "Monaco"), ("Сан-Марино", "San Marino"), ("Ватикан", "Vatican City"), ("Мальта", "Malta")], 2,
  "Ватикан меньше половины квадратного километра: его можно обойти за час.", "Vatican City is under half a square kilometre: you can walk round it in an hour.", W + "Vatican_City"),
 ("geo_004", "medium", "В какой стране больше всего часовых поясов, если считать её заморские земли?", "Which country has the most time zones, counting its overseas lands?",
  [("Россия", "Russia"), ("США", "USA"), ("Китай", "China"), ("Франция", "France")], 3,
  "Острова от Карибов до Тихого океана дают Франции 12 часовых поясов.", "Islands from the Caribbean to the Pacific give France 12 time zones.", W + "Time_in_France"),
 ("geo_005", "medium", "В какой стране больше всего озёр?", "Which country has the most lakes?",
  [("Финляндия", "Finland"), ("Россия", "Russia"), ("Канада", "Canada"), ("США", "USA")], 2,
  "В Канаде больше озёр, чем во всех остальных странах мира, вместе взятых.", "Canada has more lakes than all the other countries in the world put together.", W + "Lakes_of_Canada"),
 ("geo_006", "medium", "Какая пустыня самая большая на Земле?", "Which is the biggest desert on Earth?",
  [("Сахара", "Sahara"), ("Антарктида", "Antarctica"), ("Гоби", "Gobi"), ("Калахари", "Kalahari")], 1,
  "Пустыня — это место, где почти нет осадков, а не где жарко. В Антарктиде их меньше, чем в Сахаре.", "A desert is a place with almost no rain or snow, not a hot place. Antarctica gets less than the Sahara.", W + "Antarctica"),
 ("geo_007", "medium", "Какая река течёт через больше всего стран?", "Which river flows through the most countries?",
  [("Нил", "Nile"), ("Амазонка", "Amazon"), ("Рейн", "Rhine"), ("Дунай", "Danube")], 3,
  "Дунай проходит через десять стран и четыре столицы.", "The Danube runs through ten countries and four capitals.", W + "Danube"),
 ("geo_008", "medium", "Какая столица — самая южная в мире?", "Which is the southernmost capital in the world?",
  [("Веллингтон", "Wellington"), ("Канберра", "Canberra"), ("Буэнос-Айрес", "Buenos Aires"), ("Кейптаун", "Cape Town")], 0,
  "Веллингтон в Новой Зеландии — ещё и одна из самых ветреных столиц мира.", "Wellington in New Zealand is also one of the windiest capitals on Earth.", W + "Wellington"),
]
CV["geography"] = [
 ("geo_009", "Какова длина Великой Китайской стены со всеми ветками, в километрах?", "How long is the Great Wall of China with all its branches, in kilometres?",
  21196, (1000, 40000), 100, 12000, ("км", "km"),
  "Это больше половины длины экватора.", "That is more than half the length of the equator.", W + "Great_Wall_of_China"),
 ("geo_010", "Сколько стран в Африке?", "How many countries are there in Africa?",
  54, (10, 100), 1, 30, ("", ""),
  "Больше, чем на любом другом континенте.", "More than on any other continent.", W + "List_of_African_countries_by_area"),
 ("geo_011", "Какова глубина Марианской впадины в метрах?", "How deep is the Mariana Trench, in metres?",
  10935, (2000, 20000), 50, 6000, ("м", "m"),
  "Если опустить туда Эверест, над его вершиной осталось бы ещё два километра воды.", "Drop Everest in and there would still be two kilometres of water above its top.", W + "Challenger_Deep"),
]

MC["history"] = [
 ("his_001", "easy", "Кем тайно работал Пётр I, когда учился в Голландии?", "What job did Peter the Great secretly do while learning in Holland?",
  [("Корабельным плотником", "Ship carpenter"), ("Поваром", "Cook"), ("Часовщиком", "Watchmaker"), ("Пекарем", "Baker")], 0,
  "Царь записался на верфь под чужим именем, чтобы научиться строить флот.", "The tsar signed on at a shipyard under a false name to learn how to build a fleet.", W + "Grand_Embassy_of_Peter_the_Great"),
 ("his_002", "easy", "Кто слетал в космос и вернулся живым раньше Гагарина?", "Who flew to space and came back alive before Gagarin?",
  [("Кот Барсик", "Barsik the cat"), ("Собаки Белка и Стрелка", "The dogs Belka and Strelka"), ("Попугай Кеша", "Kesha the parrot"), ("Хомяк Пушок", "Fluffy the hamster")], 1,
  "Они облетели Землю в 1960 году. Щенка Стрелки потом подарили семье Кеннеди.", "They orbited the Earth in 1960. One of Strelka's puppies was later given to the Kennedy family.", W + "Belka_and_Strelka"),
 ("his_003", "easy", "Чем писали школьники в древнем Новгороде?", "What did schoolchildren in old Novgorod write with?",
  [("Пером на бумаге", "A quill on paper"), ("Мелом на доске", "Chalk on a board"), ("Палочкой на бересте", "A stick on birch bark"), ("Углём на стене", "Charcoal on a wall")], 2,
  "Мальчик Онфим 800 лет назад рисовал на бересте себя в виде рыцаря вместо уроков.", "800 years ago a boy called Onfim drew himself as a knight on birch bark instead of doing lessons.", W + "Onfim"),
 ("his_004", "medium", "Клеопатра жила ближе по времени к чему?", "Cleopatra lived closer in time to what?",
  [("Постройке пирамиды Хеопса", "The building of the Great Pyramid"), ("Высадке людей на Луну", "The Moon landing"), ("Изобретению колеса", "The invention of the wheel"), ("Ко всему одинаково", "All of them equally")], 1,
  "Пирамиде было уже 2500 лет, когда родилась Клеопатра.", "The pyramid was already 2,500 years old when Cleopatra was born.", W + "Cleopatra"),
 ("his_005", "medium", "Что из этого появилось раньше всех?", "Which of these came first?",
  [("Город ацтеков Теночтитлан", "The Aztec city of Tenochtitlan"), ("Мачу-Пикчу", "Machu Picchu"), ("Кирпичный Московский Кремль", "Moscow's brick Kremlin"), ("Оксфордский университет", "Oxford University")], 3,
  "В Оксфорде учили уже в 1096 году, Теночтитлан основали в 1325-м.", "Oxford was teaching by 1096; Tenochtitlan was founded in 1325.", W + "University_of_Oxford"),
 ("his_006", "medium", "Сколько длилась самая короткая война в истории?", "How long did the shortest war in history last?",
  [("Меньше часа", "Under an hour"), ("Три дня", "Three days"), ("Неделю", "A week"), ("Один обеденный перерыв в Англии", "One English tea break")], 0,
  "Англия и Занзибар в 1896 году: от 38 до 45 минут.", "Britain against Zanzibar in 1896: 38 to 45 minutes.", W + "Anglo-Zanzibar_War"),
 ("his_007", "medium", "Какого роста был Наполеон?", "How tall was Napoleon?",
  [("Около 1 м 50 см, совсем маленький", "About 1.50 m, tiny"), ("Около 1 м 69 см, как все", "About 1.69 m, average"), ("Около 1 м 90 см, великан", "About 1.90 m, a giant"), ("Он всегда был на коне", "He was always on a horse")], 1,
  "Миф о маленьком Наполеоне появился из-за разницы французских и английских дюймов и карикатур.", "The myth of a tiny Napoleon came from French versus English inches, and from cartoons.", W + "Napoleon"),
 ("his_008", "medium", "Что изобрели раньше: факс или телефон?", "Which was invented first: the fax or the telephone?",
  [("Телефон", "The telephone"), ("Факс", "The fax"), ("Их изобрели в один день", "Both on the same day"), ("Сначала изобрели радио", "Radio came before both")], 1,
  "Первый факс запатентовали в 1843 году, а телефон — только в 1876-м.", "The first fax was patented in 1843; the telephone only in 1876.", W + "Fax"),
]
CV["history"] = [
 ("his_009", "Сколько лет на самом деле длилась Столетняя война?", "How many years did the Hundred Years' War actually last?",
  116, (50, 200), 1, 50, ("лет", "years"),
  "С 1337 по 1453 год, с перерывами.", "From 1337 to 1453, with breaks.", W + "Hundred_Years%27_War"),
 ("his_010", "В каком году построили Эйфелеву башню?", "In which year was the Eiffel Tower built?",
  1889, (1700, 2000), 1, 80, ("год", "year"),
  "Её разрешили поставить всего на 20 лет, а потом собирались разобрать.", "It was only allowed to stand for 20 years and was meant to be taken down after that.", W + "Eiffel_Tower"),
 ("his_011", "Сколько лет назад жили последние мамонты?", "How many years ago did the last mammoths live?",
  4000, (0, 20000), 100, 3500, ("лет назад", "years ago"),
  "Когда в Египте уже стояли пирамиды, на острове Врангеля ещё ходили мамонты.", "The pyramids were already standing in Egypt while mammoths still walked on Wrangel Island.", W + "Woolly_mammoth"),
]

MC["science"] = [
 ("sci_001", "easy", "Что тяжелее: килограмм ваты или килограмм железа?", "Which is heavier: a kilogram of cotton wool or a kilogram of iron?",
  [("Железо", "The iron"), ("Вата", "The cotton wool"), ("Одинаково", "They weigh the same"), ("Зависит от погоды", "It depends on the weather")], 2,
  "Килограмм — это килограмм. Но мешок ваты будет размером с диван.", "A kilogram is a kilogram. The bag of cotton wool would just be the size of a sofa.", W + "Kilogram"),
 ("sci_002", "easy", "Почему небо голубое?", "Why is the sky blue?",
  [("В нём отражается море", "It reflects the sea"), ("Воздух рассеивает синий свет сильнее", "Air scatters blue light the most"), ("Космос голубого цвета", "Space is blue"), ("Его так видят только люди", "Only humans see it that way")], 1,
  "Синие лучи короче и разлетаются по всему небу, а на закате до нас доходят красные.", "Blue light scatters all over the sky; at sunset only the red gets through to us.", W + "Rayleigh_scattering"),
 ("sci_003", "easy", "Из чего сделан грифель простого карандаша?", "What is the lead in a pencil really made of?",
  [("Из свинца", "Lead"), ("Из угля", "Coal"), ("Из графита", "Graphite"), ("Из олова", "Tin")], 2,
  "Свинца там никогда не было: графит с глиной. Название осталось с древности.", "There was never any lead: it is graphite mixed with clay. The name is ancient.", W + "Pencil"),
 ("sci_004", "medium", "Какой металл лучше всех проводит электричество?", "Which metal conducts electricity best?",
  [("Медь", "Copper"), ("Золото", "Gold"), ("Серебро", "Silver"), ("Алюминий", "Aluminium")], 2,
  "Провода делают из меди только потому, что серебро слишком дорогое.", "Wires are copper only because silver is too expensive.", W + "Electrical_resistivity_and_conductivity"),
 ("sci_005", "medium", "Какая живая часть тела обходится совсем без кровеносных сосудов?", "Which living part of the body has no blood vessels at all?",
  [("Роговица глаза", "The cornea of the eye"), ("Мизинец", "The little finger"), ("Мочка уха", "The earlobe"), ("Язык", "The tongue")], 0,
  "Кислород роговица берёт прямо из воздуха.", "The cornea takes its oxygen straight from the air.", W + "Cornea"),
 ("sci_006", "medium", "Сколько весит обычное пушистое облако?", "How much does an ordinary fluffy cloud weigh?",
  [("Меньше килограмма", "Less than a kilogram"), ("Как легковая машина", "As much as a car"), ("Как сто слонов", "As much as a hundred elephants"), ("Ничего, это пар", "Nothing, it is steam")], 2,
  "Около 500 тонн воды висит в воздухе крошечными каплями.", "About 500 tonnes of water hanging in the air as tiny droplets.", "https://www.usgs.gov/special-topics/water-science-school/science/how-much-does-a-cloud-weigh"),
 ("sci_007", "medium", "Какой металл жидкий при комнатной температуре?", "Which metal is liquid at room temperature?",
  [("Олово", "Tin"), ("Ртуть", "Mercury"), ("Алюминий", "Aluminium"), ("Серебро", "Silver")], 1,
  "Ртуть замерзает только при минус 39 градусах.", "Mercury only freezes at minus 39 degrees.", W + "Mercury_(element)"),
 ("sci_008", "medium", "Какое лекарство открыли, потому что учёный не помыл посуду перед отпуском?", "Which medicine was found because a scientist left dirty dishes before a holiday?",
  [("Пенициллин", "Penicillin"), ("Аспирин", "Aspirin"), ("Зелёнку", "Brilliant green"), ("Валерьянку", "Valerian drops")], 0,
  "Флеминг вернулся в 1928 году и увидел, что плесень убила бактерии в чашке.", "Fleming came back in 1928 to find that mould had killed the bacteria in a dish.", W + "Penicillin"),
]
CV["science"] = [
 ("sci_009", "Сколько километров пролетает свет за одну секунду?", "How many kilometres does light travel in one second?",
  300000, (10000, 600000), 1000, 150000, ("км", "km"),
  "За секунду свет облетает Землю семь с половиной раз.", "In one second light could go round the Earth seven and a half times.", W + "Speed_of_light"),
 ("sci_010", "Сколько километров кровеносных сосудов в теле взрослого человека?", "How many kilometres of blood vessels are in an adult's body?",
  100000, (1000, 200000), 1000, 50000, ("км", "km"),
  "Если вытянуть их в одну нитку, она обмотает Землю два с половиной раза.", "Laid end to end they would wrap round the Earth two and a half times.", W + "Blood_vessel"),
 ("sci_011", "Сколько процентов тела взрослого человека — вода?", "What percentage of an adult's body is water?",
  60, (0, 100), 1, 30, ("%", "%"),
  "У младенцев воды ещё больше: около 75 процентов.", "Babies are even wetter: about 75 percent.", W + "Body_water"),
]

MC["nature"] = [
 ("nat_001", "easy", "Какой формы какашки у вомбата?", "What shape is wombat poop?",
  [("Звёздочки", "Stars"), ("Кубики", "Cubes"), ("Спиральки", "Spirals"), ("Сердечки", "Hearts")], 1,
  "Кубики не скатываются с камней, а вомбаты метят ими свою территорию.", "Cubes do not roll off rocks, and wombats use them to mark their territory.", W + "Wombat"),
 ("nat_002", "easy", "Почему фламинго розовые?", "Why are flamingos pink?",
  [("Из-за еды: рачков и водорослей", "Their food: shrimp and algae"), ("От смущения", "They are embarrassed"), ("Загорают на солнце", "Sunburn"), ("Красят друг друга клювами", "They paint each other")], 0,
  "Птенцы фламинго рождаются серыми и розовеют от того, что едят.", "Flamingo chicks are born grey and turn pink from what they eat.", W + "Flamingo"),
 ("nat_003", "easy", "Сколько сердец у осьминога?", "How many hearts does an octopus have?",
  [("Одно", "One"), ("Два", "Two"), ("Три", "Three"), ("Восемь, по одному на щупальце", "Eight, one per arm")], 2,
  "А ещё у него голубая кровь.", "And its blood is blue.", W + "Octopus"),
 ("nat_004", "medium", "Кто спит на воде, держась за лапы, чтобы не уплыть друг от друга?", "Who sleeps on the water holding paws so they do not drift apart?",
  [("Бобры", "Beavers"), ("Пингвины", "Penguins"), ("Каланы", "Sea otters"), ("Тюлени", "Seals")], 2,
  "Иногда каланы ещё и заворачиваются в водоросли, как в одеяло.", "Sometimes sea otters also wrap themselves in kelp like a blanket.", W + "Sea_otter"),
 ("nat_005", "medium", "Кто почти никогда не пьёт воду, потому что получает её из листьев?", "Who almost never drinks, because it gets its water from leaves?",
  [("Верблюд", "The camel"), ("Коала", "The koala"), ("Панда", "The panda"), ("Ленивец", "The sloth")], 1,
  "Коалам хватает воды из эвкалипта. Верблюды как раз пьют много, просто редко.", "Koalas get enough water from eucalyptus. Camels drink a lot, just not often.", W + "Koala"),
 ("nat_006", "medium", "Кто может повернуть голову почти на три четверти круга?", "Who can turn its head almost three quarters of the way round?",
  [("Жираф", "The giraffe"), ("Кошка", "The cat"), ("Сова", "The owl"), ("Хамелеон", "The chameleon")], 2,
  "У совы глаза не двигаются, поэтому крутится вся голова: до 270 градусов.", "An owl's eyes cannot move, so its whole head turns: up to 270 degrees.", W + "Owl"),
 ("nat_007", "medium", "Какого цвета кожа у белого медведя под шерстью?", "What colour is a polar bear's skin under its fur?",
  [("Белая", "White"), ("Розовая", "Pink"), ("Чёрная", "Black"), ("Голубая", "Light blue")], 2,
  "А шерсть у него прозрачная: она только кажется белой.", "And its fur is see-through: it only looks white.", W + "Polar_bear"),
 ("nat_008", "medium", "Кто из этих животных «смеётся», когда его щекочут?", "Which of these animals laughs when it is tickled?",
  [("Крысы", "Rats"), ("Черепахи", "Tortoises"), ("Золотые рыбки", "Goldfish"), ("Ящерицы", "Lizards")], 0,
  "Их смех такой высокий, что человек его не слышит без приборов.", "Their laugh is so high pitched that people cannot hear it without equipment.", W + "Rat"),
]
CV["nature"] = [
 ("nat_009", "Сколько раз в минуту может биться сердце колибри?", "How many times a minute can a hummingbird's heart beat?",
  1200, (100, 3000), 10, 900, ("ударов", "beats"),
  "У человека в покое около 70 ударов в минуту.", "A resting person's heart beats about 70 times a minute.", W + "Hummingbird"),
 ("nat_010", "С какой скоростью может бежать гепард, в км/ч?", "How fast can a cheetah run, in km/h?",
  100, (10, 300), 1, 40, ("км/ч", "km/h"),
  "Быстрее машины в городе, но такой рывок он держит всего полминуты.", "Faster than a car in town, but it can only keep it up for half a minute.", W + "Cheetah"),
 ("nat_011", "Сколько лет может прожить гренландская акула?", "How many years can a Greenland shark live?",
  390, (10, 1000), 5, 300, ("лет", "years"),
  "Некоторые из живущих сейчас акул плавали ещё при Петре I.", "Some sharks alive today were already swimming when Peter the Great ruled.", W + "Greenland_shark"),
]

MC["food"] = [
 ("food_001", "easy", "Что делает борщ красным?", "What makes borscht red?",
  [("Помидоры", "Tomatoes"), ("Свёкла", "Beetroot"), ("Малиновое варенье", "Raspberry jam"), ("Красный перец", "Red pepper")], 1,
  "Каплю уксуса или лимона добавляют, чтобы свёкла не побурела.", "A drop of vinegar or lemon keeps the beetroot from turning brown.", W + "Borscht"),
 ("food_002", "easy", "Какую еду нашли в египетских гробницах — и она всё ещё съедобна?", "Which food found in Egyptian tombs is still edible?",
  [("Хлеб", "Bread"), ("Сыр", "Cheese"), ("Мёд", "Honey"), ("Виноград", "Grapes")], 2,
  "Мёду было около 3000 лет. В нём почти нет воды, и микробы в нём не живут.", "The honey was about 3,000 years old. It has almost no water, so germs cannot live in it.", "https://www.smithsonianmag.com/science-nature/the-science-behind-honeys-eternal-shelf-life-1218690/"),
 ("food_003", "easy", "Почему, когда режешь лук, текут слёзы?", "Why do you cry when you cut an onion?",
  [("Лук брызгает соком в глаза", "It squirts juice in your eyes"), ("Он выпускает едкий газ", "It releases an irritating gas"), ("Лук очень грустный", "The onion is sad"), ("Пахнет как перец", "It smells like pepper")], 1,
  "Помогает холодный лук и острый нож: газа выходит меньше.", "A cold onion and a sharp knife help: less gas escapes.", W + "Syn-Propanethial-S-oxide"),
 ("food_004", "easy", "Откуда родом картошка?", "Where do potatoes come from?",
  [("Из Ирландии", "Ireland"), ("Из России", "Russia"), ("Из Германии", "Germany"), ("Из Южной Америки", "South America")], 3,
  "Индейцы в Андах выращивали её больше 7000 лет назад.", "People in the Andes grew it more than 7,000 years ago.", W + "Potato"),
 ("food_005", "medium", "Какой из этих плодов для ботаника — настоящая ягода?", "Which of these is a true berry to a botanist?",
  [("Клубника", "Strawberry"), ("Малина", "Raspberry"), ("Банан", "Banana"), ("Вишня", "Cherry")], 2,
  "А клубника и малина — вовсе не ягоды. Помидор, кстати, тоже ягода.", "Strawberries and raspberries are not berries at all. A tomato is, though.", W + "Berry_(botany)"),
 ("food_006", "medium", "Какая пряность самая дорогая в мире?", "Which spice is the most expensive in the world?",
  [("Ваниль", "Vanilla"), ("Шафран", "Saffron"), ("Кардамон", "Cardamom"), ("Корица", "Cinnamon")], 1,
  "Шафран — это тычинки цветка крокуса, их собирают руками.", "Saffron is the threads of a crocus flower, picked by hand.", W + "Saffron"),
 ("food_007", "medium", "Какого цвета была морковь раньше, чем стала оранжевой?", "What colour were carrots before they turned orange?",
  [("Синей", "Blue"), ("Фиолетовой или жёлтой", "Purple or yellow"), ("Зелёной", "Green"), ("Всегда была оранжевой", "They were always orange")], 1,
  "Оранжевую морковь вывели в Европе всего несколько веков назад.", "Orange carrots were bred in Europe only a few centuries ago.", W + "Carrot"),
 ("food_008", "medium", "Какой «орех» на самом деле родственник гороха и фасоли?", "Which nut is really a relative of peas and beans?",
  [("Грецкий", "Walnut"), ("Фундук", "Hazelnut"), ("Миндаль", "Almond"), ("Арахис", "Peanut")], 3,
  "Арахис зреет под землёй, в стручках, как горох.", "Peanuts ripen underground, in pods, like peas.", W + "Peanut"),
 ("food_009", "medium", "Из чего делают марципан?", "What is marzipan made from?",
  [("Из миндаля", "Almonds"), ("Из арахиса", "Peanuts"), ("Из кокоса", "Coconut"), ("Из фасоли", "Beans")], 0,
  "Перемолотый миндаль с сахаром: из него лепят фрукты и зверей.", "Ground almonds and sugar: people shape it into fruit and animals.", W + "Marzipan"),
]
CV["food"] = [
 ("food_010", "Сколько процентов арбуза — вода?", "What percentage of a watermelon is water?",
  92, (0, 100), 1, 7, ("%", "%"),
  "Поэтому в жару арбуз и пьют, и едят одновременно.", "That is why on a hot day you eat and drink a watermelon at once.", W + "Watermelon"),
 ("food_011", "При какой температуре начинает таять шоколад?", "At what temperature does chocolate start to melt?",
  34, (0, 100), 1, 25, ("°C", "°C"),
  "Чуть ниже температуры тела: поэтому он тает во рту, а не в руке.", "Just below body temperature: that is why it melts in your mouth.", W + "Cocoa_butter"),
 ("food_012", "Сколько цветков нужно собрать, чтобы получить грамм шафрана?", "How many flowers does it take to make one gram of saffron?",
  150, (1, 1000), 1, 140, ("цветков", "flowers"),
  "У каждого цветка всего три тоненькие тычинки.", "Each flower has only three thin threads.", W + "Saffron"),
]

MC["sport"] = [
 ("spo_001", "easy", "Что бросают в кёрлинге?", "What do you throw in curling?",
  [("Шайбу", "A puck"), ("Камень", "A stone"), ("Мяч", "A ball"), ("Метлу", "A broom")], 1,
  "Камень весит почти 20 килограммов, а метлой натирают лёд перед ним.", "The stone weighs almost 20 kilograms; the broom just polishes the ice in front of it.", W + "Curling"),
 ("spo_002", "easy", "Сколько клеток на шахматной доске?", "How many squares are on a chessboard?",
  [("36", "36"), ("49", "49"), ("64", "64"), ("100", "100")], 2,
  "Восемь на восемь: половина белых, половина чёрных.", "Eight by eight: half light, half dark.", W + "Chessboard"),
 ("spo_003", "easy", "Кто был талисманом Олимпиады-80 в Москве?", "Who was the mascot of the 1980 Moscow Olympics?",
  [("Медвежонок", "A bear cub"), ("Волк", "A wolf"), ("Заяц", "A hare"), ("Чебурашка", "Cheburashka")], 0,
  "На закрытии Мишка улетел на воздушных шарах, и стадион плакал.", "At the closing ceremony the bear floated away on balloons and the stadium cried.", W + "Misha_(mascot)"),
 ("spo_004", "medium", "Какую игру придумали, повесив на стены две корзины для персиков?", "Which game was invented by nailing two peach baskets to the walls?",
  [("Волейбол", "Volleyball"), ("Баскетбол", "Basketball"), ("Гандбол", "Handball"), ("Бейсбол", "Baseball")], 1,
  "У корзин было дно, и мяч после каждого броска доставали лестницей.", "The baskets still had bottoms, so after every score someone fetched the ball with a ladder.", W + "Basketball"),
 ("spo_005", "medium", "Почему марафон — это именно 42 км 195 м?", "Why is a marathon exactly 42.195 km?",
  [("Столько пробежал древний гонец", "An ancient messenger ran that far"), ("Так захотела королевская семья в 1908", "The royal family wanted it in 1908"), ("Это ровно 100 000 шагов", "It is exactly 100,000 steps"), ("Так длинна Великая стена", "It is the length of the Great Wall")], 1,
  "Старт поставили у Виндзорского замка, а финиш — напротив королевской ложи.", "The start was put by Windsor Castle and the finish in front of the royal box.", W + "Marathon"),
 ("spo_006", "medium", "Какой из этих видов спорта когда-то был олимпийским?", "Which of these was once an Olympic sport?",
  [("Перетягивание каната", "Tug of war"), ("Прятки", "Hide and seek"), ("Бег в мешках", "Sack race"), ("Пускание блинчиков", "Stone skipping")], 0,
  "Канат перетягивали на пяти Олимпиадах подряд, с 1900 по 1920 год.", "Tug of war was in five Olympics in a row, from 1900 to 1920.", W + "Tug_of_war_at_the_Summer_Olympics"),
 ("spo_007", "medium", "В каком спорте мяч самый маленький?", "Which sport has the smallest ball?",
  [("Гольф", "Golf"), ("Большой теннис", "Tennis"), ("Настольный теннис", "Table tennis"), ("Водное поло", "Water polo")], 2,
  "Шарик для пинг-понга 40 мм, мяч для гольфа почти 43.", "A ping pong ball is 40 mm across; a golf ball almost 43.", W + "Table_tennis"),
 ("spo_008", "medium", "Майку какого цвета надевает лидер велогонки «Тур де Франс»?", "What colour jersey does the leader of the Tour de France wear?",
  [("Зелёную", "Green"), ("Красную", "Red"), ("Белую", "White"), ("Жёлтую", "Yellow")], 3,
  "Жёлтую — потому что газета, которая придумала гонку, печаталась на жёлтой бумаге.", "Yellow, because the newspaper that started the race was printed on yellow paper.", W + "Yellow_jersey_statistics"),
]
CV["sport"] = [
 ("spo_009", "Сколько метров в марафоне?", "How many metres is a marathon?",
  42195, (30000, 60000), 5, 6000, ("м", "m"),
  "Самые быстрые бегуны пробегают его чуть больше чем за два часа.", "The fastest runners do it in just over two hours.", W + "Marathon"),
 ("spo_010", "Сколько часов длился самый долгий теннисный матч в истории?", "How many hours did the longest tennis match in history last?",
  11, (1, 30), 1, 8, ("часов", "hours"),
  "Уимблдон, 2010 год: матч шёл три дня, а табло сломалось на счёте 47:47.", "Wimbledon 2010: it ran over three days, and the scoreboard broke at 47 all.", W + "Isner%E2%80%93Mahut_match_at_the_2010_Wimbledon_Championships"),
 ("spo_011", "Какую скорость развивал Усэйн Болт на пике забега, в км/ч?", "How fast did Usain Bolt run at his peak, in km/h?",
  45, (10, 100), 1, 25, ("км/ч", "km/h"),
  "Быстрее, чем машина едет по двору, но медленнее домашней кошки на рывке.", "Faster than a car in a car park, slower than a house cat in a sprint.", W + "Usain_Bolt"),
]

MC["culture"] = [
 ("cul_001", "easy", "Где нашли Чебурашку?", "Where was Cheburashka found?",
  [("В коробке из-под обуви", "In a shoebox"), ("В ящике с апельсинами", "In a crate of oranges"), ("В дупле дерева", "In a hollow tree"), ("В посылке из Африки", "In a parcel from Africa")], 1,
  "Он наелся апельсинов в тропическом лесу и уснул прямо в ящике.", "He ate his fill of oranges in the rainforest and fell asleep in the crate.", W + "Cheburashka"),
 ("cul_002", "easy", "Как зовут пса из Простоквашино?", "What is the dog's name in Prostokvashino?",
  [("Бобик", "Bobik"), ("Тузик", "Tuzik"), ("Шарик", "Sharik"), ("Полкан", "Polkan")], 2,
  "В конце он научился фотографировать и чуть не стал знаменитым.", "In the end he learned photography and nearly became famous.", W + "Three_from_Prostokvashino"),
 ("cul_003", "easy", "Что кричит Волк в «Ну, погоди!», когда всё снова пошло не так?", "What does the Wolf shout in Nu, Pogodi! when it all goes wrong again?",
  [("«Я ещё вернусь!»", "I'll be back!"), ("«Хорошо сидим!»", "Nice sitting here!"), ("«Ну, Заяц, погоди!»", "Just you wait, Hare!"), ("«Улыбаемся и машем»", "Smile and wave")], 2,
  "Мультфильм снимали почти 20 лет, а Волка озвучивал Анатолий Папанов.", "It was made over almost 20 years, and Anatoly Papanov voiced the Wolf.", W + "Nu,_pogodi!"),
 ("cul_004", "easy", "С чем Винни-Пух полетел к пчёлам за мёдом?", "What did Winnie the Pooh fly up to the bees with?",
  [("С зонтиком", "An umbrella"), ("С воздушным шариком", "A balloon"), ("С лестницей", "A ladder"), ("С кастрюлей", "A saucepan")], 1,
  "Он притворялся тучкой, а Пятачок ходил под ним с зонтиком.", "He pretended to be a cloud while Piglet walked beneath with an umbrella.", W + "Winnie-the-Pooh_(1969_film)"),
 ("cul_005", "medium", "Какой мультфильм первым получил «Оскар» как лучший анимационный?", "Which film won the first Oscar for best animated feature?",
  [("«Король Лев»", "The Lion King"), ("«Шрек»", "Shrek"), ("«Ледниковый период»", "Ice Age"), ("«Головоломка»", "Inside Out")], 1,
  "Эту премию вообще придумали только в 2001 году.", "That award was only created in 2001.", W + "Academy_Award_for_Best_Animated_Feature"),
 ("cul_006", "medium", "Какая сказка Пушкина начинается словами «Жил старик со своею старухой…»?", "Which Pushkin tale starts: \"There lived an old man and his old wife...\"?",
  [("О царе Салтане", "Tsar Saltan"), ("О золотом петушке", "The Golden Cockerel"), ("О рыбаке и рыбке", "The Fisherman and the Fish"), ("О попе и Балде", "The Priest and Balda")], 2,
  "Старуха начала с нового корыта, а кончила тем же корытом.", "The old wife began with a new washtub and ended with the same old one.", W + "The_Tale_of_the_Fisherman_and_the_Fish"),
 ("cul_007", "medium", "Кем по профессии был автор «Алисы в Стране чудес»?", "What was the author of Alice in Wonderland by profession?",
  [("Математиком", "A mathematician"), ("Поваром", "A cook"), ("Капитаном корабля", "A ship's captain"), ("Шляпником", "A hatter")], 0,
  "Льюис Кэрролл преподавал математику в Оксфорде и придумывал головоломки.", "Lewis Carroll taught maths at Oxford and made up puzzles.", W + "Lewis_Carroll"),
 ("cul_008", "medium", "Какой художник отрезал себе часть уха?", "Which painter cut off part of his own ear?",
  [("Рембрандт", "Rembrandt"), ("Ван Гог", "Van Gogh"), ("Пикассо", "Picasso"), ("Айвазовский", "Aivazovsky")], 1,
  "Он потом написал автопортрет с перевязанным ухом.", "He later painted a self portrait with his ear bandaged.", W + "Vincent_van_Gogh"),
]
CV["culture"] = [
 ("cul_009", "В каком году вышла первая серия «Ну, погоди!»?", "In which year did the first Nu, Pogodi! episode come out?",
  1969, (1940, 2010), 1, 25, ("год", "year"),
  "Первую серию сделали за несколько месяцев, а последняя классическая вышла в 1986-м.", "The first episode took a few months; the last classic one came out in 1986.", W + "Nu,_pogodi!"),
 ("cul_010", "Сколько клавиш у обычного пианино?", "How many keys does an ordinary piano have?",
  88, (40, 150), 1, 30, ("клавиш", "keys"),
  "52 белых и 36 чёрных.", "52 white and 36 black.", W + "Piano"),
 ("cul_011", "В каком году вышла первая книга о Гарри Поттере?", "In which year was the first Harry Potter book published?",
  1997, (1980, 2015), 1, 15, ("год", "year"),
  "Рукопись отвергли двенадцать издательств подряд.", "Twelve publishers turned the manuscript down in a row.", W + "Harry_Potter_and_the_Philosopher%27s_Stone"),
]

MC["space"] = [
 ("spa_001", "easy", "Почему Марс красный?", "Why is Mars red?",
  [("Он раскалённый", "It is red hot"), ("Его покрывает ржавая пыль", "It is covered in rusty dust"), ("Его так подсвечивает Солнце", "The Sun lights it that way"), ("Это закат на Марсе", "It is a Martian sunset")], 1,
  "В марсианском песке много оксида железа — то есть ржавчины.", "Martian sand is full of iron oxide, which is rust.", W + "Mars"),
 ("spa_002", "easy", "Почему на космической станции едят лепёшки вместо хлеба?", "Why do astronauts eat tortillas instead of bread?",
  [("Хлеб там не пропекается", "Bread does not bake up there"), ("Крошки разлетаются по станции", "Crumbs float all over the station"), ("Лепёшки вкуснее", "Tortillas taste better"), ("Хлеб запрещён законом", "Bread is against the law")], 1,
  "Крошки могут попасть в глаза или в приборы.", "Crumbs can get into eyes or into the equipment.", W + "Space_food"),
 ("spa_003", "easy", "Что слышно в открытом космосе?", "What can you hear in open space?",
  [("Гул звёзд", "The hum of the stars"), ("Свист ветра", "Whistling wind"), ("Ничего: там нет воздуха", "Nothing: there is no air"), ("Музыку сфер", "Music of the spheres")], 2,
  "Звуку нужен воздух или вода. Взрывы в кино о космосе — выдумка.", "Sound needs air or water. Explosions in space films are made up.", W + "Outer_space"),
 ("spa_004", "easy", "Что происходит с ростом космонавта в невесомости?", "What happens to an astronaut's height in space?",
  [("Становится выше на несколько сантиметров", "Grows a few centimetres taller"), ("Становится ниже", "Gets shorter"), ("Не меняется", "Stays the same"), ("Вырастает на метр", "Grows a metre")], 0,
  "Позвоночник растягивается, а на Земле всё возвращается за пару недель.", "The spine stretches out, and back on Earth it shrinks back within weeks.", W + "Effect_of_spaceflight_on_the_human_body"),
 ("spa_005", "medium", "Какая планета крутится, лёжа на боку?", "Which planet spins lying on its side?",
  [("Уран", "Uranus"), ("Земля", "Earth"), ("Марс", "Mars"), ("Юпитер", "Jupiter")], 0,
  "Наверное, в древности в Уран врезалось что-то огромное.", "Probably something huge crashed into Uranus long ago.", W + "Uranus"),
 ("spa_006", "medium", "На какой планете сутки длиннее, чем год?", "On which planet is a day longer than a year?",
  [("Юпитер", "Jupiter"), ("Венера", "Venus"), ("Марс", "Mars"), ("Нептун", "Neptune")], 1,
  "Венера оборачивается вокруг себя за 243 дня, а вокруг Солнца — за 225.", "Venus turns once in 243 days but goes round the Sun in 225.", W + "Venus"),
 ("spa_007", "medium", "Что почти невозможно разглядеть с орбиты без приборов?", "What can you hardly see from orbit without instruments?",
  [("Огни больших городов", "City lights at night"), ("Пустыню Сахару", "The Sahara"), ("Гималаи", "The Himalayas"), ("Великую Китайскую стену", "The Great Wall of China")], 3,
  "Стена очень длинная, но узкая и того же цвета, что земля вокруг.", "The wall is long but narrow, and the same colour as the land around it.", W + "Great_Wall_of_China"),
 ("spa_008", "medium", "Сколько Земель поместилось бы внутри Солнца?", "How many Earths would fit inside the Sun?",
  [("Около ста", "About a hundred"), ("Около тысячи", "About a thousand"), ("Около миллиона", "About a million"), ("Ни одной, оно жидкое", "None, it is liquid")], 2,
  "Точнее, около 1,3 миллиона.", "About 1.3 million, to be exact.", W + "Sun"),
]
CV["space"] = [
 ("spa_009", "За сколько минут свет Солнца долетает до Земли?", "How many minutes does sunlight take to reach the Earth?",
  8, (0, 60), 1, 7, ("минут", "minutes"),
  "Мы всегда видим Солнце таким, каким оно было восемь минут назад.", "We always see the Sun as it was eight minutes ago.", W + "Sunlight"),
 ("spa_010", "Сколько земных дней длится год на Марсе?", "How many Earth days does a year on Mars last?",
  687, (100, 2000), 1, 500, ("дней", "days"),
  "Десятилетний на Марсе отмечал бы всего пятый день рождения.", "A ten year old would only have had five birthdays on Mars.", W + "Mars"),
 ("spa_011", "В каком году люди впервые высадились на Луну?", "In which year did people first land on the Moon?",
  1969, (1940, 2000), 1, 20, ("год", "year"),
  "Их следы на Луне лежат до сих пор: там нет ветра.", "Their footprints are still there: the Moon has no wind.", W + "Apollo_11"),
]

RACE = [
 ("Банан — это ягода, а клубника — нет.", "A banana is a berry, but a strawberry is not.", True, W + "Berry_(botany)"),
 ("У осьминога голубая кровь.", "An octopus has blue blood.", True, W + "Octopus"),
 ("Наполеон был очень низким даже для своего времени.", "Napoleon was very short even for his time.", False, W + "Napoleon"),
 ("Золотая рыбка помнит только три секунды.", "A goldfish only remembers for three seconds.", False, W + "Goldfish"),
 ("Сутки на Венере длиннее, чем год на Венере.", "A day on Venus is longer than a year on Venus.", True, W + "Venus"),
 ("Великую Китайскую стену видно с Луны.", "You can see the Great Wall of China from the Moon.", False, W + "Great_Wall_of_China"),
 ("Мёд может не портиться тысячи лет.", "Honey can keep for thousands of years.", True, W + "Honey"),
 ("Молния никогда не бьёт в одно место дважды.", "Lightning never strikes the same place twice.", False, W + "Lightning"),
 ("Помидор для ботаника — ягода.", "To a botanist, a tomato is a berry.", True, W + "Tomato"),
 ("Страус от страха прячет голову в песок.", "An ostrich hides its head in the sand when scared.", False, W + "Common_ostrich"),
 ("Акулы появились на Земле раньше деревьев.", "Sharks appeared on Earth before trees did.", True, W + "Shark"),
 ("У улитки есть зубы.", "A snail has teeth.", True, W + "Radula"),
 ("Летучие мыши слепые.", "Bats are blind.", False, W + "Bat"),
 ("Бамбук — это трава.", "Bamboo is a grass.", True, W + "Bamboo"),
 ("Пингвины живут на Северном полюсе.", "Penguins live at the North Pole.", False, W + "Penguin"),
 ("Эйфелеву башню собирались разобрать через 20 лет.", "The Eiffel Tower was meant to be taken down after 20 years.", True, W + "Eiffel_Tower"),
 ("Антарктида — самая большая пустыня на Земле.", "Antarctica is the biggest desert on Earth.", True, W + "Antarctica"),
 ("Кошки видят в полной темноте.", "Cats can see in total darkness.", False, W + "Cat"),
 ("Земля ближе всего к Солнцу в январе.", "The Earth is closest to the Sun in January.", True, W + "Perihelion_and_aphelion"),
 ("Человек использует только 10 процентов мозга.", "People only use 10 percent of their brain.", False, W + "Ten_percent_of_the_brain_myth"),
 ("Гагарин облетел Землю за 108 минут.", "Gagarin flew round the Earth in 108 minutes.", True, W + "Vostok_1"),
 ("На Марсе есть вулкан выше Эвереста.", "Mars has a volcano taller than Everest.", True, W + "Olympus_Mons"),
 ("Морская звезда — это рыба.", "A starfish is a fish.", False, W + "Starfish"),
 ("Шоколад опасен для собак.", "Chocolate is dangerous for dogs.", True, W + "Theobromine_poisoning"),
 ("Хамелеон меняет цвет, только чтобы спрятаться.", "A chameleon changes colour only to hide.", False, W + "Chameleon"),
 ("В воде звук летит быстрее, чем в воздухе.", "Sound travels faster in water than in air.", True, W + "Speed_of_sound"),
 ("Взрослый слон не умеет прыгать.", "A grown elephant cannot jump.", True, W + "Elephant"),
 ("Свет от Солнца летит до Земли около восьми минут.", "Sunlight takes about eight minutes to reach the Earth.", True, W + "Sunlight"),
 ("Красной планетой называют Венеру.", "Venus is called the Red Planet.", False, W + "Mars"),
 ("Колибри умеет летать задом наперёд.", "A hummingbird can fly backwards.", True, W + "Hummingbird"),
 ("Паук — это насекомое.", "A spider is an insect.", False, W + "Spider"),
 ("Луна каждый год немного отдаляется от Земли.", "The Moon moves a little further from the Earth every year.", True, W + "Orbit_of_the_Moon"),
 ("Факс изобрели раньше телефона.", "The fax was invented before the telephone.", True, W + "Fax"),
 ("Викинги носили шлемы с рогами.", "Vikings wore horned helmets.", False, W + "Horned_helmet"),
 ("Под белой шерстью у белого медведя чёрная кожа.", "Under its white fur a polar bear has black skin.", True, W + "Polar_bear"),
 ("Арахис растёт на деревьях.", "Peanuts grow on trees.", False, W + "Peanut"),
 ("Самая короткая война в истории длилась меньше часа.", "The shortest war in history lasted less than an hour.", True, W + "Anglo-Zanzibar_War"),
 ("Морковь когда-то была фиолетовой.", "Carrots used to be purple.", True, W + "Carrot"),
 ("Микроволновку придумали, когда у инженера в кармане растаяла шоколадка.", "The microwave oven was invented when a chocolate bar melted in an engineer's pocket.", True, W + "Microwave_oven"),
 ("У новорождённого костей больше, чем у взрослого.", "A newborn baby has more bones than an adult.", True, W + "Human_skeleton"),
]


# ------------------------------------------------------------------------------------------------ v3: the topic bank
# Every round the topic vote offers three topics drawn at random from this bank plus "A bit of everything"
# (TopicPicker). Each topic holds enough for at least one round: easy and medium multiple choice, hard closest value.

TOPICS = [
 ("world", "wld", "Вокруг света", "Around the world"),
 ("earth", "ear", "Земля и погода", "Earth and weather"),
 ("animals", "ani", "Странные звери", "Weird animals"),
 ("sea", "sea", "Под водой", "Under the sea"),
 ("kitchen", "kit", "Что на ужин", "What's for dinner"),
 ("timemachine", "tim", "Машина времени", "Time machine"),
 ("myths", "myt", "Мифы и враньё", "Myths and nonsense"),
 ("body", "bod", "Как устроен человек", "The human body"),
 ("records", "rec", "Спорт и рекорды", "Sport and records"),
 ("cartoons", "car", "Мультики нашего детства", "Cartoons we grew up with"),
 ("books", "boo", "Книги и сказки", "Books and fairy tales"),
 ("art", "art", "Музыка и картины", "Music and paintings"),
 ("inventions", "inv", "Кто это придумал?", "Who invented that?"),
 ("science", "sci", "Как это работает", "How things work"),
 ("space", "spa", "Космос", "Up in space"),
]
MIXED = ("mixed", "Всего понемногу", "A bit of everything")

# Where each v2 question goes (old id -> new topic).
REMAP = {
 "geo_001": "world", "geo_002": "world", "geo_003": "world", "geo_004": "world", "geo_005": "world", "geo_007": "world",
 "geo_008": "world", "geo_009": "world", "geo_010": "world",
 "geo_006": "earth", "geo_011": "earth", "sci_002": "earth", "sci_006": "earth",
 "nat_001": "animals", "nat_002": "animals", "nat_005": "animals", "nat_006": "animals", "nat_007": "animals",
 "nat_008": "animals", "nat_009": "animals", "nat_010": "animals",
 "nat_003": "sea", "nat_004": "sea", "nat_011": "sea",
 "food_001": "kitchen", "food_002": "kitchen", "food_003": "kitchen", "food_004": "kitchen", "food_005": "kitchen",
 "food_006": "kitchen", "food_007": "kitchen", "food_008": "kitchen", "food_009": "kitchen", "food_010": "kitchen",
 "food_011": "kitchen", "food_012": "kitchen",
 "his_001": "timemachine", "his_003": "timemachine", "his_004": "timemachine", "his_005": "timemachine",
 "his_006": "timemachine", "his_009": "timemachine", "his_010": "timemachine", "his_011": "timemachine",
 "sci_001": "myths", "spa_007": "myths",
 "sci_005": "body", "sci_010": "body", "sci_011": "body",
 "spo_001": "records", "spo_002": "records", "spo_003": "records", "spo_004": "records", "spo_005": "records",
 "spo_006": "records", "spo_007": "records", "spo_008": "records", "spo_009": "records", "spo_010": "records",
 "spo_011": "records",
 "cul_001": "cartoons", "cul_002": "cartoons", "cul_003": "cartoons", "cul_004": "cartoons", "cul_005": "cartoons",
 "cul_009": "cartoons",
 "cul_006": "books", "cul_007": "books", "cul_011": "books",
 "cul_008": "art", "cul_010": "art",
 "his_008": "inventions", "sci_003": "inventions", "sci_008": "inventions",
 "sci_004": "science", "sci_007": "science", "sci_009": "science",
 "spa_001": "space", "spa_002": "space", "spa_003": "space", "spa_004": "space", "spa_005": "space", "spa_006": "space",
 "spa_008": "space", "spa_009": "space", "spa_010": "space", "spa_011": "space", "his_002": "space",
}
DROPPED = {"his_007"}   # Napoleon moves to myths as a closest value question

# New questions: (topic, difficulty, ru, en, options, correct, fact_ru, fact_en, source) and closest value
# (topic, ru, en, answer, range, step, tolerance, unit, fact_ru, fact_en, source).
NEW_MC = [
 ("earth", "easy", "Почему мы сначала видим молнию, а гром слышим потом?", "Why do we see lightning before we hear the thunder?",
  [("Свет летит быстрее звука", "Light is faster than sound"), ("Гром начинается позже", "Thunder starts later"), ("Гром стесняется", "Thunder is shy"), ("Молния ближе к нам", "The lightning is closer")], 0,
  "Посчитай секунды до грома и раздели на три: столько километров до грозы.", "Count the seconds to the thunder and divide by three: that is how many kilometres away the storm is.", W + "Thunder"),
 ("earth", "medium", "Где на Земле измерили самый сильный мороз?", "Where on Earth was the coldest temperature ever measured?",
  [("В Оймяконе", "Oymyakon"), ("На Северном полюсе", "The North Pole"), ("В Антарктиде", "Antarctica"), ("На Аляске", "Alaska")], 2,
  "Оймякон — самое холодное место, где живут люди, а рекорд держит станция Восток.", "Oymyakon is the coldest place where people live; the record belongs to Vostok Station.", W + "Vostok_Station"),
 ("sea", "easy", "Кто из этих морских жителей — не рыба?", "Which of these sea creatures is not a fish?",
  [("Акула", "Shark"), ("Дельфин", "Dolphin"), ("Морской конёк", "Seahorse"), ("Скат", "Ray")], 1,
  "Дельфины дышат воздухом и кормят детёнышей молоком, как кошки.", "Dolphins breathe air and feed their babies milk, like cats do.", W + "Dolphin"),
 ("sea", "medium", "Кто у морских коньков вынашивает малышей?", "Who carries the babies in a seahorse family?",
  [("Мама", "Mum"), ("Папа", "Dad"), ("Бабушка", "Grandma"), ("Их приносит прилив", "The tide brings them")], 1,
  "Папа носит икринки в сумке на животе и потом «рожает» сотни мальков.", "Dad carries the eggs in a pouch on his belly and then gives birth to hundreds of babies.", W + "Seahorse"),
 ("sea", "medium", "Какое животное — самое большое из всех, кто когда-либо жил на Земле?", "Which is the biggest animal that ever lived on Earth?",
  [("Самый большой динозавр", "The biggest dinosaur"), ("Синий кит", "The blue whale"), ("Мегалодон", "Megalodon"), ("Мамонт", "The mammoth")], 1,
  "Синий кит длиннее двух автобусов и тяжелее любого динозавра.", "A blue whale is longer than two buses and heavier than any dinosaur.", W + "Blue_whale"),
 ("myths", "easy", "Что будет, если случайно проглотить жвачку?", "What happens if you accidentally swallow chewing gum?",
  [("Выйдет через пару дней, как всё", "It comes out in a day or two"), ("Пролежит в животе семь лет", "It stays in your tummy for seven years"), ("Склеит живот", "It glues your tummy shut"), ("Превратится в пузырь", "It turns into a bubble")], 0,
  "Про семь лет — выдумка. Но глотать её всё равно не стоит.", "The seven years is made up. Still, better not to swallow it.", W + "Chewing_gum"),
 ("myths", "medium", "Почему бык бросается на тряпку тореадора?", "Why does a bull charge at the bullfighter's cape?",
  [("Ненавидит красный цвет", "It hates red"), ("Злится, что тряпкой машут", "It is angry at the waving"), ("Думает, что это еда", "It thinks it is food"), ("Тореадор обзывается", "The bullfighter calls it names")], 1,
  "Быки почти не отличают красный от зелёного: их злит движение.", "Bulls can hardly tell red from green: it is the movement that angers them.", W + "Bullfighting"),
 ("myths", "medium", "Какого цвета были древнегреческие мраморные статуи, когда их сделали?", "What colour were ancient Greek marble statues when they were made?",
  [("Белые, как сейчас", "White, as they are now"), ("Ярко раскрашены", "Brightly painted"), ("Целиком позолочены", "Covered in gold"), ("Прозрачные", "See-through")], 1,
  "Краска за века стёрлась, и все решили, что греки любили белый мрамор.", "The paint wore off over the centuries, so everyone decided the Greeks loved white marble.", W + "Polychrome"),
 ("myths", "medium", "Сколько чувств у человека на самом деле?", "How many senses do people really have?",
  [("Ровно пять", "Exactly five"), ("Больше пяти", "More than five"), ("Три", "Three"), ("Шесть, вместе с интуицией", "Six, counting intuition")], 1,
  "Кроме зрения, слуха, вкуса, запаха и осязания есть чувство равновесия, тепла, боли.", "Besides sight, hearing, taste, smell and touch there is balance, temperature and pain.", W + "Sense"),
 ("body", "easy", "Сколько зубов у взрослого человека, если выросли все?", "How many teeth does an adult have if they have all grown in?",
  [("20", "20"), ("28", "28"), ("32", "32"), ("40", "40")], 2,
  "Последние четыре — зубы мудрости, они могут вырасти и в 25 лет.", "The last four are wisdom teeth; they may only come at 25.", W + "Human_tooth"),
 ("body", "easy", "Чем мы на самом деле чувствуем вкус еды больше всего?", "What do we really taste food with the most?",
  [("Только языком", "Only the tongue"), ("Носом", "The nose"), ("Зубами", "The teeth"), ("Ушами", "The ears")], 1,
  "Зажми нос — и яблоко с луком станут почти одинаковыми на вкус.", "Pinch your nose and an apple tastes almost like an onion.", W + "Flavor"),
 ("body", "medium", "Где у человека самая маленькая кость?", "Where is the smallest bone in the human body?",
  [("В ухе", "In the ear"), ("В мизинце ноги", "In the little toe"), ("В носу", "In the nose"), ("В пальце руки", "In a finger")], 0,
  "Стремечко в ухе меньше рисового зёрнышка.", "The stirrup bone in the ear is smaller than a grain of rice.", W + "Stapes"),
 ("body", "medium", "Сколько примерно литров крови у взрослого человека?", "About how many litres of blood does an adult have?",
  [("Один", "One"), ("Пять", "Five"), ("Пятнадцать", "Fifteen"), ("Пятьдесят", "Fifty")], 1,
  "Это полведра, и сердце прогоняет её всю примерно за минуту.", "That is half a bucket, and the heart pumps all of it round in about a minute.", W + "Blood_volume"),
 ("cartoons", "easy", "Что всегда говорил кот Леопольд хулиганистым мышам?", "What did Leopold the cat always say to the naughty mice?",
  [("«Ребята, давайте жить дружно!»", "Guys, let's live in peace!"), ("«Ну, погоди!»", "Just you wait!"), ("«Я вас всех съем!»", "I'll eat you all!"), ("«Кто там?»", "Who's there?")], 0,
  "Мышей звали Белый и Серый, и они всё равно продолжали пакостить.", "The mice were called White and Grey, and they kept up their pranks anyway.", W + "Adventures_of_Leopold_the_Cat"),
 ("cartoons", "medium", "Как звали попугая из «Возвращения блудного попугая»?", "What is the parrot called in The Return of the Prodigal Parrot?",
  [("Гоша", "Gosha"), ("Кеша", "Kesha"), ("Кузя", "Kuzya"), ("Яша", "Yasha")], 1,
  "«Таити, Таити… Нас и здесь неплохо кормят!»", "\"Tahiti, Tahiti... They feed us fine here too!\"", W + "The_Return_of_the_Prodigal_Parrot"),
 ("books", "easy", "Сколько гномов дружили с Белоснежкой?", "How many dwarfs were friends with Snow White?",
  [("Пять", "Five"), ("Шесть", "Six"), ("Семь", "Seven"), ("Двенадцать", "Twelve")], 2,
  "У Пушкина в похожей сказке вместо гномов семь богатырей.", "In Pushkin's similar tale the seven dwarfs are seven knights.", W + "Snow_White"),
 ("books", "easy", "Что потеряла Золушка на балу?", "What did Cinderella lose at the ball?",
  [("Корону", "Her crown"), ("Перчатку", "A glove"), ("Туфельку", "A slipper"), ("Тыкву", "The pumpkin")], 2,
  "У Перро туфелька хрустальная, а у братьев Гримм — золотая.", "In Perrault the slipper is glass; in the Brothers Grimm it is gold.", W + "Cinderella"),
 ("books", "medium", "Как зовут итальянского «брата» Буратино?", "What is Buratino's Italian original called?",
  [("Чиполлино", "Cipollino"), ("Пиноккио", "Pinocchio"), ("Джеппетто", "Geppetto"), ("Карабас", "Karabas")], 1,
  "Алексей Толстой пересказал «Пиноккио» по-своему и назвал героя Буратино.", "Alexei Tolstoy retold Pinocchio his own way and called the hero Buratino.", W + "The_Golden_Key,_or_the_Adventures_of_Buratino"),
 ("art", "easy", "Сколько струн у обычной гитары?", "How many strings does an ordinary guitar have?",
  [("Четыре", "Four"), ("Пять", "Five"), ("Шесть", "Six"), ("Семь", "Seven")], 2,
  "А у старой русской гитары было семь струн.", "The old Russian guitar had seven.", W + "Guitar"),
 ("art", "easy", "Кто нарисовал «Мону Лизу»?", "Who painted the Mona Lisa?",
  [("Микеланджело", "Michelangelo"), ("Леонардо да Винчи", "Leonardo da Vinci"), ("Ван Гог", "Van Gogh"), ("Рафаэль", "Raphael")], 1,
  "В 1911 году картину украли из Лувра и искали больше двух лет.", "In 1911 it was stolen from the Louvre and missing for over two years.", W + "Mona_Lisa"),
 ("art", "medium", "Кто написал музыку к балету «Лебединое озеро»?", "Who wrote the music for Swan Lake?",
  [("Чайковский", "Tchaikovsky"), ("Моцарт", "Mozart"), ("Рахманинов", "Rachmaninoff"), ("Бетховен", "Beethoven")], 0,
  "На первой постановке балет почти провалился, а теперь его знает весь мир.", "The first production nearly flopped; now the whole world knows it.", W + "Swan_Lake"),
 ("art", "medium", "Сколько нот в гамме «до, ре, ми, фа, соль, ля, си»?", "How many notes are in the scale do, re, mi, fa, sol, la, ti?",
  [("Пять", "Five"), ("Семь", "Seven"), ("Восемь", "Eight"), ("Двенадцать", "Twelve")], 1,
  "Восьмая «до» — это уже начало следующей гаммы.", "The eighth \"do\" already starts the next scale.", W + "Solf%C3%A8ge"),
 ("inventions", "easy", "Что изобрели, когда у инженера в кармане растаяла шоколадка?", "What was invented when a chocolate bar melted in an engineer's pocket?",
  [("Холодильник", "The fridge"), ("Микроволновку", "The microwave oven"), ("Фен", "The hair dryer"), ("Пылесос", "The vacuum cleaner")], 1,
  "Он стоял у радара. Первая микроволновка была ростом с человека.", "He was standing by a radar set. The first microwave oven was as tall as a person.", W + "Microwave_oven"),
 ("inventions", "medium", "Для чего сначала придумали пупырчатую плёнку?", "What was bubble wrap first invented as?",
  [("Как обои", "Wallpaper"), ("Для упаковки", "Packaging"), ("Как игрушку", "A toy"), ("Как скатерть", "A tablecloth")], 0,
  "Обои не продавались, а упаковка — ещё как.", "Nobody bought the wallpaper; the packaging sold like mad.", W + "Bubble_wrap"),
 ("science", "easy", "Где быстрее растает кубик льда: на металлической ложке или на деревянной доске?", "Where does an ice cube melt faster: on a metal spoon or on a wooden board?",
  [("На металле", "On the metal"), ("На дереве", "On the wood"), ("Одинаково", "Just the same"), ("Лёд в комнате не тает", "Ice does not melt indoors")], 0,
  "Металл кажется холоднее, но именно он быстрее отдаёт тепло льду.", "Metal feels colder, yet it is the one that passes heat to the ice faster.", W + "Thermal_conductivity"),
 ("science", "easy", "Почему лёд не тонет в воде?", "Why does ice float on water?",
  [("Он легче воды того же объёма", "It is lighter than as much water"), ("Его держит холод", "The cold holds it up"), ("В нём спрятан воздух", "It has air hidden in it"), ("Он тонет, просто медленно", "It sinks, just slowly")], 0,
  "Замерзая, вода расширяется. Поэтому рыбы зимуют подо льдом, а не во льду.", "Water expands as it freezes. That is why fish can live under the ice in winter.", W + "Ice"),
 ("science", "medium", "Что будет с надутым шариком в морозилке?", "What happens to a blown-up balloon in the freezer?",
  [("Лопнет", "It pops"), ("Сожмётся", "It shrinks"), ("Надуется сильнее", "It grows bigger"), ("Ничего", "Nothing")], 1,
  "Холодный воздух занимает меньше места. В тепле шарик снова расправится.", "Cold air takes up less room. Back in the warm, the balloon fills out again.", W + "Charles%27s_law"),
]
NEW_CV = [
 ("earth", "Сколько градусов мороза измерили на станции Восток в Антарктиде в 1983 году?", "How many degrees below zero were measured at Vostok Station in Antarctica in 1983?",
  89, (0, 150), 1, 40, ("° мороза", "degrees below zero"),
  "При таком морозе мыльный пузырь замерзает и падает, как стеклянный шарик.", "In that cold a soap bubble freezes and falls like a glass ball.", W + "Vostok_Station"),
 ("sea", "Сколько килограммов весит язык синего кита?", "How many kilograms does a blue whale's tongue weigh?",
  2700, (100, 10000), 50, 2500, ("кг", "kg"),
  "Примерно как целый слон.", "About as much as a whole elephant.", W + "Blue_whale"),
 ("myths", "Какого роста был Наполеон, в сантиметрах?", "How tall was Napoleon, in centimetres?",
  169, (140, 200), 1, 20, ("см", "cm"),
  "Обычный рост для француза того времени. Маленьким его сделали карикатуры.", "An ordinary height for a Frenchman of his day. Cartoons made him small.", W + "Napoleon"),
 ("myths", "Сколько миллионов лет назад вымерли динозавры?", "How many million years ago did the dinosaurs die out?",
  66, (1, 200), 1, 50, ("млн лет", "million years"),
  "Первые люди появились лишь около 300 тысяч лет назад: на динозаврах никто не катался.", "The first humans only appeared about 300,000 years ago: nobody ever rode a dinosaur.", W + "Cretaceous%E2%80%93Paleogene_extinction_event"),
 ("body", "Сколько костей у новорождённого малыша?", "How many bones does a newborn baby have?",
  300, (50, 600), 5, 150, ("костей", "bones"),
  "У взрослого их 206: многие кости со временем срастаются.", "An adult has 206: many of the bones grow together over time.", W + "Human_skeleton"),
 ("body", "Сколько раз за день моргает человек?", "How many times a day does a person blink?",
  15000, (100, 50000), 100, 12000, ("раз", "times"),
  "Около 15 раз в минуту, а за жизнь моргание занимает несколько лет с закрытыми глазами.", "About 15 times a minute: over a lifetime that adds up to years with your eyes shut.", W + "Blinking"),
 ("cartoons", "В каком году вышел первый «Шрек»?", "In which year did the first Shrek come out?",
  2001, (1980, 2020), 1, 15, ("год", "year"),
  "Сначала Шрек был героем детской книжки с картинками.", "Shrek began as the hero of a children's picture book.", W + "Shrek"),
 ("books", "Сколько лет было Пушкину, когда он закончил «Руслана и Людмилу»?", "How old was Pushkin when he finished Ruslan and Lyudmila?",
  21, (5, 80), 1, 10, ("лет", "years old"),
  "Он начал поэму ещё школьником, в Царскосельском лицее.", "He started the poem while still a schoolboy at the Lyceum.", W + "Ruslan_and_Ludmila"),
 ("art", "Сколько струн у большой концертной арфы?", "How many strings does a big concert harp have?",
  47, (5, 150), 1, 25, ("струн", "strings"),
  "Чтобы арфист не запутался, все струны «до» красят в красный цвет.", "So the harpist does not get lost, every C string is coloured red.", W + "Pedal_harp"),
 ("inventions", "В каком году впервые позвонили по мобильному телефону?", "In which year was the first mobile phone call made?",
  1973, (1900, 2010), 1, 25, ("год", "year"),
  "Изобретатель первым делом позвонил конкуренту, похвастаться.", "The inventor's first call was to a rival company, to brag.", W + "Mobile_phone"),
 ("inventions", "Сколько граммов весил первый мобильный телефон?", "How many grams did the first mobile phone weigh?",
  1100, (50, 5000), 10, 1000, ("г", "g"),
  "Больше килограмма, а заряда хватало на полчаса разговора.", "Over a kilogram, and the battery lasted half an hour of talking.", W + "Motorola_DynaTAC"),
 ("science", "При какой температуре закипает вода на вершине Эвереста?", "At what temperature does water boil on top of Everest?",
  70, (0, 100), 1, 25, ("°C", "°C"),
  "Воздух там редкий, поэтому вода кипит раньше, а макароны варятся вечность.", "The air is thin up there, so water boils sooner and pasta takes forever.", W + "Boiling_point"),
]

def loc(ru, en): return {"ru": ru, "en": en}


def main():
    errs = []
    prefix = {t[0]: t[1] for t in TOPICS}
    found = {}
    for topic in MC:
        for q in MC[topic]:
            found[q[0]] = ("mc", q)
        for q in CV[topic]:
            found[q[0]] = ("cv", q)
    entries = []      # (topic, kind, data without the id)
    for old, (kind, q) in found.items():
        if old in DROPPED:
            continue
        if old not in REMAP:
            errs.append("no topic for " + old)
            continue
        entries.append((REMAP[old], kind, q[1:]))
    entries += [(q[0], "mc", q[1:]) for q in NEW_MC]
    entries += [(q[0], "cv", q[1:]) for q in NEW_CV]
    order = {t[0]: i for i, t in enumerate(TOPICS)}
    entries.sort(key=lambda e: (order[e[0]], e[1] == "cv", {"easy": 0, "medium": 1}.get(e[2][0], 2)))
    count = {}
    questions = []
    for topic, kind, d in entries:
        count[topic] = count.get(topic, 0) + 1
        qid = "%s_%03d" % (prefix[topic], count[topic])
        if kind == "mc":
            diff, ru, en, opts, correct, fru, fen, src = d
            q = {"id": qid, "topic": topic, "format": "multipleChoice", "difficulty": diff, "text": loc(ru, en),
                 "options": [loc(a, b) for a, b in opts], "correct": correct, "fact": loc(fru, fen), "source": src}
            for a, b in opts:
                if len(a) > 40 or len(b) > 40: errs.append(qid + " option too long: " + a + " / " + b)
        else:
            ru, en, ans, rng, step, tol, unit, fru, fen, src = d
            q = {"id": qid, "topic": topic, "format": "closestValue", "difficulty": "hard", "text": loc(ru, en),
                 "answer": ans, "range": list(rng), "step": step, "tolerance": tol, "unit": loc(*unit), "fact": loc(fru, fen), "source": src}
            if not (rng[0] < ans < rng[1]): errs.append(qid + " answer not strictly inside range")
            # Room on the slider around the answer (two steps below, six above), and a lazy guess at either end of the
            # slider must score nothing.
            if ans - 2 * step < rng[0] or ans + 6 * step > rng[1]: errs.append(qid + " answer too close to an end of the slider")
            # A guess six steps off still scores.
            if tol <= 6 * step: errs.append(qid + " tolerance must be more than six steps")
            if tol >= min(ans - rng[0], rng[1] - ans): errs.append(qid + " tolerance reaches an end of the slider")
        if len(ru) > 120 or len(en) > 120: errs.append(qid + " text too long %d/%d" % (len(ru), len(en)))
        if len(fru) > 160 or len(fen) > 160: errs.append(qid + " fact too long %d/%d" % (len(fru), len(fen)))
        questions.append(q)
    # Every topic must fill a round: easy and medium multiple choice, hard closest value.
    for t in TOPICS:
        qs = [q for q in questions if q["topic"] == t[0]]
        e = sum(1 for q in qs if q["difficulty"] == "easy")
        m = sum(1 for q in qs if q["difficulty"] == "medium")
        h = sum(1 for q in qs if q["format"] == "closestValue")
        if e < 2 or m < 2 or h < 2: errs.append("%s is thin: %d easy, %d medium, %d hard" % (t[0], e, m, h))
    race = [{"id": "race_%03d" % (i + 1), "statement": loc(ru, en), "answer": a, "source": s} for i, (ru, en, a, s) in enumerate(RACE)]
    for r in race:
        if len(r["statement"]["ru"]) > 120 or len(r["statement"]["en"]) > 120: errs.append(r["id"] + " too long")
    if errs:
        print("\n".join(errs))
        sys.exit(1)

    root = sys.argv[1] if len(sys.argv) > 1 else "."
    def dump(name, data):
        with open(root + "/content/" + name, "w", encoding="utf-8", newline="\n") as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
            f.write("\n")
    dump("questions.json", questions)
    dump("race.json", race)
    topics = [{"id": t[0], "title": loc(t[2], t[3])} for t in TOPICS] + [{"id": MIXED[0], "title": loc(MIXED[1], MIXED[2]), "mixed": True}]
    dump("topics.json", topics)
    from collections import Counter
    print(len(questions), "questions in", len(TOPICS), "topics,", len(race), "race statements;",
          dict(Counter((q["format"], q["difficulty"]) for q in questions)))


main()
