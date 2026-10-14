namespace MtgLifeCounter.Core;

/// <summary>Danno ricevuto da un singolo comandante avversario.</summary>
public readonly record struct CommanderDamageEntry(PlayerId Source, int Commander, int Damage);

/// <summary>
/// Stato corrente di un giocatore. Si legge dall'esterno, ma lo modifica solo <see cref="Game"/>.
/// </summary>
public sealed class Player
{
    public const int MaxNameLength = 24;

    /// <summary>Danno da un singolo comandante che fa perdere la partita.</summary>
    public const int LethalCommanderDamage = 21;

    /// <summary>Segnalini veleno che fanno perdere la partita.</summary>
    public const int LethalPoison = 10;

    private readonly Dictionary<CounterKind, int> _counters = [];
    private readonly Dictionary<(PlayerId Source, int Commander), int> _commanderDamage = [];

    internal Player(PlayerId id, string name)
    {
        Id = id;
        Name = name;
    }

    public PlayerId Id { get; }

    public string Name { get; internal set; }

    /// <summary>True se il nome è ancora quello assegnato automaticamente ("Player 1", ...).</summary>
    public bool HasDefaultName => Name == DefaultName(Id);

    public int Life => this[CounterKind.Life];

    /// <summary>Valore di un contatore; quelli mai toccati valgono 0.</summary>
    public int this[CounterKind counter] => _counters.GetValueOrDefault(counter);

    /// <summary>Danno ricevuto dal comandante <paramref name="commander"/> di <paramref name="source"/>.</summary>
    public int CommanderDamage(PlayerId source, int commander = 0) =>
        _commanderDamage.GetValueOrDefault((source, commander));

    /// <summary>Il comandante che ha inflitto più danno a questo giocatore, se ce n'è almeno uno.</summary>
    public CommanderDamageEntry? HighestCommanderDamage =>
        _commanderDamage
            .Where(kv => kv.Value > 0)
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key.Source.Value)
            .ThenBy(kv => kv.Key.Commander)
            .Select(kv => new CommanderDamageEntry(kv.Key.Source, kv.Key.Commander, kv.Value))
            .Cast<CommanderDamageEntry?>()
            .FirstOrDefault();

    public bool HasLethalCommanderDamage => HighestCommanderDamage?.Damage >= LethalCommanderDamage;

    public bool HasLethalPoison => this[CounterKind.Poison] >= LethalPoison;

    internal static string DefaultName(PlayerId id) => $"Player {id.Value}";

    internal void Add(CounterKind counter, int delta) => _counters[counter] = this[counter] + delta;

    internal void AddCommanderDamage(PlayerId source, int commander, int delta) =>
        _commanderDamage[(source, commander)] = CommanderDamage(source, commander) + delta;

    internal void ResetCounters(int startingLife)
    {
        _counters.Clear();
        _commanderDamage.Clear();
        _counters[CounterKind.Life] = startingLife;
    }
}
