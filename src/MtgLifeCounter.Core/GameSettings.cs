namespace MtgLifeCounter.Core;

public enum GameFormat
{
    Standard,
    Commander,
}

/// <summary>
/// Configurazione scelta all'avvio della partita. Non cambia durante la partita.
/// </summary>
public sealed record GameSettings(GameFormat Format, int PlayerCount, int StartingLife)
{
    public const int MinPlayers = 2;
    public const int MaxPlayers = 6;

    public static GameSettings Standard() => new(GameFormat.Standard, 2, 20);

    public static GameSettings Commander(int playerCount = 4) => new(GameFormat.Commander, playerCount, 40);

    internal void Validate()
    {
        if (PlayerCount is < MinPlayers or > MaxPlayers)
            throw new ArgumentOutOfRangeException(nameof(PlayerCount), PlayerCount,
                $"Il numero di giocatori deve essere tra {MinPlayers} e {MaxPlayers}.");

        if (StartingLife <= 0)
            throw new ArgumentOutOfRangeException(nameof(StartingLife), StartingLife,
                "I punti vita iniziali devono essere positivi.");
    }
}
