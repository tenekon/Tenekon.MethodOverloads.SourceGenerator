using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Generation;

internal sealed class MatcherGroupInfo(BucketTypeModel? bucketType)
{
    public BucketTypeModel? BucketType { get; } = bucketType;
    public HashSet<MatcherMethodReference> MatchedMatchers { get; } = [];
}
