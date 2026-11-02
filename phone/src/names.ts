// Name and room code pre-checks (PROTOCOL.md sections 2 and 3). Only for a nicer form: the TV app decides.

/** Each character: Latin letter, Cyrillic letter (А to Я, а to я, Ё, ё), digit or space; 1 to 16 of them. */
const NAME_PATTERN = /^[A-Za-zА-Яа-яЁё0-9 ]{1,16}$/;

export function cleanName(raw: string): string {
  return raw.trim();
}

export function isValidName(raw: string): boolean {
  return NAME_PATTERN.test(cleanName(raw));
}

/** Room codes: 4 uppercase letters without I or O. */
const ROOM_PATTERN = /^[ABCDEFGHJKLMNPQRSTUVWXYZ]{4}$/;

export function cleanRoom(raw: string): string {
  return raw.trim().toUpperCase();
}

export function isValidRoom(raw: string): boolean {
  return ROOM_PATTERN.test(cleanRoom(raw));
}
