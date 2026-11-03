import { describe, expect, it } from 'vitest';
import { selectedCard } from './pickSelection';

describe('selectedCard', () => {
  it('starts on the card the TV assigned', () => {
    expect(selectedCard(null, 3, [1, 3])).toBe(3);
  });

  it('follows a tap on a free card', () => {
    expect(selectedCard(5, 3, [1, 3])).toBe(5);
  });

  it('keeps the own card even though the TV lists it as taken', () => {
    expect(selectedCard(3, 3, [1, 3])).toBe(3);
  });

  it('falls back to the assigned card when someone takes the tapped one before Confirm', () => {
    expect(selectedCard(5, 3, [1, 3, 5])).toBe(3);
  });

  it('has nothing selected without a tap or an assigned card', () => {
    expect(selectedCard(null, null, [])).toBeNull();
  });
});
