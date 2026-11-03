import { describe, expect, it } from 'vitest';
import { formatValue, sliderGrid, startIndex, stepIndex, valueAt } from './sliderValue';

describe('slider grid (closest value)', () => {
  it('starts in the middle of the range', () => {
    const g = sliderGrid(1800, 2000, 10);
    expect(g.last).toBe(20);
    expect(valueAt(g, startIndex(g))).toBe(1900);
  });

  it('snaps the middle to the step grid', () => {
    const g = sliderGrid(0, 7, 2); // grid 0, 2, 4, 6; the middle 3.5 is nearest to 4
    expect(g.last).toBe(3);
    expect(valueAt(g, startIndex(g))).toBe(4);
    const h = sliderGrid(1, 100, 5); // grid 1, 6, ..., 96; middle 50.5, nearest 51
    expect(valueAt(h, startIndex(h))).toBe(51);
    const tie = sliderGrid(0, 9, 3); // grid 0, 3, 6, 9; middle 4.5, as near to 3 as to 6: the upper one
    expect(valueAt(tie, startIndex(tie))).toBe(6);
  });

  it('every value is min + k * step and inside the range, also when max is off the grid', () => {
    const g = sliderGrid(0, 7, 2);
    expect([0, 1, 2, 3].map((k) => valueAt(g, k))).toEqual([0, 2, 4, 6]);
    expect(valueAt(g, 99)).toBe(6);
    expect(valueAt(g, -5)).toBe(0);
  });

  it('plus and minus move one step and stop at both ends', () => {
    const g = sliderGrid(1800, 2000, 10);
    expect(stepIndex(g, 10, 1)).toBe(11);
    expect(stepIndex(g, 10, -1)).toBe(9);
    expect(stepIndex(g, 20, 1)).toBe(20);
    expect(stepIndex(g, 0, -1)).toBe(0);
    expect(valueAt(g, stepIndex(g, 20, 1))).toBe(2000);
    expect(valueAt(g, stepIndex(g, 0, -1))).toBe(1800);
  });

  it('decimal steps stay exactly on the grid (no 0.30000000000000004)', () => {
    const g = sliderGrid(0, 1, 0.1);
    expect(g.last).toBe(10);
    expect(valueAt(g, 3)).toBe(0.3);
    expect(valueAt(g, 7)).toBe(0.7);
    expect(valueAt(g, startIndex(g))).toBe(0.5);
    const h = sliderGrid(0.5, 3, 0.25);
    expect(valueAt(h, 1)).toBe(0.75);
    expect(valueAt(h, h.last)).toBe(3);
    const negative = sliderGrid(-40, 60, 0.5);
    expect(valueAt(negative, startIndex(negative))).toBe(10);
    expect(valueAt(negative, 1)).toBe(-39.5);
  });

  it('a broken range or step never crashes: step 1, and no range means only min', () => {
    expect(sliderGrid(0, 10, 0).step).toBe(1);
    expect(sliderGrid(0, 10, -2).step).toBe(1);
    const flat = sliderGrid(50, 10, 5);
    expect(flat.last).toBe(0);
    expect(valueAt(flat, startIndex(flat))).toBe(50);
  });
});

describe('number display', () => {
  it('has no thousands separator and at most 2 decimals', () => {
    expect(formatValue(1889)).toBe('1889');
    expect(formatValue(1234567)).toBe('1234567');
    expect(formatValue(2.5)).toBe('2.5');
    expect(formatValue(0.126)).toBe('0.13');
    expect(formatValue(3.1)).toBe('3.1');
    expect(formatValue(-0.001)).toBe('0');
    expect(formatValue(-12.75)).toBe('-12.75');
  });
});
