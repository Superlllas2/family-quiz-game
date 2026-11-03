// Screen `topicVote` (06): the four topic doors from the TV, one button each, door 4 is "A bit of everything".
// One tap votes: the tapped door stays highlighted, the others dim, and later taps are ignored. The TV app then
// sends `waiting`.

import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import { topicVoteData, type Lang, type ScreenState } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  screen: ScreenState;
  /** The door voted for on this phaseId, if any. */
  chosen: number | null;
  voteInput: TrackedInput | null;
  onVote(door: number): void;
}

export function TopicVoteScreen(props: Props) {
  const { lang, chosen } = props;
  const { doors } = topicVoteData(props.screen.data);
  return (
    <div class="body vote">
      <h1 class="vote-title">{t(lang, 'topicVote.title')}</h1>
      <div class="door-list">
        {doors.map((title, i) => {
          const state = chosen === null ? '' : chosen === i ? ' chosen' : ' dim';
          return (
            <PressButton key={i} class={`door${state}`} onPress={() => props.onVote(i)}>
              <span class="door-title">{title}</span>
              {chosen === i ? <InputStatus lang={lang} input={props.voteInput} class="door-status" /> : null}
            </PressButton>
          );
        })}
      </div>
    </div>
  );
}
