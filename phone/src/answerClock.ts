// The answer clock (`ms`): starts when the options first appear for a phaseId.
// A new screen with the same phaseId (last call) never restarts it (PROTOCOL.md section 3, D28).

import type { PhaseId } from './protocol';

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
