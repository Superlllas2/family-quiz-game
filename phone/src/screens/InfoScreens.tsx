// Screens without a decision (06, D88): `watch` between inputs, `waiting` after one, `results` for the winner, and a
// TV sent `error`. Texts come from the TV's screen data or the locales. None of them uses hooks, so tests can call them
// as plain functions.

import { characterColor } from '../characters';
import { t } from '../i18n';
import { resultsData, textData, waitingData, type Lang, type PlayerView, type ScreenState } from '../protocol';

interface Props { lang: Lang; screen: ScreenState }

/**
 * `watch`: the player's character chip and "Смотри на экран / Eyes on the TV". Also drawn for any screen id the phone
 * does not draw (shownScreen.ts) and before the first screen arrives. No chip while the player is not known yet.
 */
export function WatchScreen(props: { lang: Lang; player: PlayerView | null }) {
  const { lang, player } = props;
  return (
    <div class="body center">
      {player ? (
        <span class="chip watch-chip" style={{ background: characterColor(player.characterId) }}>
          {player.name}
        </span>
      ) : null}
      <p class="big-text">{t(lang, 'watch.text')}</p>
    </div>
  );
}

/** `waiting`: what was locked. No list of who still answers (D88). */
export function WaitingScreen(props: Props) {
  const { locked } = waitingData(props.screen.data);
  return (
    <div class="body center">
      <p class="big-text">{t(props.lang, 'waiting.locked', { locked })}</p>
    </div>
  );
}

/**
 * `results`, the winner only (D87): "Победа! / You won!", "Твоя награда — интересный факт / Your prize: a fun fact",
 * then the fact from the TV, already in the player's language.
 */
export function ResultsScreen(props: Props) {
  const { lang } = props;
  const { fact } = resultsData(props.screen.data);
  return (
    <div class="body center">
      <p class="result-head good">{t(lang, 'results.won')}</p>
      <p class="muted-text">{t(lang, 'results.prize')}</p>
      {fact ? <p class="big-text fact-text">{fact}</p> : null}
    </div>
  );
}

/** `error` sent by the TV app while online: its text only. The TV decides what comes next. */
export function TvErrorScreen(props: Props) {
  return (
    <div class="body center">
      <p class="big-text">{textData(props.screen.data).text}</p>
    </div>
  );
}
