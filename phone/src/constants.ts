// Phone side timings, straight from docs/06_phone_screens.md and docs/PROTOCOL.md.
// content/config.json has no phone keys yet, so they live here in one place (open question in the Day 1 report).

/** Protocol version carried by every message (PROTOCOL.md section 7). */
export const PROTOCOL_VERSION = 2;

/** Reconnecting screen: retry every 2 s (06, screen `reconnecting`). Also the wait for `welcome` after a `rejoin`. */
export const RETRY_MS = 2000;

/** No ack within 1 s: resend once (06, general rules). */
export const RESEND_MS = 1000;

/** Still nothing after 3 s: show "No connection" (06, general rules). Also used for a `join` that gets no answer. */
export const NO_CONNECTION_MS = 3000;

/** `ping` every 5 s (PROTOCOL.md section 3). */
export const PING_INTERVAL_MS = 5000;

/** "More time" choices in settings: `timerMultiplier` is 1, 1.5 or 2 (PROTOCOL.md section 3, `settings`). */
export const TIMER_MULTIPLIERS: readonly number[] = [1, 1.5, 2];

/** How often a countdown on screen redraws. Display only; the TV app owns time (PROTOCOL.md section 6). */
export const COUNTDOWN_TICK_MS = 100;
