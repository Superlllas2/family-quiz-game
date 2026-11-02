// Screen `lobby` (06): "Ты в игре! / You're in!", the players as character chips, Start on every phone
// (enabled by `canStart`: 2 or more players).

import { characterColor } from '../characters';
import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import { lobbyData, type Lang, type ScreenState } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  screen: ScreenState;
  startInput: TrackedInput | null;
  onStart(): void;
}

export function LobbyScreen(props: Props) {
  const { lang, screen } = props;
  const { players, canStart } = lobbyData(screen.data);

  return (
    <div class="body lobby">
      <h1>{t(lang, 'lobby.title')}</h1>

      <ul class="chips lobby-chips">
        {players.map((p) => (
          <li key={p.playerId} class="chip" style={{ background: characterColor(p.characterId) }}>
            {p.name}
          </li>
        ))}
      </ul>

      <div class="bottom-block">
        <PressButton class="primary big" disabled={!canStart} onClick={props.onStart}>
          {t(lang, 'lobby.start')}
        </PressButton>
        {canStart ? (
          <InputStatus lang={lang} input={props.startInput} />
        ) : (
          <div class="input-status muted">{t(lang, 'lobby.waitingPlayers')}</div>
        )}
      </div>
    </div>
  );
}
