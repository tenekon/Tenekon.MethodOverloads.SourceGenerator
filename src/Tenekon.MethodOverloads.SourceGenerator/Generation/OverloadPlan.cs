using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Generation;

internal readonly record struct OverloadPlan(
    Dictionary<string, List<OverloadPlanEntry>> MethodsByNamespace,
    Dictionary<string, HashSet<MatcherMethodReference>> MatchedMatchersByNamespace,
    EquatableArray<EquatableDiagnostic> Diagnostics);