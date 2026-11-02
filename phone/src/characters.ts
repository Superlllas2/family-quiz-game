// The character roster, bundled from content/characters.json at build time (PROTOCOL.md section 4).
// Until the 3D characters exist a character is drawn as a chip in its signature color (06).

import roster from '../../content/characters.json';

/** Chip color for an unknown character id (should not happen; the TV assigns a valid one). */
const FALLBACK_COLOR = '#9AA8BA';

const colors = new Map<number, string>();
for (const c of roster as ReadonlyArray<{ id: number; color: string }>) colors.set(c.id, c.color);

export function characterColor(characterId: number): string {
  return colors.get(characterId) ?? FALLBACK_COLOR;
}
