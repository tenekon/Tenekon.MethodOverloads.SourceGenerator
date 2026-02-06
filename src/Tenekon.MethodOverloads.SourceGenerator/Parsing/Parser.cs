using System.Collections.Immutable;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;
using Tenekon.MethodOverloads.SourceGenerator.Parsing.Inputs;

namespace Tenekon.MethodOverloads.SourceGenerator.Parsing;

internal static partial class Parser
{
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
        var diagnostics = new List<EquatableDiagnostic>();

        foreach (var input in typeTargets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AddType(typesByDisplay, input.Type);

            if (!typeTargetsByDisplay.TryGetValue(input.Type.DisplayName, out var existingTarget))
            {
                typeTargetsByDisplay[input.Type.DisplayName] = new TypeTargetModel(
                    input.Type,
                    HasGenerateMethodOverloads: true,
                    input.MatcherTypeDisplays);
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

            AddType(typesByDisplay, input.ContainingType);
            AddType(typesByDisplay, input.Method.ContainingTypeDisplay, input.ContainingType);

            var key = MethodIdentity.BuildMethodIdentityKey(input.Method);
            if (!methodTargetsByKey.TryGetValue(key, out var existingTarget))
                methodTargetsByKey[key] = new MethodTargetModel(
                    input.Method,
                    HasGenerateOverloads: true,
                    input.GenerateAttributesFromAttribute,
                    input.GenerateAttributesFromSyntax,
                    input.MatcherTypeDisplays);
            else
                methodTargetsByKey[key] = existingTarget with
                {
                    GenerateAttributesFromAttribute = MergeAttributes(
                        existingTarget.GenerateAttributesFromAttribute,
                        input.GenerateAttributesFromAttribute),
                    GenerateAttributesFromSyntax = MergeAttributes(
                        existingTarget.GenerateAttributesFromSyntax,
                        input.GenerateAttributesFromSyntax),
                    MatcherTypeDisplays = MergeDisplays(existingTarget.MatcherTypeDisplays, input.MatcherTypeDisplays)
                };

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
            ..methodTargetsByKey.Values.OrderBy(
                t => MethodIdentity.BuildMethodIdentityKey(t.Method),
                StringComparer.Ordinal)
        ]);
        var matcherTypesArray = new EquatableArray<MatcherTypeModel>(
        [
            ..matcherTypesByDisplay.Values.OrderBy(t => t.Type.DisplayName, StringComparer.Ordinal)
        ]);
        return new GeneratorModel(
            types,
            typeTargetsArray,
            methodTargetsArray,
            matcherTypesArray,
            new EquatableArray<EquatableDiagnostic>([.. diagnostics]));
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

        return new EquatableArray<string>([.. set.OrderBy(x => x, StringComparer.Ordinal)]);
    }

    private static EquatableArray<GenerateOverloadsAttributeModel> MergeAttributes(
        EquatableArray<GenerateOverloadsAttributeModel> left,
        EquatableArray<GenerateOverloadsAttributeModel> right)
    {
        if (left.Items.Length == 0) return right;

        if (right.Items.Length == 0) return left;

        var set = new HashSet<GenerateOverloadsAttributeModel>();
        var merged = new List<GenerateOverloadsAttributeModel>();

        foreach (var entry in left.Items)
            if (set.Add(entry))
                merged.Add(entry);

        foreach (var entry in right.Items)
            if (set.Add(entry))
                merged.Add(entry);

        return new EquatableArray<GenerateOverloadsAttributeModel>([.. merged]);
    }
}