import { describe, expect, it } from 'vitest';
import { t } from './i18n';
import { roundEndNext } from './roundEnd';

describe('roundEnd: what comes next', () => {
  it('names the next round while there is one', () => {
    const next = roundEndNext(3);
    expect(next).toEqual({ key: 'roundEnd.next', params: { round: 3 } });
    expect(t('en', next.key, next.params)).toBe('Round 3 next');
    expect(t('ru', next.key, next.params)).toBe('Раунд 3');
  });

  it('announces the final race after the last round (nextRound null)', () => {
    const next = roundEndNext(null);
    expect(next.key).toBe('roundEnd.finalNext');
    expect(t('en', next.key, next.params)).toBe('Final race next');
    expect(t('ru', next.key, next.params)).toBe('Дальше финальная гонка');
  });
});
