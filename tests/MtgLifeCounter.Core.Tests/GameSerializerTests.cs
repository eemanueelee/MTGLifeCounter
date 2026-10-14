using System.Text.Json;
using MtgLifeCounter.Core.Persistence;

namespace MtgLifeCounter.Core.Tests;

public class GameSerializerTests
{
    private static readonly PlayerId P1 = new(1);
    private static readonly PlayerId P2 = new(2);

    private readonly ManualTimeProvider _time = new();

    [Fact]
    public void Round_trip_preserves_settings_names_values_and_history()
    {
        var game = new Game(GameSettings.Commander(3), ["Emanuele", "Mario", "Luca"], _time);
        game.AdjustCounter(P1, CounterKind.Life, -7);
        game.AdjustCounter(P2, CounterKind.Poison, 2);
        game.SetCounter(P2, CounterKind.Life, 31);

        var restored = GameSerializer.Deserialize(GameSerializer.Serialize(game), _time);

        Assert.Equal(game.Settings, restored.Settings);
        Assert.Equal(game.StartedAt, restored.StartedAt);
        Assert.Equal(game.Players.Select(p => p.Name), restored.Players.Select(p => p.Name));
        Assert.Equal(game.History, restored.History);
        Assert.Equal(33, restored.GetPlayer(P1).Life);
        Assert.Equal(31, restored.GetPlayer(P2).Life);
        Assert.Equal(2, restored.GetPlayer(P2)[CounterKind.Poison]);
    }

    [Fact]
    public void Undo_works_on_a_restored_game()
    {
        var game = new Game(GameSettings.Standard(), time: _time);
        game.AdjustCounter(P1, CounterKind.Life, -3);
        game.SetCounter(P1, CounterKind.Life, 5);

        var restored = GameSerializer.Deserialize(GameSerializer.Serialize(game), _time);
        restored.Undo();

        Assert.Equal(17, restored.GetPlayer(P1).Life);
    }

    [Fact]
    public void Tap_after_restore_does_not_merge_into_saved_history()
    {
        var game = new Game(GameSettings.Standard(), time: _time);
        game.AdjustCounter(P1, CounterKind.Life, -1);

        var restored = GameSerializer.Deserialize(GameSerializer.Serialize(game), _time);
        restored.AdjustCounter(P1, CounterKind.Life, -1);

        Assert.Equal(2, restored.History.Count);
    }

    [Fact]
    public void Json_is_readable()
    {
        var game = new Game(GameSettings.Standard(), time: _time);
        game.AdjustCounter(P1, CounterKind.Life, -3);

        var json = GameSerializer.Serialize(game);

        Assert.Contains("\"format\":\"Standard\"", json);
        Assert.Contains("\"type\":\"counterChanged\"", json);
        Assert.Contains("\"player\":1", json);
        Assert.Contains("\"counter\":\"Life\"", json);
    }

    [Fact]
    public void Unsupported_version_is_rejected()
    {
        var json = GameSerializer.Serialize(new Game(GameSettings.Standard(), time: _time))
            .Replace($"\"version\":{GameSnapshot.CurrentVersion}", "\"version\":999");

        Assert.Throws<NotSupportedException>(() => GameSerializer.Deserialize(json, _time));
    }

    [Fact]
    public void Corrupted_json_is_rejected()
    {
        Assert.ThrowsAny<JsonException>(() => GameSerializer.Deserialize("{ non è json", _time));
    }
}
