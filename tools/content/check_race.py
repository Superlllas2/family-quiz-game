# Checks the final race statements (docs/question_audit/race_plan.md, D155). Run from the repo root:
#   python tools/content/check_race.py                 every area file in tools/content/race plus content/race.json
#   python tools/content/check_race.py --merge         the same, then gives every new statement an id after the last one
#                                                      and appends it to content/race.json (only when there are no errors)
#   python tools/content/check_race.py --merge sea_a sea_b  merges only those area files (the others may still be unchecked)
# An area file is a list of {"statement": {"ru", "en"}, "answer": bool, "source": "https://...", "why": "..."}; "why"
# (one line for the checker) stays in the area file and never goes to race.json. A merged statement gets its "id" written
# back into the area file, so a second merge skips it and an id never moves.
# Errors stop a merge: the schema, the length limit, a missing source, an exact repeat. Warnings are read by a person:
# a likely repeat (shared words), a Russia keyword, a true share outside 40 to 60 % per area file.
import glob, json, os, re, sys

RACE = os.path.join("content", "race.json")
REFINED = os.path.join("content", "questions_refined.json")
AREAS = os.path.join("tools", "content", "race")
RU_LIMIT = 90
SCHEMA_LIMIT = 120
CYR = re.compile("[А-Яа-яЁё]")
STOP = set("""a an the is are was were be been of in on at to for by with and or it its it's this that than as from has have
had can could not no do does did one two three into over under more most less only all any every their they them he she his her
you your we our who which what when where there here so if but about after before up out per also just very""".split())
RUSSIA = re.compile(r"russia|soviet|ussr|moscow|petersburg|kremlin|siberia|\btsar|putin|lenin|stalin|gagarin|"
                    r"росси[яийюе]|(?<![а-яё])русск|советск|ссср|москв|петербург|кремл|сибир|царь|ленин|сталин|гагарин", re.I)


def words(text):
    out = set()
    for w in re.findall(r"[a-z0-9]+", text.lower()):
        if w in STOP or len(w) < 3 and not w.isdigit():
            continue
        out.add(w[:-1] if len(w) > 4 and w.endswith("s") else w)
    return out


def norm(text):
    return " ".join(re.findall(r"[a-zа-яё0-9]+", text.lower()))


def load(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def main():
    merge = "--merge" in sys.argv
    only = [a for a in sys.argv[1:] if not a.startswith("--")]
    errs, warns = [], []
    race = load(RACE)
    areas = {os.path.basename(p)[:-5]: load(p) for p in sorted(glob.glob(os.path.join(AREAS, "*.json")))}
    merged_ids = {r["id"] for r in race}

    # Every statement once: the ones in race.json, then the area files' ones not yet merged.
    items = [("race.json", r["id"], r) for r in race]
    for area, lst in areas.items():
        if not isinstance(lst, list):
            errs.append(area + ": must be a list")
            continue
        for i, s in enumerate(lst):
            if s.get("id") in merged_ids:
                continue
            items.append((area, "%s#%d" % (area, i + 1), s))

    for where, key, s in items:
        tag = "%s %s" % (where, key)
        st = s.get("statement")
        if not isinstance(st, dict) or set(st) != {"ru", "en"} or not all(isinstance(st[l], str) and st[l].strip() for l in ("ru", "en")):
            errs.append(tag + ": statement must be {ru, en}, both non-empty")
            continue
        if where != "race.json":
            if len(st["ru"]) > RU_LIMIT:
                errs.append("%s: ru is %d characters, max %d: %s" % (tag, len(st["ru"]), RU_LIMIT, st["ru"]))
            if set(s) - {"statement", "answer", "source", "why", "id"}:
                errs.append(tag + ": unknown keys " + ", ".join(sorted(set(s) - {"statement", "answer", "source", "why", "id"})))
        for l in ("ru", "en"):
            if len(st[l]) > SCHEMA_LIMIT:
                errs.append("%s: %s over %d characters" % (tag, l, SCHEMA_LIMIT))
            if st[l] != st[l].strip() or "  " in st[l]:
                errs.append(tag + ": stray spaces in " + l)
        if not CYR.search(st["ru"]):
            errs.append(tag + ": ru has no Russian letters")
        if CYR.search(st["en"]):
            errs.append(tag + ": en has Cyrillic letters")
        if not isinstance(s.get("answer"), bool):
            errs.append(tag + ": answer must be true or false")
        if not isinstance(s.get("source"), str) or not s["source"].startswith("http"):
            errs.append(tag + ": source must be a URL")
        if where != "race.json" and RUSSIA.search(st["ru"] + " " + st["en"]):
            warns.append("%s: Russia keyword, read it: %s" % (tag, st["ru"]))

    # Exact and likely repeats among the statements.
    seen = {}
    ws = []
    for where, key, s in items:
        st = s.get("statement")
        if not isinstance(st, dict) or "en" not in st:
            continue
        for l in ("ru", "en"):
            n = norm(st.get(l, ""))
            if n in seen:
                errs.append("%s %s repeats %s exactly: %s" % (where, key, seen[n], st[l]))
            seen[n] = where + " " + key
        ws.append((where + " " + key, words(st["en"]), st["en"]))
    for i in range(len(ws)):
        for j in range(i):
            a, b = ws[i][1], ws[j][1]
            if not a or not b:
                continue
            shared = len(a & b)
            if shared >= 3 and shared / min(len(a), len(b)) >= 0.6:
                warns.append("likely repeat: %s «%s» ~ %s «%s»" % (ws[i][0], ws[i][2], ws[j][0], ws[j][2]))

    # A statement that gives a refined question's answer away (same evening).
    if os.path.exists(REFINED):
        qs = []
        for q in load(REFINED):
            text = q["text"]["en"]
            if q.get("format") == "multipleChoice":
                text += " " + q["options"][q["correct"]]["en"]
            qs.append((q["id"], words(text)))
        for name, w, en in ws:
            if name.startswith("race.json") or len(w) < 3:
                continue
            for qid, qw in qs:
                shared = len(w & qw)
                if shared >= 3 and shared / len(w) >= 0.6:
                    warns.append("near a refined question: %s «%s» ~ %s" % (name, en, qid))

    for area, lst in areas.items():
        new = [s for s in lst if isinstance(s, dict) and s.get("id") not in merged_ids]
        if len(new) >= 10:
            share = sum(1 for s in new if s.get("answer") is True) / len(new)
            if not 0.4 <= share <= 0.6:
                warns.append("%s: %d %% true (keep 40 to 60 %%)" % (area, round(share * 100)))

    for w in warns:
        print("WARN  " + w)
    for e in errs:
        print("ERROR " + e)
    pending = sum(1 for where, _, _ in items if where != "race.json")
    total_true = sum(1 for _, _, s in items if s.get("answer") is True)
    print("%d in race.json, %d new in %d area files, %d %% true overall; %d errors, %d warnings"
          % (len(race), pending, len(areas), round(100 * total_true / max(1, len(items))), len(errs), len(warns)))
    if errs:
        sys.exit(1)

    if merge and pending:
        last = max(int(r["id"].split("_")[1]) for r in race)
        for area, lst in areas.items():
            if only and area not in only:
                continue
            changed = False
            for s in lst:
                if s.get("id") in merged_ids:
                    continue
                last += 1
                s["id"] = "race_%03d" % last
                race.append({"id": s["id"], "statement": s["statement"], "answer": s["answer"], "source": s["source"]})
                changed = True
            if changed:
                write(os.path.join(AREAS, area + ".json"), lst)
        with open(RACE, "w", encoding="utf-8") as f:
            f.write(json.dumps(race, ensure_ascii=False, indent=2) + "\n")
        print("merged %s: race.json now has %d statements" % (", ".join(only) or "every area file", len(race)))


def write(path, lst):
    with open(path, "w", encoding="utf-8") as f:
        f.write("[\n" + ",\n".join("  " + json.dumps(x, ensure_ascii=False) for x in lst) + "\n]\n")


if __name__ == "__main__":
    main()
