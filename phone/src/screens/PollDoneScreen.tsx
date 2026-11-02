// Screen `pollDone` (06): "Спасибо! / Thanks!" until the poll phase is over; then the TV sends `lobby` (D76).

import { t } from '../i18n';
import type { Lang } from '../protocol';

export function PollDoneScreen(props: { lang: Lang }) {
  return (
    <div class="body center">
      <p class="result-head">{t(props.lang, 'pollDone.thanks')}</p>
    </div>
  );
}
