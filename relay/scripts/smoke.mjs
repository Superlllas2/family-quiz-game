// Smoke test of the relay against a running Worker (wrangler dev or a deployed relay).
// Usage: node scripts/smoke.mjs [baseUrl]    default ws://127.0.0.1:8787
// Uses Node's built-in WebSocket only. Exits with code 1 on the first failed check.

const base = (process.argv[2] ?? process.env.RELAY_URL ?? "ws://127.0.0.1:8787").replace(/\/$/, "");
const LETTERS = "ABCDEFGHJKLMNPQRSTUVWXYZ";
const QUIET_MS = 400;

function roomCode() {
  let s = "";
  for (let i = 0; i < 4; i++) s += LETTERS[Math.floor(Math.random() * LETTERS.length)];
  return s;
}

function secret() {
  const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  let s = "";
  for (let i = 0; i < 32; i++) s += chars[Math.floor(Math.random() * chars.length)];
  return s;
}

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

class Client {
  constructor(label, url) {
    this.label = label;
    this.inbox = [];
    this.waiters = [];
    this.closed = null;
    this.ws = new WebSocket(url);
    this.opened = new Promise((resolve, reject) => {
      this.ws.addEventListener("open", () => resolve());
      this.ws.addEventListener("error", () => reject(new Error(`${label}: socket error`)));
    });
    this.closedPromise = new Promise((resolve) => {
      this.ws.addEventListener("close", (e) => {
        this.closed = { code: e.code, reason: e.reason };
        resolve(this.closed);
        this.flush();
      });
    });
    this.ws.addEventListener("message", (e) => {
      this.inbox.push(JSON.parse(e.data));
      this.flush();
    });
  }

  flush() {
    for (const w of [...this.waiters]) w();
  }

  send(obj) {
    this.ws.send(typeof obj === "string" ? obj : JSON.stringify(obj));
  }

  /** Takes the first inbox message matching pred, waiting up to ms. */
  take(pred, what, ms = 3000) {
    return new Promise((resolve, reject) => {
      const check = () => {
        const i = this.inbox.findIndex(pred);
        if (i >= 0) {
          done();
          resolve(this.inbox.splice(i, 1)[0]);
        }
      };
      const timer = setTimeout(() => {
        done();
        reject(new Error(`${this.label}: timed out waiting for ${what}; inbox ${JSON.stringify(this.inbox)}`));
      }, ms);
      const done = () => {
        clearTimeout(timer);
        this.waiters = this.waiters.filter((w) => w !== check);
      };
      this.waiters.push(check);
      check();
    });
  }

  async closeCode(ms = 3000) {
    const result = await Promise.race([this.closedPromise, sleep(ms).then(() => null)]);
    if (result === null) throw new Error(`${this.label}: not closed within ${ms} ms`);
    return result.code;
  }

  close() {
    if (this.closed === null) this.ws.close(1000, "smoke done");
  }
}

let passed = 0;
function check(cond, name, detail = "") {
  if (!cond) throw new Error(`FAIL ${name}${detail ? `: ${detail}` : ""}`);
  passed += 1;
  console.log(`ok   ${name}`);
}

const opened = [];
async function connect(label, path) {
  const c = new Client(label, base + path);
  opened.push(c);
  await c.opened;
  return c;
}
const tvPath = (room, s) => `/ws/tv?room=${room}&secret=${s}`;
const phonePath = (room) => `/ws/phone?room=${room}`;
const isConnected = (m) => m.relay === "connected";

async function main() {
  console.log(`relay smoke test against ${base}`);
  const room = roomCode();
  let other = roomCode();
  while (other === room) other = roomCode();
  const tvSecret = secret();

  // 4004 for a phone in an unclaimed room.
  const early = await connect("early phone", phonePath(other));
  check((await early.closeCode()) === 4004, "phone in an unclaimed room is closed with 4004");

  // TV and two phones in one room.
  const tv = await connect("tv", tvPath(room, tvSecret));
  const p1 = await connect("phone 1", phonePath(room));
  const c1 = (await tv.take(isConnected, "connected for phone 1")).conn;
  const p2 = await connect("phone 2", phonePath(room));
  const c2 = (await tv.take(isConnected, "connected for phone 2")).conn;
  check(/^c_[0-9a-f]{6}$/.test(c1) && /^c_[0-9a-f]{6}$/.test(c2) && c1 !== c2, "connected envelopes with distinct conn ids", `${c1} ${c2}`);

  // A TV and a phone in another room.
  const otherSecret = secret();
  const tvOther = await connect("tv other", tvPath(other, otherSecret));
  const pOther = await connect("phone other", phonePath(other));
  const cOther = (await tvOther.take(isConnected, "connected for other phone")).conn;

  // Phone to TV, unchanged inner message.
  const raw = '{"v":1,"type":"join","name":"Мама","lang":"ru"}';
  p1.send(raw);
  const env = await tv.take((m) => m.relay === "message", "message envelope");
  check(env.conn === c1 && JSON.stringify(env.msg) === raw, "phone message reaches the TV in a message envelope", JSON.stringify(env));

  // TV to one phone, without envelope.
  tv.send({ to: c1, msg: { v: 1, type: "welcome", playerId: "p1" } });
  const w = await p1.take((m) => m.type === "welcome", "welcome on phone 1");
  check(w.playerId === "p1" && w.relay === undefined, "TV `to` message reaches that phone without envelope");

  // TV to all.
  tv.send({ to: "all", msg: { v: 1, type: "screen", id: "lobby" } });
  await p1.take((m) => m.type === "screen", "screen on phone 1");
  await p2.take((m) => m.type === "screen", "screen on phone 2");
  check(true, "TV `to: all` reaches every phone in the room");

  // Isolation between rooms.
  tvOther.send({ to: "all", msg: { v: 1, type: "screen", id: "other" } });
  tvOther.send({ to: cOther, msg: { v: 1, type: "only-other" } });
  pOther.send({ v: 1, type: "ping", t: 1 });
  await pOther.take((m) => m.id === "other", "screen on other phone");
  await pOther.take((m) => m.type === "only-other", "to message on other phone");
  await tvOther.take((m) => m.relay === "message", "message on other TV");
  await sleep(QUIET_MS);
  check(p1.inbox.length === 0 && p2.inbox.length === 0 && tv.inbox.length === 0, "nothing crosses between rooms",
    JSON.stringify({ p1: p1.inbox, p2: p2.inbox, tv: tv.inbox }));

  // Wrong secret: 4001, the first TV keeps working.
  const intruder = await connect("intruder tv", tvPath(room, secret()));
  check((await intruder.closeCode()) === 4001, "TV with the wrong secret is closed with 4001");
  p2.send({ v: 1, type: "ping", t: 2 });
  const afterIntruder = await tv.take((m) => m.relay === "message", "message after intruder");
  check(afterIntruder.conn === c2 && tv.closed === null, "the first TV keeps the room");

  // Rate limit: 25 at once, 20 arrive, socket stays open; later messages flow.
  // Wait first: phone 2 sent a message less than a second ago, and the window is rolling.
  await sleep(1100);
  for (let i = 0; i < 25; i++) p2.send({ v: 1, type: "ping", t: 100 + i });
  await sleep(QUIET_MS * 2);
  const burst = tv.inbox.filter((m) => m.relay === "message" && m.conn === c2);
  tv.inbox.length = 0;
  check(burst.length === 20, "25 messages in under a second: exactly 20 forwarded", `got ${burst.length}`);
  check(p2.closed === null, "the rate limited phone stays connected");
  await sleep(1100);
  p2.send({ v: 1, type: "ping", t: "after" });
  const after = await tv.take((m) => m.relay === "message", "message after the rate window");
  check(after.msg.t === "after", "messages flow again after the window");

  // Close control: exactly that phone, 4002.
  tv.send({ close: c1, code: 4002 });
  check((await p1.closeCode()) === 4002, "close control closes that phone with 4002");
  const disc = await tv.take((m) => m.relay === "disconnected", "disconnected for phone 1");
  check(disc.conn === c1, "TV is told the closed phone is gone");
  tv.send({ to: "all", msg: { v: 1, type: "still-here" } });
  await p2.take((m) => m.type === "still-here", "phone 2 still receives");
  check(p2.closed === null, "the other phone stays open");

  // tvAway / tvBack, messages in between dropped.
  tv.close();
  await tv.closedPromise;
  await p2.take((m) => m.type === "tvAway", "tvAway");
  check(true, "phones get tvAway when the TV socket closes");
  p2.send({ v: 1, type: "ping", t: "while-away" });
  const lateJoiner = await connect("late phone", phonePath(room));
  check((await lateJoiner.closeCode()) === 4004, "phone joining while the TV is away is closed with 4004");
  await sleep(QUIET_MS);
  const tv2 = await connect("tv again", tvPath(room, tvSecret));
  const again = await tv2.take(isConnected, "connected for phone 2 after reconnect");
  check(again.conn === c2, "reconnected TV gets one connected per open phone");
  await p2.take((m) => m.type === "tvBack", "tvBack");
  check(true, "phones get tvBack when the TV returns with the right secret");
  await sleep(QUIET_MS);
  check(tv2.inbox.length === 0, "messages sent while the TV was away are not delivered", JSON.stringify(tv2.inbox));

  // A newer TV socket with the right secret replaces an open one; no tvAway for the phones.
  const tv3 = await connect("tv newer", tvPath(room, tvSecret));
  check((await tv2.closeCode()) === 1000, "an open TV socket is replaced by a newer one with the right secret");
  const again3 = await tv3.take(isConnected, "connected for phone 2 on the newer TV");
  await sleep(QUIET_MS);
  check(again3.conn === c2 && p2.inbox.length === 0, "the replaced TV socket does not send tvAway", JSON.stringify(p2.inbox));

  // Phone disconnect reported.
  p2.close();
  const gone = await tv3.take((m) => m.relay === "disconnected", "disconnected for phone 2");
  check(gone.conn === c2, "phone disconnect is reported to the TV");
  check(tvOther.inbox.length === 0 && pOther.inbox.length === 0, "the other room saw none of this room's traffic",
    JSON.stringify({ tvOther: tvOther.inbox, pOther: pOther.inbox }));

  // The route is checked before the upgrade (Node's fetch cannot send an Upgrade header).
  const http = base.replace(/^ws/, "http");
  const bad = await fetch(`${http}/ws/phone?room=abcd`);
  check(bad.status === 400, "bad room code is refused with HTTP 400", `status ${bad.status}`);

  console.log(`\nall ${passed} smoke checks passed`);
}

main()
  .then(() => {
    for (const c of opened) c.close();
    setTimeout(() => process.exit(0), 200);
  })
  .catch((err) => {
    console.error(err.message);
    for (const c of opened) c.close();
    setTimeout(() => process.exit(1), 200);
  });
