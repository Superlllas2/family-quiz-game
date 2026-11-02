// The answer clock (`ms`): starts when the options first appear for a phaseId.
// A new screen with the same phaseId (last call) never restarts it (PROTOCOL.md section 3, D28).
// phaseIds start again at 1 when the TV app restarts, so a screen with another phaseId forgets every older start:
// a reused phaseId must never measure from the earlier game's question.

import type { PhaseId, ScreenState } from './protocol';

export class AnswerClock {
  private readonly shownAt = new Map<PhaseId, number>();

  constructor(private readonly now: () => number) {}

  /** Call when the options are drawn. Only the first call per phaseId counts. */
  shown(phaseId: PhaseId): void {
    if (!this.shownAt.has(phaseId)) this.shownAt.set(phaseId, this.now());
  }

  /** Call for every screen that arrives: starts kept only for its phaseId. */
  arrived(phaseId: PhaseId): void {
    for (const id of [...this.shownAt.keys()]) if (id !== phaseId) this.shownAt.delete(id);
  }

  /** Forgets every start (back to join, or another room). */
  reset(): void {
    this.shownAt.clear();
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
