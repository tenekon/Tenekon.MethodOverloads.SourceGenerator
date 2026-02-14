namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct OverloadOptionsModel(
    RangeAnchorMatchMode? RangeAnchorMatchMode,
    OverloadSubsequenceStrategy? SubsequenceStrategy,
    OverloadVisibility? OverloadVisibility,
    BucketTypeModel? BucketType)
{
    public bool HasAny => RangeAnchorMatchMode.HasValue || SubsequenceStrategy.HasValue || OverloadVisibility.HasValue
        || BucketType.HasValue;
}
