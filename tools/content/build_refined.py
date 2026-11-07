# Generates content/questions_refined.json, the game's only question bank (D130; first the curated test bank of
# decisions/0020), from tools/content/refined. Run from
# the repo root:
#   python tools/content/build_refined.py
# Each tools/content/refined/<topic>.json holds whole rounds: k times (3 easy, 5 medium multiple choice, 2 hard closest value)
# with basedOn (the full bank question id it comes from, or null) and refine (same, rewrite, new); check_bank.py
# --refined checks them first. Ids are cur_<topic>_NN in file order, sorted easy, medium, closest value. A "same"
# question is copied again from content/questions.json, so it stays an exact copy of its full bank question.
# tools/content/set_aside.json lists full bank questions kept out of the refined bank, each with its reason (for
# example Russia outside the Russian topics); a refined question may not be based on one of them.
import json, os, sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import check_bank

ROOT = os.getcwd()
REFINED = os.path.join(ROOT, "tools", "content", "refined")
ORDER = {("multipleChoice", "easy"): 0, ("multipleChoice", "medium"): 1, ("closestValue", "hard"): 2}
MC = ["format", "difficulty", "text", "options", "correct", "fact", "source"]
CV = ["format", "difficulty", "text", "answer", "range", "step", "tolerance", "unit", "fact", "source"]


def main():
    sys.argv = [sys.argv[0], "--refined"]
    try:
        check_bank.main()
    except SystemExit as e:
        if e.code:
            sys.exit(e.code)
    with open(os.path.join(ROOT, "tools", "content", "bank", "topics.json"), encoding="utf-8") as f:
        topics = [t["id"] for t in json.load(f)]
    with open(os.path.join(ROOT, "content", "questions.json"), encoding="utf-8") as f:
        full = {q["id"]: q for q in json.load(f)}
    with open(os.path.join(ROOT, "tools", "content", "set_aside.json"), encoding="utf-8") as f:
        set_aside = json.load(f)
    out = []
    errors = ["set_aside.json: %s is not a full bank question" % i for i in set_aside if i not in full]
    for topic in topics:
        path = os.path.join(REFINED, topic + ".json")
        if not os.path.exists(path):
            continue
        with open(path, encoding="utf-8") as f:
            items = json.load(f)
        items = sorted(items, key=lambda q: ORDER[(q["format"], q["difficulty"])])
        for n, q in enumerate(items, 1):
            # The game loads only this bank (D130), so the checks against the full bank live here.
            based = q.get("basedOn")
            if based is not None:
                if based not in full:
                    errors.append("%s #%d: basedOn %s is not a full bank question" % (topic, n, based))
                    continue
                if full[based]["topic"] != topic:
                    errors.append("%s #%d: basedOn %s is a question of another topic" % (topic, n, based))
                if based in set_aside:
                    errors.append("%s #%d: basedOn %s is set aside: %s" % (topic, n, based, set_aside[based]))
            if q["refine"] == "same":
                src = full[q["basedOn"]]
                q = {k: src[k] for k in (MC if src["format"] == "multipleChoice" else CV)} | {"basedOn": q["basedOn"], "refine": "same"}
            keys = MC if q["format"] == "multipleChoice" else CV
            entry = {"id": "cur_%s_%02d" % (topic, n), "topic": topic}
            entry.update({k: q[k] for k in keys})
            entry["basedOn"] = q.get("basedOn")
            entry["refine"] = q["refine"]
            out.append(entry)
    if errors:
        print("\n".join(errors))
        sys.exit(1)
    target = os.path.join(ROOT, "content", "questions_refined.json")
    with open(target, "w", encoding="utf-8", newline="\n") as f:
        f.write("[\n" + ",\n".join(json.dumps(q, ensure_ascii=False) for q in out) + "\n]\n")
    kinds = {}
    for q in out:
        kinds[q["refine"]] = kinds.get(q["refine"], 0) + 1
    print("wrote %d questions in %d topics to %s (%s)" % (len(out), len({q["topic"] for q in out}), target, kinds))


if __name__ == "__main__":
    main()
