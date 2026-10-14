using MtgLifeCounter.Core;

namespace MtgLifeCounter.Web.Components.Game;

/// <param name="LethalAt">Valore a cui il giocatore perde, se il segnalino ne ha uno.</param>
public sealed record CounterInfo(CounterKind Kind, string Label, string Icon, int? LethalAt = null)
{
    public bool IsLethal(int value) => value >= LethalAt;
}

/// <summary>
/// Segnalini del giocatore mostrati in partita. Per renderne visibile un altro
/// (radiazioni, energia, esperienza, ... esistono già nel Core) basta aggiungerlo qui.
/// </summary>
public static class PlayerCounters
{
    // U+FE0E chiede la versione "testo" del simbolo invece dell'emoji a colori.
    public static readonly IReadOnlyList<CounterInfo> Visible =
    [
        new(CounterKind.Poison, "Veleno", "☠︎", Player.LethalPoison),
    ];
}
