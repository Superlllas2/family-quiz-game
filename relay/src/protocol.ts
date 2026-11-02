// Relay protocol constants. Source of truth: docs/PROTOCOL.md, section 2 ("Relay").
// These are protocol numbers shared with the phone page and the TV app, not game tuning,
// so they live here and not in content/config.json. Change them only together with PROTOCOL.md.

/** Room codes: 4 uppercase letters, no I or O. */
export const ROOM_CODE_PATTERN = /^[ABCDEFGHJKLMNPQRSTUVWXYZ]{4}$/;

/** tvSecret: a random 32 character string made by the TV app. */
export const TV_SECRET_LENGTH = 32;

/** Messages over 8 KB (UTF-8 bytes) are dropped. A message of exactly this size passes. */
export const MAX_MESSAGE_BYTES = 8192;

/** A phone may send at most this many messages per rate window. Extra messages are dropped. */
export const PHONE_RATE_LIMIT_COUNT = 20;

/** The rate window, in milliseconds (rolling). */
export const PHONE_RATE_WINDOW_MS = 1000;

/** Rooms close this long after the TV app socket closes. */
export const ROOM_CLOSE_AFTER_TV_AWAY_MS = 10 * 60 * 1000;

/** Close code sent to the TV app: wrong secret for this room. */
export const CLOSE_WRONG_SECRET = 4001;

/** Close code sent to a phone by the TV app's close control message (a newer socket took the seat). */
export const CLOSE_REPLACED_BY_TV = 4002;

/** Close code sent to a phone: no TV in this room, or the room closed. */
export const CLOSE_NO_TV = 4004;

/**
 * Close code sent to an older TV socket when the TV app reconnects with the right secret
 * while the older socket is still open. PROTOCOL.md does not name one; 1000 (normal closure)
 * is used so the TV app never mistakes it for 4001 (wrong secret).
 */
export const CLOSE_TV_REPLACED = 1000;

/** Prefix and number of random lowercase hex characters in a relay connection id ("c_7f3a9b"). */
export const CONN_ID_PREFIX = "c_";
export const CONN_ID_HEX_CHARS = 6;

/** Messages the relay itself sends to phones. */
export const MSG_TV_AWAY = '{"v":1,"type":"tvAway"}';
export const MSG_TV_BACK = '{"v":1,"type":"tvBack"}';
