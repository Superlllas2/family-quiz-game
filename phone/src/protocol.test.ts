import { describe, expect, it } from 'vitest';
import {
  choiceInputMessage, joinMessage, lobbyData, lobbyMessage, parseIncoming, pingMessage, rejoinMessage,
} from './protocol';
import { socketUrl } from './net/socketUrl';

describe('message builders match PROTOCOL.md section 3 exactly', () => {
  it('join', () => {
    expect(joinMessage('Mama', 'ru')).toStrictEqual({ v: 1, type: 'join', name: 'Mama', lang: 'ru' });
  });

  it('rejoin', () => {
    expect(rejoinMessage('t0k3n')).toStrictEqual({ v: 1, type: 'rejoin', token: 't0k3n' });
  });

  it('input of kind choice', () => {
    const msg = choiceInputMessage('a2', 16, 1, 3410);
    expect(msg).toStrictEqual({ v: 1, type: 'input', inputId: 'a2', phaseId: 16, kind: 'choice', choice: 1, ms: 3410 });
    expect(JSON.stringify(msg)).toBe('{"v":1,"type":"input","inputId":"a2","phaseId":16,"kind":"choice","choice":1,"ms":3410}');
  });

  it('lobby start', () => {
    expect(lobbyMessage('x1', 'start')).toStrictEqual({ v: 1, type: 'lobby', inputId: 'x1', action: 'start' });
  });

  it('ping', () => {
    expect(pingMessage(1234)).toStrictEqual({ v: 1, type: 'ping', t: 1234 });
  });
});

describe('incoming messages', () => {
  it('parses welcome, snapshot, screen, ack and pong', () => {
    expect(parseIncoming('{"v":1,"type":"welcome","playerId":"p3","token":"abc","room":"ABCD"}'))
      .toEqual({ type: 'welcome', playerId: 'p3', token: 'abc', room: 'ABCD' });
    const snap = parseIncoming(JSON.stringify({
      v: 1, type: 'snapshot',
      player: { playerId: 'p3', name: 'Mama', characterId: 4 },
      screen: { phaseId: 2, id: 'lobby', endsInMs: null, data: { players: [], canStart: false } },
    }));
    expect(snap).toMatchObject({ type: 'snapshot', screen: { phaseId: 2, id: 'lobby' }, player: { playerId: 'p3' } });
    expect(parseIncoming('{"v":1,"type":"screen","phaseId":5,"id":"lobby","endsInMs":null,"data":{}}'))
      .toEqual({ type: 'screen', phaseId: 5, id: 'lobby', endsInMs: null, data: {} });
    expect(parseIncoming('{"v":1,"type":"ack","inputId":"a1","ok":false,"reason":"invalid"}'))
      .toEqual({ type: 'ack', inputId: 'a1', ok: false, reason: 'invalid' });
    expect(parseIncoming('{"v":1,"type":"pong","t":10,"tv":99}')).toEqual({ type: 'pong', t: 10, tv: 99 });
  });

  it('never crashes on unknown or broken messages', () => {
    expect(parseIncoming('{"v":1,"type":"effect","effect":"freeze"}')).toBeNull();
    expect(parseIncoming('not json')).toBeNull();
    expect(parseIncoming('[1,2]')).toBeNull();
    expect(parseIncoming('null')).toBeNull();
    expect(parseIncoming(42)).toBeNull();
    expect(parseIncoming('{"type":"screen","id":"lobby"}')).toBeNull();
  });

  it('reads lobby data defensively', () => {
    expect(lobbyData({ players: [{ playerId: 'p1', name: 'A', characterId: 2 }, 'junk'], canStart: true }))
      .toEqual({ players: [{ playerId: 'p1', name: 'A', characterId: 2 }], canStart: true });
    expect(lobbyData({})).toEqual({ players: [], canStart: false });
  });
});

describe('socket URL', () => {
  it('builds wss from an https page and ws from an http page', () => {
    expect(socketUrl({ protocol: 'https:', host: 'quiz.example.dev' }, 'ABCD'))
      .toBe('wss://quiz.example.dev/ws/phone?room=ABCD');
    expect(socketUrl({ protocol: 'http:', host: '192.168.1.5:8787' }, 'ABCD'))
      .toBe('ws://192.168.1.5:8787/ws/phone?room=ABCD');
  });

  it('VITE_RELAY_HOST overrides the host, keeping its scheme, or wss for a bare host', () => {
    const page = { protocol: 'http:', host: 'localhost:5173' };
    expect(socketUrl(page, 'ABCD', 'http://localhost:8787')).toBe('ws://localhost:8787/ws/phone?room=ABCD');
    expect(socketUrl(page, 'ABCD', 'https://relay.example.dev/')).toBe('wss://relay.example.dev/ws/phone?room=ABCD');
    expect(socketUrl(page, 'ABCD', 'relay.example.dev')).toBe('wss://relay.example.dev/ws/phone?room=ABCD');
    expect(socketUrl(page, 'ABCD', '')).toBe('ws://localhost:5173/ws/phone?room=ABCD');
  });
});
