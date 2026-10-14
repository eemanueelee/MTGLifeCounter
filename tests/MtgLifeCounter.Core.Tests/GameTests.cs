namespace MtgLifeCounter.Core.Tests;

public class GameTests
{
    private static readonly PlayerId P1 = new(1);
    private static readonly PlayerId P2 = new(2);

    private readonly ManualTimeProvider _time = new();

    private Game NewStandardGame() => new(GameSettings.Standard(), time: _time);

    private void WaitPastGroupingWindow() => _time.Advance(Game.GroupingWindow + TimeSpan.FromMilliseconds(1));

    [Fact]
    public void Standard_game_starts_with_two_players_at_20_life()
    {
        var game = NewStandardGame();

        Assert.Equal(2, game.Players.Count);
        Assert.All(game.Players, p => Assert.Equal(20, p.Life));
        Assert.Equal(["Player 1", "Player 2"], game.Players.Select(p => p.Name));
        Assert.Empty(game.History);
        Assert.False(game.CanUndo);
    }

    [Fact]
    public void Commander_game_starts_at_40_life()
    {
        var game = new Game(GameSettings.Commander(4), time: _time);

        Assert.Equal(4, game.Players.Count);
        Assert.All(game.Players, p => Assert.Equal(40, p.Life));
    }

    [Fact]
    public void Other_counters_start_at_zero()
    {
        var player = NewStandardGame().GetPlayer(P1);

        Assert.Equal(0, player[CounterKind.Poison]);
        Assert.Equal(0, player[CounterKind.Energy]);
        Assert.Equal(0, player[CounterKind.Experience]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    public void Player_count_outside_2_to_6_is_rejected(int playerCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Game(new GameSettings(GameFormat.Standard, playerCount, 20)));
    }

    [Fact]
    public void Unknown_player_is_rejected()
    {
        var game = NewStandardGame();

        Assert.Throws<ArgumentOutOfRangeException>(() => game.AdjustCounter(new PlayerId(3), CounterKind.Life, -1));
    }

    [Fact]
    public void Adjust_changes_only_the_target_player()
    {
        var game = NewStandardGame();

        game.AdjustCounter(P1, CounterKind.Life, -5);

        Assert.Equal(15, game.GetPlayer(P1).Life);
        Assert.Equal(20, game.GetPlayer(P2).Life);
    }

    [Fact]
    public void Life_has_no_lower_bound()
    {
        var game = NewStandardGame();

        game.SetCounter(P1, CounterKind.Life, 2);
        game.AdjustCounter(P1, CounterKind.Life, -5);

        Assert.Equal(-3, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Quick_taps_on_same_counter_merge_into_one_event()
    {
        var game = NewStandardGame();

        for (var i = 0; i < 5; i++)
        {
            game.AdjustCounter(P1, CounterKind.Life, -1);
            _time.Advance(TimeSpan.FromMilliseconds(300));
        }

        var change = Assert.IsType<CounterChanged>(Assert.Single(game.History));
        Assert.Equal(-5, change.Delta);
        Assert.Equal(15, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Grouping_window_is_measured_from_the_last_tap()
    {
        var game = NewStandardGame();

        // Ogni tocco arriva entro la finestra dal precedente, anche se il totale la supera.
        for (var i = 0; i < 4; i++)
        {
            game.AdjustCounter(P1, CounterKind.Life, -1);
            _time.Advance(Game.GroupingWindow);
        }

        Assert.Single(game.History);
    }

    [Fact]
    public void Taps_after_the_grouping_window_create_a_new_event()
    {
        var game = NewStandardGame();

        game.AdjustCounter(P1, CounterKind.Life, -1);
        WaitPastGroupingWindow();
        game.AdjustCounter(P1, CounterKind.Life, -1);

        Assert.Equal(2, game.History.Count);
    }

    [Fact]
    public void Taps_on_different_players_are_not_merged()
    {
        var game = NewStandardGame();

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.AdjustCounter(P2, CounterKind.Life, -1);
        game.AdjustCounter(P1, CounterKind.Life, -1);

        Assert.Equal(3, game.History.Count);
    }

    [Fact]
    public void Taps_on_different_counters_are_not_merged()
    {
        var game = NewStandardGame();

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.AdjustCounter(P1, CounterKind.Poison, 1);

        Assert.Equal(2, game.History.Count);
    }

    [Fact]
    public void Taps_that_cancel_out_leave_no_event()
    {
        var game = NewStandardGame();

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.AdjustCounter(P1, CounterKind.Life, +1);

        Assert.Empty(game.History);
        Assert.Equal(20, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Taps_that_cancel_out_do_not_merge_into_an_older_event()
    {
        var game = NewStandardGame();
        game.AdjustCounter(P1, CounterKind.Life, -3);
        WaitPastGroupingWindow();

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.AdjustCounter(P1, CounterKind.Life, +1);
        game.AdjustCounter(P1, CounterKind.Life, -2);

        Assert.Equal([-3, -2], game.History.Cast<CounterChanged>().Select(c => c.Delta));
    }

    [Fact]
    public void Undo_reverts_a_whole_group_of_taps()
    {
        var game = NewStandardGame();
        game.AdjustCounter(P1, CounterKind.Life, -3);
        WaitPastGroupingWindow();

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.Undo();

        Assert.Equal(17, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Multiple_undos_walk_back_through_history()
    {
        var game = NewStandardGame();
        game.AdjustCounter(P1, CounterKind.Life, -3);
        game.AdjustCounter(P2, CounterKind.Life, -5);
        game.SetCounter(P1, CounterKind.Life, 10);

        Assert.True(game.Undo());
        Assert.Equal(17, game.GetPlayer(P1).Life);

        Assert.True(game.Undo());
        Assert.Equal(20, game.GetPlayer(P2).Life);

        Assert.True(game.Undo());
        Assert.Equal(20, game.GetPlayer(P1).Life);

        Assert.False(game.Undo());
        Assert.False(game.CanUndo);
    }

    [Fact]
    public void Tap_after_undo_starts_a_new_event()
    {
        var game = NewStandardGame();
        game.AdjustCounter(P1, CounterKind.Life, -3);
        WaitPastGroupingWindow();
        game.AdjustCounter(P1, CounterKind.Life, -1);

        game.Undo();
        game.AdjustCounter(P1, CounterKind.Life, -1);

        Assert.Equal([-3, -1], game.History.Cast<CounterChanged>().Select(c => c.Delta));
    }

    [Fact]
    public void Pending_change_shows_the_group_in_progress_until_the_window_expires()
    {
        var game = NewStandardGame();
        Assert.Null(game.PendingChange);

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.AdjustCounter(P1, CounterKind.Life, -1);
        Assert.Equal(-2, game.PendingChange?.Delta);

        WaitPastGroupingWindow();
        Assert.Null(game.PendingChange);
    }

    [Fact]
    public void Pending_change_is_cleared_by_undo_and_by_direct_set()
    {
        var game = NewStandardGame();

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.Undo();
        Assert.Null(game.PendingChange);

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.SetCounter(P2, CounterKind.Life, 10);
        Assert.Null(game.PendingChange);
    }

    [Fact]
    public void Set_records_the_difference_from_the_current_value()
    {
        var game = NewStandardGame();

        game.SetCounter(P1, CounterKind.Life, 14);

        var change = Assert.IsType<CounterChanged>(Assert.Single(game.History));
        Assert.Equal(-6, change.Delta);
        Assert.Equal(14, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Set_to_the_current_value_records_nothing()
    {
        var game = NewStandardGame();

        game.SetCounter(P1, CounterKind.Life, 20);

        Assert.Empty(game.History);
    }

    [Fact]
    public void Set_is_never_merged_with_taps()
    {
        var game = NewStandardGame();

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.SetCounter(P1, CounterKind.Life, 10);
        game.AdjustCounter(P1, CounterKind.Life, -1);

        Assert.Equal(3, game.History.Count);
        Assert.Equal(9, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Reset_restores_starting_values_and_keeps_names()
    {
        var game = NewStandardGame();
        game.RenamePlayer(P1, "Emanuele");
        game.AdjustCounter(P1, CounterKind.Life, -7);
        game.AdjustCounter(P2, CounterKind.Poison, 3);
        _time.Advance(TimeSpan.FromMinutes(10));

        game.Reset();

        Assert.All(game.Players, p => Assert.Equal(20, p.Life));
        Assert.Equal(0, game.GetPlayer(P2)[CounterKind.Poison]);
        Assert.Empty(game.History);
        Assert.Equal("Emanuele", game.GetPlayer(P1).Name);
        Assert.Equal(_time.GetUtcNow(), game.StartedAt);
    }

    [Fact]
    public void Rename_trims_and_blank_restores_default_name()
    {
        var game = NewStandardGame();

        game.RenamePlayer(P1, "  Emanuele  ");
        Assert.Equal("Emanuele", game.GetPlayer(P1).Name);

        game.RenamePlayer(P1, "   ");
        Assert.Equal("Player 1", game.GetPlayer(P1).Name);
    }

    [Fact]
    public void Long_names_are_truncated()
    {
        var game = NewStandardGame();

        game.RenamePlayer(P1, new string('x', Player.MaxNameLength + 10));

        Assert.Equal(Player.MaxNameLength, game.GetPlayer(P1).Name.Length);
    }

    [Fact]
    public void Default_name_is_recognized()
    {
        var game = NewStandardGame();
        game.RenamePlayer(P1, "Emanuele");

        Assert.False(game.GetPlayer(P1).HasDefaultName);
        Assert.True(game.GetPlayer(P2).HasDefaultName);
    }

    [Fact]
    public void Rename_is_not_undoable()
    {
        var game = NewStandardGame();
        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.RenamePlayer(P1, "Emanuele");

        game.Undo();

        Assert.Equal("Emanuele", game.GetPlayer(P1).Name);
        Assert.Equal(20, game.GetPlayer(P1).Life);
    }

    [Fact]
    public void Names_passed_at_creation_are_used_and_missing_ones_get_defaults()
    {
        var game = new Game(GameSettings.Commander(3), ["Emanuele", ""], _time);

        Assert.Equal(["Emanuele", "Player 2", "Player 3"], game.Players.Select(p => p.Name));
    }

    [Fact]
    public void Changed_is_raised_only_when_something_changes()
    {
        var game = NewStandardGame();
        var count = 0;
        game.Changed += () => count++;

        game.AdjustCounter(P1, CounterKind.Life, -1);
        game.AdjustCounter(P1, CounterKind.Life, 0);
        game.SetCounter(P1, CounterKind.Life, 19);
        game.RenamePlayer(P1, "Player 1");
        game.Undo();
        game.Undo();
        game.Reset();

        Assert.Equal(3, count); // Adjust, Undo riuscito, Reset
    }
}
