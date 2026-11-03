// Freeze (03 "Power ups"): the answer area is covered in ice; each pointerdown on the ice is one tap, and after
// `taps` taps (`powerUps.freeze.taps`, sent in the `effect` params) it breaks.

export interface FreezeState {
  taps: number;
  hits: number;
}

export function freezeStart(taps: number): FreezeState {
  return { taps: Number.isFinite(taps) ? Math.max(1, Math.round(taps)) : 1, hits: 0 };
}

export function tapIce(s: FreezeState): FreezeState {
  return s.hits >= s.taps ? s : { ...s, hits: s.hits + 1 };
}

/** Taps still needed: the counter on the ice. */
export function iceLeft(s: FreezeState): number {
  return Math.max(0, s.taps - s.hits);
}

export function iceBroken(s: FreezeState): boolean {
  return s.hits >= s.taps;
}

/** How cracked the ice looks, 0 (whole) to 1 (broken). */
export function iceCracks(s: FreezeState): number {
  return Math.min(1, s.hits / s.taps);
}
