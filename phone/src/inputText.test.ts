import { describe, expect, it } from 'vitest';
import { inputStatusKey } from './inputText';
import type { InputState, TrackedInput } from './net/inputs';

const input = (state: InputState, reason: string | null = null): TrackedInput =>
  ({ inputId: 'a1', state, roundTripMs: null, reason, resent: false });

describe('input status text', () => {
  it('sent, acked and no connection', () => {
    expect(inputStatusKey(input('sent'))).toBe('input.sent');
    expect(inputStatusKey(input('acked'))).toBe('input.acked');
    expect(inputStatusKey(input('acked', 'duplicate'))).toBe('input.acked');
    expect(inputStatusKey(input('noConnection'))).toBe('input.noConnection');
  });

  it('refused by reason: closed is time up, characterTaken the pick note, the rest not accepted', () => {
    expect(inputStatusKey(input('refused', 'closed'))).toBe('input.closed');
    expect(inputStatusKey(input('refused', 'characterTaken'))).toBe('pick.taken');
    expect(inputStatusKey(input('refused', 'stale'))).toBe('input.refused');
    expect(inputStatusKey(input('refused', 'invalid'))).toBe('input.refused');
    expect(inputStatusKey(input('refused'))).toBe('input.refused');
  });

  it('power up refusals get their own note: immune and noPowerUp', () => {
    expect(inputStatusKey(input('refused', 'immune'))).toBe('powerUp.immune');
    expect(inputStatusKey(input('refused', 'noPowerUp'))).toBe('powerUp.noPowerUp');
  });
});
