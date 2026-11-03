// Screens without a decision (06): `getReady`, `waiting`, `result`, `results`, a TV sent `error`, and a neutral
// "Look at the TV" for screen ids this build does not draw. Texts come from the TV's screen data or the locales.

import { characterColor } from '../characters';
import { wholeSeconds } from '../countdown';
import { t } from '../i18n';
import {
  getReadyData, resultData, resultsData, textData, waitingData, type Lang, type ScreenState,
} from '../protocol';
import { useRemaining } from './useRemaining';

interface Props { lang: Lang; screen: ScreenState }

/** `getReady`: a short text from the TV (for QuestionShow: the question). `prediction` is not drawn yet. */
export function GetReadyScreen(props: Props) {
  const { text } = getReadyData(props.screen.data);
  return (
    <div class="body center">
      <p class="big-text">{text}</p>
    </div>
  );
}

/** `waiting`: what was locked, and chips of who still has to answer. */
export function WaitingScreen(props: Props) {
  const { lang } = props;
  const { locked, pending } = waitingData(props.screen.data);
  return (
    <div class="body center">
      <p class="big-text">{t(lang, 'waiting.locked', { locked })}</p>
      {pending.length > 0 ? (
        <div class="pending">
          <p class="muted-text">{t(lang, 'waiting.pending')}</p>
          <ul class="chips">
            {pending.map((p, i) => (
              <li key={`${p.name}-${i}`} class="chip" style={{ background: characterColor(p.characterId) }}>
                {p.name}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </div>
  );
}

/** `result`: right or wrong with the points, streak, rank and the right answer. Power ups come on Day 4. */
export function ResultScreen(props: Props) {
  const { lang } = props;
  const r = resultData(props.screen.data);
  return (
    <div class="body center">
      <p class={r.correct ? 'result-head good' : 'result-head bad'}>
        {r.correct ? t(lang, 'result.correct', { points: r.points }) : t(lang, 'result.wrong')}
      </p>
      {r.correctText ? <p class="big-text">{t(lang, 'result.correctText', { text: r.correctText })}</p> : null}
      <div class="result-stats">
        {r.streak > 0 ? <span>{t(lang, 'result.streak', { streak: r.streak })}</span> : null}
        {r.rank !== null ? <span>{t(lang, 'result.rank', { rank: r.rank })}</span> : null}
      </div>
    </div>
  );
}

/**
 * `results`: own place, correct answers, the awards won (award texts come from the TV), and how long until
 * everyone is back in the Lobby for the next game (from `endsInMs`), so the jump never comes as a surprise.
 */
export function ResultsScreen(props: Props & { receivedAt: number; now: () => number }) {
  const { lang } = props;
  const r = resultsData(props.screen.data);
  const remaining = useRemaining(props.screen.endsInMs, props.receivedAt, props.now);
  return (
    <div class="body center">
      {r.place !== null ? <p class="result-head">{t(lang, 'results.place', { place: r.place })}</p> : null}
      <p class="big-text">{t(lang, 'results.correctCount', { count: r.correctCount })}</p>
      {r.awards.length > 0 ? (
        <div class="awards">
          <p class="muted-text">{t(lang, 'results.awards')}</p>
          <ul class="award-list">
            {r.awards.map((a, i) => <li key={i}>{a}</li>)}
          </ul>
        </div>
      ) : null}
      {remaining !== null ? (
        <p class="muted-text">{t(lang, 'results.lobbyIn', { seconds: Math.max(1, wholeSeconds(remaining)) })}</p>
      ) : null}
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

/** Any other screen id: nothing to decide on the phone. */
export function WatchTvBody(props: { lang: Lang }) {
  return (
    <div class="body center">
      <p class="big-text">{t(props.lang, 'screen.watchTv')}</p>
    </div>
  );
}
