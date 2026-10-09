namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// A matcher type is never a target, even when no target uses it: no overloads and no diagnostics.
/// As a target, the second method would produce MOG006 (its overload duplicates the first method).
/// The matcher is an abstract class because the acceptance infrastructure maps diagnostics only inside classes.
/// </summary>
[OverloadMatcher]
public abstract class Class_49_Matcher
{
    [GenerateOverloads(EndExclusive = nameof(param_b))]
    public abstract void Matcher_2(bool param_a, CancellationToken param_b);

    [GenerateOverloads(EndExclusive = nameof(param_b))]
    public abstract void Matcher_2(string? param_a, bool param_b, CancellationToken param_c);
}

public static class Class_49_AcceptanceCriterias
{
    // No overloads expected because matcher types are never targets.
}
