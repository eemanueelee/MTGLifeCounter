using MtgLifeCounter.Core;

namespace MtgLifeCounter.Web.Components.Game;

/// <summary>Un tiro di dado o di moneta, da disegnare e da mettere nell'elenco degli ultimi tiri.</summary>
/// <param name="Sides">Facce del dado; 0 per la moneta.</param>
public sealed record DiceRoll(int Sides, int Value, CoinFace? Face = null)
{
    public bool IsCoin => Face is not null;

    public string Label => IsCoin ? "Moneta" : $"d{Sides}";

    public string Text => Face switch
    {
        CoinFace.Heads => "Testa",
        CoinFace.Tails => "Croce",
        _ => Value.ToString(),
    };

    public static DiceRoll Die(int sides, int value) => new(sides, value);

    public static DiceRoll Coin(CoinFace face) => new(0, 0, face);
}
