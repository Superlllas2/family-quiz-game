// Countdowns on the phone are for display only (PROTOCOL.md section 4, `endsInMs`): the TV app decides when a
// phase ends. The remaining time counts down locally from the moment the message arrived (performance.now()).

/** Milliseconds left, never below 0; null when the screen has no countdown (`endsInMs` null). */
export function remainingMs(endsInMs: number | null, receivedAt: number, now: number): number | null {
  if (endsInMs === null) return null;
  return Math.max(0, endsInMs - (now - receivedAt));
}

/** Whole seconds shown to the player: 2001 ms shows 3, 2000 ms shows 2, 0 shows 0. */
export function wholeSeconds(ms: number): number {
  return Math.max(0, Math.ceil(ms / 1000));
}

/** Share of the time left for a countdown bar, 0 to 1. */
export function remainingShare(remaining: number, total: number | null): number {
  if (total === null || total <= 0) return remaining > 0 ? 1 : 0;
  return Math.min(1, Math.max(0, remaining / total));
}
