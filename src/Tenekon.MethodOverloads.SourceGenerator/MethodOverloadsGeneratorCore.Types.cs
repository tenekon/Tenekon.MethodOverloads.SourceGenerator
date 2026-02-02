using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Internal data structures used across generator phases.
    /// </summary>
    private sealed class TypeContext
    {
        public TypeContext(INamedTypeSymbol symbol)
        {
            Symbol = symbol;
            Methods = new List<IMethodSymbol>();
            MatcherTypes = ImmutableArray<INamedTypeSymbol>.Empty;
            MethodSyntax = new Dictionary<IMethodSymbol, MethodDeclarationSyntax>(SymbolEqualityComparer.Default);
        }

        public INamedTypeSymbol Symbol { get; }
        public List<IMethodSymbol> Methods { get; }
        public Dictionary<IMethodSymbol, MethodDeclarationSyntax> MethodSyntax { get; }
        public AttributeData? GenerateMethodOverloadsAttribute { get; set; }
        public ImmutableArray<INamedTypeSymbol> MatcherTypes { get; set; }
        public bool HasGenerateMethodOverloads { get; set; }
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
    private enum RangeAnchorMatchMode
    {
        TypeOnly,
        TypeAndName
    }
    private enum OverloadSubsequenceStrategy
    {
        PrefixOnly,
        UniqueBySignature
    }
    private enum OverloadVisibility
    {
        MatchTarget,
        Public,
        Internal,
        Private
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
    private struct GenerateOverloadsArgs
    {
        public string? BeginEnd;
        public string? Begin;
        public string? BeginExclusive;
        public string? End;
        public string? EndExclusive;

        public bool HasAny =>
            !string.IsNullOrEmpty(BeginEnd) ||
            !string.IsNullOrEmpty(Begin) ||
            !string.IsNullOrEmpty(BeginExclusive) ||
            !string.IsNullOrEmpty(End) ||
            !string.IsNullOrEmpty(EndExclusive);
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

