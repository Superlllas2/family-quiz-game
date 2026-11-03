// Fog (03 "Power ups"): a blur layer over the answer area whose opacity falls linearly to 0 over `seconds`
// (`powerUps.fog.seconds`, sent in the `effect` params), counted from the moment the `effect` arrived. While the
// player rubs (a finger down and moving), time counts FOG_RUB_SPEEDUP (2x) faster. Cleared at 0.

import { FOG_RUB_IDLE_MS, FOG_RUB_SPEEDUP } from './constants';

export interface FogState {
  totalMs: number;
  doneMs: number;
  /** The last time counted. */
  lastAt: number;
}

/** `startAt` is the effect's arrival time, so an overlay drawn later counts the time already passed. */
export function fogStart(seconds: number, startAt: number): FogState {
  const totalMs = Number.isFinite(seconds) && seconds > 0 ? seconds * 1000 : 0;
  return { totalMs, doneMs: 0, lastAt: startAt };
}

/** Counts the time from the last call to `now`, 2x if the player was rubbing. */
export function fogAdvance(s: FogState, now: number, rubbing: boolean): FogState {
  const dt = Math.max(0, now - s.lastAt);
  return { ...s, doneMs: s.doneMs + dt * (rubbing ? FOG_RUB_SPEEDUP : 1), lastAt: Math.max(s.lastAt, now) };
}

export function fogOpacity(s: FogState): number {
  if (s.totalMs <= 0) return 0;
  return Math.min(1, Math.max(0, 1 - s.doneMs / s.totalMs));
}

export function fogCleared(s: FogState): boolean {
  return s.doneMs >= s.totalMs;
}

/** Rubbing: a finger is down and moved within FOG_RUB_IDLE_MS (the gap between two pointermove events). */
export function isRubbing(down: boolean, lastMoveAt: number | null, now: number): boolean {
  return down && lastMoveAt !== null && now - lastMoveAt <= FOG_RUB_IDLE_MS;
}
