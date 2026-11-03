// Which sabotage overlay shows (06 `sabotage`, 03 "Power ups"). The TV app sends `effect` right after the Answer
// `screen` of the same phase; the effect belongs to the phaseId showing when it arrives. A new screen with the same
// phaseId (last call) keeps it, any other phaseId drops it. The overlay is drawn over the answer screen of that phaseId
// until the player clears it. The answer clock never stops for it (answerClock.ts is not touched here).

import type { TextKey } from './i18n';
import type { EffectMessage, EffectParams, PhaseId, PowerUpKind, ScreenState } from './protocol';

export interface ActiveEffect {
  phaseId: PhaseId;
  effect: PowerUpKind;
  /** The attacker's name. */
  from: string;
  params: EffectParams;
  /** performance.now() when the `effect` arrived: the fog clears from here, and `effectCleared.ms` counts from here. */
  arrivedAt: number;
  cleared: boolean;
}

/** The effect for the screen showing (`phaseId`, null before the first screen: then it is ignored). */
export function attachEffect(msg: EffectMessage, phaseId: PhaseId | null, now: number): ActiveEffect | null {
  if (phaseId === null) return null;
  return { phaseId, effect: msg.effect, from: msg.from, params: msg.params, arrivedAt: now, cleared: false };
}

/** What is left of the effect when a screen with `phaseId` arrives. */
export function effectForScreen(active: ActiveEffect | null, phaseId: PhaseId): ActiveEffect | null {
  return active && active.phaseId === phaseId ? active : null;
}

/** Screens the overlay covers: the answer screens (the race has no power ups, but nothing breaks if one comes). */
const COVERED: readonly string[] = ['answerChoice', 'answerSlider', 'answerTrueFalse'];

/** True while the overlay covers this screen: the effect's phaseId, an answer screen, not cleared yet. */
export function overlayShown(active: ActiveEffect | null, screen: ScreenState | null): boolean {
  return !!active && !!screen && !active.cleared && active.phaseId === screen.phaseId && COVERED.includes(screen.id);
}

/** Marks the effect cleared. Returns the whole ms since it arrived, or null if it was already cleared (send once). */
export function clearEffect(active: ActiveEffect | null, now: number): { effect: ActiveEffect; ms: number } | null {
  if (!active || active.cleared) return null;
  return { effect: { ...active, cleared: true }, ms: Math.max(0, Math.round(now - active.arrivedAt)) };
}

/** "{name} froze you!" and the others. */
export function effectTextKey(effect: PowerUpKind): TextKey {
  switch (effect) {
    case 'freeze':
      return 'sabotage.freeze';
    case 'slime':
      return 'sabotage.slime';
    case 'fog':
      return 'sabotage.fog';
  }
}
