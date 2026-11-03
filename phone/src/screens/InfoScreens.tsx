// Screens without a decision (06): `getReady` (except its Intro prediction), `waiting`, `result`, `roundEnd`,
// `raceResult`, `results`, a TV sent `error`, and a neutral "Look at the TV" for screen ids this build does not draw.
// Texts come from the TV's screen data or the locales.

import { characterColor } from '../characters';
import { wholeSeconds } from '../countdown';
import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import {
  getReadyData, raceResultData, resultData, resultsData, roundEndData, textData, waitingData, type Lang,
  type ScreenState,
} from '../protocol';
import { raceTrack } from '../raceTrack';
import { roundEndNext } from '../roundEnd';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';
import { useRemaining } from './useRemaining';

interface Props { lang: Lang; screen: ScreenState }

/**
 * `getReady`: a short text from the TV (for QuestionShow: the question). In the Intro the TV also sends `prediction`
 * (its text already asks "Кто победит? / Who wins tonight?"): the players as character chips, one tap sends the
 * prediction and marks that chip, the others dim. The screen stays until the TV sends the next one.
 */
export function GetReadyScreen(props: Props & {
  /** The playerId predicted on this phaseId, if any. */
  predicted: string | null;
  predictionInput: TrackedInput | null;
  onPredict(playerId: string): void;
}) {
  const { lang, predicted } = props;
  const { text, prediction } = getReadyData(props.screen.data);
  return (
    <div class="body center">
      <p class="big-text">{text}</p>
      {prediction.length > 0 ? (
        <div class="prediction">
          <div class="prediction-chips">
            {prediction.map((p) => {
              const state = predicted === null ? '' : predicted === p.playerId ? ' chosen' : ' dim';
              return (
                <PressButton
                  key={p.playerId}
                  class={`chip prediction-chip${state}`}
                  style={{ background: characterColor(p.characterId) }}
                  ariaPressed={predicted === p.playerId}
                  onPress={() => props.onPredict(p.playerId)}
                >
                  {p.name}
                </PressButton>
              );
            })}
          </div>
          <InputStatus lang={lang} input={props.predictionInput} />
        </div>
      ) : null}
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
      {r.correctText ? <p class="big-text">{r.correctText}</p> : null}
      <div class="result-stats">
        {r.streak > 0 ? <span>{t(lang, 'result.streak', { streak: r.streak })}</span> : null}
        {r.rank !== null ? <span>{t(lang, 'result.rank', { rank: r.rank })}</span> : null}
      </div>
    </div>
  );
}

/** `roundEnd`: own rank and score, and what comes next: "Раунд 3 / Round 3 next" or the final race. */
export function RoundEndScreen(props: Props) {
  const { lang } = props;
  const r = roundEndData(props.screen.data);
  const next = roundEndNext(r.nextRound);
  return (
    <div class="body center">
      {r.rank !== null ? <p class="result-head">{t(lang, 'roundEnd.rank', { rank: r.rank })}</p> : null}
      <p class="big-text">{t(lang, 'roundEnd.score', { score: r.score })}</p>
      <p class="next-text">{t(lang, next.key, next.params)}</p>
    </div>
  );
}

/**
 * `raceResult`: "Верно, +1 шаг / Right, +1 step" or "Мимо / Missed", and the player's step on a small track of
 * `finish` steps (the own step in the player's character color).
 */
export function RaceResultScreen(props: Props & { characterId: number | null }) {
  const { lang } = props;
  const r = raceResultData(props.screen.data);
  const cells = raceTrack(r.position, r.finish);
  const color = props.characterId === null ? undefined : characterColor(props.characterId);
  return (
    <div class="body center">
      <p class={r.correct ? 'result-head good' : 'result-head bad'}>
        {t(lang, r.correct ? 'race.right' : 'race.missed')}
      </p>
      {cells.length > 0 ? (
        <div class="race">
          <ol class="race-track" aria-hidden="true">
            {cells.map((cell, step) => (
              <li
                key={step}
                class={`race-cell ${cell}${step === cells.length - 1 ? ' finish' : ''}`}
                style={cell === 'here' && color ? { background: color } : undefined}
              />
            ))}
          </ol>
          <p class="muted-text">{t(lang, 'race.step', { position: r.position, finish: r.finish })}</p>
        </div>
      ) : null}
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
