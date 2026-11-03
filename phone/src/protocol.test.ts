import { describe, expect, it } from 'vitest';
import {
  answerChoiceData, answerSliderData, answerTrueFalseData, answerWindowMs, characterMessage, choiceInputMessage,
  effectClearedInputMessage, joinMessage, leaveMessage, lobbyData, lobbyMessage, parseIncoming, pickCharacterData,
  pingMessage, playerView, pollData, pollMessage, powerUpInputMessage, rejoinMessage, resultData, resultsData,
  settingsMessage, textData, topicVoteData, trueFalseInputMessage, valueInputMessage, voteInputMessage, waitingData,
} from './protocol';
import { PROTOCOL_VERSION } from './constants';
import { socketUrl } from './net/socketUrl';

describe('message builders match PROTOCOL.md section 3 exactly', () => {
  it('protocol version 2 (PROTOCOL.md section 7, D88)', () => {
    expect(PROTOCOL_VERSION).toBe(2);
  });

  it('join', () => {
    expect(joinMessage('Mama', 'ru')).toStrictEqual({ v: 2, type: 'join', name: 'Mama', lang: 'ru' });
  });

  it('rejoin', () => {
    expect(rejoinMessage('t0k3n')).toStrictEqual({ v: 2, type: 'rejoin', token: 't0k3n' });
  });

  it('input of kind choice', () => {
    const msg = choiceInputMessage('a2', 16, 1, 3410);
    expect(msg).toStrictEqual({ v: 2, type: 'input', inputId: 'a2', phaseId: 16, kind: 'choice', choice: 1, ms: 3410 });
    expect(JSON.stringify(msg)).toBe('{"v":2,"type":"input","inputId":"a2","phaseId":16,"kind":"choice","choice":1,"ms":3410}');
  });

  it('input of kind vote', () => {
    expect(JSON.stringify(voteInputMessage('a1', 14, 2)))
      .toBe('{"v":2,"type":"input","inputId":"a1","phaseId":14,"kind":"vote","door":2}');
  });

  it('input of kind value', () => {
    expect(JSON.stringify(valueInputMessage('a3', 31, 1889, 11200)))
      .toBe('{"v":2,"type":"input","inputId":"a3","phaseId":31,"kind":"value","value":1889,"ms":11200}');
    expect(valueInputMessage('a5', 31, 2.5, 900)).toStrictEqual({
      v: 2, type: 'input', inputId: 'a5', phaseId: 31, kind: 'value', value: 2.5, ms: 900,
    });
  });

  it('input of kind trueFalse', () => {
    expect(JSON.stringify(trueFalseInputMessage('a4', 80, true, 2100)))
      .toBe('{"v":2,"type":"input","inputId":"a4","phaseId":80,"kind":"trueFalse","answer":true,"ms":2100}');
    expect(trueFalseInputMessage('a6', 80, false, 0).answer).toBe(false);
  });

  it('lobby ready', () => {
    expect(lobbyMessage('x1', 'ready')).toStrictEqual({ v: 2, type: 'lobby', inputId: 'x1', action: 'ready' });
    expect(JSON.stringify(lobbyMessage('x1', 'ready'))).toBe('{"v":2,"type":"lobby","inputId":"x1","action":"ready"}');
  });

  it('lobby notReady', () => {
    expect(lobbyMessage('x2', 'notReady')).toStrictEqual({ v: 2, type: 'lobby', inputId: 'x2', action: 'notReady' });
    expect(JSON.stringify(lobbyMessage('x2', 'notReady')))
      .toBe('{"v":2,"type":"lobby","inputId":"x2","action":"notReady"}');
  });

  it('character', () => {
    expect(JSON.stringify(characterMessage('c1', 7))).toBe('{"v":2,"type":"character","inputId":"c1","characterId":7}');
  });

  it('settings has no inputId', () => {
    expect(settingsMessage('en', 1.5)).toStrictEqual({ v: 2, type: 'settings', lang: 'en', timerMultiplier: 1.5 });
  });

  it('leave', () => {
    expect(JSON.stringify(leaveMessage('l1'))).toBe('{"v":2,"type":"leave","inputId":"l1"}');
  });

  it('ping', () => {
    expect(pingMessage(1234)).toStrictEqual({ v: 2, type: 'ping', t: 1234 });
  });

  it('input of kind powerUp: the power up and the target playerId, no ms', () => {
    expect(JSON.stringify(powerUpInputMessage('u1', 22, 'freeze', 'p4')))
      .toBe('{"v":2,"type":"input","inputId":"u1","phaseId":22,"kind":"powerUp","powerUp":"freeze","target":"p4"}');
    expect(powerUpInputMessage('u2', 22, 'fog', 'p1')).toStrictEqual({
      v: 2, type: 'input', inputId: 'u2', phaseId: 22, kind: 'powerUp', powerUp: 'fog', target: 'p1',
    });
  });

  it('input of kind effectCleared: the effect and the time to clear it', () => {
    expect(JSON.stringify(effectClearedInputMessage('e1', 23, 'slime', 2140)))
      .toBe('{"v":2,"type":"input","inputId":"e1","phaseId":23,"kind":"effectCleared","effect":"slime","ms":2140}');
  });

  it('poll: answers by question id, a string for text and an option index for choice', () => {
    expect(JSON.stringify(pollMessage('p9', { fun: 'The freeze on Dad', waiting: 1, again: 0 })))
      .toBe('{"v":2,"type":"poll","inputId":"p9","answers":{"fun":"The freeze on Dad","waiting":1,"again":0}}');
    expect(pollMessage('p8', {})).toStrictEqual({ v: 2, type: 'poll', inputId: 'p8', answers: {} });
  });
});

describe('the effect message (sabotage on this phone)', () => {
  it('parses freeze, slime and fog with their params', () => {
    expect(parseIncoming('{"v":2,"type":"effect","effect":"freeze","from":"Papa","params":{"taps":6}}'))
      .toStrictEqual({ type: 'effect', effect: 'freeze', from: 'Papa', params: { taps: 6, blobs: 3, seconds: 3 } });
    expect(parseIncoming('{"v":2,"type":"effect","effect":"slime","from":"Мама","params":{"blobs":4}}'))
      .toStrictEqual({ type: 'effect', effect: 'slime', from: 'Мама', params: { taps: 6, blobs: 4, seconds: 3 } });
    expect(parseIncoming('{"v":2,"type":"effect","effect":"fog","from":"Bo","params":{"seconds":2.5}}'))
      .toStrictEqual({ type: 'effect', effect: 'fog', from: 'Bo', params: { taps: 6, blobs: 3, seconds: 2.5 } });
  });

  it('falls back to 6 taps, 3 blobs and 3 seconds for missing or broken params', () => {
    expect(parseIncoming('{"v":2,"type":"effect","effect":"freeze"}'))
      .toStrictEqual({ type: 'effect', effect: 'freeze', from: '', params: { taps: 6, blobs: 3, seconds: 3 } });
    expect(parseIncoming('{"v":2,"type":"effect","effect":"slime","from":5,"params":{"taps":0,"blobs":-2,"seconds":"x"}}'))
      .toStrictEqual({ type: 'effect', effect: 'slime', from: '', params: { taps: 6, blobs: 3, seconds: 3 } });
    // Counts are whole numbers.
    expect(parseIncoming('{"v":2,"type":"effect","effect":"freeze","params":{"taps":4.6,"blobs":2.2}}'))
      .toMatchObject({ params: { taps: 5, blobs: 2 } });
  });

  it('ignores an unknown effect, never a crash', () => {
    expect(parseIncoming('{"v":2,"type":"effect","effect":"lava","from":"Papa","params":{}}')).toBeNull();
    expect(parseIncoming('{"v":2,"type":"effect","from":"Papa"}')).toBeNull();
  });
});

describe('incoming messages', () => {
  it('parses welcome, snapshot, screen, ack and pong', () => {
    expect(parseIncoming('{"v":2,"type":"welcome","playerId":"p3","token":"abc","room":"ABCD"}'))
      .toEqual({ type: 'welcome', playerId: 'p3', token: 'abc', room: 'ABCD' });
    const snap = parseIncoming(JSON.stringify({
      v: 2, type: 'snapshot',
      player: { playerId: 'p3', name: 'Mama', characterId: 4 },
      screen: { phaseId: 2, id: 'lobby', endsInMs: null, data: { players: [], ready: false, enoughPlayers: false } },
    }));
    expect(snap).toMatchObject({ type: 'snapshot', screen: { phaseId: 2, id: 'lobby' }, player: { playerId: 'p3' } });
    expect(parseIncoming('{"v":2,"type":"screen","phaseId":5,"id":"lobby","endsInMs":null,"data":{}}'))
      .toEqual({ type: 'screen', phaseId: 5, id: 'lobby', endsInMs: null, data: {} });
    expect(parseIncoming('{"v":2,"type":"ack","inputId":"a1","ok":false,"reason":"invalid"}'))
      .toEqual({ type: 'ack', inputId: 'a1', ok: false, reason: 'invalid' });
    expect(parseIncoming('{"v":2,"type":"pong","t":10,"tv":99}')).toEqual({ type: 'pong', t: 10, tv: 99 });
  });

  it('never crashes on unknown or broken messages', () => {
    expect(parseIncoming('{"v":2,"type":"somethingNew"}')).toBeNull();
    expect(parseIncoming('not json')).toBeNull();
    expect(parseIncoming('[1,2]')).toBeNull();
    expect(parseIncoming('null')).toBeNull();
    expect(parseIncoming(42)).toBeNull();
    expect(parseIncoming('{"type":"screen","id":"lobby"}')).toBeNull();
  });

  it('reads lobby data defensively', () => {
    expect(lobbyData({
      players: [
        { playerId: 'p1', name: 'A', characterId: 2, ready: true },
        { playerId: 'p2', name: 'B', characterId: 5, ready: false },
        'junk',
      ],
      ready: true,
      enoughPlayers: true,
    })).toStrictEqual({
      players: [
        { playerId: 'p1', name: 'A', characterId: 2, ready: true },
        { playerId: 'p2', name: 'B', characterId: 5, ready: false },
      ],
      ready: true,
      enoughPlayers: true,
    });
    expect(lobbyData({})).toStrictEqual({ players: [], ready: false, enoughPlayers: false });
    // Only a real true counts; a missing ready mark is not ready.
    expect(lobbyData({ players: [{ playerId: 'p1', name: 'A', characterId: 2, ready: 'yes' }], ready: 1, enoughPlayers: 'yes' }))
      .toStrictEqual({ players: [{ playerId: 'p1', name: 'A', characterId: 2, ready: false }], ready: false, enoughPlayers: false });
    expect(lobbyData({ players: [{ playerId: 'p1', name: 'A', characterId: 2 }] }).players[0].ready).toBe(false);
  });

  it('parses the player message', () => {
    const msg = parseIncoming(JSON.stringify({
      v: 2, type: 'player',
      player: { playerId: 'p3', name: 'Mama', characterId: 4, lang: 'ru', score: 2350, rank: 2, streak: 1, timerMultiplier: 1.5 },
    }));
    expect(msg).toMatchObject({ type: 'player', player: { playerId: 'p3', score: 2350 } });
  });
});

describe('screen data readers (PROTOCOL.md section 4) never crash', () => {
  it('player view: name, character and timer multiplier; no score or rank on the phone (D88)', () => {
    expect(playerView({ playerId: 'p3', name: 'Mama', characterId: 4, score: 2350, rank: 2, timerMultiplier: 1.5 }))
      .toStrictEqual({ name: 'Mama', characterId: 4, timerMultiplier: 1.5 });
    expect(playerView({ playerId: 'p3', name: 'Mama', characterId: 4, timerMultiplier: 'x' }))
      .toStrictEqual({ name: 'Mama', characterId: 4, timerMultiplier: 1 });
  });

  it('pickCharacter', () => {
    expect(pickCharacterData({ taken: [1, 'x', 5], current: 3 })).toEqual({ taken: [1, 5], current: 3 });
    expect(pickCharacterData({})).toEqual({ taken: [], current: null });
  });

  it('countdown and error text', () => {
    expect(textData({ text: 'Игра начинается' })).toEqual({ text: 'Игра начинается' });
    expect(textData({ text: 5 })).toEqual({ text: '' });
  });

  it('answerChoice: the options and the window, no question (D88)', () => {
    expect(answerChoiceData({ options: ['a', 'b', 'c', 'd'], windowMs: 15000 }))
      .toStrictEqual({ options: ['a', 'b', 'c', 'd'], windowMs: 15000 });
    // A question field from an old TV build is not read.
    expect(answerChoiceData({ question: 'Q', options: ['a', 2, 'c', 'd', 'e'] }))
      .toStrictEqual({ options: ['a', '', 'c', 'd'], windowMs: null });
    expect(answerChoiceData({})).toStrictEqual({ options: [], windowMs: null });
  });

  it('topicVote: door titles in order, at most 4', () => {
    expect(topicVoteData({ doors: [{ title: 'Space' }, { title: 'Food' }, { title: 'Films' }, { title: 'A bit of everything' }] }))
      .toEqual({ doors: ['Space', 'Food', 'Films', 'A bit of everything'] });
    expect(topicVoteData({ doors: [{ title: 'A' }, 'junk', { title: 5 }, { title: 'D' }, { title: 'E' }] }))
      .toEqual({ doors: ['A', '', '', 'D'] });
    expect(topicVoteData({})).toEqual({ doors: [] });
  });

  it('answerSlider: range, step, unit and window, no question (D88)', () => {
    expect(answerSliderData({ min: 1800, max: 2000, step: 10, unit: 'year', windowMs: 20000 }))
      .toStrictEqual({ min: 1800, max: 2000, step: 10, unit: 'year', windowMs: 20000 });
    expect(answerSliderData({ min: 'x', max: null, step: Number.NaN }))
      .toStrictEqual({ min: 0, max: 0, step: 1, unit: '', windowMs: null });
  });

  it('answerTrueFalse: the window only, no statement (D88)', () => {
    expect(answerTrueFalseData({ windowMs: 8000 })).toStrictEqual({ windowMs: 8000 });
    expect(answerTrueFalseData({})).toStrictEqual({ windowMs: null });
  });

  it('the answer window for the countdown bar comes from every answer screen', () => {
    const s = (id: string, data: Record<string, unknown>) => ({ phaseId: 1, id, endsInMs: 5000, data });
    expect(answerWindowMs(s('answerChoice', { windowMs: 15000 }))).toBe(15000);
    expect(answerWindowMs(s('answerSlider', { windowMs: 30000 }))).toBe(30000);
    expect(answerWindowMs(s('answerTrueFalse', { windowMs: 8000 }))).toBe(8000);
    expect(answerWindowMs(s('answerTrueFalse', {}))).toBeNull();
    expect(answerWindowMs(s('topicVote', { windowMs: 10000 }))).toBeNull();
  });

  it('waiting: the locked text only, no pending list (D88)', () => {
    expect(waitingData({ locked: 'B' })).toStrictEqual({ locked: 'B' });
    expect(waitingData({ locked: 'B', pending: [{ name: 'Papa', characterId: 3 }] })).toStrictEqual({ locked: 'B' });
    expect(waitingData({})).toStrictEqual({ locked: '' });
  });

  it('result: the power up and its targets only (Day 4)', () => {
    expect(resultData({ powerUp: 'freeze', targets: [{ playerId: 'p2', name: 'B', available: true }, {}] }))
      .toStrictEqual({ powerUp: 'freeze', targets: [{ playerId: 'p2', name: 'B', available: true }] });
    expect(resultData({ powerUp: 'slime', targets: [{ playerId: 'p3', name: 'C', available: false }, { playerId: 'p4' }] }))
      .toStrictEqual({
        powerUp: 'slime',
        targets: [{ playerId: 'p3', name: 'C', available: false }, { playerId: 'p4', name: '', available: false }],
      });
    expect(resultData({ powerUp: 'fog' }).powerUp).toBe('fog');
    // An unknown power up reads as none: the phone then shows watch.
    expect(resultData({ powerUp: 'lava', targets: [] }).powerUp).toBeNull();
    expect(resultData({})).toStrictEqual({ powerUp: null, targets: [] });
  });

  it('poll: the questions, options only for choice', () => {
    expect(pollData({
      questions: [
        { id: 'fun', text: 'The most fun moment?', kind: 'text' },
        { id: 'time', text: 'Was there enough time?', kind: 'choice', options: ['Too little', 'Just right', 7] },
      ],
    })).toStrictEqual({
      questions: [
        { id: 'fun', text: 'The most fun moment?', kind: 'text', options: [] },
        { id: 'time', text: 'Was there enough time?', kind: 'choice', options: ['Too little', 'Just right', ''] },
      ],
    });
    // A text question with options from a broken file keeps none; junk and unknown kinds are left out.
    expect(pollData({
      questions: [
        { id: 'a', text: 'A', kind: 'text', options: ['x'] },
        { id: 'b', text: 'B', kind: 'rating' },
        { text: 'no id', kind: 'text' },
        'junk',
      ],
    })).toStrictEqual({ questions: [{ id: 'a', text: 'A', kind: 'text', options: [] }] });
    expect(pollData({})).toStrictEqual({ questions: [] });
  });

  it('results: the winner\'s fact (D87)', () => {
    expect(resultsData({ fact: 'Octopuses have three hearts.' })).toStrictEqual({ fact: 'Octopuses have three hearts.' });
    expect(resultsData({ place: 1, fact: 7 })).toStrictEqual({ fact: '' });
    expect(resultsData({})).toStrictEqual({ fact: '' });
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
