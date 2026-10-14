using Microsoft.JSInterop;

namespace MtgLifeCounter.Web.Services;

public sealed record ThemeInfo(string Id, string Name);

/// <summary>
/// Tema grafico dell'app. Il tema è l'attributo data-theme di &lt;html&gt;: i colori e i font
/// di ogni tema sono in css/app.css. La scelta viene ricordata nel browser e applicata
/// da js/theme.js prima che parta Blazor, così non si vede il tema sbagliato all'avvio.
///
/// Per ora l'app usa solo il tema Ludipop e il selettore non è mostrato: il servizio resta
/// pronto per quando si vorrà di nuovo far scegliere il tema (vedi themeSelectionEnabled in theme.js).
/// </summary>
public sealed class ThemeService(IJSRuntime js, ILogger<ThemeService> logger)
{
    public static readonly IReadOnlyList<ThemeInfo> Themes =
    [
        new("ludipop", "Ludipop"),
        new("arcade", "Arcade"),
    ];

    private readonly IJSInProcessRuntime _js = (IJSInProcessRuntime)js;

    public string Current
    {
        get
        {
            try
            {
                return _js.Invoke<string?>("mtgTheme.get") ?? Themes[0].Id;
            }
            catch (JSException ex)
            {
                logger.LogWarning(ex, "Impossibile leggere il tema.");
                return Themes[0].Id;
            }
        }
    }

    public void Apply(string themeId)
    {
        if (Themes.All(t => t.Id != themeId))
            throw new ArgumentOutOfRangeException(nameof(themeId), themeId, "Tema sconosciuto.");

        try
        {
            _js.InvokeVoid("mtgTheme.set", themeId);
        }
        catch (JSException ex)
        {
            logger.LogWarning(ex, "Impossibile applicare il tema.");
        }
    }
}
