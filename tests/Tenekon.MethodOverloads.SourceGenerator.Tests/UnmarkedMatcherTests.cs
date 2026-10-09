using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class UnmarkedMatcherTests
{
    [Fact]
    public void Unmarked_matcher_type_is_reported_at_every_typeof_reference()
    {
        var fixture = AcceptanceFixtureCache.Instance;

        // Class_46 lists the unmarked Class_46_Service twice: on the type and on Case_2.
        var diagnostics = fixture.Diagnostics.Where(diagnostic => diagnostic.Id == "MOG020").ToArray();

        Assert.Equal(2, diagnostics.Length);
        Assert.All(
            diagnostics,
            diagnostic => Assert.Equal("typeof(Class_46_Service)", GetLocationText(diagnostic.Location)));
        Assert.NotEqual(diagnostics[0].Location.SourceSpan, diagnostics[1].Location.SourceSpan);
    }

    private static string GetLocationText(Location location)
    {
        return location.SourceTree!.GetText().ToString(location.SourceSpan);
    }
}
