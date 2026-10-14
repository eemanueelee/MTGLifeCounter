using MtgLifeCounter.Core.Persistence;

namespace MtgLifeCounter.Core;

/// <summary>
/// Una partita: impostazioni, giocatori e cronologia degli eventi.
/// Non sa nulla della UI né di come viene salvata; chi è interessato ascolta <see cref="Changed"/>.
/// </summary>
public sealed class Game
{
    /// <summary>
    /// Tocchi consecutivi sullo stesso contatore entro questo intervallo diventano un solo evento:
    /// cinque tocchi su -1 compaiono in cronologia come un unico -5 e un Undo li annulla tutti.
    /// </summary>
    public static readonly TimeSpan GroupingWindow = TimeSpan.FromSeconds(1.5);

    /// <summary>Comandanti per giocatore: 1, oppure 2 con Partner.</summary>
    public const int MaxCommandersPerPlayer = 2;

    private readonly TimeProvider _time;
    private readonly List<Player> _players;
    private readonly List<GameEvent> _events = [];

    // Stato transitorio del raggruppamento: non viene salvato, dopo un ricaricamento il gruppo è chiuso.
    private bool _groupOpen;
    private DateTimeOffset _lastTapAt;

    /// <param name="playerNames">Nomi in ordine di posto; quelli mancanti o vuoti ricevono il nome predefinito.</param>
    public Game(GameSettings settings, IEnumerable<string?>? playerNames = null, TimeProvider? time = null)
    {
        settings.Validate();

        Settings = settings;
        _time = time ?? TimeProvider.System;
        StartedAt = _time.GetUtcNow();

        var names = playerNames?.ToList() ?? [];
        _players = Enumerable.Range(1, settings.PlayerCount)
            .Select(i => new PlayerId(i))
            .Select(id => new Player(id, NormalizeName(id, names.ElementAtOrDefault(id.Value - 1))))
            .ToList();

        Rebuild();
    }

    /// <summary>Notifica ogni modifica dello stato (UI da aggiornare, partita da salvare).</summary>
    public event Action? Changed;

    public GameSettings Settings { get; }

    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>Il giocatore estratto per iniziare, se l'estrazione è già stata fatta.</summary>
    public PlayerId? StartingPlayer { get; private set; }

    /// <summary>Tempo totale trascorso in pausa (escluso quello della pausa in corso).</summary>
    public TimeSpan ClockPausedDuration { get; private set; }

    /// <summary>Quando è iniziata la pausa in corso; null se il cronometro sta andando.</summary>
    public DateTimeOffset? ClockPausedAt { get; private set; }

    public bool IsClockRunning => ClockPausedAt is null;

    /// <summary>Durata della partita: tempo dall'inizio, meno le pause.</summary>
    public TimeSpan Elapsed
    {
        get
        {
            var end = ClockPausedAt ?? _time.GetUtcNow();
            var elapsed = end - StartedAt - ClockPausedDuration;
            return elapsed > TimeSpan.Zero ? elapsed : TimeSpan.Zero;
        }
    }

    public IReadOnlyList<Player> Players => _players;

    public IReadOnlyList<GameEvent> History => _events;

    public bool CanUndo => _events.Count > 0;

    /// <summary>
    /// Il gruppo di tocchi ancora in corso, se c'è: la UI lo mostra come variazione temporanea
    /// ("-5") finché non scade la <see cref="GroupingWindow"/>.
    /// </summary>
    public DeltaEvent? PendingChange =>
        _groupOpen && _events.Count > 0 && _events[^1] is DeltaEvent change
            && _time.GetUtcNow() - _lastTapAt <= GroupingWindow
            ? change
            : null;

    public Player GetPlayer(PlayerId id) =>
        id.Value >= 1 && id.Value <= _players.Count
            ? _players[id.Value - 1]
            : throw new ArgumentOutOfRangeException(nameof(id), id, "Giocatore inesistente.");

    /// <summary>
    /// Modifica rapida di un contatore (+1, -5, ...). Tocchi ravvicinati sullo stesso
    /// contatore dello stesso giocatore vengono uniti nello stesso evento.
    /// </summary>
    public void AdjustCounter(PlayerId playerId, CounterKind counter, int delta)
    {
        var player = GetPlayer(playerId);
        if (!counter.CanBeNegative())
            delta = Math.Max(delta, -player[counter]);

        RecordTap(new CounterChanged(playerId, counter, delta, _time.GetUtcNow()));
    }

    /// <summary>
    /// Modifica il danno che <paramref name="target"/> ha ricevuto dal comandante
    /// <paramref name="commander"/> di <paramref name="source"/>. Toglie (o restituisce)
    /// anche altrettanti punti vita. Il danno non scende mai sotto zero.
    /// </summary>
    public void AdjustCommanderDamage(PlayerId target, PlayerId source, int commander, int delta)
    {
        if (Settings.Format != GameFormat.Commander)
            throw new InvalidOperationException("Il danno da comandante esiste solo nelle partite Commander.");

        if (commander is < 0 or >= MaxCommandersPerPlayer)
            throw new ArgumentOutOfRangeException(nameof(commander), commander, "Comandante inesistente.");

        var player = GetPlayer(target);
        GetPlayer(source);

        delta = Math.Max(delta, -player.CommanderDamage(source, commander));
        RecordTap(new CommanderDamageChanged(target, source, commander, delta, _time.GetUtcNow()));
    }

    /// <summary>
    /// Imposta direttamente il nuovo totale di un contatore. È un'azione esplicita,
    /// quindi resta un evento a sé e non si unisce ai tocchi precedenti o successivi.
    /// </summary>
    public void SetCounter(PlayerId playerId, CounterKind counter, int value)
    {
        var player = GetPlayer(playerId);
        if (!counter.CanBeNegative())
            value = Math.Max(value, 0);

        var delta = value - player[counter];
        if (delta == 0)
            return;

        var change = new CounterChanged(playerId, counter, delta, _time.GetUtcNow());
        _events.Add(change);
        _groupOpen = false;

        Apply(change);
        Changed?.Invoke();
    }

    /// <summary>
    /// Estrae a caso il giocatore che inizia. Il generatore arriva da fuori,
    /// così nei test si può usare un <see cref="Random"/> con seme fisso.
    /// </summary>
    public PlayerId ChooseStartingPlayer(Random random)
    {
        var chosen = _players[random.Next(_players.Count)].Id;
        StartingPlayer = chosen;
        Changed?.Invoke();
        return chosen;
    }

    /// <summary>Mette in pausa il cronometro, o lo fa ripartire se è già in pausa.</summary>
    public void ToggleClock()
    {
        var now = _time.GetUtcNow();

        if (ClockPausedAt is { } pausedAt)
        {
            ClockPausedDuration += now - pausedAt;
            ClockPausedAt = null;
        }
        else
        {
            ClockPausedAt = now;
        }

        Changed?.Invoke();
    }

    /// <summary>Annulla l'ultimo evento. Restituisce false se non c'è nulla da annullare.</summary>
    public bool Undo()
    {
        if (!CanUndo)
            return false;

        _events.RemoveAt(_events.Count - 1);
        _groupOpen = false;

        Rebuild();
        Changed?.Invoke();
        return true;
    }

    /// <summary>
    /// Ricomincia la partita: contatori ai valori iniziali e cronologia svuotata.
    /// Impostazioni e nomi dei giocatori restano invariati; il giocatore che inizia
    /// va estratto di nuovo. Non è annullabile.
    /// </summary>
    public void Reset()
    {
        _events.Clear();
        _groupOpen = false;
        StartedAt = _time.GetUtcNow();
        StartingPlayer = null;
        ClockPausedDuration = TimeSpan.Zero;
        ClockPausedAt = null;

        Rebuild();
        Changed?.Invoke();
    }

    /// <summary>
    /// Rinomina un giocatore; un nome vuoto ripristina quello predefinito.
    /// Il nome non fa parte della cronologia, quindi Undo non lo modifica.
    /// </summary>
    public void RenamePlayer(PlayerId playerId, string? name)
    {
        var player = GetPlayer(playerId);
        var newName = NormalizeName(playerId, name);
        if (newName == player.Name)
            return;

        player.Name = newName;
        Changed?.Invoke();
    }

    public GameSnapshot ToSnapshot() => new(
        GameSnapshot.CurrentVersion,
        Settings,
        _players.Select(p => p.Name).ToList(),
        StartedAt,
        _events.ToList(),
        StartingPlayer,
        ClockPausedDuration,
        ClockPausedAt);

    /// <summary>Ricostruisce una partita salvata. Lancia un'eccezione se i dati non sono validi.</summary>
    public static Game FromSnapshot(GameSnapshot snapshot, TimeProvider? time = null)
    {
        if (snapshot.Version != GameSnapshot.CurrentVersion)
            throw new NotSupportedException($"Versione del salvataggio non supportata: {snapshot.Version}.");

        if (snapshot.PlayerNames.Count != snapshot.Settings.PlayerCount)
            throw new InvalidDataException("Il numero di nomi non corrisponde al numero di giocatori.");

        var game = new Game(snapshot.Settings, snapshot.PlayerNames, time)
        {
            StartedAt = snapshot.StartedAt,
            ClockPausedDuration = snapshot.ClockPausedDuration,
            ClockPausedAt = snapshot.ClockPausedAt,
        };
        if (snapshot.StartingPlayer is { } starting)
            game.StartingPlayer = game.GetPlayer(starting).Id;

        game._events.AddRange(snapshot.Events);
        game.Rebuild();
        return game;
    }

    /// <summary>
    /// Registra un tocco. Se continua il gruppo di tocchi in corso sullo stesso valore
    /// viene sommato all'ultimo evento invece di aggiungerne uno nuovo.
    /// </summary>
    private void RecordTap(DeltaEvent tap)
    {
        if (tap.Delta == 0)
            return;

        if (PendingChange is { } last && last.IsSameValueAs(tap))
        {
            _events.RemoveAt(_events.Count - 1);
            var mergedDelta = last.Delta + tap.Delta;

            if (mergedDelta == 0)
            {
                // I tocchi si sono annullati a vicenda: niente da ricordare in cronologia.
                _groupOpen = false;
            }
            else
            {
                _events.Add(last with { Delta = mergedDelta });
                _lastTapAt = tap.At;
            }
        }
        else
        {
            _events.Add(tap);
            _groupOpen = true;
            _lastTapAt = tap.At;
        }

        // Si applica solo la variazione di questo tocco: le precedenti sono già nello stato.
        Apply(tap);
        Changed?.Invoke();
    }

    /// <summary>Riporta i giocatori allo stato iniziale e riapplica tutti gli eventi in ordine.</summary>
    private void Rebuild()
    {
        foreach (var player in _players)
            player.ResetCounters(Settings.StartingLife);

        foreach (var gameEvent in _events)
            Apply(gameEvent);
    }

    private void Apply(GameEvent gameEvent)
    {
        switch (gameEvent)
        {
            case CounterChanged change:
                GetPlayer(change.Player).Add(change.Counter, change.Delta);
                break;
            case CommanderDamageChanged damage:
                var target = GetPlayer(damage.Target);
                target.AddCommanderDamage(damage.Source, damage.Commander, damage.Delta);
                target.Add(CounterKind.Life, -damage.Delta);
                break;
            default:
                throw new NotSupportedException($"Evento non gestito: {gameEvent.GetType().Name}.");
        }
    }

    private static string NormalizeName(PlayerId id, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Player.DefaultName(id);

        name = name.Trim();
        return name.Length <= Player.MaxNameLength ? name : name[..Player.MaxNameLength].TrimEnd();
    }
}
