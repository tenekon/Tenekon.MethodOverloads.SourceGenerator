namespace Tenekon.MethodOverloads.SourceGenerator.Model;

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
    public bool HasAny => RangeAnchorMatchMode.HasValue || SubsequenceStrategy.HasValue || OverloadVisibility.HasValue;
}