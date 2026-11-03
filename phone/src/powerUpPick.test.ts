import { describe, expect, it } from 'vitest';
import type { InputState, TrackedInput } from './net/inputs';
import { addAttempt, armPick, canTarget, pickFor, pickView, powerUpNameKey } from './powerUpPick';
import type { PowerUpTarget } from './protocol';

const tracked = (inputId: string, state: InputState, reason: string | null = null): TrackedInput =>
  ({ inputId, state, roundTripMs: null, reason, resent: false });
const target = (playerId: string, available = true): PowerUpTarget => ({ playerId, name: playerId, available });

describe('the power up pick on `result`: two taps (03 "Using")', () => {
  it('starts not armed, nothing sent', () => {
    const s = pickFor(null, 30);
    expect(s).toStrictEqual({ phaseId: 30, armed: false, attempts: [] });
    expect(pickView(s, {})).toStrictEqual({ stage: 'open', last: null, lastTarget: null, immune: [] });
  });

  it('tapping the card arms it; only then can a target be tapped', () => {
    const s = pickFor(null, 30);
    expect(canTarget(s, {}, target('p2'))).toBe(false);
    const armed = armPick(s);
    expect(armed.armed).toBe(true);
    expect(canTarget(armed, {}, target('p2'))).toBe(true);
  });

  it('a target with available false cannot be tapped', () => {
    const s = armPick(pickFor(null, 30));
    expect(canTarget(s, {}, target('p2', false))).toBe(false);
  });

  it('a refreshed screen with the same phaseId keeps the armed state; another phaseId starts over', () => {
    const s = armPick(pickFor(null, 30));
    expect(pickFor(s, 30)).toBe(s);
    expect(pickFor(s, 31)).toStrictEqual({ phaseId: 31, armed: false, attempts: [] });
  });

  it('one send per phaseId: after a send no target can be tapped while waiting or once acked', () => {
    let s = armPick(pickFor(null, 30));
    s = addAttempt(s, 'u1', 'p2');
    expect(pickView(s, {}).stage).toBe('sending');
    expect(canTarget(s, {}, target('p3'))).toBe(false);
    const sent = { u1: tracked('u1', 'sent') };
    expect(pickView(s, sent)).toMatchObject({ stage: 'sending', lastTarget: 'p2' });
    expect(canTarget(s, sent, target('p3'))).toBe(false);
    const acked = { u1: tracked('u1', 'acked') };
    expect(pickView(s, acked).stage).toBe('done');
    expect(canTarget(s, acked, target('p3'))).toBe(false);
  });

  it('immune: the player may pick again, but not the same target', () => {
    let s = armPick(pickFor(null, 30));
    s = addAttempt(s, 'u1', 'p2');
    const inputs = { u1: tracked('u1', 'refused', 'immune') };
    const view = pickView(s, inputs);
    expect(view.stage).toBe('open');
    expect(view.immune).toStrictEqual(['p2']);
    expect(view.last?.reason).toBe('immune');
    expect(canTarget(s, inputs, target('p2'))).toBe(false);
    expect(canTarget(s, inputs, target('p3'))).toBe(true);
    // The second pick goes through.
    s = addAttempt(s, 'u2', 'p3');
    const all = { ...inputs, u2: tracked('u2', 'acked') };
    expect(pickView(s, all)).toMatchObject({ stage: 'done', lastTarget: 'p3', immune: ['p2'] });
  });

  it('noPowerUp stops: no more picks on this phaseId', () => {
    let s = armPick(pickFor(null, 30));
    s = addAttempt(s, 'u1', 'p2');
    const inputs = { u1: tracked('u1', 'refused', 'noPowerUp') };
    expect(pickView(s, inputs).stage).toBe('stopped');
    expect(canTarget(s, inputs, target('p3'))).toBe(false);
  });

  it('any other refusal or no connection stops too', () => {
    for (const input of [tracked('u1', 'refused', 'closed'), tracked('u1', 'refused', 'stale'), tracked('u1', 'noConnection')]) {
      const s = addAttempt(armPick(pickFor(null, 30)), 'u1', 'p2');
      expect(pickView(s, { u1: input }).stage, `${input.state} ${input.reason}`).toBe('stopped');
    }
  });

  it('a target tap ignored by canTarget never adds an attempt (the app checks before sending)', () => {
    const s = pickFor(null, 30);
    expect(canTarget(s, {}, target('p2'))).toBe(false);
    expect(s.attempts).toHaveLength(0);
  });

  it('names the power up in the player\'s language', () => {
    expect(powerUpNameKey('freeze')).toBe('powerUp.freeze');
    expect(powerUpNameKey('slime')).toBe('powerUp.slime');
    expect(powerUpNameKey('fog')).toBe('powerUp.fog');
  });
});
