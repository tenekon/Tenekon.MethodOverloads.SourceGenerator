using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Generates overload candidates from collected metadata and options.
    /// </summary>
    private void GenerateMethods()
    {
        var matcherHasAnyMatch = new Dictionary<IMethodSymbol, bool>(SymbolEqualityComparer.Default);
        var matcherLocations = new Dictionary<IMethodSymbol, Location?>(SymbolEqualityComparer.Default);

        foreach (var pair in _typeContexts)
        {
            var typeSymbol = pair.Key;
            var typeContext = pair.Value;

            foreach (var method in typeContext.Methods)
            {
                if (method.MethodKind != MethodKind.Ordinary)
                {
                    continue;
                }

                if (_matcherTypes.Contains(method.ContainingType))
                {
                    continue;
                }

                if (method.DeclaredAccessibility == Accessibility.Private ||
                    method.DeclaredAccessibility == Accessibility.Protected)
                {
                    continue;
                }

                var methodGenerateOverloads = GetAttribute(method, GenerateOverloadsAttributeName);
                typeContext.MethodSyntax.TryGetValue(method, out var methodSyntax);
                var hasGenerateOverloadsSyntax = methodSyntax is not null && HasAttribute(methodSyntax, "GenerateOverloads");
                var hasMethodGenerateOverloads = methodGenerateOverloads is not null || hasGenerateOverloadsSyntax;

                if (!typeContext.HasGenerateMethodOverloads && !hasMethodGenerateOverloads)
                {
                    continue;
                }

                var windowSpecs = new List<WindowSpec>();
                ImmutableArray<INamedTypeSymbol> methodMatchers = ImmutableArray<INamedTypeSymbol>.Empty;
                if (hasMethodGenerateOverloads && methodSyntax is not null)
                {
                    methodMatchers = ExtractMatchers(methodGenerateOverloads, methodSyntax);
                    foreach (var matcher in methodMatchers)
                    {
                        _matcherTypes.Add(matcher);
                    }
                }

                var useMethodMatchers = methodMatchers.Length > 0;
                var useDirectGenerateOverloads = hasMethodGenerateOverloads && !useMethodMatchers;
                var useTypeMatchers = !hasMethodGenerateOverloads && typeContext.HasGenerateMethodOverloads;

                if (useDirectGenerateOverloads && methodGenerateOverloads is not null)
                {
                    if (TryCreateWindowSpec(method, methodGenerateOverloads, out var windowSpecFromAttribute, out var windowFailureFromAttribute))
                    {
                        var groupKey = "direct:" + BuildMethodGroupKey(method);
                        windowSpecs.Add(new WindowSpec(windowSpecFromAttribute.StartIndex, windowSpecFromAttribute.EndIndex, groupKey));

                        if (windowFailureFromAttribute.Kind == WindowSpecFailureKind.RedundantAnchors)
                        {
                            Report(
                                GeneratorDiagnostics.RedundantBeginEndAnchors,
                                methodGenerateOverloads.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                    ?? methodSyntax?.Identifier.GetLocation()
                                    ?? method.Locations.FirstOrDefault(),
                                method.Name);
                        }
                    }
                    else if (windowFailureFromAttribute.Kind == WindowSpecFailureKind.MissingAnchor)
                    {
                        Report(
                            GeneratorDiagnostics.InvalidWindowAnchor,
                            methodSyntax?.Identifier.GetLocation() ?? method.Locations.FirstOrDefault(),
                            windowFailureFromAttribute.AnchorKind ?? "anchor",
                            windowFailureFromAttribute.AnchorValue ?? string.Empty);
                    }
                    else if (windowFailureFromAttribute.Kind == WindowSpecFailureKind.ConflictingAnchors)
                    {
                        Report(
                            GeneratorDiagnostics.ConflictingWindowAnchors,
                            methodSyntax?.Identifier.GetLocation() ?? method.Locations.FirstOrDefault(),
                            method.Name);
                    }
                    else if (windowFailureFromAttribute.Kind == WindowSpecFailureKind.RedundantAnchors)
                    {
                        Report(
                            GeneratorDiagnostics.RedundantBeginEndAnchors,
                            methodSyntax?.Identifier.GetLocation() ?? method.Locations.FirstOrDefault(),
                            method.Name);
                    }
                    else if (windowFailureFromAttribute.Kind == WindowSpecFailureKind.ConflictingBeginAnchors)
                    {
                        Report(
                            GeneratorDiagnostics.BeginAndBeginExclusiveConflict,
                            methodGenerateOverloads.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                ?? methodSyntax?.Identifier.GetLocation()
                                ?? method.Locations.FirstOrDefault(),
                            method.Name);
                    }
                    else if (windowFailureFromAttribute.Kind == WindowSpecFailureKind.ConflictingEndAnchors)
                    {
                        Report(
                            GeneratorDiagnostics.EndAndEndExclusiveConflict,
                            methodGenerateOverloads.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                ?? methodSyntax?.Identifier.GetLocation()
                                ?? method.Locations.FirstOrDefault(),
                            method.Name);
                    }
                }
                else if (useDirectGenerateOverloads && methodSyntax is not null)
                {
                    if (TryCreateWindowSpecFromSyntax(method, methodSyntax, out var windowSpecFromSyntax, out var windowFailureFromSyntax))
                    {
                        var groupKey = "direct:" + BuildMethodGroupKey(method);
                        windowSpecs.Add(new WindowSpec(windowSpecFromSyntax.StartIndex, windowSpecFromSyntax.EndIndex, groupKey));

                        if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.RedundantAnchors)
                        {
                            Report(
                                GeneratorDiagnostics.RedundantBeginEndAnchors,
                                GetGenerateOverloadsAttributeLocation(methodSyntax)
                                    ?? methodSyntax.Identifier.GetLocation(),
                                method.Name);
                        }
                    }
                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.MissingAnchor)
                    {
                        Report(
                            GeneratorDiagnostics.InvalidWindowAnchor,
                            methodSyntax.Identifier.GetLocation(),
                            windowFailureFromSyntax.AnchorKind ?? "anchor",
                            windowFailureFromSyntax.AnchorValue ?? string.Empty);
                    }
                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.ConflictingAnchors)
                    {
                        Report(
                            GeneratorDiagnostics.ConflictingWindowAnchors,
                            methodSyntax.Identifier.GetLocation(),
                            method.Name);
                    }
                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.RedundantAnchors)
                    {
                        Report(
                            GeneratorDiagnostics.RedundantBeginEndAnchors,
                            methodSyntax.Identifier.GetLocation(),
                            method.Name);
                    }
                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.ConflictingBeginAnchors)
                    {
                        Report(
                            GeneratorDiagnostics.BeginAndBeginExclusiveConflict,
                            GetGenerateOverloadsAttributeLocation(methodSyntax)
                                ?? methodSyntax.Identifier.GetLocation(),
                            method.Name);
                    }
                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.ConflictingEndAnchors)
                    {
                        Report(
                            GeneratorDiagnostics.EndAndEndExclusiveConflict,
                            GetGenerateOverloadsAttributeLocation(methodSyntax)
                                ?? methodSyntax.Identifier.GetLocation(),
                            method.Name);
                    }
                }

                if (useMethodMatchers || useTypeMatchers)
                {
                    var matcherTypes = useMethodMatchers ? methodMatchers : typeContext.MatcherTypes;
                    foreach (var matcherType in matcherTypes)
                    {
                        var matcherMethods = matcherType.GetMembers().OfType<IMethodSymbol>()
                            .Where(m => m.MethodKind == MethodKind.Ordinary)
                            .Where(m =>
                            {
                                var generate = GetAttribute(m, GenerateOverloadsAttributeName);
                                if (generate is not null)
                                {
                                    return true;
                                }

                                var syntax = m.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).OfType<MethodDeclarationSyntax>().FirstOrDefault();
                                return syntax is not null && HasAttribute(syntax, "GenerateOverloads");
                            })
                            .ToArray();

                        foreach (var matcherMethod in SelectMatcherMethods(matcherMethods, method.Parameters.Length))
                        {
                            var matcherGenerateOverloads = GetAttribute(matcherMethod, GenerateOverloadsAttributeName);
                            var matcherMethodSyntax = matcherMethod.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).OfType<MethodDeclarationSyntax>().FirstOrDefault();
                            var hasMatcherGenerateOverloadsSyntax = matcherMethodSyntax is not null && HasAttribute(matcherMethodSyntax, "GenerateOverloads");
                            if (matcherGenerateOverloads is null && !hasMatcherGenerateOverloadsSyntax)
                            {
                                continue;
                            }

                            if (!matcherHasAnyMatch.ContainsKey(matcherMethod))
                            {
                                matcherHasAnyMatch[matcherMethod] = false;
                                matcherLocations[matcherMethod] = matcherMethodSyntax?.Identifier.GetLocation() ?? matcherMethod.Locations.FirstOrDefault();
                            }

                            var groupKey = "matcher:" + BuildMethodGroupKey(matcherMethod);
                            var matches = FindSubsequenceMatches(matcherMethod, method).ToArray();
                            if (matches.Length == 0)
                            {
                                continue;
                            }

                            matcherHasAnyMatch[matcherMethod] = true;

                            foreach (var match in matches)
                            {
                                if (matcherGenerateOverloads is not null)
                                {
                                    if (TryCreateWindowSpec(method, matcherMethod, matcherGenerateOverloads, match, out var windowSpec, out var windowFailure))
                                    {
                                        windowSpecs.Add(new WindowSpec(windowSpec.StartIndex, windowSpec.EndIndex, groupKey));

                                        if (windowFailure.Kind == WindowSpecFailureKind.RedundantAnchors)
                                        {
                                            Report(
                                                GeneratorDiagnostics.RedundantBeginEndAnchors,
                                                matcherGenerateOverloads.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                                    ?? matcherMethodSyntax?.Identifier.GetLocation()
                                                    ?? matcherMethod.Locations.FirstOrDefault(),
                                                matcherMethod.Name);
                                        }
                                    }
                                    else if (windowFailure.Kind == WindowSpecFailureKind.MissingAnchor)
                                    {
                                        Report(
                                            GeneratorDiagnostics.InvalidWindowAnchor,
                                            matcherMethodSyntax?.Identifier.GetLocation() ?? matcherMethod.Locations.FirstOrDefault(),
                                            windowFailure.AnchorKind ?? "anchor",
                                            windowFailure.AnchorValue ?? string.Empty);
                                    }
                                    else if (windowFailure.Kind == WindowSpecFailureKind.ConflictingAnchors)
                                    {
                                        Report(
                                            GeneratorDiagnostics.ConflictingWindowAnchors,
                                            matcherMethodSyntax?.Identifier.GetLocation() ?? matcherMethod.Locations.FirstOrDefault(),
                                            matcherMethod.Name);
                                    }
                                    else if (windowFailure.Kind == WindowSpecFailureKind.RedundantAnchors)
                                    {
                                        Report(
                                            GeneratorDiagnostics.RedundantBeginEndAnchors,
                                            matcherMethodSyntax?.Identifier.GetLocation() ?? matcherMethod.Locations.FirstOrDefault(),
                                            matcherMethod.Name);
                                    }
                                    else if (windowFailure.Kind == WindowSpecFailureKind.ConflictingBeginAnchors)
                                    {
                                        Report(
                                            GeneratorDiagnostics.BeginAndBeginExclusiveConflict,
                                            matcherGenerateOverloads.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                                ?? matcherMethodSyntax?.Identifier.GetLocation()
                                                ?? matcherMethod.Locations.FirstOrDefault(),
                                            matcherMethod.Name);
                                    }
                                    else if (windowFailure.Kind == WindowSpecFailureKind.ConflictingEndAnchors)
                                    {
                                        Report(
                                            GeneratorDiagnostics.EndAndEndExclusiveConflict,
                                            matcherGenerateOverloads.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                                ?? matcherMethodSyntax?.Identifier.GetLocation()
                                                ?? matcherMethod.Locations.FirstOrDefault(),
                                            matcherMethod.Name);
                                    }
                                }
                                else if (matcherMethodSyntax is not null)
                                {
                                    if (TryCreateWindowSpecFromSyntax(method, matcherMethod, matcherMethodSyntax, match, out var windowSpecFromSyntax, out var windowFailureFromSyntax))
                                    {
                                        windowSpecs.Add(new WindowSpec(windowSpecFromSyntax.StartIndex, windowSpecFromSyntax.EndIndex, groupKey));

                                        if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.RedundantAnchors)
                                        {
                                            Report(
                                                GeneratorDiagnostics.RedundantBeginEndAnchors,
                                                GetGenerateOverloadsAttributeLocation(matcherMethodSyntax)
                                                    ?? matcherMethodSyntax.Identifier.GetLocation(),
                                                matcherMethod.Name);
                                        }
                                    }
                                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.MissingAnchor)
                                    {
                                        Report(
                                            GeneratorDiagnostics.InvalidWindowAnchor,
                                            matcherMethodSyntax.Identifier.GetLocation(),
                                            windowFailureFromSyntax.AnchorKind ?? "anchor",
                                            windowFailureFromSyntax.AnchorValue ?? string.Empty);
                                    }
                                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.ConflictingAnchors)
                                    {
                                        Report(
                                            GeneratorDiagnostics.ConflictingWindowAnchors,
                                            matcherMethodSyntax.Identifier.GetLocation(),
                                            matcherMethod.Name);
                                    }
                                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.RedundantAnchors)
                                    {
                                        Report(
                                            GeneratorDiagnostics.RedundantBeginEndAnchors,
                                            matcherMethodSyntax.Identifier.GetLocation(),
                                            matcherMethod.Name);
                                    }
                                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.ConflictingBeginAnchors)
                                    {
                                        Report(
                                            GeneratorDiagnostics.BeginAndBeginExclusiveConflict,
                                            GetGenerateOverloadsAttributeLocation(matcherMethodSyntax)
                                                ?? matcherMethodSyntax.Identifier.GetLocation(),
                                            matcherMethod.Name);
                                    }
                                    else if (windowFailureFromSyntax.Kind == WindowSpecFailureKind.ConflictingEndAnchors)
                                    {
                                        Report(
                                            GeneratorDiagnostics.EndAndEndExclusiveConflict,
                                            GetGenerateOverloadsAttributeLocation(matcherMethodSyntax)
                                                ?? matcherMethodSyntax.Identifier.GetLocation(),
                                            matcherMethod.Name);
                                    }
                                }
                            }
                        }
                    }
                }

                if (windowSpecs.Count == 0)
                {
                    continue;
                }

                var options = GetEffectiveOptions(method, typeSymbol);
                if (methodSyntax is not null && TryGetOverloadOptionsFromSyntax(methodSyntax, out var syntaxOptions))
                {
                    ApplyOptions(options, syntaxOptions);
                }

                foreach (var generated in GenerateOverloadsForMethod(method, windowSpecs, options))
                {
                    if (!_methodsByNamespace.TryGetValue(generated.Namespace, out var list))
                    {
                        list = new List<GeneratedMethod>();
                        _methodsByNamespace[generated.Namespace] = list;
                    }

                    list.Add(generated);
                }
            }
        }

        foreach (var entry in matcherHasAnyMatch)
        {
            if (entry.Value)
            {
                continue;
            }

            var matcherMethod = entry.Key;
            Report(
                GeneratorDiagnostics.MatcherHasNoSubsequenceMatch,
                matcherLocations.TryGetValue(matcherMethod, out var location) ? location : matcherMethod.Locations.FirstOrDefault(),
                matcherMethod.Name);
        }
    }
    private IEnumerable<GeneratedMethod> GenerateOverloadsForMethod(
        IMethodSymbol method,
        List<WindowSpec> windowSpecs,
        GenerationOptions options)
    {
        var signatureKeys = new HashSet<string>(StringComparer.Ordinal);
        var existingKeys = BuildExistingMethodKeys(method.ContainingType, method.Name);

        var parameterCount = method.Parameters.Length;
        var originalParameters = method.Parameters;
        var optionalIndexSpecs = BuildOptionalIndexSpecs(windowSpecs, parameterCount);
        var unionOptional = optionalIndexSpecs.SelectMany(indices => indices).Distinct().ToArray();
        var defaultMap = BuildDefaultValueMap(method);
        var unionHasDefaults = unionOptional.Any(index =>
            defaultMap.TryGetValue(originalParameters[index].Name, out var hasDefault) && hasDefault);
        var paramsIndex = Array.FindIndex(originalParameters.ToArray(), p => p.IsParams);

        if (unionHasDefaults)
        {
            Report(GeneratorDiagnostics.DefaultsInWindow, method.Locations.FirstOrDefault(), method.Name);
            yield break;
        }

        if (paramsIndex >= 0 && !unionOptional.Contains(paramsIndex))
        {
            Report(GeneratorDiagnostics.ParamsOutsideWindow, method.Locations.FirstOrDefault(), method.Name);
            yield break;
        }

        var reportedRefOutIn = false;
        var reportedDuplicate = false;

        foreach (var optionalIndices in optionalIndexSpecs)
        {
            if (optionalIndices.Length == 0)
            {
                continue;
            }

            IEnumerable<int[]> omissionSets = options.SubsequenceStrategy == OverloadSubsequenceStrategy.PrefixOnly
                ? BuildPrefixOmissions(optionalIndices)
                : BuildAllOmissions(optionalIndices);

            foreach (var omittedIndices in omissionSets)
            {
                if (omittedIndices.Length == 0)
                {
                    continue;
                }

                var omittedParameters = omittedIndices.Select(i => originalParameters[i]).ToArray();

                if (omittedParameters.Any(p => p.RefKind != RefKind.None))
                {
                    if (!reportedRefOutIn)
                    {
                        Report(GeneratorDiagnostics.RefOutInOmitted, method.Locations.FirstOrDefault(), method.Name);
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

                if (keptParameters.Length == 0)
                {
                    continue;
                }

                var key = BuildSignatureKey(method.Name, method.TypeParameters.Length, keptParameters);
                if (!signatureKeys.Add(key))
                {
                    if (!reportedDuplicate)
                    {
                        Report(GeneratorDiagnostics.DuplicateSignatureSkipped, method.Locations.FirstOrDefault(), method.Name);
                        reportedDuplicate = true;
                    }
                    continue;
                }

                if (existingKeys.Contains(key))
                {
                    if (!reportedDuplicate)
                    {
                        Report(GeneratorDiagnostics.DuplicateSignatureSkipped, method.Locations.FirstOrDefault(), method.Name);
                        reportedDuplicate = true;
                    }
                    continue;
                }

                yield return new GeneratedMethod(method, keptParameters, omittedParameters, options.OverloadVisibility);
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
    private static Dictionary<string, bool> BuildDefaultValueMap(IMethodSymbol method)
    {
        var map = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is MethodDeclarationSyntax methodSyntax)
            {
                foreach (var parameter in methodSyntax.ParameterList.Parameters)
                {
                    map[parameter.Identifier.ValueText] = parameter.Default is not null;
                }
            }
        }

        return map;
    }
    private static bool IsDefaultableParameter(IParameterSymbol parameter, Dictionary<string, bool> defaultMap)
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
    private HashSet<string> BuildExistingMethodKeys(INamedTypeSymbol type, string methodName)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.MethodKind != MethodKind.Ordinary)
            {
                continue;
            }

            if (!string.Equals(method.Name, methodName, StringComparison.Ordinal))
            {
                continue;
            }

            var key = BuildSignatureKey(method.Name, method.TypeParameters.Length, method.Parameters);
            keys.Add(key);
        }

        return keys;
    }
    private static string BuildSignatureKey(string name, int arity, IEnumerable<IParameterSymbol> parameters)
    {
        var builder = new StringBuilder();
        builder.Append(name);
        builder.Append("|");
        builder.Append(arity);

        foreach (var parameter in parameters)
        {
            builder.Append("|");
            builder.Append(parameter.Type.ToDisplayString(SignatureDisplayFormat));
            builder.Append(":");
            builder.Append(parameter.RefKind);
            builder.Append(":");
            builder.Append(parameter.IsParams ? "params" : "noparams");
        }

        return builder.ToString();
    }
    private GenerationOptions GetEffectiveOptions(IMethodSymbol method, INamedTypeSymbol containingType)
    {
        var options = new GenerationOptions
        {
            RangeAnchorMatchMode = RangeAnchorMatchMode.TypeOnly,
            SubsequenceStrategy = OverloadSubsequenceStrategy.UniqueBySignature,
            OverloadVisibility = OverloadVisibility.MatchTarget
        };

        if (TryGetOverloadOptions(containingType, out var typeOptions))
        {
            ApplyOptions(options, typeOptions);
        }

        if (TryGetOverloadOptions(method, out var methodOptions))
        {
            ApplyOptions(options, methodOptions);
        }

        return options;
    }
    private bool TryGetOverloadOptions(ISymbol symbol, out OverloadOptionsSyntax options)
    {
        options = default;

        var attribute = GetAttribute(symbol, OverloadGenerationOptionsAttributeName);
        if (attribute is not null && TryGetOverloadOptionsFromAttribute(attribute, out options))
        {
            return true;
        }

        return TryGetOverloadOptionsFromSyntax(symbol, out options);
    }
    private static bool TryGetOverloadOptionsFromAttribute(AttributeData attribute, out OverloadOptionsSyntax options)
    {
        options = default;

        foreach (var arg in attribute.NamedArguments)
        {
            if (string.Equals(arg.Key, "RangeAnchorMatchMode", StringComparison.Ordinal))
            {
                if (TryGetEnumConstant(arg.Value, out RangeAnchorMatchMode value))
                {
                    options.RangeAnchorMatchMode = value;
                }
            }
            else if (string.Equals(arg.Key, "SubsequenceStrategy", StringComparison.Ordinal))
            {
                if (TryGetEnumConstant(arg.Value, out OverloadSubsequenceStrategy value))
                {
                    options.SubsequenceStrategy = value;
                }
            }
            else if (string.Equals(arg.Key, "OverloadVisibility", StringComparison.Ordinal))
            {
                if (TryGetEnumConstant(arg.Value, out OverloadVisibility value))
                {
                    options.OverloadVisibility = value;
                }
            }
        }

        return options.HasAny;
    }
    private static bool TryGetEnumConstant<TEnum>(TypedConstant constant, out TEnum value)
        where TEnum : struct
    {
        value = default;
        if (constant.Value is null)
        {
            return false;
        }

        try
        {
            value = (TEnum)Enum.ToObject(typeof(TEnum), constant.Value);
            return true;
        }
        catch
        {
            return false;
        }
    }
    private static bool TryGetOverloadVisibilityOverride(IMethodSymbol method, out OverloadVisibility visibility)
    {
        visibility = default;
        foreach (var attribute in method.GetAttributes())
        {
            var name = attribute.AttributeClass?.Name;
            if (name is null)
            {
                continue;
            }

            if (!string.Equals(name, OverloadGenerationOptionsAttributeName, StringComparison.Ordinal) &&
                !(OverloadGenerationOptionsAttributeName.EndsWith("Attribute", StringComparison.Ordinal) &&
                  string.Equals(name, OverloadGenerationOptionsAttributeName.Substring(0, OverloadGenerationOptionsAttributeName.Length - "Attribute".Length), StringComparison.Ordinal)))
            {
                continue;
            }

            foreach (var arg in attribute.NamedArguments)
            {
                if (!string.Equals(arg.Key, "OverloadVisibility", StringComparison.Ordinal))
                {
                    continue;
                }

                if (TryGetEnumConstant(arg.Value, out visibility))
                {
                    return true;
                }
            }
        }

        return false;
    }
    private static void ApplyOptions(GenerationOptions options, OverloadOptionsSyntax optionsSyntax)
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
    private static string BuildParameterSignatureKey(IEnumerable<IParameterSymbol> parameters)
    {
        var builder = new StringBuilder();
        foreach (var parameter in parameters)
        {
            builder.Append("|");
            builder.Append(parameter.Type.ToDisplayString(SignatureDisplayFormat));
            builder.Append(":");
            builder.Append(parameter.RefKind);
            builder.Append(":");
            builder.Append(parameter.IsParams ? "params" : "noparams");
        }

        return builder.ToString();
    }

    private static Location? GetGenerateOverloadsAttributeLocation(MethodDeclarationSyntax methodSyntax)
    {
        foreach (var attributeList in methodSyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (IsAttributeNameMatch(attribute.Name.ToString(), "GenerateOverloads"))
                {
                    return attribute.GetLocation();
                }
            }
        }

        return null;
    }
}

