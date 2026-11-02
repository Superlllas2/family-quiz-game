// End to end check: scripted phones against the live relay and a running TV app.
// Usage: node tools/e2e_phones.mjs <ROOM> [relayHost]  (start the TV app first; ROOM is the code on the TV)
const room = process.argv[2];
const host = process.argv[3] ?? "family-quiz-relay.stenakirpichnaya.workers.dev";
const url = `wss://${host}/ws/phone?room=${room}`;
let failures = 0;
const ok = (c, m) => { console.log(`${c ? "ok  " : "FAIL"} ${m}`); if (!c) failures++; };

function phone(label) {
  const ws = new WebSocket(url);
  const inbox = [];
  const waiters = [];
  let closeCode = null;
  ws.onmessage = (e) => {
    const m = JSON.parse(e.data);
    inbox.push(m);
    for (const w of [...waiters]) if (w.pred(m)) { waiters.splice(waiters.indexOf(w), 1); w.res(m); }
  };
  ws.onclose = (e) => { closeCode = e.code; };
  const opened = new Promise((res, rej) => { ws.onopen = res; ws.onerror = rej; });
  return {
    label, ws, inbox, opened,
    get closeCode() { return closeCode; },
    send: (m) => ws.send(JSON.stringify({ v: 1, ...m })),
    wait(pred, ms = 3000) {
      const hit = inbox.find(pred);
      if (hit) return Promise.resolve(hit);
      return new Promise((res) => {
        const w = { pred, res };
        waiters.push(w);
        setTimeout(() => { const i = waiters.indexOf(w); if (i >= 0) { waiters.splice(i, 1); res(null); } }, ms);
      });
    },
  };
}
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const a = phone("A");
await a.opened;
let t0 = performance.now();
a.send({ type: "join", name: "Мама Ёлка", lang: "ru" });
const welcomeA = await a.wait((m) => m.type === "welcome");
ok(welcomeA && welcomeA.token?.length === 24, `A welcome in ${Math.round(performance.now() - t0)} ms, token length ${welcomeA?.token?.length}`);
const snapA = await a.wait((m) => m.type === "snapshot");
ok(snapA?.screen?.id === "lobby", `A snapshot screen ${snapA?.screen?.id}, phaseId ${snapA?.screen?.phaseId}`);
console.log("     player:", JSON.stringify(snapA?.player));

const b = phone("B");
await b.opened;
b.send({ type: "join", name: "мама ёлка", lang: "en" });
const refused = await b.wait((m) => m.type === "refused");
ok(refused?.reason === "nameTaken", `duplicate name (case) refused: ${refused?.reason}`);
b.send({ type: "join", name: "Bad!", lang: "en" });
const refused2 = await b.wait((m) => m.type === "refused" && m !== refused);
ok(refused2?.reason === "badName", `bad name refused: ${refused2?.reason}`);
b.send({ type: "join", name: "Papa", lang: "en" });
const welcomeB = await b.wait((m) => m.type === "welcome");
ok(!!welcomeB, "B joined");
const lobbyA = await a.wait((m) => m.type === "screen" && m.screen?.data?.players?.length === 2 || m.type === "screen" && m.data?.players?.length === 2);
ok(!!lobbyA, `A got a lobby screen with 2 players, canStart ${lobbyA?.data?.canStart ?? lobbyA?.screen?.data?.canStart}`);

const phaseId = (lobbyA?.phaseId ?? snapA?.screen?.phaseId);
const rtts = [];
for (let i = 0; i < 5; i++) {
  const inputId = `e2e${i}`;
  t0 = performance.now();
  a.send({ type: "input", inputId, phaseId, kind: "choice", choice: i % 4, ms: 1000 + i });
  const ack = await a.wait((m) => m.type === "ack" && m.inputId === inputId);
  rtts.push(Math.round(performance.now() - t0));
  ok(ack?.ok === true, `ack ${inputId} ok=${ack?.ok} in ${rtts.at(-1)} ms`);
  await sleep(150);
}
console.log(`     input->ack round trips (ms): ${rtts.join(", ")}`);
a.send({ type: "input", inputId: "e2e0", phaseId, kind: "choice", choice: 0, ms: 1000 });
const dup = await a.wait((m) => m.type === "ack" && m.inputId === "e2e0" && m.reason === "duplicate");
ok(dup?.ok === true, `duplicate inputId acked again: reason ${dup?.reason}`);

t0 = performance.now();
a.send({ type: "ping", t: 1 });
const pong = await a.wait((m) => m.type === "pong");
ok(pong?.t === 1, `pong in ${Math.round(performance.now() - t0)} ms`);

a.send({ type: "lobby", inputId: "s1", action: "start" });
const startAck = await a.wait((m) => m.type === "ack" && m.inputId === "s1");
ok(startAck && startAck.ok === false, `lobby start acked ok=${startAck?.ok} reason ${startAck?.reason}`);

// Same-conn rejoin is a refresh (no 4002)
a.send({ type: "rejoin", token: welcomeA.token });
const reW = await a.wait((m) => m.type === "welcome" && m !== welcomeA);
await sleep(500);
ok(!!reW && a.closeCode === null, `same-conn rejoin refreshes, socket still open`);

// Second socket with A's token takes the seat; older socket is closed with 4002
const a2 = phone("A2");
await a2.opened;
a2.send({ type: "rejoin", token: welcomeA.token });
const w2 = await a2.wait((m) => m.type === "welcome");
ok(w2?.playerId === welcomeA.playerId, `rejoin from a new socket gets the same seat ${w2?.playerId}`);
await sleep(1500);
ok(a.closeCode === 4002, `older socket closed with ${a.closeCode}`);

a2.send({ type: "rejoin", token: "x".repeat(24) });
const bad = await a2.wait((m) => m.type === "refused" && m.reason === "badToken");
ok(!!bad, "bad token refused");

b.send({ v: 2, type: "ping", t: 2 });
const bv = await b.wait((m) => m.type === "refused" && m.reason === "badVersion");
ok(!!bv, "v 2 refused with badVersion");

b.send({ type: "leave", inputId: "l1" });
const leaveAck = await b.wait((m) => m.type === "ack" && m.inputId === "l1");
ok(leaveAck?.ok === true, "leave acked");

console.log(failures === 0 ? "\nall e2e checks passed" : `\n${failures} e2e checks failed`);
for (const p of [a, b, a2]) try { p.ws.close(); } catch {}
process.exit(failures ? 1 : 0);
