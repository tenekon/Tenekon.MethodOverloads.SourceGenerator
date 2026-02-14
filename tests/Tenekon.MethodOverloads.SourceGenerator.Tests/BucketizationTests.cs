using Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class BucketizationTests
{
    [Fact]
    public void Bucket_type_is_used_for_overload_output()
    {
        var fixture = AcceptanceFixtureCache.Instance;
        var bucketTree = fixture.GeneratedTrees.FirstOrDefault(tree => tree.ToString()
            .Contains("static partial class Class_43_Bucket", StringComparison.Ordinal));

        Assert.NotNull(bucketTree);

        var text = bucketTree!.ToString();
        Assert.Contains("namespace Tenekon.MethodOverloads.AcceptanceCriterias;", text, StringComparison.Ordinal);
        Assert.Contains("static partial class Class_43_Bucket", text, StringComparison.Ordinal);
        Assert.Contains("Case_1", text, StringComparison.Ordinal);
        Assert.DoesNotContain("public static class MethodOverloads", text, StringComparison.Ordinal);
    }
}