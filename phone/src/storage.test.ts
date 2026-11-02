import { describe, expect, it } from 'vitest';
import { loadLang, parseLaunch, safeStore, saveLang, tokenKey, TokenStore, type KeyValueStore } from './storage';

function memoryStore(): KeyValueStore & { data: Map<string, string> } {
  const data = new Map<string, string>();
  return {
    data,
    getItem: (k) => data.get(k) ?? null,
    setItem: (k, v) => void data.set(k, v),
    removeItem: (k) => void data.delete(k),
  };
}

describe('token key (PROTOCOL.md section 4)', () => {
  it('is fq.token.<room> without a tab', () => {
    expect(tokenKey('ABCD', null)).toBe('fq.token.ABCD');
  });

  it('is fq.token.<room>.<n> with &tab=<n>', () => {
    expect(tokenKey('ABCD', '2')).toBe('fq.token.ABCD.2');
  });

  it('reads room (uppercased) and tab from the URL', () => {
    expect(parseLaunch('?room=abcd')).toEqual({ room: 'ABCD', tab: null });
    expect(parseLaunch('?room=ABCD&tab=3')).toEqual({ room: 'ABCD', tab: '3' });
    expect(parseLaunch('')).toEqual({ room: '', tab: null });
  });

  it('TokenStore keeps one token per tab', () => {
    const store = memoryStore();
    const tab2 = new TokenStore(store, '2');
    const plain = new TokenStore(store, null);
    tab2.set('ABCD', 'tok2');
    plain.set('ABCD', 'tok0');
    expect(store.data.get('fq.token.ABCD.2')).toBe('tok2');
    expect(store.data.get('fq.token.ABCD')).toBe('tok0');
    tab2.remove('ABCD');
    expect(tab2.get('ABCD')).toBeNull();
    expect(plain.get('ABCD')).toBe('tok0');
  });
});

describe('forgetting a seat after 4002 (D67)', () => {
  it('hides the token from this tab only, also after a reload, until a new token is stored', () => {
    const shared = memoryStore();
    const oldTab = memoryStore();
    const old = new TokenStore(shared, null, oldTab);
    old.set('ABCD', 'tok1');
    old.forget('ABCD');
    expect(old.get('ABCD')).toBeNull();
    expect(new TokenStore(shared, null, oldTab).get('ABCD')).toBeNull(); // the old tab reloaded
    expect(new TokenStore(shared, null, memoryStore()).get('ABCD')).toBe('tok1'); // the newer tab
    old.set('ABCD', 'tok2');
    expect(old.get('ABCD')).toBe('tok2');
  });
});

describe('storage fallbacks', () => {
  it('falls back to memory when storage throws', () => {
    const s = safeStore(() => {
      throw new Error('blocked');
    });
    s.setItem('k', 'v');
    expect(s.getItem('k')).toBe('v');
    s.removeItem('k');
    expect(s.getItem('k')).toBeNull();
  });

  it('language defaults to Russian and is remembered', () => {
    const store = memoryStore();
    expect(loadLang(store)).toBe('ru');
    saveLang(store, 'en');
    expect(loadLang(store)).toBe('en');
  });
});
