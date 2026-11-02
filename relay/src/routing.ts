// Maps a request URL to a room socket route (docs/PROTOCOL.md, section 2, "URLs").

import { ROOM_CODE_PATTERN, TV_SECRET_LENGTH } from "./protocol";

export type Route =
  | { kind: "tv"; room: string; secret: string }
  | { kind: "phone"; room: string }
  | { kind: "bad"; status: number; reason: string }
  | { kind: "asset" };

/**
 * 32 characters after URL decoding, no control characters. PROTOCOL.md fixes only the length,
 * so any alphabet the TV app picks is accepted (a `+` arrives as a space unless it was escaped).
 */
const SECRET_PATTERN = new RegExp(`^[^\\x00-\\x1F\\x7F]{${TV_SECRET_LENGTH}}$`);

export function isValidRoomCode(code: string | null): code is string {
  return code !== null && ROOM_CODE_PATTERN.test(code);
}

export function parseRoute(url: URL): Route {
  const path = url.pathname;
  if (!path.startsWith("/ws/") && path !== "/ws") {
    return { kind: "asset" };
  }
  if (path !== "/ws/tv" && path !== "/ws/phone") {
    return { kind: "bad", status: 404, reason: "unknown socket path" };
  }
  const room = url.searchParams.get("room");
  if (!isValidRoomCode(room)) {
    return { kind: "bad", status: 400, reason: "bad room code" };
  }
  if (path === "/ws/phone") {
    return { kind: "phone", room };
  }
  const secret = url.searchParams.get("secret");
  if (secret === null || !SECRET_PATTERN.test(secret)) {
    return { kind: "bad", status: 400, reason: "bad secret" };
  }
  return { kind: "tv", room, secret };
}
