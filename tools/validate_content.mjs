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

const TRIGGERS = ["playerJoined", "startCountdown", "gameStart", "roundStart", "everyoneCorrect", "nobodyCorrect", "fastAnswer", "newLeader",
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

const facts = load("facts.json");
if (!Array.isArray(facts) || facts.length < 1) err("facts.json", "-", "needs at least one fact");
for (const f of facts ?? []) {
  unique("facts.json", f.id);
  if (!isLoc(f.text, 160)) err("facts.json", f.id, "text must have ru and en, max 160");
  if (typeof f.source !== "string" || !f.source.trim()) err("facts.json", f.id, "source is missing");
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

// Poll (docs/15_schemas.md, docs/20_playtest.md): the same checks as the TV app's PollLoader.
const poll = load("poll.json");
if (!Array.isArray(poll) || poll.length < 1) err("poll.json", "-", "needs at least one question");
for (const q of poll ?? []) {
  unique("poll.json", q.id);
  if (!isLoc(q.text, 120) || !q.text.ru.trim() || !q.text.en.trim()) err("poll.json", q.id, "text must have non empty ru and en, max 120");
  if (q.kind === "choice") {
    if (!Array.isArray(q.options) || q.options.length < 2) err("poll.json", q.id, "options must list 2 or more choices");
    else q.options.forEach((o, i) => { if (!isLoc(o, 40)) err("poll.json", q.id, `options[${i}] must have ru and en, max 40`); });
  } else if (q.kind === "text") {
    if (Array.isArray(q.options) && q.options.length) err("poll.json", q.id, "options are only for choice questions");
  } else err("poll.json", q.id, "kind must be choice or text");
}

// Characters (docs/15_schemas.md): ids 0..N-1 in order, at least players.max of them, unique names and colors,
// and every color readable behind the chip text color #14161C (contrast ratio 4.5 or more).
const characters = load("characters.json");
const lin = (c) => { c /= 255; return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4; };
const lum = (h) => { const n = parseInt(h.slice(1), 16); return 0.2126 * lin(n >> 16) + 0.7152 * lin((n >> 8) & 255) + 0.0722 * lin(n & 255); };
const contrast = (a, b) => (Math.max(lum(a), lum(b)) + 0.05) / (Math.min(lum(a), lum(b)) + 0.05);
const maxPlayers = load("config.json").players?.max ?? 0;
if (!Array.isArray(characters) || characters.length < maxPlayers) err("characters.json", "-", "needs at least players.max characters");
const charNames = { ru: new Set(), en: new Set() };
const charColors = new Set();
characters.forEach((c, i) => {
  if (c.id !== i) err("characters.json", c.id, "ids must be 0, 1, 2, ... in order");
  if (!isLoc(c.name, 16)) err("characters.json", c.id, "name must have ru and en, max 16");
  else for (const l of ["ru", "en"]) {
    const k = c.name[l].toLowerCase();
    if (charNames[l].has(k)) err("characters.json", c.id, "duplicate " + l + " name");
    charNames[l].add(k);
  }
  if (!isLoc(c.personality, 60)) err("characters.json", c.id, "personality must have ru and en, max 60");
  if (!/^#[0-9A-Fa-f]{6}$/.test(c.color ?? "")) err("characters.json", c.id, "color must be #RRGGBB");
  else {
    if (charColors.has(c.color.toUpperCase())) err("characters.json", c.id, "duplicate color");
    charColors.add(c.color.toUpperCase());
    if (contrast(c.color, "#14161C") < 4.5) err("characters.json", c.id, "color needs a contrast of 4.5 with #14161C");
  }
  for (const f of ["look", "entrance", "idlePose", "happyReaction", "sadReaction", "cardHold", "victory"]) {
    if (typeof c[f] !== "string" || !c[f].trim()) err("characters.json", c.id, f + " missing");
  }
});

const cfg = load("config.json");
// Every key in the table of docs/03_rules.md must exist.
const REQUIRED = [
  "configVersion", "timers.intro", "timers.topicVote", "timers.questionShow", "timers.answer.multipleChoice",
  "timers.answer.closestValue", "timers.lastCall", "timers.reveal", "timers.scores", "timers.roundEnd",
  "timers.finalIntro", "timers.results", "rounds.count", "rounds.questionsPerRound", "rounds.multipliers",
  "rounds.difficultyOrder", "rounds.formatOrder", "topics.doors", "topics.minUnseenToOffer", "topics.mixedTopicId",
  "scoring.maxPoints", "scoring.speedPenalty", "scoring.roundTo", "scoring.fastAnswerMs", "closestValue.closestBonus",
  "closestValue.minPoints", "closestValue.exactShare",
  "powerUps.enabled", "powerUps.types", "powerUps.targeting", "powerUps.drawChanceLeader", "powerUps.maxHeld",
  "powerUps.maxHitsPerQuestion", "powerUps.immunityQuestions", "powerUps.onPhone", "powerUps.freeze.taps",
  "powerUps.slime.blobs", "powerUps.fog.seconds", "race.maxHeadStart", "race.finishSteps", "race.maxQuestions",
  "race.stepsPerCorrect", "race.answer", "race.reveal", "players.min", "players.max", "players.timerMultiplierOptions",
  "players.seatHoldSeconds", "lobby.startCountdownSeconds", "net.inputGraceMs", "net.tvReconnectSeconds",
  "hosts.minGapSeconds", "hosts.bubbleSeconds", "hosts.streakTrigger", "poll.enabled", "poll.maxSeconds", "feedback.enabled", "feedback.stars", "feedback.items", "music.volume", "music.fadeSeconds", "sfx.volume", "sfx.hostVoice", "timers.lastCallEnabled",
  "staging.extraSeconds.questionShow", "staging.extraSeconds.reveal", "staging.extraSeconds.scores",
  "staging.extraSeconds.roundEnd", "staging.lobby.seatPopSeconds", "staging.intro.establishSeconds",
  "staging.intro.lightsUpSeconds", "staging.questionShow.curtainOpenSeconds", "staging.questionShow.questionInSeconds",
  "staging.answer.suspenseRiseSeconds", "staging.reveal.cardsUpSeconds", "staging.reveal.reactionSeconds",
  "staging.scores.overlayInSeconds", "staging.results.winnerWalkSeconds",
];
for (const key of REQUIRED) {
  if (key.split(".").reduce((o, k) => (o == null ? undefined : o[k]), cfg) === undefined) err("config.json", key, "missing key");
}
if (!topics.some((t) => t.id === cfg.topics?.mixedTopicId && t.mixed)) err("config.json", "topics.mixedTopicId", "must name the mixed topic");

// Staging (docs/21_staging.md): extras are 0 or more, and every staging animation fits inside its phase
// (the phase timer plus its staging extra).
const st = cfg.staging ?? {};
const tm = cfg.timers ?? {};
const ex = st.extraSeconds ?? {};
const fit = (ok, key, msg) => { if (!ok) err("config.json", key, msg); };
for (const [k, v] of Object.entries(ex)) fit(typeof v === "number" && v >= 0, "staging.extraSeconds." + k, "must be 0 or more");
fit(st.questionShow?.curtainOpenSeconds <= tm.questionShow + ex.questionShow, "staging.questionShow.curtainOpenSeconds", "must fit inside QuestionShow");
fit(st.questionShow?.questionInSeconds < tm.questionShow + ex.questionShow, "staging.questionShow.questionInSeconds", "must be shorter than QuestionShow");
fit(st.reveal?.cardsUpSeconds + st.reveal?.reactionSeconds <= tm.reveal + ex.reveal, "staging.reveal", "cards up plus reactions must fit inside Reveal");
fit(st.scores?.overlayInSeconds < tm.scores + ex.scores, "staging.scores.overlayInSeconds", "must be shorter than Scores");
fit(st.answer?.suspenseRiseSeconds <= Math.min(tm.answer?.multipleChoice, tm.answer?.closestValue), "staging.answer.suspenseRiseSeconds", "must fit inside the shortest answer window");
fit(st.intro?.establishSeconds < tm.intro, "staging.intro.establishSeconds", "must be shorter than the Intro");
fit(st.intro?.lightsUpSeconds >= 0 && st.intro?.lightsUpSeconds <= st.intro?.establishSeconds, "staging.intro.lightsUpSeconds", "must fit inside the establishing shot");
fit(st.results?.winnerWalkSeconds < tm.results, "staging.results.winnerWalkSeconds", "must be shorter than Results");

// lastPlaceCanWin (docs/02_show_flow.md): with the largest head start gap (leader maxHeadStart, last place 0),
// (leader head start - last place head start) < maxQuestions and (finishSteps - last place head start) <= maxQuestions.
const r = cfg.race ?? {};
if (!(r.maxHeadStart - 0 < r.maxQuestions)) err("config.json", "race", "last place could not win: maxHeadStart must be less than maxQuestions");
if (!(r.finishSteps - 0 <= r.maxQuestions)) err("config.json", "race", "last place could not win: finishSteps must be at most maxQuestions");

// UI strings (docs/15_schemas.md, D65): dotted lower camel case keys, both languages non empty, same placeholders.
// show_scripts.json (docs/15_schemas.md): known segments; standard has rounds.count rounds and one results.
const SEGMENTS = ["intro", "round", "finalIntro", "finalRace", "results", "poll", "miniGame"];
const scripts = load("show_scripts.json") ?? {};
for (const [mode, segments] of Object.entries(scripts)) {
  if (!Array.isArray(segments) || segments.length === 0) { err("show_scripts.json", mode, "must be a non empty list of segment names"); continue; }
  for (const s of segments) if (!SEGMENTS.includes(s)) err("show_scripts.json", mode, `unknown segment "${s}"`);
}
if (!Array.isArray(scripts.standard)) err("show_scripts.json", "standard", "the standard mode is missing");
else {
  const rounds = scripts.standard.filter((s) => s === "round").length;
  if (rounds !== cfg.rounds?.count) err("show_scripts.json", "standard", `has ${rounds} rounds, must match rounds.count`);
  if (scripts.standard.filter((s) => s === "results").length !== 1) err("show_scripts.json", "standard", "must hold exactly one results");
}

// One Standard game without a repeat: every slot kind rounds.count times, and race.maxQuestions statements.
const slotNeeds = {};
(cfg.rounds?.formatOrder ?? []).forEach((f, i) => {
  const key = f + " " + cfg.rounds.difficultyOrder[i];
  slotNeeds[key] = (slotNeeds[key] ?? 0) + cfg.rounds.count;
});
for (const [key, need] of Object.entries(slotNeeds)) {
  const [format, difficulty] = key.split(" ");
  const have = questions.filter((q) => q.format === format && q.difficulty === difficulty).length;
  if (have < need) err("questions.json", "-", `needs at least ${need} ${format} questions of difficulty ${difficulty} for one game, has ${have}`);
}
if (race.length < (cfg.race?.maxQuestions ?? 0)) err("race.json", "-", "needs at least race.maxQuestions statements for one race");

const ui = load("ui_strings.json");
const holes = (s) => (s.match(/\{[a-zA-Z]+\}/g) ?? []).sort().join(",");
if (!ui || typeof ui !== "object" || Array.isArray(ui)) err("ui_strings.json", "-", "must be an object from key to {ru, en}");
else for (const [k, v] of Object.entries(ui)) {
  if (!/^(tv|screen)(\.[a-z][a-zA-Z0-9]*)+$/.test(k)) err("ui_strings.json", k, "key must start with tv. or screen. and use dotted lower camel case");
  if (!isLoc(v) || !v.ru.trim() || !v.en.trim()) err("ui_strings.json", k, "must have non empty ru and en");
  else if (holes(v.ru) !== holes(v.en)) err("ui_strings.json", k, "placeholders differ between ru and en");
}
// Question feedback (decisions/0018, D110): every rating item has its row texts.
for (const item of cfg.feedback?.items ?? []) {
  for (const part of ["text", "low", "high"]) {
    if (!ui?.[`screen.rate.${item}.${part}`]) err("ui_strings.json", `screen.rate.${item}.${part}`, "missing: feedback.items lists this item");
  }
}

// Coverage report for game night 1
const perTopic = {};
for (const q of questions) {
  perTopic[q.topic] ??= { multipleChoice: 0, closestValue: 0 };
  perTopic[q.topic][q.format]++;
}
console.log("Questions per topic:", JSON.stringify(perTopic));
console.log(`${questions.length} questions, ${race.length} race statements, ${facts.length} facts, ${lines.length} host lines, ${characters.length} characters, ${Object.keys(ui ?? {}).length} UI strings`);

if (errors.length) {
  console.error(`\n${errors.length} error(s):\n` + errors.join("\n"));
  process.exit(1);
}
console.log("Content OK");
