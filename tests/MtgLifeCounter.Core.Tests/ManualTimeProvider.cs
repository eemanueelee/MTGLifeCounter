namespace MtgLifeCounter.Core.Tests;

/// <summary>Orologio controllato dai test: il tempo avanza solo con <see cref="Advance"/>.</summary>
internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2026, 10, 13, 20, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}
