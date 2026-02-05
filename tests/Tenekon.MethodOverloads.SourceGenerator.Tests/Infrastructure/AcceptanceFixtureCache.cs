namespace Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

public static class AcceptanceFixtureCache
{
    private static readonly Lazy<AcceptanceFixture> Cache = new(() => new AcceptanceFixture());

    public static AcceptanceFixture Instance => Cache.Value;
}
