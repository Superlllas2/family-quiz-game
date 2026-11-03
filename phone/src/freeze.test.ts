import { describe, expect, it } from 'vitest';
import { freezeStart, iceBroken, iceCracks, iceLeft, tapIce } from './freeze';

describe('freeze: tap the ice to break it (03 "Power ups")', () => {
  it('breaks after exactly `taps` taps', () => {
    let s = freezeStart(6);
    for (let i = 0; i < 5; i++) {
      s = tapIce(s);
      expect(iceBroken(s), `tap ${i + 1}`).toBe(false);
    }
    expect(iceLeft(s)).toBe(1);
    s = tapIce(s);
    expect(iceBroken(s)).toBe(true);
    expect(iceLeft(s)).toBe(0);
  });

  it('counts down the taps left', () => {
    let s = freezeStart(6);
    expect(iceLeft(s)).toBe(6);
    s = tapIce(tapIce(s));
    expect(iceLeft(s)).toBe(4);
  });

  it('more taps after breaking change nothing', () => {
    let s = freezeStart(2);
    for (let i = 0; i < 5; i++) s = tapIce(s);
    expect(iceLeft(s)).toBe(0);
    expect(iceCracks(s)).toBe(1);
  });

  it('cracks grow with every tap, from 0 to 1', () => {
    let s = freezeStart(4);
    expect(iceCracks(s)).toBe(0);
    s = tapIce(s);
    expect(iceCracks(s)).toBe(0.25);
  });

  it('a broken count of taps still needs at least one tap', () => {
    expect(iceLeft(freezeStart(0))).toBe(1);
    expect(iceLeft(freezeStart(Number.NaN))).toBe(1);
  });
});
