// The rating card (D109): drawn over `watch`, `waiting` and `result` in Reveal and Scores while the screen carries
// `rate`. Every star tap shows at once and is sent at once; a later tap on the same row is a new input that replaces
// the earlier one. A row shows this phone's latest tap for the question, else the TV's `given` (after a reload).
// Closing hides the card for this question only: the next question's card shows again.

import type { RateData, RateItem } from './protocol';

export interface RatingTap { stars: number; inputId: string }

export interface RatingState {
  /** The question these taps and the close belong to. */
  questionId: string;
  /** The latest tap per item id. */
  taps: Record<string, RatingTap>;
  closed: boolean;
}

/** The state for the card of this question: kept for the same question, else a fresh one. */
export function ratingFor(state: RatingState | null, questionId: string): RatingState {
  return state && state.questionId === questionId ? state : { questionId, taps: {}, closed: false };
}

/** A tap on star `stars` of an item's row, sent as `inputId`. */
export function tapStar(state: RatingState, item: string, stars: number, inputId: string): RatingState {
  return { ...state, taps: { ...state.taps, [item]: { stars, inputId } } };
}

export function closeCard(state: RatingState): RatingState {
  return state.closed ? state : { ...state, closed: true };
}

/** The stars filled on a row: the latest local tap for this question, else the TV's `given` (0: none). */
export function shownStars(state: RatingState | null, questionId: string, item: RateItem): number {
  const tap = state && state.questionId === questionId ? state.taps[item.id] : undefined;
  return tap ? tap.stars : item.given;
}

/** The card shows unless the player closed it for this question. */
export function cardShown(state: RatingState | null, rate: RateData): boolean {
  return !(state && state.questionId === rate.questionId && state.closed);
}
