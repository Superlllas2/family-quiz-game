import { describe, expect, it } from 'vitest';
import {
  answerChoiceData, characterMessage, choiceInputMessage, getReadyData, joinMessage, leaveMessage, lobbyData,
  lobbyMessage, parseIncoming, pickCharacterData, pingMessage, playerView, pollDoneData, rejoinMessage, resultData,
  resultsData, settingsMessage, textData, waitingData,
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

  it('lobby cancelStart and playAgain', () => {
    expect(lobbyMessage('x2', 'cancelStart')).toStrictEqual({ v: 1, type: 'lobby', inputId: 'x2', action: 'cancelStart' });
    expect(lobbyMessage('x3', 'playAgain')).toStrictEqual({ v: 1, type: 'lobby', inputId: 'x3', action: 'playAgain' });
  });

  it('character', () => {
    expect(JSON.stringify(characterMessage('c1', 7))).toBe('{"v":1,"type":"character","inputId":"c1","characterId":7}');
  });

  it('settings has no inputId', () => {
    expect(settingsMessage('en', 1.5)).toStrictEqual({ v: 1, type: 'settings', lang: 'en', timerMultiplier: 1.5 });
  });

  it('leave', () => {
    expect(JSON.stringify(leaveMessage('l1'))).toBe('{"v":1,"type":"leave","inputId":"l1"}');
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

  it('parses the player message', () => {
    const msg = parseIncoming(JSON.stringify({
      v: 1, type: 'player',
      player: { playerId: 'p3', name: 'Mama', characterId: 4, lang: 'ru', score: 2350, rank: 2, streak: 1, timerMultiplier: 1.5 },
    }));
    expect(msg).toMatchObject({ type: 'player', player: { playerId: 'p3', score: 2350 } });
  });
});

describe('screen data readers (PROTOCOL.md section 4) never crash', () => {
  it('player view, with defaults for missing fields', () => {
    expect(playerView({ playerId: 'p3', name: 'Mama', characterId: 4, score: 2350, rank: 2, timerMultiplier: 1.5 }))
      .toEqual({ name: 'Mama', characterId: 4, score: 2350, rank: 2, timerMultiplier: 1.5 });
    expect(playerView({ playerId: 'p3', name: 'Mama', characterId: 4, score: 'x' }))
      .toEqual({ name: 'Mama', characterId: 4, score: 0, rank: null, timerMultiplier: 1 });
  });

  it('pickCharacter', () => {
    expect(pickCharacterData({ taken: [1, 'x', 5], current: 3 })).toEqual({ taken: [1, 5], current: 3 });
    expect(pickCharacterData({})).toEqual({ taken: [], current: null });
  });

  it('countdown and error text', () => {
    expect(textData({ text: 'Игра начинается' })).toEqual({ text: 'Игра начинается' });
    expect(textData({ text: 5 })).toEqual({ text: '' });
  });

  it('getReady, with or without prediction', () => {
    expect(getReadyData({ text: 'Q?' })).toEqual({ text: 'Q?', prediction: [] });
    expect(getReadyData({ text: 'Intro', prediction: [{ playerId: 'p1', name: 'A', characterId: 2 }, null] }))
      .toEqual({ text: 'Intro', prediction: [{ playerId: 'p1', name: 'A', characterId: 2 }] });
  });

  it('answerChoice', () => {
    expect(answerChoiceData({ question: 'Q', options: ['a', 'b', 'c', 'd'], windowMs: 15000 }))
      .toEqual({ question: 'Q', options: ['a', 'b', 'c', 'd'], windowMs: 15000 });
    expect(answerChoiceData({ options: ['a', 2, 'c', 'd', 'e'] }))
      .toEqual({ question: '', options: ['a', '', 'c', 'd'], windowMs: null });
    expect(answerChoiceData({})).toEqual({ question: '', options: [], windowMs: null });
  });

  it('waiting', () => {
    expect(waitingData({ locked: 'B', pending: [{ name: 'Papa', characterId: 3 }, 7] }))
      .toEqual({ locked: 'B', pending: [{ name: 'Papa', characterId: 3 }] });
    expect(waitingData({})).toEqual({ locked: '', pending: [] });
  });

  it('result, with powerUp null and no targets (Day 2)', () => {
    expect(resultData({ correct: true, points: 850, correctText: 'Paris', streak: 2, rank: 1, powerUp: null, targets: [] }))
      .toEqual({ correct: true, points: 850, correctText: 'Paris', streak: 2, rank: 1, powerUp: null, targets: [] });
    expect(resultData({})).toEqual({
      correct: false, points: 0, correctText: '', streak: 0, rank: null, powerUp: null, targets: [],
    });
    expect(resultData({ powerUp: 'freeze', targets: [{ playerId: 'p2', name: 'B', available: true }, {}] }).targets)
      .toEqual([{ playerId: 'p2', name: 'B', available: true }]);
  });

  it('results', () => {
    expect(resultsData({ place: 2, correctCount: 3, awards: ['Fastest', 4, ''] }))
      .toEqual({ place: 2, correctCount: 3, awards: ['Fastest'] });
    expect(resultsData({})).toEqual({ place: null, correctCount: 0, awards: [] });
  });

  it('pollDone', () => {
    expect(pollDoneData({ canPlayAgain: true })).toEqual({ canPlayAgain: true });
    expect(pollDoneData({ canPlayAgain: 'yes' })).toEqual({ canPlayAgain: false });
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
