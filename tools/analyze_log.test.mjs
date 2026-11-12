// Tests of tools/analyze_log.mjs against the hand-written logs in tools/fixtures/. Run: node --test tools/
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { analyze, formatText, parseLines, splitGames, idleTimeline, IDLE_LIMIT_MS } from "./analyze_log.mjs";

// The fixtures were written for the first idle limit of 11 s; the over and not over checks keep using it.
const FIXTURE_LIMIT = { idleLimitMs: 11000 };

const here = dirname(fileURLToPath(import.meta.url));
const fixture = (name) => readFileSync(join(here, "fixtures", name), "utf8");

test("a broken line is skipped and counted, a blank line is ignored", () => {
  const { events, skipped } = parseLines(fixture("one_game.jsonl"));
  assert.equal(skipped.length, 1);
  assert.equal(skipped[0].line, 13);
  assert.equal(events.length, 70);
  const report = analyze(fixture("one_game.jsonl"));
  assert.equal(report.skippedLines, 1);
  assert.equal(report.games.length, 1);
});

test("one game: idle share and longest stretch, exact", () => {
  const [g] = analyze(fixture("one_game.jsonl"), FIXTURE_LIMIT).games;
  // Active: TopicVote 4000-7000, Answer 11000-14000, Reveal while Mama holds slime 14000-16000 and after her rejoin
  // 18000-19000, Answer 25000-29000 (Kid back, Papa's effectCleared input is not an answer), race 48000-51000.
  // Idle: Intro 3000, QuestionShow 4000, 16000-18000 (the only holder disconnected) 2000, 19000-25000 6000,
  // 29000-48000 19000 (Reveal and Scores of the last question: pick window hidden though Kid holds fog; RoundEnd;
  // FinalIntro), RaceReveal 2000, Results 20000.
  assert.equal(g.idle.gameMs, 72000);
  assert.equal(g.idle.idleMs, 56000);
  assert.equal(g.idle.idleShare, 56000 / 72000);
  // 29000 to 40000: Reveal, Scores, RoundEnd; FinalIntro breaks it. Exactly at the limit is not over.
  assert.equal(g.idle.longestStretch.ms, 11000);
  assert.equal(g.idle.longestStretch.startT, 29000);
  assert.equal(g.idle.longestStretch.endT, 40000);
  assert.deepEqual(g.idle.longestStretch.phases, ["Reveal", "Scores", "RoundEnd"]);
  assert.equal(g.idle.longestStretch.over, false);
  assert.equal(g.idle.lastRound, 1);
  assert.equal(g.idle.lastSlot, 2);
  assert.equal(g.idle.idleByPhase.Results, 20000);
  assert.equal(g.idle.idleByPhase.Intro, 3000);
  assert.equal(g.idle.idleByPhase.Reveal, 1500 + 3500); // 16000-17500 (holder away), the last question
  assert.equal(g.idle.idleByPhase.Scores, 500 + 2000 + 3500);
});

test("one game: status, players, phase durations", () => {
  const [g] = analyze(fixture("one_game.jsonl")).games;
  assert.equal(g.status, "finished");
  assert.equal(g.abandoned, false);
  assert.equal(g.winner, "Mama");
  assert.deepEqual(g.players.map((p) => p.name), ["Mama", "Papa", "Kid"]);
  const a = g.phaseDurations.Answer;
  assert.equal(a.count, 2);
  assert.equal(a.min, 3000);
  assert.equal(a.avg, 3500);
  assert.equal(a.max, 4000);
  assert.deepEqual(a.budgetsMs, [15000, 20000]);
  assert.deepEqual(a.stagingExtrasMs, [0]);
  assert.deepEqual(a.reasons, { allDone: 2 });
  assert.deepEqual(g.phaseDurations.Intro.reasons, { skip: 1 });
  // The Poll after gameEnded belongs to this game; the Lobby after it does not.
  assert.equal(g.phaseDurations.Poll.count, 1);
  assert.equal(g.phaseDurations.Poll.max, 17000);
  assert.equal(g.phaseDurations.Lobby, undefined);
});

test("one game: correct rate per question and race statement", () => {
  const [g] = analyze(fixture("one_game.jsonl")).games;
  assert.deepEqual(g.questions.map((q) => [q.questionId, q.round, q.slot, q.answered, q.correct, q.players]), [
    ["geo_001", 1, 1, 2, 1, 3],
    ["geo_002", 1, 2, 3, 2, 3],
  ]);
  assert.equal(g.questions[0].correctRate, 1 / 3);
  assert.deepEqual(g.race.map((q) => [q.questionId, q.answered, q.correct]), [["r_007", 3, 2]]);
});

test("one game: power ups, targets, effects, host lines, poll", () => {
  const report = analyze(fixture("one_game.jsonl"));
  const [g] = report.games;
  assert.deepEqual(g.powerUps.byType, { slime: { drawn: 1, used: 1 }, fog: { drawn: 1, used: 0 } });
  assert.deepEqual(g.powerUps.byPlayer, { Mama: { drawn: { slime: 1 }, used: { slime: 1 } }, Kid: { drawn: { fog: 1 }, used: {} } });
  assert.deepEqual(g.powerUps.effectCleared, { slime: { count: 1, avgMs: 2650 } });
  assert.deepEqual(g.targets, { Mama: { Papa: 1 } });
  assert.deepEqual(g.hostLines.byTrigger, { gameStart: 2, newLeader: 1, powerUpUsed: 1 });
  assert.deepEqual(g.hostLines.byHost, { konstantin: 2, anna: 2 });
  assert.deepEqual(g.poll, { submissions: 3, choice: { waiting: { 1: 2, 0: 1 }, time: { 1: 2 } }, text: { fun: ["The slime", "x"] } });
  assert.deepEqual(report.outside.hostLinesByTrigger, { playerJoined: 1 });
});

test("two games in one file, the second abandoned, names and connections carried over", () => {
  const text = fixture("two_games.jsonl");
  const { games } = splitGames(parseLines(text).events);
  assert.equal(games.length, 2);
  assert.equal(games[1].statusAtStart.get("p2"), "disconnected", "Papa disconnected in the Lobby before game 2");
  assert.equal(games[1].statusAtStart.get("p3"), "gone");

  const report = analyze(text, FIXTURE_LIMIT);
  assert.equal(report.skippedLines, 0);
  const [g1, g2] = report.games;
  assert.equal(g1.game, 1);
  assert.equal(g1.status, "finished");
  // Idle: Intro 30000, QuestionShow 4000, 142000-161500 19500, RaceReveal 2000, Results 20000.
  assert.equal(g1.idle.gameMs, 85000);
  assert.equal(g1.idle.idleMs, 75500);
  // Reveal (with a 500 ms staging extra), Scores and RoundEnd: 142000 to 153500.
  assert.equal(g1.idle.longestStretch.ms, 11500);
  assert.equal(g1.idle.longestStretch.over, true);
  assert.deepEqual(g1.phaseDurations.Reveal.stagingExtrasMs, [500]);
  assert.deepEqual(g1.poll, { submissions: 2, choice: { again: { 0: 2 } }, text: { fun: ["race"] } });
  assert.equal(g1.phaseDurations.StartCountdown, undefined);

  assert.equal(g2.game, 2);
  assert.equal(g2.status, "abandoned");
  assert.equal(g2.abandoned, true);
  assert.equal(g2.winner, null);
  assert.deepEqual(g2.players.map((p) => p.name), ["Mama", "Papa", "Dyadya"]);
  // Game 2 has no Intro and no FinalIntro: Mama's freeze keeps the abandoned Reveal active; only QuestionShow is idle.
  assert.equal(g2.idle.gameMs, 10000);
  assert.equal(g2.idle.idleMs, 4000);
  assert.equal(g2.idle.idleShare, 0.4);
  assert.equal(g2.idle.longestStretch.ms, 4000);
  assert.equal(g2.idle.lastRound, null);
  assert.deepEqual(g2.phaseDurations.Reveal.reasons, { abandoned: 1 });
  assert.equal(g2.poll.submissions, 0);
  assert.deepEqual(report.outside.hostLinesByTrigger, { startCountdown: 1, playerJoined: 1 });

  const out = formatText(report);
  assert.match(out, /Game 2 \(ABANDONED: Back to lobby\)/);
  assert.match(out, /Longest idle stretch .*11500 ms \(limit 11000 ms\) OVER by 500 ms/);
})

test("the default idle limit is 21 s (D126): an 11.5 s stretch is not over", () => {
  assert.equal(IDLE_LIMIT_MS, 32000);
  const [g1] = analyze(fixture("two_games.jsonl")).games;
  assert.equal(g1.idle.longestStretch.limitMs, 32000);
  assert.equal(g1.idle.longestStretch.over, false);
});

test("a cut off log: the game ends at its last line and is incomplete", () => {
  const lines = fixture("two_games.jsonl").split("\n").slice(0, 20).join("\n");
  const [g] = analyze(lines).games;
  assert.equal(g.status, "incomplete");
  assert.equal(g.endT, 142000);
  // Intro 30000 and QuestionShow 4000 idle; TopicVote and Answer active.
  assert.equal(g.idle.gameMs, 42000);
  assert.equal(g.idle.idleMs, 34000);
});

test("idle timeline: out of order lines are sorted, a Poll inside the window waits for every connected phone", () => {
  const e = (t, event, fields = {}) => ({ t, event, ...fields });
  const window = [
    e(0, "gameStarted", { players: ["a", "b"] }),
    e(0, "phaseStarted", { phase: "Poll", phaseId: 1 }),
    e(3000, "poll", { answers: {} }),
    e(1000, "poll", { answers: {} }), // written late: counts at 1000
    e(2000, "playerLeft", { playerId: "b", reason: "disconnected" }),
    e(5000, "gameEnded", { reason: "finished" }),
  ];
  // 0-1000: 0 of 2 polls, active. 1000-2000: 1 of 2, active. 2000 on: 1 of 1 connected, idle.
  const idle = idleTimeline(window, { startT: 0, endT: 5000, players: ["a", "b"], statusAtStart: new Map() });
  assert.equal(idle.gameMs, 5000);
  assert.equal(idle.idleMs, 3000);
  assert.equal(idle.longestStretch.ms, 3000);
});
