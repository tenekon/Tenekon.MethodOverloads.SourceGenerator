using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Generation;

internal readonly record struct OverloadPlanEntry(
    MethodModel Method,
    ParameterModel[] KeptParameters,
    ParameterModel[] OmittedParameters,
    OverloadVisibility OverloadVisibility,
    IReadOnlyCollection<MatcherMethodReference>? MatchedMatcherMethods,
    BucketTypeModel? BucketType)
{
    public string Namespace => BucketType?.Namespace ?? Method.ContainingNamespace;
}
