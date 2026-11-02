// Every visible text on the phone page comes from the locale files (06, general rules).

import en from './locales/en.json';
import ru from './locales/ru.json';
import type { Lang } from './protocol';

export type TextKey = keyof typeof ru;

export const locales: Record<Lang, Record<TextKey, string>> = { ru, en };

/** Looks up a text and fills {placeholders}. Falls back to Russian, then to the key itself. */
export function t(lang: Lang, key: TextKey, params?: Record<string, string | number>): string {
  let text = locales[lang][key] ?? locales.ru[key] ?? key;
  if (params) {
    for (const [name, value] of Object.entries(params)) text = text.split(`{${name}}`).join(String(value));
  }
  return text;
}
