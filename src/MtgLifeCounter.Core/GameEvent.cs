using System.Text.Json.Serialization;

namespace MtgLifeCounter.Core;

/// <summary>
/// Un'azione avvenuta durante la partita. L'elenco ordinato degli eventi è l'unica fonte
/// di verità: lo stato dei giocatori si ottiene riapplicandoli in ordine, e Undo rimuove
/// semplicemente l'ultimo.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CounterChanged), "counterChanged")]
[JsonDerivedType(typeof(CommanderDamageChanged), "commanderDamageChanged")]
public abstract record GameEvent(DateTimeOffset At);

/// <summary>
/// Evento che sposta un valore di una certa quantità. Tocchi ravvicinati sullo stesso
/// valore vengono uniti in un solo evento sommando le variazioni.
/// </summary>
public abstract record DeltaEvent(int Delta, DateTimeOffset At) : GameEvent(At)
{
    /// <summary>True se <paramref name="other"/> modifica lo stesso valore e può quindi essere unito a questo.</summary>
    internal abstract bool IsSameValueAs(DeltaEvent other);
}

/// <summary>
/// Variazione di un contatore di un giocatore. Si registra sempre la differenza,
/// anche quando l'utente inserisce direttamente il nuovo totale.
/// </summary>
public sealed record CounterChanged(PlayerId Player, CounterKind Counter, int Delta, DateTimeOffset At)
    : DeltaEvent(Delta, At)
{
    internal override bool IsSameValueAs(DeltaEvent other) =>
        other is CounterChanged c && c.Player == Player && c.Counter == Counter;
}

/// <summary>
/// Danno da comandante subito da <paramref name="Target"/> a opera del comandante
/// <paramref name="Commander"/> (0, oppure 1 per il Partner) di <paramref name="Source"/>.
/// Come da regolamento, il danno fa perdere anche altrettanti punti vita.
/// </summary>
public sealed record CommanderDamageChanged(PlayerId Target, PlayerId Source, int Commander, int Delta, DateTimeOffset At)
    : DeltaEvent(Delta, At)
{
    internal override bool IsSameValueAs(DeltaEvent other) =>
        other is CommanderDamageChanged c && c.Target == Target && c.Source == Source && c.Commander == Commander;
}
