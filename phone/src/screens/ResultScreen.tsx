// Screen `result` (06, 03 "Using"): the power up pick in Reveal and Scores, sent only to a player holding one. Two
// taps: the big power up card, then one of the target buttons that appear below it. Targets the TV marks unavailable
// (already hit or immune) are greyed and cannot be tapped. Not picking keeps the power up for a later question.
// The pick rules live in powerUpPick.ts; this view uses no hooks, so tests can call it as a plain function.

import { t } from '../i18n';
import { powerUpNameKey, type PickView } from '../powerUpPick';
import type { Lang, PowerUpKind, PowerUpTarget } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  powerUp: PowerUpKind;
  targets: PowerUpTarget[];
  armed: boolean;
  view: PickView;
  /** True if tapping this target now sends the power up. */
  canTarget(target: PowerUpTarget): boolean;
  onArm(): void;
  onTarget(target: PowerUpTarget): void;
}

export function ResultScreen(props: Props) {
  const { lang, powerUp, armed, view } = props;
  const picked = view.stage === 'sending' || view.stage === 'done' ? view.lastTarget : null;
  return (
    <div class="body power-pick">
      <PressButton
        class={`power-card power-${powerUp}${armed ? ' armed' : ''}`}
        ariaPressed={armed}
        disabled={view.stage === 'stopped'}
        onPress={props.onArm}
      >
        <span class="power-name">{t(lang, powerUpNameKey(powerUp))}</span>
        {armed ? null : <span class="power-hint">{t(lang, 'powerUp.tapToUse')}</span>}
      </PressButton>
      {armed ? (
        <>
          <p class="power-title">{t(lang, 'powerUp.pickTarget')}</p>
          <div class="target-list">
            {props.targets.map((target) => {
              const state = picked === null ? '' : picked === target.playerId ? ' chosen' : ' dim';
              return (
                <PressButton
                  key={target.playerId}
                  class={`target${state}`}
                  disabled={picked !== target.playerId && !props.canTarget(target)}
                  onPress={() => props.onTarget(target)}
                >
                  {target.name}
                </PressButton>
              );
            })}
          </div>
        </>
      ) : (
        <p class="muted-text power-keep">{t(lang, 'powerUp.keep')}</p>
      )}
      <InputStatus lang={lang} input={view.last} class="power-status" />
    </div>
  );
}
