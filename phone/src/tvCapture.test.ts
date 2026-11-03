// The phone page against what the real TV app sends: fixtures/tvCapture.json holds every frame three scripted phones
// received in a whole game (tools/e2e_game.mjs --dump, TV build and live relay). Each frame must parse, and each
// screen reader must return the TV's own values, not its defaults: a field name or shape mismatch between the TV app
// and the phone page fails here.
//
// The capture is still from a protocol version 1 game. Version 2 (D88) removed the `getReady`, `roundEnd` and
// `raceResult` screens, the question, statement and pending fields, and changed `result` (power up only) and `results`
// (the winner's fact only). Those are skipped here; the fields that did not change are still checked. The fixture
// will be recaptured from a real version 2 game, and then `watch` and the new `results` are checked too.

import { describe, expect, it } from 'vitest';
import capture from './fixtures/tvCapture.json';
import {
  answerChoiceData,
  answerSliderData,
  answerTrueFalseData,
  lobbyData,
  parseIncoming,
  pickCharacterData,
  playerView,
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

  it('has every screen that version 2 kept in the capture', () => {
    const kept = [
      'pickCharacter', 'lobby', 'countdown', 'topicVote', 'answerChoice', 'answerSlider', 'answerTrueFalse', 'waiting',
    ];
    for (const id of kept) {
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
    for (const s of screens('countdown')) {
      expect(textData(s.data).text).toBe(s.data.text);
      expect(textData(s.data).text).not.toBe('');
      expect(s.endsInMs).toBeGreaterThan(0);
    }
    for (const s of screens('topicVote')) {
      const d = topicVoteData(s.data);
      expect(d.doors).toEqual((s.data.doors as Array<{ title: string }>).map((x) => x.title));
      expect(d.doors).toHaveLength(4);
    }
    // Answer screens: only the fields version 2 kept (the version 1 question and statement are not read).
    for (const s of screens('answerChoice')) {
      const d = answerChoiceData(s.data);
      expect(d.options).toEqual(s.data.options);
      expect(d.options).toHaveLength(4);
      expect(d.windowMs).toBe(s.data.windowMs);
    }
    for (const s of screens('answerSlider')) {
      const d = answerSliderData(s.data);
      expect(d).toEqual({
        min: s.data.min, max: s.data.max, step: s.data.step, unit: s.data.unit, windowMs: s.data.windowMs,
      });
      expect(d.max).toBeGreaterThan(d.min);
      expect(d.step).toBeGreaterThan(0);
    }
    for (const s of screens('answerTrueFalse')) {
      const d = answerTrueFalseData(s.data);
      expect(d.windowMs).toBe(s.data.windowMs);
      expect(d.windowMs).toBeGreaterThan(0);
    }
    // `waiting`: the locked text (the version 1 pending list is not read).
    for (const s of screens('waiting')) {
      const d = waitingData(s.data);
      expect(d.locked).toBe(s.data.locked);
      expect(d.locked).not.toBe('');
    }
    for (const s of screens('pollDone')) {
      expect(s.data).toEqual({}); // D76: no Play again, so no canPlayAgain
    }
    // Not checked until the recapture: `getReady`, `roundEnd`, `raceResult` (gone in version 2), `result` and
    // `results` (their version 1 fields are gone), `watch` (new in version 2).
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
