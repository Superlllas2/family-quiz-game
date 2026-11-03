// Pure helpers of the scripted phones in tools/e2e_game.mjs (that script opens sockets when loaded, so its pure parts
// live here, tested by tools/e2e_helpers.test.mjs). `rnd` is a function giving numbers in [0, 1), like Math.random.

/**
 * The slider's start value, as the phone page computes it (phone/src/sliderValue.ts): the value on the grid
 * min + k * step nearest the middle of the range, the upper one on a tie.
 */
export function sliderMiddle(min, max, step) {
  const s = step > 0 ? step : 1;
  const last = max > min ? Math.floor((max - min) / s + 1e-9) : 0;
  const k = Math.min(last, Math.max(0, Math.round((max - min) / 2 / s)));
  const decimals = (x) => { const t = String(x); return t.includes("e") ? 10 : t.includes(".") ? Math.min(10, t.length - t.indexOf(".") - 1) : 0; };
  return Number((min + k * s).toFixed(Math.max(decimals(min), decimals(s))));
}

/** How the phone writes a value: no thousands separator, at most 2 decimals (the TV writes it the same way). */
export const valueText = (v) => String(Number(v.toFixed(2)) || 0);

/** A random available target's playerId from `result.data.targets`, or null when none is available. */
export function pickTarget(targets, rnd) {
  const open = (Array.isArray(targets) ? targets : []).filter((t) => t && t.available === true && typeof t.playerId === "string");
  if (open.length === 0) return null;
  return open[Math.min(open.length - 1, Math.floor(rnd() * open.length))].playerId;
}

/** How long a scripted phone takes to clear a sabotage overlay: 1 to 3 s. */
export const clearDelayMs = (rnd) => 1000 + Math.floor(rnd() * 2001);

/**
 * Answers to every poll question (`poll.data.questions`): a random option index for a choice question, a short text
 * (at most 200 characters) for a text question. Questions of another kind are left out.
 */
export function pollAnswers(questions, rnd, label = "") {
  const answers = {};
  for (const q of Array.isArray(questions) ? questions : []) {
    if (q.kind === "choice" && Array.isArray(q.options) && q.options.length > 0) {
      answers[q.id] = Math.min(q.options.length - 1, Math.floor(rnd() * q.options.length));
    } else if (q.kind === "text") {
      answers[q.id] = `Scripted phone ${label} ${q.id}`.slice(0, 200);
    }
  }
  return answers;
}
