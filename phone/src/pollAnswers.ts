// The poll's answers (20_playtest.md, PROTOCOL.md section 3 `poll` answers): question id to an option index for
// `choice`, a string for `text` (trimmed, at most POLL_TEXT_MAX characters). Unanswered questions, blank texts and
// answers that do not fit their question are left out.

import { POLL_TEXT_MAX } from './constants';
import type { PollAnswers, PollQuestion } from './protocol';

/** What the player has entered so far, by question id (text as typed, not trimmed yet). */
export type PollDraft = Readonly<Record<string, number | string>>;

/** At most POLL_TEXT_MAX characters, never ending in half of a surrogate pair. */
function cap(text: string): string {
  if (text.length <= POLL_TEXT_MAX) return text;
  const cut = text.slice(0, POLL_TEXT_MAX);
  const lastCode = cut.charCodeAt(cut.length - 1);
  return lastCode >= 0xd800 && lastCode <= 0xdbff ? cut.slice(0, -1) : cut;
}

export function chooseOption(draft: PollDraft, id: string, index: number): PollDraft {
  return { ...draft, [id]: index };
}

export function writeText(draft: PollDraft, id: string, text: string): PollDraft {
  return { ...draft, [id]: cap(text) };
}

/** The `answers` object to send, in question order. */
export function pollAnswers(questions: readonly PollQuestion[], draft: PollDraft): PollAnswers {
  const answers: PollAnswers = {};
  for (const q of questions) {
    const a = draft[q.id];
    if (q.kind === 'choice') {
      if (typeof a === 'number' && Number.isInteger(a) && a >= 0 && a < q.options.length) answers[q.id] = a;
    } else if (typeof a === 'string') {
      const text = cap(a.trim()).trim();
      if (text !== '') answers[q.id] = text;
    }
  }
  return answers;
}

/** The page index inside 0 to count - 1 (0 when there are no questions). */
export function clampPage(page: number, count: number): number {
  return Math.max(0, Math.min(count - 1, Math.floor(page)));
}
