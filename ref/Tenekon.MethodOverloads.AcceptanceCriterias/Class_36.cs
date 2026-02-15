namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Matcher type A for matcher-union testing.
/// </summary>
internal interface Class_36_MatcherA
{
    /// <summary>
    /// Optional window anchored at param_b.
    /// </summary>
    [GenerateOverloads(nameof(param_b))]
    void Matcher_1(int param_a, string? param_b);
}

/// <summary>
/// Matcher type B for matcher-union testing.
/// </summary>
internal interface Class_36_MatcherB
{
    /// <summary>
    /// Optional window anchored at param_c.
    /// </summary>
    [GenerateOverloads(nameof(param_c))]
    void Matcher_1(int param_a, string? param_b, bool param_c);
}

/// <summary>
/// Multiple Matchers-only GenerateOverloads attributes on a single method.
/// </summary>
public abstract class Class_36
{
    /// <summary>
    /// Target method using two matcher groups that should be unioned.
    /// </summary>
    [GenerateOverloads(Matchers = [typeof(Class_36_MatcherA)])]
    [GenerateOverloads(Matchers = [typeof(Class_36_MatcherB)])]
    public abstract void Case_1(int param_1, string? param_2, bool param_3);
}

public static class Class_36_AcceptanceCriterias
{
    public static void Case_1(this Class_36 source, int param_1, bool param_3)
    {
        source.Case_1(param_1, param_2: null, param_3);
    }

    public static void Case_1(this Class_36 source, int param_1, string? param_2)
    {
        source.Case_1(param_1, param_2, default(bool));
    }
}
