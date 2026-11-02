// Top bar on every screen during a game (06): the player's name on their character chip, the score, and the
// gear for settings. On answer screens it also carries the countdown (a number and a thin bar underneath).

import { characterColor } from '../characters';
import { t } from '../i18n';
import type { Lang, PlayerView } from '../protocol';
import { GearIcon } from './Icons';
import { PressButton } from './PressButton';

export interface TopBarTimer {
  seconds: number;
  /** 0 to 1, for the bar. */
  share: number;
}

interface Props {
  lang: Lang;
  player: PlayerView | null;
  timer?: TopBarTimer | null;
  onSettings(): void;
}

export function TopBar(props: Props) {
  const { lang, player, timer } = props;
  return (
    <header class="top-bar">
      <div class="top-row">
        {player ? (
          <span class="chip top-chip" style={{ background: characterColor(player.characterId) }}>
            {player.name}
          </span>
        ) : (
          <span class="top-chip-space" />
        )}
        {timer ? <span class="top-timer">{timer.seconds}</span> : null}
        <span class="top-score">{player ? t(lang, 'topBar.score', { score: player.score }) : ''}</span>
        <PressButton class="gear-button" ariaLabel={t(lang, 'topBar.settings')} onClick={props.onSettings}>
          <GearIcon />
        </PressButton>
      </div>
      {timer ? (
        <div class="timer-track">
          <div class="timer-fill" style={{ transform: `scaleX(${timer.share})` }} />
        </div>
      ) : null}
    </header>
  );
}
