// Screen `lobby` (06, D76): "Ты в игре! / You're in!", the players as character chips with a ready mark each,
// and one big toggle: Ready, or Not ready once tapped. The TV app starts the countdown when everyone is ready.

import { characterColor } from '../characters';
import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import type { Lang, ReadyPlayer } from '../protocol';
import { CheckMark } from './Icons';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  players: ReadyPlayer[];
  /** This player is ready: the last tap until the TV's next screen, then the TV's flag. */
  ready: boolean;
  enoughPlayers: boolean;
  readyInput: TrackedInput | null;
  onToggle(): void;
}

export function LobbyScreen(props: Props) {
  const { lang, players, ready, enoughPlayers } = props;
  const status = !enoughPlayers ? 'lobby.waitingPlayers' : ready ? 'lobby.waitingOthers' : null;

  return (
    <div class="body lobby">
      <h1 class="lobby-title">
        {t(lang, 'lobby.title')}
        <span class="hand lobby-accent">{t(lang, 'lobby.accent')}</span>
      </h1>

      <ul class="chips lobby-chips">
        {players.map((p) => (
          <li
            key={p.playerId}
            class={p.ready ? 'chip ready-chip' : 'chip'}
            style={{ background: characterColor(p.characterId) }}
          >
            {p.ready ? <CheckMark label={t(lang, 'lobby.ready')} /> : null}
            <span class="chip-name">{p.name}</span>
          </li>
        ))}
      </ul>

      <div class="bottom-block">
        <div class="input-status muted">{status ? t(lang, status) : null}</div>
        <PressButton class={ready ? 'big' : 'primary big'} ariaPressed={ready} onClick={props.onToggle}>
          {t(lang, ready ? 'lobby.notReady' : 'lobby.ready')}
        </PressButton>
        <InputStatus lang={lang} input={props.readyInput} />
      </div>
    </div>
  );
}
