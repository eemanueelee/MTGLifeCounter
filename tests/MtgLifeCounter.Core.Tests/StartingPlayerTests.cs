using MtgLifeCounter.Core.Persistence;

namespace MtgLifeCounter.Core.Tests;

public class StartingPlayerTests
{
    private readonly ManualTimeProvider _time = new();

    [Fact]
    public void No_starting_player_until_one_is_chosen()
    {
        var game = new Game(GameSettings.Commander(4), time: _time);

        Assert.Null(game.StartingPlayer);
    }

    [Fact]
    public void Chosen_player_is_one_of_the_players_and_is_remembered()
    {
        var game = new Game(GameSettings.Commander(4), time: _time);

        var chosen = game.ChooseStartingPlayer(new Random(42));

        Assert.Contains(game.Players, p => p.Id == chosen);
        Assert.Equal(chosen, game.StartingPlayer);
    }

    [Fact]
    public void Every_player_can_be_chosen()
    {
        var game = new Game(GameSettings.Commander(6), time: _time);
        var random = new Random(1);

        var chosen = Enumerable.Range(0, 300)
            .Select(_ => game.ChooseStartingPlayer(random))
            .ToHashSet();

        Assert.Equal(game.Players.Select(p => p.Id).ToHashSet(), chosen);
    }

    [Fact]
    public void Choosing_does_not_touch_history()
    {
        var game = new Game(GameSettings.Standard(), time: _time);

        game.ChooseStartingPlayer(new Random(7));

        Assert.Empty(game.History);
        Assert.False(game.CanUndo);
    }

    [Fact]
    public void Reset_clears_the_starting_player()
    {
        var game = new Game(GameSettings.Standard(), time: _time);
        game.ChooseStartingPlayer(new Random(7));

        game.Reset();

        Assert.Null(game.StartingPlayer);
    }

    [Fact]
    public void Starting_player_survives_save_and_load()
    {
        var game = new Game(GameSettings.Commander(5), time: _time);
        var chosen = game.ChooseStartingPlayer(new Random(3));

        var restored = GameSerializer.Deserialize(GameSerializer.Serialize(game), _time);

        Assert.Equal(chosen, restored.StartingPlayer);
    }

    [Fact]
    public void Saves_made_before_this_feature_still_load()
    {
        // Salvataggio scritto da una versione dell'app che non conosceva "startingPlayer".
        const string json = """
            {"version":1,"settings":{"format":"Standard","playerCount":2,"startingLife":20},
             "playerNames":["Emanuele","Mario"],"startedAt":"2026-10-13T20:00:00+00:00","events":[]}
            """;

        var game = GameSerializer.Deserialize(json, _time);

        Assert.Null(game.StartingPlayer);
        Assert.Equal("Emanuele", game.GetPlayer(new PlayerId(1)).Name);
    }
}
