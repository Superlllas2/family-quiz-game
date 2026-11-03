import { describe, expect, it } from 'vitest';
import { shownScreen } from './shownScreen';

describe('which screen the phone draws for a TV screen id (06, D88)', () => {
  it('draws every version 2 screen it has a view for', () => {
    for (const id of [
      'pickCharacter', 'lobby', 'countdown', 'watch', 'topicVote', 'answerChoice', 'answerSlider', 'answerTrueFalse',
      'waiting', 'results', 'pollDone', 'error',
    ]) {
      expect(shownScreen(id), id).toBe(id);
    }
  });

  it('shows watch for an unknown screen id, never a crash', () => {
    expect(shownScreen('somethingNew')).toBe('watch');
    expect(shownScreen('')).toBe('watch');
  });

  it('shows watch for the screens version 2 removed', () => {
    for (const id of ['getReady', 'roundEnd', 'raceResult']) expect(shownScreen(id), id).toBe('watch');
  });

  it('shows watch for result until the power up pick exists (Day 4)', () => {
    expect(shownScreen('result')).toBe('watch');
  });
});
