using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;
using Tenekon.MethodOverloads.SourceGenerator.Parsing.Inputs;

namespace Tenekon.MethodOverloads.SourceGenerator.Parsing;

internal static class TargetFactory
{
    public static TypeTargetInput? CreateTypeTarget(
        GeneratorAttributeSyntaxContext attributeContext,
        CancellationToken cancellationToken)
    {
        if (attributeContext.TargetSymbol is not INamedTypeSymbol typeSymbol) return null;

        return CreateTypeTargetFromSymbol(typeSymbol, cancellationToken);
    }

    public static MethodTargetInput? CreateMethodTarget(
        GeneratorAttributeSyntaxContext attributeContext,
        CancellationToken cancellationToken)
    {
        if (attributeContext.TargetSymbol is not IMethodSymbol methodSymbol) return null;

        return CreateMethodTargetFromSymbol(methodSymbol, cancellationToken);
    }

    public static TypeTargetInput? CreateTypeTargetFromSymbol(
        INamedTypeSymbol typeSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var attributes = RoslynHelpers.GetAttributes(typeSymbol, "GenerateMethodOverloadsAttribute");
        if (attributes.IsDefaultOrEmpty) return null;

        var typeModel = Parser.BuildTypeModel(typeSymbol, cancellationToken);
        var (matcherDisplays, matcherModels) = Parser.ExtractMatcherTypes(attributes, cancellationToken);
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

        var methodModel = Parser.BuildMethodModel(methodSymbol, cancellationToken);
        var typeModel = Parser.BuildTypeModel(methodSymbol.ContainingType, cancellationToken);
        var (attributeModels, syntaxModels) = Parser.ExtractGenerateOverloadsAttributes(
            methodSymbol,
            cancellationToken);
        var (matcherDisplays, matcherModels) = Parser.ExtractMatcherTypes(attributes, cancellationToken);

        return new MethodTargetInput(
            methodModel,
            typeModel,
            new EquatableArray<GenerateOverloadsAttributeModel>(attributeModels),
            new EquatableArray<GenerateOverloadsAttributeModel>(syntaxModels),
            new EquatableArray<string>(matcherDisplays),
            new EquatableArray<MatcherTypeModel>(matcherModels));
    }
}