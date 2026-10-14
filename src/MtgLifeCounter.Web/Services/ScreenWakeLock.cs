using Microsoft.JSInterop;

namespace MtgLifeCounter.Web.Services;

/// <summary>
/// Impedisce lo spegnimento automatico dello schermo (Screen Wake Lock API).
/// Se il browser non la supporta non succede nulla e l'app funziona normalmente.
/// </summary>
public sealed class ScreenWakeLock(IJSRuntime js, ILogger<ScreenWakeLock> logger) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    public Task EnableAsync() => InvokeAsync("enable");

    public Task DisableAsync() => InvokeAsync("disable");

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
    }

    private async Task InvokeAsync(string function)
    {
        try
        {
            _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/wake-lock.js");
            await _module.InvokeVoidAsync(function);
        }
        catch (JSException ex)
        {
            logger.LogWarning(ex, "Wake lock non disponibile.");
        }
    }
}
