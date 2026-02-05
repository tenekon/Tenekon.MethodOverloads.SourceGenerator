using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tenekon.MethodOverloads.SourceGenerator.Models;
using Tenekon.MethodOverloads.SourceGenerator.Parsing;
using Tenekon.MethodOverloads.SourceGenerator.SourceFormatting;

namespace Tenekon.MethodOverloads.SourceGenerator;

/// <summary>
/// Generates method overloads based on GenerateOverloads attributes.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class MethodOverloadsGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugGuard.MaybeLaunchDebuggerOnStartup();

        context.RegisterPostInitializationOutput(static postContext =>
        {
            // This generates the correct "internal sealed partial class" definition of EmbeddedAttribute
            // as expected by the Roslyn compiler.
            // See: https://github.com/dotnet/roslyn/issues/76584
            postContext.AddEmbeddedAttributeDefinition();

            postContext.AddSource(
                "GenerateOverloadsAttribute.g.cs",
                GeneratorAttributesSource.GenerateOverloadsAttribute);

            postContext.AddSource(
                "GenerateMethodOverloadsAttribute.g.cs",
                GeneratorAttributesSource.GenerateMethodOverloadsAttribute);

            postContext.AddSource(
                "OverloadGenerationOptionsAttribute.g.cs",
                GeneratorAttributesSource.OverloadGenerationOptionsAttribute);

            postContext.AddSource("MatcherUsageAttribute.g.cs", GeneratorAttributesSource.MatcherUsageAttribute);
        });

        var attributesOnlyProvider = context.AnalyzerConfigOptionsProvider.Select(static (provider, _) =>
        {
            if (provider.GlobalOptions.TryGetValue(
                    "build_property.TenekonMethodOverloadsSourceGeneratorAttributesOnly",
                    out var raw))
                return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(raw, "1", StringComparison.Ordinal);

            return false;
        });

        var typeTargets = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeNames.GenerateMethodOverloadsAttribute,
                static (node, _) => node is BaseTypeDeclarationSyntax,
                static (attributeContext, cancellationToken) => Parser.CreateTypeTarget(
                    attributeContext,
                    cancellationToken))
            .Where(static input => input.HasValue)
            .Select(static (input, _) => input!.Value);

        var methodTargets = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeNames.GenerateOverloadsAttribute,
                static (node, _) => node is MethodDeclarationSyntax,
                static (attributeContext, cancellationToken) => Parser.CreateMethodTarget(
                    attributeContext,
                    cancellationToken))
            .Where(static input => input.HasValue)
            .Select(static (input, _) => input!.Value);

        var modelProvider = typeTargets.Collect()
            .Combine(methodTargets.Collect())
            .Select(static (pair, cancellationToken) => Parser.Parse(pair.Left, pair.Right, cancellationToken));

        var combined = modelProvider.Combine(attributesOnlyProvider);

        context.RegisterSourceOutput(
            combined,
            static (productionContext, pair) =>
            {
                var model = pair.Left;
                if (pair.Right || model is null) return;

                SourceFormatter.GenerateSourceFiles(productionContext, model);
            });
    }
}