using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Generation;

internal sealed class OverloadPlanBuilder
{
    private const string DefaultBucketName = "MethodOverloads";
    private readonly GeneratorModel _model;
    private readonly Dictionary<string, TypeModel> _typesByDisplay;
    private readonly Dictionary<string, TypeTargetModel> _typeTargetsByDisplay;
    private readonly Dictionary<string, MatcherTypeModel> _matcherTypesByDisplay;
    private readonly HashSet<string> _matcherTypeDisplays;
    private readonly Dictionary<OverloadGroupKey, List<OverloadPlanEntry>> _methodsByGroup;
    private readonly Dictionary<OverloadGroupKey, MatcherGroupInfo> _matchedMatchersByGroup;
    private readonly List<EquatableDiagnostic> _diagnostics;

    public OverloadPlanBuilder(GeneratorModel model)
    {
        _model = model;
        _typesByDisplay = model.Types.Items.ToDictionary(
            type => type.DisplayName,
            type => type,
            StringComparer.Ordinal);
        _typeTargetsByDisplay = model.TypeTargets.Items.ToDictionary(
            target => target.Type.DisplayName,
            target => target,
            StringComparer.Ordinal);
        _matcherTypesByDisplay = model.MatcherTypes.Items.ToDictionary(
            target => target.Type.DisplayName,
            target => target,
            StringComparer.Ordinal);
        _matcherTypeDisplays = new HashSet<string>(_matcherTypesByDisplay.Keys, StringComparer.Ordinal);
        _methodsByGroup = new Dictionary<OverloadGroupKey, List<OverloadPlanEntry>>();
        _matchedMatchersByGroup = new Dictionary<OverloadGroupKey, MatcherGroupInfo>();
        _diagnostics = [];
    }

    public OverloadPlan Build()
    {
        BuildMethods();
        return new OverloadPlan(
            _methodsByGroup,
            _matchedMatchersByGroup,
            new EquatableArray<EquatableDiagnostic>([.. _diagnostics]));
    }

    private void Report(DiagnosticDescriptor descriptor, SourceLocationModel? location, params string[] messageArgs)
    {
        _diagnostics.Add(new EquatableDiagnostic(descriptor, location, new EquatableArray<string>([.. messageArgs])));
    }

    private void BuildMethods()
    {
        var matcherHasAnyMatch = new Dictionary<MatcherMethodReference, bool>();
        var matcherLocations = new Dictionary<MatcherMethodReference, SourceLocationModel?>();
        var matchedMatchersByTarget = new Dictionary<string, HashSet<MatcherMethodReference>>(StringComparer.Ordinal);

        var methodTargetsByKey = _model.MethodTargets.Items.ToDictionary(
            target => MethodIdentity.BuildMethodIdentityKey(target.Method),
            target => target,
            StringComparer.Ordinal);

        var candidateMethods = new Dictionary<string, MethodModel>(StringComparer.Ordinal);
        foreach (var typeTarget in _model.TypeTargets.Items)
        foreach (var method in typeTarget.Type.Methods.Items)
        {
            var key = MethodIdentity.BuildMethodIdentityKey(method);
            if (!candidateMethods.ContainsKey(key)) candidateMethods[key] = method;
        }

        foreach (var methodTarget in _model.MethodTargets.Items)
        {
            var key = MethodIdentity.BuildMethodIdentityKey(methodTarget.Method);
            if (!candidateMethods.ContainsKey(key)) candidateMethods[key] = methodTarget.Method;
        }

        foreach (var method in candidateMethods.Values)
        {
            if (!method.IsOrdinary) continue;

            if (_matcherTypeDisplays.Contains(method.ContainingTypeDisplay)) continue;

            if (method.DeclaredAccessibility == Accessibility.Private
                || method.DeclaredAccessibility == Accessibility.Protected)
                continue;

            _typeTargetsByDisplay.TryGetValue(method.ContainingTypeDisplay, out var typeTarget);
            methodTargetsByKey.TryGetValue(MethodIdentity.BuildMethodIdentityKey(method), out var methodTarget);
            TypeModel? containingType = null;
            if (_typesByDisplay.TryGetValue(method.ContainingTypeDisplay, out var typeModel))
                containingType = typeModel;

            var hasTypeGenerate = typeTarget.HasGenerateMethodOverloads;
            var hasMethodGenerate = methodTarget.HasGenerateOverloads;

            if (!hasTypeGenerate && !hasMethodGenerate) continue;

            var effectiveMethod = ApplySuppliedParameterTypes(method);

            var windowSpecs = new List<WindowSpec>();
            var optionsByGroupKey = new Dictionary<string, GenerationOptions>(StringComparer.Ordinal);
            var methodMatchers = EquatableArray<string>.Empty;
            var methodAttributes = EquatableArray<GenerateOverloadsAttributeModel>.Empty;
            var directOptions = GenerationOptionsResolver.ResolveGenerationOptions(
                effectiveMethod,
                containingType,
                matcherMethod: null,
                matcherType: null);

            if (directOptions.BucketType is { IsValid: false } invalidBucket)
            {
                ReportInvalidBucketType(invalidBucket, method.IdentifierLocation);
                continue;
            }

            if (hasMethodGenerate)
            {
                methodMatchers = methodTarget.MatcherTypeDisplays;
                methodAttributes = SelectGenerateAttributes(
                    methodTarget.GenerateAttributesFromAttribute,
                    methodTarget.GenerateAttributesFromSyntax);
            }

            if (hasMethodGenerate && effectiveMethod.Parameters.Items.Length == 0)
            {
                var location = GetAttributeLocation(methodAttributes) ?? method.IdentifierLocation;
                Report(GeneratorDiagnostics.ParameterlessTargetMethod, location, effectiveMethod.Name);
                continue;
            }

            if (hasMethodGenerate && HasMatchersWindowConflict(effectiveMethod, methodAttributes))
                continue;

            var useMethodMatchers = methodMatchers.Items.Length > 0;
            var directAttributes = methodAttributes.Items.Where(attr => !attr.HasMatchers).ToArray();
            var useDirectGenerateOverloads = hasMethodGenerate && directAttributes.Length > 0;
            var useTypeMatchers = !hasMethodGenerate && hasTypeGenerate;

            if (useDirectGenerateOverloads)
                for (var index = 0; index < directAttributes.Length; index++)
                {
                    var attribute = directAttributes[index];
                    var groupKey = "direct:" + BuildMethodGroupKey(effectiveMethod) + "#" + index;

                    if (TryCreateWindowSpecFromArgs(
                            effectiveMethod,
                            effectiveMethod,
                            attribute.Args,
                            ParameterMatch.Identity(effectiveMethod.Parameters.Items.Length),
                            out var windowSpecFromAttribute,
                            out var windowFailureFromAttribute))
                    {
                        windowSpecs.Add(
                            new WindowSpec(
                                windowSpecFromAttribute.StartIndex,
                                windowSpecFromAttribute.EndIndex,
                                groupKey));
                        optionsByGroupKey[groupKey] = directOptions;

                        if (windowFailureFromAttribute.Kind == WindowSpecFailureKind.RedundantAnchors)
                            Report(
                                GeneratorDiagnostics.RedundantBeginEndAnchors,
                                attribute.Args.AttributeLocation ?? attribute.Args.SyntaxAttributeLocation
                                ?? method.IdentifierLocation,
                                method.Name);
                    }
                    else
                    {
                        ReportWindowFailure(windowFailureFromAttribute, attribute.Args, method.Name);
                    }
                }
            if (useMethodMatchers || useTypeMatchers)
            {
                var matcherTypes = useMethodMatchers ? methodMatchers : typeTarget.MatcherTypeDisplays;
                foreach (var matcherTypeDisplay in matcherTypes.Items)
                {
                    if (!_matcherTypesByDisplay.TryGetValue(matcherTypeDisplay, out var matcherType)) continue;

                    foreach (var matcherMethod in SelectMatcherMethods(
                                 matcherType.MatcherMethods,
                                 method.Parameters.Items.Length))
                    {
                        var matcherMethodModel = matcherMethod.Method;
                        if (!matcherMethodModel.IsOrdinary) continue;

                        var matcherAttributes = SelectGenerateAttributes(
                            matcherMethod.GenerateAttributesFromAttribute,
                            matcherMethod.GenerateAttributesFromSyntax);

                        if (matcherMethodModel.Parameters.Items.Length == 0)
                        {
                            var location = GetAttributeLocation(matcherAttributes)
                                ?? matcherMethodModel.IdentifierLocation;
                            Report(GeneratorDiagnostics.ParameterlessTargetMethod, location, matcherMethodModel.Name);
                            continue;
                        }

                        if (HasMatchersWindowConflict(matcherMethodModel, matcherAttributes))
                            continue;

                        var matcherDirectAttributes = matcherAttributes.Items.Where(attribute => !attribute.HasMatchers)
                            .ToArray();
                        if (matcherDirectAttributes.Length == 0) continue;

                        var matcherRef = new MatcherMethodReference(
                            matcherMethodModel.ContainingTypeDisplay,
                            matcherType.Type.DeclaredAccessibility,
                            matcherMethodModel.Name,
                            matcherMethodModel.Parameters.Items.Length,
                            matcherMethodModel.ContainingNamespace);

                        if (!matcherHasAnyMatch.ContainsKey(matcherRef))
                        {
                            matcherHasAnyMatch[matcherRef] = false;
                            matcherLocations[matcherRef] = matcherMethodModel.IdentifierLocation;
                        }

                        var effectiveMatcherMethod = ApplySuppliedParameterTypes(matcherMethodModel);
                        var groupKey = "matcher:" + BuildMethodGroupKey(effectiveMatcherMethod);
                        var matcherOptions = GenerationOptionsResolver.ResolveGenerationOptions(
                            effectiveMethod,
                            containingType,
                            matcherMethodModel,
                            matcherType);
                        if (matcherOptions.BucketType is { IsValid: false } invalidMatcherBucket)
                        {
                            ReportInvalidBucketType(invalidMatcherBucket, matcherMethodModel.IdentifierLocation);
                            continue;
                        }
                        var matches = FindSubsequenceMatches(
                                effectiveMatcherMethod,
                                effectiveMethod,
                                matcherOptions.RangeAnchorMatchMode)
                            .ToArray();
                        if (matches.Length == 0) continue;

                        matcherHasAnyMatch[matcherRef] = true;
                        optionsByGroupKey[groupKey] = matcherOptions;

                        var targetKey = MethodIdentity.BuildMethodIdentityKey(effectiveMethod);
                        if (!matchedMatchersByTarget.TryGetValue(targetKey, out var matchedMatchers))
                        {
                            matchedMatchers = [];
                            matchedMatchersByTarget[targetKey] = matchedMatchers;
                        }

                        matchedMatchers.Add(matcherRef);

                        var matcherNamespace = matcherOptions.BucketType?.Namespace ?? effectiveMethod.ContainingNamespace;
                        var matcherGroupKey = BuildGroupKey(matcherNamespace, matcherOptions.BucketType);
                        if (!_matchedMatchersByGroup.TryGetValue(matcherGroupKey, out var matcherGroup))
                        {
                            matcherGroup = new MatcherGroupInfo(matcherOptions.BucketType);
                            _matchedMatchersByGroup[matcherGroupKey] = matcherGroup;
                        }

                        matcherGroup.MatchedMatchers.Add(matcherRef);

                        foreach (var match in matches)
                        foreach (var attribute in matcherDirectAttributes)
                            if (TryCreateWindowSpecFromArgs(
                                    effectiveMethod,
                                    effectiveMatcherMethod,
                                    attribute.Args,
                                    match,
                                    out var windowSpec,
                                    out var windowFailure))
                            {
                                windowSpecs.Add(new WindowSpec(windowSpec.StartIndex, windowSpec.EndIndex, groupKey));

                                if (windowFailure.Kind == WindowSpecFailureKind.RedundantAnchors)
                                    Report(
                                        GeneratorDiagnostics.RedundantBeginEndAnchors,
                                        attribute.Args.AttributeLocation ?? attribute.Args.SyntaxAttributeLocation
                                        ?? matcherMethodModel.IdentifierLocation,
                                        matcherMethodModel.Name);
                            }
                            else
                            {
                                ReportWindowFailure(windowFailure, attribute.Args, matcherMethodModel.Name);
                            }
                    }
                }
            }

            if (windowSpecs.Count == 0) continue;

            matchedMatchersByTarget.TryGetValue(
                MethodIdentity.BuildMethodIdentityKey(effectiveMethod),
                out var matchedMatcherMethods);
            foreach (var generated in GenerateOverloadsForMethod(
                         effectiveMethod,
                         windowSpecs,
                         optionsByGroupKey,
                         matchedMatcherMethods))
            {
                var groupKey = BuildGroupKey(generated.Namespace, generated.BucketType);
                if (!_methodsByGroup.TryGetValue(groupKey, out var list))
                {
                    list = [];
                    _methodsByGroup[groupKey] = list;
                }

                list.Add(generated);
            }
        }

        foreach (var entry in matcherHasAnyMatch)
        {
            if (entry.Value) continue;

            Report(
                GeneratorDiagnostics.MatcherHasNoSubsequenceMatch,
                matcherLocations.TryGetValue(entry.Key, out var location) ? location : null,
                entry.Key.MethodName);
        }
    }

    private IEnumerable<OverloadPlanEntry> GenerateOverloadsForMethod(
        MethodModel method,
        List<WindowSpec> windowSpecs,
        Dictionary<string, GenerationOptions> optionsByGroupKey,
        IReadOnlyCollection<MatcherMethodReference>? matchedMatcherMethods)
    {
        var signatureKeys = new HashSet<string>(StringComparer.Ordinal);
        var existingKeys = BuildExistingMethodKeys(method.ContainingTypeDisplay, method.Name);

        var parameterCount = method.Parameters.Items.Length;
        var originalParameters = method.Parameters.Items;
        var optionalIndexSpecs = BuildOptionalIndexSpecs(windowSpecs, parameterCount);
        var unionOptional = optionalIndexSpecs.SelectMany(spec => spec.Indices).Distinct().ToArray();
        var defaultMap = BuildDefaultValueMap(method);
        var unionHasDefaults = unionOptional.Any(index => defaultMap.TryGetValue(
            originalParameters[index].Name,
            out var hasDefault) && hasDefault);
        var paramsIndex = Array.FindIndex(originalParameters.ToArray(), p => p.IsParams);

        if (unionHasDefaults)
        {
            Report(GeneratorDiagnostics.DefaultsInWindow, method.IdentifierLocation, method.Name);
            yield break;
        }

        if (paramsIndex >= 0 && !unionOptional.Contains(paramsIndex))
        {
            Report(GeneratorDiagnostics.ParamsOutsideWindow, method.IdentifierLocation, method.Name);
            yield break;
        }

        var reportedRefOutIn = false;
        var reportedDuplicate = false;

        foreach (var optionalSpec in optionalIndexSpecs)
        {
            var options = optionsByGroupKey[optionalSpec.GroupKey];
            var optionalIndices = optionalSpec.Indices;
            if (optionalIndices.Length == 0) continue;

            var omissionSets = options.SubsequenceStrategy == OverloadSubsequenceStrategy.PrefixOnly
                ? BuildPrefixOmissions(optionalIndices)
                : BuildAllOmissions(optionalIndices);

            foreach (var omittedIndices in omissionSets)
            {
                if (omittedIndices.Length == 0) continue;

                var omittedParameters = omittedIndices.Select(i => originalParameters[i]).ToArray();

                if (omittedParameters.Any(p => p.RefKind != RefKind.None))
                {
                    if (!reportedRefOutIn)
                    {
                        Report(GeneratorDiagnostics.RefOutInOmitted, method.IdentifierLocation, method.Name);
                        reportedRefOutIn = true;
                    }
                    continue;
                }

                if (omittedParameters.All(p => IsDefaultableParameter(p, defaultMap))) continue;

                var keptParameters = originalParameters.Where((_, index) => Array.IndexOf(omittedIndices, index) < 0)
                    .ToArray();

                var key = MethodIdentity.BuildSignatureKey(method.Name, method.TypeParameterCount, keptParameters);
                if (!signatureKeys.Add(key))
                {
                    if (!reportedDuplicate)
                    {
                        Report(GeneratorDiagnostics.DuplicateSignatureSkipped, method.IdentifierLocation, method.Name);
                        reportedDuplicate = true;
                    }
                    continue;
                }

                if (existingKeys.Contains(key))
                {
                    if (!reportedDuplicate)
                    {
                        Report(GeneratorDiagnostics.DuplicateSignatureSkipped, method.IdentifierLocation, method.Name);
                        reportedDuplicate = true;
                    }
                    continue;
                }

                yield return new OverloadPlanEntry(
                    method,
                    keptParameters,
                    omittedParameters,
                    options.OverloadVisibility,
                    matchedMatcherMethods,
                    options.BucketType);
            }
        }
    }

    private static EquatableArray<GenerateOverloadsAttributeModel> SelectGenerateAttributes(
        EquatableArray<GenerateOverloadsAttributeModel> fromAttribute,
        EquatableArray<GenerateOverloadsAttributeModel> fromSyntax)
    {
        return fromAttribute.Items.Length > 0 ? fromAttribute : fromSyntax;
    }

    private static SourceLocationModel? GetAttributeLocation(EquatableArray<GenerateOverloadsAttributeModel> attributes)
    {
        foreach (var attribute in attributes.Items)
        {
            var location = attribute.Args.AttributeLocation
                ?? attribute.Args.SyntaxAttributeLocation ?? attribute.Args.MethodIdentifierLocation;
            if (location is not null) return location;
        }

        return null;
    }

    private bool HasMatchersWindowConflict(
        MethodModel method,
        EquatableArray<GenerateOverloadsAttributeModel> attributes)
    {
        foreach (var attribute in attributes.Items)
        {
            if (!attribute.HasMatchers || !attribute.Args.HasAny) continue;

            var location = attribute.Args.AttributeLocation
                ?? attribute.Args.SyntaxAttributeLocation ?? method.IdentifierLocation;
            Report(GeneratorDiagnostics.WindowAndMatchersConflict, location, method.Name);
            return true;
        }

        return false;
    }

    private static List<OptionalIndexSpec> BuildOptionalIndexSpecs(List<WindowSpec> windowSpecs, int parameterCount)
    {
        var specs = new List<OptionalIndexSpec>();

        foreach (var group in windowSpecs.GroupBy(spec => spec.GroupKey, StringComparer.Ordinal))
        {
            var union = new SortedSet<int>();
            var groupSpecs = new List<int[]>();

            foreach (var windowSpec in group)
            {
                var start = Clamp(windowSpec.StartIndex, min: 0, parameterCount - 1);
                var end = Clamp(windowSpec.EndIndex, min: 0, parameterCount - 1);

                if (start > end) continue;

                var indices = Enumerable.Range(start, end - start + 1).ToArray();
                specs.Add(new OptionalIndexSpec(indices, group.Key));
                groupSpecs.Add(indices);

                foreach (var index in indices) union.Add(index);
            }

            if (groupSpecs.Count > 1 && union.Count > 0)
            {
                var unionIndices = union.ToArray();
                if (!groupSpecs.Any(spec => spec.SequenceEqual(unionIndices)))
                    specs.Add(new OptionalIndexSpec(unionIndices, group.Key));
            }
        }

        return specs;
    }

    private static Dictionary<string, bool> BuildDefaultValueMap(MethodModel method)
    {
        var map = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var parameter in method.Parameters.Items) map[parameter.Name] = parameter.HasDefaultFromSyntax;

        return map;
    }

    private static bool IsDefaultableParameter(ParameterModel parameter, Dictionary<string, bool> defaultMap)
    {
        return parameter.IsOptional || parameter.HasExplicitDefaultValue
            || (defaultMap.TryGetValue(parameter.Name, out var hasDefault) && hasDefault);
    }

    private static IEnumerable<int[]> BuildAllOmissions(int[] optionalIndices)
    {
        var count = optionalIndices.Length;
        var max = 1 << count;
        for (var mask = 1; mask < max; mask++)
        {
            var list = new List<int>();
            for (var i = 0; i < count; i++)
                if ((mask & (1 << i)) != 0)
                    list.Add(optionalIndices[i]);

            yield return list.ToArray();
        }
    }

    private static IEnumerable<int[]> BuildPrefixOmissions(int[] optionalIndices)
    {
        var count = optionalIndices.Length;
        for (var omit = 1; omit <= count; omit++) yield return optionalIndices.Skip(count - omit).ToArray();
    }

    private HashSet<string> BuildExistingMethodKeys(string containingTypeDisplay, string methodName)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        if (!_typesByDisplay.TryGetValue(containingTypeDisplay, out var typeModel)) return keys;

        foreach (var signature in typeModel.MethodSignatures.Items)
        {
            if (!string.Equals(signature.Name, methodName, StringComparison.Ordinal)) continue;

            var key = MethodIdentity.BuildSignatureKey(
                signature.Name,
                signature.TypeParameterCount,
                signature.Parameters.Items);
            keys.Add(key);
        }

        return keys;
    }


    private void ReportWindowFailure(WindowSpecFailure failure, GenerateOverloadsArgsModel args, string methodName)
    {
        var location = args.SyntaxAttributeLocation ?? args.AttributeLocation ?? args.MethodIdentifierLocation;
        if (failure.Kind == WindowSpecFailureKind.MissingAnchor)
            Report(
                GeneratorDiagnostics.InvalidWindowAnchor,
                location,
                failure.AnchorKind ?? "anchor",
                failure.AnchorValue ?? string.Empty);
        else if (failure.Kind == WindowSpecFailureKind.ConflictingAnchors)
            Report(GeneratorDiagnostics.ConflictingWindowAnchors, location, methodName);
        else if (failure.Kind == WindowSpecFailureKind.RedundantAnchors)
            Report(GeneratorDiagnostics.RedundantBeginEndAnchors, location, methodName);
        else if (failure.Kind == WindowSpecFailureKind.ConflictingBeginAnchors)
            Report(GeneratorDiagnostics.BeginAndBeginExclusiveConflict, location, methodName);
        else if (failure.Kind == WindowSpecFailureKind.ConflictingEndAnchors)
            Report(GeneratorDiagnostics.EndAndEndExclusiveConflict, location, methodName);
    }

    private IEnumerable<ParameterMatch> FindSubsequenceMatches(
        MethodModel matcherMethod,
        MethodModel targetMethod,
        RangeAnchorMatchMode matchMode)
    {
        var matcherParams = matcherMethod.Parameters.Items;
        var targetParams = targetMethod.Parameters.Items;

        if (matcherParams.Length == 0 || matcherParams.Length > targetParams.Length) yield break;

        var indices = new int[matcherParams.Length];

        foreach (var match in FindMatchesRecursive(
                     matcherParams,
                     targetParams,
                     matchMode,
                     matcherIndex: 0,
                     targetIndex: 0,
                     indices)) yield return match;
    }

    private IEnumerable<ParameterMatch> FindMatchesRecursive(
        ImmutableArray<ParameterModel> matcherParams,
        ImmutableArray<ParameterModel> targetParams,
        RangeAnchorMatchMode matchMode,
        int matcherIndex,
        int targetIndex,
        int[] indices)
    {
        if (matcherIndex == matcherParams.Length)
        {
            yield return new ParameterMatch(indices.ToArray());
            yield break;
        }

        for (var i = targetIndex; i < targetParams.Length; i++)
            if (IsMatch(matcherParams[matcherIndex], targetParams[i], matchMode))
            {
                indices[matcherIndex] = i;
                foreach (var match in FindMatchesRecursive(
                             matcherParams,
                             targetParams,
                             matchMode,
                             matcherIndex + 1,
                             i + 1,
                             indices)) yield return match;
            }
    }

    private static bool IsMatch(ParameterModel matcherParam, ParameterModel targetParam, RangeAnchorMatchMode matchMode)
    {
        if (!AreTypesEquivalent(matcherParam.TypeDisplay, targetParam.TypeDisplay)) return false;

        if (matcherParam.RefKind != targetParam.RefKind) return false;

        if (matcherParam.IsParams != targetParam.IsParams) return false;

        if (matchMode == RangeAnchorMatchMode.TypeAndName)
            return string.Equals(matcherParam.Name, targetParam.Name, StringComparison.Ordinal);

        return true;
    }

    private static bool AreTypesEquivalent(string matcherType, string targetType)
    {
        return string.Equals(matcherType, targetType, StringComparison.Ordinal);
    }

    private static IEnumerable<MatcherMethodModel> SelectMatcherMethods(
        EquatableArray<MatcherMethodModel> matcherMethods,
        int targetParameterCount)
    {
        return matcherMethods.Items.Length == 0 ? Array.Empty<MatcherMethodModel>() : matcherMethods.Items;
    }

    private static bool TryCreateWindowSpecFromArgs(
        MethodModel targetMethod,
        MethodModel matcherMethod,
        GenerateOverloadsArgsModel args,
        ParameterMatch match,
        out WindowSpec windowSpec,
        out WindowSpecFailure failure)
    {
        windowSpec = default;
        failure = new WindowSpecFailure(WindowSpecFailureKind.None, anchorKind: null, anchorValue: null);

        var matcherParams = matcherMethod.Parameters.Items;

        var startIndex = 0;
        var endIndex = targetMethod.Parameters.Items.Length - 1;
        if (match.TargetIndices.Length > 0)
        {
            startIndex = match.TargetIndices[0];
            endIndex = match.TargetIndices[match.TargetIndices.Length - 1];
        }

        if (!string.IsNullOrEmpty(args.BeginEnd) && (!string.IsNullOrEmpty(args.Begin)
                || !string.IsNullOrEmpty(args.End) || !string.IsNullOrEmpty(args.BeginExclusive)
                || !string.IsNullOrEmpty(args.EndExclusive)))
        {
            failure = new WindowSpecFailure(
                WindowSpecFailureKind.ConflictingAnchors,
                anchorKind: null,
                anchorValue: null);
            return false;
        }

        if (!string.IsNullOrEmpty(args.Begin) && !string.IsNullOrEmpty(args.BeginExclusive))
        {
            failure = new WindowSpecFailure(WindowSpecFailureKind.ConflictingBeginAnchors, "Begin", args.Begin);
            return false;
        }

        if (!string.IsNullOrEmpty(args.End) && !string.IsNullOrEmpty(args.EndExclusive))
        {
            failure = new WindowSpecFailure(WindowSpecFailureKind.ConflictingEndAnchors, "End", args.End);
            return false;
        }

        if (!string.IsNullOrEmpty(args.BeginEnd))
            args = args with { Begin = args.BeginEnd, End = args.BeginEnd };
        else if (!string.IsNullOrEmpty(args.Begin) && !string.IsNullOrEmpty(args.End)
                 && string.Equals(args.Begin, args.End, StringComparison.Ordinal))
            failure = new WindowSpecFailure(WindowSpecFailureKind.RedundantAnchors, "BeginEnd", args.Begin);

        if (!string.IsNullOrEmpty(args.Begin))
        {
            var beginIdx = IndexOfParameter(matcherParams, args.Begin!);
            if (beginIdx < 0)
            {
                failure = new WindowSpecFailure(WindowSpecFailureKind.MissingAnchor, "Begin", args.Begin);
                return false;
            }

            startIndex = match.TargetIndices[beginIdx];
        }

        if (!string.IsNullOrEmpty(args.BeginExclusive))
        {
            var beginIdx = IndexOfParameter(matcherParams, args.BeginExclusive!);
            if (beginIdx < 0)
            {
                failure = new WindowSpecFailure(
                    WindowSpecFailureKind.MissingAnchor,
                    "BeginExclusive",
                    args.BeginExclusive);
                return false;
            }

            startIndex = match.TargetIndices[beginIdx] + 1;
        }

        if (!string.IsNullOrEmpty(args.End))
        {
            var endIdx = IndexOfParameter(matcherParams, args.End!);
            if (endIdx < 0)
            {
                failure = new WindowSpecFailure(WindowSpecFailureKind.MissingAnchor, "End", args.End);
                return false;
            }

            endIndex = match.TargetIndices[endIdx];
        }

        if (!string.IsNullOrEmpty(args.EndExclusive))
        {
            var endIdx = IndexOfParameter(matcherParams, args.EndExclusive!);
            if (endIdx < 0)
            {
                failure = new WindowSpecFailure(WindowSpecFailureKind.MissingAnchor, "EndExclusive", args.EndExclusive);
                return false;
            }

            endIndex = match.TargetIndices[endIdx] - 1;
        }

        windowSpec = new WindowSpec(startIndex, endIndex);
        return true;
    }

    private static int IndexOfParameter(ImmutableArray<ParameterModel> parameters, string name)
    {
        for (var i = 0; i < parameters.Length; i++)
            if (string.Equals(parameters[i].Name, name, StringComparison.Ordinal))
                return i;

        return -1;
    }

    private static string BuildMethodGroupKey(MethodModel method)
    {
        var builder = new StringBuilder();
        builder.Append(method.ContainingTypeDisplay);
        builder.Append(".");
        builder.Append(method.Name);
        builder.Append("|");
        builder.Append(method.TypeParameterCount);

        foreach (var parameter in method.Parameters.Items)
        {
            builder.Append("|");
            builder.Append(parameter.TypeDisplay);
            builder.Append(":");
            builder.Append(parameter.RefKind);
            builder.Append(":");
            builder.Append(parameter.IsParams ? "params" : "noparams");
        }

        return builder.ToString();
    }

    private static OverloadGroupKey BuildGroupKey(string namespaceName, BucketTypeModel? bucketType)
    {
        if (bucketType is null) return new OverloadGroupKey(namespaceName, DefaultBucketName, IsDefault: true);

        return new OverloadGroupKey(namespaceName, bucketType.Value.Name, IsDefault: false);
    }

    private void ReportInvalidBucketType(BucketTypeModel bucketType, SourceLocationModel? fallbackLocation)
    {
        var location = bucketType.AttributeLocation ?? fallbackLocation;
        var reason = string.IsNullOrWhiteSpace(bucketType.InvalidReason)
            ? "Invalid bucket type"
            : bucketType.InvalidReason!;
        Report(GeneratorDiagnostics.InvalidBucketType, location, bucketType.DisplayName, reason);
    }

    private MethodModel ApplySuppliedParameterTypes(MethodModel method)
    {
        if (method.SupplyParameterTypes.Items.Length == 0) return method;

        var methodTypeParameters = new HashSet<string>(method.TypeParameterNames.Items, StringComparer.Ordinal);
        var replacements = new Dictionary<string, (string Display, string Signature)>(StringComparer.Ordinal);
        var conflicts = new HashSet<string>(StringComparer.Ordinal);

        foreach (var supply in method.SupplyParameterTypes.Items)
        {
            if (!supply.IsValid)
            {
                var location = supply.NameLocation
                               ?? supply.TypeLocation
                               ?? supply.AttributeLocation
                               ?? method.IdentifierLocation;
                var reason = string.IsNullOrWhiteSpace(supply.InvalidReason)
                    ? "Invalid SupplyParameterType."
                    : supply.InvalidReason!;
                Report(GeneratorDiagnostics.InvalidSupplyParameterType, location, method.Name, reason);
                continue;
            }

            if (!methodTypeParameters.Contains(supply.TypeParameterName))
            {
                var location = supply.NameLocation
                               ?? supply.AttributeLocation
                               ?? method.IdentifierLocation;
                Report(
                    GeneratorDiagnostics.SupplyParameterTypeMissingTypeParameter,
                    location,
                    supply.TypeParameterName,
                    method.Name);
                continue;
            }

            if (replacements.TryGetValue(supply.TypeParameterName, out var existing))
            {
                if (!string.Equals(existing.Display, supply.SuppliedTypeDisplay, StringComparison.Ordinal)
                    || !string.Equals(existing.Signature, supply.SuppliedSignatureTypeDisplay, StringComparison.Ordinal))
                {
                    Report(
                        GeneratorDiagnostics.SupplyParameterTypeConflicting,
                        supply.AttributeLocation ?? method.IdentifierLocation,
                        supply.TypeParameterName,
                        method.Name);
                    conflicts.Add(supply.TypeParameterName);
                }
                continue;
            }

            replacements[supply.TypeParameterName] = (supply.SuppliedTypeDisplay, supply.SuppliedSignatureTypeDisplay);
        }

        if (conflicts.Count > 0)
            foreach (var name in conflicts)
                replacements.Remove(name);

        if (replacements.Count == 0) return method;

        var originalTypeParams = method.TypeParameterNames.Items;
        var remainingTypeParams = originalTypeParams
            .Where(name => !replacements.ContainsKey(name))
            .ToArray();
        var removedNames = new HashSet<string>(replacements.Keys, StringComparer.Ordinal);
        var invocationTypeArguments = originalTypeParams
            .Select(name => replacements.TryGetValue(name, out var replacement) ? replacement.Display : name)
            .ToArray();

        var updatedParameters = method.Parameters.Items
            .Select(parameter => parameter with
            {
                TypeDisplay = ReplaceTypeParameters(parameter.TypeDisplay, replacements, useSignature: false),
                SignatureTypeDisplay = ReplaceTypeParameters(parameter.SignatureTypeDisplay, replacements,
                    useSignature: true)
            })
            .ToArray();

        return method with
        {
            ReturnTypeDisplay = ReplaceTypeParameters(method.ReturnTypeDisplay, replacements, useSignature: false),
            TypeParameterNames = new EquatableArray<string>(remainingTypeParams.ToImmutableArray()),
            TypeParameterCount = remainingTypeParams.Length,
            TypeParameterConstraints = RemoveTypeParameterConstraints(method.TypeParameterConstraints, removedNames),
            InvocationTypeArguments = new EquatableArray<string>(invocationTypeArguments.ToImmutableArray()),
            Parameters = new EquatableArray<ParameterModel>(updatedParameters.ToImmutableArray())
        };
    }

    private static string ReplaceTypeParameters(
        string input,
        IReadOnlyDictionary<string, (string Display, string Signature)> replacements,
        bool useSignature)
    {
        if (string.IsNullOrEmpty(input) || replacements.Count == 0) return input;

        var builder = new StringBuilder(input.Length);
        var index = 0;
        while (index < input.Length)
        {
            var current = input[index];
            if (current == '@')
            {
                var start = index;
                index++;
                if (index < input.Length && IsIdentifierStart(input[index]))
                {
                    index++;
                    while (index < input.Length && IsIdentifierPart(input[index])) index++;
                    var token = input.Substring(start, index - start);
                    var lookup = token.Substring(startIndex: 1);
                    if (replacements.TryGetValue(lookup, out var replacement))
                        builder.Append(useSignature ? replacement.Signature : replacement.Display);
                    else
                        builder.Append(token);
                    continue;
                }

                builder.Append('@');
                continue;
            }

            if (IsIdentifierStart(current))
            {
                var start = index;
                index++;
                while (index < input.Length && IsIdentifierPart(input[index])) index++;
                var token = input.Substring(start, index - start);
                if (replacements.TryGetValue(token, out var replacement))
                    builder.Append(useSignature ? replacement.Signature : replacement.Display);
                else
                    builder.Append(token);
                continue;
            }

            builder.Append(current);
            index++;
        }

        return builder.ToString();
    }

    private static bool IsIdentifierStart(char value)
    {
        return char.IsLetter(value) || value == '_';
    }

    private static bool IsIdentifierPart(char value)
    {
        return char.IsLetterOrDigit(value) || value == '_';
    }

    private static string RemoveTypeParameterConstraints(string constraints, HashSet<string> removedNames)
    {
        if (string.IsNullOrWhiteSpace(constraints) || removedNames.Count == 0) return constraints;

        var parts = constraints.Split(new[] { "where " }, StringSplitOptions.RemoveEmptyEntries);
        var kept = new List<string>();

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.Length == 0) continue;

            var colonIndex = trimmed.IndexOf(value: ':');
            var name = colonIndex >= 0 ? trimmed.Substring(startIndex: 0, colonIndex).Trim() : trimmed;
            if (removedNames.Contains(name)) continue;

            kept.Add("where " + trimmed);
        }

        return string.Join(" ", kept);
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;

        if (value > max) return max;

        return value;
    }

    private readonly struct WindowSpec(int startIndex, int endIndex, string groupKey)
    {
        public WindowSpec(int startIndex, int endIndex) : this(startIndex, endIndex, string.Empty)
        {
        }

        public int StartIndex { get; } = startIndex;
        public int EndIndex { get; } = endIndex;
        public string GroupKey { get; } = groupKey;
    }

    private readonly struct OptionalIndexSpec(int[] indices, string groupKey)
    {
        public int[] Indices { get; } = indices;
        public string GroupKey { get; } = groupKey;
    }

    private readonly struct ParameterMatch(int[] targetIndices)
    {
        public int[] TargetIndices { get; } = targetIndices;

        public static ParameterMatch Identity(int length)
        {
            var indices = new int[length];
            for (var i = 0; i < length; i++) indices[i] = i;

            return new ParameterMatch(indices);
        }
    }

    private enum WindowSpecFailureKind
    {
        None,
        MissingAnchor,
        ConflictingAnchors,
        RedundantAnchors,
        ConflictingBeginAnchors,
        ConflictingEndAnchors
    }

    private readonly struct WindowSpecFailure(WindowSpecFailureKind kind, string? anchorKind, string? anchorValue)
    {
        public WindowSpecFailureKind Kind { get; } = kind;
        public string? AnchorKind { get; } = anchorKind;
        public string? AnchorValue { get; } = anchorValue;
    }
}
