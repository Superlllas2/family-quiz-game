# Generates content/questions.json, race.json and topic titles for the questions v2 rewrite.
# MC: (id, difficulty, ru, en, [(ru, en) x4], correct_index, fact_ru, fact_en, source)
# CV: (id, ru, en, answer, (min, max), step, tolerance, (unit_ru, unit_en), fact_ru, fact_en, source)
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
  4000, (0, 20000), 100, 6000, ("лет назад", "years ago"),
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
 ("nat_010", "Сколько часов в сутки спит коала?", "How many hours a day does a koala sleep?",
  20, (0, 24), 1, 8, ("часов", "hours"),
  "Эвкалипт почти не даёт сил, вот коала и экономит.", "Eucalyptus gives very little energy, so the koala saves it.", W + "Koala"),
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
  92, (0, 100), 1, 30, ("%", "%"),
  "Поэтому в жару арбуз и пьют, и едят одновременно.", "That is why on a hot day you eat and drink a watermelon at once.", W + "Watermelon"),
 ("food_011", "При какой температуре начинает таять шоколад?", "At what temperature does chocolate start to melt?",
  34, (0, 100), 1, 25, ("°C", "°C"),
  "Чуть ниже температуры тела: поэтому он тает во рту, а не в руке.", "Just below body temperature: that is why it melts in your mouth.", W + "Cocoa_butter"),
 ("food_012", "Сколько цветков нужно собрать, чтобы получить грамм шафрана?", "How many flowers does it take to make one gram of saffron?",
  150, (1, 1000), 1, 300, ("цветков", "flowers"),
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
  8, (0, 60), 1, 20, ("минут", "minutes"),
  "Мы всегда видим Солнце таким, каким оно было восемь минут назад.", "We always see the Sun as it was eight minutes ago.", W + "Sunlight"),
 ("spa_010", "Сколько земных дней длится год на Марсе?", "How many Earth days does a year on Mars last?",
  687, (100, 2000), 1, 500, ("дней", "days"),
  "Десятилетний на Марсе отмечал бы всего пятый день рождения.", "A ten year old would only have had five birthdays on Mars.", W + "Mars"),
 ("spa_011", "В каком году люди впервые высадились на Луну?", "In which year did people first land on the Moon?",
  1969, (1940, 2000), 1, 20, ("год", "year"),
  "Их следы на Луне лежат до сих пор: там нет ветра.", "Their footprints are still there: the Moon has no wind.", W + "Apollo_11"),
]

TOPICS = {
 "geography": ("Вокруг света", "Around the world"),
 "history": ("Машина времени", "Time machine"),
 "science": ("Как это устроено", "How things work"),
 "nature": ("Странные звери", "Weird animals"),
 "food": ("Что на ужин", "What's for dinner"),
 "sport": ("Спорт и рекорды", "Sport and records"),
 "culture": ("Мультики, книги, кино", "Cartoons, books, films"),
 "space": ("Космос", "Up in space"),
 "mixed": ("Всего понемногу", "A bit of everything"),
}

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

def loc(ru, en): return {"ru": ru, "en": en}

errs = []
questions = []
for topic in MC:
    for (qid, diff, ru, en, opts, correct, fru, fen, src) in MC[topic]:
        q = {"id": qid, "topic": topic, "format": "multipleChoice", "difficulty": diff, "text": loc(ru, en),
             "options": [loc(a, b) for a, b in opts], "correct": correct, "fact": loc(fru, fen), "source": src}
        questions.append(q)
        if len(ru) > 120 or len(en) > 120: errs.append(qid + " text too long %d/%d" % (len(ru), len(en)))
        for a, b in opts:
            if len(a) > 40 or len(b) > 40: errs.append(qid + " option too long: " + a + " / " + b)
        if len(fru) > 160 or len(fen) > 160: errs.append(qid + " fact too long %d/%d" % (len(fru), len(fen)))
    for (qid, ru, en, ans, rng, step, tol, unit, fru, fen, src) in CV[topic]:
        q = {"id": qid, "topic": topic, "format": "closestValue", "difficulty": "hard", "text": loc(ru, en),
             "answer": ans, "range": list(rng), "step": step, "tolerance": tol, "unit": loc(*unit), "fact": loc(fru, fen), "source": src}
        questions.append(q)
        if not (rng[0] < ans < rng[1]): errs.append(qid + " answer not strictly inside range")
        if len(ru) > 120 or len(en) > 120: errs.append(qid + " text too long %d/%d" % (len(ru), len(en)))
        if len(fru) > 160 or len(fen) > 160: errs.append(qid + " fact too long %d/%d" % (len(fru), len(fen)))
ids = [q["id"] for q in questions]
if len(ids) != len(set(ids)): errs.append("duplicate ids")
race = [{"id": "race_%03d" % (i + 1), "statement": loc(ru, en), "answer": a, "source": s} for i, (ru, en, a, s) in enumerate(RACE)]
for r in race:
    if len(r["statement"]["ru"]) > 120 or len(r["statement"]["en"]) > 120: errs.append(r["id"] + " too long")

if errs:
    print("\n".join(errs)); sys.exit(1)
root = sys.argv[1]
def dump(name, data):
    with open(root + "/content/" + name, "w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, ensure_ascii=False, indent=2); f.write("\n")
dump("questions.json", questions)
dump("race.json", race)
topics = json.load(open(root + "/content/topics.json", encoding="utf-8"))
for t in topics:
    if t["id"] in TOPICS: t["title"] = loc(*TOPICS[t["id"]])
dump("topics.json", topics)
from collections import Counter
print(len(questions), "questions", Counter((q["format"], q["difficulty"]) for q in questions), len(race), "race statements")
