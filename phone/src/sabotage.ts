// Which sabotage overlay shows (06 `sabotage`, 03 "Power ups"). The TV app sends one `effect` per hit, in order, right
// after the Answer `screen` of the same phase (up to `powerUps.maxHitsPerQuestion`, D145); after a reload it sends
// every effect not cleared yet, in order, right after the snapshot. An effect belongs to the phaseId showing when it
// arrives. The effects of one phaseId play one after another (D145): the first shows from its arrival; when it is
// cleared the next one starts showing at once, with its own text and its own fog timer. A new screen with the same
// phaseId (last call) keeps the queue, any other phaseId drops it. The overlay is drawn over the answer screen of that
// phaseId and takes every tap until the queue is empty. The TV only accepts clears in order, so only the effect showing
// can be cleared. The answer clock never stops for any of this (answerClock.ts is not touched here).
// The cap (D148): each effect carries `capMs`, the sabotage time left from its arrival. When that runs out every effect
// still waiting melts away at once, without an `effectCleared` (the TV knows the cap).

import type { TextKey } from './i18n';
import type { EffectMessage, EffectParams, PhaseId, PowerUpKind, ScreenState } from './protocol';

export interface QueuedEffect {
  /** 0, 1, 2 in arrival order within the phaseId: the overlay's key, so each effect gets a fresh layer. */
  seq: number;
  effect: PowerUpKind;
  /** The attacker's name. */
  from: string;
  params: EffectParams;
}

/** The effect showing: `startedAt` is when it started showing (the fog counts from here, and `effectCleared.ms`). */
export interface ShownEffect extends QueuedEffect {
  startedAt: number;
}

export interface EffectQueue {
  phaseId: PhaseId;
  /** The effects not cleared yet, in arrival order; the first one is showing. */
  waiting: readonly QueuedEffect[];
  /** performance.now() when the first of `waiting` started showing. */
  shownAt: number;
  /** The seq of the next effect to arrive. */
  nextSeq: number;
  /** performance.now() when the cap runs out and every effect left melts (D148). */
  capEndsAt: number;
}

/**
 * Adds an `effect` for the screen showing (`phaseId`; null before the first screen: then it is ignored). It shows at
 * once if nothing else is showing, else it waits its turn.
 */
export function addEffect(
  queue: EffectQueue | null, msg: EffectMessage, phaseId: PhaseId | null, now: number,
): EffectQueue | null {
  if (phaseId === null) return queue;
  const capEndsAt = now + msg.params.capMs;
  const q: EffectQueue = queue && queue.phaseId === phaseId
    ? queue
    : { phaseId, waiting: [], shownAt: now, nextSeq: 0, capEndsAt };
  const item: QueuedEffect = { seq: q.nextSeq, effect: msg.effect, from: msg.from, params: msg.params };
  return {
    phaseId,
    waiting: [...q.waiting, item],
    shownAt: q.waiting.length === 0 ? now : q.shownAt,
    nextSeq: q.nextSeq + 1,
    capEndsAt: Math.min(q.capEndsAt, capEndsAt),
  };
}

/** At or after the cap (D148): the queue with every effect gone; before it, the queue as it is. */
export function meltIfDue(queue: EffectQueue | null, now: number): EffectQueue | null {
  if (!queue || queue.waiting.length === 0 || now < queue.capEndsAt) return queue;
  return { ...queue, waiting: [], shownAt: now };
}

/** What is left of the queue when a screen with `phaseId` arrives. */
export function queueForScreen(queue: EffectQueue | null, phaseId: PhaseId): EffectQueue | null {
  return queue && queue.phaseId === phaseId ? queue : null;
}

/** The effect showing now, or null when none is left. */
export function shownEffect(queue: EffectQueue | null): ShownEffect | null {
  const first = queue?.waiting[0];
  return queue && first ? { ...first, startedAt: queue.shownAt } : null;
}

/** Screens the overlay covers: the answer screens (the race has no power ups, but nothing breaks if one comes). */
const COVERED: readonly string[] = ['answerChoice', 'answerSlider', 'answerTrueFalse'];

/** True while the overlay covers this screen: the queue's phaseId, an answer screen, an effect not cleared yet. */
export function overlayShown(queue: EffectQueue | null, screen: ScreenState | null): boolean {
  return !!queue && !!screen && queue.waiting.length > 0 && queue.phaseId === screen.phaseId
    && COVERED.includes(screen.id);
}

/**
 * Clears the effect showing, if it is `seq` (a late call for an effect already cleared does nothing). Returns the
 * queue with the next effect showing from `now`, the cleared effect and the whole ms since it started showing.
 */
export function clearShown(
  queue: EffectQueue | null, seq: number, now: number,
): { queue: EffectQueue; cleared: QueuedEffect; ms: number } | null {
  const first = queue?.waiting[0];
  if (!queue || !first || first.seq !== seq) return null;
  return {
    queue: { ...queue, waiting: queue.waiting.slice(1), shownAt: now },
    cleared: first,
    ms: Math.max(0, Math.round(now - queue.shownAt)),
  };
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
