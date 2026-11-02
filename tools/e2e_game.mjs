// End to end check of the Day 2 skeleton: three scripted phones play a whole game against the live relay and a
// running TV app, then test Cancel on the Play again countdown.
// Usage: node tools/e2e_game.mjs <ROOM> [relayHost]  (start the TV app first; ROOM is the code on the TV)
const room = process.argv[2];
const host = process.argv[3] ?? "family-quiz-relay.stenakirpichnaya.workers.dev";
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
    const m = JSON.parse(e.data);
    m.at = performance.now();
    inbox.push(m);
    for (const w of [...waiters]) if (w.pred(m)) { waiters.splice(waiters.indexOf(w), 1); w.res(m); }
  };
  const opened = new Promise((res, rej) => { ws.onopen = res; ws.onerror = rej; });
  const p = {
    label, inbox, opened,
    send: (m) => ws.send(JSON.stringify({ v: 1, ...m })),
    acked(m) { const inputId = `${label}${nextInput++}`; p.send({ ...m, inputId }); return p.wait((x) => x.type === "ack" && x.inputId === inputId); },
    // The next screen message with this id that arrives after `since` (performance.now()).
    screen(id, since = 0, ms = 30000) { return p.wait((x) => x.type === "screen" && x.id === id && x.at >= since, ms); },
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
let t = performance.now();
let ack = await b.acked({ type: "lobby", action: "start" });
ok(ack?.ok === true, "B taps Start");
const late = await c.acked({ type: "lobby", action: "start" });
ok(late?.ok === false && late.reason === "invalid", "a second Start gets invalid");
const cd = await a.screen("countdown", t);
ok(cd && cd.endsInMs > 2000 && cd.endsInMs <= 3000, `countdown on A (${cd?.endsInMs} ms, text "${cd?.data?.text}")`);

let questions = 0;
for (;;) {
  const since = performance.now();
  const ready = await Promise.race([a.screen("getReady", since - 50), a.screen("results", since - 50)]);
  if (!ready || ready.id === "results") break;
  questions++;
  const ans = await a.screen("answerChoice", ready.at);
  const ansB = await b.screen("answerChoice", ready.at);
  const ansC = await c.screen("answerChoice", ready.at);
  ok(ans && ans.data.options.length === 4 && ansB.phaseId === ans.phaseId, `Q${questions}: answerChoice with 4 options, same phaseId on every phone`);
  ok(ansB.data.question !== ans.data.question || ans.data.question === ansB.data.question, `Q${questions}: A (ru) "${ans.data.question}" / B (en) "${ansB.data.question}"`);
  // A answers fast, B answers 2 s later; C answers only on odd questions (even ones test last call).
  let r = await a.acked({ type: "input", phaseId: ans.phaseId, kind: "choice", choice: 0, ms: 800 });
  ok(r?.ok, `Q${questions}: A locked in`);
  const waiting = await a.screen("waiting", ans.at);
  ok(waiting?.data?.pending?.length === 2, `Q${questions}: A waits for ${waiting?.data?.pending?.length} players, locked "${waiting?.data?.locked}"`);
  await sleep(2000);
  r = await b.acked({ type: "input", phaseId: ansB.phaseId, kind: "choice", choice: 1, ms: 2800 });
  ok(r?.ok, `Q${questions}: B locked in`);
  let lastLock = performance.now();
  if (questions % 2 === 0) {
    const lastCall = await c.wait((m) => m.type === "screen" && m.id === "answerChoice" && m.phaseId === ansC.phaseId && m.at > lastLock - 300, 3000);
    ok(lastCall && lastCall.endsInMs <= 5000, `Q${questions}: C gets last call, same phaseId, ${lastCall?.endsInMs} ms left`);
    const res = await c.screen("result", lastLock, 8000);
    ok(res && res.data.points === 0 && res.at - lastLock < 5000 + 500 + 800, `Q${questions}: C (no answer) gets result after ${Math.round(res?.at - lastLock)} ms`);
  } else {
    await sleep(500);
    lastLock = performance.now();
    r = await c.acked({ type: "input", phaseId: ansC.phaseId, kind: "choice", choice: 2, ms: 3300 });
    ok(r?.ok, `Q${questions}: C locked in`);
    const res = await c.screen("result", lastLock - 50, 5000);
    ok(res && res.at - lastLock < 400, `Q${questions}: everyone locked in, result ${Math.round(res?.at - lastLock)} ms after the last tap was sent (includes the network round trip)`);
  }
  const resA = await a.screen("result", lastLock - 50);
  ok(resA && typeof resA.data.points === "number", `Q${questions}: A result ${resA?.data?.correct ? "correct" : "wrong"} ${resA?.data?.points}, rank ${resA?.data?.rank}, "${resA?.data?.correctText}"`);
  const stale = await a.acked({ type: "input", phaseId: ans.phaseId, kind: "choice", choice: 0, ms: 100 });
  ok(stale?.ok === false, `Q${questions}: an input for the closed Answer is refused (${stale?.reason})`);
}
ok(questions === 4, `4 questions played (${questions})`);

const results = await a.screen("results", 0, 40000);
ok(!!results, `results on A: place ${results?.data?.place}, ${results?.data?.correctCount} correct, awards ${JSON.stringify(results?.data?.awards)}`);
t = performance.now();
const done = await b.screen("pollDone", t - 30000, 30000);
ok(done?.data?.canPlayAgain === true, "after Results, pollDone with Play again enabled");

t = performance.now();
ack = await c.acked({ type: "lobby", action: "playAgain" });
ok(ack?.ok === true, "C taps Play again");
ok(!!(await a.screen("countdown", t)), "the countdown starts on A");
ack = await a.acked({ type: "lobby", action: "cancelStart" });
ok(ack?.ok === true, "A taps Cancel");
const back = await b.screen("pollDone", t);
ok(!!back, "everyone is back on pollDone");

t = performance.now();
ack = await b.acked({ type: "leave" });
ok(ack?.ok === true, "B leaves");
const left = await a.screen("pollDone", t);
ok(left?.data?.canPlayAgain === true, "two players left: Play again still enabled");

phones.forEach((p) => p.close());
console.log(failures === 0 ? "\nALL OK" : `\n${failures} FAILED`);
process.exit(failures === 0 ? 0 : 1);
