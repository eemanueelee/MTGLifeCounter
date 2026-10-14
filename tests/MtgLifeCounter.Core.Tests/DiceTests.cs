namespace MtgLifeCounter.Core.Tests;

public class DiceTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(20)]
    [InlineData(100)]
    public void Roll_produces_every_face_and_nothing_else(int sides)
    {
        var random = new Random(123);

        var results = Enumerable.Range(0, sides * 60).Select(_ => Dice.Roll(random, sides)).ToHashSet();

        Assert.Equal(Enumerable.Range(1, sides).ToHashSet(), results);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-6)]
    public void A_die_needs_at_least_two_sides(int sides)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Dice.Roll(new Random(1), sides));
    }

    [Fact]
    public void Coin_lands_on_both_faces()
    {
        var random = new Random(5);

        var faces = Enumerable.Range(0, 100).Select(_ => Dice.Flip(random)).ToHashSet();

        Assert.Equal([CoinFace.Heads, CoinFace.Tails], faces.Order());
    }
}
