// The phone page against what the real TV app sends: fixtures/tvCapture.json holds every frame three scripted phones
// received in a whole Day 2 game (tools/e2e_game.mjs --dump, TV build and live relay). Each frame must parse, and
// each screen reader must return the TV's own values, not its defaults: a field name or shape mismatch between the
// TV app and the phone page fails here.

import { describe, expect, it } from 'vitest';
import capture from './fixtures/tvCapture.json';
import {
  answerChoiceData,
  answerSliderData,
  answerTrueFalseData,
  getReadyData,
  lobbyData,
  parseIncoming,
  pickCharacterData,
  playerView,
  raceResultData,
  resultData,
  resultsData,
  roundEndData,
  textData,
  topicVoteData,
  waitingData,
  type IncomingMessage,
  type ScreenState,
} from './protocol';

const frames = capture as string[];
const raw = frames.map((f) => JSON.parse(f) as Record<string, unknown>);
const parsed = frames.map((f) => parseIncoming(f));

function screens(id: string): ScreenState[] {
  const out: ScreenState[] = [];
  for (const m of parsed) {
    if (m?.type === 'screen' && m.id === id) out.push(m);
    if (m?.type === 'snapshot' && m.screen?.id === id) out.push(m.screen);
  }
  return out;
}

describe('the phone page parses every frame the TV app sent in a whole game', () => {
  it('parses every frame', () => {
    frames.forEach((f, i) => expect(parsed[i], f).not.toBeNull());
  });

  it('has every Day 2 screen in the capture', () => {
    for (const id of ['pickCharacter', 'lobby', 'countdown', 'getReady', 'answerChoice', 'waiting', 'result', 'results']) {
      expect(screens(id).length, id).toBeGreaterThan(0);
    }
  });

  it('reads the player object from snapshot and player messages', () => {
    raw.forEach((m, i) => {
      if (m.type !== 'player' && m.type !== 'snapshot') return;
      const p = (m.player ?? null) as Record<string, unknown> | null;
      expect(p, frames[i]).not.toBeNull();
      const msg = parsed[i] as IncomingMessage;
      const info = msg.type === 'player' ? msg.player : msg.type === 'snapshot' ? msg.player : null;
      expect(info).not.toBeNull();
      const view = playerView(info!);
      expect(view.name).toBe(p!.name);
      expect(view.characterId).toBe(p!.characterId);
      expect(view.score).toBe(p!.score);
      expect(view.rank).toBe(p!.rank);
      expect(view.timerMultiplier).toBe(p!.timerMultiplier);
    });
  });

  it('reads each screen with the values the TV sent', () => {
    for (const s of screens('pickCharacter')) {
      const d = pickCharacterData(s.data);
      expect(d.taken).toEqual(s.data.taken);
      expect(d.current).toBe(s.data.current);
    }
    for (const s of screens('lobby')) {
      const d = lobbyData(s.data);
      expect(d.players).toEqual(s.data.players);
      expect(d.ready).toBe(s.data.ready);
      expect(d.enoughPlayers).toBe(s.data.enoughPlayers);
    }
    for (const s of [...screens('countdown'), ...screens('getReady')]) {
      expect(textData(s.data).text).toBe(s.data.text);
      expect(textData(s.data).text).not.toBe('');
      expect(s.endsInMs).toBeGreaterThan(0);
    }
    for (const s of screens('answerChoice')) {
      const d = answerChoiceData(s.data);
      expect(d.question).toBe(s.data.question);
      expect(d.options).toEqual(s.data.options);
      expect(d.options).toHaveLength(4);
      expect(d.windowMs).toBe(s.data.windowMs);
    }
    for (const s of screens('waiting')) {
      const d = waitingData(s.data);
      expect(d.locked).toBe(s.data.locked);
      expect(d.locked).not.toBe('');
      expect(d.pending).toEqual(s.data.pending);
    }
    for (const s of screens('result')) {
      const d = resultData(s.data);
      expect(d.correct).toBe(s.data.correct);
      expect(d.points).toBe(s.data.points);
      expect(d.correctText).toBe(s.data.correctText);
      expect(d.streak).toBe(s.data.streak);
      expect(d.rank).toBe(s.data.rank);
      expect(d.powerUp).toBeNull();
      expect(d.targets).toEqual([]);
    }
    for (const s of screens('results')) {
      const d = resultsData(s.data);
      expect(d.place).toBe(s.data.place);
      expect(d.correctCount).toBe(s.data.correctCount);
      expect(d.awards).toEqual(s.data.awards);
    }
    // Day 3 screens: checked once the fixture is re-captured from a Day 3 TV build (none in a Day 2 capture).
    for (const s of screens('getReady')) {
      const raw = (s.data.prediction ?? []) as unknown[];
      expect(getReadyData(s.data).prediction).toEqual(raw);
    }
    for (const s of screens('topicVote')) {
      const d = topicVoteData(s.data);
      expect(d.doors).toEqual((s.data.doors as Array<{ title: string }>).map((x) => x.title));
      expect(d.doors).toHaveLength(4);
    }
    for (const s of screens('answerSlider')) {
      const d = answerSliderData(s.data);
      expect(d).toEqual({
        question: s.data.question, min: s.data.min, max: s.data.max, step: s.data.step, unit: s.data.unit,
        windowMs: s.data.windowMs,
      });
      expect(d.max).toBeGreaterThan(d.min);
      expect(d.step).toBeGreaterThan(0);
    }
    for (const s of screens('answerTrueFalse')) {
      const d = answerTrueFalseData(s.data);
      expect(d.statement).toBe(s.data.statement);
      expect(d.statement).not.toBe('');
      expect(d.windowMs).toBe(s.data.windowMs);
    }
    for (const s of screens('roundEnd')) {
      const d = roundEndData(s.data);
      expect(d.rank).toBe(s.data.rank);
      expect(d.score).toBe(s.data.score);
      expect(d.nextRound).toBe(s.data.nextRound ?? null);
    }
    for (const s of screens('raceResult')) {
      const d = raceResultData(s.data);
      expect(d).toEqual({ correct: s.data.correct, position: s.data.position, finish: s.data.finish });
    }
    for (const s of screens('pollDone')) {
      expect(s.data).toEqual({}); // D76: no Play again, so no canPlayAgain
    }
  });

  it('reads every ack', () => {
    raw.forEach((m, i) => {
      if (m.type !== 'ack') return;
      const a = parsed[i] as IncomingMessage;
      expect(a.type).toBe('ack');
      if (a.type === 'ack') {
        expect(a.inputId).toBe(m.inputId);
        expect(a.ok).toBe(m.ok);
        if (m.ok === false) expect(a.reason).toBe(m.reason);
      }
    });
  });
});
