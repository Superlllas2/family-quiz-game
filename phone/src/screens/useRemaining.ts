// Redraws a countdown every COUNTDOWN_TICK_MS and returns the milliseconds left (display only).

import { useEffect, useState } from 'preact/hooks';
import { COUNTDOWN_TICK_MS } from '../constants';
import { remainingMs } from '../countdown';

export function useRemaining(endsInMs: number | null, receivedAt: number, now: () => number): number | null {
  const [, setTick] = useState(0);
  useEffect(() => {
    if (endsInMs === null) return;
    const h = setInterval(() => setTick((n) => n + 1), COUNTDOWN_TICK_MS);
    return () => clearInterval(h);
  }, [endsInMs, receivedAt]);
  // Computed from the clock at render time, so a new message (last call) is right on its first frame.
  return remainingMs(endsInMs, receivedAt, now());
}
