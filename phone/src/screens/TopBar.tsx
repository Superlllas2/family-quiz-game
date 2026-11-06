// Top bar on every screen during a game (06): the player's name on their character chip and the gear for settings.
// No score or rank: those are on the TV only (D88). On answer screens it also carries the countdown: a clock token, the
// whole seconds on a face whose wedge drains with the share of the window left (the TV's clock token).

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
        {timer ? (
          <span class="top-timer">
            <svg class="clock-face" viewBox="0 0 36 36" aria-hidden="true">
              <circle
                class="clock-wedge"
                cx="18"
                cy="18"
                r="9"
                pathLength={100}
                stroke-dasharray="100"
                stroke-dashoffset={String(100 * (1 - timer.share))}
                transform="rotate(-90 18 18)"
              />
            </svg>
            <b class="clock-number">{timer.seconds}</b>
          </span>
        ) : null}
        <PressButton class="gear-button" ariaLabel={t(lang, 'topBar.settings')} onClick={props.onSettings}>
          <GearIcon />
        </PressButton>
      </div>
    </header>
  );
}
