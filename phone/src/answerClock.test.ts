import { describe, expect, it } from 'vitest';
import { AnswerClock, screenDrawn } from './answerClock';

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

describe('answer clock and drawn screens', () => {
  const answer = (phaseId: number, endsInMs: number) => ({ phaseId, id: 'answerChoice', endsInMs, data: {} });

  it('last call: a second answerChoice with the same phaseId and a smaller endsInMs keeps the clock', () => {
    let now = 500;
    const clock = new AnswerClock(() => now);
    screenDrawn(clock, answer(16, 15000));
    now = 9000;
    screenDrawn(clock, answer(16, 3000)); // last call
    now = 10200;
    expect(clock.ms(16)).toBe(9700);
  });

  it('screens without options do not start the clock', () => {
    let now = 0;
    const clock = new AnswerClock(() => now);
    screenDrawn(clock, { phaseId: 15, id: 'getReady', endsInMs: 4000, data: {} });
    screenDrawn(clock, { phaseId: 16, id: 'waiting', endsInMs: 4000, data: {} });
    now = 1000;
    expect(clock.ms(15)).toBe(0);
    expect(clock.ms(16)).toBe(0);
  });

  it('after a reload (a new clock) it starts when the restored screen is drawn', () => {
    let now = 40000;
    const clock = new AnswerClock(() => now);
    screenDrawn(clock, answer(16, 6000));
    now = 41500;
    expect(clock.ms(16)).toBe(1500);
  });
});

describe('answer clock across games and TV restarts (phaseIds start again at 1)', () => {
  it('a screen with another phaseId forgets the old start, so a reused phaseId starts fresh', () => {
    let now = 0;
    const clock = new AnswerClock(() => now);
    clock.shown(4); // first TV session, question 1
    now = 200000;
    clock.arrived(1); // the restarted TV's lobby
    clock.arrived(4);
    clock.shown(4); // the new game's question 1, same phaseId
    now = 201300;
    expect(clock.ms(4)).toBe(1300);
  });

  it('a screen with the same phaseId (last call, waiting) keeps the start', () => {
    let now = 0;
    const clock = new AnswerClock(() => now);
    clock.shown(4);
    now = 5000;
    clock.arrived(4);
    clock.shown(4);
    now = 6000;
    expect(clock.ms(4)).toBe(6000);
  });

  it('reset forgets every start (back to join)', () => {
    let now = 0;
    const clock = new AnswerClock(() => now);
    clock.shown(13);
    clock.reset();
    now = 90000;
    clock.shown(13);
    now = 91000;
    expect(clock.ms(13)).toBe(1000);
  });
});
