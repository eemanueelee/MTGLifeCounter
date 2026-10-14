using System.Text.Json;
using System.Text.Json.Serialization;

namespace MtgLifeCounter.Core.Persistence;

/// <summary>
/// Converte una partita in JSON e viceversa. Dove salvare il testo (localStorage, file, ...)
/// è responsabilità di chi lo usa.
/// </summary>
public static class GameSerializer
{
    public static string Serialize(Game game) =>
        JsonSerializer.Serialize(game.ToSnapshot(), CoreJsonContext.Default.GameSnapshot);

    /// <summary>
    /// Ricostruisce la partita dal JSON. Lancia <see cref="JsonException"/> o un'altra eccezione
    /// se il contenuto è corrotto o di una versione non supportata.
    /// </summary>
    public static Game Deserialize(string json, TimeProvider? time = null)
    {
        var snapshot = JsonSerializer.Deserialize(json, CoreJsonContext.Default.GameSnapshot)
            ?? throw new JsonException("Salvataggio vuoto.");
        return Game.FromSnapshot(snapshot, time);
    }
}

// Serializzazione generata a compile time: nessuna reflection, compatibile con il trimming di Blazor WASM.
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(GameSnapshot))]
internal sealed partial class CoreJsonContext : JsonSerializerContext;
