// Slime (03 "Power ups"): `blobs` blobs (`powerUps.slime.blobs`, sent in the `effect` params) cover the answer area;
// a finger that is down and moves through a blob wipes it. The layout is deterministic for a count and an area size.
// Progress is kept per blob as a share of its radius, so a new size (a rotation) keeps what was already wiped.
// A blob is gone once the swipes through it add up to SLIME_WIPE_SHARE of its radius: a tap alone never wipes.

import { SLIME_WIPE_SHARE } from './constants';

export interface Point { x: number; y: number }
export interface Blob { x: number; y: number; r: number }

/** Progress per blob: how far swipes went through it, in radii. */
export type SlimeProgress = readonly number[];

const EPS = 1e-9;

/**
 * Places `count` blobs in a `width` x `height` area (px). They zigzag along the long side, at 30% and 70% across the
 * short side, each in its own slot along the long side, so neighbours never overlap and every blob stays inside.
 */
export function slimeLayout(count: number, width: number, height: number): Blob[] {
  const n = Math.max(0, Math.floor(count));
  if (n === 0 || !(width > 0) || !(height > 0)) return [];
  const tall = height >= width;
  const long = tall ? height : width;
  const short = tall ? width : height;
  const r = n === 1 ? Math.min(0.4 * short, 0.5 * long) : Math.min(0.3 * short, (0.5 * long) / n);
  const blobs: Blob[] = [];
  for (let i = 0; i < n; i++) {
    const along = ((i + 0.5) * long) / n;
    const across = n === 1 ? 0.5 * short : (i % 2 === 0 ? 0.3 : 0.7) * short;
    blobs.push(tall ? { x: across, y: along, r } : { x: along, y: across, r });
  }
  return blobs;
}

/** The length of the part of the segment a to b inside the blob (0 for a miss or no movement). */
export function chordLength(a: Point, b: Point, blob: Blob): number {
  const dx = b.x - a.x;
  const dy = b.y - a.y;
  const qa = dx * dx + dy * dy;
  if (qa === 0) return 0;
  const fx = a.x - blob.x;
  const fy = a.y - blob.y;
  const qb = 2 * (fx * dx + fy * dy);
  const qc = fx * fx + fy * fy - blob.r * blob.r;
  const disc = qb * qb - 4 * qa * qc;
  if (disc <= 0) return 0;
  const root = Math.sqrt(disc);
  const lo = Math.max(0, (-qb - root) / (2 * qa));
  const hi = Math.min(1, (-qb + root) / (2 * qa));
  return hi > lo ? (hi - lo) * Math.sqrt(qa) : 0;
}

export function slimeStart(count: number): number[] {
  return new Array<number>(Math.max(0, Math.floor(count))).fill(0);
}

/** A finger that is down moved from `from` to `to`: every blob it went through gets wiped that much. */
export function swipe(blobs: readonly Blob[], progress: SlimeProgress, from: Point, to: Point): number[] {
  return progress.map((p, i) => {
    const b = blobs[i];
    if (!b || b.r <= 0 || p >= SLIME_WIPE_SHARE - EPS) return p;
    return p + chordLength(from, to, b) / b.r;
  });
}

/** How much of blob `i` is left, 1 (untouched) to 0 (gone): its opacity. */
export function blobLeft(progress: SlimeProgress, i: number): number {
  const p = progress[i] ?? 0;
  if (p >= SLIME_WIPE_SHARE - EPS) return 0;
  return 1 - p / SLIME_WIPE_SHARE;
}

export function slimeCleared(progress: SlimeProgress): boolean {
  return progress.every((_, i) => blobLeft(progress, i) === 0);
}
