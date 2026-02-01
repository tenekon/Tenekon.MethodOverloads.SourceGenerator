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
        });

        var compilationProvider = context.CompilationProvider;

        context.RegisterSourceOutput(compilationProvider, static (productionContext, compilation) =>
        {
            var generator = new MethodOverloadsGeneratorCore(compilation, productionContext);
            generator.Execute();
        });
    }
}
