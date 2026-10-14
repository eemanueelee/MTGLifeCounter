using MtgLifeCounter.Core.Persistence;

namespace MtgLifeCounter.Core.Tests;

public class GameClockTests
{
    private readonly ManualTimeProvider _time = new();

    private Game NewGame() => new(GameSettings.Standard(), time: _time);

    [Fact]
    public void Clock_runs_from_the_start_of_the_game()
    {
        var game = NewGame();

        _time.Advance(TimeSpan.FromMinutes(12));

        Assert.True(game.IsClockRunning);
        Assert.Equal(TimeSpan.FromMinutes(12), game.Elapsed);
    }

    [Fact]
    public void Pause_freezes_the_clock_and_resume_continues_from_there()
    {
        var game = NewGame();
        _time.Advance(TimeSpan.FromMinutes(5));

        game.ToggleClock();
        _time.Advance(TimeSpan.FromMinutes(30));

        Assert.False(game.IsClockRunning);
        Assert.Equal(TimeSpan.FromMinutes(5), game.Elapsed);

        game.ToggleClock();
        _time.Advance(TimeSpan.FromMinutes(2));

        Assert.True(game.IsClockRunning);
        Assert.Equal(TimeSpan.FromMinutes(7), game.Elapsed);
    }

    [Fact]
    public void Several_pauses_add_up()
    {
        var game = NewGame();

        for (var i = 0; i < 3; i++)
        {
            _time.Advance(TimeSpan.FromMinutes(1));
            game.ToggleClock();
            _time.Advance(TimeSpan.FromMinutes(10));
            game.ToggleClock();
        }

        Assert.Equal(TimeSpan.FromMinutes(3), game.Elapsed);
    }

    [Fact]
    public void Toggling_the_clock_is_not_part_of_the_history()
    {
        var game = NewGame();

        game.ToggleClock();

        Assert.Empty(game.History);
    }

    [Fact]
    public void Reset_restarts_a_paused_clock_from_zero()
    {
        var game = NewGame();
        _time.Advance(TimeSpan.FromMinutes(20));
        game.ToggleClock();

        game.Reset();
        _time.Advance(TimeSpan.FromSeconds(30));

        Assert.True(game.IsClockRunning);
        Assert.Equal(TimeSpan.FromSeconds(30), game.Elapsed);
    }

    [Fact]
    public void Paused_clock_survives_save_and_load()
    {
        var game = NewGame();
        _time.Advance(TimeSpan.FromMinutes(8));
        game.ToggleClock();

        var json = GameSerializer.Serialize(game);
        _time.Advance(TimeSpan.FromHours(1)); // l'app resta chiusa un'ora
        var restored = GameSerializer.Deserialize(json, _time);

        Assert.False(restored.IsClockRunning);
        Assert.Equal(TimeSpan.FromMinutes(8), restored.Elapsed);
    }

    [Fact]
    public void Running_clock_keeps_counting_while_the_app_is_closed()
    {
        var game = NewGame();
        _time.Advance(TimeSpan.FromMinutes(8));
        game.ToggleClock();
        _time.Advance(TimeSpan.FromMinutes(2));
        game.ToggleClock();

        var json = GameSerializer.Serialize(game);
        _time.Advance(TimeSpan.FromMinutes(5));
        var restored = GameSerializer.Deserialize(json, _time);

        Assert.Equal(TimeSpan.FromMinutes(13), restored.Elapsed);
    }

    [Fact]
    public void Saves_without_clock_data_count_from_the_start_of_the_game()
    {
        const string json = """
            {"version":1,"settings":{"format":"Standard","playerCount":2,"startingLife":20},
             "playerNames":["Emanuele","Mario"],"startedAt":"2026-10-13T20:00:00+00:00","events":[]}
            """;

        var restored = GameSerializer.Deserialize(json, _time);

        Assert.True(restored.IsClockRunning);
        Assert.Equal(_time.GetUtcNow() - new DateTimeOffset(2026, 10, 13, 20, 0, 0, TimeSpan.Zero), restored.Elapsed);
    }
}
