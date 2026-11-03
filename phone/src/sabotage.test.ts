import { describe, expect, it } from 'vitest';
import type { EffectMessage, ScreenState } from './protocol';
import { attachEffect, clearEffect, effectForScreen, effectTextKey, overlayShown } from './sabotage';

const freeze: EffectMessage = { type: 'effect', effect: 'freeze', from: 'Papa', params: { taps: 6, blobs: 3, seconds: 3 } };
const screen = (id: string, phaseId: number): ScreenState => ({ phaseId, id, endsInMs: 15000, data: {} });

describe('a sabotage effect belongs to the phaseId showing when it arrives (06 `sabotage`)', () => {
  it('attaches to the current screen\'s phaseId with its arrival time', () => {
    expect(attachEffect(freeze, 16, 1200)).toStrictEqual({
      phaseId: 16, effect: 'freeze', from: 'Papa', params: { taps: 6, blobs: 3, seconds: 3 }, arrivedAt: 1200, cleared: false,
    });
  });

  it('is ignored when no screen is showing yet', () => {
    expect(attachEffect(freeze, null, 1200)).toBeNull();
  });

  it('a new screen with the same phaseId (last call) keeps it; any other phaseId drops it', () => {
    const e = attachEffect(freeze, 16, 1200);
    expect(effectForScreen(e, 16)).toBe(e);
    expect(effectForScreen(e, 17)).toBeNull();
    expect(effectForScreen(null, 16)).toBeNull();
  });

  it('draws the overlay on an answer screen of its phaseId until cleared', () => {
    const e = attachEffect(freeze, 16, 1200);
    expect(overlayShown(e, screen('answerChoice', 16))).toBe(true);
    expect(overlayShown(e, screen('answerSlider', 16))).toBe(true);
    expect(overlayShown(e, screen('answerTrueFalse', 16))).toBe(true);
    expect(overlayShown(e, screen('answerChoice', 17))).toBe(false);
    expect(overlayShown(e, screen('waiting', 16))).toBe(false);
    expect(overlayShown(e, screen('watch', 16))).toBe(false);
    expect(overlayShown(e, null)).toBe(false);
    expect(overlayShown(null, screen('answerChoice', 16))).toBe(false);
    const done = clearEffect(e, 3000)!.effect;
    expect(overlayShown(done, screen('answerChoice', 16))).toBe(false);
  });

  it('clearing gives the whole ms since the effect arrived, once', () => {
    const e = attachEffect(freeze, 16, 1200.4);
    const first = clearEffect(e, 3541.2);
    expect(first).toMatchObject({ ms: 2341 });
    expect(first!.effect.cleared).toBe(true);
    expect(clearEffect(first!.effect, 4000)).toBeNull();
    expect(clearEffect(null, 4000)).toBeNull();
  });

  it('names the attacker in the player\'s language', () => {
    expect(effectTextKey('freeze')).toBe('sabotage.freeze');
    expect(effectTextKey('slime')).toBe('sabotage.slime');
    expect(effectTextKey('fog')).toBe('sabotage.fog');
  });
});
