// End to end check of the Day 3 show, protocol version 2 (the phone is input only, D88): three scripted phones tap
// Ready (D76) and play a whole Standard show against the live relay and a running TV app: 4 rounds of topic vote and
// 4 questions (3 multiple choice, 1 closest value on the slider; the answer screens carry no question text), the final
// race with true or false (no statement on the phone), Results (`results` with a fact for the winner only, D87), then
// back in the Lobby; one leaves. Between decisions every phone gets `watch` (Intro, QuestionShow, Reveal and Scores,
// RoundEnd, FinalIntro, RaceReveal, Results for the others).
// Usage: node tools/e2e_game.mjs <ROOM> [relayHost] [--dump <file>]  (start the TV app first; ROOM is the code on the TV)
// --dump writes every text frame the phones received to a JSON file (the phone page tests parse such a capture).
import { writeFileSync } from "node:fs";
const args = process.argv.slice(2);
const dumpAt = args.indexOf("--dump");
const dump = dumpAt >= 0 ? args.splice(dumpAt, 2)[1] : null;
const room = args[0];
const host = args[1] ?? "family-quiz-relay.stenakirpichnaya.workers.dev";
const received = [];
const url = `wss://${host}/ws/phone?room=${room}`;
let failures = 0;
const ok = (c, m) => { console.log(`${c ? "ok  " : "FAIL"} ${m}`); if (!c) failures++; };
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
let nextInput = 1;

function phone(label) {
  const ws = new WebSocket(url);
  const inbox = [];
  const waiters = [];
  ws.onmessage = (e) => {
    received.push(e.data);
    const m = JSON.parse(e.data);
    m.at = performance.now();
    inbox.push(m);
    for (const w of [...waiters]) if (w.pred(m)) { waiters.splice(waiters.indexOf(w), 1); w.res(m); }
  };
  const opened = new Promise((res, rej) => { ws.onopen = res; ws.onerror = rej; });
  const p = {
    label, inbox, opened,
    send: (m) => ws.send(JSON.stringify({ v: 2, ...m })),
    acked(m) { const inputId = `${label}${nextInput++}`; p.send({ ...m, inputId }); return p.wait((x) => x.type === "ack" && x.inputId === inputId); },
    // The next screen message with this id that arrives after `since` (performance.now()).
    screen(id, since = 0, ms = 30000) { return p.wait((x) => x.type === "screen" && x.id === id && x.at >= since, ms); },
    // Reading the screens in order: the next screen after the cursor that is not a resend of the last one (same
    // phaseId and id: last call, a waiting list update). Null after `ms` without one.
    cursor: 0,
    last: null,
    async nextScreen(ms = 45000) {
      const fresh = (x) => x.type === "screen" && inbox.indexOf(x) >= p.cursor
        && !(p.last && x.phaseId === p.last.phaseId && x.id === p.last.id);
      const s = await p.wait(fresh, ms);
      if (s) { p.cursor = inbox.indexOf(s) + 1; p.last = s; }
      return s;
    },
    wait(pred, ms = 5000) {
      const hit = inbox.find(pred);
      if (hit) return Promise.resolve(hit);
      return new Promise((res) => {
        const w = { pred, res };
        waiters.push(w);
        setTimeout(() => { const i = waiters.indexOf(w); if (i >= 0) { waiters.splice(i, 1); res(null); } }, ms);
      });
    },
    close: () => ws.close(),
  };
  return p;
}

const names = ["Mama", "Papa", "Kid"];
const phones = names.map((n, i) => phone(String.fromCharCode(65 + i)));
await Promise.all(phones.map((p) => p.opened));
for (const [i, p] of phones.entries()) {
  p.send({ type: "join", name: names[i] + Math.floor(Math.random() * 900 + 100), lang: i === 1 ? "en" : "ru" });
  const snap = await p.wait((m) => m.type === "snapshot");
  ok(snap?.screen?.id === "pickCharacter", `${p.label} joins and sees pickCharacter`);
  const ack = await p.acked({ type: "character", characterId: snap.screen.data.current });
  ok(ack?.ok === true, `${p.label} picks character ${snap.screen.data.current}`);
  ok(!!(await p.screen("lobby")), `${p.label} then sees the lobby`);
}

const [a, b, c] = phones;
const readyMarks = (s) => (s?.data?.players ?? []).map((p) => p.ready);
let t = performance.now();
let ack = await b.acked({ type: "lobby", action: "start" });
ok(ack?.ok === false && ack.reason === "invalid", "the old Start action is gone (invalid)");
ack = await b.acked({ type: "lobby", action: "ready" });
ok(ack?.ok === true, "B taps Ready");
let seen = await a.wait((m) => m.type === "screen" && m.id === "lobby" && m.at >= t && readyMarks(m)[1] === true);
ok(seen && seen.data.ready === false && seen.data.enoughPlayers === true, `A's lobby shows B ready: ${JSON.stringify(readyMarks(seen))}`);
ack = await a.acked({ type: "lobby", action: "ready" });
ok(ack?.ok === true, "A taps Ready");
await sleep(300);
ok(!a.inbox.some((m) => m.type === "screen" && m.id === "countdown"), "two of three ready: no countdown yet");
t = performance.now();
ack = await c.acked({ type: "lobby", action: "ready" });
ok(ack?.ok === true, "C taps Ready, the last one");
let cd = await a.screen("countdown", t);
ok(cd && cd.endsInMs > 2000 && cd.endsInMs <= 3000, `countdown on A (${cd?.endsInMs} ms, text "${cd?.data?.text}")`);
t = performance.now();
ack = await c.acked({ type: "lobby", action: "notReady" });
ok(ack?.ok === true, "C taps Not ready during the countdown");
seen = await a.screen("lobby", t);
ok(seen && readyMarks(seen).join() === "true,true,false", `everyone back in the lobby, marks ${JSON.stringify(readyMarks(seen))}`);
t = performance.now();
ack = await c.acked({ type: "lobby", action: "ready" });
ok(ack?.ok === true, "C taps Ready again");
cd = await a.screen("countdown", t);
ok(cd && cd.endsInMs > 2000, `the countdown runs again (${cd?.endsInMs} ms)`);

// The slider's start value, as the phone page computes it (phone/src/sliderValue.ts): the value on the grid
// min + k * step nearest the middle of the range, the upper one on a tie.
function sliderMiddle(min, max, step) {
  const s = step > 0 ? step : 1;
  const last = max > min ? Math.floor((max - min) / s + 1e-9) : 0;
  const k = Math.min(last, Math.max(0, Math.round((max - min) / 2 / s)));
  const decimals = (x) => { const t = String(x); return t.includes("e") ? 10 : t.includes(".") ? Math.min(10, t.length - t.indexOf(".") - 1) : 0; };
  return Number((min + k * s).toFixed(Math.max(decimals(min), decimals(s))));
}
// How the phone writes a value: no thousands separator, at most 2 decimals (the TV writes it the same way).
const valueText = (v) => String(Number(v.toFixed(2)) || 0);

// Each phone plays the show on its own, screen by screen, with its own timing: A always answers first, B after
// 1 s, C after 2 s; C leaves its second question open to get a last call.
async function play(p, i) {
  const log = { votes: 0, questions: 0, sliders: 0, race: 0, watches: 0, results: null, beforeLobby: null, lobby: null };
  let lastAnswer = null; // the input sent in the last Answer, resent on the next watch (Reveal) to check it is refused
  let prev = null; // the id of the screen before this one
  const say = (m) => `${p.label} ${m}`;
  // Version 2 sends no question, statement or pending list to the phones (D88).
  const lacks = (s, ...fields) => fields.every((f) => !Object.hasOwn(s.data ?? {}, f));
  for (;;) {
    const s = await p.nextScreen();
    if (!s) { ok(false, say(`gets no new screen for 45 s (last: ${p.last?.id})`)); return log; }
    const before = prev;
    prev = s.id;
    switch (s.id) {
      case "countdown":
        break;
      case "waiting":
        if (!(typeof s.data.locked === "string" && s.data.locked !== "" && lacks(s, "pending"))) {
          ok(false, say(`waiting has only a locked text: ${JSON.stringify(s.data)}`));
        }
        break;
      case "watch":
        log.watches++;
        if (Object.keys(s.data ?? {}).length !== 0) ok(false, say(`watch has no data: ${JSON.stringify(s.data)}`));
        if (i === 0 && lastAnswer) {
          // Reveal: the Answer is closed.
          const stale = await p.acked({ ...lastAnswer, ms: 100 });
          ok(stale?.ok === false, say(`an input for the closed Answer is refused (${stale?.reason})`));
        }
        lastAnswer = null;
        break;
      case "topicVote": {
        log.votes++;
        ok(Array.isArray(s.data.doors) && s.data.doors.length === 4 && s.data.doors.every((d) => typeof d.title === "string" && d.title !== ""),
          say(`round ${log.votes}: topicVote doors ${JSON.stringify(s.data.doors.map((d) => d.title))}`));
        await sleep(i * 700);
        const r = await p.acked({ type: "input", phaseId: s.phaseId, kind: "vote", door: i % 4 });
        ok(r?.ok === true, say(`votes for door ${i % 4}`));
        if (i === 0) {
          const w = await p.wait((m) => m.type === "screen" && m.id === "waiting" && m.phaseId === s.phaseId, 3000);
          ok(w && typeof w.data.locked === "string" && w.data.locked !== "" && lacks(w, "pending"),
            say(`waits with the door locked "${w?.data?.locked}", no pending list`));
        }
        break;
      }
      case "answerChoice": {
        log.questions++;
        const q = log.questions;
        ok(s.data.options?.length === 4 && s.data.windowMs > 0 && lacks(s, "question"),
          say(`Q${q}: answerChoice with 4 options and no question text ${JSON.stringify(s.data.options)}`));
        if (i === 2 && q === 2) {
          // C leaves this one open: once A and B locked in, C gets a last call with the same phaseId.
          const lastCall = await p.wait((m) => m.type === "screen" && m.id === "answerChoice" && m.phaseId === s.phaseId && m !== s, 20000);
          ok(lastCall && lastCall.endsInMs <= 5000, say(`Q${q}: gets last call, same phaseId, ${lastCall?.endsInMs} ms left`));
          break;
        }
        await sleep(i * 1000);
        const msg = { type: "input", phaseId: s.phaseId, kind: "choice", choice: i, ms: 800 + i * 1000 };
        const r = await p.acked(msg);
        ok(r?.ok === true, say(`Q${q}: locked in choice ${i}`));
        lastAnswer = msg;
        if (i === 0) {
          const w = await p.wait((m) => m.type === "screen" && m.id === "waiting" && m.phaseId === s.phaseId, 3000);
          ok(w && typeof w.data.locked === "string" && w.data.locked !== "" && lacks(w, "pending"),
            say(`Q${q}: waits with "${w?.data?.locked}" locked, no pending list`));
        }
        break;
      }
      case "answerSlider": {
        log.questions++;
        log.sliders++;
        const q = log.questions;
        const { min, max, step, unit, windowMs } = s.data;
        ok(typeof min === "number" && typeof max === "number" && max > min && step > 0 && windowMs > 0 && lacks(s, "question"),
          say(`Q${q}: answerSlider from ${min} to ${max}, step ${step}, unit "${unit}", no question text`));
        const value = sliderMiddle(min, max, step);
        const k = (value - min) / step;
        ok(value >= min && value <= max && Math.abs(k - Math.round(k)) < 1e-6, say(`Q${q}: the middle ${value} is on the step grid`));
        if (i === 1) {
          const r = await p.acked({ type: "input", phaseId: s.phaseId, kind: "value", value: max + step, ms: 500 });
          ok(r?.ok === false && r.reason === "invalid", say(`Q${q}: a value outside the range is refused (${r?.reason})`));
        }
        await sleep(i * 1000);
        const msg = { type: "input", phaseId: s.phaseId, kind: "value", value, ms: 1500 + i * 1000 };
        const r = await p.acked(msg);
        ok(r?.ok === true, say(`Q${q}: locked in ${value}`));
        lastAnswer = msg;
        if (i === 0) {
          const w = await p.wait((m) => m.type === "screen" && m.id === "waiting" && m.phaseId === s.phaseId, 3000);
          ok(typeof w?.data?.locked === "string" && w.data.locked.startsWith(valueText(value)), say(`Q${q}: waiting shows the value, locked "${w?.data?.locked}"`));
        }
        break;
      }
      case "answerTrueFalse": {
        log.race++;
        ok(s.data.windowMs > 0 && lacks(s, "statement"), say(`race ${log.race}: answerTrueFalse with no statement text`));
        await sleep(i * 500);
        const r = await p.acked({ type: "input", phaseId: s.phaseId, kind: "trueFalse", answer: i % 2 === 0, ms: 900 + i * 500 });
        ok(r?.ok === true, say(`race ${log.race}: answers ${i % 2 === 0}`));
        break;
      }
      case "results":
        log.results = s;
        ok(typeof s.data.fact === "string" && s.data.fact !== "" && Object.keys(s.data).length === 1,
          say(`wins: results with only the fact "${s.data.fact}"`));
        break;
      case "lobby":
        ok(log.race >= 1, say("is back in the lobby after the race"));
        log.beforeLobby = before;
        log.lobby = s;
        return log;
      default:
        // getReady, roundEnd and raceResult are gone in version 2; result only comes with power ups (Day 4).
        ok(false, say(`gets an unexpected screen ${s.id} ${JSON.stringify(s.data)}`));
        break;
    }
  }
}

// Each phone reads the show from its own countdown screen on (a lobby update just before it is not the end).
for (const p of phones) {
  const start = await p.screen("countdown", t);
  ok(!!start, `${p.label} sees the countdown`);
  p.cursor = start ? p.inbox.indexOf(start) + 1 : p.inbox.length;
  p.last = start;
}
const logs = await Promise.all(phones.map((p, i) => play(p, i)));
const [logA] = logs;
ok(logs.every((l) => l.votes === 4), `4 topic votes on every phone (${logs.map((l) => l.votes)})`);
ok(logA.questions === 16 && logA.sliders === 4, `16 questions played, 4 of them closest value (${logA.questions}, ${logA.sliders})`);
ok(logA.race >= 1 && logA.race <= 12, `the race took ${logA.race} questions (1 to 12)`);
ok(logs.every((l) => l.watches > 0), `every phone watches the TV between decisions (${logs.map((l) => l.watches)} watch screens)`);
const winners = logs.filter((l) => l.results);
ok(winners.length === 1, `exactly one phone gets results, the winner (${winners.length})`);
logs.forEach((l, i) => {
  const want = l.results ? "results" : "watch";
  ok(l.beforeLobby === want, `${phones[i].label} sees ${want} on Results (${l.beforeLobby})`);
});

const resultsAt = winners[0]?.results.at ?? logA.lobby?.at ?? 0;
const back = logs[1].lobby;
ok(back && back.data.ready === false && readyMarks(back).every((r) => r === false), `after Results, B is back in the lobby, nobody ready ${JSON.stringify(readyMarks(back))}`);
const zero = await a.wait((m) => m.type === "player" && m.at >= resultsAt && m.player?.score === 0, 30000);
ok(!!zero, "A's score is back to 0");

t = performance.now();
ack = await b.acked({ type: "leave" });
ok(ack?.ok === true, "B leaves");
const left = await a.wait((m) => m.type === "screen" && m.id === "lobby" && m.at >= t && m.data.players.length === 2);
ok(left?.data?.enoughPlayers === true, "two players left: still enough to start");

phones.forEach((p) => p.close());
if (dump) writeFileSync(dump, JSON.stringify(received, null, 1) + "\n");
console.log(failures === 0 ? "\nALL OK" : `\n${failures} FAILED`);
process.exit(failures === 0 ? 0 : 1);
