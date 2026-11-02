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

/** Per tab key (sessionStorage) for a token this tab gave up after close code 4002. */
export function forgottenKey(room: string, tab: string | null): string {
  return `fq.forgotten.${room}${tab ? `.${tab}` : ''}`;
}

function memoryKeyValueStore(): KeyValueStore {
  const data = new Map<string, string>();
  return {
    getItem: (k) => data.get(k) ?? null,
    setItem: (k, v) => void data.set(k, v),
    removeItem: (k) => void data.delete(k),
  };
}

/**
 * The seat token per room. `store` is shared by every tab of the browser (localStorage); `tabStore`
 * belongs to this tab only (sessionStorage, kept across reloads of the tab).
 */
export class TokenStore {
  constructor(
    private readonly store: KeyValueStore,
    private readonly tab: string | null,
    private readonly tabStore: KeyValueStore = memoryKeyValueStore(),
  ) {}

  get(room: string): string | null {
    if (!room) return null;
    const token = this.store.getItem(tokenKey(room, this.tab));
    if (!token) return null;
    // A token this tab gave up (4002) now belongs to the newer tab: this tab must not use it.
    return token === this.tabStore.getItem(forgottenKey(room, this.tab)) ? null : token;
  }

  set(room: string, token: string): void {
    if (!room) return;
    this.store.setItem(tokenKey(room, this.tab), token);
    this.tabStore.removeItem(forgottenKey(room, this.tab));
  }

  remove(room: string): void {
    if (!room) return;
    this.store.removeItem(tokenKey(room, this.tab));
    this.tabStore.removeItem(forgottenKey(room, this.tab));
  }

  /**
   * Close code 4002: another tab took this seat. This tab forgets the token without deleting it from the
   * shared store, because the newer tab holds the same token and still needs it.
   */
  forget(room: string): void {
    if (!room) return;
    const token = this.store.getItem(tokenKey(room, this.tab));
    if (token) this.tabStore.setItem(forgottenKey(room, this.tab), token);
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
