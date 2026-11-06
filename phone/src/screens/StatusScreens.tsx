// Screens `reconnecting` and `error` (06) driven by the connection, outside a game screen (no top bar).

import { t, type TextKey } from '../i18n';
import type { ErrorReason } from '../net/session';
import type { Lang } from '../protocol';
import { Spinner } from './Icons';
import { PressButton } from './PressButton';

export function ReconnectingScreen(props: { lang: Lang }) {
  return (
    <div class="screen center">
      <Spinner large />
      <p class="big-text">{t(props.lang, 'reconnecting.text')}</p>
    </div>
  );
}

const ERROR_TEXT: Record<ErrorReason, TextKey> = {
  otherTab: 'error.otherTab',
  roomNotFound: 'error.roomNotFound',
  roomFull: 'error.roomFull',
  nameTaken: 'error.nameTaken',
  badName: 'error.badName',
  gameStarted: 'error.gameStarted',
  badVersion: 'error.badVersion',
  unknown: 'error.unknown',
};

export function ErrorScreen(props: { lang: Lang; reason: ErrorReason; onBack(): void }) {
  return (
    <div class="screen center">
      <p class="big-text note-card">{t(props.lang, ERROR_TEXT[props.reason])}</p>
      <PressButton class="primary big" onClick={props.onBack}>
        {t(props.lang, 'error.back')}
      </PressButton>
    </div>
  );
}
