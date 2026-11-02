// Everything one room does, as plain TypeScript (docs/PROTOCOL.md, section 2).
// The Durable Object in room.ts is a thin adapter around this class; tests drive it with fakes.
// The relay knows rooms and connections, nothing about quizzes: no game logic here.

import {
  CLOSE_NO_TV,
  CLOSE_TV_REPLACED,
  CLOSE_WRONG_SECRET,
  CONN_ID_HEX_CHARS,
  CONN_ID_PREFIX,
  MAX_MESSAGE_BYTES,
  MSG_TV_AWAY,
  MSG_TV_BACK,
  PHONE_RATE_LIMIT_COUNT,
  PHONE_RATE_WINDOW_MS,
  ROOM_CLOSE_AFTER_TV_AWAY_MS,
} from "./protocol";

/** One accepted WebSocket, as the room sees it. */
export interface RelaySocket {
  send(text: string): void;
  close(code: number, reason: string): void;
}

export interface Clock {
  /** Milliseconds; only differences matter. */
  now(): number;
}

export interface IdSource {
  /** `chars` random lowercase hex characters. */
  randomHex(chars: number): string;
}

/** Durable storage for the room's claimed TV secret. */
export interface SecretStore {
  load(): Promise<string | null>;
  save(secret: string): void;
  /** Deletes everything the room stored. */
  clear(): void;
}

/** One pending wake up per room (the room close). */
export interface AlarmScheduler {
  schedule(atMs: number): void;
  cancel(): void;
}

export interface RoomDeps {
  clock: Clock;
  ids: IdSource;
  store: SecretStore;
  alarm: AlarmScheduler;
}

/** A frame as it arrives from a socket: text, or binary (always dropped: the protocol is text only). */
export type Frame = string | ArrayBuffer | ArrayBufferView;

interface Phone {
  conn: string;
  socket: RelaySocket;
  /** Times of the frames accepted by the rate limiter in the current rolling window, oldest first. */
  recent: number[];
}

const utf8 = new TextEncoder();

function byteLength(text: string): number {
  // Every UTF-16 unit is at most 3 UTF-8 bytes, so short strings need no encoding.
  if (text.length * 3 <= MAX_MESSAGE_BYTES) return text.length;
  return utf8.encode(text).length;
}

function isPlainObject(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

/** Close codes a WebSocket endpoint may send: 1000, or the registered/private range 3000 to 4999. */
function isSendableCloseCode(code: unknown): code is number {
  return Number.isInteger(code) && (code === 1000 || ((code as number) >= 3000 && (code as number) <= 4999));
}

export class RoomLogic {
  private readonly deps: RoomDeps;
  private initialized = false;
  /** The secret that claimed this room, or null if the room is free. */
  private secret: string | null = null;
  /** The TV app socket, or null when no TV is connected. */
  private tv: RelaySocket | null = null;
  private readonly phonesBySocket = new Map<RelaySocket, Phone>();
  private readonly phonesByConn = new Map<string, Phone>();
  /** Conn ids handed out since the room opened, so a closed phone's id is never reused. */
  private readonly usedConns = new Set<string>();

  constructor(deps: RoomDeps) {
    this.deps = deps;
  }

  /** Loads the persisted secret. Call once before any other method. */
  async init(): Promise<void> {
    this.secret = await this.deps.store.load();
    this.initialized = true;
  }

  /** A TV app socket was accepted. Returns true if it is now the room's TV. */
  tvConnect(socket: RelaySocket, secret: string): boolean {
    this.assertInitialized();
    if (this.secret === null) {
      this.secret = secret;
      this.deps.store.save(secret);
    } else if (this.secret !== secret) {
      socket.close(CLOSE_WRONG_SECRET, "wrong secret");
      return false;
    }

    // Always cancel: after an eviction this instance may not know a room close is pending.
    this.deps.alarm.cancel();

    const old = this.tv;
    const wasAway = old === null && this.phonesBySocket.size > 0;
    this.tv = socket;
    if (old !== null) {
      // Forget the old socket first, so its close event is not taken for the TV leaving.
      old.close(CLOSE_TV_REPLACED, "replaced by a newer TV connection");
    }

    // The TV hears about every open phone before any phone hears tvBack (phones answer it with rejoin).
    for (const phone of this.phonesBySocket.values()) {
      socket.send(connectedEnvelope(phone.conn));
    }
    if (wasAway) {
      for (const phone of this.phonesBySocket.values()) {
        phone.socket.send(MSG_TV_BACK);
      }
    }
    return true;
  }

  /** A phone socket was accepted. Returns its conn id, or null if it was rejected with 4004. */
  phoneConnect(socket: RelaySocket): string | null {
    this.assertInitialized();
    if (this.tv === null) {
      socket.close(CLOSE_NO_TV, "no TV in this room");
      return null;
    }
    const conn = this.newConn();
    const phone: Phone = { conn, socket, recent: [] };
    this.phonesBySocket.set(socket, phone);
    this.phonesByConn.set(conn, phone);
    this.tv.send(connectedEnvelope(conn));
    return conn;
  }

  /** A frame arrived on a socket of this room. */
  onMessage(socket: RelaySocket, frame: Frame): void {
    if (socket === this.tv) {
      this.onTvMessage(frame);
      return;
    }
    const phone = this.phonesBySocket.get(socket);
    if (phone !== undefined) {
      this.onPhoneMessage(phone, frame);
    }
    // Anything else (a rejected or replaced socket) is ignored.
  }

  /** A socket of this room closed or failed. Safe to call more than once. */
  onClose(socket: RelaySocket): void {
    if (socket === this.tv) {
      this.tv = null;
      for (const phone of this.phonesBySocket.values()) {
        phone.socket.send(MSG_TV_AWAY);
      }
      this.deps.alarm.schedule(this.deps.clock.now() + ROOM_CLOSE_AFTER_TV_AWAY_MS);
      return;
    }
    const phone = this.phonesBySocket.get(socket);
    if (phone !== undefined) {
      this.removePhone(phone);
    }
  }

  /** The room close alarm fired. */
  onAlarm(): void {
    this.assertInitialized();
    if (this.tv !== null) {
      return;
    }
    const phones = [...this.phonesBySocket.values()];
    this.phonesBySocket.clear();
    this.phonesByConn.clear();
    this.usedConns.clear();
    for (const phone of phones) {
      phone.socket.close(CLOSE_NO_TV, "room closed");
    }
    this.secret = null;
    this.deps.store.clear();
  }

  private onTvMessage(frame: Frame): void {
    if (typeof frame !== "string" || byteLength(frame) > MAX_MESSAGE_BYTES) return;
    const parsed = tryParse(frame);
    if (!isPlainObject(parsed)) return;

    if ("to" in parsed) {
      const { to, msg } = parsed;
      if (typeof to !== "string" || !isPlainObject(msg)) return;
      const text = JSON.stringify(msg);
      if (to === "all") {
        for (const phone of this.phonesBySocket.values()) phone.socket.send(text);
        return;
      }
      this.phonesByConn.get(to)?.socket.send(text);
      return;
    }

    if ("close" in parsed) {
      const { close: conn, code } = parsed;
      if (typeof conn !== "string" || !isSendableCloseCode(code)) return;
      const phone = this.phonesByConn.get(conn);
      if (phone === undefined) return;
      phone.socket.close(code, "closed by the TV app");
      this.removePhone(phone);
      return;
    }
    // Unknown shape: dropped.
  }

  private onPhoneMessage(phone: Phone, frame: Frame): void {
    if (!this.allowByRate(phone)) return;
    if (typeof frame !== "string" || byteLength(frame) > MAX_MESSAGE_BYTES) return;
    if (!isPlainObject(tryParse(frame))) return;
    if (this.tv === null) return; // never buffered
    // Built around the raw text so the inner message reaches the TV byte for byte unchanged.
    this.tv.send(`{"relay":"message","conn":"${phone.conn}","msg":${frame}}`);
  }

  /** Rolling window over the frames accepted so far; dropped frames do not count. */
  private allowByRate(phone: Phone): boolean {
    const now = this.deps.clock.now();
    const recent = phone.recent;
    while (recent.length > 0 && recent[0] <= now - PHONE_RATE_WINDOW_MS) recent.shift();
    if (recent.length >= PHONE_RATE_LIMIT_COUNT) return false;
    recent.push(now);
    return true;
  }

  private removePhone(phone: Phone): void {
    this.phonesBySocket.delete(phone.socket);
    this.phonesByConn.delete(phone.conn);
    this.tv?.send(`{"relay":"disconnected","conn":"${phone.conn}"}`);
  }

  private newConn(): string {
    for (;;) {
      const conn = CONN_ID_PREFIX + this.deps.ids.randomHex(CONN_ID_HEX_CHARS);
      if (!this.usedConns.has(conn)) {
        this.usedConns.add(conn);
        return conn;
      }
    }
  }

  private assertInitialized(): void {
    if (!this.initialized) throw new Error("RoomLogic.init() must finish first");
  }
}

function connectedEnvelope(conn: string): string {
  return `{"relay":"connected","conn":"${conn}"}`;
}

function tryParse(text: string): unknown {
  try {
    return JSON.parse(text);
  } catch {
    return undefined;
  }
}
