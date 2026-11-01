// Validates the files in content/ against docs/15_schemas.md.
// Usage: node tools/validate_content.mjs
import { readFileSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..", "content");
const load = (f) => JSON.parse(readFileSync(join(root, f), "utf8"));
const errors = [];
const err = (file, id, msg) => errors.push(`${file} ${id}: ${msg}`);

const isLoc = (v, max) =>
  v && typeof v.ru === "string" && typeof v.en === "string" &&
  (max === undefined || (v.ru.length <= max && v.en.length <= max));

const TRIGGERS = ["gameStart", "roundStart", "everyoneCorrect", "nobodyCorrect", "fastAnswer", "newLeader",
  "streak3", "powerUpUsed", "closestExact", "lastPlaceCorrect", "roundEnd", "finalStart", "raceWinner", "lastCall"];

const topics = load("topics.json");
const topicIds = new Set(topics.map((t) => t.id));
topics.forEach((t) => { if (!isLoc(t.title, 30)) err("topics.json", t.id, "title must have ru and en, max 30"); });
if (topics.filter((t) => t.mixed).length !== 1) err("topics.json", "-", "exactly one topic must be mixed");

const ids = new Set();
const unique = (file, id) => { if (ids.has(id)) err(file, id, "duplicate id"); ids.add(id); };

const questions = load("questions.json");
for (const q of questions) {
  unique("questions.json", q.id);
  if (!topicIds.has(q.topic) || topics.find((t) => t.id === q.topic)?.mixed) err("questions.json", q.id, "unknown or mixed topic");
  if (!["easy", "medium", "hard"].includes(q.difficulty)) err("questions.json", q.id, "bad difficulty");
  if (!isLoc(q.text, 120)) err("questions.json", q.id, "text must have ru and en, max 120");
  if (!q.source) err("questions.json", q.id, "source missing");
  if (q.format === "multipleChoice") {
    if (!Array.isArray(q.options) || q.options.length !== 4) err("questions.json", q.id, "needs 4 options");
    else q.options.forEach((o, i) => { if (!isLoc(o, 40)) err("questions.json", q.id, `option ${i} must have ru and en, max 40`); });
    if (!Number.isInteger(q.correct) || q.correct < 0 || q.correct > 3) err("questions.json", q.id, "correct must be 0 to 3");
  } else if (q.format === "closestValue") {
    const [min, max] = q.range ?? [];
    if (!(min < q.answer && q.answer < max)) err("questions.json", q.id, "answer must lie strictly inside range");
    if (!(q.step > 0) || !(q.tolerance > 0)) err("questions.json", q.id, "step and tolerance must be positive");
    if (!isLoc(q.unit)) err("questions.json", q.id, "unit must have ru and en");
  } else err("questions.json", q.id, "bad format");
}

const race = load("race.json");
for (const r of race) {
  unique("race.json", r.id);
  if (!isLoc(r.statement, 120)) err("race.json", r.id, "statement must have ru and en, max 120");
  if (typeof r.answer !== "boolean") err("race.json", r.id, "answer must be true or false");
}

const lines = load("host_lines.json");
for (const l of lines) {
  unique("host_lines.json", l.id);
  if (!["konstantin", "ilja"].includes(l.host)) err("host_lines.json", l.id, "bad host");
  if (!TRIGGERS.includes(l.trigger)) err("host_lines.json", l.id, "unknown trigger");
  if (!isLoc(l.text, 90)) err("host_lines.json", l.id, "text must have ru and en, max 90");
}

const cfg = load("config.json");
if (cfg.race.finishSteps > cfg.race.maxQuestions) err("config.json", "race", "last place could not win: finishSteps > maxQuestions");

// Coverage report for game night 1
const perTopic = {};
for (const q of questions) {
  perTopic[q.topic] ??= { multipleChoice: 0, closestValue: 0 };
  perTopic[q.topic][q.format]++;
}
console.log("Questions per topic:", JSON.stringify(perTopic));
console.log(`${questions.length} questions, ${race.length} race statements, ${lines.length} host lines`);

if (errors.length) {
  console.error(`\n${errors.length} error(s):\n` + errors.join("\n"));
  process.exit(1);
}
console.log("Content OK");
