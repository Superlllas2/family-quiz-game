// The phone socket URL (PROTOCOL.md section 2): wss://<relay-host>/ws/phone?room=CODE.
// By default the relay serves this page, so the host comes from the page location.
// VITE_RELAY_HOST overrides it (for `vite dev` against a local relay): "http://localhost:8787" keeps
// its scheme (http -> ws, https -> wss); a bare host means wss.

export interface PageLocation { protocol: string; host: string }

export function socketUrl(page: PageLocation, room: string, override?: string): string {
  let scheme: 'ws' | 'wss';
  let host: string;
  const o = (override ?? '').trim();
  if (o !== '') {
    const at = o.indexOf('://');
    if (at >= 0) {
      const given = o.slice(0, at).toLowerCase();
      scheme = given === 'http' || given === 'ws' ? 'ws' : 'wss';
      host = o.slice(at + 3);
    } else {
      scheme = 'wss';
      host = o;
    }
    host = host.replace(/\/.*$/, '');
  } else {
    scheme = page.protocol === 'https:' ? 'wss' : 'ws';
    host = page.host;
  }
  return `${scheme}://${host}/ws/phone?room=${encodeURIComponent(room)}`;
}
