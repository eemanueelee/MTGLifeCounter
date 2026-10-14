namespace MtgLifeCounter.Core;

/// <summary>
/// Tipi di contatore di un giocatore. Per aggiungerne uno nuovo basta un nuovo valore:
/// la gestione di modifiche, cronologia e Undo è la stessa per tutti.
/// I valori sono salvati per nome, quindi l'ordine si può cambiare senza rompere i salvataggi.
/// </summary>
public enum CounterKind
{
    Life,
    Poison,
    Energy,
    Experience,
    Rad,
}

public static class CounterKindExtensions
{
    /// <summary>La vita può scendere sotto zero; i segnalini no.</summary>
    public static bool CanBeNegative(this CounterKind counter) => counter == CounterKind.Life;
}
