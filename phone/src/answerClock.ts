// The answer clock (`ms`): starts when the options first appear for a phaseId.
// A new screen with the same phaseId (last call) never restarts it (PROTOCOL.md section 3, D28).

import type { PhaseId, ScreenState } from './protocol';

export class AnswerClock {
  private readonly shownAt = new Map<PhaseId, number>();

  constructor(private readonly now: () => number) {}

  /** Call when the options are drawn. Only the first call per phaseId counts. */
  shown(phaseId: PhaseId): void {
    if (!this.shownAt.has(phaseId)) this.shownAt.set(phaseId, this.now());
  }

  /** Whole milliseconds since the options appeared for this phaseId (0 if they never did). */
  ms(phaseId: PhaseId): number {
    const start = this.shownAt.get(phaseId);
    if (start === undefined) return 0;
    return Math.max(0, Math.round(this.now() - start));
  }
}

/** Screen ids that show answer options; drawing one starts the clock for its phaseId. */
const ANSWER_SCREENS: readonly string[] = ['answerChoice'];

/** Called when a screen is drawn. After a page reload this is the restored screen, so the clock starts then. */
export function screenDrawn(clock: AnswerClock, screen: ScreenState): void {
  if (ANSWER_SCREENS.includes(screen.id)) clock.shown(screen.phaseId);
}
