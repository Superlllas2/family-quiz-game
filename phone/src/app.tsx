// The phone page: draws exactly one screen, chosen from the session status and the last `screen` from the TV app.
// No game rules here; the TV app decides everything. The settings overlay can open over any game screen.

import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'preact/hooks';
import type { AnswerClock } from './answerClock';
import { remainingShare, wholeSeconds } from './countdown';
import { t } from './i18n';
import type { InputSender, TrackedInput } from './net/inputs';
import type { Session, SessionStatus } from './net/session';
import { chooseOption, pollAnswers, writeText, type PollDraft } from './pollAnswers';
import { addAttempt, armPick, canTarget, pickFor, pickView, type PickState } from './powerUpPick';
import {
  answerWindowMs, characterMessage, choiceInputMessage, effectClearedInputMessage, leaveMessage, lobbyData,
  lobbyMessage, playerView, pollData, pollMessage, powerUpInputMessage, rateData, rateInputMessage, resultData,
  passInputMessage, settingsMessage, trueFalseInputMessage, valueInputMessage, voteInputMessage,
  type AckedMessage, type IncomingMessage, type Lang, type PhaseId, type PlayerView, type ScreenState,
} from './protocol';
import { cardShown, closeCard, ratingFor, shownStars, tapStar, type RatingState } from './rating';
import { shownReady, toggleReady } from './readyToggle';
import { addEffect, clearShown, meltIfDue, overlayShown, queueForScreen, shownEffect, type EffectQueue } from './sabotage';
import { AnswerChoiceScreen } from './screens/AnswerChoiceScreen';
import { AnswerSliderScreen } from './screens/AnswerSliderScreen';
import { AnswerTrueFalseScreen } from './screens/AnswerTrueFalseScreen';
import { CountdownScreen } from './screens/CountdownScreen';
import { PausedScreen, ResultsScreen, TvErrorScreen, WaitingScreen, WatchScreen } from './screens/InfoScreens';
import { JoinScreen } from './screens/JoinScreen';
import { LobbyScreen } from './screens/LobbyScreen';
import { PickCharacterScreen } from './screens/PickCharacterScreen';
import { PollDoneScreen } from './screens/PollDoneScreen';
import { PollScreen } from './screens/PollScreen';
import { RatingCard } from './screens/RatingCard';
import { ResultScreen } from './screens/ResultScreen';
import { SabotageOverlay } from './screens/SabotageOverlay';
import { SettingsOverlay } from './screens/SettingsOverlay';
import { ErrorScreen, ReconnectingScreen } from './screens/StatusScreens';
import { TopicVoteScreen } from './screens/TopicVoteScreen';
import { TopBar } from './screens/TopBar';
import { useRemaining } from './screens/useRemaining';
import { shownScreen } from './shownScreen';

export interface AppDeps {
  session: Session;
  sender: InputSender;
  /** Subscribes to input state changes from the sender. */
  onInputChange(listener: (input: TrackedInput) => void): () => void;
  clock: AnswerClock;
  /** performance.now(): when a screen message arrived, for the countdowns. */
  now(): number;
  initialRoom: string;
  initialLang: Lang;
  saveLang(lang: Lang): void;
  newInputId(): string;
}

/** The last screen from the TV app and when it arrived. */
interface Current { screen: ScreenState; receivedAt: number }

/**
 * One button's input on one screen: its status shows only while that screen's phaseId is current. `answer` is the one
 * decision of a game phase: a vote, a choice, a value or true or false.
 */
type Slot = 'ready' | 'character' | 'answer';
interface SlotInput { inputId: string; phaseId: PhaseId; characterId?: number }

/** The poll in progress on one phaseId: the page shown, the answers so far and the submit, if sent. */
interface PollState { phaseId: PhaseId; page: number; draft: PollDraft; inputId: string | null }

/** Answer screens: drawn inside the answer area, where a sabotage overlay can cover them. */
const ANSWER_SCREENS: readonly string[] = ['answerChoice', 'answerSlider', 'answerTrueFalse'];

export function App({ deps }: { deps: AppDeps }) {
  const { session, sender, clock } = deps;
  const [status, setStatus] = useState<SessionStatus>(() => session.status);
  const [current, setCurrent] = useState<Current | null>(null);
  const [player, setPlayer] = useState<PlayerView | null>(null);
  const [lang, setLangState] = useState<Lang>(deps.initialLang);
  const [inputs, setInputs] = useState<Record<string, TrackedInput>>({});
  const [slots, setSlots] = useState<Partial<Record<Slot, SlotInput>>>({});
  /** What this phone sent for the current phase's one decision (door, choice, value, true or false). */
  const [answer, setAnswer] = useState<{ phaseId: PhaseId; value: number | boolean | 'pass' } | null>(null);
  /** The last Ready or Not ready tap, shown at once until the TV's next screen (D76). */
  const [readyTap, setReadyTap] = useState<boolean | null>(null);
  /** The phaseId already answered: set at once, so a second tap before the re-render is ignored too. */
  const answeredPhase = useRef<PhaseId | null>(null);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [leaveId, setLeaveId] = useState<string | null>(null);
  /**
   * The sabotage effects on this phone, played one after another, tied to the phaseId showing when they arrived
   * (sabotage.ts, D145). A ref too: several effects arrive in one burst, and a clear must see the latest queue.
   */
  const [effects, setEffectsState] = useState<EffectQueue | null>(null);
  const effectsRef = useRef<EffectQueue | null>(null);
  const setEffects = (next: EffectQueue | null): void => {
    effectsRef.current = next;
    setEffectsState(next);
  };
  /** The phaseId of the last screen, read when an `effect` arrives (the message handler is made once). */
  const phaseRef = useRef<PhaseId | null>(null);
  /** The power up pick and the poll; refs too, so a second tap before the re-render sees the first one. */
  const [pick, setPickState] = useState<PickState | null>(null);
  const pickRef = useRef<PickState | null>(null);
  const [poll, setPollState] = useState<PollState | null>(null);
  const pollRef = useRef<PollState | null>(null);
  const setPick = (next: PickState | null): void => {
    pickRef.current = next;
    setPickState(next);
  };
  const setPoll = (next: PollState | null): void => {
    pollRef.current = next;
    setPollState(next);
  };
  /** The `paused` screen while the pause menu is open on the laptop (D105); `current` stays the screen under it. */
  const [paused, setPaused] = useState<ScreenState | null>(null);
  /** The rating card (D109): the taps and the close for one question; a ref too, for taps before the re-render. */
  const [rating, setRatingState] = useState<RatingState | null>(null);
  const ratingRef = useRef<RatingState | null>(null);
  const setRating = (next: RatingState | null): void => {
    ratingRef.current = next;
    setRatingState(next);
  };
  /** The last star tapped, so only that star pops; `seq` changes on every tap. */
  const [starPop, setStarPop] = useState<{ item: string; stars: number; seq: number } | null>(null);
  const popSeq = useRef(0);

  // Layout effects run during the first render's commit, before main.tsx starts the session.
  useLayoutEffect(() => {
    const showScreen = (next: ScreenState): void => {
      // `paused` (D105) goes over the current screen and changes nothing under it, whatever its phaseId: on Continue
      // the TV re-sends the current screen with the same phaseId, so the answer, the sabotage and the answer clock
      // carry on. The time shown paused is left out of the answer time (D112).
      if (next.id === 'paused') {
        setPaused(next);
        clock.pauseBegan();
        return;
      }
      setPaused(null);
      clock.pauseEnded();
      // Always a new arrival time, also for a last call with the same phaseId (the countdown re-syncs).
      setCurrent({ screen: next, receivedAt: deps.now() });
      clock.arrived(next.phaseId);
      // A last call (same phaseId) keeps the sabotage queue; any other phaseId drops it.
      phaseRef.current = next.phaseId;
      setEffects(queueForScreen(effectsRef.current, next.phaseId));
      // From here the TV's own ready flag shows again.
      setReadyTap(null);
      // The pick status lives as long as pickCharacter is showing: its refresh after characterTaken may or may
      // not carry a new phaseId, and the note must survive it.
      if (next.id !== 'pickCharacter') {
        setSlots((all) => {
          if (!all.character) return all;
          const { character: _gone, ...rest } = all;
          return rest;
        });
      }
    };
    const onMessage = (msg: IncomingMessage): void => {
      switch (msg.type) {
        case 'snapshot':
          // Right after a snapshot the TV sends again every effect not cleared yet, in order (D145): start over.
          setEffects(null);
          if (msg.player) setPlayer(playerView(msg.player));
          if (msg.screen) showScreen(msg.screen);
          break;
        case 'screen': {
          const { type: _type, ...next } = msg;
          showScreen(next);
          break;
        }
        case 'player':
          setPlayer(playerView(msg.player));
          break;
        case 'ack':
          sender.handleAck(msg);
          break;
        case 'effect':
          setEffects(addEffect(effectsRef.current, msg, phaseRef.current, deps.now()));
          break;
        default:
          break; // welcome and refused are handled by the session.
      }
    };
    const offSession = session.subscribe({
      status: (s) => {
        setStatus(s);
        if (s.kind === 'join' || s.kind === 'error') {
          setCurrent(null);
          setPlayer(null);
          setSettingsOpen(false);
          setAnswer(null);
          answeredPhase.current = null;
          clock.reset();
          phaseRef.current = null;
          setEffects(null);
          pickRef.current = null;
          setPickState(null);
          pollRef.current = null;
          setPollState(null);
          setPaused(null);
          ratingRef.current = null;
          setRatingState(null);
          setStarPop(null);
        }
      },
      message: onMessage,
    });
    const offInputs = deps.onInputChange((input) => setInputs((all) => ({ ...all, [input.inputId]: input })));
    return () => {
      offSession();
      offInputs();
    };
  }, [session, sender, deps, clock]);

  useEffect(() => {
    document.documentElement.lang = lang;
    document.title = t(lang, 'app.title');
  }, [lang]);

  // The sabotage cap (D148): when it runs out, every effect still on the screen melts away (no effectCleared).
  useEffect(() => {
    if (!effects || effects.waiting.length === 0) return undefined;
    const timer = setTimeout(() => setEffects(meltIfDue(effectsRef.current, deps.now())),
      Math.max(0, effects.capEndsAt - deps.now()));
    return () => clearTimeout(timer);
  }, [effects, deps]);

  const track = useCallback((input: TrackedInput) => {
    setInputs((all) => ({ ...all, [input.inputId]: input }));
  }, []);

  // Leave game: once the TV app acked `leave`, the token goes and join is shown (PROTOCOL.md section 4).
  const leaveInput = leaveId ? inputs[leaveId] ?? null : null;
  useEffect(() => {
    if (leaveInput?.state !== 'acked') return;
    setLeaveId(null);
    setSettingsOpen(false);
    sender.clear();
    session.leave();
  }, [leaveInput?.state, sender, session]);

  const changeLang = (l: Lang): void => {
    setLangState(l);
    deps.saveLang(l);
  };

  const screen = current?.screen ?? null;

  /** Sends an acked message for a button on the current screen. */
  const sendFor = (slot: Slot, msg: AckedMessage, extra?: Partial<SlotInput>): void => {
    if (!screen) return;
    setSlots((all) => ({ ...all, [slot]: { inputId: msg.inputId, phaseId: screen.phaseId, ...extra } }));
    track(sender.send(msg));
  };

  const slotInput = (slot: Slot): TrackedInput | null => {
    const s = slots[slot];
    if (!s || !screen || s.phaseId !== screen.phaseId) return null;
    return inputs[s.inputId] ?? null;
  };

  /**
   * Sends the one decision of the current phase from screen `screenId`. One tap locks: a second tap on the same phaseId
   * is ignored. `ms` comes from the answer clock (0 on screens that do not start it), read here: a PressButton calls
   * this when the finger lifts inside it (D143), so `ms` is the moment the answer counts, not the touch.
   */
  const decide = (
    screenId: string,
    value: number | boolean | 'pass',
    build: (inputId: string, phaseId: PhaseId, ms: number) => AckedMessage,
  ): void => {
    if (!screen || screen.id !== screenId) return;
    if (overlayShown(effectsRef.current, screen)) return; // the overlay takes every tap until all effects are cleared
    const phaseId = screen.phaseId;
    if (answeredPhase.current === phaseId) return; // one tap locks
    answeredPhase.current = phaseId;
    const ms = clock.ms(phaseId);
    setAnswer({ phaseId, value });
    sendFor('answer', build(deps.newInputId(), phaseId, ms));
  };

  /** What was sent on the current screen's phaseId, if it is of that type. */
  const sentNumber = (): number | null =>
    answer && screen && answer.phaseId === screen.phaseId && typeof answer.value === 'number' ? answer.value : null;
  const sentTrueFalse = (): boolean | 'pass' | null =>
    answer && screen && answer.phaseId === screen.phaseId && (typeof answer.value === 'boolean' || answer.value === 'pass')
      ? answer.value
      : null;

  const multiplier = player?.timerMultiplier ?? 1;

  switch (status.kind) {
    case 'join':
    case 'joining':
      return (
        <JoinScreen
          lang={lang}
          initialRoom={deps.initialRoom || session.currentRoom}
          busy={status.kind === 'joining'}
          noConnection={status.kind === 'join' && status.problem === 'noConnection'}
          onLang={changeLang}
          onJoin={(room, name) => session.join(room, name, lang)}
        />
      );
    case 'reconnecting':
      return <ReconnectingScreen lang={lang} />;
    case 'error':
      return <ErrorScreen lang={lang} reason={status.reason} onBack={() => session.back()} />;
    case 'online':
      break;
  }

  const openSettings = (): void => setSettingsOpen(true);
  let topBar = <TopBar lang={lang} player={player} onSettings={openSettings} />;
  let body;

  if (!current || !screen) {
    body = <WatchScreen lang={lang} player={player} />;
  } else {
    // Anything the phone does not draw (an unknown id, or `result` until power ups exist) is `watch` (shownScreen.ts).
    switch (shownScreen(screen.id)) {
      case 'pickCharacter': {
        const s = slots.character;
        const pick = s && s.characterId !== undefined
          ? { characterId: s.characterId, input: inputs[s.inputId] ?? null }
          : null;
        body = (
          <PickCharacterScreen
            lang={lang}
            screen={screen}
            pick={pick}
            onPick={(id) => sendFor('character', characterMessage(deps.newInputId(), id), { characterId: id })}
          />
        );
        break;
      }
      case 'lobby': {
        const d = lobbyData(screen.data);
        const ready = shownReady(d.ready, readyTap);
        body = (
          <LobbyScreen
            lang={lang}
            players={d.players}
            ready={ready}
            enoughPlayers={d.enoughPlayers}
            readyInput={slotInput('ready')}
            onToggle={() => {
              const next = toggleReady(ready);
              setReadyTap(next.ready);
              sendFor('ready', lobbyMessage(deps.newInputId(), next.action));
            }}
          />
        );
        break;
      }
      case 'countdown':
        body = (
          <CountdownScreen
            lang={lang}
            screen={screen}
            receivedAt={current.receivedAt}
            now={deps.now}
            notReadyInput={slotInput('ready')}
            onNotReady={() => sendFor('ready', lobbyMessage(deps.newInputId(), 'notReady'))}
          />
        );
        break;
      case 'topicVote':
        body = (
          <TopicVoteScreen
            lang={lang}
            screen={screen}
            chosen={sentNumber()}
            voteInput={slotInput('answer')}
            onVote={(door) => decide('topicVote', door, (id, phaseId) => voteInputMessage(id, phaseId, door))}
          />
        );
        break;
      case 'answerChoice':
        topBar = (
          <AnswerTopBar lang={lang} player={player} current={current} now={deps.now} onSettings={openSettings} />
        );
        body = (
          <AnswerChoiceScreen
            lang={lang}
            screen={screen}
            clock={clock}
            chosen={sentNumber()}
            choiceInput={slotInput('answer')}
            onChoose={(index) =>
              decide('answerChoice', index, (id, phaseId, ms) => choiceInputMessage(id, phaseId, index, ms))}
          />
        );
        break;
      case 'answerSlider':
        topBar = (
          <AnswerTopBar lang={lang} player={player} current={current} now={deps.now} onSettings={openSettings} />
        );
        body = (
          <AnswerSliderScreen
            key={screen.phaseId}
            lang={lang}
            screen={screen}
            clock={clock}
            locked={sentNumber()}
            valueInput={slotInput('answer')}
            onLock={(value) =>
              decide('answerSlider', value, (id, phaseId, ms) => valueInputMessage(id, phaseId, value, ms))}
          />
        );
        break;
      case 'answerTrueFalse':
        topBar = (
          <AnswerTopBar lang={lang} player={player} current={current} now={deps.now} onSettings={openSettings} />
        );
        body = (
          <AnswerTrueFalseScreen
            lang={lang}
            screen={screen}
            clock={clock}
            chosen={sentTrueFalse()}
            answerInput={slotInput('answer')}
            onAnswer={(value) =>
              decide('answerTrueFalse', value, (id, phaseId, ms) => trueFalseInputMessage(id, phaseId, value, ms))}
            onPass={() => decide('answerTrueFalse', 'pass', (id, phaseId, ms) => passInputMessage(id, phaseId, ms))}
          />
        );
        break;
      case 'waiting':
        body = <WaitingScreen lang={lang} screen={screen} />;
        break;
      case 'result': {
        const d = resultData(screen.data);
        const powerUp = d.powerUp;
        if (!powerUp) {
          body = <WatchScreen lang={lang} player={player} />; // nothing to pick with
          break;
        }
        const phaseId = screen.phaseId;
        const state = pickFor(pick, phaseId);
        body = (
          <ResultScreen
            lang={lang}
            powerUp={powerUp}
            targets={d.targets}
            armed={state.armed}
            view={pickView(state, inputs)}
            canTarget={(target) => canTarget(state, inputs, target)}
            onArm={() => setPick(armPick(pickFor(pickRef.current, phaseId)))}
            onTarget={(target) => {
              // The ref, not the render's state: a second tap before the re-render must see the first send.
              const latest = pickFor(pickRef.current, phaseId);
              if (!canTarget(latest, inputs, target)) return;
              const inputId = deps.newInputId();
              setPick(addAttempt(latest, inputId, target.playerId));
              track(sender.send(powerUpInputMessage(inputId, phaseId, powerUp, target.playerId)));
            }}
          />
        );
        break;
      }
      case 'results':
        body = <ResultsScreen lang={lang} screen={screen} />;
        break;
      case 'poll': {
        const { questions } = pollData(screen.data);
        const phaseId = screen.phaseId;
        const pollFor = (s: PollState | null): PollState =>
          s && s.phaseId === phaseId ? s : { phaseId, page: 0, draft: {}, inputId: null };
        const state = pollFor(poll);
        const edit = (change: (s: PollState) => PollState): void => {
          const latest = pollFor(pollRef.current);
          if (latest.inputId === null) setPoll(change(latest));
        };
        body = (
          <PollScreen
            lang={lang}
            questions={questions}
            page={state.page}
            draft={state.draft}
            sent={state.inputId !== null}
            pollInput={state.inputId ? inputs[state.inputId] ?? null : null}
            onPage={(page) => setPoll({ ...pollFor(pollRef.current), page })}
            onChoose={(id, index) => edit((s) => ({ ...s, draft: chooseOption(s.draft, id, index) }))}
            onText={(id, text) => edit((s) => ({ ...s, draft: writeText(s.draft, id, text) }))}
            onSubmit={() => {
              const latest = pollFor(pollRef.current);
              if (latest.inputId !== null) return; // one submit
              const inputId = deps.newInputId();
              setPoll({ ...latest, inputId });
              track(sender.send(pollMessage(inputId, pollAnswers(questions, latest.draft))));
            }}
          />
        );
        break;
      }
      case 'pollDone':
        body = <PollDoneScreen lang={lang} />;
        break;
      case 'paused': // never the current screen: drawn over it below
        body = <WatchScreen lang={lang} player={player} />;
        break;
      case 'error':
        body = <TvErrorScreen lang={lang} screen={screen} />;
        break;
      case 'watch':
        body = <WatchScreen lang={lang} player={player} />;
        break;
    }

    // Answer screens sit in the answer area, always, so the answer screen is never remounted when an overlay comes
    // or goes. The sabotage overlay covers that area only: the top bar's countdown stays visible.
    if (ANSWER_SCREENS.includes(screen.id)) {
      // Several effects play one after another (D145): only the first not cleared shows; clearing it sends its
      // `effectCleared` and the next one starts showing at once, in a fresh layer (the key changes).
      const hit = overlayShown(effects, screen) ? shownEffect(effects) : null;
      const phaseId = screen.phaseId;
      body = (
        <div class="answer-area">
          {body}
          {hit ? (
            <SabotageOverlay
              key={`${phaseId}-${hit.seq}-${hit.startedAt}`}
              lang={lang}
              effect={hit}
              now={deps.now}
              onCleared={() => {
                const done = clearShown(effectsRef.current, hit.seq, deps.now());
                if (!done) return;
                setEffects(done.queue);
                // Telemetry only: acked like every input, but no status is shown. The TV takes clears in order only;
                // clearShown only clears the one showing, so they go out in order.
                sender.send(effectClearedInputMessage(deps.newInputId(), done.queue.phaseId, done.cleared.effect, done.ms));
              }}
            />
          ) : null}
        </div>
      );
    }
  }

  // The rating card (D109) over watch, waiting and result while the screen carries `rate`, until closed for this
  // question. It covers the power up pick while open; closing shows the pick again, untouched.
  const rate = screen ? rateData(screen) : null;
  let rateCard = null;
  if (screen && rate && cardShown(rating, rate)) {
    const phaseId = screen.phaseId;
    const { questionId } = rate;
    const shown: Record<string, number> = {};
    for (const item of rate.items) shown[item.id] = shownStars(rating, questionId, item);
    rateCard = (
      <RatingCard
        lang={lang}
        rate={rate}
        shown={shown}
        pop={rating?.questionId === questionId ? starPop : null}
        onStar={(item, stars) => {
          const inputId = deps.newInputId();
          setRating(tapStar(ratingFor(ratingRef.current, questionId), item, stars, inputId));
          popSeq.current += 1;
          setStarPop({ item, stars, seq: popSeq.current });
          track(sender.send(rateInputMessage(inputId, phaseId, item, stars)));
        }}
        onClose={() => setRating(closeCard(ratingFor(ratingRef.current, questionId)))}
      />
    );
  }

  return (
    <div class="game">
      {topBar}
      {body}
      {rateCard}
      {settingsOpen ? (
        <SettingsOverlay
          lang={lang}
          timerMultiplier={multiplier}
          leaveInput={leaveInput}
          onLang={(l) => {
            changeLang(l);
            session.send(settingsMessage(l, multiplier));
          }}
          onMultiplier={(m) => session.send(settingsMessage(lang, m))}
          onLeave={() => {
            const id = deps.newInputId();
            setLeaveId(id);
            track(sender.send(leaveMessage(id)));
          }}
          onClose={() => setSettingsOpen(false)}
        />
      ) : null}
      {paused ? <PausedScreen lang={lang} screen={paused} /> : null}
    </div>
  );
}

/** The top bar with the answer countdown on every answer screen: whole seconds and a bar of the player's window. */
function AnswerTopBar(props: {
  lang: Lang;
  player: PlayerView | null;
  current: Current;
  now: () => number;
  onSettings(): void;
}) {
  const { screen, receivedAt } = props.current;
  const remaining = useRemaining(screen.endsInMs, receivedAt, props.now);
  const total = answerWindowMs(screen) ?? screen.endsInMs;
  const timer = remaining === null ? null : { seconds: wholeSeconds(remaining), share: remainingShare(remaining, total) };
  return <TopBar lang={props.lang} player={props.player} timer={timer} onSettings={props.onSettings} />;
}
