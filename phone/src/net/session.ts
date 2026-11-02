// The connection and reconnect state machine (PROTOCOL.md sections 2, 4 and 5). Plain TypeScript, no DOM:
// the socket factory, token store and timers are injected.
//
// Rules it implements:
// - join opens the socket and sends `join`; `welcome` stores the token.
// - On load, on visibilitychange to visible, when the socket closes and on `tvBack`: if a token is stored,
//   open a socket if needed and send `rejoin`. While disconnected: status `reconnecting`, retry every 2 s.
//   A `rejoin` with no `welcome` or `refused` within 2 s counts as a lost connection (the socket is replaced).
// - `tvAway`: status `reconnecting`, keep the socket, wait for `tvBack`.
// - Close code 4002: error `otherTab`, and no reconnect of any kind until back().
// - Close code 4004: with a token, a lost connection (the TV may be restarting); without one, error `roomNotFound`.
// - `refused` `badToken`: delete the token, show join. Other reasons: error screen until back().

import { NO_CONNECTION_MS, PING_INTERVAL_MS, RETRY_MS } from '../constants';
import {
  joinMessage, parseIncoming, pingMessage, rejoinMessage,
  type IncomingMessage, type Lang, type PhoneMessage,
} from '../protocol';
import type { TokenStore } from '../storage';
import type { TimerHandle, Timers } from './timers';

export const CLOSE_OTHER_TAB = 4002;
export const CLOSE_NO_ROOM = 4004;

const CONNECTING = 0;
const OPEN = 1;

/** The part of the browser WebSocket the session uses. */
export interface SocketLike {
  readonly readyState: number;
  send(data: string): void;
  close(code?: number, reason?: string): void;
  onopen: ((ev: unknown) => void) | null;
  onmessage: ((ev: { data: unknown }) => void) | null;
  onclose: ((ev: { code: number }) => void) | null;
  onerror: ((ev: unknown) => void) | null;
}

export type ErrorReason =
  | 'otherTab' | 'roomNotFound'
  | 'roomFull' | 'nameTaken' | 'badName' | 'gameStarted' | 'badVersion' | 'unknown';

export type SessionStatus =
  | { kind: 'join'; problem: 'noConnection' | null }
  | { kind: 'joining' }
  | { kind: 'online' }
  | { kind: 'reconnecting' }
  | { kind: 'error'; reason: ErrorReason };

export type RejoinTrigger = 'load' | 'visible' | 'close' | 'tvBack' | 'retry';

export interface SessionDeps {
  createSocket(url: string): SocketLike;
  urlFor(room: string): string;
  tokens: TokenStore;
  timers: Timers;
  log?: (...args: unknown[]) => void;
}

export interface SessionListener {
  status?(status: SessionStatus): void;
  /** Every TV message except the ones the session consumes on its own (tvAway, tvBack, pong). */
  message?(msg: IncomingMessage): void;
}

const REFUSED_ERRORS: readonly string[] = ['roomFull', 'nameTaken', 'badName', 'gameStarted', 'badVersion'];

export class Session {
  private socket: SocketLike | null = null;
  private room = '';
  private statusValue: SessionStatus = { kind: 'join', problem: null };
  private readonly listeners = new Set<SessionListener>();

  /** 4002 received: nothing reconnects until back(). */
  private blocked = false;
  /** An error screen is showing: no automatic reconnect until back(). */
  private suspended = false;
  /** The relay said the TV app is away; the socket stays open until tvBack. */
  private tvAway = false;
  private pendingJoin: { name: string; lang: Lang } | null = null;
  private awaitingWelcome = false;

  private retryTimer: TimerHandle | null = null;
  private welcomeTimer: TimerHandle | null = null;
  private joinTimer: TimerHandle | null = null;
  private pingTimer: TimerHandle | null = null;

  constructor(private readonly deps: SessionDeps) {}

  get status(): SessionStatus {
    return this.statusValue;
  }

  get currentRoom(): string {
    return this.room;
  }

  subscribe(listener: SessionListener): () => void {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  }

  /** Page load. Rejoins at once if this page holds a token for the room. */
  start(room: string): void {
    this.room = room;
    if (this.deps.tokens.get(room)) this.resume('load');
    else this.setStatus({ kind: 'join', problem: null });
  }

  /** The Join button. */
  join(room: string, name: string, lang: Lang): void {
    if (this.blocked) return;
    if (room !== this.room) {
      this.dropSocket();
      this.room = room;
    }
    this.suspended = false;
    this.pendingJoin = { name, lang };
    this.setStatus({ kind: 'joining' });
    this.clearTimer('joinTimer');
    this.joinTimer = this.deps.timers.setTimeout(() => {
      this.joinTimer = null;
      if (!this.pendingJoin) return;
      this.pendingJoin = null;
      this.setStatus({ kind: 'join', problem: 'noConnection' });
    }, NO_CONNECTION_MS);

    const s = this.liveSocket();
    if (!s) this.connect();
    else if (s.readyState === OPEN) this.sendRaw(joinMessage(name, lang));
    // CONNECTING: onOpen sends the join.
  }

  /** The page became visible again. */
  onVisible(): void {
    this.resume('visible');
  }

  /** The Back button on the error screen. */
  back(): void {
    this.blocked = false;
    this.suspended = false;
    this.pendingJoin = null;
    this.clearTimer('joinTimer');
    this.setStatus({ kind: 'join', problem: null });
  }

  /** Sends a message if the socket is open. Returns false if it could not be sent. */
  send(msg: PhoneMessage): boolean {
    return this.sendRaw(msg);
  }

  /** Rejoin if a token is stored: open a socket if needed and send `rejoin`. */
  resume(trigger: RejoinTrigger): void {
    if (this.blocked || this.suspended || this.pendingJoin) return;
    if (!this.deps.tokens.get(this.room)) return;

    const s = this.liveSocket();
    if (s && this.tvAway) {
      // The relay drops messages while the TV app is away; tvBack will trigger the rejoin.
      this.setStatus({ kind: 'reconnecting' });
      return;
    }
    if (!s) {
      this.setStatus({ kind: 'reconnecting' });
      this.connect();
      return;
    }
    if (s.readyState === CONNECTING) {
      if (this.statusValue.kind !== 'online') this.setStatus({ kind: 'reconnecting' });
      return; // onOpen sends the rejoin.
    }
    this.deps.log?.('rejoin', trigger);
    this.sendRejoin();
  }

  // ---- socket ----

  /** The current socket if it is connecting or open; drops a closing or closed one. */
  private liveSocket(): SocketLike | null {
    const s = this.socket;
    if (!s) return null;
    if (s.readyState === CONNECTING || s.readyState === OPEN) return s;
    this.dropSocket();
    return null;
  }

  private connect(): void {
    if (this.socket) return;
    let s: SocketLike;
    try {
      s = this.deps.createSocket(this.deps.urlFor(this.room));
    } catch (e) {
      this.deps.log?.('socket failed', e);
      this.onClose(1006);
      return;
    }
    this.socket = s;
    s.onopen = () => {
      if (this.socket === s) this.onOpen();
    };
    s.onmessage = (ev) => {
      if (this.socket === s) this.onText(ev.data);
    };
    s.onclose = (ev) => {
      if (this.socket !== s) return;
      this.socket = null;
      this.onClose(ev.code);
    };
    s.onerror = () => { /* a close event follows */ };
  }

  /** Forgets the current socket without running the close handling. */
  private dropSocket(): void {
    const s = this.socket;
    this.socket = null;
    this.stopPing();
    this.clearTimer('welcomeTimer');
    this.awaitingWelcome = false;
    this.tvAway = false;
    if (!s) return;
    s.onopen = s.onmessage = s.onclose = s.onerror = null;
    try {
      s.close();
    } catch { /* already closed */ }
  }

  private onOpen(): void {
    this.clearTimer('retryTimer');
    this.startPing();
    if (this.pendingJoin) {
      this.sendRaw(joinMessage(this.pendingJoin.name, this.pendingJoin.lang));
    } else if (!this.blocked && !this.suspended && this.deps.tokens.get(this.room)) {
      this.sendRejoin();
    }
  }

  private onClose(code: number): void {
    this.deps.log?.('socket closed', code);
    this.stopPing();
    this.clearTimer('welcomeTimer');
    this.awaitingWelcome = false;
    this.tvAway = false;

    if (code === CLOSE_OTHER_TAB) {
      this.blocked = true;
      this.pendingJoin = null;
      this.clearTimer('joinTimer');
      this.clearTimer('retryTimer');
      this.setStatus({ kind: 'error', reason: 'otherTab' });
      return;
    }
    if (this.blocked) return;

    if (this.pendingJoin) {
      this.pendingJoin = null;
      this.clearTimer('joinTimer');
      if (code === CLOSE_NO_ROOM) this.showError('roomNotFound');
      else this.setStatus({ kind: 'join', problem: 'noConnection' });
      return;
    }
    if (this.suspended) return;

    if (this.deps.tokens.get(this.room)) {
      this.setStatus({ kind: 'reconnecting' });
      this.scheduleRetry();
    } else if (code === CLOSE_NO_ROOM && this.statusValue.kind !== 'join') {
      this.showError('roomNotFound');
    }
  }

  private scheduleRetry(): void {
    if (this.retryTimer !== null) return;
    this.retryTimer = this.deps.timers.setTimeout(() => {
      this.retryTimer = null;
      this.resume('retry');
    }, RETRY_MS);
  }

  private sendRejoin(): void {
    const token = this.deps.tokens.get(this.room);
    if (!token) return;
    this.awaitingWelcome = true;
    this.sendRaw(rejoinMessage(token));
    this.clearTimer('welcomeTimer');
    this.welcomeTimer = this.deps.timers.setTimeout(() => {
      this.welcomeTimer = null;
      if (!this.awaitingWelcome || this.tvAway || this.blocked || this.suspended) return;
      // No answer to the rejoin: the socket may look open but be dead (phone woke from sleep). Replace it.
      this.deps.log?.('no welcome, replacing the socket');
      this.dropSocket();
      this.setStatus({ kind: 'reconnecting' });
      this.connect();
    }, RETRY_MS);
  }

  private sendRaw(msg: PhoneMessage): boolean {
    const s = this.socket;
    if (!s || s.readyState !== OPEN) return false;
    try {
      s.send(JSON.stringify(msg));
      return true;
    } catch {
      return false;
    }
  }

  private startPing(): void {
    this.stopPing();
    this.pingTimer = this.deps.timers.setInterval(() => {
      this.sendRaw(pingMessage(Math.round(this.deps.timers.now())));
    }, PING_INTERVAL_MS);
  }

  private stopPing(): void {
    if (this.pingTimer !== null) {
      this.deps.timers.clearInterval(this.pingTimer);
      this.pingTimer = null;
    }
  }

  // ---- messages ----

  private onText(data: unknown): void {
    const msg = parseIncoming(data);
    if (!msg) {
      this.deps.log?.('ignored message', data);
      return;
    }
    switch (msg.type) {
      case 'welcome':
        this.onWelcome(msg.token, msg.room);
        break;
      case 'refused':
        this.onRefused(msg.reason);
        break;
      case 'tvAway':
        this.onTvAway();
        return;
      case 'tvBack':
        this.tvAway = false;
        this.resume('tvBack');
        return;
      case 'pong':
        this.deps.log?.('pong, round trip ms', Math.round(this.deps.timers.now() - msg.t));
        return;
      default:
        break;
    }
    for (const l of this.listeners) l.message?.(msg);
  }

  private onWelcome(token: string, room: string): void {
    const r = room ? room.toUpperCase() : this.room;
    this.room = r;
    this.deps.tokens.set(r, token);
    this.pendingJoin = null;
    this.awaitingWelcome = false;
    this.tvAway = false;
    this.suspended = false;
    this.clearTimer('joinTimer');
    this.clearTimer('welcomeTimer');
    this.clearTimer('retryTimer');
    this.setStatus({ kind: 'online' });
  }

  private onRefused(reason: string): void {
    this.pendingJoin = null;
    this.awaitingWelcome = false;
    this.clearTimer('joinTimer');
    this.clearTimer('welcomeTimer');
    this.clearTimer('retryTimer');
    if (reason === 'badToken') {
      this.deps.tokens.remove(this.room);
      this.setStatus({ kind: 'join', problem: null });
      return;
    }
    this.showError(REFUSED_ERRORS.includes(reason) ? (reason as ErrorReason) : 'unknown');
  }

  private onTvAway(): void {
    this.tvAway = true;
    this.awaitingWelcome = false;
    this.clearTimer('welcomeTimer');
    if (this.pendingJoin) {
      // The join is dropped by the relay while the TV app is away.
      this.pendingJoin = null;
      this.clearTimer('joinTimer');
      this.setStatus({ kind: 'join', problem: 'noConnection' });
      return;
    }
    if (this.blocked || this.suspended) return;
    if (this.deps.tokens.get(this.room)) this.setStatus({ kind: 'reconnecting' });
  }

  // ---- helpers ----

  private showError(reason: ErrorReason): void {
    this.suspended = true;
    this.clearTimer('retryTimer');
    this.setStatus({ kind: 'error', reason });
  }

  private clearTimer(name: 'retryTimer' | 'welcomeTimer' | 'joinTimer'): void {
    const h = this[name];
    if (h !== null) {
      this.deps.timers.clearTimeout(h);
      this[name] = null;
    }
  }

  private setStatus(status: SessionStatus): void {
    const prev = this.statusValue;
    this.statusValue = status;
    if (sameStatus(prev, status)) return;
    for (const l of this.listeners) l.status?.(status);
  }
}

function sameStatus(a: SessionStatus, b: SessionStatus): boolean {
  if (a.kind !== b.kind) return false;
  if (a.kind === 'join' && b.kind === 'join') return a.problem === b.problem;
  if (a.kind === 'error' && b.kind === 'error') return a.reason === b.reason;
  return true;
}
