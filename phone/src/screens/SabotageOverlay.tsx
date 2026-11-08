// The sabotage overlay (06 `sabotage`, 03 "Power ups"): drawn by app.tsx over the answer area (buttons or slider)
// while the effects' phaseId is current and one is not cleared yet. Several effects play one after another (D145):
// app.tsx draws one layer per effect, keyed so the next effect gets a fresh layer with its own text and fog timer.
// It takes every tap on the answer area; the top bar and its countdown stay visible above it. The answer clock keeps running underneath: this component never touches it.
// Gesture rules are in freeze.ts, slime.ts and fog.ts; this file only feeds them pointer events and draws the result.
// Each layer calls onCleared exactly once.

import { useLayoutEffect, useRef, useState } from 'preact/hooks';
import { fogAdvance, fogCleared, fogOpacity, fogStart, isRubbing } from '../fog';
import { freezeStart, iceBroken, iceCracks, iceLeft, tapIce } from '../freeze';
import { t, type TextKey } from '../i18n';
import type { Lang } from '../protocol';
import { effectTextKey, type ShownEffect } from '../sabotage';
import { blobLeft, slimeCleared, slimeLayout, slimeStart, swipe, type Point } from '../slime';

interface Props {
  lang: Lang;
  effect: ShownEffect;
  /** performance.now(), the same clock the effect's start time comes from. */
  now(): number;
  onCleared(): void;
}

export function SabotageOverlay(props: Props) {
  const { effect } = props;
  switch (effect.effect) {
    case 'freeze':
      return <FreezeLayer {...props} />;
    case 'slime':
      return <SlimeLayer {...props} />;
    case 'fog':
      return <FogLayer {...props} />;
  }
}

/** Calls onCleared the first time only. */
function useOnce(onCleared: () => void): () => void {
  const done = useRef(false);
  const latest = useRef(onCleared);
  latest.current = onCleared;
  return () => {
    if (done.current) return;
    done.current = true;
    latest.current();
  };
}

/** "{name} froze you!" and a hint; no pointer events of its own, so a tap on the text counts on the layer. */
function Caption(props: { lang: Lang; effect: ShownEffect; hint: TextKey }) {
  const { lang, effect } = props;
  const name = effect.from.trim() || t(lang, 'sabotage.someone');
  return (
    <div class="sabotage-caption">
      <p class="sabotage-text">{t(lang, effectTextKey(effect.effect), { name })}</p>
      <p class="sabotage-hint">{t(lang, props.hint)}</p>
    </div>
  );
}

function FreezeLayer(props: Props) {
  const state = useRef(freezeStart(props.effect.params.taps));
  const [, redraw] = useState(0);
  const cleared = useOnce(props.onCleared);

  const down = (e: PointerEvent): void => {
    e.preventDefault();
    state.current = tapIce(state.current);
    redraw((n) => n + 1);
    if (iceBroken(state.current)) cleared();
  };

  const s = state.current;
  return (
    <div class="sabotage sabotage-freeze" onPointerDown={down} style={{ '--cracks': String(iceCracks(s)) }}>
      <Caption lang={props.lang} effect={props.effect} hint="sabotage.freezeHint" />
      {/* key: the number pops again on every tap */}
      <span key={s.hits} class="ice-count">{iceLeft(s)}</span>
    </div>
  );
}

function SlimeLayer(props: Props) {
  const layer = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState<{ w: number; h: number }>({ w: 0, h: 0 });
  const [progress, setProgress] = useState(() => slimeStart(props.effect.params.blobs));
  const progressRef = useRef(progress);
  const strokes = useRef(new Map<number, Point>());
  const cleared = useOnce(props.onCleared);

  // Measure the answer area; again when it changes size (rotation, keyboard).
  useLayoutEffect(() => {
    const el = layer.current;
    if (!el) return;
    const measure = (): void => {
      const r = el.getBoundingClientRect();
      setSize((old) => (old.w === r.width && old.h === r.height ? old : { w: r.width, h: r.height }));
    };
    measure();
    if (typeof ResizeObserver === 'undefined') {
      window.addEventListener('resize', measure);
      return () => window.removeEventListener('resize', measure);
    }
    const ro = new ResizeObserver(measure);
    ro.observe(el);
    return () => ro.disconnect();
  }, []);

  const blobs = slimeLayout(progress.length, size.w, size.h);

  const point = (e: PointerEvent): Point => {
    const r = (e.currentTarget as HTMLElement).getBoundingClientRect();
    return { x: e.clientX - r.left, y: e.clientY - r.top };
  };
  const down = (e: PointerEvent): void => {
    e.preventDefault();
    (e.currentTarget as HTMLElement).setPointerCapture?.(e.pointerId);
    strokes.current.set(e.pointerId, point(e));
  };
  const move = (e: PointerEvent): void => {
    const from = strokes.current.get(e.pointerId);
    if (!from) return;
    const to = point(e);
    strokes.current.set(e.pointerId, to);
    const next = swipe(blobs, progressRef.current, from, to);
    if (next.every((p, i) => p === progressRef.current[i])) return;
    progressRef.current = next;
    setProgress(next);
    if (slimeCleared(next)) cleared();
  };
  const up = (e: PointerEvent): void => {
    strokes.current.delete(e.pointerId);
  };

  return (
    <div
      ref={layer}
      class="sabotage sabotage-slime"
      onPointerDown={down}
      onPointerMove={move}
      onPointerUp={up}
      onPointerCancel={up}
    >
      {blobs.map((b, i) => {
        const left = blobLeft(progress, i);
        return left > 0 ? (
          <span
            key={i}
            class="slime-blob"
            style={{
              left: `${b.x - b.r}px`, top: `${b.y - b.r}px`, width: `${2 * b.r}px`, height: `${2 * b.r}px`,
              opacity: String(0.35 + 0.65 * left),
            }}
          />
        ) : null;
      })}
      <Caption lang={props.lang} effect={props.effect} hint="sabotage.slimeHint" />
    </div>
  );
}

function FogLayer(props: Props) {
  const layer = useRef<HTMLDivElement>(null);
  const fog = useRef(fogStart(props.effect.params.seconds, props.effect.startedAt));
  const down = useRef(new Set<number>());
  const lastMove = useRef<number | null>(null);
  const cleared = useOnce(props.onCleared);
  const now = props.now;

  // One frame at a time: count the time since the last frame (2x while rubbing) and set the opacity directly,
  // without a render per frame.
  useLayoutEffect(() => {
    let frame = 0;
    const tick = (): void => {
      const at = now();
      fog.current = fogAdvance(fog.current, at, isRubbing(down.current.size > 0, lastMove.current, at));
      if (layer.current) layer.current.style.opacity = String(fogOpacity(fog.current));
      if (fogCleared(fog.current)) {
        cleared();
        return;
      }
      frame = requestAnimationFrame(tick);
    };
    tick();
    return () => cancelAnimationFrame(frame);
  }, []);

  const press = (e: PointerEvent): void => {
    e.preventDefault();
    (e.currentTarget as HTMLElement).setPointerCapture?.(e.pointerId);
    down.current.add(e.pointerId);
  };
  const move = (e: PointerEvent): void => {
    if (down.current.has(e.pointerId)) lastMove.current = now();
  };
  const release = (e: PointerEvent): void => {
    down.current.delete(e.pointerId);
  };

  return (
    <div
      ref={layer}
      class="sabotage sabotage-fog"
      onPointerDown={press}
      onPointerMove={move}
      onPointerUp={release}
      onPointerCancel={release}
    >
      <Caption lang={props.lang} effect={props.effect} hint="sabotage.fogHint" />
    </div>
  );
}
