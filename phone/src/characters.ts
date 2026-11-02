// The character roster, bundled from content/characters.json at build time (PROTOCOL.md section 4): one copy
// of the file, imported from the repo. Until the 3D characters exist a character is drawn as a chip or a card
// in its signature color (06).

import roster from '../../content/characters.json';
import type { Lang } from './protocol';

/** Chip color for an unknown character id (should not happen; the TV assigns a valid one). */
const FALLBACK_COLOR = '#9AA8BA';

export interface Character {
  id: number;
  color: string;
  name: Record<Lang, string>;
  personality: Record<Lang, string>;
}

/** Every character in roster order. */
export const characters: readonly Character[] = (roster as ReadonlyArray<Character>).map((c) => ({
  id: c.id,
  color: c.color,
  name: { ru: c.name.ru, en: c.name.en },
  personality: { ru: c.personality.ru, en: c.personality.en },
}));

const byId = new Map<number, Character>(characters.map((c) => [c.id, c]));

export function characterColor(characterId: number): string {
  return byId.get(characterId)?.color ?? FALLBACK_COLOR;
}

export function character(characterId: number): Character | null {
  return byId.get(characterId) ?? null;
}
