using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Model;

namespace Tenekon.MethodOverloads.SourceGenerator.Parsing;

internal static partial class Parser
{
    public static TypeTargetInput? CreateTypeTarget(
        GeneratorAttributeSyntaxContext attributeContext,
        CancellationToken cancellationToken)
    {
        if (attributeContext.TargetSymbol is not INamedTypeSymbol typeSymbol) return null;

        cancellationToken.ThrowIfCancellationRequested();

        var typeModel = BuildTypeModel(typeSymbol, cancellationToken);
        var sourceFile = CreateSourceFileModel(attributeContext.TargetNode.SyntaxTree, cancellationToken);

        var attribute = RoslynHelpers.GetAttribute(typeSymbol, "GenerateMethodOverloadsAttribute");
        var (matcherDisplays, matcherModels) = ExtractMatcherTypes(attribute, cancellationToken);

        return new TypeTargetInput(
            typeModel,
            new EquatableArray<string>(matcherDisplays),
            new EquatableArray<MatcherTypeModel>(matcherModels),
            sourceFile);
    }

    public static MethodTargetInput? CreateMethodTarget(
        GeneratorAttributeSyntaxContext attributeContext,
        CancellationToken cancellationToken)
    {
        if (attributeContext.TargetSymbol is not IMethodSymbol methodSymbol) return null;

        cancellationToken.ThrowIfCancellationRequested();

        var methodModel = BuildMethodModel(
            methodSymbol,
            cancellationToken,
            out var syntaxOptions,
            out var optionsFromAttribute);
        var typeModel = BuildTypeModel(methodSymbol.ContainingType, cancellationToken);
        var sourceFile = CreateSourceFileModel(attributeContext.TargetNode.SyntaxTree, cancellationToken);

        var (attributeArgs, syntaxArgs) = ExtractGenerateOverloadsArgs(methodSymbol, cancellationToken);
        var attribute = RoslynHelpers.GetAttribute(methodSymbol, "GenerateOverloadsAttribute");
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

    public static GeneratorModel? Parse(
        ImmutableArray<TypeTargetInput> typeTargets,
        ImmutableArray<MethodTargetInput> methodTargets,
        CancellationToken cancellationToken)
    {
        if (typeTargets.IsDefaultOrEmpty && methodTargets.IsDefaultOrEmpty) return null;

        var typesByDisplay = new Dictionary<string, TypeModel>(StringComparer.Ordinal);
        var typeTargetsByDisplay = new Dictionary<string, TypeTargetModel>(StringComparer.Ordinal);
        var methodTargetsByKey = new Dictionary<string, MethodTargetModel>(StringComparer.Ordinal);
        var matcherTypesByDisplay = new Dictionary<string, MatcherTypeModel>(StringComparer.Ordinal);
        var sourceFilesByPath = new Dictionary<string, SourceFileModel>(StringComparer.Ordinal);
        var diagnostics = new List<EquatableDiagnostic>();

        foreach (var input in typeTargets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AddSourceFile(sourceFilesByPath, input.SourceFile);
            AddType(typesByDisplay, input.Type);

            if (!typeTargetsByDisplay.TryGetValue(input.Type.DisplayName, out var existingTarget))
            {
                typeTargetsByDisplay[input.Type.DisplayName] = new TypeTargetModel(
                    input.Type,
                    HasGenerateMethodOverloads: true,
                    input.MatcherTypeDisplays,
                    input.Type.Options);
            }
            else
            {
                var mergedMatchers = MergeDisplays(existingTarget.MatcherTypeDisplays, input.MatcherTypeDisplays);
                typeTargetsByDisplay[input.Type.DisplayName] = existingTarget with
                {
                    MatcherTypeDisplays = mergedMatchers
                };
            }

            MergeMatcherTypes(matcherTypesByDisplay, input.MatcherTypes, typesByDisplay);
        }

        foreach (var input in methodTargets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AddSourceFile(sourceFilesByPath, input.SourceFile);
            AddType(typesByDisplay, input.ContainingType);
            AddType(typesByDisplay, input.Method.ContainingTypeDisplay, input.ContainingType);

            var key = BuildMethodIdentityKey(input.Method);
            if (!methodTargetsByKey.ContainsKey(key))
                methodTargetsByKey[key] = new MethodTargetModel(
                    input.Method,
                    HasGenerateOverloads: true,
                    input.GenerateArgsFromAttribute,
                    input.GenerateArgsFromSyntax,
                    input.MatcherTypeDisplays,
                    input.OptionsFromAttributeOrSyntax,
                    input.SyntaxOptions,
                    input.OptionsFromAttribute);

            MergeMatcherTypes(matcherTypesByDisplay, input.MatcherTypes, typesByDisplay);
        }

        var types = new EquatableArray<TypeModel>(
        [
            ..typesByDisplay.Values.OrderBy(t => t.DisplayName, StringComparer.Ordinal)
        ]);
        var typeTargetsArray = new EquatableArray<TypeTargetModel>(
        [
            ..typeTargetsByDisplay.Values.OrderBy(t => t.Type.DisplayName, StringComparer.Ordinal)
        ]);
        var methodTargetsArray = new EquatableArray<MethodTargetModel>(
        [
            ..methodTargetsByKey.Values.OrderBy(t => BuildMethodIdentityKey(t.Method), StringComparer.Ordinal)
        ]);
        var matcherTypesArray = new EquatableArray<MatcherTypeModel>(
        [
            ..matcherTypesByDisplay.Values.OrderBy(t => t.Type.DisplayName, StringComparer.Ordinal)
        ]);
        var sourceFiles = new EquatableArray<SourceFileModel>(
        [
            ..sourceFilesByPath.Values.OrderBy(f => f.Path, StringComparer.Ordinal)
        ]);

        return new GeneratorModel(
            types,
            typeTargetsArray,
            methodTargetsArray,
            matcherTypesArray,
            sourceFiles,
            new EquatableArray<EquatableDiagnostic>([..diagnostics]));
    }

    private static void AddSourceFile(Dictionary<string, SourceFileModel> sourceFilesByPath, SourceFileModel sourceFile)
    {
        if (!string.IsNullOrEmpty(sourceFile.Path) && !sourceFilesByPath.ContainsKey(sourceFile.Path))
            sourceFilesByPath[sourceFile.Path] = sourceFile;
    }

    private static void AddType(Dictionary<string, TypeModel> typesByDisplay, TypeModel type)
    {
        if (!typesByDisplay.ContainsKey(type.DisplayName)) typesByDisplay[type.DisplayName] = type;
    }

    private static void AddType(Dictionary<string, TypeModel> typesByDisplay, string displayName, TypeModel type)
    {
        if (!typesByDisplay.ContainsKey(displayName)) typesByDisplay[displayName] = type;
    }

    private static void MergeMatcherTypes(
        Dictionary<string, MatcherTypeModel> matcherTypesByDisplay,
        EquatableArray<MatcherTypeModel> matcherTypes,
        Dictionary<string, TypeModel> typesByDisplay)
    {
        foreach (var matcher in matcherTypes.Items)
        {
            if (!matcherTypesByDisplay.ContainsKey(matcher.Type.DisplayName))
                matcherTypesByDisplay[matcher.Type.DisplayName] = matcher;

            AddType(typesByDisplay, matcher.Type);
        }
    }

    private static EquatableArray<string> MergeDisplays(EquatableArray<string> left, EquatableArray<string> right)
    {
        if (left.Items.Length == 0) return right;

        if (right.Items.Length == 0) return left;

        var set = new HashSet<string>(left.Items, StringComparer.Ordinal);
        foreach (var entry in right.Items) set.Add(entry);

        return new EquatableArray<string>([..set.OrderBy(x => x, StringComparer.Ordinal)]);
    }
}