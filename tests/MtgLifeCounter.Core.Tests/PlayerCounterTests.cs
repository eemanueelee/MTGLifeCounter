using MtgLifeCounter.Core.Persistence;

namespace MtgLifeCounter.Core.Tests;

public class PlayerCounterTests
{
    private static readonly PlayerId P1 = new(1);

    private readonly ManualTimeProvider _time = new();

    private Game NewGame() => new(GameSettings.Standard(), time: _time);

    private void WaitPastGroupingWindow() => _time.Advance(Game.GroupingWindow + TimeSpan.FromMilliseconds(1));

    [Theory]
    [InlineData(CounterKind.Poison)]
    [InlineData(CounterKind.Rad)]
    public void Counters_do_not_change_life(CounterKind counter)
    {
        var game = NewGame();

        game.AdjustCounter(P1, counter, 3);

        Assert.Equal(3, game.GetPlayer(P1)[counter]);
        Assert.Equal(20, game.GetPlayer(P1).Life);
    }

    [Theory]
    [InlineData(CounterKind.Poison)]
    [InlineData(CounterKind.Rad)]
    public void Counters_never_go_below_zero(CounterKind counter)
    {
        var game = NewGame();
        game.AdjustCounter(P1, counter, 2);
        WaitPastGroupingWindow();

        game.AdjustCounter(P1, counter, -5);

        Assert.Equal(0, game.GetPlayer(P1)[counter]);
    }

    [Fact]
    public void Removing_a_counter_that_is_not_there_records_nothing()
    {
        var game = NewGame();

        game.AdjustCounter(P1, CounterKind.Poison, -1);

        Assert.Empty(game.History);
    }

    [Fact]
    public void Setting_a_negative_counter_value_stops_at_zero()
    {
        var game = NewGame();
        game.AdjustCounter(P1, CounterKind.Rad, 4);

        game.SetCounter(P1, CounterKind.Rad, -3);

        Assert.Equal(0, game.GetPlayer(P1)[CounterKind.Rad]);
    }

    [Fact]
    public void Ten_poison_counters_are_lethal()
    {
        var game = NewGame();

        game.AdjustCounter(P1, CounterKind.Poison, 9);
        Assert.False(game.GetPlayer(P1).HasLethalPoison);

        game.AdjustCounter(P1, CounterKind.Poison, 1);
        Assert.True(game.GetPlayer(P1).HasLethalPoison);
    }

    [Fact]
    public void Rad_counters_survive_save_and_load()
    {
        var game = NewGame();
        game.AdjustCounter(P1, CounterKind.Rad, 6);

        var restored = GameSerializer.Deserialize(GameSerializer.Serialize(game), _time);

        Assert.Equal(6, restored.GetPlayer(P1)[CounterKind.Rad]);
    }
}
