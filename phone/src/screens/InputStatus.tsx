// "Отправлено / Sent" with a small spinner, then a check mark when the ack arrives (06, general rules).
// The round trip is shown in small text for the Day 1 check "ack back within 300 ms".

import { t } from '../i18n';
import { inputStatusKey } from '../inputText';
import type { TrackedInput } from '../net/inputs';
import type { Lang } from '../protocol';
import { CheckMark, Spinner } from './Icons';

export function InputStatus(props: { lang: Lang; input: TrackedInput | null; class?: string }) {
  const { lang, input } = props;
  const extra = props.class ? ` ${props.class}` : '';
  if (!input) return <div class={`input-status${extra}`} />;
  const text = t(lang, inputStatusKey(input));
  switch (input.state) {
    case 'sent':
      return (
        <div class={`input-status${extra}`}>
          <Spinner />
          <span>{text}</span>
        </div>
      );
    case 'acked':
      return (
        <div class={`input-status ok${extra}`}>
          <CheckMark label={text} />
          <small>{t(lang, 'input.roundTrip', { ms: input.roundTripMs ?? 0 })}</small>
        </div>
      );
    case 'refused':
      return (
        <div class={`input-status muted${extra}`}>
          <span>{text}</span>
        </div>
      );
    case 'noConnection':
      return (
        <div class={`input-status bad${extra}`}>
          <span>{text}</span>
        </div>
      );
  }
}
