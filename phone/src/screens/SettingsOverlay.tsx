// Screen `settings` (06): an overlay opened from the gear, over the current screen (which stays drawn, so the
// answer clock keeps running). Language RU or EN, "More time" 1x, 1.5x or 2x, Leave game (with a confirm step).
// The language applies on this phone at once; both go to the TV app in a `settings` message (no ack).
// The current "More time" value is the one in the `player` object.

import { useState } from 'preact/hooks';
import { TIMER_MULTIPLIERS } from '../constants';
import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import type { Lang } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  timerMultiplier: number;
  leaveInput: TrackedInput | null;
  onLang(lang: Lang): void;
  onMultiplier(value: number): void;
  onLeave(): void;
  onClose(): void;
}

export function SettingsOverlay(props: Props) {
  const { lang, timerMultiplier } = props;
  const [confirming, setConfirming] = useState(false);
  const leaving = props.leaveInput?.state === 'sent' || props.leaveInput?.state === 'acked';

  return (
    <div class="overlay" role="dialog" aria-label={t(lang, 'settings.title')}>
      <div class="screen settings">
        <h1>{t(lang, 'settings.title')}</h1>

        <section class="setting">
          <span class="setting-label">{t(lang, 'settings.language')}</span>
          <div class="seg-group" role="group">
            {(['ru', 'en'] as const).map((l) => (
              <PressButton
                key={l}
                class={l === lang ? 'seg selected' : 'seg'}
                ariaPressed={l === lang}
                onClick={() => props.onLang(l)}
              >
                {t(lang, l === 'ru' ? 'lang.ru' : 'lang.en')}
              </PressButton>
            ))}
          </div>
        </section>

        <section class="setting">
          <span class="setting-label">{t(lang, 'settings.moreTime')}</span>
          <div class="seg-group" role="group">
            {TIMER_MULTIPLIERS.map((m) => (
              <PressButton
                key={m}
                class={m === timerMultiplier ? 'seg selected' : 'seg'}
                ariaPressed={m === timerMultiplier}
                onClick={() => props.onMultiplier(m)}
              >
                {t(lang, 'settings.multiplier', { value: m.toLocaleString(lang) })}
              </PressButton>
            ))}
          </div>
        </section>

        <section class="setting leave">
          {confirming ? (
            <>
              <p class="leave-text">{t(lang, 'settings.leaveConfirm')}</p>
              <div class="leave-row">
                <PressButton class="big" disabled={leaving} onClick={() => setConfirming(false)}>
                  {t(lang, 'settings.leaveNo')}
                </PressButton>
                <PressButton class="big danger" disabled={leaving} onClick={props.onLeave}>
                  {t(lang, 'settings.leaveYes')}
                </PressButton>
              </div>
              <InputStatus lang={lang} input={props.leaveInput} />
            </>
          ) : (
            <PressButton class="big danger-outline" onClick={() => setConfirming(true)}>
              {t(lang, 'settings.leave')}
            </PressButton>
          )}
        </section>

        <PressButton class="primary big settings-close" onClick={props.onClose}>
          {t(lang, 'settings.close')}
        </PressButton>
      </div>
    </div>
  );
}
