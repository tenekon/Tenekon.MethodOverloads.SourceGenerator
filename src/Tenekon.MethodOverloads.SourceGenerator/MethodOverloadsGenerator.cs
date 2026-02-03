using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class MethodOverloadsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postContext =>
        {
            postContext.AddSource("GenerateOverloadsAttribute.g.cs", GeneratorAttributesSource.GenerateOverloadsAttribute);
            postContext.AddSource("GenerateMethodOverloadsAttribute.g.cs", GeneratorAttributesSource.GenerateMethodOverloadsAttribute);
            postContext.AddSource("OverloadGenerationOptionsAttribute.g.cs", GeneratorAttributesSource.OverloadGenerationOptionsAttribute);
            postContext.AddSource("MatcherUsageAttribute.g.cs", GeneratorAttributesSource.MatcherUsageAttribute);
        });

        var attributesOnlyProvider = context.AnalyzerConfigOptionsProvider
            .Select((provider, _) =>
            {
                if (provider.GlobalOptions.TryGetValue(
                        "build_property.TenekonMethodOverloadsSourceGeneratorAttributesOnly",
                        out var raw))
                {
                    return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(raw, "1", StringComparison.Ordinal);
                }

                return false;
            });

        var compilationProvider = context.CompilationProvider;

        var combined = compilationProvider.Combine(attributesOnlyProvider);

        context.RegisterSourceOutput(combined, static (productionContext, pair) =>
        {
            var (compilation, attributesOnly) = pair;
            if (attributesOnly)
            {
                return;
            }

            var generator = new MethodOverloadsGeneratorCore(compilation, productionContext);
            generator.Execute();
        });
    }
}
