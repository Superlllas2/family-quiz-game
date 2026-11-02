import { describe, expect, it } from 'vitest';
import en from './locales/en.json';
import ru from './locales/ru.json';
import { t } from './i18n';

const placeholders = (s: string): string[] => (s.match(/\{[a-zA-Z]+\}/g) ?? []).sort();

describe('locale files', () => {
  it('ru and en have the same keys', () => {
    expect(Object.keys(en).sort()).toEqual(Object.keys(ru).sort());
  });

  it('every text is non-empty and uses the same placeholders in both languages', () => {
    const ruTexts = ru as Record<string, string>;
    const enTexts = en as Record<string, string>;
    for (const key of Object.keys(ruTexts)) {
      expect(ruTexts[key].trim(), key).not.toBe('');
      expect(enTexts[key].trim(), key).not.toBe('');
      expect(placeholders(enTexts[key]), key).toEqual(placeholders(ruTexts[key]));
    }
  });

  it('has no emoji', () => {
    const all = JSON.stringify([ru, en]);
    expect(/\p{Extended_Pictographic}/u.test(all)).toBe(false);
  });

  it('fills placeholders', () => {
    expect(t('en', 'input.roundTrip', { ms: 42 })).toBe('42 ms');
    expect(t('ru', 'reconnecting.text')).toBe('Переподключение…');
  });
});
