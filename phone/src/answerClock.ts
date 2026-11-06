// The answer clock (`ms`): starts when the options first appear for a phaseId.
// A new screen with the same phaseId (last call) never restarts it (PROTOCOL.md section 3, D28).
// phaseIds start again at 1 when the TV app restarts, so a screen with another phaseId forgets every older start:
// a reused phaseId must never measure from the earlier game's question.
// The time this phone showed the `paused` screen after the options appeared is left out (D112): a pause never costs
// speed points. On Continue the TV re-sends the current screen with the same phaseId, which keeps the start.

import type { PhaseId, ScreenState } from './protocol';

interface Start {
  /** When the options appeared. */
  at: number;
  /** Milliseconds of finished pauses since then. */
  pausedMs: number;
}

export class AnswerClock {
  private readonly starts = new Map<PhaseId, Start>();
  /** When the current pause began; null while not paused. */
  private pausedSince: number | null = null;

  constructor(private readonly now: () => number) {}

  /** Call when the options are drawn. Only the first call per phaseId counts. */
  shown(phaseId: PhaseId): void {
    if (!this.starts.has(phaseId)) this.starts.set(phaseId, { at: this.now(), pausedMs: 0 });
  }

  /** Call for every screen that arrives: starts kept only for its phaseId. */
  arrived(phaseId: PhaseId): void {
    for (const id of [...this.starts.keys()]) if (id !== phaseId) this.starts.delete(id);
  }

  /** Call when the `paused` screen arrives. A second call while paused changes nothing. */
  pauseBegan(): void {
    if (this.pausedSince === null) this.pausedSince = this.now();
  }

  /** Call when any other screen arrives: the pause just ended is left out of every running answer time. */
  pauseEnded(): void {
    if (this.pausedSince === null) return;
    for (const start of this.starts.values()) start.pausedMs += this.pausedSpan(start);
    this.pausedSince = null;
  }

  /** Forgets every start and any pause (back to join, or another room). */
  reset(): void {
    this.starts.clear();
    this.pausedSince = null;
  }

  /** Whole milliseconds since the options appeared for this phaseId, pauses left out (0 if they never did). */
  ms(phaseId: PhaseId): number {
    const start = this.starts.get(phaseId);
    if (start === undefined) return 0;
    const paused = start.pausedMs + (this.pausedSince === null ? 0 : this.pausedSpan(start));
    return Math.max(0, Math.round(this.now() - start.at - paused));
  }

  /** The part of the current pause after this start. */
  private pausedSpan(start: Start): number {
    if (this.pausedSince === null) return 0;
    return Math.max(0, this.now() - Math.max(this.pausedSince, start.at));
  }
}

/** Screen ids that show answer options; drawing one starts the clock for its phaseId. */
const ANSWER_SCREENS: readonly string[] = ['answerChoice', 'answerSlider', 'answerTrueFalse'];

/** Called when a screen is drawn. After a page reload this is the restored screen, so the clock starts then. */
export function screenDrawn(clock: AnswerClock, screen: ScreenState): void {
  if (ANSWER_SCREENS.includes(screen.id)) clock.shown(screen.phaseId);
}
