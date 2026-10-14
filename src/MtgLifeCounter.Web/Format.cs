namespace MtgLifeCounter.Web;

public static class Format
{
    /// <summary>Variazione con segno sempre visibile, usando il vero segno meno tipografico: "+3", "−5".</summary>
    public static string Delta(int delta) => delta switch
    {
        > 0 => $"+{delta}",
        < 0 => $"−{-delta}",
        _ => "0",
    };

    /// <summary>Numero con il segno meno tipografico.</summary>
    public static string Number(int value) => value < 0 ? $"−{-value}" : value.ToString();

    /// <summary>Durata in stile cronometro: "7:05", "47:32", "1:02:03".</summary>
    public static string Duration(TimeSpan duration) =>
        duration.TotalHours >= 1
            ? $"{(int)duration.TotalHours}:{duration.Minutes:00}:{duration.Seconds:00}"
            : $"{duration.Minutes}:{duration.Seconds:00}";
}
