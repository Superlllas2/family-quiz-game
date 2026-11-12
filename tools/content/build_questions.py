# Generates content/questions.json, race.json and topics.json. Run from the repo root:
#   python tools/content/build_questions.py .
# The round questions come from the bank in tools/content/bank: topics.json lists the topics (a topic vote offers three
# of them at random plus "A bit of everything"), and one <topic>.json per topic holds its 50 questions without ids,
# checked by tools/content/check_bank.py (schema limits, the kinds a round uses, closest value slider rules). Ids are
# <topic>_NNN in file order, sorted easy, medium, closest value. The race statements are below. Then run
# node tools/validate_content.mjs.
import json, os, sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import check_bank

W = "https://en.wikipedia.org/wiki/"

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

MIXED = ("mixed", "Всего понемногу", "A bit of everything")


def loc(ru, en): return {"ru": ru, "en": en}


def main():
    root = sys.argv[1] if len(sys.argv) > 1 else "."
    os.chdir(root)
    with open(os.path.join(check_bank.BANK, "topics.json"), encoding="utf-8") as f:
        topics = json.load(f)
    ids = [t["id"] for t in topics]
    errs = []
    if len(set(ids)) != len(ids):
        errs.append("bank/topics.json repeats a topic id")
    for t in topics:
        if len(t["ru"]) > 30 or len(t["en"]) > 30:
            errs.append(t["id"] + " title is longer than 30 characters")
    order = {"easy": 0, "medium": 1, "hard": 2}
    questions = []
    for t in topics:
        path = os.path.join(check_bank.BANK, t["id"] + ".json")
        if not os.path.exists(path):
            errs.append(path + " is missing")
            continue
        errs += check_bank.check_file(path, set(ids))
        with open(path, encoding="utf-8") as f:
            bank = json.load(f)
        bank = sorted(bank, key=lambda q: order.get(q.get("difficulty"), 3))   # stable: keeps the file order per kind
        for i, q in enumerate(bank):
            out = {"id": "%s_%03d" % (t["id"], i + 1), "topic": t["id"]}
            out.update(q)
            questions.append(out)
    race = [{"id": "race_%03d" % (i + 1), "statement": loc(ru, en), "answer": a, "source": s} for i, (ru, en, a, s) in enumerate(RACE)]
    for r in race:
        if len(r["statement"]["ru"]) > 120 or len(r["statement"]["en"]) > 120: errs.append(r["id"] + " too long")
    if errs:
        print("\n".join(errs))
        sys.exit(1)

    def dump(name, data):
        with open(os.path.join("content", name), "w", encoding="utf-8", newline="\n") as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
            f.write("\n")
    dump("questions.json", questions)
    dump("race.json", race)
    # The shelf fields (D154) come with each topic: shelf, category, lean, russian.
    dump("topics.json", [{"id": t["id"], "title": loc(t["ru"], t["en"]), "shelf": t["shelf"], "category": t["category"],
                          "lean": t["lean"], "russian": t["russian"]} for t in topics]
         + [{"id": MIXED[0], "title": loc(MIXED[1], MIXED[2]), "mixed": True}])
    from collections import Counter
    print(len(questions), "questions in", len(topics), "topics,", len(race), "race statements;",
          dict(Counter((q["format"], q["difficulty"]) for q in questions)))


main()
