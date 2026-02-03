using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal enum RangeAnchorMatchMode
{
    TypeOnly,
    TypeAndName
}

internal enum OverloadSubsequenceStrategy
{
    PrefixOnly,
    UniqueBySignature
}

internal enum OverloadVisibility
{
    MatchTarget,
    Public,
    Internal,
    Private
}

internal readonly record struct OverloadOptionsModel(
    RangeAnchorMatchMode? RangeAnchorMatchMode,
    OverloadSubsequenceStrategy? SubsequenceStrategy,
    OverloadVisibility? OverloadVisibility)
{
    public bool HasAny =>
        RangeAnchorMatchMode.HasValue || SubsequenceStrategy.HasValue || OverloadVisibility.HasValue;
}

internal sealed partial class MethodOverloadsGeneratorCore
{
    internal readonly record struct EquatableArray<T>(ImmutableArray<T> Items)
    {
        public static EquatableArray<T> Empty => new(ImmutableArray<T>.Empty);

        public bool Equals(EquatableArray<T> other)
        {
            var left = Items;
            var right = other.Items;
            if (left.Length != right.Length)
            {
                return false;
            }

            for (var i = 0; i < left.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(left[i], right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = 17;
            foreach (var item in Items)
            {
                hash = (hash * 31) + (item is null ? 0 : item.GetHashCode());
            }

            return hash;
        }
    }

    internal readonly record struct SourceLocationModel(
        string Path,
        int SpanStart,
        int SpanLength,
        int StartLine,
        int StartCharacter,
        int EndLine,
        int EndCharacter)
    {
        public Location ToLocation(IReadOnlyDictionary<string, SyntaxTree> syntaxTreesByPath)
        {
            var span = new TextSpan(SpanStart, SpanLength);
            if (!string.IsNullOrEmpty(Path) && syntaxTreesByPath.TryGetValue(Path, out var tree))
            {
                return Location.Create(tree, span);
            }

            var lineSpan = new LinePositionSpan(
                new LinePosition(StartLine, StartCharacter),
                new LinePosition(EndLine, EndCharacter));
            return Location.Create(Path, span, lineSpan);
        }
    }

    internal readonly record struct GeneratorInputModel(
        EquatableArray<TypeModel> Types,
        EquatableArray<TypeTargetModel> TypeTargets,
        EquatableArray<MethodTargetModel> MethodTargets,
        EquatableArray<MatcherTypeModel> MatcherTypes);

    internal readonly record struct TypeModel(
        string DisplayName,
        string NamespaceName,
        EquatableArray<MethodModel> Methods,
        EquatableArray<MethodSignatureModel> MethodSignatures,
        OverloadOptionsModel Options);

    internal readonly record struct MethodSignatureModel(
        string Name,
        int TypeParameterCount,
        EquatableArray<ParameterSignatureModel> Parameters);

    internal readonly record struct ParameterSignatureModel(
        string SignatureTypeDisplay,
        RefKind RefKind,
        bool IsParams);

    internal readonly record struct MethodModel(
        string Name,
        string ContainingTypeDisplay,
        string ContainingNamespace,
        string ReturnTypeDisplay,
        bool IsStatic,
        bool IsExtensionMethod,
        Accessibility DeclaredAccessibility,
        int TypeParameterCount,
        EquatableArray<string> TypeParameterNames,
        string TypeParameterConstraints,
        EquatableArray<ParameterModel> Parameters,
        SourceLocationModel? IdentifierLocation,
        bool IsOrdinary,
        OverloadOptionsModel Options,
        bool OptionsFromAttribute);

    internal readonly record struct ParameterModel(
        string Name,
        string TypeDisplay,
        string SignatureTypeDisplay,
        RefKind RefKind,
        bool IsParams,
        bool IsOptional,
        bool HasExplicitDefaultValue,
        bool HasDefaultFromSyntax);

    internal readonly record struct TypeTargetModel(
        TypeModel Type,
        bool HasGenerateMethodOverloads,
        EquatableArray<string> MatcherTypeDisplays,
        OverloadOptionsModel Options);

    internal readonly record struct MethodTargetModel(
        MethodModel Method,
        bool HasGenerateOverloads,
        GenerateOverloadsArgsModel? GenerateArgsFromAttribute,
        GenerateOverloadsArgsModel? GenerateArgsFromSyntax,
        EquatableArray<string> MatcherTypeDisplays,
        OverloadOptionsModel OptionsFromAttributeOrSyntax,
        OverloadOptionsModel? SyntaxOptions,
        bool OptionsFromAttribute);

    internal readonly record struct MatcherTypeModel(
        TypeModel Type,
        OverloadOptionsModel Options,
        EquatableArray<MatcherMethodModel> MatcherMethods);

    internal readonly record struct MatcherMethodModel(
        MethodModel Method,
        GenerateOverloadsArgsModel? GenerateArgsFromAttribute,
        GenerateOverloadsArgsModel? GenerateArgsFromSyntax,
        OverloadOptionsModel OptionsFromAttributeOrSyntax,
        OverloadOptionsModel? SyntaxOptions);

    internal readonly record struct MatcherMethodReference(
        string ContainingTypeDisplay,
        string MethodName,
        int ParameterCount,
        string NamespaceName);

    internal readonly record struct GenerateOverloadsArgsModel(
        string? BeginEnd,
        string? Begin,
        string? BeginExclusive,
        string? End,
        string? EndExclusive,
        SourceLocationModel? AttributeLocation,
        SourceLocationModel? MethodIdentifierLocation,
        SourceLocationModel? SyntaxAttributeLocation)
    {
        public bool HasAny =>
            !string.IsNullOrEmpty(BeginEnd) ||
            !string.IsNullOrEmpty(Begin) ||
            !string.IsNullOrEmpty(BeginExclusive) ||
            !string.IsNullOrEmpty(End) ||
            !string.IsNullOrEmpty(EndExclusive);
    }
    private readonly struct WindowSpec
    {
        public WindowSpec(int startIndex, int endIndex)
            : this(startIndex, endIndex, string.Empty)
        {
        }

        public WindowSpec(int startIndex, int endIndex, string groupKey)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
            GroupKey = groupKey;
        }

        public int StartIndex { get; }
        public int EndIndex { get; }
        public string GroupKey { get; }
    }
    private readonly struct ParameterMatch
    {
        public ParameterMatch(int[] targetIndices)
        {
            TargetIndices = targetIndices;
        }

        public int[] TargetIndices { get; }

        public static ParameterMatch Identity(int length)
        {
            var indices = new int[length];
            for (var i = 0; i < length; i++)
            {
                indices[i] = i;
            }

            return new ParameterMatch(indices);
        }
    }
    private struct GenerationOptions
    {
        public RangeAnchorMatchMode RangeAnchorMatchMode;
        public OverloadSubsequenceStrategy SubsequenceStrategy;
        public OverloadVisibility OverloadVisibility;
    }
    private struct OverloadOptionsSyntax
    {
        public RangeAnchorMatchMode? RangeAnchorMatchMode;
        public OverloadSubsequenceStrategy? SubsequenceStrategy;
        public OverloadVisibility? OverloadVisibility;

        public bool HasAny =>
            RangeAnchorMatchMode.HasValue || SubsequenceStrategy.HasValue || OverloadVisibility.HasValue;
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

    private readonly struct WindowSpecFailure
    {
        public WindowSpecFailure(WindowSpecFailureKind kind, string? anchorKind, string? anchorValue)
        {
            Kind = kind;
            AnchorKind = anchorKind;
            AnchorValue = anchorValue;
        }

        public WindowSpecFailureKind Kind { get; }
        public string? AnchorKind { get; }
        public string? AnchorValue { get; }
    }
}

