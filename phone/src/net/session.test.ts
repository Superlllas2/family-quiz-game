import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { TokenStore, type KeyValueStore } from '../storage';
import { Session, type SessionStatus, type SocketLike } from './session';
import { globalTimers } from './timers';

class FakeSocket implements SocketLike {
  readyState = 0;
  sent: Record<string, unknown>[] = [];
  closedByClient = false;
  onopen: ((ev: unknown) => void) | null = null;
  onmessage: ((ev: { data: unknown }) => void) | null = null;
  onclose: ((ev: { code: number }) => void) | null = null;
  onerror: ((ev: unknown) => void) | null = null;

  constructor(readonly url: string) {}

  send(data: string): void {
    if (this.readyState !== 1) throw new Error('not open');
    this.sent.push(JSON.parse(data));
  }

  close(): void {
    this.closedByClient = true;
    this.readyState = 3;
  }

  // ---- test controls ----
  open(): void {
    this.readyState = 1;
    this.onopen?.({});
  }

  receive(msg: Record<string, unknown>): void {
    this.onmessage?.({ data: JSON.stringify({ v: 2, ...msg }) });
  }

  serverClose(code: number): void {
    this.readyState = 3;
    this.onclose?.({ code });
  }

  types(): unknown[] {
    return this.sent.map((m) => m.type);
  }
}

function memoryStore(): KeyValueStore {
  const data = new Map<string, string>();
  return {
    getItem: (k) => data.get(k) ?? null,
    setItem: (k, v) => void data.set(k, v),
    removeItem: (k) => void data.delete(k),
  };
}

describe('session: connection and reconnects (PROTOCOL.md sections 2, 4, 5)', () => {
  let sockets: FakeSocket[];
  let tokens: TokenStore;
  let statuses: SessionStatus[];
  let session: Session;

  const lastSocket = () => sockets[sockets.length - 1];
  const status = () => session.status;

  let sharedStore: KeyValueStore;

  function makeSession(tab: string | null = null): Session {
    sharedStore = memoryStore();
    tokens = new TokenStore(sharedStore, tab, memoryStore());
    const s = new Session({
      createSocket: (url) => {
        const sock = new FakeSocket(url);
        sockets.push(sock);
        return sock;
      },
      urlFor: (room) => `wss://relay.test/ws/phone?room=${room}`,
      tokens,
      timers: globalTimers(() => Date.now()),
    });
    s.subscribe({ status: (st) => statuses.push(st) });
    return s;
  }

  beforeEach(() => {
    vi.useFakeTimers();
    sockets = [];
    statuses = [];
    session = makeSession();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  /** Joins and gets welcomed; returns the socket. */
  function joinAndWelcome(token = 'tok123'): FakeSocket {
    session.start('ABCD');
    session.join('ABCD', 'Mama', 'ru');
    const sock = lastSocket();
    sock.open();
    sock.receive({ type: 'welcome', playerId: 'p1', token, room: 'ABCD' });
    return sock;
  }

  it('without a token, load shows join and opens no socket', () => {
    session.start('ABCD');
    expect(status()).toEqual({ kind: 'join', problem: null });
    expect(sockets).toHaveLength(0);
  });

  it('join opens the socket, sends join, and welcome stores the token', () => {
    session.start('ABCD');
    session.join('ABCD', 'Mama', 'en');
    expect(status().kind).toBe('joining');
    expect(lastSocket().url).toBe('wss://relay.test/ws/phone?room=ABCD');
    lastSocket().open();
    expect(lastSocket().sent).toEqual([{ v: 2, type: 'join', name: 'Mama', lang: 'en' }]);
    lastSocket().receive({ type: 'welcome', playerId: 'p1', token: 'tok123', room: 'ABCD' });
    expect(status().kind).toBe('online');
    expect(tokens.get('ABCD')).toBe('tok123');
  });

  it('the token goes under the tab key when the page has &tab=<n>', () => {
    const store = memoryStore();
    tokens = new TokenStore(store, '2');
    session = new Session({
      createSocket: (url) => {
        const sock = new FakeSocket(url);
        sockets.push(sock);
        return sock;
      },
      urlFor: (room) => room,
      tokens,
      timers: globalTimers(() => Date.now()),
    });
    joinAndWelcome('tabtok');
    expect(store.getItem('fq.token.ABCD.2')).toBe('tabtok');
    expect(store.getItem('fq.token.ABCD')).toBeNull();
  });

  it('on load with a stored token: opens a socket and sends rejoin on open', () => {
    tokens.set('ABCD', 'tok123');
    session.start('ABCD');
    expect(status().kind).toBe('reconnecting');
    expect(sockets).toHaveLength(1);
    lastSocket().open();
    expect(lastSocket().sent).toEqual([{ v: 2, type: 'rejoin', token: 'tok123' }]);
    lastSocket().receive({ type: 'welcome', playerId: 'p1', token: 'tok123', room: 'ABCD' });
    expect(status().kind).toBe('online');
  });

  it('retries every 2 s after the socket closes, then rejoins', () => {
    joinAndWelcome();
    lastSocket().serverClose(1006);
    expect(status().kind).toBe('reconnecting');
    expect(sockets).toHaveLength(1);
    vi.advanceTimersByTime(1999);
    expect(sockets).toHaveLength(1);
    vi.advanceTimersByTime(1);
    expect(sockets).toHaveLength(2);
    // That attempt fails too: another retry 2 s later.
    lastSocket().serverClose(1006);
    vi.advanceTimersByTime(2000);
    expect(sockets).toHaveLength(3);
    lastSocket().open();
    expect(lastSocket().types()).toEqual(['rejoin']);
    lastSocket().receive({ type: 'welcome', playerId: 'p1', token: 'tok123', room: 'ABCD' });
    expect(status().kind).toBe('online');
    vi.advanceTimersByTime(10000);
    expect(sockets).toHaveLength(3);
  });

  it('a rejoin with no answer within 2 s replaces the socket', () => {
    tokens.set('ABCD', 'tok123');
    session.start('ABCD');
    lastSocket().open();
    vi.advanceTimersByTime(2000);
    expect(sockets).toHaveLength(2);
    expect(sockets[0].closedByClient).toBe(true);
    expect(status().kind).toBe('reconnecting');
  });

  it('visibilitychange to visible sends rejoin on the open socket', () => {
    const sock = joinAndWelcome();
    session.onVisible();
    expect(sock.types()).toEqual(['join', 'rejoin']);
    expect(status().kind).toBe('online');
  });

  it('visibilitychange to visible with a dead socket opens a new one', () => {
    const sock = joinAndWelcome();
    sock.readyState = 3; // the browser killed it while asleep, no close event yet
    session.onVisible();
    expect(sockets).toHaveLength(2);
    expect(status().kind).toBe('reconnecting');
  });

  it('tvAway shows reconnecting and keeps the socket; tvBack sends rejoin', () => {
    const sock = joinAndWelcome();
    sock.receive({ type: 'tvAway' });
    expect(status().kind).toBe('reconnecting');
    expect(sock.closedByClient).toBe(false);
    vi.advanceTimersByTime(10000);
    expect(sockets).toHaveLength(1); // no reconnect loop while the TV is away
    session.onVisible();
    const noPings = () => sock.sent.filter((m) => m.type !== 'ping');
    expect(noPings().map((m) => m.type)).toEqual(['join']); // no rejoin while the TV is away
    sock.receive({ type: 'tvBack' });
    expect(noPings()).toEqual([
      { v: 2, type: 'join', name: 'Mama', lang: 'ru' },
      { v: 2, type: 'rejoin', token: 'tok123' },
    ]);
    sock.receive({ type: 'welcome', playerId: 'p1', token: 'tok123', room: 'ABCD' });
    expect(status().kind).toBe('online');
  });

  it('close code 4002 shows otherTab and stops every reconnect path until Back', () => {
    const sock = joinAndWelcome();
    sock.serverClose(4002);
    expect(status()).toEqual({ kind: 'error', reason: 'otherTab' });
    vi.advanceTimersByTime(60000);
    session.onVisible();
    session.resume('tvBack');
    session.resume('close');
    session.join('ABCD', 'Mama', 'ru');
    expect(sockets).toHaveLength(1);
    expect(status()).toEqual({ kind: 'error', reason: 'otherTab' });
    // This tab forgets the seat (D67); the shared store keeps it for the newer tab.
    expect(tokens.get('ABCD')).toBeNull();
    expect(sharedStore.getItem('fq.token.ABCD')).toBe('tok123');
    session.back();
    expect(status()).toEqual({ kind: 'join', problem: null });
    expect(sockets).toHaveLength(1); // Back alone does not reconnect
  });

  it('after 4002 and Back, nothing rejoins the old seat; a fresh join gets a new one', () => {
    joinAndWelcome();
    lastSocket().serverClose(4002);
    session.back();
    session.onVisible();
    session.resume('tvBack');
    vi.advanceTimersByTime(10000);
    expect(sockets).toHaveLength(1);
    session.join('ABCD', 'Mama2', 'ru');
    const sock = lastSocket();
    sock.open();
    expect(sock.types()).toEqual(['join']);
    sock.receive({ type: 'welcome', playerId: 'p2', token: 'tok456', room: 'ABCD' });
    expect(tokens.get('ABCD')).toBe('tok456');
  });

  it('close code 4004 with a token: reconnecting, retry every 2 s', () => {
    const sock = joinAndWelcome();
    sock.serverClose(4004);
    expect(status().kind).toBe('reconnecting');
    vi.advanceTimersByTime(2000);
    expect(sockets).toHaveLength(2);
    lastSocket().serverClose(4004);
    expect(status().kind).toBe('reconnecting');
    vi.advanceTimersByTime(2000);
    expect(sockets).toHaveLength(3);
  });

  it('close code 4004 without a token: room not found, no retry', () => {
    session.start('WXYZ');
    session.join('WXYZ', 'Mama', 'ru');
    lastSocket().serverClose(4004);
    expect(status()).toEqual({ kind: 'error', reason: 'roomNotFound' });
    vi.advanceTimersByTime(10000);
    expect(sockets).toHaveLength(1);
    session.back();
    expect(status()).toEqual({ kind: 'join', problem: null });
  });

  it('refused badToken deletes the token and shows join', () => {
    tokens.set('ABCD', 'old');
    session.start('ABCD');
    lastSocket().open();
    lastSocket().receive({ type: 'refused', reason: 'badToken' });
    expect(tokens.get('ABCD')).toBeNull();
    expect(status()).toEqual({ kind: 'join', problem: null });
    vi.advanceTimersByTime(10000);
    expect(sockets).toHaveLength(1);
  });

  it.each(['roomFull', 'nameTaken', 'badName', 'gameStarted', 'badVersion'] as const)(
    'refused %s shows the error screen; Back returns to join',
    (reason) => {
      session.start('ABCD');
      session.join('ABCD', 'Mama', 'ru');
      lastSocket().open();
      lastSocket().receive({ type: 'refused', reason });
      expect(status()).toEqual({ kind: 'error', reason });
      session.back();
      expect(status()).toEqual({ kind: 'join', problem: null });
      // Joining again reuses the open socket.
      session.join('ABCD', 'Papa', 'ru');
      expect(sockets).toHaveLength(1);
      expect(lastSocket().sent[1]).toEqual({ v: 2, type: 'join', name: 'Papa', lang: 'ru' });
    },
  );

  it('a join with no answer in 3 s shows no connection on the join screen', () => {
    session.start('ABCD');
    session.join('ABCD', 'Mama', 'ru');
    lastSocket().open();
    vi.advanceTimersByTime(3000);
    expect(status()).toEqual({ kind: 'join', problem: 'noConnection' });
  });

  it('sends ping every 5 s while connected, and stops after close', () => {
    const sock = joinAndWelcome();
    vi.advanceTimersByTime(5000);
    expect(sock.sent[sock.sent.length - 1]).toMatchObject({ v: 2, type: 'ping' });
    expect(typeof sock.sent[sock.sent.length - 1].t).toBe('number');
    vi.advanceTimersByTime(5000);
    expect(sock.types().filter((x) => x === 'ping')).toHaveLength(2);
    sock.serverClose(4002);
    const count = sock.sent.length;
    vi.advanceTimersByTime(20000);
    expect(sock.sent).toHaveLength(count);
  });

  it('leave deletes the token, closes the socket and shows join; nothing reconnects', () => {
    const sock = joinAndWelcome();
    session.leave();
    expect(tokens.get('ABCD')).toBeNull();
    expect(sharedStore.getItem('fq.token.ABCD')).toBeNull();
    expect(sock.closedByClient).toBe(true);
    expect(status()).toEqual({ kind: 'join', problem: null });
    expect(session.currentRoom).toBe('ABCD');
    session.onVisible();
    session.resume('tvBack');
    vi.advanceTimersByTime(10000);
    expect(sockets).toHaveLength(1);
    // A fresh join opens a new socket and sends join, not rejoin.
    session.join('ABCD', 'Mama', 'ru');
    lastSocket().open();
    expect(sockets).toHaveLength(2);
    expect(lastSocket().types()).toEqual(['join']);
  });

  it('leave with &tab=<n> deletes only the token of that tab', () => {
    const store = memoryStore();
    store.setItem('fq.token.ABCD', 'other-tab-seat');
    tokens = new TokenStore(store, '2');
    session = new Session({
      createSocket: (url) => {
        const sock = new FakeSocket(url);
        sockets.push(sock);
        return sock;
      },
      urlFor: (room) => room,
      tokens,
      timers: globalTimers(() => Date.now()),
    });
    joinAndWelcome('tabtok');
    expect(store.getItem('fq.token.ABCD.2')).toBe('tabtok');
    session.leave();
    expect(store.getItem('fq.token.ABCD.2')).toBeNull();
    expect(store.getItem('fq.token.ABCD')).toBe('other-tab-seat');
  });

  it('passes TV messages on and ignores unknown ones without crashing', () => {
    const got: unknown[] = [];
    session.subscribe({ message: (m) => got.push(m.type) });
    const sock = joinAndWelcome();
    sock.onmessage?.({ data: 'garbage' });
    sock.receive({ type: 'effect', effect: 'freeze' });
    sock.receive({ type: 'screen', phaseId: 3, id: 'lobby', endsInMs: null, data: {} });
    sock.receive({ type: 'ack', inputId: 'a1', ok: true });
    sock.receive({ type: 'pong', t: 1 });
    expect(got).toEqual(['welcome', 'screen', 'ack']);
    expect(status().kind).toBe('online');
  });
});
