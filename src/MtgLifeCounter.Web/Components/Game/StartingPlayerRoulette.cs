namespace MtgLifeCounter.Web.Components.Game;

/// <summary>
/// Animazione "roulette" per mostrare chi inizia: una luce gira sui posti in ordine di gioco,
/// rallenta e si ferma sul giocatore già estratto. Il risultato resta visibile per qualche
/// secondo. Un tocco durante la rotazione salta al risultato; un tocco sul risultato chiude.
/// </summary>
public sealed class StartingPlayerRoulette : IDisposable
{
    // Almeno questo numero di passi, così anche con 2 giocatori la luce fa qualche giro.
    private const int MinSteps = 12;
    private const double FastStepMs = 60;
    private const double SlowStepMs = 380;
    private static readonly TimeSpan ResultDuration = TimeSpan.FromSeconds(2.5);

    private CancellationTokenSource? _spin;
    private CancellationTokenSource? _show;
    private int _winnerSeat;
    private int? _litSeat;
    private bool _disposed;

    // Ogni esecuzione ha il suo numero: una vecchia esecuzione interrotta non tocca più lo stato.
    private int _run;

    /// <summary>True finché l'animazione o il risultato sono sullo schermo.</summary>
    public bool IsRunning { get; private set; }

    public bool HasLanded { get; private set; }

    public RevealState StateOf(int seat)
    {
        if (!IsRunning)
            return RevealState.None;

        if (HasLanded)
            return seat == _winnerSeat ? RevealState.Winner : RevealState.Dim;

        return seat == _litSeat ? RevealState.Lit : RevealState.Dim;
    }

    /// <param name="render">Chiede un nuovo render della pagina.</param>
    /// <param name="onLanded">Chiamato quando la luce si ferma (ad esempio per vibrare).</param>
    public async Task RunAsync(int playerCount, int winnerSeat, Func<Task> render, Func<Task>? onLanded = null)
    {
        Stop();
        var run = ++_run;
        _spin = new CancellationTokenSource();
        _show = new CancellationTokenSource();
        _winnerSeat = winnerSeat;
        IsRunning = true;
        HasLanded = false;

        try
        {
            await SpinAsync(playerCount, render, _spin.Token);
        }
        catch (OperationCanceledException)
        {
            // Tocco durante la rotazione: si passa subito al risultato.
        }

        if (!IsCurrent(run))
            return;

        HasLanded = true;
        _litSeat = null;
        await render();
        if (onLanded is not null)
            await onLanded();

        try
        {
            await Task.Delay(ResultDuration, _show.Token);
        }
        catch (OperationCanceledException)
        {
            // Tocco sul risultato: si chiude subito.
        }

        if (!IsCurrent(run))
            return;

        IsRunning = false;
        HasLanded = false;
        await render();
    }

    /// <summary>Tocco dell'utente: salta la rotazione oppure chiude il risultato.</summary>
    public void Skip()
    {
        if (!IsRunning)
            return;

        if (HasLanded)
            _show?.Cancel();
        else
            _spin?.Cancel();
    }

    public void Dispose()
    {
        _disposed = true;
        Stop();
    }

    private async Task SpinAsync(int playerCount, Func<Task> render, CancellationToken token)
    {
        // Giri completi più i passi che servono per finire esattamente sul vincitore.
        var rounds = Math.Max(2, (int)Math.Ceiling(MinSteps / (double)playerCount));
        var steps = rounds * playerCount + _winnerSeat;

        for (var step = 0; step < steps; step++)
        {
            _litSeat = step % playerCount;
            await render();

            // Rallentamento progressivo, come una ruota che si ferma.
            var progress = (double)step / steps;
            var delay = FastStepMs + (SlowStepMs - FastStepMs) * progress * progress;
            await Task.Delay(TimeSpan.FromMilliseconds(delay), token);
        }
    }

    private bool IsCurrent(int run) => !_disposed && run == _run;

    private void Stop()
    {
        _spin?.Cancel();
        _show?.Cancel();
        IsRunning = false;
        HasLanded = false;
    }
}
