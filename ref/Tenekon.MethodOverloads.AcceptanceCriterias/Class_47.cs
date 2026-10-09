using System.Diagnostics.CodeAnalysis;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// A matcher type is never a target: GenerateMethodOverloads on a type marked with [OverloadMatcher] produces MOG021
/// and is ignored.
/// </summary>
[OverloadMatcher]
[SuppressMessage("MethodOverloadsGenerator", "MOG021")]
[GenerateMethodOverloads]
public abstract class Class_47_Matcher
{
    [GenerateOverloads(nameof(param_b))]
    public abstract void Case_1(int param_a, string? param_b);
}

public static class Class_47_AcceptanceCriterias
{
    // No overloads expected because matcher types are never targets.
}
