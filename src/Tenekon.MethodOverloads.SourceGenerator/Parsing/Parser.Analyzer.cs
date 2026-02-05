using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Model;

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
        var sourceFile = CreateSourceFileModelFromSymbol(typeSymbol, attribute, cancellationToken);

        return new TypeTargetInput(
            typeModel,
            new EquatableArray<string>(matcherDisplays),
            new EquatableArray<MatcherTypeModel>(matcherModels),
            sourceFile);
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
        var sourceFile = CreateSourceFileModelFromSymbol(methodSymbol, attribute, cancellationToken);

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
            new EquatableArray<MatcherTypeModel>(matcherModels),
            sourceFile);
    }

    private static SourceFileModel CreateSourceFileModelFromSymbol(
        ISymbol symbol,
        AttributeData? attribute,
        CancellationToken cancellationToken)
    {
        var tree = attribute?.ApplicationSyntaxReference?.SyntaxTree;
        if (tree is null)
        {
            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            tree = syntaxRef?.SyntaxTree;
        }

        if (tree is null)
        {
            var location = symbol.Locations.FirstOrDefault(loc => loc.SourceTree is not null);
            tree = location?.SourceTree;
        }

        if (tree is null) return new SourceFileModel(string.Empty, string.Empty, LanguageVersion.Default);

        return CreateSourceFileModel(tree, cancellationToken);
    }
}