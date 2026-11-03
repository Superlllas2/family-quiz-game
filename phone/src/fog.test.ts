import { describe, expect, it } from 'vitest';
import { FOG_RUB_IDLE_MS, FOG_RUB_SPEEDUP } from './constants';
import { fogAdvance, fogCleared, fogOpacity, fogStart, isRubbing } from './fog';

describe('fog: clears by itself in `seconds`, faster while rubbed (03 "Power ups")', () => {
  it('starts fully opaque at the effect\'s arrival', () => {
    const s = fogStart(3, 1000);
    expect(fogOpacity(s)).toBe(1);
    expect(fogCleared(s)).toBe(false);
  });

  it('falls linearly to 0 over the seconds', () => {
    let s = fogStart(3, 1000);
    s = fogAdvance(s, 1750, false);
    expect(fogOpacity(s)).toBeCloseTo(0.75);
    s = fogAdvance(s, 2500, false);
    expect(fogOpacity(s)).toBeCloseTo(0.5);
    s = fogAdvance(s, 3999, false);
    expect(fogCleared(s)).toBe(false);
    s = fogAdvance(s, 4000, false);
    expect(fogOpacity(s)).toBe(0);
    expect(fogCleared(s)).toBe(true);
  });

  it('rubbing counts time 2x', () => {
    expect(FOG_RUB_SPEEDUP).toBe(2);
    let s = fogStart(3, 0);
    s = fogAdvance(s, 750, true);
    expect(fogOpacity(s)).toBeCloseTo(0.5);
    s = fogAdvance(s, 1500, true);
    expect(fogCleared(s)).toBe(true);
  });

  it('rubbing only part of the time: in between', () => {
    let s = fogStart(3, 0);
    s = fogAdvance(s, 1000, true); // 2000 ms done
    s = fogAdvance(s, 1500, false); // 2500 ms done
    expect(fogOpacity(s)).toBeCloseTo(1 / 6);
  });

  it('time never runs backwards and opacity never goes under 0', () => {
    let s = fogStart(3, 1000);
    s = fogAdvance(s, 900, true);
    expect(fogOpacity(s)).toBe(1);
    s = fogAdvance(s, 99999, false);
    expect(fogOpacity(s)).toBe(0);
  });

  it('a start drawn later (after a reload of the overlay) counts the time since arrival', () => {
    const s = fogAdvance(fogStart(3, 1000), 2500, false);
    expect(fogOpacity(s)).toBeCloseTo(0.5);
  });

  it('broken seconds clear at once rather than never', () => {
    expect(fogCleared(fogAdvance(fogStart(0, 0), 0, false))).toBe(true);
    expect(fogCleared(fogAdvance(fogStart(Number.NaN, 0), 0, false))).toBe(true);
  });
});

describe('rubbing: a finger down and moving', () => {
  it('needs the finger down', () => {
    expect(isRubbing(false, 100, 110)).toBe(false);
  });

  it('needs a move within the idle gap', () => {
    expect(isRubbing(true, 100, 100 + FOG_RUB_IDLE_MS)).toBe(true);
    expect(isRubbing(true, 100, 101 + FOG_RUB_IDLE_MS)).toBe(false);
    expect(isRubbing(true, null, 100)).toBe(false);
  });
});
