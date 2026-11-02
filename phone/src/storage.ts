// localStorage access behind a small interface so the logic is testable without a browser.

import type { Lang } from './protocol';

export interface KeyValueStore {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
}

/** Token key per room; a page opened with &tab=<n> keeps its own seat (PROTOCOL.md section 4). */
export function tokenKey(room: string, tab: string | null): string {
  return tab ? `fq.token.${room}.${tab}` : `fq.token.${room}`;
}

export interface LaunchParams { room: string; tab: string | null }

/** Reads ?room= (uppercased) and &tab= from the page URL query. */
export function parseLaunch(search: string): LaunchParams {
  const q = new URLSearchParams(search);
  const room = (q.get('room') ?? '').trim().toUpperCase();
  const tab = (q.get('tab') ?? '').trim();
  return { room, tab: tab === '' ? null : tab };
}

/** Wraps a browser Storage: any access can throw (private mode, blocked storage), so it falls back to memory. */
export function safeStore(get: () => KeyValueStore | undefined): KeyValueStore {
  const memory = new Map<string, string>();
  const real = (): KeyValueStore | undefined => {
    try {
      return get();
    } catch {
      return undefined;
    }
  };
  return {
    getItem(key) {
      try {
        const s = real();
        if (s) return s.getItem(key);
      } catch { /* fall back to memory */ }
      return memory.get(key) ?? null;
    },
    setItem(key, value) {
      memory.set(key, value);
      try { real()?.setItem(key, value); } catch { /* memory only */ }
    },
    removeItem(key) {
      memory.delete(key);
      try { real()?.removeItem(key); } catch { /* memory only */ }
    },
  };
}

export class TokenStore {
  constructor(private readonly store: KeyValueStore, private readonly tab: string | null) {}

  get(room: string): string | null {
    if (!room) return null;
    const token = this.store.getItem(tokenKey(room, this.tab));
    return token ? token : null;
  }

  set(room: string, token: string): void {
    if (room) this.store.setItem(tokenKey(room, this.tab), token);
  }

  remove(room: string): void {
    if (room) this.store.removeItem(tokenKey(room, this.tab));
  }
}

export const LANG_KEY = 'fq.lang';

/** The language toggle defaults to Russian (06, screen `join`). */
export function loadLang(store: KeyValueStore): Lang {
  return store.getItem(LANG_KEY) === 'en' ? 'en' : 'ru';
}

export function saveLang(store: KeyValueStore, lang: Lang): void {
  store.setItem(LANG_KEY, lang);
}
