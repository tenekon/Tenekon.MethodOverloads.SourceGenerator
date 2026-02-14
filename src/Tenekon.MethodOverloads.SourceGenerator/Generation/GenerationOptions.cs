using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Generation;

internal struct GenerationOptions
{
    public RangeAnchorMatchMode RangeAnchorMatchMode;
    public OverloadSubsequenceStrategy SubsequenceStrategy;
    public OverloadVisibility OverloadVisibility;
    public BucketTypeModel? BucketType;
}
