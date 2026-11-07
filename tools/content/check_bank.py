# Checks the question bank sources in tools/content/bank: one <topic>.json per topic of tools/content/bank/topics.json,
# each a list of 50 questions without ids (build_questions.py gives the ids). Run from the repo root:
#   python tools/content/check_bank.py                     every topic file there is
#   python tools/content/check_bank.py tools/content/bank/space.json [...]
#   python tools/content/check_bank.py --refined [files]   the refined bank in tools/content/refined (decisions/0020, D130):
#                                                          whole rounds per topic, k times (3 easy, 5 medium, 2 hard), so 10
#                                                          or 20 questions, with basedOn (a v4 question id or null) and
#                                                          refine (same, rewrite, new)
# Rules: the schema limits of docs/15_schemas.md, the kinds a round can use (config.json: easy and medium multiple
# choice, hard closest value), and closest value sliders that leave room and never pay for a guess at either end.
import json, os, re, sys

BANK = os.path.join("tools", "content", "bank")
COUNTS = {("multipleChoice", "easy"): 15, ("multipleChoice", "medium"): 23, ("closestValue", "hard"): 12}
MC_KEYS = {"format", "difficulty", "text", "options", "correct", "fact", "source", "kids"}
CV_KEYS = {"format", "difficulty", "text", "answer", "range", "step", "tolerance", "unit", "fact", "source", "kids"}
CYR = re.compile("[А-Яа-яЁё]")


def loc(value, limit, where, errs, allow_empty=False):
    if not isinstance(value, dict) or set(value) != {"ru", "en"}:
        errs.append(where + " must be {\"ru\": ..., \"en\": ...}")
        return
    for lang in ("ru", "en"):
        s = value[lang]
        if not isinstance(s, str):
            errs.append(where + "." + lang + " must be a string")
            continue
        if not allow_empty and not s.strip():
            errs.append(where + "." + lang + " is empty")
        if limit and len(s) > limit:
            errs.append("%s.%s is %d characters, max %d: %s" % (where, lang, len(s), limit, s))
        if s != s.strip() or "  " in s:
            errs.append(where + "." + lang + " has stray spaces")
    if isinstance(value.get("ru"), str) and value["ru"].strip() and not CYR.search(value["ru"]) and not re.search(r"\d", value["ru"]):
        errs.append(where + ".ru has no Russian letters: " + value["ru"])
    if isinstance(value.get("en"), str) and CYR.search(value["en"]):
        errs.append(where + ".en has Cyrillic letters: " + value["en"])


def on_grid(x, lo, step):
    k = (x - lo) / step
    return abs(k - round(k)) < 1e-6


REFINED = os.path.join("tools", "content", "refined")
REFINED_COUNTS = {("multipleChoice", "easy"): 3, ("multipleChoice", "medium"): 5, ("closestValue", "hard"): 2}
REFINE_KINDS = {"same", "rewrite", "new"}
refined_mode = False


def check_question(q, where, errs):
    if not isinstance(q, dict):
        errs.append(where + " is not an object")
        return
    kind = (q.get("format"), q.get("difficulty"))
    if kind not in COUNTS:
        errs.append(where + " must be easy or medium multipleChoice, or hard closestValue (got %s %s)" % kind)
        return
    keys = MC_KEYS if kind[0] == "multipleChoice" else CV_KEYS
    if refined_mode:
        keys = keys | {"basedOn", "refine", "id"}
        if q.get("refine") not in REFINE_KINDS:
            errs.append(where + ".refine must be same, rewrite or new")
        based = q.get("basedOn")
        if q.get("refine") == "new" and based is not None:
            errs.append(where + ".basedOn must be null for a new question")
        if q.get("refine") in ("same", "rewrite") and not (isinstance(based, str) and re.match(r"^[a-z0-9_]+_\d{3}$", based)):
            errs.append(where + ".basedOn must be the v4 question id it comes from")
    extra = set(q) - keys
    if extra:
        errs.append(where + " has unknown fields " + ", ".join(sorted(extra)))
    missing = (keys - {"kids", "basedOn", "id"}) - set(q)
    if missing:
        errs.append(where + " is missing " + ", ".join(sorted(missing)))
        return
    loc(q["text"], 120, where + ".text", errs)
    loc(q["fact"], 160, where + ".fact", errs)
    if not isinstance(q["source"], str) or not q["source"].startswith("http"):
        errs.append(where + ".source must be a URL")
    if "kids" in q and not isinstance(q["kids"], bool):
        errs.append(where + ".kids must be true or false")
    if kind[0] == "multipleChoice":
        opts = q["options"]
        if not isinstance(opts, list) or len(opts) != 4:
            errs.append(where + ".options must list 4 options")
            return
        for i, o in enumerate(opts):
            loc(o, 40, "%s.options[%d]" % (where, i), errs)
        for lang in ("ru", "en"):
            seen = [o.get(lang, "").strip().lower() for o in opts if isinstance(o, dict)]
            if len(set(seen)) != len(seen):
                errs.append(where + ".options repeat in " + lang)
        if not isinstance(q["correct"], int) or isinstance(q["correct"], bool) or not 0 <= q["correct"] <= 3:
            errs.append(where + ".correct must be 0 to 3")
    else:
        ans, rng, step, tol = q["answer"], q["range"], q["step"], q["tolerance"]
        nums = [ans, step, tol] + (rng if isinstance(rng, list) else [])
        if not isinstance(rng, list) or len(rng) != 2 or not all(isinstance(x, (int, float)) and not isinstance(x, bool) for x in nums):
            errs.append(where + " answer, range [min, max], step and tolerance must be numbers")
            return
        lo, hi = rng
        if not (step > 0 and tol > 0):
            errs.append(where + " step and tolerance must be positive")
            return
        if not lo < ans < hi:
            errs.append(where + " answer must lie strictly inside the range")
        if not on_grid(ans, lo, step) or not on_grid(hi, lo, step):
            errs.append(where + " answer and max must sit on the slider grid: min + a whole number of steps")
        if ans - 2 * step < lo or ans + 6 * step > hi:
            errs.append(where + " needs room on the slider: answer - 2 steps >= min and answer + 6 steps <= max")
        if tol <= 6 * step:
            errs.append(where + " tolerance must be more than 6 steps (a guess 6 steps off still scores)")
        if tol >= min(ans - lo, hi - ans):
            errs.append(where + " tolerance must be less than the distance from the answer to either end of the slider")
        if (hi - lo) / step > 2000:
            errs.append(where + " the slider has more than 2000 steps: use a bigger step")
        loc(q["unit"], 20, where + ".unit", errs, allow_empty=True)


def check_file(path, topic_ids):
    errs = []
    name = os.path.splitext(os.path.basename(path))[0]
    if name not in topic_ids:
        return [path + ": " + name + " is not a topic id in bank/topics.json"]
    try:
        with open(path, encoding="utf-8") as f:
            data = json.load(f)
    except Exception as e:
        return [path + ": not valid JSON: " + str(e)]
    if not isinstance(data, list):
        return [path + ": must be a JSON list"]
    count = {}
    texts = {}
    ids = {}
    based = {}
    for i, q in enumerate(data):
        where = "%s[%d]" % (name, i)
        check_question(q, where, errs)
        if isinstance(q, dict) and refined_mode:
            # A refined question keeps its id for good (seen lists and ratings refer to it); a new one gets none
            # until build_refined.py gives it the next free number.
            if "id" in q:
                if not re.match(r"^cur_%s_\d{2,3}$" % re.escape(name), str(q["id"])):
                    errs.append("%s.id must be cur_%s_NN" % (where, name))
                elif q["id"] in ids:
                    errs.append("%s repeats the id of %s[%d]" % (where, name, ids[q["id"]]))
                ids[q.get("id")] = i
            if q.get("basedOn"):
                if q["basedOn"] in based:
                    errs.append("%s is based on %s, like %s[%d]" % (where, q["basedOn"], name, based[q["basedOn"]]))
                based[q["basedOn"]] = i
        if isinstance(q, dict):
            kind = (q.get("format"), q.get("difficulty"))
            count[kind] = count.get(kind, 0) + 1
            t = q.get("text", {}).get("ru", "") if isinstance(q.get("text"), dict) else ""
            key = re.sub(r"\W+", " ", t.lower()).strip()
            if key in texts:
                errs.append("%s repeats the question of %s[%d]" % (where, name, texts[key]))
            texts[key] = i
    counts = COUNTS
    if refined_mode:
        # A refined topic holds whole rounds: k times the 3, 5, 2 mix (D130 asks for at least 20, so k = 2 and up).
        k = max(1, round(sum(count.values()) / sum(REFINED_COUNTS.values())))
        counts = {kind: n * k for kind, n in REFINED_COUNTS.items()}
    for kind, n in counts.items():
        if count.get(kind, 0) != n:
            errs.append("%s has %d %s %s questions, needs exactly %d" % (name, count.get(kind, 0), kind[1], kind[0], n))
    return errs


def main():
    global refined_mode
    args = sys.argv[1:]
    if "--refined" in args:
        refined_mode = True
        args.remove("--refined")
    folder = REFINED if refined_mode else BANK
    with open(os.path.join(BANK, "topics.json"), encoding="utf-8") as f:
        topic_ids = {t["id"] for t in json.load(f)}
    paths = args or sorted(os.path.join(folder, p) for p in os.listdir(folder) if p.endswith(".json") and p != "topics.json")
    errs = []
    for p in paths:
        errs += check_file(p, topic_ids)
    if errs:
        print("\n".join(errs))
        print("%d problems" % len(errs))
        sys.exit(1)
    print("OK: %d file(s)" % len(paths))


if __name__ == "__main__":
    main()
