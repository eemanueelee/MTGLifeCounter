namespace MtgLifeCounter.Core;

public enum CoinFace
{
    Heads,
    Tails,
}

/// <summary>
/// Dadi e moneta. Il generatore arriva da fuori, così nei test si può usare
/// un <see cref="Random"/> con seme fisso.
/// </summary>
public static class Dice
{
    /// <summary>I dadi che si usano al tavolo, dal d4 al d100.</summary>
    public static readonly IReadOnlyList<int> StandardSides = [4, 6, 8, 10, 12, 20, 100];

    /// <summary>Tira un dado da <paramref name="sides"/> facce: risultato da 1 a <paramref name="sides"/>.</summary>
    public static int Roll(Random random, int sides)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sides, 2);
        return random.Next(1, sides + 1);
    }

    public static CoinFace Flip(Random random) =>
        random.Next(2) == 0 ? CoinFace.Heads : CoinFace.Tails;
}
