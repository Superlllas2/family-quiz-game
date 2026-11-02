// Screen `countdown` (06): the TV's text ("Игра начинается / Starting"), a big 3, 2, 1 from `endsInMs`,
// and Not ready on every phone (`lobby` action `notReady`): the TV app stops the countdown and sends `lobby` (D76).

import { wholeSeconds } from '../countdown';
import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import { textData, type Lang, type ScreenState } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';
import { useRemaining } from './useRemaining';

interface Props {
  lang: Lang;
  screen: ScreenState;
  receivedAt: number;
  now: () => number;
  notReadyInput: TrackedInput | null;
  onNotReady(): void;
}

export function CountdownScreen(props: Props) {
  const { lang, screen } = props;
  const { text } = textData(screen.data);
  const remaining = useRemaining(screen.endsInMs, props.receivedAt, props.now);
  // 3, 2, 1: the last second stays on 1 until the TV sends the next screen.
  const shown = remaining === null ? null : Math.max(1, wholeSeconds(remaining));

  return (
    <div class="body center">
      <p class="big-text">{text}</p>
      <div class="countdown-number">{shown}</div>
      <div class="bottom-block">
        <PressButton class="big" onClick={props.onNotReady}>
          {t(lang, 'lobby.notReady')}
        </PressButton>
        <InputStatus lang={lang} input={props.notReadyInput} />
      </div>
    </div>
  );
}
