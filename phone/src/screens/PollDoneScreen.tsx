// Screen `pollDone` (06): "Спасибо! / Thanks!" and Play again on every phone, enabled by `canPlayAgain`.

import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import { pollDoneData, type Lang, type ScreenState } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  screen: ScreenState;
  playAgainInput: TrackedInput | null;
  onPlayAgain(): void;
}

export function PollDoneScreen(props: Props) {
  const { lang } = props;
  const { canPlayAgain } = pollDoneData(props.screen.data);
  return (
    <div class="body center">
      <p class="result-head">{t(lang, 'pollDone.thanks')}</p>
      <div class="bottom-block">
        <PressButton class="primary big" disabled={!canPlayAgain} onClick={props.onPlayAgain}>
          {t(lang, 'pollDone.playAgain')}
        </PressButton>
        <InputStatus lang={lang} input={props.playAgainInput} />
      </div>
    </div>
  );
}
