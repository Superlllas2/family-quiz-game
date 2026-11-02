// Composition root: builds the session, the input sender and the clock, wires the browser events, renders.

import { render } from 'preact';
import { AnswerClock } from './answerClock';
import { App, type AppDeps } from './app';
import { newInputId } from './ids';
import { InputSender, type TrackedInput } from './net/inputs';
import { Session, type SocketLike } from './net/session';
import { socketUrl } from './net/socketUrl';
import { globalTimers } from './net/timers';
import { loadLang, parseLaunch, safeStore, saveLang, TokenStore } from './storage';
import { setupWakeLock } from './wakeLock';
import './style.css';

const log = (...args: unknown[]): void => console.log('[fq]', ...args);

const launch = parseLaunch(location.search);
const store = safeStore(() => window.localStorage);
const now = (): number => performance.now();
const timers = globalTimers(now);

const session = new Session({
  createSocket: (url) => new WebSocket(url) as unknown as SocketLike,
  urlFor: (room) => socketUrl(location, room, import.meta.env.VITE_RELAY_HOST as string | undefined),
  tokens: new TokenStore(store, launch.tab),
  timers,
  log,
});

const inputListeners = new Set<(input: TrackedInput) => void>();
const sender = new InputSender({
  transmit: (msg) => session.send(msg),
  timers,
  onChange: (input) => inputListeners.forEach((l) => l(input)),
  log,
});

const deps: AppDeps = {
  session,
  sender,
  onInputChange: (listener) => {
    inputListeners.add(listener);
    return () => inputListeners.delete(listener);
  },
  clock: new AnswerClock(now),
  initialRoom: launch.room,
  initialLang: loadLang(store),
  saveLang: (lang) => saveLang(store, lang),
  newInputId: () => newInputId(),
};

render(<App deps={deps} />, document.getElementById('app')!);

document.addEventListener('visibilitychange', () => {
  if (document.visibilityState === 'visible') session.onVisible();
});
setupWakeLock(document, navigator);

session.start(launch.room);
