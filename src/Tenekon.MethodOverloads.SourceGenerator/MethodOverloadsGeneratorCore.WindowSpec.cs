using System.Collections.Immutable;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Builds optional-parameter windows from attributes and syntax.
    /// </summary>
    private static bool TryCreateWindowSpecFromArgs(
        MethodModel targetMethod,
        MethodModel matcherMethod,
        GenerateOverloadsArgsModel args,
        ParameterMatch match,
        out WindowSpec windowSpec,
        out WindowSpecFailure failure)
    {
        windowSpec = default;
        failure = new WindowSpecFailure(WindowSpecFailureKind.None, null, null);

        var matcherParams = matcherMethod.Parameters.Items;

        var startIndex = 0;
        var endIndex = targetMethod.Parameters.Items.Length - 1;
        if (match.TargetIndices.Length > 0)
        {
            startIndex = match.TargetIndices[0];
            endIndex = match.TargetIndices[match.TargetIndices.Length - 1];
        }

        if (!string.IsNullOrEmpty(args.BeginEnd) &&
            (!string.IsNullOrEmpty(args.Begin) ||
             !string.IsNullOrEmpty(args.End) ||
             !string.IsNullOrEmpty(args.BeginExclusive) ||
             !string.IsNullOrEmpty(args.EndExclusive)))
        {
            failure = new WindowSpecFailure(WindowSpecFailureKind.ConflictingAnchors, null, null);
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
        {
            args = args with { Begin = args.BeginEnd, End = args.BeginEnd };
        }
        else if (!string.IsNullOrEmpty(args.Begin) &&
                 !string.IsNullOrEmpty(args.End) &&
                 string.Equals(args.Begin, args.End, StringComparison.Ordinal))
        {
            failure = new WindowSpecFailure(WindowSpecFailureKind.RedundantAnchors, "BeginEnd", args.Begin);
        }

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
                failure = new WindowSpecFailure(WindowSpecFailureKind.MissingAnchor, "BeginExclusive", args.BeginExclusive);
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
        {
            if (string.Equals(parameters[i].Name, name, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }
}
