// Screen Wake Lock after the first tap, and again whenever the page becomes visible (06).
// Failures are ignored: plain http has no wake lock, and some browsers refuse it.

interface WakeLockSentinelLike { released: boolean }
interface WakeLockLike { request(type: 'screen'): Promise<WakeLockSentinelLike> }

export function setupWakeLock(doc: Document, nav: Navigator): void {
  let tapped = false;
  let sentinel: WakeLockSentinelLike | null = null;

  const request = async (): Promise<void> => {
    const wakeLock = (nav as Navigator & { wakeLock?: WakeLockLike }).wakeLock;
    if (!wakeLock || (sentinel && !sentinel.released)) return;
    try {
      sentinel = await wakeLock.request('screen');
    } catch {
      /* ignored */
    }
  };

  doc.addEventListener('pointerdown', () => {
    tapped = true;
    void request();
  }, { capture: true, passive: true });

  doc.addEventListener('visibilitychange', () => {
    if (tapped && doc.visibilityState === 'visible') void request();
  });
}
