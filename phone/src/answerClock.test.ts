import { describe, expect, it } from 'vitest';
import { AnswerClock } from './answerClock';

describe('answer clock (ms)', () => {
  it('measures from the moment the options first appeared', () => {
    let now = 1000;
    const clock = new AnswerClock(() => now);
    clock.shown(16);
    now = 4410.4;
    expect(clock.ms(16)).toBe(3410);
  });

  it('a new screen with the same phaseId does not restart it', () => {
    let now = 0;
    const clock = new AnswerClock(() => now);
    clock.shown(16);
    now = 10000;
    clock.shown(16); // last call: same phaseId
    now = 12000;
    expect(clock.ms(16)).toBe(12000);
  });

  it('a new phaseId starts a new clock', () => {
    let now = 0;
    const clock = new AnswerClock(() => now);
    clock.shown(16);
    now = 5000;
    clock.shown(17);
    now = 6000;
    expect(clock.ms(17)).toBe(1000);
    expect(clock.ms(16)).toBe(6000);
  });
});
