import { describe, expect, it } from "vitest";
import { parseRoute } from "../src/routing";

const base = "https://relay.example";
const secret = "0123456789abcdef0123456789ABCDEF";
const route = (path: string) => parseRoute(new URL(base + path));

describe("parseRoute", () => {
  it("routes a TV socket to its room with its secret", () => {
    expect(route(`/ws/tv?room=ABCD&secret=${secret}`)).toEqual({ kind: "tv", room: "ABCD", secret });
  });

  it("routes a phone socket to its room", () => {
    expect(route("/ws/phone?room=WXYZ")).toEqual({ kind: "phone", room: "WXYZ" });
  });

  it("keeps rooms apart: different codes give different rooms", () => {
    const a = route("/ws/phone?room=ABCD");
    const b = route("/ws/phone?room=WXYZ");
    expect(a).toMatchObject({ room: "ABCD" });
    expect(b).toMatchObject({ room: "WXYZ" });
  });

  it.each([
    ["missing", "/ws/phone"],
    ["empty", "/ws/phone?room="],
    ["lowercase", "/ws/phone?room=abcd"],
    ["contains I", "/ws/phone?room=ABCI"],
    ["contains O", "/ws/phone?room=OBCD"],
    ["digits", "/ws/phone?room=AB12"],
    ["too short", "/ws/phone?room=ABC"],
    ["too long", "/ws/phone?room=ABCDE"],
    ["tv with bad room", `/ws/tv?room=AB&secret=${secret}`],
  ])("rejects a bad room code (%s)", (_name, path) => {
    expect(route(path)).toMatchObject({ kind: "bad", status: 400 });
  });

  it.each([
    ["missing", "/ws/tv?room=ABCD"],
    ["too short", `/ws/tv?room=ABCD&secret=${secret.slice(1)}`],
    ["too long", `/ws/tv?room=ABCD&secret=${secret}x`],
    ["contains a control character", `/ws/tv?room=ABCD&secret=${secret.slice(1)}%0A`],
  ])("rejects a bad TV secret (%s)", (_name, path) => {
    expect(route(path)).toMatchObject({ kind: "bad", status: 400 });
  });

  it("accepts any 32 character secret alphabet, compared after URL decoding", () => {
    const b64 = "ab+/cd==".repeat(4);
    expect(route(`/ws/tv?room=ABCD&secret=${encodeURIComponent(b64)}`)).toEqual({ kind: "tv", room: "ABCD", secret: b64 });
    // Unescaped, a + arrives as a space; the same TV sends the same text every time, so it still matches.
    expect(route(`/ws/tv?room=ABCD&secret=${b64}`)).toEqual({ kind: "tv", room: "ABCD", secret: b64.replaceAll("+", " ") });
  });

  it("rejects unknown socket paths", () => {
    expect(route("/ws/admin?room=ABCD")).toMatchObject({ kind: "bad", status: 404 });
    expect(route("/ws")).toMatchObject({ kind: "bad", status: 404 });
  });

  it("leaves every other path to the static assets", () => {
    expect(route("/?room=ABCD")).toEqual({ kind: "asset" });
    expect(route("/index.html")).toEqual({ kind: "asset" });
    expect(route("/assets/app.js")).toEqual({ kind: "asset" });
    expect(route("/wsx")).toEqual({ kind: "asset" });
  });
});
