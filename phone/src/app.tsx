// The phone page: draws exactly one screen, chosen from the session status and the last `screen` from the TV app.
// No game rules here; the TV app decides everything. The settings overlay can open over any game screen.

import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'preact/hooks';
import type { AnswerClock } from './answerClock';
import { remainingShare, wholeSeconds } from './countdown';
import { t } from './i18n';
import type { InputSender, TrackedInput } from './net/inputs';
import type { Session, SessionStatus } from './net/session';
import {
  answerChoiceData, characterMessage, choiceInputMessage, leaveMessage, lobbyMessage, playerView, settingsMessage,
  type AckedMessage, type IncomingMessage, type Lang, type PhaseId, type PlayerView, type ScreenState,
} from './protocol';
import { AnswerChoiceScreen } from './screens/AnswerChoiceScreen';
import { CountdownScreen } from './screens/CountdownScreen';
import {
  GetReadyScreen, ResultScreen, ResultsScreen, TvErrorScreen, WaitingScreen, WatchTvBody,
} from './screens/InfoScreens';
import { JoinScreen } from './screens/JoinScreen';
import { LobbyScreen } from './screens/LobbyScreen';
import { PickCharacterScreen } from './screens/PickCharacterScreen';
import { PollDoneScreen } from './screens/PollDoneScreen';
import { SettingsOverlay } from './screens/SettingsOverlay';
import { ErrorScreen, ReconnectingScreen } from './screens/StatusScreens';
import { TopBar } from './screens/TopBar';
import { useRemaining } from './screens/useRemaining';

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

/** One button's input on one screen: its status shows only while that screen's phaseId is current. */
type Slot = 'start' | 'cancel' | 'playAgain' | 'character' | 'choice';
interface SlotInput { inputId: string; phaseId: PhaseId; characterId?: number }

export function App({ deps }: { deps: AppDeps }) {
  const { session, sender, clock } = deps;
  const [status, setStatus] = useState<SessionStatus>(() => session.status);
  const [current, setCurrent] = useState<Current | null>(null);
  const [player, setPlayer] = useState<PlayerView | null>(null);
  const [lang, setLangState] = useState<Lang>(deps.initialLang);
  const [inputs, setInputs] = useState<Record<string, TrackedInput>>({});
  const [slots, setSlots] = useState<Partial<Record<Slot, SlotInput>>>({});
  const [choice, setChoice] = useState<{ phaseId: PhaseId; index: number } | null>(null);
  /** The phaseId already answered: set at once, so a second tap before the re-render is ignored too. */
  const answeredPhase = useRef<PhaseId | null>(null);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [leaveId, setLeaveId] = useState<string | null>(null);

  // Layout effects run during the first render's commit, before main.tsx starts the session.
  useLayoutEffect(() => {
    const showScreen = (next: ScreenState): void => {
      // Always a new arrival time, also for a last call with the same phaseId (the countdown re-syncs).
      setCurrent({ screen: next, receivedAt: deps.now() });
      clock.arrived(next.phaseId);
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
          setChoice(null);
          answeredPhase.current = null;
          clock.reset();
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

  const onChoose = (index: number): void => {
    if (!screen || screen.id !== 'answerChoice') return;
    const phaseId = screen.phaseId;
    if (answeredPhase.current === phaseId) return; // one tap locks
    answeredPhase.current = phaseId;
    const ms = clock.ms(phaseId);
    setChoice({ phaseId, index });
    sendFor('choice', choiceInputMessage(deps.newInputId(), phaseId, index, ms));
  };

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
    body = <WatchTvBody lang={lang} />;
  } else {
    switch (screen.id) {
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
      case 'lobby':
        body = (
          <LobbyScreen
            lang={lang}
            screen={screen}
            startInput={slotInput('start')}
            onStart={() => sendFor('start', lobbyMessage(deps.newInputId(), 'start'))}
          />
        );
        break;
      case 'countdown':
        body = (
          <CountdownScreen
            lang={lang}
            screen={screen}
            receivedAt={current.receivedAt}
            now={deps.now}
            cancelInput={slotInput('cancel')}
            onCancel={() => sendFor('cancel', lobbyMessage(deps.newInputId(), 'cancelStart'))}
          />
        );
        break;
      case 'getReady':
        body = <GetReadyScreen lang={lang} screen={screen} />;
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
            chosen={choice && choice.phaseId === screen.phaseId ? choice.index : null}
            choiceInput={slotInput('choice')}
            onChoose={onChoose}
          />
        );
        break;
      case 'waiting':
        body = <WaitingScreen lang={lang} screen={screen} />;
        break;
      case 'result':
        body = <ResultScreen lang={lang} screen={screen} />;
        break;
      case 'results':
        body = <ResultsScreen lang={lang} screen={screen} />;
        break;
      case 'pollDone':
        body = (
          <PollDoneScreen
            lang={lang}
            screen={screen}
            playAgainInput={slotInput('playAgain')}
            onPlayAgain={() => sendFor('playAgain', lobbyMessage(deps.newInputId(), 'playAgain'))}
          />
        );
        break;
      case 'error':
        body = <TvErrorScreen lang={lang} screen={screen} />;
        break;
      default:
        body = <WatchTvBody lang={lang} />;
        break;
    }
  }

  return (
    <div class="game">
      {topBar}
      {body}
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
    </div>
  );
}

/** The top bar with the answer countdown: whole seconds and a bar of the player's window. */
function AnswerTopBar(props: {
  lang: Lang;
  player: PlayerView | null;
  current: Current;
  now: () => number;
  onSettings(): void;
}) {
  const { screen, receivedAt } = props.current;
  const remaining = useRemaining(screen.endsInMs, receivedAt, props.now);
  const total = answerChoiceData(screen.data).windowMs ?? screen.endsInMs;
  const timer = remaining === null ? null : { seconds: wholeSeconds(remaining), share: remainingShare(remaining, total) };
  return <TopBar lang={props.lang} player={props.player} timer={timer} onSettings={props.onSettings} />;
}
