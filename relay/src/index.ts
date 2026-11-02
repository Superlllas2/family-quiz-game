// Worker entry: /ws/tv and /ws/phone go to the room's Durable Object; every other path is the
// built phone page from static assets (docs/PROTOCOL.md, section 2, "URLs").

import { parseRoute } from "./routing";

export { Room } from "./room";

export default {
  async fetch(request, env): Promise<Response> {
    const route = parseRoute(new URL(request.url));
    switch (route.kind) {
      case "asset":
        return env.ASSETS.fetch(request);
      case "bad":
        return new Response(route.reason, { status: route.status });
      default: {
        if (request.headers.get("Upgrade")?.toLowerCase() !== "websocket") {
          return new Response("expected a WebSocket upgrade", { status: 426 });
        }
        const room = env.ROOM.get(env.ROOM.idFromName(route.room));
        return room.fetch(request);
      }
    }
  },
} satisfies ExportedHandler<Env>;
