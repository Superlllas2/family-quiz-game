import { describe, expect, it } from 'vitest';
import type { RateData, RateItem } from './protocol';
import { cardShown, closeCard, ratingFor, shownStars, tapStar } from './rating';

const item = (id: string, given: number): RateItem => ({ id, text: id, low: 'low', high: 'high', given });
const card = (questionId: string, given = 0): RateData => ({
  questionId, title: 'Rate it', close: 'Close', stars: 5, items: [item('fun', given), item('guess', 0)],
});

describe('rating card state (D109)', () => {
  it('shows the TV\'s given until this phone taps', () => {
    const rate = card('q1', 4);
    expect(shownStars(null, 'q1', rate.items[0])).toBe(4);
    expect(shownStars(ratingFor(null, 'q1'), 'q1', rate.items[1])).toBe(0);
  });

  it('a tap shows at once, over the given', () => {
    const rate = card('q1', 4);
    const s = tapStar(ratingFor(null, 'q1'), 'fun', 2, 'r1');
    expect(shownStars(s, 'q1', rate.items[0])).toBe(2);
    expect(shownStars(s, 'q1', rate.items[1])).toBe(0);
  });

  it('a later tap on the same row replaces the earlier one, with its own input id', () => {
    let s = tapStar(ratingFor(null, 'q1'), 'fun', 2, 'r1');
    s = tapStar(s, 'fun', 5, 'r2');
    expect(s.taps.fun).toStrictEqual({ stars: 5, inputId: 'r2' });
    expect(shownStars(s, 'q1', item('fun', 2))).toBe(5);
  });

  it('a re-sent screen with an older given keeps the newer local tap', () => {
    const s = tapStar(ratingFor(null, 'q1'), 'fun', 3, 'r1');
    expect(shownStars(ratingFor(s, 'q1'), 'q1', card('q1', 1).items[0])).toBe(3);
  });

  it('the next question forgets the taps', () => {
    const s = tapStar(ratingFor(null, 'q1'), 'fun', 3, 'r1');
    const next = ratingFor(s, 'q2');
    expect(next.taps).toStrictEqual({});
    expect(shownStars(next, 'q2', card('q2', 0).items[0])).toBe(0);
    // Taps of another question never show on this one.
    expect(shownStars(s, 'q2', card('q2', 0).items[0])).toBe(0);
  });

  it('closing hides the card for this question only', () => {
    const closed = closeCard(tapStar(ratingFor(null, 'q1'), 'fun', 3, 'r1'));
    expect(cardShown(null, card('q1'))).toBe(true);
    expect(cardShown(closed, card('q1'))).toBe(false);
    expect(cardShown(ratingFor(closed, 'q1'), card('q1'))).toBe(false); // the same question re-sent (Scores, resume)
    expect(cardShown(closed, card('q2'))).toBe(true);
    expect(cardShown(ratingFor(closed, 'q2'), card('q2'))).toBe(true);
  });

  it('closing keeps the taps', () => {
    const closed = closeCard(tapStar(ratingFor(null, 'q1'), 'fun', 3, 'r1'));
    expect(closed.taps.fun.stars).toBe(3);
    expect(closeCard(closed)).toBe(closed);
  });
});
