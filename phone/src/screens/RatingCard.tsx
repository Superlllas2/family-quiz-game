// The rating card (D109) over `watch`, `waiting` and `result` in Reveal and Scores: a title, a close button and one
// row of stars per item, the leftmost 1 and the rightmost `stars`, with the low and high labels under the ends.
// A tap fills the row when the finger lifts (PressButton, D143), before the ack. The state lives in app.tsx
// (rating.ts); this view uses no hooks, so tests can call it as a plain function.

import { t } from '../i18n';
import type { Lang, RateData } from '../protocol';
import { CloseIcon, StarIcon } from './Icons';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  rate: RateData;
  /** Stars filled on each row, by item id. */
  shown: Record<string, number>;
  /** The last tap, so only that star pops: changes on every tap. */
  pop: { item: string; stars: number; seq: number } | null;
  onStar(item: string, stars: number): void;
  onClose(): void;
}

export function RatingCard(props: Props) {
  const { lang, rate, shown, pop } = props;
  const close = rate.close || t(lang, 'rate.close');
  const stars = Array.from({ length: rate.stars }, (_, i) => i + 1);
  return (
    <div class="rate-layer">
      <section class="rate-card" role="dialog" aria-label={rate.title || close}>
        <div class="rate-head">
          {rate.title ? <h2 class="rate-title">{rate.title}</h2> : <span />}
          <PressButton class="rate-close" ariaLabel={close} onPress={props.onClose}>
            <CloseIcon />
          </PressButton>
        </div>
        {rate.items.map((item) => {
          const filled = shown[item.id] ?? 0;
          return (
            <div class="rate-row" key={item.id}>
              {item.text ? <p class="rate-text">{item.text}</p> : null}
              <div class="rate-stars" role="group" aria-label={item.text || undefined}>
                {stars.map((k) => {
                  const popping = pop && pop.item === item.id && pop.stars === k;
                  return (
                    <PressButton
                      key={k}
                      class={k <= filled ? 'star on' : 'star'}
                      ariaPressed={k === filled}
                      ariaLabel={t(lang, 'rate.star', { n: k, total: rate.stars })}
                      onPress={() => props.onStar(item.id, k)}
                    >
                      <span class={popping ? 'star-shape pop' : 'star-shape'} key={popping ? `p${pop.seq}` : 's'}>
                        <StarIcon />
                      </span>
                    </PressButton>
                  );
                })}
              </div>
              {item.low || item.high ? (
                <div class="rate-ends">
                  <span>{item.low}</span>
                  <span>{item.high}</span>
                </div>
              ) : null}
            </div>
          );
        })}
      </section>
    </div>
  );
}
