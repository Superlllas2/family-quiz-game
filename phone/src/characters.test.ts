import { describe, expect, it } from 'vitest';
import { character, characterColor, characters } from './characters';

describe('character roster (content/characters.json, bundled)', () => {
  it('has ids 0 to 15 with a color, name and personality line in both languages', () => {
    expect(characters.map((c) => c.id)).toEqual([...Array(16).keys()]);
    for (const c of characters) {
      expect(c.color).toMatch(/^#[0-9A-Fa-f]{6}$/);
      for (const lang of ['ru', 'en'] as const) {
        expect(c.name[lang].trim(), `${c.id} name ${lang}`).not.toBe('');
        expect(c.personality[lang].trim(), `${c.id} personality ${lang}`).not.toBe('');
      }
    }
  });

  it('falls back for an unknown id', () => {
    expect(character(99)).toBeNull();
    expect(characterColor(99)).toBe('#9AA8BA');
    expect(characterColor(0)).toBe(characters[0].color);
  });
});
