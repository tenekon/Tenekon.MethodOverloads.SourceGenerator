using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Generates overload candidates from collected metadata and options.
    /// </summary>
    private void GenerateMethods()
    {
        var matcherHasAnyMatch = new Dictionary<MatcherMethodReference, bool>();
        var matcherLocations = new Dictionary<MatcherMethodReference, SourceLocationModel?>();
        var matchedMatchersByTarget = new Dictionary<string, HashSet<MatcherMethodReference>>(StringComparer.Ordinal);

        var methodTargetsByKey = _input.MethodTargets.Items.ToDictionary(
            target => BuildMethodIdentityKey(target.Method),
            target => target,
            StringComparer.Ordinal);

        var candidateMethods = new Dictionary<string, MethodModel>(StringComparer.Ordinal);
        foreach (var typeTarget in _input.TypeTargets.Items)
        {
            foreach (var method in typeTarget.Type.Methods.Items)
            {
                var key = BuildMethodIdentityKey(method);
                if (!candidateMethods.ContainsKey(key))
                {
                    candidateMethods[key] = method;
                }
            }
        }

        foreach (var methodTarget in _input.MethodTargets.Items)
        {
            var key = BuildMethodIdentityKey(methodTarget.Method);
            if (!candidateMethods.ContainsKey(key))
            {
                candidateMethods[key] = methodTarget.Method;
            }
        }

        foreach (var method in candidateMethods.Values)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (!method.IsOrdinary)
            {
                continue;
            }

            if (_matcherTypeDisplays.Contains(method.ContainingTypeDisplay))
            {
                continue;
            }

            if (method.DeclaredAccessibility == Accessibility.Private ||
                method.DeclaredAccessibility == Accessibility.Protected)
            {
                continue;
            }

            _typeTargetsByDisplay.TryGetValue(method.ContainingTypeDisplay, out var typeTarget);
            methodTargetsByKey.TryGetValue(BuildMethodIdentityKey(method), out var methodTarget);

            var hasTypeGenerate = typeTarget.HasGenerateMethodOverloads;
            var hasMethodGenerate = methodTarget.HasGenerateOverloads;

            if (!hasTypeGenerate && !hasMethodGenerate)
            {
                continue;
            }

            var windowSpecs = new List<WindowSpec>();
            var methodMatchers = EquatableArray<string>.Empty;

            if (hasMethodGenerate)
            {
                methodMatchers = methodTarget.MatcherTypeDisplays;
            }

            if (hasMethodGenerate && method.Parameters.Items.Length == 0)
            {
                var location = methodTarget.GenerateArgsFromAttribute?.AttributeLocation
                    ?? methodTarget.GenerateArgsFromSyntax?.SyntaxAttributeLocation
                    ?? method.IdentifierLocation;
                Report(GeneratorDiagnostics.ParameterlessTargetMethod, location, method.Name);
                continue;
            }

            var useMethodMatchers = methodMatchers.Items.Length > 0;
            var useDirectGenerateOverloads = hasMethodGenerate && !useMethodMatchers;
            var useTypeMatchers = !hasMethodGenerate && hasTypeGenerate;

            if (useDirectGenerateOverloads)
            {
                if (methodTarget.GenerateArgsFromAttribute is not null)
                {
                    if (TryCreateWindowSpecFromArgs(method, method, methodTarget.GenerateArgsFromAttribute.Value,
                            ParameterMatch.Identity(method.Parameters.Items.Length),
                            out var windowSpecFromAttribute, out var windowFailureFromAttribute))
                    {
                        var groupKey = "direct:" + BuildMethodGroupKey(method);
                        windowSpecs.Add(new WindowSpec(windowSpecFromAttribute.StartIndex, windowSpecFromAttribute.EndIndex, groupKey));

                        if (windowFailureFromAttribute.Kind == WindowSpecFailureKind.RedundantAnchors)
                        {
                            Report(
                                GeneratorDiagnostics.RedundantBeginEndAnchors,
                                methodTarget.GenerateArgsFromAttribute.Value.AttributeLocation
                                ?? method.IdentifierLocation,
                                method.Name);
                        }
                    }
                    else
                    {
                        ReportWindowFailure(windowFailureFromAttribute, methodTarget.GenerateArgsFromAttribute.Value, method.Name);
                    }
                }
                else if (methodTarget.GenerateArgsFromSyntax is not null)
                {
                    if (TryCreateWindowSpecFromArgs(method, method, methodTarget.GenerateArgsFromSyntax.Value,
                            ParameterMatch.Identity(method.Parameters.Items.Length),
                            out var windowSpecFromSyntax, out var windowFailureFromSyntax))
                    {
                        var groupKey = "direct:" + BuildMethodGroupKey(method);
                        windowSpecs.Add(new WindowSpec(windowSpecFromSyntax.StartIndex, windowSpecFromSyntax.EndIndex, groupKey));

                        if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.RedundantAnchors)
                        {
                            Report(
                                GeneratorDiagnostics.RedundantBeginEndAnchors,
                                methodTarget.GenerateArgsFromSyntax.Value.SyntaxAttributeLocation
                                ?? method.IdentifierLocation,
                                method.Name);
                        }
                    }
                    else
                    {
                        ReportWindowFailure(windowFailureFromSyntax, methodTarget.GenerateArgsFromSyntax.Value, method.Name);
                    }
                }
            }

            if (useMethodMatchers || useTypeMatchers)
            {
                var matcherTypes = useMethodMatchers ? methodMatchers : typeTarget.MatcherTypeDisplays;
                foreach (var matcherTypeDisplay in matcherTypes.Items)
                {
                    if (!_matcherTypesByDisplay.TryGetValue(matcherTypeDisplay, out var matcherType))
                    {
                        continue;
                    }

                    foreach (var matcherMethod in SelectMatcherMethods(matcherType.MatcherMethods, method.Parameters.Items.Length))
                    {
                        var matcherMethodModel = matcherMethod.Method;
                        if (!matcherMethodModel.IsOrdinary)
                        {
                            continue;
                        }

                        if (matcherMethodModel.Parameters.Items.Length == 0)
                        {
                            var location = matcherMethod.GenerateArgsFromAttribute?.AttributeLocation
                                ?? matcherMethod.GenerateArgsFromSyntax?.SyntaxAttributeLocation
                                ?? matcherMethodModel.IdentifierLocation;
                            Report(GeneratorDiagnostics.ParameterlessTargetMethod, location, matcherMethodModel.Name);
                            continue;
                        }

                        var matcherRef = new MatcherMethodReference(
                            matcherMethodModel.ContainingTypeDisplay,
                            matcherMethodModel.Name,
                            matcherMethodModel.Parameters.Items.Length,
                            matcherMethodModel.ContainingNamespace);

                        if (!matcherHasAnyMatch.ContainsKey(matcherRef))
                        {
                            matcherHasAnyMatch[matcherRef] = false;
                            matcherLocations[matcherRef] = matcherMethodModel.IdentifierLocation;
                        }

                        var groupKey = "matcher:" + BuildMethodGroupKey(matcherMethodModel);
                        var matches = FindSubsequenceMatches(matcherMethodModel, method).ToArray();
                        if (matches.Length == 0)
                        {
                            continue;
                        }

                        matcherHasAnyMatch[matcherRef] = true;

                        var targetKey = BuildMethodIdentityKey(method);
                        if (!matchedMatchersByTarget.TryGetValue(targetKey, out var matchedMatchers))
                        {
                            matchedMatchers = [];
                            matchedMatchersByTarget[targetKey] = matchedMatchers;
                        }

                        matchedMatchers.Add(matcherRef);

                        if (!_matchedMatchersByNamespace.TryGetValue(method.ContainingNamespace, out var matchedByNamespace))
                        {
                            matchedByNamespace = [];
                            _matchedMatchersByNamespace[method.ContainingNamespace] = matchedByNamespace;
                        }

                        matchedByNamespace.Add(matcherRef);

                        foreach (var match in matches)
                        {
                            var args = matcherMethod.GenerateArgsFromAttribute ?? matcherMethod.GenerateArgsFromSyntax;
                            if (args is null)
                            {
                                continue;
                            }

                            if (TryCreateWindowSpecFromArgs(method, matcherMethodModel, args.Value, match,
                                    out var windowSpec, out var windowFailure))
                            {
                                windowSpecs.Add(new WindowSpec(windowSpec.StartIndex, windowSpec.EndIndex, groupKey));

                                if (windowFailure.Kind == WindowSpecFailureKind.RedundantAnchors)
                                {
                                    Report(
                                        GeneratorDiagnostics.RedundantBeginEndAnchors,
                                        args.Value.AttributeLocation
                                        ?? args.Value.SyntaxAttributeLocation
                                        ?? matcherMethodModel.IdentifierLocation,
                                        matcherMethodModel.Name);
                                }
                            }
                            else
                            {
                                ReportWindowFailure(windowFailure, args.Value, matcherMethodModel.Name);
                            }
                        }
                    }
                }
            }

            if (windowSpecs.Count == 0)
            {
                continue;
            }

            var options = GetEffectiveOptions(method, methodTarget);
            if (methodTarget.SyntaxOptions.HasValue)
            {
                ApplyOptions(options, methodTarget.SyntaxOptions.Value);
            }

            matchedMatchersByTarget.TryGetValue(BuildMethodIdentityKey(method), out var matchedMatcherMethods);
            foreach (var generated in GenerateOverloadsForMethod(method, windowSpecs, options, matchedMatcherMethods))
            {
                if (!_methodsByNamespace.TryGetValue(generated.Namespace, out var list))
                {
                    list = [];
                    _methodsByNamespace[generated.Namespace] = list;
                }

                list.Add(generated);
            }
        }

        foreach (var entry in matcherHasAnyMatch)
        {
            if (entry.Value)
            {
                continue;
            }

            Report(
                GeneratorDiagnostics.MatcherHasNoSubsequenceMatch,
                matcherLocations.TryGetValue(entry.Key, out var location) ? location : null,
                entry.Key.MethodName);
        }
    }

    private IEnumerable<GeneratedMethod> GenerateOverloadsForMethod(
        MethodModel method,
        List<WindowSpec> windowSpecs,
        GenerationOptions options,
        IReadOnlyCollection<MatcherMethodReference>? matchedMatcherMethods)
    {
        var signatureKeys = new HashSet<string>(StringComparer.Ordinal);
        var existingKeys = BuildExistingMethodKeys(method.ContainingTypeDisplay, method.Name);

        var parameterCount = method.Parameters.Items.Length;
        var originalParameters = method.Parameters.Items;
        var optionalIndexSpecs = BuildOptionalIndexSpecs(windowSpecs, parameterCount);
        var unionOptional = optionalIndexSpecs.SelectMany(indices => indices).Distinct().ToArray();
        var defaultMap = BuildDefaultValueMap(method);
        var unionHasDefaults = unionOptional.Any(index =>
            defaultMap.TryGetValue(originalParameters[index].Name, out var hasDefault) && hasDefault);
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

        foreach (var optionalIndices in optionalIndexSpecs)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (optionalIndices.Length == 0)
            {
                continue;
            }

            IEnumerable<int[]> omissionSets = options.SubsequenceStrategy == OverloadSubsequenceStrategy.PrefixOnly
                ? BuildPrefixOmissions(optionalIndices)
                : BuildAllOmissions(optionalIndices);

            foreach (var omittedIndices in omissionSets)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                if (omittedIndices.Length == 0)
                {
                    continue;
                }

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

                if (omittedParameters.All(p => IsDefaultableParameter(p, defaultMap)))
                {
                    continue;
                }

                var keptParameters = originalParameters
                    .Where((_, index) => Array.IndexOf(omittedIndices, index) < 0)
                    .ToArray();

                var key = BuildSignatureKey(method.Name, method.TypeParameterCount, keptParameters);
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

                yield return new GeneratedMethod(method, keptParameters, omittedParameters, options.OverloadVisibility, matchedMatcherMethods);
            }
        }
    }

    private static List<int[]> BuildOptionalIndexSpecs(List<WindowSpec> windowSpecs, int parameterCount)
    {
        var specs = new List<int[]>();

        foreach (var group in windowSpecs.GroupBy(spec => spec.GroupKey, StringComparer.Ordinal))
        {
            var union = new SortedSet<int>();
            var groupSpecs = new List<int[]>();

            foreach (var windowSpec in group)
            {
                var start = Clamp(windowSpec.StartIndex, 0, parameterCount - 1);
                var end = Clamp(windowSpec.EndIndex, 0, parameterCount - 1);

                if (start > end)
                {
                    continue;
                }

                var indices = Enumerable.Range(start, end - start + 1).ToArray();
                specs.Add(indices);
                groupSpecs.Add(indices);

                foreach (var index in indices)
                {
                    union.Add(index);
                }
            }

            if (groupSpecs.Count > 1 && union.Count > 0)
            {
                var unionIndices = union.ToArray();
                if (!groupSpecs.Any(spec => spec.SequenceEqual(unionIndices)))
                {
                    specs.Add(unionIndices);
                }
            }
        }

        return specs;
    }

    private static Dictionary<string, bool> BuildDefaultValueMap(MethodModel method)
    {
        var map = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var parameter in method.Parameters.Items)
        {
            map[parameter.Name] = parameter.HasDefaultFromSyntax;
        }

        return map;
    }

    private static bool IsDefaultableParameter(ParameterModel parameter, Dictionary<string, bool> defaultMap)
    {
        return parameter.IsOptional ||
               parameter.HasExplicitDefaultValue ||
               (defaultMap.TryGetValue(parameter.Name, out var hasDefault) && hasDefault);
    }

    private static IEnumerable<int[]> BuildAllOmissions(int[] optionalIndices)
    {
        var count = optionalIndices.Length;
        var max = 1 << count;
        for (var mask = 1; mask < max; mask++)
        {
            var list = new List<int>();
            for (var i = 0; i < count; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    list.Add(optionalIndices[i]);
                }
            }

            yield return list.ToArray();
        }
    }

    private static IEnumerable<int[]> BuildPrefixOmissions(int[] optionalIndices)
    {
        var count = optionalIndices.Length;
        for (var omit = 1; omit <= count; omit++)
        {
            yield return optionalIndices.Skip(count - omit).ToArray();
        }
    }

    private HashSet<string> BuildExistingMethodKeys(string containingTypeDisplay, string methodName)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        if (!_typesByDisplay.TryGetValue(containingTypeDisplay, out var typeModel))
        {
            return keys;
        }

        foreach (var signature in typeModel.MethodSignatures.Items)
        {
            if (!string.Equals(signature.Name, methodName, StringComparison.Ordinal))
            {
                continue;
            }

            var key = BuildSignatureKey(signature.Name, signature.TypeParameterCount, signature.Parameters.Items);
            keys.Add(key);
        }

        return keys;
    }

    private static string BuildSignatureKey(string name, int arity, IEnumerable<ParameterModel> parameters)
    {
        var builder = new StringBuilder();
        builder.Append(name);
        builder.Append("|");
        builder.Append(arity);

        foreach (var parameter in parameters)
        {
            builder.Append("|");
            builder.Append(parameter.SignatureTypeDisplay);
            builder.Append(":");
            builder.Append(parameter.RefKind);
            builder.Append(":");
            builder.Append(parameter.IsParams ? "params" : "noparams");
        }

        return builder.ToString();
    }

    private static string BuildSignatureKey(string name, int arity, IEnumerable<ParameterSignatureModel> parameters)
    {
        var builder = new StringBuilder();
        builder.Append(name);
        builder.Append("|");
        builder.Append(arity);

        foreach (var parameter in parameters)
        {
            builder.Append("|");
            builder.Append(parameter.SignatureTypeDisplay);
            builder.Append(":");
            builder.Append(parameter.RefKind);
            builder.Append(":");
            builder.Append(parameter.IsParams ? "params" : "noparams");
        }

        return builder.ToString();
    }

    private GenerationOptions GetEffectiveOptions(MethodModel method, MethodTargetModel methodTarget)
    {
        var options = new GenerationOptions
        {
            RangeAnchorMatchMode = RangeAnchorMatchMode.TypeOnly,
            SubsequenceStrategy = OverloadSubsequenceStrategy.UniqueBySignature,
            OverloadVisibility = OverloadVisibility.MatchTarget
        };

        if (_typesByDisplay.TryGetValue(method.ContainingTypeDisplay, out var typeModel) &&
            typeModel.Options.HasAny)
        {
            ApplyOptions(options, typeModel.Options);
        }

        if (method.Options.HasAny)
        {
            ApplyOptions(options, method.Options);
        }

        return options;
    }

    private static void ApplyOptions(GenerationOptions options, OverloadOptionsModel optionsSyntax)
    {
        if (optionsSyntax.RangeAnchorMatchMode.HasValue)
        {
            options.RangeAnchorMatchMode = optionsSyntax.RangeAnchorMatchMode.Value;
        }

        if (optionsSyntax.SubsequenceStrategy.HasValue)
        {
            options.SubsequenceStrategy = optionsSyntax.SubsequenceStrategy.Value;
        }

        if (optionsSyntax.OverloadVisibility.HasValue)
        {
            options.OverloadVisibility = optionsSyntax.OverloadVisibility.Value;
        }
    }

    private static string BuildParameterSignatureKey(IEnumerable<ParameterModel> parameters)
    {
        var builder = new StringBuilder();
        foreach (var parameter in parameters)
        {
            builder.Append("|");
            builder.Append(parameter.SignatureTypeDisplay);
            builder.Append(":");
            builder.Append(parameter.RefKind);
            builder.Append(":");
            builder.Append(parameter.IsParams ? "params" : "noparams");
        }

        return builder.ToString();
    }

    private void ReportWindowFailure(WindowSpecFailure failure, GenerateOverloadsArgsModel args, string methodName)
    {
        var location = args.SyntaxAttributeLocation ?? args.AttributeLocation ?? args.MethodIdentifierLocation;
        if (failure.Kind == WindowSpecFailureKind.MissingAnchor)
        {
            Report(
                GeneratorDiagnostics.InvalidWindowAnchor,
                location,
                failure.AnchorKind ?? "anchor",
                failure.AnchorValue ?? string.Empty);
        }
        else if (failure.Kind == WindowSpecFailureKind.ConflictingAnchors)
        {
            Report(
                GeneratorDiagnostics.ConflictingWindowAnchors,
                location,
                methodName);
        }
        else if (failure.Kind == WindowSpecFailureKind.RedundantAnchors)
        {
            Report(
                GeneratorDiagnostics.RedundantBeginEndAnchors,
                location,
                methodName);
        }
        else if (failure.Kind == WindowSpecFailureKind.ConflictingBeginAnchors)
        {
            Report(
                GeneratorDiagnostics.BeginAndBeginExclusiveConflict,
                location,
                methodName);
        }
        else if (failure.Kind == WindowSpecFailureKind.ConflictingEndAnchors)
        {
            Report(
                GeneratorDiagnostics.EndAndEndExclusiveConflict,
                location,
                methodName);
        }
    }
}
