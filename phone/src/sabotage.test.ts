import { describe, expect, it } from 'vitest';
import { AnswerClock, screenDrawn } from './answerClock';
import type { EffectMessage, ScreenState } from './protocol';
import { addEffect, clearShown, effectTextKey, overlayShown, queueForScreen, shownEffect } from './sabotage';

const params = { taps: 10, blobs: 5, seconds: 6 };
const freeze: EffectMessage = { type: 'effect', effect: 'freeze', from: 'Papa', params };
const slime: EffectMessage = { type: 'effect', effect: 'slime', from: 'Mama', params };
const fog: EffectMessage = { type: 'effect', effect: 'fog', from: 'Bo', params };
const screen = (id: string, phaseId: number): ScreenState => ({ phaseId, id, endsInMs: 15000, data: {} });

describe('a sabotage effect belongs to the phaseId showing when it arrives (06 `sabotage`)', () => {
  it('the first effect shows at once, from its arrival time', () => {
    const q = addEffect(null, freeze, 16, 1200);
    expect(shownEffect(q)).toStrictEqual({ seq: 0, effect: 'freeze', from: 'Papa', params, startedAt: 1200 });
  });

  it('is ignored when no screen is showing yet', () => {
    expect(addEffect(null, freeze, null, 1200)).toBeNull();
    expect(shownEffect(null)).toBeNull();
  });

  it('a new screen with the same phaseId (last call) keeps the queue; any other phaseId drops it', () => {
    const q = addEffect(addEffect(null, freeze, 16, 1200), slime, 16, 1201);
    expect(queueForScreen(q, 16)).toBe(q);
    expect(queueForScreen(q, 17)).toBeNull();
    expect(queueForScreen(null, 16)).toBeNull();
  });

  it('an effect for another phaseId than the queue\'s starts a new queue', () => {
    const q = addEffect(addEffect(null, freeze, 16, 1200), slime, 17, 5000);
    expect(q!.phaseId).toBe(17);
    expect(shownEffect(q)).toMatchObject({ effect: 'slime', startedAt: 5000 });
  });

  it('draws the overlay on an answer screen of its phaseId until every effect is cleared', () => {
    let q = addEffect(addEffect(null, freeze, 16, 1200), slime, 16, 1200);
    expect(overlayShown(q, screen('answerChoice', 16))).toBe(true);
    expect(overlayShown(q, screen('answerSlider', 16))).toBe(true);
    expect(overlayShown(q, screen('answerTrueFalse', 16))).toBe(true);
    expect(overlayShown(q, screen('answerChoice', 17))).toBe(false);
    expect(overlayShown(q, screen('waiting', 16))).toBe(false);
    expect(overlayShown(q, screen('watch', 16))).toBe(false);
    expect(overlayShown(q, null)).toBe(false);
    expect(overlayShown(null, screen('answerChoice', 16))).toBe(false);
    q = clearShown(q, 0, 3000)!.queue;
    expect(overlayShown(q, screen('answerChoice', 16))).toBe(true); // the slime is still to come
    q = clearShown(q, 1, 4000)!.queue;
    expect(overlayShown(q, screen('answerChoice', 16))).toBe(false);
    expect(shownEffect(q)).toBeNull();
  });
});

describe('several effects on one phone play one after another (D145)', () => {
  it('they arrive in order; only the first shows, the others wait without a start time', () => {
    let q = addEffect(null, freeze, 16, 1000);
    q = addEffect(q, slime, 16, 1001);
    q = addEffect(q, fog, 16, 1002);
    expect(q!.waiting.map((e) => e.effect)).toEqual(['freeze', 'slime', 'fog']);
    expect(q!.waiting.map((e) => e.seq)).toEqual([0, 1, 2]);
    expect(shownEffect(q)).toMatchObject({ seq: 0, effect: 'freeze', startedAt: 1000 });
  });

  it('clearing the shown one gives its ms from when it started showing, and the next starts showing at once', () => {
    let q = addEffect(addEffect(addEffect(null, freeze, 16, 1000.4), slime, 16, 1001), fog, 16, 1002);
    const first = clearShown(q, 0, 3541.2)!;
    expect(first.cleared.effect).toBe('freeze');
    expect(first.ms).toBe(2541);
    q = first.queue;
    expect(shownEffect(q)).toMatchObject({ seq: 1, effect: 'slime', from: 'Mama', startedAt: 3541.2 });

    const second = clearShown(q, 1, 5000)!;
    expect(second.cleared.effect).toBe('slime');
    expect(second.ms).toBe(1459); // from 3541.2, when the slime started showing, not from its arrival
    q = second.queue;
    expect(shownEffect(q)).toMatchObject({ seq: 2, effect: 'fog', startedAt: 5000 });

    const third = clearShown(q, 2, 11000)!;
    expect(third.ms).toBe(6000);
    expect(shownEffect(third.queue)).toBeNull();
  });

  it('a later effect arriving while one shows waits its turn; one arriving after all are cleared shows at once', () => {
    let q = addEffect(null, freeze, 16, 1000);
    q = clearShown(q, 0, 2000)!.queue;
    q = addEffect(q, fog, 16, 2500);
    expect(shownEffect(q)).toMatchObject({ seq: 1, effect: 'fog', startedAt: 2500 });
  });

  it('only the shown effect can be cleared, once: the TV accepts clears in order only', () => {
    let q = addEffect(addEffect(null, freeze, 16, 1000), slime, 16, 1000);
    expect(clearShown(q, 1, 2000)).toBeNull(); // the slime is not showing yet
    q = clearShown(q, 0, 2000)!.queue;
    expect(clearShown(q, 0, 2100)).toBeNull(); // the freeze is already cleared
    expect(clearShown(null, 0, 2100)).toBeNull();
    expect(clearShown(q, 1, 2100)).not.toBeNull();
  });

  it('a reload gets back every effect not cleared yet, in order, and plays them the same way', () => {
    // After a reload the TV sends the snapshot, then one `effect` per uncleared hit; the page starts empty.
    let q = addEffect(null, slime, 16, 100);
    q = addEffect(q, fog, 16, 101);
    expect(shownEffect(q)).toMatchObject({ effect: 'slime', startedAt: 100 });
    q = clearShown(q, 0, 900)!.queue;
    expect(shownEffect(q)).toMatchObject({ effect: 'fog', startedAt: 900 });
  });

  it('the answer clock keeps running under the overlays: effects, a last call and the clears never reset it', () => {
    let now = 1000;
    const clock = new AnswerClock(() => now);
    const answer = screen('answerChoice', 16);
    // The app's order: the screen arrives and is drawn, then the effects arrive right after it.
    clock.arrived(answer.phaseId);
    screenDrawn(clock, answer);
    now = 1040;
    let q = addEffect(addEffect(null, freeze, answer.phaseId, now), slime, answer.phaseId, now);
    now = 3000;
    // Last call: the same phaseId again.
    const lastCall = { ...answer, endsInMs: 5000 };
    clock.arrived(lastCall.phaseId);
    screenDrawn(clock, lastCall);
    q = queueForScreen(q, lastCall.phaseId);
    expect(overlayShown(q, lastCall)).toBe(true);
    now = 4200;
    q = clearShown(q, 0, now)!.queue;
    now = 4400;
    q = clearShown(q, 1, now)!.queue;
    now = 4500;
    expect(clock.ms(answer.phaseId)).toBe(3500);
  });

  it('names the attacker in the player\'s language', () => {
    expect(effectTextKey('freeze')).toBe('sabotage.freeze');
    expect(effectTextKey('slime')).toBe('sabotage.slime');
    expect(effectTextKey('fog')).toBe('sabotage.fog');
  });
});
