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

// ---- sabotage overlays (03 "Power ups", 06 `sabotage`) ----
// The TV app sends the real values in the `effect` params, from content/config.json `powerUps.freeze.taps`,
// `powerUps.slime.blobs` and `powerUps.fog.seconds`. These are only the fallbacks for a message without them,
// kept equal to the config values (D148).

/** Freeze: taps on the ice to break it, if the `effect` params do not say. */
export const DEFAULT_FREEZE_TAPS = 15;

/** Slime: blobs to wipe off, if the `effect` params do not say. */
export const DEFAULT_SLIME_BLOBS = 6;

/** Fog: seconds until it has cleared by itself, if the `effect` params do not say. */
export const DEFAULT_FOG_SECONDS = 6;

/**
 * The sabotage cap (D148): ms after the first effect of an Answer arrives until every effect still showing melts away,
 * if the `effect` params do not say (`powerUps.maxSabotageSeconds`, 12 s).
 */
export const DEFAULT_SABOTAGE_CAP_MS = 12000;

/** Fog: rubbing makes the fog clear this many times faster (03: "Rubbing the screen clears it 2x faster"). */
export const FOG_RUB_SPEEDUP = 2;

/** Fog: a finger that is down counts as rubbing while it moved within this many ms (the gap between move events). */
export const FOG_RUB_IDLE_MS = 150;

/** Slime: a blob is wiped once the swipes through it add up to this share of its radius (a tap alone never wipes). */
export const SLIME_WIPE_SHARE = 0.6;

/** Poll: the longest answer to a text question (PROTOCOL.md section 3, `poll` answers). */
export const POLL_TEXT_MAX = 200;

/**
 * Rating card (D109): stars per row if the screen's `rate` does not say. The TV app sends the real count from
 * content/config.json `feedback.stars`.
 */
export const DEFAULT_RATE_STARS = 5;
