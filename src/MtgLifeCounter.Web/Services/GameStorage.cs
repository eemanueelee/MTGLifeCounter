using Microsoft.JSInterop;
using MtgLifeCounter.Core;
using MtgLifeCounter.Core.Persistence;

namespace MtgLifeCounter.Web.Services;

/// <summary>
/// Salva la partita in corso nel localStorage del browser.
/// Le chiamate sono sincrone: quando un tocco è stato gestito, è anche già salvato.
/// </summary>
public sealed class GameStorage(IJSRuntime js, ILogger<GameStorage> logger)
{
    private const string CurrentGameKey = "mtg-life-counter.current-game";

    private readonly IJSInProcessRuntime _js = (IJSInProcessRuntime)js;

    /// <summary>La partita salvata, oppure null se non c'è o se il salvataggio non è leggibile.</summary>
    public Game? LoadCurrentGame()
    {
        try
        {
            var json = _js.Invoke<string?>("localStorage.getItem", CurrentGameKey);
            return json is null ? null : GameSerializer.Deserialize(json);
        }
        catch (Exception ex)
        {
            // Salvataggio corrotto, di una versione incompatibile o storage non accessibile:
            // meglio ripartire da zero che bloccare l'app.
            logger.LogWarning(ex, "Impossibile caricare la partita salvata.");
            return null;
        }
    }

    public void SaveCurrentGame(Game game)
    {
        try
        {
            _js.InvokeVoid("localStorage.setItem", CurrentGameKey, GameSerializer.Serialize(game));
        }
        catch (Exception ex)
        {
            // Ad esempio storage pieno o disabilitato: la partita continua comunque in memoria.
            logger.LogWarning(ex, "Impossibile salvare la partita.");
        }
    }
}
