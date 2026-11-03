// Reads a playtest event log (docs/15_schemas.md, "Event log (telemetry)") and prints the derived numbers of each
// game in it (docs/15_schemas.md, "Derived numbers"; docs/20_playtest.md, "After the game night").
// Usage: node tools/analyze_log.mjs <events.jsonl> [--json]
//
// How the numbers are reconstructed from the log, and every approximation:
//
// - Lines: one JSON object per line. Blank lines are ignored. A line that does not parse, or is not an object with a
//   string `event` and a numeric `t`, is skipped and counted (with its line number).
// - Games: a game runs from `gameStarted` to the first `gameEnded` after it (its game time is gameStarted.t to
//   gameEnded.t, the same window as the live IdleMeter). Lines after `gameEnded` up to the next `gameStarted` still
//   belong to that game for the Poll phase, its `poll` lines and phase durations (the poll comes after Results).
//   A game with no `gameEnded` (cut off log) ends at its last line and is marked incomplete. `gameEnded.reason`
//   `backToLobby` marks the game abandoned.
// - Lines inside a game window are stable sorted by `t` before the idle timeline is built (the core writes them in
//   order; sorting only guards against out of order lines). A line before gameStarted.t counts at gameStarted.t, one
//   after gameEnded.t at gameEnded.t.
// - Players: the game's players are `gameStarted.players`. Names come from every `playerJoined` in the file (the
//   last name of an id wins); an id without one shows as the id. Connection state is followed over the whole file,
//   so a phone that disconnected in the Lobby before a game is still disconnected at its start: `playerJoined` and
//   `playerRejoined` mean connected, `playerLeft` reason `disconnected` means not connected (seat held), `left` and
//   `seatExpired` mean gone. A game player never seen before counts as connected.
// - Idle (docs/15_schemas.md, "Idle"): a moment is idle when no connected player can send an answer, vote, power up
//   pick or poll input. Mirrors Game.AnyoneCanAnswer and DevTools/IdleMeter: the state is taken after every line at
//   one `t`, and the time up to the next `t` counts with that state.
//   - TopicVote is active while a connected player has no `input` kind `vote` for its phaseId.
//   - Answer and RaceQuestion are active while a connected player has no `input` of an answer kind (`choice` or
//     `value`; `trueFalse`) for its phaseId. Other input kinds (`powerUp`, `effectCleared`) and `inputRefused` do
//     not count. APPROXIMATION: personal deadlines (timer multipliers, last call, net.inputGraceMs) are not in the
//     log, so a player who timed out still counts as able to answer until the phase ends; the live meter goes idle
//     for that player at deadline plus grace.
//   - Reveal and Scores (one phaseId) are active while a connected player holds a power up: held from
//     `powerUpDrawn` until that player's `powerUpUsed`, lost by everyone when the RoundEnd of the game's last round
//     starts, and at the game end. The pick window is hidden on the last question of the last round, so Reveal and
//     Scores of that question are idle. APPROXIMATION: the last round and its last question are inferred from the
//     log, not from content/config.json: the last round is the highest `round` on a phaseStarted before FinalIntro,
//     its last question the highest `slot` in that round. A game with no FinalIntro (abandoned earlier) has no last
//     round: nothing is lost at a RoundEnd and no pick window is hidden. A holder with no available target (all hit
//     or immune, or aimUp with nobody above) still counts as active: the targets are not in the log.
//   - Poll is active while fewer `poll` lines than connected players arrived in that phase (poll lines carry no
//     player id). The Poll phase normally comes after gameEnded, so it is outside the game time.
//   - Every other phase (Intro, QuestionShow, RoundEnd, FinalIntro, RaceReveal, Results), the moment between a
//     phaseEnded and the next phaseStarted, and the time before the first phaseStarted are idle.
// - Longest idle stretch: like IdleMeter, Intro, FinalIntro and Results count as idle in the idle share but are left
//   out of the stretch and break it (a stretch never joins across them). Compared with the starting limit of 11 s
//   (docs/19_acceptance.md, D56); OVER only when above it. A live log's `t` is the frame the line was written in, so
//   a phase can last a few ms longer by `t` than its `durationMs` (RoundEnd 4004 ms for 4000): the current timers
//   then read a few ms OVER the 11 s limit. The amount is printed; no tolerance is applied.
// - Phase durations: every phase that started in the game's lines except Lobby and StartCountdown (those belong to
//   the time between games), matched to its phaseEnded by phase name and phaseId. Budget and staging extra come from
//   phaseStarted (Answer budgets are the base window before timer multipliers). A phase with no phaseEnded is counted
//   under `open` and left out of min/avg/max.
// - Correct rate per question: each Answer phaseStarted with a questionId, its `input` lines of kind `choice` or
//   `value` (one per player, the first wins), correct = `correct: true`, rate = correct / players in the game. Race
//   statements the same from RaceQuestion and `trueFalse`.
// - Polls: answers that are numbers are choice questions (counted per option index), strings are text questions
//   (listed). The question kinds are not read from content/poll.json.
// - Host lines: counted per trigger and per host inside the game window; lines outside every game window (the Lobby
//   greetings) are counted under `outside`.
import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { fileURLToPath } from "node:url";

/** The starting idle limit in ms (docs/19_acceptance.md, D42, D56). */
export const IDLE_LIMIT_MS = 11000;

/** Phases that count as idle but are left out of the longest stretch and break it (IdleMeter). */
export const STRETCH_EXCLUDED = new Set(["Intro", "FinalIntro", "Results"]);

/** Phases of the time between games, left out of the phase durations. */
const BETWEEN_GAMES = new Set(["Lobby", "StartCountdown"]);

const ANSWER_KINDS = { TopicVote: ["vote"], Answer: ["choice", "value"], RaceQuestion: ["trueFalse"] };

/** Splits the text into events. Returns { events, skipped: [{line, text}] }; each event gets `_line` (1 based). */
export function parseLines(text) {
  const events = [];
  const skipped = [];
  const lines = text.split(/\r?\n/);
  lines.forEach((raw, i) => {
    const line = raw.trim();
    if (line === "") return;
    let obj = null;
    try {
      obj = JSON.parse(line);
    } catch {
      obj = null;
    }
    if (obj === null || typeof obj !== "object" || Array.isArray(obj) || typeof obj.event !== "string" || typeof obj.t !== "number") {
      skipped.push({ line: i + 1, text: line.length > 120 ? line.slice(0, 117) + "..." : line });
      return;
    }
    obj._line = i + 1;
    events.push(obj);
  });
  return { events, skipped };
}

/** Connection state of every player id over the file: "connected", "disconnected" or "gone". */
function applyConnection(status, e) {
  switch (e.event) {
    case "playerJoined":
    case "playerRejoined":
      status.set(e.playerId, "connected");
      break;
    case "playerLeft":
      status.set(e.playerId, e.reason === "disconnected" ? "disconnected" : "gone");
      break;
  }
}

/**
 * Splits the events into games. Each game: { start (the gameStarted line), lines (gameStarted up to the next
 * gameStarted), statusAtStart (Map id to connection state just before gameStarted) }. Also returns `names` (Map id to
 * name over the whole file) and `outside` (lines before the first game).
 */
export function splitGames(events) {
  const names = new Map();
  const status = new Map();
  const games = [];
  const outside = [];
  let current = null;
  for (const e of events) {
    if (e.event === "gameStarted") {
      current = { start: e, lines: [], statusAtStart: new Map(status) };
      games.push(current);
    }
    if (current) current.lines.push(e);
    else outside.push(e);
    if (e.event === "playerJoined" && typeof e.name === "string") names.set(e.playerId, e.name);
    applyConnection(status, e);
  }
  return { games, names, outside };
}

function stats(values) {
  if (values.length === 0) return { min: null, avg: null, max: null };
  const sum = values.reduce((a, b) => a + b, 0);
  return { min: Math.min(...values), avg: sum / values.length, max: Math.max(...values) };
}

const inc = (obj, key, by = 1) => { obj[key] = (obj[key] ?? 0) + by; };

/** The idle share and longest stretch of one game window (lines already cut to the window). */
export function idleTimeline(windowLines, { startT, endT, players, statusAtStart }) {
  const sorted = windowLines.map((e, i) => ({ e, i, t: Math.min(endT, Math.max(startT, e.t)) }))
    .sort((a, b) => a.t - b.t || a.i - b.i);

  // The last round and its last question (see the comment at the top).
  let lastRound = null;
  const finalIntro = sorted.find((x) => x.e.event === "phaseStarted" && x.e.phase === "FinalIntro");
  if (finalIntro) {
    for (const x of sorted) {
      if (x === finalIntro) break;
      if (x.e.event === "phaseStarted" && typeof x.e.round === "number") lastRound = Math.max(lastRound ?? 0, x.e.round);
    }
  }
  let lastSlot = null;
  if (lastRound !== null) {
    for (const x of sorted) {
      if (x.e.event === "phaseStarted" && x.e.round === lastRound && typeof x.e.slot === "number") lastSlot = Math.max(lastSlot ?? 0, x.e.slot);
    }
  }

  const inGame = new Set(players);
  const status = new Map();
  for (const id of players) status.set(id, statusAtStart.get(id) === undefined ? "connected" : statusAtStart.get(id));
  const st = { phase: null, phaseId: null, round: null, slot: null, inputs: new Map(), held: new Map(), polls: 0 };

  const apply = (e) => {
    switch (e.event) {
      case "playerJoined":
      case "playerRejoined":
      case "playerLeft":
        if (inGame.has(e.playerId)) applyConnection(status, e);
        break;
      case "phaseStarted":
        st.phase = e.phase;
        st.phaseId = e.phaseId;
        st.round = typeof e.round === "number" ? e.round : null;
        st.slot = typeof e.slot === "number" ? e.slot : null;
        if (e.phase === "RoundEnd" && lastRound !== null && e.round === lastRound) st.held.clear();
        if (e.phase === "Poll") st.polls = 0;
        break;
      case "phaseEnded":
        if (e.phase === st.phase && e.phaseId === st.phaseId) { st.phase = null; st.phaseId = null; }
        break;
      case "input": {
        if (!st.inputs.has(e.phaseId)) st.inputs.set(e.phaseId, []);
        st.inputs.get(e.phaseId).push(e);
        break;
      }
      case "powerUpDrawn":
        st.held.set(e.playerId, e.powerUp);
        break;
      case "powerUpUsed":
        st.held.delete(e.playerId);
        break;
      case "poll":
        st.polls++;
        break;
      case "gameEnded":
        st.held.clear();
        break;
    }
  };

  const connected = () => players.filter((id) => status.get(id) === "connected");
  const pickHidden = () => lastRound !== null && st.round === lastRound && st.slot === lastSlot;
  const active = () => {
    const kinds = ANSWER_KINDS[st.phase];
    if (kinds) {
      const done = new Set((st.inputs.get(st.phaseId) ?? []).filter((x) => kinds.includes(x.kind)).map((x) => x.playerId));
      return connected().some((id) => !done.has(id));
    }
    if (st.phase === "Reveal" || st.phase === "Scores") {
      return !pickHidden() && connected().some((id) => st.held.has(id));
    }
    if (st.phase === "Poll") return st.polls < connected().length;
    return false;
  };
  const countsForStretch = () => !STRETCH_EXCLUDED.has(st.phase);

  let gameMs = 0;
  let idleMs = 0;
  let current = 0;
  let currentStart = startT;
  let currentPhases = [];
  let longest = { ms: 0, startT: null, endT: null, phases: [] };
  const idleByPhase = {};

  // State at the start: every line at startT.
  let k = 0;
  while (k < sorted.length && sorted[k].t === startT) apply(sorted[k++].e);
  let prevT = startT;
  let prevIdle = !active();
  let prevCounts = countsForStretch();
  let prevPhase = st.phase;

  const advance = (t) => {
    const delta = Math.max(0, t - prevT);
    gameMs += delta;
    if (prevIdle && delta > 0) {
      idleMs += delta;
      inc(idleByPhase, prevPhase ?? "(between phases)", delta);
      if (prevCounts) {
        if (current === 0) { currentStart = prevT; currentPhases = []; }
        current += delta;
        if (currentPhases[currentPhases.length - 1] !== (prevPhase ?? "(between phases)")) currentPhases.push(prevPhase ?? "(between phases)");
        if (current > longest.ms) longest = { ms: current, startT: currentStart, endT: t, phases: [...currentPhases] };
      }
    }
    prevT = t;
  };

  while (k < sorted.length) {
    const t = sorted[k].t;
    advance(t);
    while (k < sorted.length && sorted[k].t === t) apply(sorted[k++].e);
    prevIdle = !active();
    prevCounts = countsForStretch();
    prevPhase = st.phase;
    if (!prevIdle || !prevCounts) current = 0;
  }
  advance(endT);

  return {
    gameMs,
    idleMs,
    idleShare: gameMs > 0 ? idleMs / gameMs : 0,
    idleByPhase,
    longestStretch: { ...longest, limitMs: IDLE_LIMIT_MS, over: longest.ms > IDLE_LIMIT_MS },
    lastRound,
    lastSlot,
  };
}

/** Every derived number of one game (from splitGames). */
export function analyzeGame(game, names) {
  const name = (id) => (id == null ? null : names.get(id) ?? id);
  const start = game.start;
  const players = Array.isArray(start.players) ? start.players : [];
  const endIndex = game.lines.findIndex((e) => e.event === "gameEnded");
  const ended = endIndex >= 0 ? game.lines[endIndex] : null;
  const windowLines = endIndex >= 0 ? game.lines.slice(0, endIndex + 1) : game.lines;
  const startT = start.t;
  const endT = ended ? ended.t : Math.max(startT, ...windowLines.map((e) => e.t));
  const status = ended ? (ended.reason === "backToLobby" ? "abandoned" : ended.reason ?? "finished") : "incomplete";

  const idle = idleTimeline(windowLines, { startT, endT, players, statusAtStart: game.statusAtStart });

  // Phase durations, reasons.
  const phases = {};
  const open = new Map();
  for (const e of game.lines) {
    if (e.event === "phaseStarted" && !BETWEEN_GAMES.has(e.phase)) {
      const p = (phases[e.phase] ??= { count: 0, durations: [], budgets: new Set(), stagingExtras: new Set(), reasons: {}, open: 0 });
      p.count++;
      if (typeof e.budgetMs === "number") p.budgets.add(e.budgetMs);
      if (typeof e.stagingExtraMs === "number") p.stagingExtras.add(e.stagingExtraMs);
      open.set(`${e.phase}#${e.phaseId}`, p);
    } else if (e.event === "phaseEnded") {
      const key = `${e.phase}#${e.phaseId}`;
      const p = open.get(key);
      if (!p) continue;
      open.delete(key);
      if (typeof e.durationMs === "number") p.durations.push(e.durationMs);
      inc(p.reasons, e.reason ?? "(none)");
    }
  }
  for (const p of open.values()) p.open++;
  const phaseDurations = {};
  for (const [phase, p] of Object.entries(phases)) {
    phaseDurations[phase] = {
      count: p.count,
      ...stats(p.durations),
      budgetsMs: [...p.budgets].sort((a, b) => a - b),
      stagingExtrasMs: [...p.stagingExtras].sort((a, b) => a - b),
      reasons: p.reasons,
      open: p.open,
    };
  }

  // Correct rate per question and race statement.
  const inputsByPhase = new Map();
  for (const e of windowLines) {
    if (e.event !== "input") continue;
    if (!inputsByPhase.has(e.phaseId)) inputsByPhase.set(e.phaseId, []);
    inputsByPhase.get(e.phaseId).push(e);
  }
  const rate = (e, kinds) => {
    const seen = new Set();
    let correct = 0;
    for (const x of inputsByPhase.get(e.phaseId) ?? []) {
      if (!kinds.includes(x.kind) || seen.has(x.playerId)) continue;
      seen.add(x.playerId);
      if (x.correct === true) correct++;
    }
    return {
      phaseId: e.phaseId,
      questionId: e.questionId,
      ...(e.round !== undefined ? { round: e.round } : {}),
      ...(e.slot !== undefined ? { slot: e.slot } : {}),
      answered: seen.size,
      correct,
      players: players.length,
      correctRate: players.length > 0 ? correct / players.length : 0,
    };
  };
  const questions = [];
  const race = [];
  for (const e of windowLines) {
    if (e.event !== "phaseStarted" || e.questionId == null) continue;
    if (e.phase === "Answer") questions.push(rate(e, ANSWER_KINDS.Answer));
    else if (e.phase === "RaceQuestion") race.push(rate(e, ANSWER_KINDS.RaceQuestion));
  }

  // Power ups, effects, targeting, host lines.
  const byPlayer = {};
  const byType = {};
  const effects = {};
  const targets = {};
  const hostLines = { byTrigger: {}, byHost: {}, total: 0 };
  for (const e of windowLines) {
    switch (e.event) {
      case "powerUpDrawn": {
        const p = (byPlayer[name(e.playerId)] ??= { drawn: {}, used: {} });
        inc(p.drawn, e.powerUp);
        inc((byType[e.powerUp] ??= { drawn: 0, used: 0 }), "drawn");
        break;
      }
      case "powerUpUsed": {
        const p = (byPlayer[name(e.playerId)] ??= { drawn: {}, used: {} });
        inc(p.used, e.powerUp);
        inc((byType[e.powerUp] ??= { drawn: 0, used: 0 }), "used");
        inc((targets[name(e.playerId)] ??= {}), name(e.target));
        break;
      }
      case "effectCleared": {
        const x = (effects[e.effect] ??= { count: 0, totalMs: 0 });
        x.count++;
        if (typeof e.ms === "number") x.totalMs += e.ms;
        break;
      }
      case "hostLine":
        hostLines.total++;
        inc(hostLines.byTrigger, e.trigger);
        inc(hostLines.byHost, e.host);
        break;
    }
  }
  const effectCleared = {};
  for (const [effect, x] of Object.entries(effects)) effectCleared[effect] = { count: x.count, avgMs: x.count > 0 ? x.totalMs / x.count : null };

  // Poll answers: every poll line of the game, normally after gameEnded.
  const polls = game.lines.filter((e) => e.event === "poll");
  const poll = summarizePolls(polls);

  return {
    game: start.game ?? null,
    seed: start.seed ?? null,
    mode: start.mode ?? null,
    status,
    abandoned: status === "abandoned",
    startT,
    endT,
    durationMs: ended?.durationMs ?? null,
    winner: name(ended?.winner),
    players: players.map((id) => ({ playerId: id, name: name(id) })),
    idle,
    phaseDurations,
    questions,
    race,
    powerUps: { byPlayer, byType, effectCleared },
    targets,
    hostLines,
    poll,
  };
}

/** Poll lines to { submissions, choice: {id: {index: count}}, text: {id: [answers]} }. */
export function summarizePolls(polls) {
  const choice = {};
  const text = {};
  for (const p of polls) {
    for (const [id, answer] of Object.entries(p.answers ?? {})) {
      if (typeof answer === "number") inc((choice[id] ??= {}), String(answer));
      else if (typeof answer === "string") (text[id] ??= []).push(answer);
    }
  }
  return { submissions: polls.length, choice, text };
}

/** The whole report of a log's text. */
export function analyze(text) {
  const { events, skipped } = parseLines(text);
  const { games, names, outside } = splitGames(events);
  const reports = games.map((g) => analyzeGame(g, names));
  // Host lines and polls outside every game window (before the first game, or after a gameEnded).
  const outsideHost = {};
  const outsidePolls = outside.filter((e) => e.event === "poll");
  const countHost = (e) => { if (e.event === "hostLine") inc(outsideHost, e.trigger); };
  outside.forEach(countHost);
  for (const g of games) {
    const endIndex = g.lines.findIndex((e) => e.event === "gameEnded");
    if (endIndex >= 0) g.lines.slice(endIndex + 1).forEach(countHost);
  }
  return {
    lines: events.length + skipped.length,
    skippedLines: skipped.length,
    skipped,
    games: reports,
    outside: { hostLinesByTrigger: outsideHost, pollsBeforeFirstGame: summarizePolls(outsidePolls) },
  };
}

const sec = (ms) => (ms == null ? "-" : `${(ms / 1000).toFixed(1)} s`);
const pct = (x) => `${(x * 100).toFixed(1)}%`;
const list = (values) => (values.length === 0 ? "-" : values.map((v) => sec(v)).join(" / "));
const counts = (obj) => Object.entries(obj).map(([k, v]) => `${k} ${v}`).join(", ") || "-";

/** The report as text lines. */
export function formatText(report, file = "") {
  const out = [];
  const w = (s = "") => out.push(s);
  w(`${file ? file + ": " : ""}${report.games.length} game(s), ${report.lines} line(s), ${report.skippedLines} unparsable line(s) skipped`);
  for (const s of report.skipped) w(`  skipped line ${s.line}: ${s.text}`);
  for (const [i, g] of report.games.entries()) {
    w();
    w(`=== Game ${g.game ?? i + 1} (${g.status.toUpperCase()}${g.abandoned ? ": Back to lobby" : ""}), seed ${g.seed ?? "-"}, mode ${g.mode ?? "-"} ===`);
    w(`Players (${g.players.length}): ${g.players.map((p) => p.name).join(", ")}${g.winner ? `; winner ${g.winner}` : ""}`);
    w(`Game time ${sec(g.endT - g.startT)} (t ${g.startT} to ${g.endT})`);
    const L = g.idle.longestStretch;
    w(`Idle share ${pct(g.idle.idleShare)} (${sec(g.idle.idleMs)} of ${sec(g.idle.gameMs)}); idle by phase: ${Object.entries(g.idle.idleByPhase).filter(([, v]) => v >= 50).map(([k, v]) => `${k} ${sec(v)}`).join(", ") || "-"}`);
    // Idle by phase leaves out entries under 50 ms (frame gaps between a phaseEnded and the next phase; --json has them).
    // Milliseconds here: a live log can be over the limit by a frame (11006 ms), which one decimal would hide.
    w(`Longest idle stretch outside Intro, FinalIntro, Results: ${L.ms} ms (limit ${L.limitMs} ms)${L.over ? ` OVER by ${L.ms - L.limitMs} ms` : ""}${L.startT != null ? `, t ${L.startT} to ${L.endT}: ${L.phases.join(" > ")}` : ""}`);
    w();
    w("Phase durations: count, min / avg / max, budget, staging extra, reasons");
    for (const [phase, p] of Object.entries(g.phaseDurations)) {
      w(`  ${phase.padEnd(13)} ${String(p.count).padStart(3)}  ${sec(p.min)} / ${sec(p.avg)} / ${sec(p.max)}  budget ${list(p.budgetsMs)}  staging ${list(p.stagingExtrasMs)}  ${counts(p.reasons)}${p.open ? `, open ${p.open}` : ""}`);
    }
    w();
    w("Correct rate per question (correct / players)");
    for (const q of g.questions) {
      w(`  R${q.round ?? "?"} Q${q.slot ?? "?"} ${String(q.questionId).padEnd(14)} answered ${q.answered}/${q.players}, correct ${q.correct}/${q.players} = ${pct(q.correctRate)}`);
    }
    if (g.race.length > 0) w("Race statements");
    for (const q of g.race) {
      w(`  ${String(q.questionId).padEnd(14)} answered ${q.answered}/${q.players}, correct ${q.correct}/${q.players} = ${pct(q.correctRate)}`);
    }
    w();
    w("Power ups drawn / used");
    for (const [type, x] of Object.entries(g.powerUps.byType)) w(`  ${type.padEnd(8)} drawn ${x.drawn}, used ${x.used}`);
    for (const [player, x] of Object.entries(g.powerUps.byPlayer)) w(`  ${player}: drawn ${counts(x.drawn)}; used ${counts(x.used)}`);
    if (Object.keys(g.powerUps.byType).length === 0) w("  none");
    for (const [effect, x] of Object.entries(g.powerUps.effectCleared)) w(`  ${effect} cleared ${x.count} time(s), average ${x.avgMs == null ? "-" : Math.round(x.avgMs) + " ms"}`);
    w("Who targeted whom");
    for (const [attacker, x] of Object.entries(g.targets)) w(`  ${attacker} -> ${counts(x)}`);
    if (Object.keys(g.targets).length === 0) w("  nobody");
    w(`Host lines (${g.hostLines.total}): ${counts(g.hostLines.byTrigger)}; by host: ${counts(g.hostLines.byHost)}`);
    w(`Poll: ${g.poll.submissions} submission(s)`);
    for (const [id, x] of Object.entries(g.poll.choice)) w(`  ${id}: ${Object.entries(x).map(([k, v]) => `option ${k}: ${v}`).join(", ")}`);
    for (const [id, x] of Object.entries(g.poll.text)) for (const a of x) w(`  ${id}: "${a}"`);
  }
  const oh = report.outside.hostLinesByTrigger;
  if (Object.keys(oh).length > 0) {
    w();
    w(`Host lines outside games: ${counts(oh)}`);
  }
  return out.join("\n");
}

function main(argv) {
  const json = argv.includes("--json");
  const files = argv.filter((a) => a !== "--json");
  if (files.length !== 1) {
    console.error("Usage: node tools/analyze_log.mjs <events.jsonl> [--json]");
    return 2;
  }
  let text;
  try {
    text = readFileSync(files[0], "utf8");
  } catch (e) {
    console.error(`Cannot read ${files[0]}: ${e.message}`);
    return 2;
  }
  const report = analyze(text);
  console.log(json ? JSON.stringify(report, null, 2) : formatText(report, files[0]));
  return 0;
}

const samePath = (a, b) => (process.platform === "win32" ? a.toLowerCase() === b.toLowerCase() : a === b);
if (process.argv[1] && samePath(fileURLToPath(import.meta.url), resolve(process.argv[1]))) {
  process.exitCode = main(process.argv.slice(2));
}
