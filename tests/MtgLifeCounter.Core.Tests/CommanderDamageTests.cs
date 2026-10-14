using MtgLifeCounter.Core.Persistence;

namespace MtgLifeCounter.Core.Tests;

public class CommanderDamageTests
{
    private static readonly PlayerId P1 = new(1);
    private static readonly PlayerId P2 = new(2);
    private static readonly PlayerId P3 = new(3);

    private readonly ManualTimeProvider _time = new();

    private Game NewCommanderGame() => new(GameSettings.Commander(4), time: _time);

    private void WaitPastGroupingWindow() => _time.Advance(Game.GroupingWindow + TimeSpan.FromMilliseconds(1));

    [Fact]
    public void Commander_damage_is_tracked_per_source_and_also_reduces_life()
    {
        var game = NewCommanderGame();

        game.AdjustCommanderDamage(P1, P2, 0, 7);

        var target = game.GetPlayer(P1);
        Assert.Equal(7, target.CommanderDamage(P2));
        Assert.Equal(0, target.CommanderDamage(P3));
        Assert.Equal(33, target.Life);
        Assert.Equal(40, game.GetPlayer(P2).Life);
    }

    [Fact]
    public void Partner_commanders_are_tracked_separately()
    {
        var game = NewCommanderGame();

        game.AdjustCommanderDamage(P1, P2, 0, 8);
        game.AdjustCommanderDamage(P1, P2, 1, 3);

        Assert.Equal(8, game.GetPlayer(P1).CommanderDamage(P2, 0));
        Assert.Equal(3, game.GetPlayer(P1).CommanderDamage(P2, 1));
        Assert.Equal(29, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Reducing_commander_damage_gives_life_back_and_never_goes_below_zero()
    {
        var game = NewCommanderGame();
        game.AdjustCommanderDamage(P1, P2, 0, 2);
        WaitPastGroupingWindow();

        game.AdjustCommanderDamage(P1, P2, 0, -5);

        Assert.Equal(0, game.GetPlayer(P1).CommanderDamage(P2));
        Assert.Equal(40, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Removing_damage_that_was_never_dealt_records_nothing()
    {
        var game = NewCommanderGame();

        game.AdjustCommanderDamage(P1, P2, 0, -1);

        Assert.Empty(game.History);
        Assert.Equal(40, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Quick_taps_on_the_same_commander_merge_into_one_event()
    {
        var game = NewCommanderGame();

        for (var i = 0; i < 4; i++)
            game.AdjustCommanderDamage(P1, P2, 0, 1);

        var damage = Assert.IsType<CommanderDamageChanged>(Assert.Single(game.History));
        Assert.Equal(4, damage.Delta);
        Assert.Equal(4, game.PendingChange?.Delta);
    }

    [Fact]
    public void Taps_on_different_sources_or_on_life_are_not_merged()
    {
        var game = NewCommanderGame();

        game.AdjustCommanderDamage(P1, P2, 0, 1);
        game.AdjustCommanderDamage(P1, P3, 0, 1);
        game.AdjustCommanderDamage(P1, P3, 1, 1);
        game.AdjustCounter(P1, CounterKind.Life, -1);

        Assert.Equal(4, game.History.Count);
    }

    [Fact]
    public void Undo_reverts_both_damage_and_life()
    {
        var game = NewCommanderGame();
        game.AdjustCommanderDamage(P1, P2, 0, 6);

        game.Undo();

        Assert.Equal(0, game.GetPlayer(P1).CommanderDamage(P2));
        Assert.Equal(40, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Twenty_one_damage_from_a_single_commander_is_lethal()
    {
        var game = NewCommanderGame();
        game.AdjustCommanderDamage(P1, P2, 0, 20);
        game.AdjustCommanderDamage(P1, P3, 0, 20);

        Assert.False(game.GetPlayer(P1).HasLethalCommanderDamage);

        game.AdjustCommanderDamage(P1, P3, 0, 1);

        Assert.True(game.GetPlayer(P1).HasLethalCommanderDamage);
        Assert.Equal(new CommanderDamageEntry(P3, 0, 21), game.GetPlayer(P1).HighestCommanderDamage);
    }

    [Fact]
    public void Highest_commander_damage_is_null_when_no_damage_was_taken()
    {
        var game = NewCommanderGame();

        Assert.Null(game.GetPlayer(P1).HighestCommanderDamage);
        Assert.False(game.GetPlayer(P1).HasLethalCommanderDamage);
    }

    [Fact]
    public void Reset_clears_commander_damage()
    {
        var game = NewCommanderGame();
        game.AdjustCommanderDamage(P1, P2, 0, 21);

        game.Reset();

        Assert.Null(game.GetPlayer(P1).HighestCommanderDamage);
        Assert.Equal(40, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Commander_damage_is_rejected_outside_commander_games()
    {
        var game = new Game(GameSettings.Standard(), time: _time);

        Assert.Throws<InvalidOperationException>(() => game.AdjustCommanderDamage(P1, P2, 0, 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Unknown_commander_index_is_rejected(int commander)
    {
        var game = NewCommanderGame();

        Assert.Throws<ArgumentOutOfRangeException>(() => game.AdjustCommanderDamage(P1, P2, commander, 1));
    }

    [Fact]
    public void Commander_damage_survives_save_and_load()
    {
        var game = NewCommanderGame();
        game.AdjustCommanderDamage(P1, P2, 0, 5);
        game.AdjustCommanderDamage(P1, P2, 1, 2);

        var restored = GameSerializer.Deserialize(GameSerializer.Serialize(game), _time);

        Assert.Equal(5, restored.GetPlayer(P1).CommanderDamage(P2, 0));
        Assert.Equal(2, restored.GetPlayer(P1).CommanderDamage(P2, 1));
        Assert.Equal(33, restored.GetPlayer(P1).Life);
        Assert.Equal(game.History, restored.History);
    }
}
