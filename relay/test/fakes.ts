// Test doubles for RoomLogic: sockets, clock, ids, storage and alarm.

import type { AlarmScheduler, Clock, IdSource, RelaySocket, SecretStore } from "../src/roomLogic";
import { RoomLogic } from "../src/roomLogic";

export class FakeSocket implements RelaySocket {
  readonly sent: string[] = [];
  closed: { code: number; reason: string } | null = null;

  constructor(readonly label: string) {}

  send(text: string): void {
    if (this.closed) {
      throw new Error(`${this.label}: send after close: ${text}`);
    }
    this.sent.push(text);
  }

  close(code: number, reason: string): void {
    if (this.closed) {
      throw new Error(`${this.label}: closed twice`);
    }
    this.closed = { code, reason };
  }

  /** Every sent frame parsed as JSON. */
  json(): unknown[] {
    return this.sent.map((s) => JSON.parse(s));
  }

  /** Forget what was sent so far. */
  clear(): void {
    this.sent.length = 0;
  }
}

export class FakeClock implements Clock {
  constructor(public t = 1_000_000) {}
  now(): number {
    return this.t;
  }
}

/** Deterministic ids; can be told to repeat values to test uniqueness. */
export class FakeIds implements IdSource {
  private n = 0;
  readonly forced: string[] = [];
  randomHex(chars: number): string {
    const next = this.forced.shift();
    if (next !== undefined) return next;
    this.n += 1;
    return this.n.toString(16).padStart(chars, "0").slice(-chars);
  }
}

/** Storage that outlives a RoomLogic instance, like Durable Object storage outlives eviction. */
export class FakeStorage implements SecretStore {
  value: string | null = null;
  clearCount = 0;
  async load(): Promise<string | null> {
    return this.value;
  }
  save(secret: string): void {
    this.value = secret;
  }
  clear(): void {
    this.value = null;
    this.clearCount += 1;
  }
}

export class FakeAlarm implements AlarmScheduler {
  at: number | null = null;
  schedule(atMs: number): void {
    this.at = atMs;
  }
  cancel(): void {
    this.at = null;
  }
}

/** One room with its fakes, plus helpers. */
export class TestRoom {
  readonly clock: FakeClock;
  readonly ids = new FakeIds();
  readonly alarm: FakeAlarm;
  logic!: RoomLogic;

  constructor(readonly storage = new FakeStorage(), clock?: FakeClock, alarm?: FakeAlarm) {
    this.clock = clock ?? new FakeClock();
    this.alarm = alarm ?? new FakeAlarm();
  }

  /** Builds a fresh RoomLogic (also used to simulate a Durable Object eviction). */
  async start(): Promise<this> {
    this.logic = new RoomLogic({
      clock: this.clock,
      ids: this.ids,
      store: this.storage,
      alarm: this.alarm,
    });
    await this.logic.init();
    return this;
  }

  tv(secret: string, label = "tv"): FakeSocket {
    const s = new FakeSocket(label);
    this.logic.tvConnect(s, secret);
    return s;
  }

  phone(label = "phone"): FakeSocket {
    const s = new FakeSocket(label);
    this.logic.phoneConnect(s);
    return s;
  }

  /** The conn id the TV was told about in the last `connected` envelope sent to it. */
  lastConn(tv: FakeSocket): string {
    const msgs = tv.json() as { relay?: string; conn?: string }[];
    for (let i = msgs.length - 1; i >= 0; i--) {
      if (msgs[i].relay === "connected") return msgs[i].conn as string;
    }
    throw new Error("no connected envelope");
  }

  /** Moves the clock forward and fires the alarm if it is due. */
  advance(ms: number): void {
    this.clock.t += ms;
    if (this.alarm.at !== null && this.clock.t >= this.alarm.at) {
      this.alarm.at = null;
      this.logic.onAlarm();
    }
  }
}

export const SECRET_A = "A".repeat(16) + "b".repeat(16);
export const SECRET_B = "Z".repeat(16) + "y".repeat(16);
