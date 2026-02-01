using PublicApiGenerator;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public class PublicApiTests
{
    [Fact]
    public Task PublicApi_HasNoChanges()
    {
        var publicApi = typeof(MethodOverloadsGenerator).Assembly.GeneratePublicApi(
            new ApiGeneratorOptions
            {
                ExcludeAttributes =
                [
                    "System.Runtime.CompilerServices.InternalsVisibleToAttribute",
                    "System.Reflection.AssemblyMetadataAttribute"
                ]
            });

        return Verify(publicApi);
    }
}