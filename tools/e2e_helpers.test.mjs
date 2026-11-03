// Tests of the scripted phones' pure helpers (tools/e2e_helpers.mjs). Run: node --test tools/
import { test } from "node:test";
import assert from "node:assert/strict";
import { sliderMiddle, valueText, pickTarget, clearDelayMs, pollAnswers } from "./e2e_helpers.mjs";

const fixed = (...values) => { let i = 0; return () => values[i++ % values.length]; };

test("sliderMiddle stays on the step grid, the upper value on a tie", () => {
  assert.equal(sliderMiddle(0, 100, 1), 50);
  assert.equal(sliderMiddle(0, 10, 4), 4);
  assert.equal(sliderMiddle(0, 3, 1), 2);
  assert.equal(sliderMiddle(0.5, 1.5, 0.1), 1);
  assert.equal(valueText(1889), "1889");
  assert.equal(valueText(1.005), "1");
});

test("pickTarget picks only available targets, null when there is none", () => {
  const targets = [
    { playerId: "p1", name: "Mama", available: false },
    { playerId: "p2", name: "Papa", available: true },
    { playerId: "p3", name: "Kid", available: true },
  ];
  assert.equal(pickTarget(targets, fixed(0)), "p2");
  assert.equal(pickTarget(targets, fixed(0.99)), "p3");
  assert.equal(pickTarget([{ playerId: "p1", available: false }], fixed(0.5)), null);
  assert.equal(pickTarget(undefined, fixed(0.5)), null);
});

test("clearDelayMs is 1 to 3 s", () => {
  assert.equal(clearDelayMs(fixed(0)), 1000);
  assert.equal(clearDelayMs(fixed(0.999999)), 3000);
});

test("pollAnswers answers every question: an option index for choice, a short text for text", () => {
  const questions = [
    { id: "fun", text: "The most fun moment?", kind: "text", options: [] },
    { id: "waiting", text: "When?", kind: "choice", options: ["Never", "Between questions", "During results", "In the final"] },
    { id: "again", text: "Again?", kind: "choice", options: ["Yes", "Maybe", "No"] },
  ];
  const answers = pollAnswers(questions, fixed(0.99, 0.0), "A");
  assert.deepEqual(Object.keys(answers), ["fun", "waiting", "again"]);
  assert.equal(typeof answers.fun, "string");
  assert.ok(answers.fun.length > 0 && answers.fun.length <= 200);
  assert.equal(answers.waiting, 3);
  assert.equal(answers.again, 0);
});
