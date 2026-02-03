using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Finds matcher-to-target subsequence alignments.
    /// </summary>
    private IEnumerable<ParameterMatch> FindSubsequenceMatches(MethodModel matcherMethod, MethodModel targetMethod)
    {
        var matchMode = ResolveMatchMode(targetMethod, matcherMethod);

        var matcherParams = matcherMethod.Parameters.Items;
        var targetParams = targetMethod.Parameters.Items;

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

    private static bool IsMatch(ParameterModel matcherParam, ParameterModel targetParam, RangeAnchorMatchMode matchMode)
    {
        if (!AreTypesEquivalent(matcherParam.TypeDisplay, targetParam.TypeDisplay))
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

    private RangeAnchorMatchMode ResolveMatchMode(MethodModel targetMethod, MethodModel matcherMethod)
    {
        if (TryGetRangeAnchorMatchMode(targetMethod, out var matchMode))
        {
            return matchMode;
        }

        if (_typesByDisplay.TryGetValue(targetMethod.ContainingTypeDisplay, out var typeModel) &&
            TryGetRangeAnchorMatchMode(typeModel.Options, out matchMode))
        {
            return matchMode;
        }

        if (TryGetRangeAnchorMatchMode(matcherMethod, out matchMode))
        {
            return matchMode;
        }

        if (_matcherTypesByDisplay.TryGetValue(matcherMethod.ContainingTypeDisplay, out var matcherType) &&
            TryGetRangeAnchorMatchMode(matcherType.Options, out matchMode))
        {
            return matchMode;
        }

        return RangeAnchorMatchMode.TypeOnly;
    }

    private bool TryGetRangeAnchorMatchMode(MethodModel method, out RangeAnchorMatchMode matchMode)
    {
        matchMode = RangeAnchorMatchMode.TypeOnly;
        if (TryGetOverloadOptions(method, out var optionsSyntax) &&
            optionsSyntax.RangeAnchorMatchMode.HasValue)
        {
            matchMode = optionsSyntax.RangeAnchorMatchMode.Value;
            return true;
        }

        return false;
    }

    private bool TryGetRangeAnchorMatchMode(OverloadOptionsModel options, out RangeAnchorMatchMode matchMode)
    {
        matchMode = RangeAnchorMatchMode.TypeOnly;
        if (options.RangeAnchorMatchMode.HasValue)
        {
            matchMode = options.RangeAnchorMatchMode.Value;
            return true;
        }

        return false;
    }

    private static bool AreTypesEquivalent(string matcherType, string targetType)
    {
        return string.Equals(matcherType, targetType, StringComparison.Ordinal);
    }

    private static IEnumerable<MatcherMethodModel> SelectMatcherMethods(EquatableArray<MatcherMethodModel> matcherMethods, int targetParameterCount)
    {
        return matcherMethods.Items.Length == 0 ? Array.Empty<MatcherMethodModel>() : matcherMethods.Items;
    }
}
