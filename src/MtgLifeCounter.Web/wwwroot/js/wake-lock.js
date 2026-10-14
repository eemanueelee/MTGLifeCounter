// Mantiene lo schermo acceso mentre la partita è aperta.
// Il browser rilascia il blocco quando la pagina va in background:
// al ritorno in primo piano viene richiesto di nuovo in automatico.

let wanted = false;
let sentinel = null;
let pending = null;

function acquire() {
    if (!wanted || sentinel || pending) return pending;
    if (!('wakeLock' in navigator) || document.visibilityState !== 'visible') return null;

    pending = navigator.wakeLock.request('screen')
        .then(lock => {
            sentinel = lock;
            lock.addEventListener('release', () => {
                if (sentinel === lock) sentinel = null;
            });
            // disable() potrebbe essere arrivato mentre la richiesta era in corso.
            if (!wanted) return release();
        })
        .catch(() => {
            // Non supportato o negato (es. risparmio energetico): l'app funziona comunque.
        })
        .finally(() => {
            pending = null;
        });

    return pending;
}

async function release() {
    const lock = sentinel;
    sentinel = null;
    if (lock) {
        try { await lock.release(); } catch { /* già rilasciato */ }
    }
}

document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') acquire();
});

export async function enable() {
    wanted = true;
    await acquire();
}

export async function disable() {
    wanted = false;
    await release();
}
