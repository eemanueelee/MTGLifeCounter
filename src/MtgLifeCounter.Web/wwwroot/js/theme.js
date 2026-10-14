// Tema grafico: attributo data-theme su <html>, ricordato nel browser.
// Caricato in <head> prima di tutto il resto, così l'app parte già con il tema giusto.
(function () {
    const storageKey = 'mtg-life-counter.theme';
    const defaultTheme = 'ludipop';

    // Per ora l'app ha un solo tema (Ludipop) e la scelta non è mostrata: si ignora un'eventuale
    // scelta salvata in passato, altrimenti chi aveva scelto Arcade non potrebbe più tornare indietro.
    // Per riattivare la scelta: rimettere il selettore nella Home (vedi ThemeService) e impostare true.
    const themeSelectionEnabled = false;

    // Colore della barra di sistema del telefono per ogni tema (come --bg in app.css).
    const barColors = { ludipop: '#e4e2ee', arcade: '#0a0a12' };

    function read() {
        try {
            return localStorage.getItem(storageKey);
        } catch {
            return null;
        }
    }

    function apply(theme) {
        if (!(theme in barColors)) theme = defaultTheme;
        document.documentElement.dataset.theme = theme;
        const meta = document.querySelector('meta[name="theme-color"]');
        if (meta) meta.content = barColors[theme];
        return theme;
    }

    window.mtgTheme = {
        get: () => document.documentElement.dataset.theme,
        set: theme => {
            const applied = apply(theme);
            try {
                localStorage.setItem(storageKey, applied);
            } catch {
                // Storage non disponibile: il tema vale solo fino alla chiusura.
            }
        },
    };

    apply(themeSelectionEnabled ? (read() ?? defaultTheme) : defaultTheme);
})();
