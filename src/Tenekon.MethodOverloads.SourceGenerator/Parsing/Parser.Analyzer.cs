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

        var attribute = RoslynHelpers.GetAttribute(typeSymbol, "GenerateMethodOverloadsAttribute");
        if (attribute is null) return null;

        var typeModel = BuildTypeModel(typeSymbol, cancellationToken);
        var (matcherDisplays, matcherModels) = ExtractMatcherTypes(attribute, cancellationToken);
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

        var attribute = RoslynHelpers.GetAttribute(methodSymbol, "GenerateOverloadsAttribute");
        if (attribute is null) return null;

        var methodModel = BuildMethodModel(
            methodSymbol,
            cancellationToken,
            out var syntaxOptions,
            out var optionsFromAttribute);
        var typeModel = BuildTypeModel(methodSymbol.ContainingType, cancellationToken);
        var (attributeArgs, syntaxArgs) = ExtractGenerateOverloadsArgs(methodSymbol, cancellationToken);
        var (matcherDisplays, matcherModels) = ExtractMatcherTypes(attribute, cancellationToken);

        return new MethodTargetInput(
            methodModel,
            typeModel,
            attributeArgs,
            syntaxArgs,
            new EquatableArray<string>(matcherDisplays),
            methodModel.Options,
            syntaxOptions.HasAny ? syntaxOptions : null,
            optionsFromAttribute,
            new EquatableArray<MatcherTypeModel>(matcherModels));
    }
}