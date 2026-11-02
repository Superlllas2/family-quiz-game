import { describe, expect, it } from 'vitest';
import { remainingMs, remainingShare, wholeSeconds } from './countdown';

describe('countdown display (endsInMs)', () => {
  it('counts down from the moment the message arrived', () => {
    expect(remainingMs(3000, 1000, 1000)).toBe(3000);
    expect(remainingMs(3000, 1000, 2500)).toBe(1500);
    expect(remainingMs(3000, 1000, 9000)).toBe(0);
  });

  it('has no countdown when endsInMs is null', () => {
    expect(remainingMs(null, 0, 500)).toBeNull();
  });

  it('shows whole seconds, rounded up (3, 2, 1)', () => {
    expect(wholeSeconds(3000)).toBe(3);
    expect(wholeSeconds(2999)).toBe(3);
    expect(wholeSeconds(2001)).toBe(3);
    expect(wholeSeconds(2000)).toBe(2);
    expect(wholeSeconds(1)).toBe(1);
    expect(wholeSeconds(0)).toBe(0);
  });

  it('gives the bar share of the time left, clamped to 0 to 1', () => {
    expect(remainingShare(5000, 10000)).toBe(0.5);
    expect(remainingShare(12000, 10000)).toBe(1);
    expect(remainingShare(0, 10000)).toBe(0);
    expect(remainingShare(100, null)).toBe(1);
  });
});
