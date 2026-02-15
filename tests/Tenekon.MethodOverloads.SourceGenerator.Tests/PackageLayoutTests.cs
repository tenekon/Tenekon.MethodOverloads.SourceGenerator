namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

using Infrastructure;

public sealed class PackageLayoutTests(PackageLayoutFixture fixture) : IClassFixture<PackageLayoutFixture>
{
    [Theory]
    [InlineData("analyzers/dotnet/cs/Tenekon.MethodOverloads.SourceGenerator.dll")]
    [InlineData("analyzers/dotnet/cs/Tenekon.MethodOverloads.Attributes.dll")]
    [InlineData("build/Tenekon.MethodOverloads.SourceGenerator.props")]
    [InlineData("buildTransitive/Tenekon.MethodOverloads.SourceGenerator.props")]
    [InlineData("Tenekon.MethodOverloads.SourceGenerator.Common.props")]
    [InlineData("lib/netstandard2.0/_._")]
    public void Package_Contains_Expected_Entries(string entry)
    {
        Assert.Contains(entry, fixture.Entries);
    }

    [Fact]
    public void Package_DoesNotPlaceAnalyzerUnderNetstandard()
    {
        Assert.DoesNotContain(
            fixture.Entries,
            e => e.StartsWith("analyzers/dotnet/cs/netstandard", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Package_UsesLibPlaceholderOnly()
    {
        var libEntries = fixture.Entries.Where(e => e.StartsWith("lib/", StringComparison.OrdinalIgnoreCase)).ToArray();
        Assert.Equal(new[] { "lib/netstandard2.0/_._" }, libEntries);
    }
}