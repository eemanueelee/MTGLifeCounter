namespace MtgLifeCounter.Web.Components.Game;

/// <summary>
/// Posizione di un giocatore nella griglia (2 colonne) e rotazione del suo pannello,
/// in modo che sia leggibile da chi siede su quel lato del telefono.
/// </summary>
/// <param name="Rotation">0 = in basso, 90 = lato sinistro, 180 = in alto, 270 = lato destro.</param>
/// <param name="Position">Descrizione del posto, mostrata nella configurazione.</param>
public sealed record SeatPlacement(int Row, int Column, int ColumnSpan, int Rotation, string Position)
{
    public bool IsSideways => Rotation is 90 or 270;

    public string GridArea => $"{Row} / {Column} / span 1 / span {ColumnSpan}";
}

/// <summary>
/// Disposizione dei giocatori attorno al telefono, appoggiato in verticale al centro del tavolo.
/// I posti seguono il senso orario: l'ordine dei giocatori è quello di gioco.
/// </summary>
public sealed record TableLayout(IReadOnlyList<string> RowTracks, int ToolbarRow, IReadOnlyList<SeatPlacement> Seats)
{
    private const string Fill = "1fr";
    private const string Toolbar = "auto";

    public string GridTemplateRows => string.Join(' ', RowTracks);

    public static TableLayout For(int playerCount) => playerCount switch
    {
        2 => new([Fill, Toolbar, Fill], 2,
        [
            new(1, 1, 2, 180, "in alto"),
            new(3, 1, 2, 0, "in basso"),
        ]),
        3 => new([Fill, Toolbar, Fill], 2,
        [
            new(1, 1, 2, 180, "in alto"),
            new(3, 2, 1, 270, "a destra"),
            new(3, 1, 1, 90, "a sinistra"),
        ]),
        4 => new([Fill, Toolbar, Fill], 2,
        [
            new(1, 1, 1, 90, "a sinistra, in alto"),
            new(1, 2, 1, 270, "a destra, in alto"),
            new(3, 2, 1, 270, "a destra, in basso"),
            new(3, 1, 1, 90, "a sinistra, in basso"),
        ]),
        5 => new([Fill, Fill, Toolbar, Fill], 3,
        [
            new(1, 1, 2, 180, "in alto"),
            new(2, 2, 1, 270, "a destra, in alto"),
            new(4, 2, 1, 270, "a destra, in basso"),
            new(4, 1, 1, 90, "a sinistra, in basso"),
            new(2, 1, 1, 90, "a sinistra, in alto"),
        ]),
        6 => new([Fill, Fill, Toolbar, Fill], 3,
        [
            new(1, 1, 1, 90, "a sinistra, in alto"),
            new(1, 2, 1, 270, "a destra, in alto"),
            new(2, 2, 1, 270, "a destra, al centro"),
            new(4, 2, 1, 270, "a destra, in basso"),
            new(4, 1, 1, 90, "a sinistra, in basso"),
            new(2, 1, 1, 90, "a sinistra, al centro"),
        ]),
        _ => throw new ArgumentOutOfRangeException(nameof(playerCount), playerCount, "Layout non disponibile."),
    };
}
