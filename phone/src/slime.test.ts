import { describe, expect, it } from 'vitest';
import { SLIME_WIPE_SHARE } from './constants';
import { blobLeft, chordLength, slimeCleared, slimeLayout, slimeStart, swipe } from './slime';

describe('slime layout: deterministic, inside the answer area, not overlapping much', () => {
  it('is the same for the same count and size', () => {
    expect(slimeLayout(3, 360, 600)).toStrictEqual(slimeLayout(3, 360, 600));
  });

  it('places `blobs` blobs', () => {
    expect(slimeLayout(3, 360, 600)).toHaveLength(3);
    expect(slimeLayout(1, 360, 600)).toHaveLength(1);
    expect(slimeLayout(6, 360, 600)).toHaveLength(6);
  });

  for (const [n, w, h] of [[1, 360, 600], [3, 360, 600], [3, 360, 300], [3, 800, 400], [5, 360, 600], [8, 320, 500]]) {
    it(`${n} blobs in ${w} x ${h}: inside the area, overlaps under a fifth of the radii`, () => {
      const blobs = slimeLayout(n, w, h);
      for (const b of blobs) {
        expect(b.r).toBeGreaterThan(0);
        expect(b.x - b.r).toBeGreaterThanOrEqual(0);
        expect(b.x + b.r).toBeLessThanOrEqual(w);
        expect(b.y - b.r).toBeGreaterThanOrEqual(0);
        expect(b.y + b.r).toBeLessThanOrEqual(h);
      }
      for (let i = 0; i < blobs.length; i++) {
        for (let j = i + 1; j < blobs.length; j++) {
          const a = blobs[i];
          const b = blobs[j];
          const overlap = a.r + b.r - Math.hypot(a.x - b.x, a.y - b.y);
          expect(overlap, `${i} and ${j}`).toBeLessThan((a.r + b.r) * 0.2);
        }
      }
    });
  }

  it('3 blobs on a phone answer area cover a good part of it', () => {
    const blobs = slimeLayout(3, 360, 600);
    const covered = blobs.reduce((sum, b) => sum + Math.PI * b.r * b.r, 0);
    expect(covered / (360 * 600)).toBeGreaterThan(0.3);
  });

  it('an empty area places nothing', () => {
    expect(slimeLayout(3, 0, 600)).toStrictEqual([]);
  });
});

describe('slime hit test: the part of a swipe inside a blob', () => {
  const blob = { x: 100, y: 100, r: 50 };

  it('a swipe across the middle crosses the whole diameter', () => {
    expect(chordLength({ x: 0, y: 100 }, { x: 200, y: 100 }, blob)).toBeCloseTo(100);
  });

  it('a swipe that starts inside counts from its start', () => {
    expect(chordLength({ x: 100, y: 100 }, { x: 200, y: 100 }, blob)).toBeCloseTo(50);
  });

  it('a move wholly inside counts its whole length', () => {
    expect(chordLength({ x: 90, y: 100 }, { x: 110, y: 100 }, blob)).toBeCloseTo(20);
  });

  it('a swipe that misses counts nothing', () => {
    expect(chordLength({ x: 0, y: 0 }, { x: 200, y: 0 }, blob)).toBe(0);
    expect(chordLength({ x: 0, y: 100 }, { x: 40, y: 100 }, blob)).toBe(0);
  });

  it('a tap (no movement) counts nothing', () => {
    expect(chordLength({ x: 100, y: 100 }, { x: 100, y: 100 }, blob)).toBe(0);
  });
});

describe('slime: swipe across each blob to wipe it (03 "Power ups")', () => {
  const blobs = slimeLayout(3, 360, 600);

  it('starts with every blob there', () => {
    const s = slimeStart(3);
    expect(s).toStrictEqual([0, 0, 0]);
    expect(blobs.map((_, i) => blobLeft(s, i))).toStrictEqual([1, 1, 1]);
    expect(slimeCleared(s)).toBe(false);
  });

  it('one swipe right through a blob wipes it, and only it', () => {
    const b = blobs[0];
    const s = swipe(blobs, slimeStart(3), { x: b.x - b.r, y: b.y }, { x: b.x + b.r, y: b.y });
    expect(blobLeft(s, 0)).toBe(0);
    expect(blobLeft(s, 1)).toBe(1);
    expect(blobLeft(s, 2)).toBe(1);
  });

  it('short moves add up; the blob fades as it is wiped', () => {
    const b = blobs[1];
    let s = slimeStart(3);
    const step = b.r * 0.1;
    s = swipe(blobs, s, { x: b.x, y: b.y }, { x: b.x + step, y: b.y });
    expect(blobLeft(s, 1)).toBeGreaterThan(0);
    expect(blobLeft(s, 1)).toBeLessThan(1);
    for (let k = 1; k < Math.ceil(SLIME_WIPE_SHARE / 0.1) + 1; k++) {
      s = swipe(blobs, s, { x: b.x + step * k, y: b.y }, { x: b.x + step * (k + 1), y: b.y });
    }
    expect(blobLeft(s, 1)).toBe(0);
  });

  it('a tap on a blob does not wipe it', () => {
    const b = blobs[2];
    const s = swipe(blobs, slimeStart(3), { x: b.x, y: b.y }, { x: b.x, y: b.y });
    expect(blobLeft(s, 2)).toBe(1);
  });

  it('is cleared once every blob is gone', () => {
    let s = slimeStart(3);
    for (const b of blobs) {
      expect(slimeCleared(s)).toBe(false);
      s = swipe(blobs, s, { x: b.x - b.r, y: b.y }, { x: b.x + b.r, y: b.y });
    }
    expect(slimeCleared(s)).toBe(true);
  });

  it('progress is kept as a share of the radius, so a new size (rotation) keeps it', () => {
    const b = blobs[0];
    const s = swipe(blobs, slimeStart(3), { x: b.x - b.r, y: b.y }, { x: b.x + b.r, y: b.y });
    const bigger = slimeLayout(3, 720, 1200);
    expect(blobLeft(s, 0)).toBe(0);
    // The same progress array on the bigger layout: blob 0 is still gone, a swipe on blob 1 still works.
    const s2 = swipe(bigger, s, { x: bigger[1].x - bigger[1].r, y: bigger[1].y }, { x: bigger[1].x + bigger[1].r, y: bigger[1].y });
    expect(blobLeft(s2, 0)).toBe(0);
    expect(blobLeft(s2, 1)).toBe(0);
  });
});
