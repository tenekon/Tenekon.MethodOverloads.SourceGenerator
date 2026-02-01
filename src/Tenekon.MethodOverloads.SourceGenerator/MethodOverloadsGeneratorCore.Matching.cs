using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Finds matcher-to-target subsequence alignments.
    /// </summary>
    private IEnumerable<ParameterMatch> FindSubsequenceMatches(IMethodSymbol matcherMethod, IMethodSymbol targetMethod)
    {
        var matchMode = ResolveMatchMode(targetMethod, targetMethod.ContainingType, matcherMethod, matcherMethod.ContainingType);

        var matcherParams = matcherMethod.Parameters;
        var targetParams = targetMethod.Parameters;

        if (matcherParams.Length == 0 || matcherParams.Length > targetParams.Length)
        {
            yield break;
        }

        var indices = new int[matcherParams.Length];

        foreach (var match in FindMatchesRecursive(matcherParams, targetParams, matchMode, 0, 0, indices))
        {
            yield return match;
        }
    }
    private IEnumerable<ParameterMatch> FindMatchesRecursive(
        ImmutableArray<IParameterSymbol> matcherParams,
        ImmutableArray<IParameterSymbol> targetParams,
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
        {
            if (IsMatch(matcherParams[matcherIndex], targetParams[i], matchMode))
            {
                indices[matcherIndex] = i;
                foreach (var match in FindMatchesRecursive(matcherParams, targetParams, matchMode, matcherIndex + 1, i + 1, indices))
                {
                    yield return match;
                }
            }
        }
    }
    private static bool IsMatch(IParameterSymbol matcherParam, IParameterSymbol targetParam, RangeAnchorMatchMode matchMode)
    {
        if (!AreTypesEquivalent(matcherParam.Type, targetParam.Type))
        {
            return false;
        }

        if (matcherParam.RefKind != targetParam.RefKind)
        {
            return false;
        }

        if (matcherParam.IsParams != targetParam.IsParams)
        {
            return false;
        }

        if (matchMode == RangeAnchorMatchMode.TypeAndName)
        {
            return string.Equals(matcherParam.Name, targetParam.Name, StringComparison.Ordinal);
        }

        return true;
    }
    private RangeAnchorMatchMode ResolveMatchMode(
        IMethodSymbol targetMethod,
        INamedTypeSymbol targetType,
        IMethodSymbol matcherMethod,
        INamedTypeSymbol matcherType)
    {
        if (TryGetRangeAnchorMatchMode(targetMethod, out var matchMode))
        {
            return matchMode;
        }

        if (TryGetRangeAnchorMatchMode(targetType, out matchMode))
        {
            return matchMode;
        }

        if (TryGetRangeAnchorMatchMode(matcherMethod, out matchMode))
        {
            return matchMode;
        }

        if (TryGetRangeAnchorMatchMode(matcherType, out matchMode))
        {
            return matchMode;
        }

        return RangeAnchorMatchMode.TypeOnly;
    }
    private bool TryGetRangeAnchorMatchMode(ISymbol symbol, out RangeAnchorMatchMode matchMode)
    {
        matchMode = RangeAnchorMatchMode.TypeOnly;
        if (TryGetOverloadOptions(symbol, out var optionsSyntax) &&
            optionsSyntax.RangeAnchorMatchMode.HasValue)
        {
            matchMode = optionsSyntax.RangeAnchorMatchMode.Value;
            return true;
        }

        return false;
    }
    private static bool AreTypesEquivalent(ITypeSymbol matcherType, ITypeSymbol targetType)
    {
        if (SymbolEqualityComparer.Default.Equals(matcherType, targetType))
        {
            return true;
        }

        var matcherDisplay = matcherType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var targetDisplay = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return string.Equals(matcherDisplay, targetDisplay, StringComparison.Ordinal);
    }
    private static IEnumerable<IMethodSymbol> SelectMatcherMethods(IMethodSymbol[] matcherMethods, int targetParameterCount)
    {
        if (matcherMethods.Length == 0)
        {
            return Array.Empty<IMethodSymbol>();
        }

        var selected = new List<IMethodSymbol>();
        foreach (var group in matcherMethods.GroupBy(m => m.Name, StringComparer.Ordinal))
        {
            if (group.Count() > 1)
            {
                var minCount = group.Min(m => m.Parameters.Length);
                selected.AddRange(group.Where(m => m.Parameters.Length == minCount));
            }
            else
            {
                selected.Add(group.First());
            }
        }

        if (selected.Any(m => m.Parameters.Length == targetParameterCount))
        {
            selected = selected.Where(m => m.Parameters.Length == targetParameterCount).ToList();
        }

        return selected;
    }
}

