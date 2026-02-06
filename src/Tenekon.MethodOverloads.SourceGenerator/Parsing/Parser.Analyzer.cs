using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Parsing;

internal static partial class Parser
{
    public static TypeTargetInput? CreateTypeTargetFromSymbol(
        INamedTypeSymbol typeSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var attributes = RoslynHelpers.GetAttributes(typeSymbol, "GenerateMethodOverloadsAttribute");
        if (attributes.IsDefaultOrEmpty) return null;

        var typeModel = BuildTypeModel(typeSymbol, cancellationToken);
        var (matcherDisplays, matcherModels) = ExtractMatcherTypes(attributes, cancellationToken);
        return new TypeTargetInput(
            typeModel,
            new EquatableArray<string>(matcherDisplays),
            new EquatableArray<MatcherTypeModel>(matcherModels));
    }

    public static MethodTargetInput? CreateMethodTargetFromSymbol(
        IMethodSymbol methodSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var attributes = RoslynHelpers.GetAttributes(methodSymbol, "GenerateOverloadsAttribute");
        if (attributes.IsDefaultOrEmpty) return null;

        var methodModel = BuildMethodModel(
            methodSymbol,
            cancellationToken,
            out var syntaxOptions,
            out var optionsFromAttribute);
        var typeModel = BuildTypeModel(methodSymbol.ContainingType, cancellationToken);
        var (attributeModels, syntaxModels) = ExtractGenerateOverloadsAttributes(methodSymbol, cancellationToken);
        var (matcherDisplays, matcherModels) = ExtractMatcherTypes(attributes, cancellationToken);

        return new MethodTargetInput(
            methodModel,
            typeModel,
            new EquatableArray<GenerateOverloadsAttributeModel>(attributeModels),
            new EquatableArray<GenerateOverloadsAttributeModel>(syntaxModels),
            new EquatableArray<string>(matcherDisplays),
            methodModel.Options,
            syntaxOptions.HasAny ? syntaxOptions : null,
            optionsFromAttribute,
            new EquatableArray<MatcherTypeModel>(matcherModels));
    }
}
