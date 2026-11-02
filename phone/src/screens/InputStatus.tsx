// "Отправлено / Sent" with a small spinner, then a check mark when the ack arrives (06, general rules).
// The round trip is shown in small text for the Day 1 check "ack back within 300 ms".

import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import type { Lang } from '../protocol';
import { CheckMark, Spinner } from './Icons';

export function InputStatus(props: { lang: Lang; input: TrackedInput | null }) {
  const { lang, input } = props;
  if (!input) return <div class="input-status" />;
  switch (input.state) {
    case 'sent':
      return (
        <div class="input-status">
          <Spinner />
          <span>{t(lang, 'input.sent')}</span>
        </div>
      );
    case 'acked':
      return (
        <div class="input-status ok">
          <CheckMark label={t(lang, 'input.acked')} />
          <small>{t(lang, 'input.roundTrip', { ms: input.roundTripMs ?? 0 })}</small>
        </div>
      );
    case 'refused':
      return (
        <div class="input-status muted">
          <span>{t(lang, 'input.refused')}</span>
        </div>
      );
    case 'noConnection':
      return (
        <div class="input-status bad">
          <span>{t(lang, 'input.noConnection')}</span>
        </div>
      );
  }
}
