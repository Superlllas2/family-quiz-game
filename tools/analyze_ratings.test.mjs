import { test } from "node:test";
import assert from "node:assert/strict";
import { collectRatings, analyze, report } from "./analyze_ratings.mjs";

const line = (o) => JSON.stringify({ t: 1, session: "s1", build: "b", event: "questionRated", phaseId: 7, ...o });

const log = [
  line({ playerId: "p1", questionId: "q1", topic: "food", format: "multipleChoice", difficulty: "easy", item: "fun", stars: 2, correct: true }),
  line({ playerId: "p1", questionId: "q1", topic: "food", format: "multipleChoice", difficulty: "easy", item: "fun", stars: 5, correct: true }),
  line({ playerId: "p2", questionId: "q1", topic: "food", format: "multipleChoice", difficulty: "easy", item: "fun", stars: 3, correct: false }),
  line({ playerId: "p2", questionId: "q1", topic: "food", format: "multipleChoice", difficulty: "easy", item: "fact", stars: 4, correct: false }),
  line({ playerId: "p1", questionId: "q2", topic: "space", format: "closestValue", difficulty: "hard", item: "fun", stars: 1, correct: null }),
  line({ playerId: "p2", questionId: "q2", topic: "space", format: "closestValue", difficulty: "hard", item: "fun", stars: 2, correct: null }),
  "{not json",
  JSON.stringify({ t: 2, event: "input", playerId: "p1" }),
].join("\n");

test("only the latest tap per player, question and item counts", () => {
  const { ratings, lines, skipped } = collectRatings([{ session: "f", text: log }]);
  assert.equal(lines, 6);
  assert.equal(skipped, 1);
  assert.equal(ratings.length, 5);
  assert.equal(ratings.find((r) => r.playerId === "p1" && r.questionId === "q1").stars, 5);
});

test("per question means, raters and the share right", () => {
  const { ratings } = collectRatings([{ session: "f", text: log }]);
  const result = analyze(ratings, { audit: new Map([["q2", { score: 40, action: "REPLACE" }], ["q1", { score: 80, action: "KEEP" }]]) });
  const q1 = result.perQuestion.find((q) => q.key === "q1");
  assert.equal(q1.raters, 2);
  assert.equal(q1.correctShare, 0.5);
  assert.equal(q1.items.fun.mean, 4);
  assert.equal(q1.items.fact.n, 1);
  assert.deepEqual(result.best.map((q) => q.key), ["q1", "q2"]);
  assert.equal(result.byCorrect.right.fun.mean, 5);
  assert.equal(result.byCorrect["no answer"].fun.n, 2);
  assert.equal(result.audit.joined, 2);
  assert.equal(result.totals.players, 2);
});

test("the banks compare overall, per shared topic and per question pair", () => {
  const rated = (o) => line({ topic: "food", format: "multipleChoice", difficulty: "easy", item: "fun", correct: true, ...o });
  const text = [
    rated({ playerId: "p1", questionId: "food_001", stars: 2 }),
    rated({ playerId: "p2", questionId: "food_001", stars: 3, bank: "full" }),
    rated({ playerId: "p1", questionId: "cur_food_01", stars: 5, bank: "curated", session: "s2" }),
    rated({ playerId: "p1", questionId: "cur_food_02", stars: 4, bank: "curated", session: "s2" }),
  ].join("\n");
  const { ratings } = collectRatings([{ session: "f", text }]);
  const result = analyze(ratings, { curated: new Map([["cur_food_01", { basedOn: "food_001", refine: "rewrite" }], ["cur_food_02", { basedOn: null, refine: "new" }]]) });
  const banks = Object.fromEntries(result.banks.perBank.map((g) => [g.key, g.items.fun.mean]));
  assert.deepEqual(banks, { full: 2.5, curated: 4.5 });
  assert.equal(result.banks.sharedTopics.length, 1);
  assert.equal(result.banks.pairs.length, 1);
  assert.equal(result.banks.pairs[0].refine, "rewrite");
  assert.match(report(result), /Full bank against curated bank/);
});

test("the report says when feedback was never on", () => {
  assert.match(report(analyze([])), /Was question feedback switched on/);
  const { ratings } = collectRatings([{ session: "f", text: log }]);
  assert.match(report(analyze(ratings)), /## By topic/);
});
