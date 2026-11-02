// Validates the files in content/ against docs/15_schemas.md and docs/03_rules.md.
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
for (const host of ["konstantin", "ilja"]) {
  if (!lines.some((l) => l.host === host && l.trigger === "gameStart")) err("host_lines.json", host, "needs a gameStart line (the Intro plays one per host)");
}

const SHAPES = ["circle", "square", "triangle", "diamond", "star", "hexagon"];
const avatars = load("avatars.json");
if (!Array.isArray(avatars) || avatars.length !== 12) err("avatars.json", "-", "must hold exactly 12 avatars");
const avatarIds = new Set();
for (const a of avatars) {
  if (!Number.isInteger(a.id) || a.id < 0 || a.id > 11) err("avatars.json", a.id, "id must be 0 to 11");
  if (avatarIds.has(a.id)) err("avatars.json", a.id, "duplicate id");
  avatarIds.add(a.id);
  if (!/^#[0-9A-Fa-f]{6}$/.test(a.color ?? "")) err("avatars.json", a.id, "color must be #RRGGBB");
  if (!SHAPES.includes(a.shape)) err("avatars.json", a.id, `shape must be one of ${SHAPES.join(", ")}`);
}

const cfg = load("config.json");
// Every key in the table of docs/03_rules.md must exist.
const REQUIRED = [
  "configVersion", "timers.intro", "timers.topicVote", "timers.questionShow", "timers.answer.multipleChoice",
  "timers.answer.closestValue", "timers.lastCall", "timers.reveal", "timers.scores", "timers.roundEnd",
  "timers.finalIntro", "timers.results", "rounds.count", "rounds.questionsPerRound", "rounds.multipliers",
  "rounds.difficultyOrder", "rounds.formatOrder", "topics.doors", "topics.minUnseenToOffer", "topics.mixedTopicId",
  "scoring.maxPoints", "scoring.speedPenalty", "scoring.roundTo", "scoring.fastAnswerMs", "closestValue.closestBonus",
  "powerUps.enabled", "powerUps.types", "powerUps.targeting", "powerUps.drawChanceLeader", "powerUps.maxHeld",
  "powerUps.maxHitsPerQuestion", "powerUps.immunityQuestions", "powerUps.onPhone", "powerUps.freeze.taps",
  "powerUps.slime.blobs", "powerUps.fog.seconds", "race.maxHeadStart", "race.finishSteps", "race.maxQuestions",
  "race.stepsPerCorrect", "race.answer", "race.reveal", "players.min", "players.max", "players.timerMultiplierOptions",
  "players.seatHoldSeconds", "lobby.startCountdownSeconds", "net.inputGraceMs", "net.tvReconnectSeconds",
  "hosts.minGapSeconds", "hosts.bubbleSeconds", "hosts.streakTrigger", "poll.enabled", "poll.maxSeconds",
];
for (const key of REQUIRED) {
  if (key.split(".").reduce((o, k) => (o == null ? undefined : o[k]), cfg) === undefined) err("config.json", key, "missing key");
}
if (!topics.some((t) => t.id === cfg.topics?.mixedTopicId && t.mixed)) err("config.json", "topics.mixedTopicId", "must name the mixed topic");

// lastPlaceCanWin (docs/02_show_flow.md): with the largest head start gap (leader maxHeadStart, last place 0),
// (leader head start - last place head start) < maxQuestions and (finishSteps - last place head start) <= maxQuestions.
const r = cfg.race ?? {};
if (!(r.maxHeadStart - 0 < r.maxQuestions)) err("config.json", "race", "last place could not win: maxHeadStart must be less than maxQuestions");
if (!(r.finishSteps - 0 <= r.maxQuestions)) err("config.json", "race", "last place could not win: finishSteps must be at most maxQuestions");

// Coverage report for game night 1
const perTopic = {};
for (const q of questions) {
  perTopic[q.topic] ??= { multipleChoice: 0, closestValue: 0 };
  perTopic[q.topic][q.format]++;
}
console.log("Questions per topic:", JSON.stringify(perTopic));
console.log(`${questions.length} questions, ${race.length} race statements, ${lines.length} host lines, ${avatars.length} avatars`);

if (errors.length) {
  console.error(`\n${errors.length} error(s):\n` + errors.join("\n"));
  process.exit(1);
}
console.log("Content OK");
