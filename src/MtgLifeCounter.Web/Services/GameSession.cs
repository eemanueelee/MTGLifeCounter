using MtgLifeCounter.Core;

namespace MtgLifeCounter.Web.Services;

/// <summary>
/// Tiene la partita corrente per tutta l'app e la salva automaticamente a ogni modifica.
/// </summary>
public sealed class GameSession(GameStorage storage)
{
    public Game? Current { get; private set; }

    /// <summary>
    /// True quando è appena stato estratto chi inizia e la schermata di gioco deve mostrarlo.
    /// Una partita ripresa dal salvataggio non lo mostra di nuovo.
    /// </summary>
    public bool StartingPlayerRevealPending { get; private set; }

    public void LoadSavedGame()
    {
        if (storage.LoadCurrentGame() is { } game)
            Attach(game);
    }

    /// <summary>Inizia una nuova partita, che sostituisce quella corrente, ed estrae chi inizia.</summary>
    public Game StartNew(GameSettings settings, IEnumerable<string?> playerNames)
    {
        var game = new Game(settings, playerNames);
        Attach(game);
        ChooseStartingPlayer(game);
        return game;
    }

    /// <summary>Ricomincia la partita corrente con gli stessi giocatori ed estrae di nuovo chi inizia.</summary>
    public void Restart()
    {
        if (Current is null)
            return;

        Current.Reset();
        ChooseStartingPlayer(Current);
    }

    /// <summary>La schermata di gioco chiama questo metodo quando inizia a mostrare l'estrazione.</summary>
    public void MarkStartingPlayerRevealed() => StartingPlayerRevealPending = false;

    private void ChooseStartingPlayer(Game game)
    {
        // Changed fa partire anche il salvataggio.
        game.ChooseStartingPlayer(Random.Shared);
        StartingPlayerRevealPending = true;
    }

    private void Attach(Game game)
    {
        if (Current is not null)
            Current.Changed -= SaveCurrent;

        Current = game;
        Current.Changed += SaveCurrent;
    }

    private void SaveCurrent() => storage.SaveCurrentGame(Current!);
}
