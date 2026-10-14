namespace MtgLifeCounter.Web.Components.Game;

/// <summary>Aspetto di un pannello durante l'estrazione di chi inizia.</summary>
public enum RevealState
{
    /// <summary>Nessuna estrazione in corso.</summary>
    None,

    /// <summary>La "luce" della roulette è su questo pannello.</summary>
    Lit,

    /// <summary>Pannello spento: la luce è altrove.</summary>
    Dim,

    /// <summary>Giocatore estratto: inizia lui.</summary>
    Winner,
}
