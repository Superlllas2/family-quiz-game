// inputId: a short random string per input (PROTOCOL.md section 3).

const ALPHABET = 'abcdefghijklmnopqrstuvwxyz0123456789';

export function newInputId(length = 8): string {
  const bytes = new Uint8Array(length);
  if (typeof crypto !== 'undefined' && typeof crypto.getRandomValues === 'function') {
    crypto.getRandomValues(bytes);
  } else {
    for (let i = 0; i < length; i++) bytes[i] = Math.floor(Math.random() * 256);
  }
  let id = '';
  for (const b of bytes) id += ALPHABET[b % ALPHABET.length];
  return id;
}
