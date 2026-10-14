using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MtgLifeCounter.Web;
using MtgLifeCounter.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<GameStorage>();
builder.Services.AddSingleton<GameSession>();
builder.Services.AddSingleton<ScreenWakeLock>();
builder.Services.AddSingleton<ThemeService>();

var host = builder.Build();

// Riprende l'eventuale partita in corso prima del primo render.
host.Services.GetRequiredService<GameSession>().LoadSavedGame();

await host.RunAsync();
