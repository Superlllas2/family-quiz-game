// Which screen the phone draws for a screen id from the TV app (06, D88). The phone is an input device: anything it
// has nothing to draw for shows `watch` ("Смотри на экран / Eyes on the TV"), never a crash: ids it does not know,
// and the screens version 2 removed.

/** The screen ids the phone page draws itself. */
export type ShownScreen =
  | 'pickCharacter'
  | 'lobby'
  | 'countdown'
  | 'watch'
  | 'topicVote'
  | 'answerChoice'
  | 'answerSlider'
  | 'answerTrueFalse'
  | 'waiting'
  | 'result'
  | 'results'
  | 'poll'
  | 'pollDone'
  | 'paused'
  | 'error';

const DRAWN: ReadonlySet<string> = new Set<ShownScreen>([
  'pickCharacter',
  'lobby',
  'countdown',
  'watch',
  'topicVote',
  'answerChoice',
  'answerSlider',
  'answerTrueFalse',
  'waiting',
  'result',
  'results',
  'poll',
  'pollDone',
  'paused',
  'error',
]);

/** The screen to draw for the TV's screen id: the id itself when the phone draws it, else `watch`. */
export function shownScreen(id: string): ShownScreen {
  return DRAWN.has(id) ? (id as ShownScreen) : 'watch';
}
