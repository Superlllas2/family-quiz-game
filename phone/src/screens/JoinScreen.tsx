// Screen `join` (06): room code (filled from the URL), name, RU/EN toggle (default Russian), Join.

import { useState } from 'preact/hooks';
import { t } from '../i18n';
import { cleanName, cleanRoom, isValidName, isValidRoom } from '../names';
import type { Lang } from '../protocol';
import { Spinner } from './Icons';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  initialRoom: string;
  busy: boolean;
  noConnection: boolean;
  onLang(lang: Lang): void;
  onJoin(room: string, name: string): void;
}

export function JoinScreen(props: Props) {
  const { lang } = props;
  const [room, setRoom] = useState(props.initialRoom);
  const [name, setName] = useState('');

  const roomOk = isValidRoom(room);
  const nameOk = isValidName(name);
  const canJoin = roomOk && nameOk && !props.busy;

  const submit = (e: Event): void => {
    e.preventDefault();
    if (canJoin) props.onJoin(cleanRoom(room), cleanName(name));
  };

  return (
    <form class="screen join" onSubmit={submit} autocomplete="off">
      <div class="lang-toggle" role="group" aria-label={t(lang, 'join.langLabel')}>
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

      <h1>{t(lang, 'join.title')}</h1>

      <label class="field">
        <span>{t(lang, 'join.roomLabel')}</span>
        <input
          class="room-input"
          value={room}
          maxLength={4}
          inputMode="text"
          autoCapitalize="characters"
          autoCorrect="off"
          spellcheck={false}
          onInput={(e) => setRoom((e.currentTarget as HTMLInputElement).value.toUpperCase())}
        />
        <small class={room !== '' && !roomOk ? 'hint bad' : 'hint'}>{t(lang, 'join.roomHint')}</small>
      </label>

      <label class="field">
        <span>{t(lang, 'join.nameLabel')}</span>
        <input
          value={name}
          enterKeyHint="go"
          autoCapitalize="words"
          autoCorrect="off"
          spellcheck={false}
          onInput={(e) => setName((e.currentTarget as HTMLInputElement).value)}
        />
        <small class={name !== '' && !nameOk ? 'hint bad' : 'hint'}>{t(lang, 'join.nameHint')}</small>
      </label>

      <div class="join-bottom">
        <PressButton type="submit" class="primary big" disabled={!canJoin}>
          {props.busy ? (
            <span class="busy"><Spinner /> {t(lang, 'join.joining')}</span>
          ) : (
            t(lang, 'join.button')
          )}
        </PressButton>
        <p class="hint bad join-problem">{props.noConnection ? t(lang, 'join.noConnection') : ''}</p>
      </div>
    </form>
  );
}
