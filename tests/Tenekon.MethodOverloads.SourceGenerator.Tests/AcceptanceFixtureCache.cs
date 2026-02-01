namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public static class AcceptanceFixtureCache
{
    private static readonly Lazy<AcceptanceFixture> Cache = new(() => new AcceptanceFixture());

    public static AcceptanceFixture Instance => Cache.Value;
}
