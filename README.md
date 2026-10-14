# MTG Life Counter

Contapunti per Magic: The Gathering fatto per l'associazione [LudiPop](https://ludipop.it/).
Funziona dal browser del telefono, si installa come app e va anche senza connessione.

- Da 2 a 6 giocatori, Standard e Commander
- Danni da commander segnati direttamente al tavolo, veleno
- Annulla, giocatore iniziale estratto a sorte, durata della partita
- Dadi (d4…d100) e moneta
- Lo schermo resta acceso durante la partita

## Sviluppo

Serve il [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/MtgLifeCounter.Web
```

```bash
dotnet test
```

| Cartella | Contenuto |
|---|---|
| `src/MtgLifeCounter.Core` | Regole della partita, senza interfaccia: eventi, annulla, salvataggio |
| `src/MtgLifeCounter.Web` | L'app Blazor WebAssembly (PWA) |
| `tests/MtgLifeCounter.Core.Tests` | Test del Core |
| `docs/SPECIFICA.md` | Specifica iniziale |

## Pubblicazione

A ogni push su `main` il workflow `.github/workflows/deploy.yml` esegue i test e pubblica
l'app su GitHub Pages.

## Crediti

Font [Urbanist](https://fonts.google.com/specimen/Urbanist) e [Bungee](https://fonts.google.com/specimen/Bungee)
con licenza SIL Open Font License (vedi `src/MtgLifeCounter.Web/wwwroot/fonts`).
Logo LudiPop di proprietà dell'associazione LudiPop.
Magic: The Gathering è un marchio di Wizards of the Coast; questo progetto non è affiliato né approvato da Wizards of the Coast.
