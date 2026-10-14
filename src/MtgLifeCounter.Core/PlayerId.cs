using System.Text.Json;
using System.Text.Json.Serialization;

namespace MtgLifeCounter.Core;

/// <summary>
/// Identificatore interno di un giocatore: la sua posizione al tavolo, a partire da 1.
/// </summary>
[JsonConverter(typeof(PlayerIdJsonConverter))]
public readonly record struct PlayerId(int Value)
{
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Serializza <see cref="PlayerId"/> come semplice numero invece che come oggetto.
/// </summary>
internal sealed class PlayerIdJsonConverter : JsonConverter<PlayerId>
{
    public override PlayerId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, PlayerId value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);
}
