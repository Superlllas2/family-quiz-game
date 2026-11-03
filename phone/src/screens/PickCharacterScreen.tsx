// Screen `pickCharacter` (06): a grid of character cards from content/characters.json, each in its signature
// color with the character's name and personality line. Taken ones are greyed; the TV's assigned one starts selected.
// A tap only selects a card; Confirm sends `character` for it, so a slip of the finger never picks. `characterTaken`
// shows a short note and the TV sends a refreshed screen. The grid scrolls inside the screen (16 cards do not fit on
// a phone, lobby only); Confirm stays below it, always in reach.

import { useState } from 'preact/hooks';
import { characters } from '../characters';
import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import { selectedCard } from '../pickSelection';
import { pickCharacterData, type Lang, type ScreenState } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  screen: ScreenState;
  /** The last `character` input on this screen and the id it asked for. */
  pick: { characterId: number; input: TrackedInput | null } | null;
  onPick(characterId: number): void;
}

export function PickCharacterScreen(props: Props) {
  const { lang, screen, pick } = props;
  const { taken, current } = pickCharacterData(screen.data);
  const [tapped, setTapped] = useState<number | null>(null);
  const state = pick?.input?.state;
  const busy = state === 'sent';
  // While a pick is on its way (or acked, before the TV moves on), the confirmed card shows as selected.
  const selected = pick && (state === 'sent' || state === 'acked') ? pick.characterId : selectedCard(tapped, current, taken);

  return (
    <div class="body pick">
      <h1 class="pick-title">{t(lang, 'pick.title')}</h1>
      <div class="pick-grid">
        {characters.map((c) => {
          const isTaken = taken.includes(c.id) && c.id !== current;
          const isSelected = c.id === selected;
          const cls = `card${isSelected ? ' selected' : ''}${isTaken ? ' taken' : ''}`;
          return (
            <PressButton
              key={c.id}
              class={cls}
              style={{ background: c.color }}
              disabled={isTaken || busy}
              ariaPressed={isSelected}
              onClick={() => setTapped(c.id)}
            >
              <span class="card-name">{c.name[lang]}</span>
              <span class="card-line">{c.personality[lang]}</span>
              {isTaken ? <span class="card-taken">{t(lang, 'pick.takenLabel')}</span> : null}
            </PressButton>
          );
        })}
      </div>
      <div class="bottom-block">
        <PressButton
          class="big"
          disabled={selected === null || busy}
          onClick={() => {
            if (selected !== null) props.onPick(selected);
          }}
        >
          {t(lang, 'pick.confirm')}
        </PressButton>
        <InputStatus lang={lang} input={pick?.input ?? null} />
      </div>
    </div>
  );
}
