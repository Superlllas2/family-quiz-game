// The Durable Object for one room: a thin adapter from Cloudflare's WebSocket and storage APIs
// to RoomLogic. All behaviour lives in roomLogic.ts (docs/PROTOCOL.md, section 2).

import { DurableObject } from "cloudflare:workers";
import { parseRoute } from "./routing";
import { RoomLogic, type RelaySocket } from "./roomLogic";

const SECRET_KEY = "tvSecret";

/** Wraps a server WebSocket; send and close never throw into the room logic. */
class WorkerSocket implements RelaySocket {
  constructor(private readonly ws: WebSocket) {}

  send(text: string): void {
    try {
      this.ws.send(text);
    } catch {
      // The peer is already gone; its close event will follow.
    }
  }

  close(code: number, reason: string): void {
    try {
      this.ws.close(code, reason);
    } catch {
      // Already closing.
    }
  }
}

function randomHex(chars: number): string {
  const bytes = new Uint8Array(Math.ceil(chars / 2));
  crypto.getRandomValues(bytes);
  let out = "";
  for (const b of bytes) out += b.toString(16).padStart(2, "0");
  return out.slice(0, chars);
}

export class Room extends DurableObject<Env> {
  private readonly logic: RoomLogic;

  constructor(ctx: DurableObjectState, env: Env) {
    super(ctx, env);
    const storage = ctx.storage;
    this.logic = new RoomLogic({
      clock: { now: () => Date.now() },
      ids: { randomHex },
      store: {
        load: async () => (await storage.get<string>(SECRET_KEY)) ?? null,
        // Not awaited: the output gate holds outgoing frames until the write is durable.
        save: (secret) => void storage.put(SECRET_KEY, secret),
        clear: () => void storage.deleteAll(),
      },
      alarm: {
        schedule: (atMs) => void storage.setAlarm(atMs),
        cancel: () => void storage.deleteAlarm(),
      },
    });
    // Load the secret before any request reaches the room.
    void ctx.blockConcurrencyWhile(() => this.logic.init());
  }

  async fetch(request: Request): Promise<Response> {
    const route = parseRoute(new URL(request.url));
    if (route.kind !== "tv" && route.kind !== "phone") {
      return new Response("not found", { status: 404 });
    }
    if (request.headers.get("Upgrade")?.toLowerCase() !== "websocket") {
      return new Response("expected a WebSocket upgrade", { status: 426 });
    }

    const pair = new WebSocketPair();
    const client = pair[0];
    const server = pair[1];
    server.accept();
    const socket = new WorkerSocket(server);

    server.addEventListener("message", (event) => {
      this.logic.onMessage(socket, event.data);
    });
    server.addEventListener("close", () => {
      this.logic.onClose(socket);
    });
    server.addEventListener("error", () => {
      this.logic.onClose(socket);
    });

    // Rejections (4001, 4004) accept first and then close, so the client sees the close code.
    if (route.kind === "tv") {
      this.logic.tvConnect(socket, route.secret);
    } else {
      this.logic.phoneConnect(socket);
    }
    return new Response(null, { status: 101, webSocket: client });
  }

  async alarm(): Promise<void> {
    this.logic.onAlarm();
  }
}
