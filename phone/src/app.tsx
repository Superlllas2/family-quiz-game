// The phone page: draws exactly one screen, chosen from the session status and the last `screen` from the TV app.
// No game rules here; the TV app decides everything.

import { useCallback, useEffect, useLayoutEffect, useState } from 'preact/hooks';
import type { AnswerClock } from './answerClock';
import { t } from './i18n';
import type { InputSender, TrackedInput } from './net/inputs';
import type { Session, SessionStatus } from './net/session';
import { choiceInputMessage, lobbyMessage, type IncomingMessage, type Lang, type ScreenState } from './protocol';
import { JoinScreen } from './screens/JoinScreen';
import { LobbyScreen } from './screens/LobbyScreen';
import { ErrorScreen, ReconnectingScreen, WatchTvScreen } from './screens/StatusScreens';

export interface AppDeps {
  session: Session;
  sender: InputSender;
  /** Subscribes to input state changes from the sender. */
  onInputChange(listener: (input: TrackedInput) => void): () => void;
  clock: AnswerClock;
  initialRoom: string;
  initialLang: Lang;
  saveLang(lang: Lang): void;
  newInputId(): string;
}

export function App({ deps }: { deps: AppDeps }) {
  const { session, sender, clock } = deps;
  const [status, setStatus] = useState<SessionStatus>(() => session.status);
  const [screen, setScreen] = useState<ScreenState | null>(null);
  const [lang, setLangState] = useState<Lang>(deps.initialLang);
  const [inputs, setInputs] = useState<Record<string, TrackedInput>>({});
  const [startId, setStartId] = useState<string | null>(null);
  const [padId, setPadId] = useState<string | null>(null);

  // Layout effects run during the first render's commit, before main.tsx starts the session.
  useLayoutEffect(() => {
    const onMessage = (msg: IncomingMessage): void => {
      switch (msg.type) {
        case 'snapshot':
          if (msg.screen) setScreen(msg.screen);
          break;
        case 'screen': {
          const { type: _type, ...next } = msg;
          setScreen(next);
          break;
        }
        case 'ack':
          sender.handleAck(msg);
          break;
        default:
          break; // welcome and refused are handled by the session; player is not drawn yet.
      }
    };
    const offSession = session.subscribe({
      status: (s) => {
        setStatus(s);
        if (s.kind === 'join' || s.kind === 'error') setScreen(null);
      },
      message: onMessage,
    });
    const offInputs = deps.onInputChange((input) => setInputs((all) => ({ ...all, [input.inputId]: input })));
    return () => {
      offSession();
      offInputs();
    };
  }, [session, sender, deps]);

  useEffect(() => {
    document.documentElement.lang = lang;
    document.title = t(lang, 'app.title');
  }, [lang]);

  const track = useCallback((input: TrackedInput) => {
    setInputs((all) => ({ ...all, [input.inputId]: input }));
  }, []);

  const changeLang = (l: Lang): void => {
    setLangState(l);
    deps.saveLang(l);
  };

  const onStart = (): void => {
    const id = deps.newInputId();
    setStartId(id);
    track(sender.send(lobbyMessage(id, 'start')));
  };

  // TEMPORARY (Day 1 test pad).
  const onChoice = (choice: number): void => {
    if (!screen) return;
    const ms = clock.ms(screen.phaseId);
    const id = deps.newInputId();
    setPadId(id);
    track(sender.send(choiceInputMessage(id, screen.phaseId, choice, ms)));
  };

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
      if (screen?.id === 'lobby') {
        return (
          <LobbyScreen
            lang={lang}
            screen={screen}
            clock={clock}
            startInput={startId ? inputs[startId] ?? null : null}
            padInput={padId ? inputs[padId] ?? null : null}
            onStart={onStart}
            onChoice={onChoice}
          />
        );
      }
      return <WatchTvScreen lang={lang} />;
  }
}
