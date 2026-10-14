# MTG Life Counter

## 1. Obiettivo del progetto

L'applicazione è un life counter per Magic: The Gathering, pensato principalmente per essere utilizzato da smartphone durante una partita fisica.

L'obiettivo è offrire un'interfaccia:

- molto rapida da utilizzare;
- leggibile a colpo d'occhio;
- ottimizzata per il touchscreen;
- utilizzabile offline;
- senza necessità di account;
- senza backend per le funzionalità principali;
- installabile come PWA su Android;
- sufficientemente completa da gestire sia partite normali sia Commander.

L'app sarà inizialmente sviluppata come Blazor WebAssembly e pubblicata come applicazione web statica.

## 2. Filosofia dell'app

L'app deve privilegiare la velocità durante una partita.

L'utente non dovrebbe essere costretto a:

- aprire menu per modificare frequentemente un valore;
- confermare ogni modifica;
- navigare tra numerose schermate;
- creare un account;
- avere una connessione Internet.

Le informazioni più importanti devono essere sempre visibili nella schermata principale della partita.

### Principio generale

Pochi tocchi per modificare qualsiasi valore importante.

## 3. Tipologie di partita

L'app dovrà supportare inizialmente:

### Partita normale

- 2 giocatori
- 20 punti vita iniziali

### Commander

- 2–6 giocatori
- 40 punti vita iniziali
- danno da comandante
- eventualmente Partner

### Possibili modalità future

- 3–6 giocatori non Commander
- Two-Headed Giant
- Planechase
- Archenemy
- altre modalità multiplayer

Queste modalità non fanno necessariamente parte della prima versione.

## 4. Avvio dell'app

All'apertura viene mostrata la schermata principale.

Possibile struttura:

```text
┌─────────────────────────┐
│                         │
│       MTG COUNTER       │
│                         │
│     [ NUOVA PARTITA ]   │
│                         │
│       [ STORICO ]       │
│                         │
│      [ IMPOSTAZIONI ]   │
│                         │
└─────────────────────────┘
```

La schermata deve essere minimale.

L'utente deve poter iniziare una partita con un solo tap.

## 5. Creazione della partita

Premendo **Nuova partita**, l'utente può configurare la partita.

Configurazione minima:

- modalità;
- numero di giocatori;
- punti vita iniziali;
- nomi dei giocatori.

Esempio:

```text
NUOVA PARTITA

Modalità
○ Standard
○ Commander

Giocatori
[ 2 ]

Punti vita
[ 20 ]

          [ INIZIA ]
```

Le impostazioni disponibili devono cambiare in base alla modalità selezionata.

Per Commander:

```text
Modalità
Commander

Giocatori
[ 4 ]

Punti vita
40

          [ INIZIA ]
```

## 6. Schermata principale della partita

Questa è la schermata più importante dell'app.

Per una partita a due giocatori:

```text
┌─────────────────────────┐
│       PLAYER 1          │
│                         │
│          40             │
│                         │
│   -5   -1   +1   +5    │
│                         │
├─────────────────────────┤
│                         │
│       PLAYER 2          │
│                         │
│          40             │
│                         │
│   -5   -1   +1   +5    │
│                         │
├─────────────────────────┤
│  UNDO     DADI     MENU │
└─────────────────────────┘
```

La disposizione deve adattarsi automaticamente al numero di giocatori.

## 7. Gestione dei punti vita

Ogni giocatore dispone di un valore Life.

Azioni rapide:

- `-1`
- `+1`
- `-5`
- `+5`

Il valore non deve avere necessariamente un limite minimo o massimo imposto dall'app.

Esempio:

```text
40 → 39
39 → 34
34 → 35
```

L'app deve registrare ogni modifica nella cronologia della partita.

## 8. Modifica diretta dei punti vita

Oltre ai pulsanti rapidi, l'utente deve poter modificare direttamente il totale.

Esempio:

```text
        27
```

Toccando il numero si apre una modalità di modifica.

Possibili modalità:

### Inserimento del nuovo totale

```text
Nuovi punti vita

[ 14 ]

       CONFERMA
```

### Oppure modifica tramite tastierino

```text
┌───────────────┐
│       14      │
├───────────────┤
│ 1 │ 2 │ 3 │
│ 4 │ 5 │ 6 │
│ 7 │ 8 │ 9 │
│   │ 0 │ ← │
└───────────────┘
```

La modalità definitiva verrà decisa durante la progettazione della UI.

## 9. Cronologia delle modifiche

Ogni modifica dei punti vita deve essere registrata.

Esempio:

```text
PLAYER 1

40 → 37   -3
37 → 32   -5
32 → 33   +1
```

La cronologia permette di:

- effettuare Undo;
- eventualmente visualizzare la storia della partita;
- salvare la partita nello storico.

## 10. Undo

L'utente deve poter annullare l'ultima azione.

Esempio:

```text
PLAYER 1: 32

        ↶ UNDO

PLAYER 1: 37
```

L'Undo deve funzionare sulle modifiche registrate nella cronologia.

Idealmente deve essere possibile effettuare più Undo consecutivi.

## 11. Reset

Durante una partita deve essere disponibile un comando per ricominciare la partita.

Il reset deve richiedere conferma:

```text
RICOMINCIA PARTITA?

Tutti i punti vita e i valori
verranno ripristinati.

[ ANNULLA ]   [ RESET ]
```

## 12. Gestione dei giocatori

Ogni giocatore deve avere almeno:

- nome;
- punti vita;
- identificatore interno;
- eventuali valori speciali.

Il nome deve essere modificabile.

Esempio:

```text
PLAYER 1
```

può diventare:

```text
Emanuele
```

La personalizzazione del colore o dell'avatar può essere aggiunta successivamente.

## 13. Commander

La modalità Commander aggiunge:

- 40 punti vita iniziali;
- danno da comandante;
- eventuale Partner.

Per ogni giocatore dovrà essere possibile tenere traccia del danno ricevuto da ogni comandante avversario.

Esempio con quattro giocatori:

```text
Emanuele

Life
32

Commander Damage

Mario       7
Luca        0
Andrea      11
```

Il limite di 21 danni da comandante deve essere evidenziato.

## 14. Partner

Nel caso di Commander con Partner, un giocatore può avere due comandanti.

L'app deve quindi poter rappresentare:

```text
Commander 1
8 damage

Commander 2
3 damage
```

Il danno deve essere associato al singolo comandante e non semplicemente al giocatore avversario.

## 15. Poison

L'app deve poter gestire i poison counter.

Esempio:

```text
Life
18

Poison
6
```

Il valore deve essere modificabile rapidamente.

Il raggiungimento della soglia di eliminazione deve essere evidenziato.

## 16. Energy

L'app può supportare gli Energy counter.

Esempio:

```text
Energy
⚡ 7
```

Devono essere disponibili modifiche rapide analoghe ai punti vita.

## 17. Experience

L'app può supportare gli Experience counter.

Esempio:

```text
Experience
⭐ 4
```

Anche questo valore deve poter essere modificato rapidamente.

## 18. Monarch

L'app deve poter rappresentare lo stato di Monarch.

Possibile indicazione:

```text
👑 Emanuele is Monarch
```

La proprietà della Monarchia deve poter essere trasferita rapidamente a un altro giocatore.

## 19. Initiative

Analogamente alla Monarchia, l'app deve poter rappresentare chi possiede l'Initiative.

## 20. Dado

L'app deve includere un generatore di dadi.

Possibili opzioni:

- d4
- d6
- d8
- d10
- d12
- d20
- d100

L'interfaccia deve consentire un lancio rapido.

Esempio:

```text
D20

       17

     [ ROLL ]
```

Il risultato deve essere casuale e chiaramente visibile.

## 21. Coin flip

Deve essere disponibile anche il lancio di una moneta.

```text
      HEADS

    [ FLIP ]
```

## 22. Turn timer

Una possibile funzionalità è un timer della partita o del turno.

Possibili modalità:

- timer globale;
- timer per giocatore;
- semplice cronometro.

Questa funzionalità verrà definita successivamente perché potrebbe influenzare significativamente la UI.

## 23. Menu durante la partita

Il menu non deve occupare permanentemente spazio prezioso.

Possibili azioni:

```text
MENU

Continua partita

Cronologia

Impostazioni partita

Nuova partita

Termina partita
```

Le funzioni utilizzate frequentemente devono rimanere direttamente accessibili dalla schermata principale.

## 24. Fine partita

Una partita può terminare quando:

- rimane un solo giocatore;
- l'utente seleziona manualmente "Termina partita".

La schermata finale potrebbe mostrare:

```text
PARTITA TERMINATA

🏆 Emanuele

Life: 12

Durata
47:32

[ SALVA ]

[ NUOVA PARTITA ]
```

## 25. Storico delle partite

Lo storico deve permettere di consultare le partite precedenti.

Esempio:

```text
STORICO

5 ottobre 2026
Commander
4 giocatori
Vittoria: Emanuele

4 ottobre 2026
Standard
2 giocatori
Vittoria: Luca
```

Inizialmente lo storico sarà salvato localmente sul dispositivo.

Non è previsto un account.

## 26. Persistenza locale

L'app deve poter conservare localmente:

- impostazioni;
- preferenze dell'utente;
- storico delle partite;
- eventualmente una partita in corso.

L'obiettivo è evitare di perdere una partita se la pagina viene accidentalmente chiusa o ricaricata.

## 27. Offline

Una caratteristica importante è la possibilità di utilizzare l'app senza connessione Internet.

Una volta installata/caricata come PWA, l'app dovrebbe poter funzionare anche offline.

Le funzionalità principali non devono dipendere da:

- API esterne;
- database remoto;
- autenticazione;
- connessione Internet.

## 28. PWA

L'app sarà progettata per poter essere installata come applicazione sul telefono.

Su Android l'utente potrà aggiungerla alla schermata Home.

L'esperienza desiderata è:

```text
Home screen
     ↓
MTG Counter
     ↓
App a schermo quasi pieno
     ↓
Nessun browser visibile
```

## 29. Personalizzazione

Possibili impostazioni:

- tema chiaro/scuro;
- colori dei giocatori;
- dimensione del testo;
- orientamento preferito;
- punti vita iniziali;
- valori dei pulsanti rapidi;
- vibrazione al tocco;
- suoni;
- durata timer.

La personalizzazione avanzata non è necessaria per il primo MVP.

## 30. MVP

La prima versione funzionante dovrebbe essere volutamente limitata.

### MVP — obbligatorio

- 2 giocatori;
- 20 punti vita;
- modifica `+1 / -1`;
- modifica `+5 / -5`;
- modifica diretta;
- Undo;
- Reset;
- nomi giocatori;
- schermata mobile;
- schermo sempre acceso durante la partita;
- salvataggio della partita corrente;
- funzionamento offline/PWA;
- deploy su GitHub Pages.

### Versione successiva

- 3–6 giocatori;
- Commander;
- Commander Damage;
- Partner;
- Poison;
- Energy;
- Experience;
- Monarch;
- Initiative;
- dadi;
- coin flip;
- cronologia;
- storico partite.

### Funzionalità future

- timer;
- temi;
- statistiche;
- personalizzazione avanzata;
- modalità multiplayer;
- sincronizzazione tra dispositivi;
- eventuale backend.

## 31. Requisiti non funzionali

L'app deve essere:

### Rapida

Le modifiche dei valori devono essere immediate.

### Mobile-first

La UI deve essere progettata prima per smartphone e successivamente adattata a desktop/tablet.

### Offline-first

Il funzionamento principale non deve dipendere dalla rete.

### Semplice

La schermata della partita deve avere il minor numero possibile di elementi inutili.

### Affidabile

Una modifica ai punti vita non deve andare persa accidentalmente.

### Sempre visibile

Durante una partita lo schermo non deve spegnersi automaticamente, anche se nessuno lo tocca per diversi minuti.

- Il blocco dello spegnimento è attivo solo mentre la schermata della partita è aperta; nelle altre schermate vale il comportamento normale del telefono.
- Se l'app passa in background (cambio app, blocco manuale) il blocco viene rilasciato e va riattivato automaticamente al ritorno.
- Se il dispositivo o il browser non supportano la funzione, l'app continua a funzionare normalmente.

### Estendibile

L'architettura deve permettere di aggiungere nuove tipologie di counter senza riscrivere il sistema di base.

## 32. Architettura tecnica — prima proposta

### Frontend

```text
Blazor WebAssembly
        │
        ├── Pages
        │
        ├── Components
        │
        ├── Services
        │
        └── Models
```

Framework UI:

```text
MudBlazor
```

## 33. Modello dati iniziale

Il concetto centrale sarà una `Game`.

```text
Game
│
├── Settings
│
├── Players
│     ├── Player
│     ├── Player
│     └── ...
│
└── History
      ├── LifeChange
      ├── LifeChange
      └── ...
```

Un `Player` conterrà i valori relativi al giocatore.

Esempio concettuale:

```text
Player
├── Id
├── Name
├── Life
├── Poison
├── Energy
├── Experience
└── ...
```

## 34. Separazione tra UI e logica

La UI non dovrà contenere direttamente tutta la logica della partita.

Indicativamente:

```text
UI
 ↓
GameService
 ↓
Game
 ↓
Persistence
```

I componenti Blazor visualizzano e modificano lo stato attraverso i servizi.

Questo renderà più semplice:

- testare la logica;
- aggiungere nuove funzionalità;
- modificare la UI;
- cambiare successivamente il sistema di persistenza.

## 35. Struttura iniziale del progetto

Proposta:

```text
MtgLifeCounter/
│
├── Components/
│   ├── Game/
│   │   ├── PlayerPanel.razor
│   │   ├── LifeControls.razor
│   │   └── ...
│   │
│   ├── Common/
│   │   └── ...
│   │
│   └── Layout/
│
├── Models/
│   ├── Game.cs
│   ├── Player.cs
│   ├── GameSettings.cs
│   └── LifeChange.cs
│
├── Services/
│   ├── GameService.cs
│   ├── StorageService.cs
│   └── RandomService.cs
│
├── Pages/
│   ├── Home.razor
│   ├── Game.razor
│   └── Settings.razor
│
└── wwwroot/
    ├── css/
    ├── images/
    ├── icons/
    └── manifest.json
```

Questa struttura è provvisoria e verrà modificata quando definiremo meglio le funzionalità.

## 36. Deployment

Il progetto sarà pubblicato tramite:

```text
GitHub
   ↓
GitHub Actions
   ↓
dotnet publish
   ↓
GitHub Pages
```

Il sito sarà quindi disponibile gratuitamente tramite un indirizzo `github.io`.

Successivamente sarà eventualmente possibile associare un dominio personalizzato.

## 37. Principio architetturale fondamentale

L'app deve essere progettata in modo che la partita non dipenda dalla pagina web.

La pagina è solamente una rappresentazione dello stato della partita.

Concettualmente:

```text
             ┌──────────────┐
             │     Game     │
             │    State     │
             └──────┬───────┘
                    │
          ┌─────────┼─────────┐
          ↓         ↓         ↓
       UI Mobile  Storage   History
```

Questo permetterà in futuro di cambiare completamente l'interfaccia senza dover riscrivere la logica di gioco.

## 38. Decisioni ancora da prendere

Prima di iniziare a scrivere il codice definitivo dovremo decidere:

- layout preciso della schermata a 2 giocatori;
- layout per 3–6 giocatori;
- come modificare direttamente i punti vita;
- gestione del tap/swipe;
- orientamento portrait/landscape;
- sistema di colori;
- gestione del Commander Damage;
- struttura dei counter;
- come salvare i dati;
- struttura dello storico;
- gestione della partita interrotta;
- timer;
- sistema di temi;
- eventuale supporto desktop;
- nome definitivo dell'app.

Queste decisioni verranno prese prima di implementare le relative funzionalità, evitando di costruire l'app attorno a supposizioni che poi dovremo cambiare.
