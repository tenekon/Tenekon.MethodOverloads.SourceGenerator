using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Generation;

internal readonly record struct OverloadPlan(
    Dictionary<OverloadGroupKey, List<OverloadPlanEntry>> MethodsByGroup,
    Dictionary<OverloadGroupKey, MatcherGroupInfo> MatchedMatchersByGroup,
    EquatableArray<EquatableDiagnostic> Diagnostics);
