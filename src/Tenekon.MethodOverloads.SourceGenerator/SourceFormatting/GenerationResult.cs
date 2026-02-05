using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Model;

namespace Tenekon.MethodOverloads.SourceGenerator.SourceFormatting;

internal readonly record struct GenerationResult(
    Dictionary<string, List<GeneratedMethod>> MethodsByNamespace,
    Dictionary<string, HashSet<MatcherMethodReference>> MatchedMatchersByNamespace,
    EquatableArray<EquatableDiagnostic> Diagnostics);