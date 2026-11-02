import { describe, expect, it } from 'vitest';
import { cleanName, cleanRoom, isValidName, isValidRoom } from './names';

describe('name pre-check (PROTOCOL.md section 3)', () => {
  it('trims before checking', () => {
    expect(cleanName('  Mama  ')).toBe('Mama');
    expect(isValidName('  Mama  ')).toBe(true);
  });

  it('accepts 1 and 16 characters, refuses 0 and 17', () => {
    expect(isValidName('A')).toBe(true);
    expect(isValidName('A'.repeat(16))).toBe(true);
    expect(isValidName('A'.repeat(17))).toBe(false);
    expect(isValidName('')).toBe(false);
    expect(isValidName('    ')).toBe(false);
  });

  it('counts length after trimming', () => {
    expect(isValidName(`  ${'b'.repeat(16)}  `)).toBe(true);
  });

  it('accepts Latin, Cyrillic including Ё and ё, digits and inner spaces', () => {
    expect(isValidName('Mama 2')).toBe(true);
    expect(isValidName('Мама')).toBe(true);
    expect(isValidName('Ёжик')).toBe(true);
    expect(isValidName('ёлка')).toBe(true);
    expect(isValidName('Дядя Фёдор')).toBe(true);
    expect(isValidName('АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ'.slice(0, 16))).toBe(true);
    expect(isValidName('абвгдежзийклмнопрстуфхцчшщъыьэюя'.slice(16))).toBe(true);
  });

  it('refuses emoji, symbols, punctuation and other scripts', () => {
    expect(isValidName('Mama\u{1F600}')).toBe(false);
    expect(isValidName('❤')).toBe(false);
    expect(isValidName('Mama!')).toBe(false);
    expect(isValidName('a-b')).toBe(false);
    expect(isValidName('a_b')).toBe(false);
    expect(isValidName('Zoë')).toBe(false);
    expect(isValidName('Ελένη')).toBe(false);
    expect(isValidName('a\tb')).toBe(false);
    expect(isValidName('١٢')).toBe(false);
  });
});

describe('room code pre-check', () => {
  it('uppercases and accepts 4 letters without I or O', () => {
    expect(cleanRoom(' abcd ')).toBe('ABCD');
    expect(isValidRoom('abcd')).toBe(true);
    expect(isValidRoom('ABCI')).toBe(false);
    expect(isValidRoom('ABCO')).toBe(false);
    expect(isValidRoom('ABC')).toBe(false);
    expect(isValidRoom('ABCDE')).toBe(false);
  });
});
