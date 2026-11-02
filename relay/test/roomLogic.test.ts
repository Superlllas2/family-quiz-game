// Room behaviour from docs/PROTOCOL.md section 2 and the Day 1 relay checks in docs/19_acceptance.md.

import { beforeEach, describe, expect, it } from "vitest";
import {
  CLOSE_NO_TV,
  CLOSE_REPLACED_BY_TV,
  CLOSE_TV_REPLACED,
  CLOSE_WRONG_SECRET,
  MAX_MESSAGE_BYTES,
  PHONE_RATE_LIMIT_COUNT,
  PHONE_RATE_WINDOW_MS,
  ROOM_CLOSE_AFTER_TV_AWAY_MS,
} from "../src/protocol";
import { FakeSocket, FakeStorage, SECRET_A, SECRET_B, TestRoom } from "./fakes";

const TV_AWAY = { v: 1, type: "tvAway" };
const TV_BACK = { v: 1, type: "tvBack" };
const join = (name: string) => JSON.stringify({ v: 1, type: "join", name, lang: "ru" });

let room: TestRoom;

beforeEach(async () => {
  room = await new TestRoom().start();
});

describe("phone connect", () => {
  it("assigns a conn id c_ + 6 lowercase hex and tells the TV", () => {
    const tv = room.tv(SECRET_A);
    room.phone();
    const [msg] = tv.json() as { relay: string; conn: string }[];
    expect(msg.relay).toBe("connected");
    expect(msg.conn).toMatch(/^c_[0-9a-f]{6}$/);
    expect(tv.sent).toHaveLength(1);
  });

  it("never gives two open phones the same conn id", () => {
    const tv = room.tv(SECRET_A);
    room.ids.forced.push("aaaaaa", "aaaaaa", "bbbbbb");
    room.phone("p1");
    room.phone("p2");
    const conns = (tv.json() as { conn: string }[]).map((m) => m.conn);
    expect(conns).toEqual(["c_aaaaaa", "c_bbbbbb"]);
  });

  it("rejects a phone with 4004 when the room was never claimed", () => {
    const p = room.phone();
    expect(p.closed?.code).toBe(CLOSE_NO_TV);
    expect(p.sent).toEqual([]);
  });

  it("rejects a phone with 4004 while the TV is away", () => {
    const tv = room.tv(SECRET_A);
    room.logic.onClose(tv);
    const tv2Sent = tv.sent.length;
    const p = room.phone();
    expect(p.closed?.code).toBe(CLOSE_NO_TV);
    expect(tv.sent.length).toBe(tv2Sent);
  });

  it("ignores messages and the close event of a rejected phone", () => {
    const p = room.phone();
    room.logic.onMessage(p, join("Mama"));
    room.logic.onClose(p);
    const tv = room.tv(SECRET_A);
    expect(tv.sent).toEqual([]);
  });
});

describe("forwarding", () => {
  it("wraps a phone message in a message envelope without changing it", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    const conn = room.lastConn(tv);
    tv.clear();
    const raw = '{ "v":1, "type":"join", "name":"Мама",  "lang":"ru", "x":1.50 }';
    room.logic.onMessage(p, raw);
    expect(tv.sent).toEqual([`{"relay":"message","conn":"${conn}","msg":${raw}}`]);
    expect(JSON.parse(tv.sent[0])).toEqual({ relay: "message", conn, msg: JSON.parse(raw) });
  });

  it("sends a TV `to` message to that phone only, without envelope", () => {
    const tv = room.tv(SECRET_A);
    const p1 = room.phone("p1");
    const c1 = room.lastConn(tv);
    const p2 = room.phone("p2");
    const msg = { v: 1, type: "welcome", playerId: "p1", token: "t", room: "ABCD" };
    room.logic.onMessage(tv, JSON.stringify({ to: c1, msg }));
    expect(p1.json()).toEqual([msg]);
    expect(p2.sent).toEqual([]);
  });

  it("sends a TV `to: all` message to every phone", () => {
    const tv = room.tv(SECRET_A);
    const phones = [room.phone("p1"), room.phone("p2"), room.phone("p3")];
    const msg = { v: 1, type: "screen", phaseId: 3, id: "lobby", endsInMs: null, data: {} };
    room.logic.onMessage(tv, JSON.stringify({ to: "all", msg }));
    for (const p of phones) expect(p.json()).toEqual([msg]);
  });

  it("drops TV messages to an unknown conn, unknown shapes and invalid JSON", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    for (const bad of [
      JSON.stringify({ to: "c_ffffff", msg: { v: 1 } }),
      JSON.stringify({ to: room.lastConn(tv) }),
      JSON.stringify({ to: room.lastConn(tv), msg: "text" }),
      JSON.stringify({ to: room.lastConn(tv), msg: [1, 2] }),
      JSON.stringify({ hello: "world" }),
      JSON.stringify([1, 2, 3]),
      "null",
      "{not json",
    ]) {
      room.logic.onMessage(tv, bad);
    }
    expect(p.sent).toEqual([]);
    expect(p.closed).toBeNull();
    expect(tv.closed).toBeNull();
  });

  it("drops phone messages that are not JSON objects", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    tv.clear();
    for (const bad of ["{oops", "", "42", '"text"', "[1]", "null"]) {
      room.logic.onMessage(p, bad);
    }
    expect(tv.sent).toEqual([]);
    expect(p.closed).toBeNull();
  });

  it("drops binary frames from phones and the TV", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    tv.clear();
    room.logic.onMessage(p, new TextEncoder().encode(join("Mama")).buffer as ArrayBuffer);
    room.logic.onMessage(tv, new TextEncoder().encode(JSON.stringify({ to: "all", msg: {} })).buffer as ArrayBuffer);
    expect(tv.sent).toEqual([]);
    expect(p.sent).toEqual([]);
  });
});

describe("rooms are separate", () => {
  it("a phone in ABCD never gets WXYZ messages, and TV ABCD never gets WXYZ phone messages", async () => {
    const abcd = room;
    const wxyz = await new TestRoom().start();
    const tvA = abcd.tv(SECRET_A, "tvA");
    const tvW = wxyz.tv(SECRET_B, "tvW");
    const phoneA = abcd.phone("phoneA");
    const phoneW = wxyz.phone("phoneW");
    const connW = wxyz.lastConn(tvW);
    tvA.clear();
    tvW.clear();

    // Conn ids are per room, so the WXYZ phone's conn may equal an ABCD conn: target it from TV ABCD.
    wxyz.logic.onMessage(tvW, JSON.stringify({ to: "all", msg: { v: 1, type: "screen", for: "W" } }));
    wxyz.logic.onMessage(tvW, JSON.stringify({ to: connW, msg: { v: 1, type: "ack", for: "W" } }));
    wxyz.logic.onMessage(phoneW, join("Papa"));

    expect(phoneA.sent).toEqual([]);
    expect(tvA.sent).toEqual([]);
    expect(phoneW.sent).toHaveLength(2);
    expect(tvW.sent).toHaveLength(1);

    abcd.logic.onMessage(phoneA, join("Mama"));
    abcd.logic.onMessage(tvA, JSON.stringify({ to: "all", msg: { v: 1, for: "A" } }));
    expect(tvA.sent).toHaveLength(1);
    expect(phoneA.json()).toEqual([{ v: 1, for: "A" }]);
    expect(phoneW.sent).toHaveLength(2);
    expect(tvW.sent).toHaveLength(1);
  });

  it("a socket from another room is ignored by this room", async () => {
    const wxyz = await new TestRoom().start();
    const tvA = room.tv(SECRET_A);
    const tvW = wxyz.tv(SECRET_B);
    const phoneW = wxyz.phone();
    tvA.clear();
    room.logic.onMessage(phoneW, join("Papa"));
    room.logic.onMessage(tvW, JSON.stringify({ to: "all", msg: { v: 1 } }));
    room.logic.onClose(phoneW);
    expect(tvA.sent).toEqual([]);
    expect(phoneW.sent).toEqual([]);
  });
});

describe("TV secret", () => {
  it("claims an unclaimed room and stores the secret", () => {
    const tv = room.tv(SECRET_A);
    expect(tv.closed).toBeNull();
    expect(room.storage.value).toBe(SECRET_A);
  });

  it("closes a second TV with the wrong secret with 4001 and keeps the first", () => {
    const tv1 = room.tv(SECRET_A, "tv1");
    const p = room.phone();
    tv1.clear();
    const tv2 = room.tv(SECRET_B, "tv2");
    expect(tv2.closed?.code).toBe(CLOSE_WRONG_SECRET);
    expect(tv2.sent).toEqual([]);
    expect(tv1.closed).toBeNull();
    expect(p.sent).toEqual([]);

    // The intruder cannot send, and its close does not mark the TV away.
    room.logic.onMessage(tv2, JSON.stringify({ to: "all", msg: { v: 1, evil: true } }));
    room.logic.onClose(tv2);
    expect(p.sent).toEqual([]);
    expect(room.alarm.at).toBeNull();

    room.logic.onMessage(p, join("Mama"));
    expect(tv1.sent).toHaveLength(1);
    expect(room.storage.value).toBe(SECRET_A);
  });

  it("rejects a wrong secret while the TV is away", () => {
    const tv1 = room.tv(SECRET_A, "tv1");
    room.logic.onClose(tv1);
    const tv2 = room.tv(SECRET_B, "tv2");
    expect(tv2.closed?.code).toBe(CLOSE_WRONG_SECRET);
    expect(room.alarm.at).not.toBeNull();
  });

  it("lets the right secret reconnect", () => {
    const tv1 = room.tv(SECRET_A, "tv1");
    room.logic.onClose(tv1);
    const tv2 = room.tv(SECRET_A, "tv2");
    expect(tv2.closed).toBeNull();
    const p = room.phone();
    expect(p.closed).toBeNull();
    expect(tv2.sent).toHaveLength(1);
  });

  it("replaces an older TV socket that is still open; its later close does not mean tvAway", () => {
    const tv1 = room.tv(SECRET_A, "tv1");
    const p = room.phone();
    const tv2 = room.tv(SECRET_A, "tv2");
    expect(tv1.closed?.code).toBe(CLOSE_TV_REPLACED);
    expect(tv2.closed).toBeNull();
    // The new TV is told about the phone that is still open.
    expect(tv2.json()).toEqual([{ relay: "connected", conn: room.lastConn(tv2) }]);

    room.logic.onClose(tv1);
    room.logic.onMessage(tv1, JSON.stringify({ to: "all", msg: { v: 1, old: true } }));
    expect(p.sent).toEqual([]);
    expect(room.alarm.at).toBeNull();

    room.logic.onMessage(p, join("Mama"));
    expect(tv2.sent).toHaveLength(2);
  });

  it("remembers the secret across an eviction of the room object", async () => {
    const tv1 = room.tv(SECRET_A, "tv1");
    room.logic.onClose(tv1);
    const storage = room.storage;
    const after = await new TestRoom(storage, room.clock).start();
    const intruder = after.tv(SECRET_B, "intruder");
    expect(intruder.closed?.code).toBe(CLOSE_WRONG_SECRET);
    const tv2 = after.tv(SECRET_A, "tv2");
    expect(tv2.closed).toBeNull();
  });

  it("cancels the pending room close on every successful TV connect, also after an eviction", async () => {
    const tv1 = room.tv(SECRET_A, "tv1");
    room.logic.onClose(tv1);
    expect(room.alarm.at).toBe(room.clock.t + ROOM_CLOSE_AFTER_TV_AWAY_MS);
    // A fresh instance does not know the TV was away, but must still cancel the alarm.
    const after = await new TestRoom(room.storage, room.clock, room.alarm).start();
    after.tv(SECRET_A, "tv2");
    expect(room.alarm.at).toBeNull();
  });
});

describe("TV away and back", () => {
  it("sends tvAway to every phone when the TV socket closes, and schedules the room close", () => {
    const tv = room.tv(SECRET_A);
    const p1 = room.phone("p1");
    const p2 = room.phone("p2");
    room.logic.onClose(tv);
    expect(p1.json()).toEqual([TV_AWAY]);
    expect(p2.json()).toEqual([TV_AWAY]);
    expect(p1.closed).toBeNull();
    expect(room.alarm.at).toBe(room.clock.t + ROOM_CLOSE_AFTER_TV_AWAY_MS);
  });

  it("treats a TV socket error like a close, once", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    room.logic.onClose(tv);
    room.logic.onClose(tv);
    expect(p.json()).toEqual([TV_AWAY]);
  });

  it("drops phone messages while the TV is away and never delivers them later", () => {
    const tv1 = room.tv(SECRET_A, "tv1");
    const p = room.phone();
    room.logic.onClose(tv1);
    room.logic.onMessage(p, join("Mama"));
    room.logic.onMessage(p, JSON.stringify({ v: 1, type: "ping", t: 5 }));

    const tv2 = room.tv(SECRET_A, "tv2");
    const kinds = (tv2.json() as { relay: string }[]).map((m) => m.relay);
    expect(kinds).toEqual(["connected"]);
    room.advance(PHONE_RATE_WINDOW_MS * 5);
    expect((tv2.json() as { relay: string }[]).map((m) => m.relay)).toEqual(["connected"]);
  });

  it("on reconnect sends the TV one connected per open phone, then tvBack to every phone", () => {
    const order: string[] = [];
    const tv1 = room.tv(SECRET_A, "tv1");
    const phones = [room.phone("p1"), room.phone("p2"), room.phone("p3")];
    const conns = (tv1.json() as { conn: string }[]).map((m) => m.conn);
    room.logic.onClose(phones[1]);
    room.logic.onClose(tv1);
    for (const p of phones) p.clear();

    const tv2 = new FakeSocket("tv2");
    const wrap = (s: FakeSocket) => {
      const send = s.send.bind(s);
      s.send = (text: string) => {
        order.push(`${s.label}:${text}`);
        send(text);
      };
    };
    wrap(tv2);
    phones.forEach(wrap);
    room.logic.tvConnect(tv2, SECRET_A);

    expect(tv2.json()).toEqual([
      { relay: "connected", conn: conns[0] },
      { relay: "connected", conn: conns[2] },
    ]);
    expect(phones[0].json()).toEqual([TV_BACK]);
    expect(phones[1].json()).toEqual([]);
    expect(phones[2].json()).toEqual([TV_BACK]);
    // Every connected reaches the TV before any phone hears tvBack (phones answer tvBack with rejoin).
    const lastConnected = order.map((o) => o.startsWith("tv2:")).lastIndexOf(true);
    const firstBack = order.findIndex((o) => o.includes("tvBack"));
    expect(lastConnected).toBeLessThan(firstBack);

    // Messages flow again.
    room.logic.onMessage(phones[0], join("Mama"));
    expect(tv2.json()[2]).toMatchObject({ relay: "message", conn: conns[0] });
  });

  it("does not send tvBack when the TV replaces a socket that was never away", () => {
    room.tv(SECRET_A, "tv1");
    const p = room.phone();
    room.tv(SECRET_A, "tv2");
    expect(p.sent).toEqual([]);
  });

  it("reports phone disconnects to the TV; not while the TV is away", () => {
    const tv = room.tv(SECRET_A);
    const p1 = room.phone("p1");
    const c1 = room.lastConn(tv);
    const p2 = room.phone("p2");
    tv.clear();
    room.logic.onClose(p1);
    expect(tv.json()).toEqual([{ relay: "disconnected", conn: c1 }]);
    room.logic.onClose(p1);
    expect(tv.sent).toHaveLength(1);

    room.logic.onClose(tv);
    room.logic.onClose(p2);
    const tv2 = room.tv(SECRET_A, "tv2");
    expect(tv2.sent).toEqual([]);
  });
});

describe("close control message", () => {
  it("closes exactly that phone with 4002, forwards nothing, and reports the disconnect", () => {
    const tv = room.tv(SECRET_A);
    const p1 = room.phone("p1");
    const c1 = room.lastConn(tv);
    const p2 = room.phone("p2");
    const p3 = room.phone("p3");
    tv.clear();
    room.logic.onMessage(tv, JSON.stringify({ close: c1, code: CLOSE_REPLACED_BY_TV }));

    expect(p1.closed?.code).toBe(CLOSE_REPLACED_BY_TV);
    expect(p1.sent).toEqual([]);
    expect(p2.closed).toBeNull();
    expect(p3.closed).toBeNull();
    expect(p2.sent).toEqual([]);
    expect(tv.json()).toEqual([{ relay: "disconnected", conn: c1 }]);

    // The closed phone is gone: messages to it and its late events are ignored.
    room.logic.onMessage(tv, JSON.stringify({ to: c1, msg: { v: 1 } }));
    room.logic.onMessage(p1, join("Ghost"));
    room.logic.onClose(p1);
    expect(tv.sent).toHaveLength(1);
    room.logic.onMessage(tv, JSON.stringify({ to: "all", msg: { v: 1 } }));
    expect(p2.sent).toHaveLength(1);
    expect(p3.sent).toHaveLength(1);
  });

  it("drops a close for an unknown conn or with a code a socket cannot carry", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    const c = room.lastConn(tv);
    tv.clear();
    for (const bad of [
      { close: "c_ffffff", code: 4002 },
      { close: c, code: 1006 },
      { close: c, code: 5000 },
      { close: c, code: 4002.5 },
      { close: c, code: "4002" },
      { close: c },
    ]) {
      room.logic.onMessage(tv, JSON.stringify(bad));
    }
    expect(p.closed).toBeNull();
    expect(tv.sent).toEqual([]);
  });
});

describe("size limit", () => {
  /** A phone message of exactly n UTF-8 bytes. */
  const phoneMessageOfBytes = (n: number, char = "a") => {
    const head = '{"v":1,"pad":"';
    const tail = '"}';
    const charBytes = new TextEncoder().encode(char).length;
    const count = (n - head.length - tail.length) / charBytes;
    const text = head + char.repeat(count) + tail;
    expect(new TextEncoder().encode(text).length).toBe(n);
    return text;
  };

  it("forwards a phone message of exactly 8 KB and drops one byte more", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    tv.clear();
    room.logic.onMessage(p, phoneMessageOfBytes(MAX_MESSAGE_BYTES));
    expect(tv.sent).toHaveLength(1);
    room.logic.onMessage(p, phoneMessageOfBytes(MAX_MESSAGE_BYTES + 1));
    expect(tv.sent).toHaveLength(1);
    expect(p.closed).toBeNull();
  });

  it("counts UTF-8 bytes, not characters", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    tv.clear();
    // 2 bytes per Cyrillic letter: under 8192 characters but over 8192 bytes.
    const text = phoneMessageOfBytes(MAX_MESSAGE_BYTES + 2, "я");
    expect(text.length).toBeLessThan(MAX_MESSAGE_BYTES);
    room.logic.onMessage(p, text);
    expect(tv.sent).toEqual([]);
  });

  it("drops a TV message over 8 KB", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    const big = JSON.stringify({ to: "all", msg: { v: 1, pad: "a".repeat(MAX_MESSAGE_BYTES) } });
    room.logic.onMessage(tv, big);
    expect(p.sent).toEqual([]);
    expect(tv.closed).toBeNull();
  });
});

describe("rate limit", () => {
  it("forwards exactly 20 of 25 messages in under a second and keeps the socket open", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    tv.clear();
    for (let i = 0; i < 25; i++) {
      room.logic.onMessage(p, JSON.stringify({ v: 1, type: "ping", t: i }));
      room.advance(10);
    }
    expect(tv.sent).toHaveLength(PHONE_RATE_LIMIT_COUNT);
    expect((tv.json() as { msg: { t: number } }[]).map((m) => m.msg.t)).toEqual(
      Array.from({ length: 20 }, (_, i) => i),
    );
    expect(p.closed).toBeNull();
  });

  it("lets messages flow again once the window has passed", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    tv.clear();
    const first = room.clock.t;
    for (let i = 0; i < 25; i++) room.logic.onMessage(p, JSON.stringify({ v: 1, t: i }));
    expect(tv.sent).toHaveLength(20);

    room.clock.t = first + PHONE_RATE_WINDOW_MS - 1;
    room.logic.onMessage(p, JSON.stringify({ v: 1, t: "early" }));
    expect(tv.sent).toHaveLength(20);

    room.clock.t = first + PHONE_RATE_WINDOW_MS;
    room.logic.onMessage(p, JSON.stringify({ v: 1, t: "late" }));
    expect(tv.sent).toHaveLength(21);
    expect(JSON.parse(tv.sent[20]).msg.t).toBe("late");
  });

  it("is a rolling window: never more than 20 in any second", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    tv.clear();
    const forwardedAt: number[] = [];
    for (let i = 0; i < 300; i++) {
      const before = tv.sent.length;
      room.logic.onMessage(p, JSON.stringify({ v: 1, i }));
      if (tv.sent.length > before) forwardedAt.push(room.clock.t);
      room.advance(7);
    }
    for (const t of forwardedAt) {
      expect(forwardedAt.filter((x) => x >= t && x < t + PHONE_RATE_WINDOW_MS).length).toBeLessThanOrEqual(20);
    }
    expect(forwardedAt.length).toBeGreaterThanOrEqual(40);
  });

  it("counts each phone separately", () => {
    const tv = room.tv(SECRET_A);
    const p1 = room.phone("p1");
    const p2 = room.phone("p2");
    tv.clear();
    for (let i = 0; i < 25; i++) room.logic.onMessage(p1, JSON.stringify({ v: 1, i }));
    for (let i = 0; i < 5; i++) room.logic.onMessage(p2, JSON.stringify({ v: 1, i }));
    expect(tv.sent).toHaveLength(25);
  });
});

describe("room close", () => {
  it("closes every phone with 4004 ten minutes after the TV left, and frees the code", () => {
    const tv = room.tv(SECRET_A);
    const p1 = room.phone("p1");
    const p2 = room.phone("p2");
    room.logic.onClose(tv);

    room.advance(ROOM_CLOSE_AFTER_TV_AWAY_MS - 1);
    expect(p1.closed).toBeNull();

    room.advance(1);
    expect(p1.closed?.code).toBe(CLOSE_NO_TV);
    expect(p2.closed?.code).toBe(CLOSE_NO_TV);
    expect(room.storage.value).toBeNull();
    expect(room.storage.clearCount).toBe(1);

    // A phone now gets 4004; a new TV with a new secret can claim the code.
    expect(room.phone("late").closed?.code).toBe(CLOSE_NO_TV);
    const newTv = room.tv(SECRET_B, "newTv");
    expect(newTv.closed).toBeNull();
    expect(newTv.sent).toEqual([]);
    expect(room.storage.value).toBe(SECRET_B);
    const p3 = room.phone("p3");
    expect(p3.closed).toBeNull();
    expect(p3.sent).toEqual([]);
  });

  it("an alarm that fires while a TV is connected does nothing", () => {
    const tv = room.tv(SECRET_A);
    const p = room.phone();
    room.logic.onAlarm();
    expect(p.closed).toBeNull();
    expect(tv.closed).toBeNull();
    expect(room.storage.value).toBe(SECRET_A);
  });

  it("frees the code after an eviction too (alarm wakes a fresh instance)", async () => {
    const storage = new FakeStorage();
    const first = await new TestRoom(storage).start();
    const tv = first.tv(SECRET_A);
    first.logic.onClose(tv);
    const fresh = await new TestRoom(storage, first.clock).start();
    fresh.logic.onAlarm();
    expect(storage.value).toBeNull();
    expect(fresh.tv(SECRET_B).closed).toBeNull();
  });
});
