// Sums the question ratings of one or more game night logs (decisions/0018, D109 to D111; docs/15_schemas.md,
// "questionRated") and prints a Markdown report: how each question, topic, difficulty and format played with the
// family, and how the ratings compare with the editorial audit (docs/question_audit/).
// Usage: node tools/analyze_ratings.mjs <events.jsonl | folder>... [--json] [--min <raters>] [--out <file.md>]
//
// - Inputs: event log files, or folders searched recursively for events_*.jsonl.
// - A rating is one questionRated line. Players may change a row until Scores ends, so only the latest line per
//   session, player, question and item counts.
// - Items: fun ("Did you like the question?"), guess ("Could you work it out without knowing?"), fact ("Was the answer
//   interesting?"), each 1 to 5. Unknown items are kept and shown too.
// - correct is the player's own answer to that question (true, false, null when they sent none); the report splits
//   fun by it, so a question that only pleases the players who got it right shows up.
// - Questions and the audit are read from content/questions.json and docs/question_audit/sample_audit.json next to
//   this script, when present. The audit covers a 600 question sample, so only part of the rated questions join it.
// - --min: the fewest raters a question needs to appear in the best and worst lists (default 2).
// - Banks (decisions/0020): Nov 6, 2026 lines carry bank (full or curated). Lines without it (before decision 0020, or
//   after D130 made the refined questions the only bank) count by their question id: cur_... is curated, else full. The
//   report compares the banks overall, per topic that both banks played, and per pair of questions: a curated question
//   (content/questions_refined.json) next to the full bank question it is based on (refine same: the identical
//   question; rewrite: the same fact asked better).

import { readFileSync, readdirSync, statSync, existsSync, writeFileSync } from "node:fs";
import { join, dirname, basename } from "node:path";
import { fileURLToPath } from "node:url";

export const ITEMS = ["fun", "guess", "fact"];

/** The bank of a line without a bank field: the refined (curated) questions have cur_ ids (D130). */
export const bankOf = (questionId) => (questionId.startsWith("cur_") ? "curated" : "full");

/** Reads every questionRated line of the given texts ({session, text}) and keeps the latest per player, question and item. */
export function collectRatings(logs) {
  const latest = new Map();
  let lines = 0;
  let skipped = 0;
  for (const { session, text } of logs) {
    for (const raw of text.split(/\r?\n/)) {
      if (!raw.trim()) continue;
      let line;
      try {
        line = JSON.parse(raw);
      } catch {
        skipped++;
        continue;
      }
      if (!line || line.event !== "questionRated") continue;
      if (typeof line.questionId !== "string" || typeof line.item !== "string" || !Number.isInteger(line.stars)) {
        skipped++;
        continue;
      }
      lines++;
      const s = line.session ?? session;
      latest.set(`${s}\n${line.playerId}\n${line.questionId}\n${line.item}`, { ...line, bank: line.bank ?? bankOf(line.questionId), session: s });
    }
  }
  return { ratings: [...latest.values()], lines, skipped };
}

const mean = (xs) => (xs.length ? xs.reduce((a, b) => a + b, 0) / xs.length : null);

function pearson(xs, ys) {
  const n = xs.length;
  if (n < 3) return null;
  const mx = mean(xs);
  const my = mean(ys);
  let sxy = 0;
  let sxx = 0;
  let syy = 0;
  for (let i = 0; i < n; i++) {
    sxy += (xs[i] - mx) * (ys[i] - my);
    sxx += (xs[i] - mx) ** 2;
    syy += (ys[i] - my) ** 2;
  }
  return sxx && syy ? sxy / Math.sqrt(sxx * syy) : null;
}

/** Groups ratings by a key and gives per item the mean and count, plus raters and the share of raters who were right. */
export function summarize(ratings, keyOf) {
  const groups = new Map();
  for (const r of ratings) {
    const key = keyOf(r);
    if (!groups.has(key)) groups.set(key, { key, items: {}, raters: new Map() });
    const g = groups.get(key);
    (g.items[r.item] ??= []).push(r.stars);
    g.raters.set(`${r.session}\n${r.playerId}\n${r.questionId}`, r.correct);
  }
  return [...groups.values()].map((g) => {
    const answered = [...g.raters.values()].filter((c) => c === true || c === false);
    const items = {};
    for (const [item, xs] of Object.entries(g.items)) items[item] = { mean: mean(xs), n: xs.length };
    return {
      key: g.key,
      raters: g.raters.size,
      correctShare: answered.length ? answered.filter((c) => c).length / answered.length : null,
      items,
    };
  });
}

/**
 * The bank comparison (decisions/0020): per bank, per topic both banks played, and per curated question next to the full
 * bank question it is based on. curated maps a curated id to {basedOn, refine}.
 */
export function compareBanks(ratings, curated = new Map()) {
  const perBank = summarize(ratings, (r) => r.bank ?? "full");
  const byTopicBank = summarize(ratings, (r) => `${r.topic}\n${r.bank ?? "full"}`);
  const topics = new Map();
  for (const g of byTopicBank) {
    const [topic, bank] = g.key.split("\n");
    if (!topics.has(topic)) topics.set(topic, {});
    topics.get(topic)[bank] = g;
  }
  const sharedTopics = [...topics.entries()].filter(([, b]) => b.full && b.curated).map(([topic, b]) => ({ topic, full: b.full, curated: b.curated }));
  const perQuestion = new Map(summarize(ratings, (r) => r.questionId).map((q) => [q.key, q]));
  const pairs = [];
  for (const [id, meta] of curated) {
    if (!meta.basedOn) continue;
    const c = perQuestion.get(id);
    const f = perQuestion.get(meta.basedOn);
    if (c && f) pairs.push({ curated: id, full: meta.basedOn, refine: meta.refine, curatedStats: c, fullStats: f });
  }
  return { perBank, sharedTopics, pairs };
}

/** Everything the report shows, as data (--json prints it). */
export function analyze(ratings, { questions = new Map(), audit = new Map(), min = 2, curated = new Map() } = {}) {
  const items = [...new Set([...ITEMS, ...ratings.map((r) => r.item)])];
  const perQuestion = summarize(ratings, (r) => r.questionId).map((q) => {
    const meta = questions.get(q.key);
    const a = audit.get(q.key);
    return {
      ...q,
      topic: meta?.topic ?? ratings.find((r) => r.questionId === q.key)?.topic,
      difficulty: meta?.difficulty,
      format: meta?.format,
      text: meta?.text,
      auditScore: a?.score ?? null,
      auditAction: a?.action ?? null,
    };
  });
  const funOf = (q) => q.items.fun?.mean ?? null;
  const ranked = perQuestion.filter((q) => q.raters >= min && funOf(q) != null).sort((a, b) => funOf(b) - funOf(a));
  const joined = perQuestion.filter((q) => q.auditScore != null && funOf(q) != null);
  const byCorrect = {};
  for (const [label, test] of [["right", (c) => c === true], ["wrong", (c) => c === false], ["no answer", (c) => c == null]]) {
    const rs = ratings.filter((r) => test(r.correct));
    byCorrect[label] = Object.fromEntries(items.map((i) => {
      const xs = rs.filter((r) => r.item === i).map((r) => r.stars);
      return [i, { mean: mean(xs), n: xs.length }];
    }));
  }
  const itemCorrelation = {};
  for (const [a, b] of [["fun", "guess"], ["fun", "fact"], ["guess", "fact"]]) {
    const both = perQuestion.filter((q) => q.items[a] && q.items[b]);
    itemCorrelation[`${a}~${b}`] = pearson(both.map((q) => q.items[a].mean), both.map((q) => q.items[b].mean));
  }
  return {
    items,
    totals: {
      ratings: ratings.length,
      questions: perQuestion.length,
      players: new Set(ratings.map((r) => `${r.session}\n${r.playerId}`)).size,
      sessions: new Set(ratings.map((r) => r.session)).size,
    },
    perQuestion,
    perTopic: summarize(ratings, (r) => r.topic ?? "?"),
    perDifficulty: summarize(ratings, (r) => r.difficulty ?? "?"),
    perFormat: summarize(ratings, (r) => r.format ?? "?"),
    byCorrect,
    itemCorrelation,
    best: ranked.slice(0, 10),
    worst: ranked.slice(-10).reverse(),
    banks: compareBanks(ratings, curated),
    audit: {
      joined: joined.length,
      funVsAudit: pearson(joined.map((q) => q.auditScore), joined.map(funOf)),
      replaceButLiked: joined.filter((q) => q.auditAction === "REPLACE" && funOf(q) >= 4).map((q) => q.key),
      keepButDisliked: joined.filter((q) => q.auditAction === "KEEP" && funOf(q) <= 2).map((q) => q.key),
    },
  };
}

const f1 = (x) => (x == null ? "–" : x.toFixed(1));
const f2 = (x) => (x == null ? "–" : x.toFixed(2));
const pct = (x) => (x == null ? "–" : Math.round(x * 100) + " %");

function table(rows, items, first) {
  const head = `| ${first} | Raters | Right | ${items.map((i) => i).join(" | ")} |\n| --- | --- | --- | ${items.map(() => "---").join(" | ")} |`;
  const body = rows
    .map((r) => `| ${r.label ?? r.key} | ${r.raters} | ${pct(r.correctShare)} | ${items.map((i) => (r.items[i] ? `${f1(r.items[i].mean)} (${r.items[i].n})` : "–")).join(" | ")} |`)
    .join("\n");
  return `${head}\n${body}`;
}

/** The Markdown report. */
export function report(result) {
  const { items, totals } = result;
  const label = (q) => `${q.key}${q.text ? `: ${q.text}` : ""}`;
  const out = [];
  out.push("# Question ratings");
  out.push("");
  out.push(`${totals.ratings} ratings from ${totals.players} players in ${totals.sessions} session(s), ${totals.questions} questions rated. ` +
    "Items: fun (did you like it), guess (could you work it out without knowing), fact (was the answer interesting); 1 to 5 stars, mean (count). " +
    "Right: share of raters who answered the question correctly.");
  out.push("");
  if (!totals.ratings) {
    out.push("No questionRated lines found. Was question feedback switched on in the TV settings panel (Tab)?");
    return out.join("\n");
  }
  const b = result.banks;
  if (b.perBank.length > 1 || b.perBank.some((g) => g.key === "curated")) {
    out.push("## Full bank against curated bank");
    out.push("");
    out.push(table(b.perBank, items, "Bank"));
    out.push("");
    if (b.sharedTopics.length) {
      out.push("Topics both banks played:");
      out.push("");
      out.push(table(b.sharedTopics.flatMap((t) => [{ ...t.full, label: `${t.topic} (full)` }, { ...t.curated, label: `${t.topic} (curated)` }]), items, "Topic"));
      out.push("");
    }
    if (b.pairs.length) {
      out.push("The same fact in both banks (same: the identical question; rewrite: the same fact asked better):");
      out.push("");
      out.push(table(b.pairs.flatMap((p) => [{ ...p.fullStats, label: `${p.full} (full)` }, { ...p.curatedStats, label: `${p.curated} (${p.refine})` }]), items, "Question"));
      out.push("");
    } else {
      out.push("No question was rated in both banks yet (a curated question and its full bank question never come to the same family twice; they meet across families or after Reset seen).");
      out.push("");
    }
  }
  out.push("## By difficulty and format");
  out.push("");
  out.push(table(result.perDifficulty, items, "Difficulty"));
  out.push("");
  out.push(table(result.perFormat, items, "Format"));
  out.push("");
  out.push("## Fun by the player's own answer");
  out.push("");
  out.push(`| Answer | ${items.join(" | ")} |\n| --- | ${items.map(() => "---").join(" | ")} |`);
  for (const [k, v] of Object.entries(result.byCorrect)) out.push(`| ${k} | ${items.map((i) => `${f1(v[i].mean)} (${v[i].n})`).join(" | ")} |`);
  out.push("");
  out.push(`How the items move together per question (Pearson r): ${Object.entries(result.itemCorrelation).map(([k, v]) => `${k} ${f2(v)}`).join(", ")}.`);
  out.push("");
  out.push("## By topic");
  out.push("");
  out.push(table([...result.perTopic].sort((a, b) => (b.items.fun?.mean ?? 0) - (a.items.fun?.mean ?? 0)), items, "Topic"));
  out.push("");
  out.push("## Most liked questions (fun)");
  out.push("");
  out.push(table(result.best.map((q) => ({ ...q, label: label(q) })), items, "Question"));
  out.push("");
  out.push("## Least liked questions (fun)");
  out.push("");
  out.push(table(result.worst.map((q) => ({ ...q, label: label(q) })), items, "Question"));
  out.push("");
  out.push("## Against the editorial audit");
  out.push("");
  const a = result.audit;
  out.push(`${a.joined} rated questions were in the audit sample. Correlation of the audit score with fun: ${f2(a.funVsAudit)}` +
    (a.joined < 10 ? " (too few questions to read much into it)." : "."));
  if (a.replaceButLiked.length) out.push(`- The audit said REPLACE, the family rated fun 4 or more: ${a.replaceButLiked.join(", ")}`);
  if (a.keepButDisliked.length) out.push(`- The audit said KEEP, the family rated fun 2 or less: ${a.keepButDisliked.join(", ")}`);
  out.push("");
  out.push("## Every rated question");
  out.push("");
  out.push(table([...result.perQuestion].sort((x, y) => x.key.localeCompare(y.key)).map((q) => ({
    ...q, label: `${q.key} (${q.difficulty ?? "?"}${q.auditScore != null ? `, audit ${q.auditScore}` : ""})`,
  })), items, "Question"));
  return out.join("\n");
}

function logFiles(path) {
  if (statSync(path).isDirectory()) {
    return readdirSync(path).flatMap((name) => {
      const full = join(path, name);
      if (statSync(full).isDirectory()) return logFiles(full);
      return /^events_.*\.jsonl$/.test(name) ? [full] : [];
    });
  }
  return [path];
}

function loadJson(path) {
  return existsSync(path) ? JSON.parse(readFileSync(path, "utf8")) : null;
}

function main(argv) {
  const args = [...argv];
  const json = args.includes("--json");
  let min = 2;
  let outFile = null;
  const paths = [];
  for (let i = 0; i < args.length; i++) {
    if (args[i] === "--json") continue;
    if (args[i] === "--min") min = Number(args[++i]);
    else if (args[i] === "--out") outFile = args[++i];
    else paths.push(args[i]);
  }
  if (!paths.length) {
    console.error("Usage: node tools/analyze_ratings.mjs <events.jsonl | folder>... [--json] [--min <raters>] [--out <file.md>]");
    process.exit(2);
  }
  const logs = paths.flatMap(logFiles).map((file) => ({ session: basename(file), text: readFileSync(file, "utf8") }));
  const { ratings, skipped } = collectRatings(logs);
  const root = join(dirname(fileURLToPath(import.meta.url)), "..");
  const questions = new Map((loadJson(join(root, "content", "questions.json")) ?? []).map((q) => [q.id, { ...q, text: q.text?.en }]));
  const audit = new Map((loadJson(join(root, "docs", "question_audit", "sample_audit.json")) ?? []).map((r) => [r.id, r]));
  const refinedList = loadJson(join(root, "content", "questions_refined.json")) ?? [];
  for (const q of refinedList) questions.set(q.id, { ...q, text: q.text?.en });
  const curated = new Map(refinedList.map((q) => [q.id, { basedOn: q.basedOn, refine: q.refine }]));
  const result = analyze(ratings, { questions, audit, min, curated });
  const text = json ? JSON.stringify(result, null, 1) : report(result);
  if (outFile) writeFileSync(outFile, text + "\n");
  else console.log(text);
  if (skipped) console.error(`${skipped} unreadable lines skipped`);
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) main(process.argv.slice(2));
