namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class MatcherUsageStubTests
{
    [Fact]
    public void Matcher_usage_stub_is_emitted_when_no_overloads_generated()
    {
        var fixture = AcceptanceFixtureCache.Instance;
        var stubTree = fixture.GeneratedTrees.FirstOrDefault(tree =>
            tree.FilePath.Contains("_MatcherUsage.g.cs", StringComparison.OrdinalIgnoreCase) &&
            tree.ToString().Contains("MatcherUsageAttribute(nameof(Match))", StringComparison.Ordinal));

        Assert.NotNull(stubTree);

        var text = stubTree!.ToString();
        Assert.Contains("public static class MethodOverloads", text, StringComparison.Ordinal);
        Assert.Contains("MatcherUsageAttribute", text, StringComparison.Ordinal);
        Assert.Contains("nameof(Match)", text, StringComparison.Ordinal);
        Assert.DoesNotContain("static void", text, StringComparison.Ordinal);
    }
}
