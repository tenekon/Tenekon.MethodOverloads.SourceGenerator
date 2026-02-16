using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        context.RegisterPostInitializationOutput(static postContext =>
        {
            // Use once we do not embed attributes ourselves
            // // This generates the correct "internal sealed partial class" definition of EmbeddedAttribute
            // // as expected by the Roslyn compiler.
            // // See: https://github.com/dotnet/roslyn/issues/76584
            // postContext.AddEmbeddedAttributeDefinition();
            
            postContext.AddSource(
                "EmbeddedAttribute.g.cs",
                LoadAttributeSource(GeneratorAttributesSource.EmbeddedAttribute));

            postContext.AddSource(
                "GenerateOverloadsAttribute.g.cs",
                LoadAttributeSource(GeneratorAttributesSource.GenerateOverloadsAttribute));

            postContext.AddSource(
                "GenerateMethodOverloadsAttribute.g.cs",
                LoadAttributeSource(GeneratorAttributesSource.GenerateMethodOverloadsAttribute));

            postContext.AddSource(
                "OverloadGenerationOptionsAttribute.g.cs",
                LoadAttributeSource(GeneratorAttributesSource.OverloadGenerationOptionsAttribute));

            postContext.AddSource(
                "SupplyParameterTypeAttribute.g.cs",
                LoadAttributeSource(GeneratorAttributesSource.SupplyParameterTypeAttribute));

            postContext.AddSource(
                "MatcherUsageAttribute.g.cs",
                LoadAttributeSource(GeneratorAttributesSource.MatcherUsageAttribute));
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
                static (attributeContext, cancellationToken) => TargetFactory.CreateTypeTarget(
                    attributeContext,
                    cancellationToken))
            .Where(static input => input.HasValue)
            .Select(static (input, _) => input!.Value);

        var methodTargets = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeNames.GenerateOverloadsAttribute,
                static (node, _) => node is MethodDeclarationSyntax,
                static (attributeContext, cancellationToken) => TargetFactory.CreateMethodTarget(
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

    private static string LoadAttributeSource(string resourceName)
    {
        var assembly = typeof(AssemblyMarker).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Missing embedded attribute resource: " + resourceName);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }
}
