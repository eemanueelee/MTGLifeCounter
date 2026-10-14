namespace MtgLifeCounter.Core.Persistence;

/// <summary>
/// Forma salvabile di una partita. Contiene solo dati: lo stato dei giocatori
/// si ricava riapplicando gli eventi.
/// </summary>
/// <param name="StartingPlayer">Assente nei salvataggi precedenti all'estrazione di chi inizia.</param>
/// <param name="ClockPausedDuration">
/// Assente nei salvataggi precedenti al cronometro: vale zero, cioè cronometro in corsa da <paramref name="StartedAt"/>.
/// </param>
/// <param name="ClockPausedAt">Inizio della pausa in corso; null se il cronometro sta andando.</param>
public sealed record GameSnapshot(
    int Version,
    GameSettings Settings,
    IReadOnlyList<string> PlayerNames,
    DateTimeOffset StartedAt,
    IReadOnlyList<GameEvent> Events,
    PlayerId? StartingPlayer = null,
    TimeSpan ClockPausedDuration = default,
    DateTimeOffset? ClockPausedAt = null)
{
    /// <summary>Da incrementare a ogni modifica incompatibile del formato salvato.</summary>
    public const int CurrentVersion = 1;
}
