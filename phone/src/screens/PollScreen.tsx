// Screen `poll` (06, 20_playtest.md): one question per page with a page indicator (1/7), Back and Next, and Submit on
// the last page. A choice question has one button per option (a tap selects, another tap changes it); a text question
// has a text box of at most 200 characters. Questions may be skipped. Submit sends once; the TV app then sends
// `pollDone`. The answers live in app.tsx (pollAnswers.ts builds what is sent); this view uses no hooks.

import { POLL_TEXT_MAX } from '../constants';
import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import { clampPage, type PollDraft } from '../pollAnswers';
import type { Lang, PollQuestion } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  questions: PollQuestion[];
  page: number;
  draft: PollDraft;
  /** Submit was tapped: everything is locked and the status shows. */
  sent: boolean;
  pollInput: TrackedInput | null;
  onPage(page: number): void;
  onChoose(id: string, index: number): void;
  onText(id: string, text: string): void;
  onSubmit(): void;
}

export function PollScreen(props: Props) {
  const { lang, questions, draft, sent } = props;
  const total = questions.length;
  const page = clampPage(props.page, total);
  const q: PollQuestion | undefined = questions[page];
  const last = page >= total - 1;

  let answer = null;
  if (q?.kind === 'choice') {
    const chosen = draft[q.id];
    answer = (
      <div class="poll-options">
        {q.options.map((option, i) => (
          <PressButton
            key={i}
            class={`poll-option${chosen === i ? ' selected' : ''}`}
            ariaPressed={chosen === i}
            disabled={sent}
            onPress={() => props.onChoose(q.id, i)}
          >
            {option}
          </PressButton>
        ))}
      </div>
    );
  } else if (q) {
    const text = typeof draft[q.id] === 'string' ? (draft[q.id] as string) : '';
    answer = (
      <div class="poll-text">
        <textarea
          class="poll-textarea"
          maxLength={POLL_TEXT_MAX}
          value={text}
          placeholder={t(lang, 'poll.textPlaceholder')}
          disabled={sent}
          aria-label={q.text}
          onInput={(e) => props.onText(q.id, (e.currentTarget as HTMLTextAreaElement).value)}
        />
        <div class="poll-text-foot">
          <span>{t(lang, 'poll.skip')}</span>
          <span>{t(lang, 'poll.count', { count: text.length, max: POLL_TEXT_MAX })}</span>
        </div>
      </div>
    );
  }

  return (
    <div class="body poll">
      {total > 0 ? <p class="poll-page">{t(lang, 'poll.page', { page: page + 1, total })}</p> : null}
      {q ? <h1 class="poll-question">{q.text}</h1> : null}
      {answer}
      <div class="bottom-block poll-bottom">
        <div class="poll-nav">
          <PressButton class="poll-back" disabled={page === 0} onPress={() => props.onPage(page - 1)}>
            {t(lang, 'poll.back')}
          </PressButton>
          {last ? (
            <PressButton class="primary poll-submit" disabled={sent} onPress={props.onSubmit}>
              {t(lang, 'poll.submit')}
            </PressButton>
          ) : (
            <PressButton class="primary poll-next" onPress={() => props.onPage(page + 1)}>
              {t(lang, 'poll.next')}
            </PressButton>
          )}
        </div>
        <InputStatus lang={lang} input={sent ? props.pollInput : null} />
      </div>
    </div>
  );
}
